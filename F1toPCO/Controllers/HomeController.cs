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
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace F1toPCO.Controllers {

    public class HomeController : Controller {

        #region Properties

        public string F1ChurchCode {
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

        public int? AttributeID {
            get {
                if (Session["AttributeID"] != null) {
                    return (int)Session["AttributeID"];
                }
                return null;
            }
            set {
                if (Session["AttributeID"] != null) {
                    Session["AttributeID"] = value;
                }
                else {
                    Session.Add("AttributeID", value);
                }
            }
        }

        public Model.MatchHelper Matches {
            get {
                if (Session["Matches"] == null) {
                    Session["Matches"] = new Model.MatchHelper();
                }
                return (Model.MatchHelper)Session["Matches"];
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

        public Model.MatchHelper NoMatches {
            get {
                if (Session["NoMatches"] == null) {
                    Session["NoMatches"] = new Model.MatchHelper();
                }
                return (Model.MatchHelper)Session["NoMatches"];
            }
            set {
                if (Session["NoMatches"] != null) {
                    Session["NoMatches"] = value;
                }
                else {
                    Session.Add("NoMatches", value);
                }
            }
        }

        public List<Model.F1.person> PersonErrors {
            get {
                if (Session["PersonErrors"] == null) {
                    Session["PersonErrors"] = new List<Model.F1.person>();
                }
                return (List<Model.F1.person>)Session["PersonErrors"];
            }
            set {
                if (Session["PersonErrors"] != null) {
                    Session["PersonErrors"] = value;
                }
                else {
                    Session.Add("PersonErrors", value);
                }
            }
        }
        #endregion

        #region Actions

        public ActionResult Index() {
            Session.Remove("PersonErrors");
            return View();
        }

        public ActionResult ChurchCode() {
            return View();
        }

        [HttpPost]
        public ActionResult ChurchCode(string churchCode) {

            this.F1ChurchCode = churchCode;

            if (this.F1AccessToken != null && this.PCOAccessToken != null) {
                return RedirectToAction("Ready");
            }
            else {
                this.GetF1RequestToken();
            }

            string callback = Url.ToPublicUrl(new Uri(Util.URL.f1CalBack, UriKind.Relative));
            return Redirect(string.Format(Util.URL.f1AuthorizeUrl, this.F1ChurchCode, this.F1RequestToken.Value, callback));
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

                return RedirectToAction("Ready");
            }
        }

        public ActionResult Ready() {
            if (this.F1AccessToken == null || this.PCOAccessToken == null) {
                return RedirectToAction("ChurchCode");
            }
            else {
                return View();
            }
        }

        public ActionResult Sync() {
            DateTime? lastRun = null;
            Model.F1.people f1People = null;
            Model.PCO.person person = null;

            //Get the ID of the attribute with the name SyncMe.  This is the attribute
            //that should be added to the people that need to be synced.
            this.AttributeID = this.F1GetAttributeID("SyncMe");

            if (this.AttributeID != 0) {
                lastRun = this.GetLastRun();

                if (lastRun != null) {
                    //Get the people that have been updated since the last time we ran
                    f1People = this.F1GetPeopleByLastUpdatedDate(lastRun.Value.ToString());
                }
                else {
                    //Since we've never run just get all people with the attribute
                    f1People = this.F1GetPeopleByAttribute(this.AttributeID.Value);
                }
               
                //Filter out the people who don't have the SyncMe Attribute.
                List<Model.F1.person> filteredPeople = f1People.FindByAttributeID(this.AttributeID.Value);

                foreach (Model.F1.person p in filteredPeople) {
                    //Get the comment for the attribute to see if we already now the PCOID.
                    Model.F1.peopleAttribute peopleAttribute = p.attributes.FindByID(this.AttributeID.Value);

                    if (peopleAttribute != null && !string.IsNullOrEmpty(peopleAttribute.comment)) {
                        /// PCO ID FOUND
                        /// We have the PCO ID and can update the record with confidence that it is
                        /// the correct person.

                        person = this.PCOGetPersonByID(Convert.ToInt32(peopleAttribute.comment));
                        if (person != null) {
                            try {
                                this.UpdatePerson(p, ref person);
                                this.PCOUpdatePerson(this.SerializeEntity(person), peopleAttribute.comment);
                            }
                            catch {
                                this.PersonErrors.Add(p);
                            }
                        }
                        else {
                            this.NoMatches.Add(new Model.MatchHelperData { F1Person = p, PCOPeople = null });
                        }
                    }
                    else {
                        /// NO ID FOUND ///
                        /// Didn't find a PCO ID in the attribute so we need to look up by name.

                        Model.PCO.people people = null;
                        people = this.PCOGetPersonByName(p.lastName + ", " + (string.IsNullOrEmpty(p.goesByName) ? p.firstName.Substring(0, 1) : p.goesByName.Substring(0, 1)));
                        
                        var email = p.communications.FindByCommunicationTypeName("Email");

                        if (email != null) {
                            people = people.FindByEmailAddress("Email");
                        }

                        switch (people.person.Count) {
                            case 0:

                                break;
                            case 1:

                                break;

                            default:

                                break;
                        }

                        if (people.person.Count > 0) {
                            //search by emial

                            //if 1 do the update

                            //if multiple add to collection


                        }




                        if (people.person.Count == 1) {
                            /// ONE MATCH FOUND ///
                            /// Update the PCO person based on the F1 data.  If data has been changed
                            /// save it to PCO.  Also add the PCO ID to the F1 attribute so we don't
                            /// have to look them up my name next time.

                            Model.PCO.person matchPerson = people.person.FirstOrDefault();
                            try {
                                try {
                                    this.UpdatePerson(p, ref matchPerson);
                                    if (matchPerson.IsDirty) {
                                        this.PCOUpdatePerson(this.SerializeEntity(matchPerson), matchPerson.id.Value);
                                    }
                                }
                                catch {
                                    this.PersonErrors.Add(p);
                                }
                            }
                            catch {
                                this.PersonErrors.Add(p);
                            }

                            peopleAttribute.comment = matchPerson.id.Value;
                            this.F1UpdatePeopleAttribute(peopleAttribute);
                        }
                        else if (people.person.Count == 0) {
                            ///NO MATCH FOUND ///
                            ///Just need to add it to the no match filter.

                            this.NoMatches.Add(new Model.MatchHelperData { F1Person = p, PCOPeople = null });
                        }
                        else {
                            /// MULTIPLE MATCHES ///
                            /// See if we can narrow down who we are looking for based on email address.
                            /// If we can't find them based on email then add them to the no match collection.

                            Model.PCO.person filteredPerson = null;
                            var email = p.communications.FindByCommunicationTypeName("Email");

                            if (email != null) {
                                filteredPerson = people.FindByEmailAddress(email.communicationValue);

                                if (filteredPerson != null) {
                                    try {
                                        this.UpdatePerson(p, ref filteredPerson);
                                        this.PCOUpdatePerson(this.SerializeEntity(filteredPerson), filteredPerson.id.Value);
                                    }
                                    catch {
                                        this.PersonErrors.Add(p);
                                    }
                                }
                            }

                            if (filteredPerson == null) {
                                this.Matches.Add(new Model.MatchHelperData { F1Person = p, PCOPeople = people });
                            }
                        }
                    }
                }

                if (this.NoMatches.Count > 0) {
                    return RedirectToAction("NonMatches");
                }

                if (this.Matches.Count > 0) {
                    return RedirectToAction("MultipleMatches");
                }
                return RedirectToAction("Success");
            }
            else {
                return View("NoAttribute");
            }
        }

        public ActionResult NonMatches() {
            return View(this.NoMatches);
        }

        public ActionResult ProcessNoMatches() {
            foreach (string s in Request.Form) {
                if (s.StartsWith("SyncIt")) {
                    string f1IndividualID = s.Split('-')[1];

                    Model.F1.person p = this.NoMatches.FindF1PersonByID(f1IndividualID);

                    if (!this.AttributeID.HasValue) {
                        this.AttributeID = this.F1GetAttributeID("SyncMe");
                    }
                    Model.F1.peopleAttribute peopleAttribute = p.attributes.FindByID(this.AttributeID.Value);

                    if (Request.Form[s].ToString() == "1") {
                        F1toPCO.Model.PCO.person pcop = new F1toPCO.Model.PCO.person();
                        this.UpdatePerson(p, ref pcop);

                        try {
                            Model.PCO.person createdPerson = this.PCOCreatePerson(this.SerializeEntity(pcop));
                            if (createdPerson != null) {
                                peopleAttribute.comment = createdPerson.id.Value;
                                this.F1UpdatePeopleAttribute(peopleAttribute);
                            }
                        }
                        catch {
                            this.PersonErrors.Add(p);

                        }
                    }
                    else {
                        this.F1DeletePeopleAttribute(peopleAttribute);
                    }
                }
            }
            this.NoMatches.Clear();

            if (this.Matches.Count > 0) {
                return RedirectToAction("MultipleMatches");
            }

            return RedirectToAction("Success");
        }

        public ActionResult MultipleMatches() {
            return View(this.Matches);
        }

        public ActionResult ProcessMatches() {

            foreach (string f1ID in Request.Form) {

                Model.PCO.person createdPerson = null;
                Model.F1.person p = this.Matches.FindF1PersonByID(f1ID);

                if (!this.AttributeID.HasValue) {
                    this.AttributeID = this.F1GetAttributeID("SyncMe");
                }

                Model.F1.peopleAttribute peopleAttribute = p.attributes.FindByID(this.AttributeID.Value);

                string f1IndividualID = Request.Form[f1ID].ToString();
                switch (Request.Form[f1ID].ToString()) {
                    case "0":
                        this.F1DeletePeopleAttribute(peopleAttribute);
                        break;

                    case "-1":
                        Model.PCO.person newPCO = new Model.PCO.person();
                        this.UpdatePerson(p, ref newPCO);

                        try {
                            createdPerson = this.PCOCreatePerson(this.SerializeEntity(newPCO));
                        }
                        catch {
                            this.PersonErrors.Add(p);
                        }

                        if (createdPerson != null) {
                            peopleAttribute.comment = createdPerson.id.Value;
                            this.F1UpdatePeopleAttribute(peopleAttribute);
                        }

                        break;

                    default:
                        var pcoPerson = this.Matches.FindPCOPersonByID(Request.Form[f1ID].ToString());
                        this.UpdatePerson(p, ref pcoPerson);

                        try {
                            this.PCOUpdatePerson(this.SerializeEntity(pcoPerson), Request.Form[f1ID].ToString());
                        }
                        catch {
                            this.PersonErrors.Add(p);
                        }
                        peopleAttribute.comment = pcoPerson.id.Value;
                        this.F1UpdatePeopleAttribute(peopleAttribute);

                        break;
                }
            }
            this.Matches.Clear();

            return RedirectToAction("Success");
        }

        public ActionResult Success() {
            this.SaveLastRun();
            this.Reset();
            return View(this.PersonErrors);
        }

        public ActionResult Trouble() {
            return View("Error");
        }

        public ActionResult Terms() {
            return View();
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
                Authority = string.Format(Util.URL.f1BaseUrl, this.F1ChurchCode),
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
                Authority = string.Format(Util.URL.f1BaseUrl, this.F1ChurchCode),
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

            //Save the ind id of the person that logged in.
            if (response.ResponseUri != null) {
                string responseUri = response.ResponseUri.ToString();
                this.SaveTerms(responseUri.Substring(responseUri.LastIndexOf("/") + 1));
            }
        }

        private void GetPCORequestToken() {

            var pcoCreds = new OAuthCredentials {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = Util.PrivateConsts.pcoConsumerKey,
                ConsumerSecret = Util.PrivateConsts.pcoConsumerSecret,
                CallbackUrl = Url.ToPublicUrl(new Uri(Util.URL.pcoCallback, UriKind.Relative))
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f1Emails"></param>
        /// <param name="pcoEmails"></param>
        private void UpdateEmailCommunications(F1toPCO.Model.F1.communications f1Emails, F1toPCO.Model.PCO.emailAddresses pcoEmails) {
            foreach (F1toPCO.Model.F1.EntityType et in F1toPCO.Model.F1.emailSyncType.Items) {
                F1toPCO.Model.F1.communication tmpF1Email = f1Emails.FindByCommunicationTypeName(et.F1Type);
                F1toPCO.Model.PCO.emailAddress tmpPCOEmail = pcoEmails.FindByLocation(et.PCOType);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f1Phones"></param>
        /// <param name="pcoPhones"></param>
        private void UpdatePhoneCommunications(F1toPCO.Model.F1.communications f1Phones, F1toPCO.Model.PCO.phoneNumbers pcoPhones) {
            foreach (F1toPCO.Model.F1.EntityType et in F1toPCO.Model.F1.phoneSyncType.Items) {
                F1toPCO.Model.F1.communication tmpF1Phone = f1Phones.FindByCommunicationTypeName(et.F1Type);
                F1toPCO.Model.PCO.phoneNumber tmpPCOPhone = pcoPhones.FindByLocation(et.PCOType);

                if (tmpF1Phone != null) {
                    if (tmpPCOPhone == null) {
                        tmpPCOPhone = new Model.PCO.phoneNumber();
                        tmpPCOPhone.number = tmpF1Phone.communicationValue;
                        tmpPCOPhone.location = et.PCOType;
                        pcoPhones.phoneNumber.Add(tmpPCOPhone);
                    }
                    else {
                        tmpPCOPhone.number = tmpF1Phone.communicationValue;
                    }
                }
                else {
                    if (tmpPCOPhone != null) {
                        pcoPhones.phoneNumber.Remove(tmpPCOPhone);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f1Person"></param>
        /// <param name="pcoPerson"></param>
        private void UpdatePerson(F1toPCO.Model.F1.person f1Person, ref F1toPCO.Model.PCO.person pcoPerson) {

            pcoPerson.firstname = string.IsNullOrEmpty(f1Person.goesByName) ? f1Person.firstName : f1Person.goesByName;
            pcoPerson.lastname = f1Person.lastName;

            //Emails
            F1toPCO.Model.F1.communications emailComms = new F1toPCO.Model.F1.communications();
            emailComms.items = f1Person.communications.FindByGeneralCommunicationType(F1toPCO.Model.F1.communicationGeneralType.Email);
            UpdateEmailCommunications(emailComms, pcoPerson.contactData.emailAddresses);

            //Phone numbers
            F1toPCO.Model.F1.communications phoneComs = new F1toPCO.Model.F1.communications();
            phoneComs.items = f1Person.communications.FindByGeneralCommunicationType(F1toPCO.Model.F1.communicationGeneralType.Telephone);
            UpdatePhoneCommunications(phoneComs, pcoPerson.contactData.phoneNumbers);

            //Address
            F1toPCO.Model.F1.address primaryAddress = f1Person.addresses.FindByType("Primary");
            F1toPCO.Model.PCO.address pcoAddress = pcoPerson.contactData.addresses.FindByLocation("Home");
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
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Save the last time we ran
        /// </summary>
        private void SaveLastRun() {
            SqlCommand sqlcom = new SqlCommand();
            var constring = ConfigurationManager.ConnectionStrings["F1toPCO"];
            SqlConnection sqlConn = new SqlConnection(constring.ToString());

            sqlcom.Connection = sqlConn;
            sqlcom.CommandType = CommandType.StoredProcedure;
            sqlcom.CommandText = "UpdateLastRun";            
            sqlcom.Parameters.Add(new SqlParameter("@ChurchCode", this.F1ChurchCode));
            sqlConn.Open();
            sqlcom.ExecuteNonQuery();
            sqlConn.Close();
        }

        private void SaveTerms(string individual) {
            SqlCommand sqlcom = new SqlCommand();
            var constring = ConfigurationManager.ConnectionStrings["F1toPCO"];
            SqlConnection sqlConn = new SqlConnection(constring.ToString());

            sqlcom.Connection = sqlConn;
            sqlcom.CommandType = CommandType.StoredProcedure;
            sqlcom.CommandText = "UpdateTerms";            
            sqlcom.Parameters.Add(new SqlParameter("@ChurchCode", this.F1ChurchCode));
            sqlcom.Parameters.Add(new SqlParameter("@Individual", individual));
            sqlConn.Open();
            sqlcom.ExecuteNonQuery();
            sqlConn.Close();
        }        

        /// <summary>
        /// Get the last time we ran.
        /// </summary>
        /// <returns></returns>
        private DateTime? GetLastRun() {
            SqlCommand sqlcom = new SqlCommand();
            var constring = ConfigurationManager.ConnectionStrings["F1toPCO"];
            SqlConnection sqlConn = new SqlConnection(constring.ToString());

            sqlcom.Connection = sqlConn;
            sqlcom.CommandType = CommandType.Text;
            sqlcom.CommandText ="Select LastRun FROM ChurchData WHERE ChurchCode = '" + this.F1ChurchCode + "'";
            sqlConn.Open();
            var lastRun = sqlcom.ExecuteScalar();
            sqlConn.Close();
            return lastRun as DateTime?;
        }        

        /// <summary>
        /// Removes all the stuff from session so we can start over
        /// </summary>
        private void Reset() {
            Session.Remove("AttributeID");
            Session.Remove("Matches");
            Session.Remove("NoMatches");            
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
                    Authority = string.Format(Util.URL.f1BaseUrl, this.F1ChurchCode),
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
                        attributeId = attributes.FindAttributeIDByName(name);
                    }
                }
                else {
                    throw new Exception("An error occured: Status code: " + response.StatusCode, response.InnerException);
                }
            }
            return attributeId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastDate"></param>
        /// <returns></returns>
        private F1toPCO.Model.F1.people F1GetPeopleByLastUpdatedDate(string lastDate) {
            F1toPCO.Model.F1.people peopleCollection = null;

            RestRequest request = new RestRequest {
                Path = "People/Search"
            };
            request.AddParameter("lastUpdatedDate", lastDate);
            request.AddParameter("recordsperpage", "1000");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeId"></param>
        /// <returns></returns>
        private F1toPCO.Model.F1.people F1GetPeopleByAttribute(int attributeId) {

            F1toPCO.Model.F1.people peopleCollection = null;

            RestRequest request = new RestRequest {
                Path = "People/Search"
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private F1toPCO.Model.PCO.person PCOGetPersonByID(int id) {

            F1toPCO.Model.PCO.people peopleResults = null;

            var request = new RestRequest {
                Path = "people.xml"
            };

            request.AddParameter("people_ids", id.ToString());

            using (RestResponse response = PCOClient.Request(request)) {
                if (response.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(response.ContentStream)) {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(F1toPCO.Model.PCO.people));

                        // Deserialize the response into a Person object.
                        peopleResults = xmlSerializer.Deserialize(streamReader) as F1toPCO.Model.PCO.people;
                    }
                }
                else {
                    throw new Exception("An error occured: Status code: " + response.StatusCode, response.InnerException);
                }
            }
            return peopleResults != null ? peopleResults.person.FirstOrDefault() : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="peopleAttribute"></param>
        /// <returns></returns>
        private bool F1UpdatePeopleAttribute(Model.F1.peopleAttribute peopleAttribute) {

            var request = new RestRequest {
                Path = string.Format("People/{0}/Attributes/{1}", peopleAttribute.person.id, peopleAttribute.id),
                VersionPath = "v1",
                Method = WebMethod.Put,
                Entity = this.SerializeEntity(peopleAttribute)
            };

            RestResponse response = F1Client.Request(request);
            if (response.StatusCode == HttpStatusCode.OK) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="peopleAttribute"></param>
        /// <returns></returns>
        private bool F1DeletePeopleAttribute(Model.F1.peopleAttribute peopleAttribute) {
            var request = new RestRequest {
                Path = string.Format("People/{0}/Attributes/{1}", peopleAttribute.person.id, peopleAttribute.id),
                VersionPath = "v1",
                Method = WebMethod.Delete
            };

            RestResponse response = F1Client.Request(request);
            if (response.StatusCode == HttpStatusCode.NoContent) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private Model.PCO.person PCOCreatePerson(string xml) {

            F1toPCO.Model.PCO.person person = null;

            var request = new RestRequest {
                Path = "people.xml",
                Method = WebMethod.Post,
                Entity = xml
            };

            using (RestResponse response = PCOClient.Request(request)) {
                if (response.StatusCode == HttpStatusCode.Created) {
                    using (StreamReader streamReader = new StreamReader(response.ContentStream)) {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(F1toPCO.Model.PCO.person));

                        // Deserialize the response into a Person object.
                        person = xmlSerializer.Deserialize(streamReader) as F1toPCO.Model.PCO.person;
                    }
                }
                else {
                    throw new Exception("An error occured: Status code: " + response.StatusCode, response.InnerException);
                }
            }
            return person;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
