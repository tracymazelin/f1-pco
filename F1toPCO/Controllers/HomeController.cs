using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using F1toPCO.Util;
using Hammock;
using Hammock.Authentication.OAuth;
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
                if (Session["F1AccessToken"] != null) {
                    return (Token)Session["F1AccessToken"];
                }
                return null;
                //return new Token("3c5e5d61-f99b-4fdf-bc4e-e8ed7600a78f", "2cf5cb49-b275-4e76-9a10-2ee631b00dc8");
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
                //return new Token("VdCpi2nqbilzyPpqMGoa", "qjUrqMHkHJcJGmnKyAjfqBsgROXHEJieU5jiQxoE");
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

        public List<Model.MatchHelper> Matches {
            get {
                if (Session["Matches"] == null) {
                    Session["Matches"] = new List<Model.MatchHelper>();
                }
                return (List<Model.MatchHelper>)Session["Matches"];
            }
            set {
                if (Session["Matches"] != null) {
                    Session["Matches"] = value;
                }
                else {
                    Session.Add("Matches", value);
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

            if (this.F1AccessToken != null && this.PCOAccessToken != null) {
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
            Model.PCO.person person = null;

            attributeId = this.F1GetAttributeID("SyncMe");

            Model.F1.people f1People = this.F1GetPeopleByAttribute(attributeId);

            foreach (F1toPCO.Model.F1.person p in f1People.items) {
                //Get the comment for the attribute to see if we already now the PCOID.
                string comment = this.ExtractComment(attributeId, p.attributes.peopleAttribute);

                if (comment != string.Empty) {
                    //We have the id.  Update the record if needed.
                    person = this.PCOGetPersonByID(Convert.ToInt32(comment));
                    this.UpdatePerson(p, ref person);
                    this.PCOUpdatePerson(this.SerializeEntity(person), comment);
                }
                else {
                    //Look at the person by name
                    Model.PCO.people people = null;
                    people = this.PCOGetPersonByName(p.lastName + ", " + (string.IsNullOrEmpty(p.goesByName) ? p.firstName.Substring(0, 1) : p.goesByName.Substring(0, 1)));

                    if (people.person.Count == 1) {
                        //Only one match.
                        Model.PCO.person matchPerson = people.person.FirstOrDefault();
                        this.UpdatePerson(p, ref matchPerson);
                        this.PCOUpdatePerson(this.SerializeEntity(matchPerson), matchPerson.id.Value);
                    }
                    else if (people.person.Count == 0) {
                        //create
                        //make sure to write the ID to the sync me attribute

                        F1toPCO.Model.PCO.person pcop = new F1toPCO.Model.PCO.person();
                        this.UpdatePerson(p, ref pcop);

                        this.PCOCreatePerson(this.SerializeEntity(pcop));
                    }
                    else {
                        //Multiples.  Need user input.
                        //See if we can narrow it down to one user based on the email.
                        //if not, then we need user input.
                        if (!this.FindMatchViaEmailAndUpdate(p, people)) {
                            Model.MatchHelper matches = new Model.MatchHelper();
                            matches.F1Person = p;
                            matches.PCOPeople = people;
                            this.Matches.Add(matches);
                        }
                    }
                }
            }

            if (this.Matches.Count > 0) {
                return RedirectToAction("MultipleMatches");
            }
            else {
                return View("Success");
            }
        }

        public ActionResult MultipleMatches() {
            return View(this.Matches);
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

        private bool FindMatchViaEmailAndUpdate(F1toPCO.Model.F1.person f1Person, F1toPCO.Model.PCO.people pcoPeople) {
            var email = f1Person.communications.items.Where(e => e.communicationType.name == "Email").FirstOrDefault().communicationValue;

            if (email != null) {
                var filteredPerson = pcoPeople.person.Where(x =>
                                                       x.contactData.emailAddresses.emailAddress == x.contactData.emailAddresses.emailAddress.Where(y =>
                                                                                                                                                    y.address == email))
                                                       .FirstOrDefault();
                if (filteredPerson != null) {
                    this.UpdatePerson(f1Person, ref filteredPerson);
                    this.PCOUpdatePerson(this.SerializeEntity(filteredPerson), filteredPerson.id.Value);
                    return true;
                }
            }
            return false;
        }

        private void UpdateEmailCommunications(F1toPCO.Model.F1.communications f1Emails, F1toPCO.Model.PCO.emailAddresses pcoEmails) {
            foreach (F1toPCO.Model.F1.EntityType et in F1toPCO.Model.F1.emailSyncType.Items) {
                F1toPCO.Model.F1.communication tmpF1Email = f1Emails.items.Where(y => y.communicationType.name == et.F1Type).FirstOrDefault();
                F1toPCO.Model.PCO.emailAddress tmpPCOEmail = pcoEmails.emailAddress.Where(x => x.location == et.PCOType).FirstOrDefault();

                if (tmpF1Email != null) {
                    if (tmpPCOEmail == null) {
                        tmpPCOEmail = new Model.PCO.emailAddress();
                        tmpPCOEmail.address = tmpF1Email.communicationValue;
                        tmpPCOEmail.location = et.PCOType;
                        pcoEmails.emailAddress.Add(tmpPCOEmail);
                    }
                    else {
                        tmpPCOEmail.address = tmpF1Email.communicationValue;
                    }
                }
                else {
                    if (tmpPCOEmail != null) {
                        pcoEmails.emailAddress.Remove(tmpPCOEmail);
                    }
                }

            }
        }

        private void UpdatePhoneCommunications(F1toPCO.Model.F1.communications f1Phones, F1toPCO.Model.PCO.phoneNumbers pcoPhones) {
            foreach (F1toPCO.Model.F1.EntityType et in F1toPCO.Model.F1.phoneSyncType.Items) {
                F1toPCO.Model.F1.communication tmpF1Phone = f1Phones.items.Where(y => y.communicationType.name == et.F1Type).FirstOrDefault();
                F1toPCO.Model.PCO.phoneNumber tmpPCOPhone = pcoPhones.phoneNumber.Where(x => x.location == et.PCOType).FirstOrDefault();

                if (tmpF1Phone != null) {
                    if (tmpPCOPhone == null) {
                        tmpPCOPhone = new Model.PCO.phoneNumber();
                        tmpPCOPhone.number = tmpF1Phone.communicationValue;
                        tmpPCOPhone.location = et.PCOType;
                        pcoPhones.phoneNumber.Add(tmpPCOPhone);
                    }
                    else {
                        tmpPCOPhone.location = tmpF1Phone.communicationValue;
                    }
                }
                else {
                    if (tmpPCOPhone != null) {
                        pcoPhones.phoneNumber.Remove(tmpPCOPhone);
                    }

                }

            }
        }

        private void UpdatePerson(F1toPCO.Model.F1.person f1Person, ref F1toPCO.Model.PCO.person pcoPerson) {

            pcoPerson.firstname = string.IsNullOrEmpty(f1Person.goesByName) ? f1Person.firstName : f1Person.goesByName;
            pcoPerson.lastname = f1Person.lastName;

            //Emails
            F1toPCO.Model.F1.communications emailComms = new F1toPCO.Model.F1.communications();
            emailComms.items = f1Person.communications.items.Where(y => y.communicationGeneralType == F1toPCO.Model.F1.communicationGeneralType.Email).ToList();
            UpdateEmailCommunications(emailComms, pcoPerson.contactData.emailAddresses);

            //Phone numbers
            F1toPCO.Model.F1.communications phoneComs = new F1toPCO.Model.F1.communications();
            phoneComs.items = f1Person.communications.items.Where(y => y.communicationGeneralType == F1toPCO.Model.F1.communicationGeneralType.Telephone).ToList();
            UpdatePhoneCommunications(phoneComs, pcoPerson.contactData.phoneNumbers);

            //Address
            F1toPCO.Model.F1.address primaryAddress = f1Person.addresses.items.Where(x => x.addressType.name == "Primary").FirstOrDefault();
            F1toPCO.Model.PCO.address pcoAddress = pcoPerson.contactData.addresses.address.Where(x => x.location == "Home").FirstOrDefault();
            if (primaryAddress != null) {
                if (pcoAddress == null) {
                    pcoAddress = new F1toPCO.Model.PCO.address();
                    pcoPerson.contactData.addresses.address.Add(pcoAddress);
                }
                pcoAddress.street = FormatStreet(primaryAddress);
                pcoAddress.city = primaryAddress.city;
                pcoAddress.state = primaryAddress.stProvince;
                pcoAddress.zip = primaryAddress.postalCode;
            }
            else {
                if (pcoAddress != null) {
                    pcoPerson.contactData.addresses.address.Remove(pcoAddress);
                }
            }
        }

        /// <summary>
        /// Formats the address 1, address 2 and address 3 lines into one address line.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private string FormatStreet(F1toPCO.Model.F1.address address) {
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
        private string ExtractComment(int attributeId, List<F1toPCO.Model.F1.peopleAttribute> peopleAttribute) {
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
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(Model.F1.attributeGroups));

                        // Deserialize the response into a Person object.
                        Model.F1.attributeGroups attributes = xmlSerializer.Deserialize(streamReader) as Model.F1.attributeGroups;

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

        private F1toPCO.Model.F1.people F1GetPeopleByAttribute(int attributeId) {

            F1toPCO.Model.F1.people peopleCollection = null;

            RestRequest request = new RestRequest {
                Path = string.Format("People/Search", attributeId.ToString())
            };
            request.AddParameter("attribute", attributeId.ToString());
            request.AddParameter("recordsperpage", "100");
            request.AddParameter("include", "attributes,addresses,communications");

            using (RestResponse response = F1Client.Request(request)) {
                if (response.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(response.ContentStream)) {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(F1toPCO.Model.F1.people));

                        // Deserialize the response into a Person object.
                        peopleCollection = xmlSerializer.Deserialize(streamReader) as F1toPCO.Model.F1.people;
                    }
                }
                else {
                    throw new Exception("An error occured: Status code: " + response.StatusCode, response.InnerException);
                }
            }
            return peopleCollection;
        }

        private F1toPCO.Model.PCO.person PCOGetPersonByID(int id) {

            F1toPCO.Model.PCO.people peeps = null;

            var request = new RestRequest {
                Path = "people.xml"
            };

            request.AddParameter("people_ids", id.ToString());

            using (RestResponse response = PCOClient.Request(request)) {
                if (response.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(response.ContentStream)) {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(F1toPCO.Model.PCO.people));

                        // Deserialize the response into a Person object.
                        peeps = xmlSerializer.Deserialize(streamReader) as F1toPCO.Model.PCO.people;
                    }
                }
                else {
                    throw new Exception("An error occured: Status code: " + response.StatusCode, response.InnerException);
                }
            }
            return peeps != null ? peeps.person.FirstOrDefault() : null;
        }

        private Model.PCO.people PCOGetPersonByName(string name) {
            F1toPCO.Model.PCO.people peeps = null;

            var request = new RestRequest {
                Path = "people.xml"
            };

            request.AddParameter("name", name);

            using (RestResponse response = PCOClient.Request(request)) {
                if (response.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(response.ContentStream)) {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(F1toPCO.Model.PCO.people));

                        // Deserialize the response into a Person object.
                        peeps = xmlSerializer.Deserialize(streamReader) as F1toPCO.Model.PCO.people;
                    }
                }
                else {
                    throw new Exception("An error occured: Status code: " + response.StatusCode, response.InnerException);
                }
            }
            return peeps;
        }

        private bool PCOCreatePerson(string xml) {

            //var request = new RestRequest {
            //    Path = "people.xml",
            //    Method = WebMethod.Post,
            //    Entity = xml
            //};

            //RestResponse response = PCOClient.Request(request);
            //if (response.StatusCode == HttpStatusCode.OK) {
            //    return true;
            //}
            return true;
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
