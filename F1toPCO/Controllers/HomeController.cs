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
using F1toPCO.Model;
using System.Xml;
using System.Text;
using Hammock.Web;

namespace F1toPCO.Controllers {

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
                //if (Session["F1AccessToken"] != null) {
                //    return (Token)Session["F1AccessToken"];
                //}
                //return null;
                return new Token("3c5e5d61-f99b-4fdf-bc4e-e8ed7600a78f", "2cf5cb49-b275-4e76-9a10-2ee631b00dc8");
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
                //if (Session["PCOAccessToken"] != null) {
                //    return (Token)Session["PCOAccessToken"];
                //}
                //return null;
                return new Token("VdCpi2nqbilzyPpqMGoa", "qjUrqMHkHJcJGmnKyAjfqBsgROXHEJieU5jiQxoE");
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

            if (this.F1AccessToken.Value != null && this.PCOAccessToken.Value != null) {
                return RedirectToAction("StartSync");
            }
            else {
                this.GetF1RequestToken();
            }

            return Redirect(string.Format(Util.URL.f1AuthorizeUrl, this.ChurchCode, this.F1RequestToken.Value, Util.URL.f1CalBack));
        }

        public ActionResult CallBack() {

            string provider = Request.QueryString["provider"];

            if (provider == "f1") {
                this.GetF1AccessToken();

                this.GetPCORequestToken();

                return Redirect(string.Format(Util.URL.pcoAuthorizeUrl, this.PCORequestToken.Value));
            }
            else {
                this.PCORequestToken.Verifier = Request.QueryString["oauth_verifier"];

                this.GetPCOAccessToken();

                return RedirectToAction("StartSync");
            }
        }

        public ActionResult StartSync() {
            int attributeId = int.MinValue;
            PCOperson person = null;
            string payload = null;

            attributeId = this.F1GetAttributeID("SyncMe");

            People f1People = this.F1GetPeopleByAttribute(attributeId);

            foreach (Person p in f1People.items) {
                //Get the comment for the attribute to see if we already now the PCOID.
                string comment = this.ExtractComment(attributeId, p.attributes.peopleAttribute);

                if (comment != string.Empty) {
                    //We have the id.  Update the record if needed.
                    person = this.PCOGetPersonByID(Convert.ToInt32(comment));

                    this.PCODeletePerson(comment);
                    //this.UpdatePerson(p, ref person);

                    //payload = this.SerializeEntity(person);

                    //this.PCOUpdatePerson(payload, comment);
                }
                else {
                    PCOperson pcop = new PCOperson();
                    pcop.firstname = p.firstName;
                    pcop.lastname = p.lastName;
                    pcop.name = p.firstName + " " + (string.IsNullOrEmpty(p.goesByName) ? p.lastName : p.goesByName + " " + p.lastName);
                    pcop.contactdata = new Contactdata();
                    pcop.contactdata.addresses = new Addresses();
                    pcop.contactdata.phonenumbers = new Phonenumbers();
                    pcop.contactdata.emailaddresses = new Emailaddresses();
                    this.UpdatePerson(p, ref pcop);

                    payload = this.SerializeEntity(pcop);

                    this.PCOCreatePerson(payload);
                    //Check PCO to see if user exists.
                    //Remember to write the ID back to the attribute for this user if we find one.

                }
            }

            return View();
        }

        public ActionResult Trouble() {
            return View("Error");
        }

        #endregion

        #region Private Methods

        #region OAuth

        private void GetF1RequestToken() {
            this.F1RequestToken = new Token();

            var creds = new OAuthCredentials {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = Util.PrivateConsts.f1ConsumerKey,
                ConsumerSecret = Util.PrivateConsts.f1ConsumerSecret
            };

            var client = new RestClient {
                Authority = string.Format(Util.URL.f1BaseUrl, this.ChurchCode),
                VersionPath = "v1",
                Credentials = creds
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
                ConsumerKey = Util.PrivateConsts.f1ConsumerKey,
                ConsumerSecret = Util.PrivateConsts.f1ConsumerSecret,
                Token = this.F1RequestToken.Value,
                TokenSecret = this.F1RequestToken.Secret
            };

            var client = new RestClient {
                Authority = string.Format(Util.URL.f1BaseUrl, this.ChurchCode),
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
                ConsumerKey = Util.PrivateConsts.pcoConsumerKey,
                ConsumerSecret = Util.PrivateConsts.pcoConsumerSecret,
                CallbackUrl = Util.URL.pcoCallback
            };

            var pcoClient = new RestClient {
                Authority = Util.URL.pcoBaseUrl + "/oauth",
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
                ConsumerKey = Util.PrivateConsts.pcoConsumerKey,
                ConsumerSecret = Util.PrivateConsts.pcoConsumerSecret,
                Token = this.PCORequestToken.Value,
                TokenSecret = this.PCORequestToken.Secret,
                Verifier = this.PCORequestToken.Verifier
            };

            var client = new RestClient {
                Authority = Util.URL.pcoBaseUrl + "/oauth",
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

        #region Helpers

        private void UpdatePerson(Person f1Person, ref PCOperson pcoPerson) {
           
            //Emails
            communications emailComms = new communications();
            emailComms.items = f1Person.communications.items.Where(y => (y.communicationGeneralType == communicationGeneralType.Email) &&
                                                                        (y.communicationType.name == "Email" ||
                                                                         y.communicationType.name == "Work Email"))
                                                                        .ToList();
            foreach (communication c in emailComms.items) {
                Emailaddress email = pcoPerson.contactdata.emailaddresses.emailaddress.Where(e => e.location == emailSyncType.Items.Where(f => f.F1Type == c.communicationType.name).FirstOrDefault().PCOType).FirstOrDefault();
                if (email != null) {
                    if (c.lastUpdatedDate >= Convert.ToDateTime(pcoPerson.updatedat.Value)) { }
                    if (email.address != c.communicationValue) {
                        email.address = c.communicationValue;
                    }
                }
                else {
                    email = new Emailaddress();
                    email.address = c.communicationValue;
                    email.location = emailSyncType.Items.Where(e => e.F1Type == c.communicationType.name).FirstOrDefault().PCOType;
                    pcoPerson.contactdata.emailaddresses.emailaddress.Add(email);
                }
            }

            //Phone numbers
            communications phoneComs = new communications();
            phoneComs.items = f1Person.communications.items.Where(y => (y.communicationGeneralType == communicationGeneralType.Telephone) &&
                                                                        (y.communicationType.name == "Home Phone" ||
                                                                         y.communicationType.name == "Work Phone" ||
                                                                         y.communicationType.name == "Mobile" ||
                                                                         y.communicationType.name == "Fax"))
                                                                        .ToList();
            foreach (communication c in phoneComs.items) {
                Phonenumber phone = pcoPerson.contactdata.phonenumbers.phonenumber.Where(e => e.location == phoneSyncType.Items.Where(f => f.F1Type == c.communicationType.name).FirstOrDefault().PCOType).FirstOrDefault();
                if (phone != null) {
                    if (c.lastUpdatedDate >= Convert.ToDateTime(pcoPerson.updatedat.Value)) { }
                    if (phone.number != c.communicationValue) {
                        phone.number = c.communicationValue;
                    }
                }
                else {
                    phone = new Phonenumber();
                    phone.number = c.communicationValue;
                    phone.location = phoneSyncType.Items.Where(e => e.F1Type == c.communicationType.name).FirstOrDefault().PCOType;
                    pcoPerson.contactdata.phonenumbers.phonenumber.Add(phone);
                }
            }

            //Address
            address primaryAddress = f1Person.addresses.items.Where(x => x.addressType.name == "Primary").FirstOrDefault();
            if (primaryAddress != null) {
                Address pcoAddress = pcoPerson.contactdata.addresses.address.Where(x => x.location == "Home").FirstOrDefault();
                if (pcoAddress == null) {
                    pcoAddress = new Address();
                    pcoPerson.contactdata.addresses.address.Add(pcoAddress);
                }
                pcoAddress.street = FormatStreet(primaryAddress);
                pcoAddress.city = primaryAddress.city;
                pcoAddress.state = primaryAddress.stProvince;
                pcoAddress.zip = primaryAddress.postalCode;
            }
        }

        /// <summary>
        /// Formats the address 1, address 2 and address 3 lines into one address line.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private string FormatStreet(address address) {
            StringBuilder retAddress = new StringBuilder();

            retAddress.Append(address.address1);
            if (!string.IsNullOrEmpty(address.address2)) {
                retAddress.Append("\n").Append(address.address2);
            }
            if (!string.IsNullOrEmpty(address.address3)) {
                retAddress.Append("\n").Append(address.address3);
            }

            return retAddress.ToString().Trim();
        }

        /// <summary>
        /// Queries peopleAttribute to see if the specified attributeId exists for the person object.
        /// </summary>
        /// <param name="attributeId"></param>
        /// <param name="peopleAttribute"></param>
        /// <returns>string</returns>
        private string ExtractComment(int attributeId, List<peopleAttribute> peopleAttribute) {
            string ret = string.Empty;

            var comment = (from a in peopleAttribute
                           from y in a.attributeGroup.attribute
                           where y.id == attributeId.ToString()
                           select a.comment).FirstOrDefault();

            ret = comment as string;

            return ret;
        }

        private string SerializeEntity(object entity) {
            string returnXml = null;
            System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer(entity.GetType());

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            MemoryStream mem = new MemoryStream();
            XmlTextWriter xml = new XmlTextWriter(mem, Encoding.UTF8);

            xmls.Serialize(xml, entity, ns);

            returnXml = Encoding.UTF8.GetString(mem.GetBuffer());
            returnXml = returnXml.Substring(returnXml.IndexOf(Convert.ToChar(60)));
            returnXml = returnXml.Substring(0, (returnXml.LastIndexOf(Convert.ToChar(62)) + 1));

            return returnXml;
        }

        #endregion

        #region REST

        #region Properties

        private OAuthCredentials F1Credentials {
            get {
                OAuthCredentials creds;
                if (Session["F1Creds"] == null) {
                    creds = new OAuthCredentials {
                        Type = OAuthType.AccessToken,
                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                        ConsumerKey = Util.PrivateConsts.f1ConsumerKey,
                        ConsumerSecret = Util.PrivateConsts.f1ConsumerSecret,
                        Token = this.F1AccessToken.Value,
                        TokenSecret = this.F1AccessToken.Secret
                    };
                    Session["F1Creds"] = creds;
                }
                else {
                    creds = Session["F1Creds"] as OAuthCredentials;
                }
                return creds;
            }
        }

        private OAuthCredentials PCOCredentials {
            get {
                OAuthCredentials creds;
                if (Session["PCOCreds"] == null) {
                    creds = new OAuthCredentials {
                        Type = OAuthType.AccessToken,
                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                        ConsumerKey = Util.PrivateConsts.pcoConsumerKey,
                        ConsumerSecret = Util.PrivateConsts.pcoConsumerSecret,
                        Token = this.PCOAccessToken.Value,
                        TokenSecret = this.PCOAccessToken.Secret
                    };
                    Session["PCOCreds"] = creds;
                }
                else {
                    creds = Session["PCOCreds"] as OAuthCredentials;
                }
                return creds;
            }
        }

        private RestClient F1Client {
            get {
                return new RestClient {
                    Authority = string.Format(Util.URL.f1BaseUrl, this.ChurchCode),
                    VersionPath = "v1",
                    Credentials = F1Credentials
                };
            }
        }

        private RestClient PCOClient {
            get {
                return new RestClient {
                    Authority = "https://www.planningcenteronline.com",
                    Credentials = PCOCredentials
                };
            }
        }

        #endregion

        #region Methods

        private int F1GetAttributeID(string name) {

            int attributeId = 0;

            RestRequest request = new RestRequest {
                Path = "People/AttributeGroups"
            };

            using (RestResponse response = F1Client.Request(request)) {
                if (response.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(response.ContentStream)) {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(attributeGroups));

                        // Deserialize the response into a Person object.
                        attributeGroups attributes = xmlSerializer.Deserialize(streamReader) as attributeGroups;

                        var id = (from a in attributes.attributeGroup
                                  from y in a.attribute
                                  where y.name == name
                                  select y.id).FirstOrDefault();

                        attributeId = Convert.ToInt32(id);
                    }
                }
                else {
                    throw new Exception("An error occured: Status code: " + response.StatusCode, response.InnerException);
                }
            }
            return attributeId;
        }

        private People F1GetPeopleByAttribute(int attributeId) {

            People peopleCollection = null;

            RestRequest request = new RestRequest {
                Path = string.Format("People/Search", attributeId.ToString())
            };
            request.AddParameter("attribute", attributeId.ToString());
            request.AddParameter("recordsperpage", "100");
            request.AddParameter("include", "attributes,addresses,communications");

            using (RestResponse response = F1Client.Request(request)) {
                if (response.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(response.ContentStream)) {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(People));

                        // Deserialize the response into a Person object.
                        peopleCollection = xmlSerializer.Deserialize(streamReader) as People;
                    }
                }
                else {
                    throw new Exception("An error occured: Status code: " + response.StatusCode, response.InnerException);
                }
            }
            return peopleCollection;
        }

        private PCOperson PCOGetPersonByID(int id) {

            PCOPeople peeps = null;

            var request = new RestRequest {
                Path = "people.xml"
            };

            request.AddParameter("people_ids", id.ToString());

            using (RestResponse response = PCOClient.Request(request)) {
                if (response.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(response.ContentStream)) {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(PCOPeople));

                        // Deserialize the response into a Person object.
                        peeps = xmlSerializer.Deserialize(streamReader) as PCOPeople;
                    }
                }
                else {
                    throw new Exception("An error occured: Status code: " + response.StatusCode, response.InnerException);
                }
            }
            return peeps != null ? peeps.Person.FirstOrDefault() : null;
        }

        private bool PCOCreatePerson(string xml) {

            var request = new RestRequest {
                Path = "people.xml",
                Method = WebMethod.Post,
                Entity = xml
            };

            RestResponse response = PCOClient.Request(request);
            if (response.StatusCode == HttpStatusCode.OK) {
                return true;
            }
            return false;
        }

        private bool PCOUpdatePerson(string xml, string id) {

            var request = new RestRequest {
                Path = "people/" + id + ".xml",
                Method = WebMethod.Put,
                Entity = xml
            };

            RestResponse response = PCOClient.Request(request);
            if (response.StatusCode == HttpStatusCode.OK) {
                return true;
            }
            return false;
        }

        private bool PCODeletePerson(string id) {

            var request = new RestRequest {
                Path = "people/" + id + ".xml",
                Method = WebMethod.Delete,
                //Entity = xml
            };

            RestResponse response = PCOClient.Request(request);
            if (response.StatusCode == HttpStatusCode.OK) {
                return true;
            }
            return false;
        }

        #endregion

        #endregion

        #endregion

    }
}
