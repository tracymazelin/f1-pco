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

            this.GetF1RequestToken();

            return Redirect(string.Format(PrivateConsts.f1AuthorizeUrl, this.ChurchCode, this.F1RequestToken.Value, PrivateConsts.f1CalBack));
        }

        public ActionResult CallBack() {

            string provider = Request.QueryString["provider"];

            if (provider == "f1") {
                this.GetF1AccessToken();

                this.GetPCORequestToken();

                return Redirect(string.Format(PrivateConsts.pcoAuthorizeUrl, this.PCORequestToken.Value));
            }
            else {
                this.PCORequestToken.Verifier = Request.QueryString["oauth_verifier"];

                this.GetPCOAccessToken();

                return RedirectToAction("StartSync");
            }
        }

        public ActionResult StartSync() {
            int attributeID = this.GetAttributeID("SyncMe");

            results peeps = this.GetF1People(attributeID);

            return View(peeps);
        }

        #endregion

        #region Private Methods

        #region OAuth

        private void GetF1RequestToken() {
            this.F1RequestToken = new Token();

            var f1Creds = new OAuthCredentials {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = "2",
                ConsumerSecret = "f7d02059-a105-45e0-85c9-7387565f322b",
                //ConsumerKey = "163",
                //ConsumerSecret = "de1bee74-93c1-4a72-b6e5-0192e5569219"
            };

            var client = new RestClient {
                //Authority = string.Format("https://{0}.staging.fellowshiponeapi.com", this.ChurchCode),
                Authority = string.Format("http://{0}.fellowshiponeapi.local", this.ChurchCode),
                VersionPath = "v1",
                Credentials = f1Creds
            };

            var request = new RestRequest {
                Path = "Tokens/RequestToken"
            };

            RestResponse response = client.Request(request);

            var collection = HttpUtility.ParseQueryString(response.Content);
            this.F1RequestToken.Value = collection["oauth_token"];
            this.F1RequestToken.Secret = collection["oauth_token_secret"];
        }

        private void GetF1AccessToken() {
            this.F1AccessToken = new Token();

            var creds = new OAuthCredentials {
                Type = OAuthType.AccessToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                //ConsumerKey = "163",
                //ConsumerSecret = "de1bee74-93c1-4a72-b6e5-0192e5569219",
                ConsumerKey = "2",
                ConsumerSecret = "f7d02059-a105-45e0-85c9-7387565f322b",
                Token = this.F1RequestToken.Value,
                TokenSecret = this.F1RequestToken.Secret
            };

            var client = new RestClient {
                Authority = string.Format("http://{0}.fellowshiponeapi.local", this.ChurchCode),
                //Authority = string.Format("https://{0}.staging.fellowshiponeapi.com", this.ChurchCode),
                VersionPath = "v1",
                Credentials = creds
            };

            var request = new RestRequest {
                Path = "Tokens/AccessToken"
            };

            RestResponse response = client.Request(request);

            var collection = HttpUtility.ParseQueryString(response.Content);
            this.F1AccessToken.Value = collection["oauth_token"];
            this.F1AccessToken.Secret = collection["oauth_token_secret"];
        }

        private void GetPCORequestToken() {

            var pcoCreds = new OAuthCredentials {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = "HfK94IoIKmm40sHeVykg",
                ConsumerSecret = "wBWSl0szv2PhuGSxBUf7xyUjnnW389Bzou6EgPFA",
                CallbackUrl = PrivateConsts.pcoCallback
            };

            var pcoClient = new RestClient {
                Authority = "https://www.planningcenteronline.com/oauth",                
                Credentials = pcoCreds
            };

            var pcoRequest = new RestRequest {
                Path = "request_token"
            };

            RestResponse pcoResponse = pcoClient.Request(pcoRequest);

            this.PCORequestToken = new Token();
            var pcoCollection = HttpUtility.ParseQueryString(pcoResponse.Content);
            this.PCORequestToken.Value = pcoCollection["oauth_token"];
            this.PCORequestToken.Secret = pcoCollection["oauth_token_secret"];
        }

        private void GetPCOAccessToken() {
            this.PCOAccessToken = new Token();

            var creds = new OAuthCredentials {
                Type = OAuthType.AccessToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = "HfK94IoIKmm40sHeVykg",
                ConsumerSecret = "wBWSl0szv2PhuGSxBUf7xyUjnnW389Bzou6EgPFA",
                Token = this.PCORequestToken.Value,
                TokenSecret = this.PCORequestToken.Secret,
                Verifier = this.PCORequestToken.Verifier
            };

            var client = new RestClient {
                Authority = "https://www.planningcenteronline.com/oauth",
                Credentials = creds
            };

            var request = new RestRequest {
                Path = "access_token"
            };

            RestResponse response = client.Request(request);

            var collection = HttpUtility.ParseQueryString(response.Content);
            this.PCOAccessToken.Value = collection["oauth_token"];
            this.PCOAccessToken.Secret = collection["oauth_token_secret"];
        }
        
        #endregion

        #region REST

        private int GetAttributeID(string name) {

            int ret = 0;

            var creds = new OAuthCredentials {
                Type = OAuthType.AccessToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                //ConsumerKey = "163",
                //ConsumerSecret = "de1bee74-93c1-4a72-b6e5-0192e5569219",
                ConsumerKey = "2",
                ConsumerSecret = "f7d02059-a105-45e0-85c9-7387565f322b",
                Token = this.F1AccessToken.Value,
                TokenSecret = this.F1AccessToken.Secret
            };

            var client = new RestClient {
                Authority = string.Format("http://{0}.fellowshiponeapi.local", this.ChurchCode),
                //Authority = string.Format("https://{0}.staging.fellowshiponeapi.com", this.ChurchCode),
                VersionPath = "v1",
                Credentials = creds
            };

            var request = new RestRequest {
                Path = "People/AttributeGroups"
            };

            RestResponse response = client.Request(request);
            if (response.StatusCode == HttpStatusCode.OK) {
                using (StreamReader streamReader = new StreamReader(response.ContentStream)) {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(attributeGroups));

                    // Deserialize the response into a Person object.
                    attributeGroups attributes = xmlSerializer.Deserialize(streamReader) as attributeGroups;

                    var attributeId = (from a in attributes.attributeGroup
                                  where a.attribute.name == "Baptism"
                                  select a.attribute.id).FirstOrDefault();
                    ret = Convert.ToInt32(attributeId);                    
                }
            }
            return ret;
        }

        private results GetF1People(int attributeId) {

            results peopleCollection = null;

            var creds = new OAuthCredentials {
                Type = OAuthType.AccessToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                //ConsumerKey = "163",
                //ConsumerSecret = "de1bee74-93c1-4a72-b6e5-0192e5569219",
                ConsumerKey = "2",
                ConsumerSecret = "f7d02059-a105-45e0-85c9-7387565f322b",
                Token = this.F1AccessToken.Value,
                TokenSecret = this.F1AccessToken.Secret
            };

            var client = new RestClient {
                //Authority = string.Format("https://{0}.staging.fellowshiponeapi.com", this.ChurchCode),
                Authority = string.Format("http://{0}.fellowshiponeapi.local", this.ChurchCode),
                VersionPath = "v1",
                Credentials = creds
            };

            var request = new RestRequest {
                Path = string.Format("People/Search", attributeId.ToString())
            };
            request.AddParameter("attribute", attributeId.ToString());
            request.AddParameter("recordsperpage", "100");

            RestResponse response = client.Request(request);
            if (response.StatusCode == HttpStatusCode.OK) {
                using (StreamReader streamReader = new StreamReader(response.ContentStream)) {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(results));

                    // Deserialize the response into a Person object.
                    peopleCollection = xmlSerializer.Deserialize(streamReader) as results;
                }
            }
            return peopleCollection;
        }

        #endregion

        #endregion

    }
}
