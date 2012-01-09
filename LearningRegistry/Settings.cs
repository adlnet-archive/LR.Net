// Copyright 2011 ADL - http://www.adlnet.gov/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using LearningRegistry.RDDD;
using LearningRegistry.Paradata;
namespace LearningRegistry
{
    public class Settings
    {
        static public string LR_Integration_KeyID()
        {
            return (ConfigurationManager.AppSettings["LR_Integration_KeyID"]);
        }
        static public string LR_Integration_NodeUsername()
        {
            return (ConfigurationManager.AppSettings["LR_Integration_NodeUsername"]);
        }
        static public string LR_Integration_NodePassword()
        {
            return (ConfigurationManager.AppSettings["LR_Integration_NodePassword"]);
        }
        static public string LR_Integration_PublishURL()
        {
            return (ConfigurationManager.AppSettings["LR_Integration_PublishURL"]);
        }
        static public string LR_Integration_KeyPassPhrase()
        {
            return (ConfigurationManager.AppSettings["LR_Integration_KeyPassPhrase"]);
        }
        static public string LR_Integration_GPGLocation()
        {
            return (ConfigurationManager.AppSettings["LR_Integration_GPGLocation"]);
        }
        static public string LR_Integration_PublicKeyURL()
        {
            return (ConfigurationManager.AppSettings["LR_Integration_PublicKeyURL"]);
        }
        static public string LR_Integration_SubmitterName()
        {
            return (ConfigurationManager.AppSettings["LR_Integration_SubmitterName"]);
        }
        static public string LR_Integration_SignerName()
        {
            return (ConfigurationManager.AppSettings["LR_Integration_SignerName"]);
        }
    }
}