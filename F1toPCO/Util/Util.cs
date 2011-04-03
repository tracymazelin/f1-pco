using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using F1toPCO.Util;

namespace F1toPCO.Util
{
    public class URL {
        //Staging
        //public const string f1BaseUrl = "https://{0}.staging.fellowshiponeapi.com";
        //public const string f1AuthorizeUrl = "https://{0}.staging.fellowshiponeapi.com/v1/PortalUser/Login?oauth_token={1}&oauth_callback={2}";
        
        //Prod
        public const string f1BaseUrl = "https://{0}.fellowshiponeapi.com";
        public const string f1AuthorizeUrl = "https://{0}.fellowshiponeapi.com/v1/PortalUser/Login?oauth_token={1}&oauth_callback={2}";
        
        public const string f1CalBack = "http://localhost/F1toPCO/Home/CallBack?provider=f1";

        public const string pcoBaseUrl = "https://www.planningcenteronline.com";
        public const string pcoAuthorizeUrl = "https://www.planningcenteronline.com/oauth/authorize?oauth_token={0}";
        public const string pcoCallback = "http://localhost/F1toPCO/Home/CallBack?provider=pco";
    }

    public class Token
    {
        public string Value;
        public string Secret;
        public string Verifier;

        public Token() { }
        public Token(string Value, string Secret) {
            this.Value = Value;
            this.Secret = Secret;
        }
    }
}
