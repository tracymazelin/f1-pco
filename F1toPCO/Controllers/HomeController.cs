using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using F1toPCO.Util;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using Hammock.Authentication.OAuth;
using Hammock;

namespace F1toPCO.Controllers {


    [HandleError]
    public class HomeController : Controller {

        #region Properties
       
        public string ChurchCode {
            get {
                if (Session["ChurchCode"] != null) {
                    return (string)Session["ChurchCode"];
                }
                return null;
            }
            set {
                if (Session["ChurchCode"] != null) {
                    Session["ChurchCode"] = value;
                }
                else {
                    Session.Add("ChurchCode", value);
                }
            }
        }

        public Token F1AccessToken {
            get {
                if (Session["F1AccessToken"] != null) {
                    return (Token)Session["F1AccessToken"];
                }
                return null;
            }
            set {
                if (Session["F1AccessToken"] != null) {
                    Session["F1AccessToken"] = value;
                }
                else {
                    Session.Add("F1AccessToken", value);
                }
            }
        }

        public Token PCOAccessToken {
            get {
                if (Session["PCOAccessToken"] != null) {
                    return (Token)Session["PCOAccessToken"];
                }
                return null;
            }
            set {
                if (Session["PCOAccessToken"] != null) {
                    Session["PCOAccessToken"] = value;
                }
                else {
                    Session.Add("PCOAccessToken", value);
                }
            }
        }
        
        public Token F1RequestToken {
            get {
                if (Session["F1RequestToken"] != null) {
                    return (Token)Session["F1RequestToken"];
                }
                return null;
            }
            set {
                if (Session["F1RequestToken"] != null) {
                    Session["F1RequestToken"] = value;
                }
                else {
                    Session.Add("F1RequestToken", value);
                }
            }
        }

        public Token PCORequestToken {
            get {
                if (Session["PCORequestToken"] != null) {
                    return (Token)Session["PCORequestToken"];
                }
                return null;
            }
            set {
                if (Session["PCORequestToken"] != null) {
                    Session["PCORequestToken"] = value;
                }
                else {
                    Session.Add("PCORequestToken", value);
                }
            }
        }
        #endregion

        #region Actions

        public ActionResult Index() {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string churchCode) {

            this.ChurchCode = churchCode;

            var f1Creds = new OAuthCredentials {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = "163",
                ConsumerSecret = "de1bee74-93c1-4a72-b6e5-0192e5569219"
            };

            var client = new RestClient {
                Authority = string.Format("https://{0}.staging.fellowshiponeapi.com", this.ChurchCode),
                VersionPath = "v1",
                Credentials = f1Creds
            };

            var request = new RestRequest {
                Path = "Tokens/RequestToken"
            };

            RestResponse response = client.Request(request);

            var collection = HttpUtility.ParseQueryString(response.Content);
            this.F1RequestToken.Value = collection[0];
            this.F1RequestToken.Secret = collection[1];

            return Redirect(string.Format("https://{0}.staging.fellowshiponeapi.com/v1/PortalUser/Login?oauth_token={1}&oauth_callback={2}", this.ChurchCode, this.F1RequestToken.Value, "http://localhost/F1toPCO/CallBack"));

            //            this.ChurchCode = churchCode;
            ////            this.f1OAuth = new OAuthUtil("fellowshiponeapi.local", this.ChurchCode, "v1", "PortalUser", "163", "de1bee74-93c1-4a72-b6e5-0192e5569219");
            //            this.f1OAuth = new OAuthUtil("fellowshiponeapi.local", this.ChurchCode, "v1", "PortalUser", "7", "fdf335cc-1863-4c66-8c29-baeac856c895");

            //            this.RequestToken = this.f1OAuth.GetRequestToken();

            //            if (this.RequestToken != null) {
            //                string callBack = string.Format("http://{0}/F1toPCO/Home/CallBack", Request.Url.Host);

            //                string authLink = f1OAuth.RequestUserAuth(this.RequestToken.Value, callBack);

            //                return new RedirectResult(authLink);
            //            }

            //            return View();
        }

        public ActionResult CallBack() {
            string personUrl = null;

            var creds = new OAuthCredentials {
                Type = OAuthType.AccessToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = "163",
                ConsumerSecret = "de1bee74-93c1-4a72-b6e5-0192e5569219",
                Token = this.F1RequestToken.Value,
                TokenSecret = this.F1RequestToken.Secret
            };

            var client = new RestClient {
                Authority = string.Format("https://{0}.staging.fellowshiponeapi.com", this.ChurchCode),
                VersionPath = "v1",
                Credentials = creds
            };

             var request = new RestRequest {
                Path = "Tokens/AccessToken"
            };

            RestResponse response = client.Request(request);

            var collection = HttpUtility.ParseQueryString(response.Content);
            this.F1AccessToken.Value = collection["oauth_token"];
            this.F1AccessToken.Secret = collection["oauth_secret"];

            return View();

        //    this.AccessToken = this.f1OAuth.GetAccessToken(this.RequestToken, out personUrl);

        //    this.pcoOAuth = new OAuthUtil();

        //    Token test = this.pcoOAuth.GetRequestToken();

        //    if (test != null) {
        //        string callBack = string.Format("http://{0}/F1toPCO/CallBack", Request.Url.Host);

        //        string authLink = this.pcoOAuth.RequestUserAuth(test.Value, callBack);

        //        return new RedirectResult(authLink);
        //    }

        //    try {
        //        // Create a request to the API to obtain the person.
        //        HttpWebRequest webRequest = this.f1OAuth.CreateWebRequestFromPartialUrl(string.Format("People/Search?searchFor={0}", "klein"), this.AccessToken, HttpRequestMethod.GET);

        //        using (WebResponse webResponse = webRequest.GetResponse()) {
        //            using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream())) {
        //                XmlSerializer xmlSerializer = new XmlSerializer(typeof(results));

        //                // Deserialize the response into a Person object.
        //                results peeps = xmlSerializer.Deserialize(streamReader) as results;
        //            }
        //        }
        //    }
        //    catch (WebException ex) {
        //        // TODO: add logging.
        //        throw;
        //    }
        //    return View();
        }

        #endregion

    }
}
