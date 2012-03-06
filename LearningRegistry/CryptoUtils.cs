using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using LearningRegistry;
using LearningRegistry.RDDD;

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
		
		private List<string> _publicKeyLocation;
		private string _privateKey;
		private string _passPhrase;
        private string _userId;
		
		public PgpSigner (IEnumerable<string> publicKeyLocation, string privateKey, string userId, string passPhrase)
		{
			_publicKeyLocation = publicKeyLocation.ToList();
			_privateKey = privateKey;
            _userId = userId;
			_passPhrase = passPhrase;
		}
		
		public lr_document Sign(lr_document document)
		{
			//Nullify any previous signature info or fields populated by the server
			document.digital_signature = null;
			
			//Bencode the document data
			string bencodedMsg = document.Bencode();
			
			//Clear sign the bencoded document
			string clearSignedMessage = signEnvelopeData(getHash(bencodedMsg));
			
			//Create a signature based on the clearSignedMessage
			lr_digital_signature signature = new lr_digital_signature();
			signature.signing_method = signingMethod;
			Console.WriteLine("message: " + clearSignedMessage);
			signature.signature = clearSignedMessage;
			signature.key_location = _publicKeyLocation;
			
			//Add the signature to the original document
			document.digital_signature = signature;
			
			return document;
		}
		
		private string getHash(string s)
		{
			var csp = new System.Security.Cryptography.SHA256Managed();
			var utf8Encoding = new System.Text.UTF8Encoding();
			byte[] result = csp.ComputeHash(utf8Encoding.GetBytes(s));
			return Convert.ToBase64String(result)+"\n";
		}
		
		private string signEnvelopeData(string msg)
		{
			Stream privateKeyStream = getPrivateKeyStream(_privateKey);
			
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
				byte[] msgBytes = utf8Encoding.GetBytes(msg);
				sigGen.Update(msgBytes, 0, msgBytes.Length);
				aOut.Write(msgBytes, 0, msgBytes.Length);
				bOut = new BcpgOutputStream(aOut);
				aOut.EndClearText();
				sigGen.Generate().Encode(bOut);
				using (BinaryReader br = new BinaryReader(result))
				{
					br.BaseStream.Position = 0;
					return utf8Encoding.GetString(br.ReadBytes((int)result.Length));
				}
			}
			catch (Exception e)
			{	Console.WriteLine("This happened: " + e.Message);
				throw new Exception("Signing Failed: " + e.Message);
			}
			finally
			{
				try
				{
					if (privateKeyStream != null)
						privateKeyStream.Close();
					//if(bOut != null)
						//bOut.Close();
					//aOut.Close();
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
			catch (Exception e)
			{
				throw new Exception("Invalid private key stream, reason: " + e.Message);
			}
			
			var keyRings = pgpSec.GetKeyRings();
			foreach( PgpSecretKeyRing keyRing in keyRings)
			{
				foreach(PgpSecretKey key in keyRing.GetSecretKeys())
				{
                    if (key.UserIds.Cast<String>().Where(id => id == _userId).Count() > 0)
                    {
                        try
                        {
                            key.ExtractPrivateKey(_passPhrase.ToCharArray());
                            return key;
                        }
                        catch { continue; }
                    }
				}
			}
			
			throw new Exception("Could not find a valid signing key");
		}
	}
}

