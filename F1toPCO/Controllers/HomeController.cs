using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using F1toPCO.Util;
using System.Net;
using System.IO;
using System.Xml.Serialization;

namespace F1toPCO.Controllers
{


    [HandleError]
    public class HomeController : Controller {

        #region Properties

        public OAuthUtil f1OAuth {
            get {
                if (Session["f1OAuth"] != null) {
                    return (OAuthUtil)Session["f1OAuth"];
                }
                return null;
            }
            set {
                if (Session["f1OAuth"] != null) {
                    Session["f1OAuth"] = value;
                }
                else {
                    Session.Add("f1OAuth", value);
                }
            }
        }

        public OAuthUtil pcoOAuth {
            get {
                if (Session["pcoOAuth"] != null) {
                    return (OAuthUtil)Session["pcoOAuth"];
                }
                return null;
            }
            set {
                if (Session["pcoOAuth"] != null) {
                    Session["pcoOAuth"] = value;
                }
                else {
                    Session.Add("pcoOAuth", value);
                }
            }
        }

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
        public Token AccessToken {
            get {
                if (Session["AccessToken"] != null) {
                    return (Token)Session["AccessToken"];
                }
                return null;
            }
            set {
                if (Session["AccessToken"] != null) {
                    Session["AccessToken"] = value;
                }
                else {
                    Session.Add("AccessToken", value);
                }
            }
        }
        public Token RequestToken {
            get {
                if (Session["RequestToken"] != null) {
                    return (Token)Session["RequestToken"];
                }
                return null;
            }
            set {
                if (Session["RequestToken"] != null) {
                    Session["RequestToken"] = value;
                }
                else {
                    Session.Add("RequestToken", value);
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
//            this.f1OAuth = new OAuthUtil("fellowshiponeapi.local", this.ChurchCode, "v1", "PortalUser", "163", "de1bee74-93c1-4a72-b6e5-0192e5569219");
            this.f1OAuth = new OAuthUtil("fellowshiponeapi.local", this.ChurchCode, "v1", "PortalUser", "7", "fdf335cc-1863-4c66-8c29-baeac856c895");

            this.RequestToken = this.f1OAuth.GetRequestToken();

            if (this.RequestToken != null) {
                string callBack = string.Format("http://{0}/F1toPCO/Home/CallBack", Request.Url.Host);

                string authLink = f1OAuth.RequestUserAuth(this.RequestToken.Value, callBack);

                return new RedirectResult(authLink);
            }

            return View();
        }

        public ActionResult CallBack() {
            string personUrl = null;

            this.AccessToken = this.f1OAuth.GetAccessToken(this.RequestToken, out personUrl);

            this.pcoOAuth = new OAuthUtil();

            Token test = this.pcoOAuth.GetRequestToken();

            if (test != null) {
                string callBack = string.Format("http://{0}/F1toPCO/CallBack", Request.Url.Host);

                string authLink = this.pcoOAuth.RequestUserAuth(test.Value, callBack);

                return new RedirectResult(authLink);
            }

            try {
				// Create a request to the API to obtain the person.
				HttpWebRequest webRequest = this.f1OAuth.CreateWebRequestFromPartialUrl(string.Format("People/Search?searchFor={0}", "klein"), this.AccessToken, HttpRequestMethod.GET);
                               
                using (WebResponse webResponse = webRequest.GetResponse()) {
                    using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream())) {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(results));

                        // Deserialize the response into a Person object.
                        results peeps  = xmlSerializer.Deserialize(streamReader) as results;
                    }
                }
            }
            catch (WebException ex) {
                // TODO: add logging.
                throw;
            }
            return View();
        }
        
        #endregion
       
    }
}
