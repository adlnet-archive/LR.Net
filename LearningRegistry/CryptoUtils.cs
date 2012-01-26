using System;
using LearningRegistry;
using LearningRegistry.RDDD;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace LearningRegistry
{
	public class PgpSigner
	{
		private const string pgpRegex = ".*-----BEGIN PGP PRIVATE KEY BLOCK-----.*-----END PGP PRIVATE KEY BLOCK-----.*";
		private const string signingMethod = "LR-PGP.1.0";
		
		private string _publicKeyLocation;
		private string _privateKey;
		private string _passPhrase;
		
		public PgpSigner (string publicKeyLocation, string privateKey, string passPhrase)
		{
			_publicKeyLocation = publicKeyLocation;
			_privateKey = privateKey;
			_passPhrase = passPhrase;
		}
		
		public lr_Envelope Sign(lr_Envelope envelope)
		{
			string bencodedMsg = envelope.Bencode();
			string clearSignedMessage = signEnvelopData(bencodedMsg);
			//TODO: Finish the signing stuff
			return envelope;
		}
		
		private string signEnvelopData(string msg)
		{
			Stream privateKeyStream = getPrivateKeyStream(msg);
			
			MemoryStream result = new MemoryStream();
			ArmoredOutputStream aOut = new ArmoredOutputStream(result);
			BcpgOutputStream bOut = null;
			char[] privateKeyPassword = _passPhrase.ToCharArray();
			var utf8Encoding = new System.Text.UTF8Encoding();
			try
			{
				PgpSecretKey sk = readSecretKey(privateKeyStream);
				PgpPrivateKey pk = sk.ExtractPrivateKey(privateKeyPassword);
				PgpSignatureGenerator sigGen = new PgpSignatureGenerator(sk.PublicKey.Algorithm,HashAlgorithmTag.Sha256);
				PgpSignatureSubpacketGenerator spGen = new PgpSignatureSubpacketGenerator();
				                                                        
				var enumerator = sk.PublicKey.GetUserIds().GetEnumerator();
				if(enumerator.MoveNext())
				{
					spGen.SetSignerUserId(false, (string)enumerator.Current);
					sigGen.SetHashedSubpackets(spGen.Generate());
				}
				
				aOut.BeginClearText(HashAlgorithmTag.Sha256);
				sigGen.InitSign(PgpSignature.CanonicalTextDocument, pk);
				byte[] msgBytes = utf8Encoding.GetBytes(msg.ToCharArray());
				sigGen.Update(msgBytes, 0, msgBytes.Length);
				aOut.Write(msgBytes, 0, msgBytes.Length);
				bOut = new BcpgOutputStream(aOut);
				aOut.EndClearText();
				sigGen.Generate().Encode(bOut);
				using (BinaryReader br = new BinaryReader(result))
					return utf8Encoding.GetString(br.ReadBytes((int)result.Length));
				
			}
			catch (Exception e)
			{	
				throw new Exception("Signing Failed: " + e.Message);
			}
			finally
			{
				try
				{
					if (privateKeyStream != null)
						privateKeyStream.Close();
					if(bOut != null)
						bOut.Close();
					aOut.Close();
					result.Close();
				} catch (IOException) {}
			}
		}
		
		private Stream getPrivateKeyStream(String privateKey)
		{
			try
			{
				var regex = new System.Text.RegularExpressions.Regex(pgpRegex);
				if(regex.IsMatch(privateKey))
				{
					return new MemoryStream(new System.Text.UTF8Encoding().GetBytes(privateKey));
				}
				else
				{
					return new FileStream(privateKey, FileMode.Open);
				}
			}
			catch (IOException)
			{
				throw new Exception("Not a valid path or data string");
			}
		}
		
		private PgpSecretKey readSecretKey(Stream privateKeyStream)
		{
			PgpSecretKeyRingBundle pgpSec;
			try
			{
				pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));
			}
			catch (Exception)
			{
				throw new Exception("Invalid private key stream");
			}
			
			var keyRings = pgpSec.GetKeyRings();
			foreach( PgpSecretKeyRing keyRing in keyRings)
			{
				foreach(PgpSecretKey key in keyRing.GetSecretKeys())
				{
					if( key.IsSigningKey )
						return key;
				}
			}
			
			throw new Exception("Could not find a valid signing key");
		}
	}
}

