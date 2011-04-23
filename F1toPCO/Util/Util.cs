using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using F1toPCO.Util;
using System.Web.Mvc;

namespace F1toPCO.Util
{
    public static class URL {
        //Staging
        //public const string f1BaseUrl = "https://{0}.staging.fellowshiponeapi.com";
        //public const string f1AuthorizeUrl = "https://{0}.staging.fellowshiponeapi.com/v1/PortalUser/Login?oauth_token={1}&oauth_callback={2}";
        
        //Prod
        public const string f1BaseUrl = "https://{0}.fellowshiponeapi.com";
        public const string f1AuthorizeUrl = "https://{0}.fellowshiponeapi.com/v1/PortalUser/Login?oauth_token={1}&oauth_callback={2}";
        public const string f1CalBack = "CallBack?provider=f1";
        
        public const string pcoBaseUrl = "https://www.planningcenteronline.com";
        public const string pcoAuthorizeUrl = "https://www.planningcenteronline.com/oauth/authorize?oauth_token={0}";
        public const string pcoCallback = "CallBack?provider=pco";
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

    public class UIHelper {
        public static string FormatAddress(object address) {
            StringBuilder ret = new StringBuilder();
            if (address != null) {
                if (address.GetType() == typeof(Model.F1.address)) {
                    Model.F1.address f1Address = address as Model.F1.address;
                    ret.Append(f1Address.address1).Append("<BR/>");
                    if (!string.IsNullOrEmpty(f1Address.address2))
                        ret.Append(f1Address.address2).Append("<BR />");
                    if (!string.IsNullOrEmpty(f1Address.address3))
                        ret.Append(f1Address.address3).Append("<BR />");
                    if (!string.IsNullOrEmpty(f1Address.city)) {
                        ret.Append(f1Address.city);
                        if (!string.IsNullOrEmpty(f1Address.stProvince) || !string.IsNullOrEmpty(f1Address.postalCode)) {
                            ret.Append(", ");
                        }
                    }
                    if (!string.IsNullOrEmpty(f1Address.stProvince))
                        ret.Append(f1Address.stProvince).Append(" ");
                    if (!string.IsNullOrEmpty(f1Address.postalCode))
                        ret.Append(f1Address.postalCode);
                }
                else {
                    Model.PCO.address pcoAddress = address as Model.PCO.address;
                    ret.Append(pcoAddress.street).Append("<BR/>");
                    if (!string.IsNullOrEmpty(pcoAddress.city)) {
                        ret.Append(pcoAddress.city);
                        if (!string.IsNullOrEmpty(pcoAddress.state) || !string.IsNullOrEmpty(pcoAddress.zip)) {
                            ret.Append(", ");
                        }
                    }
                    if (!string.IsNullOrEmpty(pcoAddress.state))
                        ret.Append(pcoAddress.state).Append(" ");
                    if (!string.IsNullOrEmpty(pcoAddress.zip))
                        ret.Append(pcoAddress.zip);
                }
            }

            return ret.ToString();
        }

    }
}
