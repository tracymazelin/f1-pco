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
    public class OAuthUtil
    {
        #region Properties
        private string _consumerKey = string.Empty;
        public string ConsumerKey
        {
            get { return _consumerKey; }
        }
        private string _consumerSecret = string.Empty;
        public string ConsumerSecret
        {
            get { return _consumerSecret; }
        }
        private string _baseAPIUrl = string.Empty;
        protected string BaseAPIUrl
        {
            get { return _baseAPIUrl; }
        }
        private string _apiVersion = string.Empty;
        protected string ApiVersion
        {
            get { return _apiVersion; }
        }
        private string _churchCode = string.Empty;
        protected string ChurchCode
        {
            get { return _churchCode; }
        }
        private string _requestUrl = string.Empty;
        protected string RequestUrl
        {
            get { return _requestUrl; }
        }
        private string _userAuthorizeUrl = string.Empty;
        protected string UserAuthorizeUrl
        {
            get { return _userAuthorizeUrl; }
        }
        private string _accessUrl = string.Empty;
        protected string AccessUrl
        {
            get { return _accessUrl; }
        }
        #endregion Properties

        public OAuthUtil(string baseAPIUrl, string churchCode, string apiVersion, string f1LoginMethod, string consumerKey, string consumerSecret)
        {
            _baseAPIUrl = baseAPIUrl;
            _apiVersion = apiVersion;
            _churchCode = churchCode;
            _requestUrl = CreateAPIUrl(baseAPIUrl, apiVersion, "Tokens/RequestToken");
            _userAuthorizeUrl = CreateAPIUrl(baseAPIUrl, apiVersion, f1LoginMethod + "/Login");
            _accessUrl = CreateAPIUrl(baseAPIUrl, apiVersion, "Tokens/AccessToken");
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
        }

        public OAuthUtil() {
            _requestUrl = "https://www.planningcenteronline.com/oauth/request_token";
            _accessUrl = "https://www.planningcenteronline.com/oauth/access_token";
            _userAuthorizeUrl = "https://www.planningcenteronline.com/oauth/authorize";
            _consumerKey = "HfK94IoIKmm40sHeVykg";
            _consumerSecret = "wBWSl0szv2PhuGSxBUf7xyUjnnW389Bzou6EgPFA";
        }
 
        #region Constants
        private const string OAuthVersion = "1.0";
        private const string OAuthParameterPrefix = "oauth_";
    
        private const string OAuthConsumerKeyKey = "oauth_consumer_key";
        private const string OAuthCallbackKey = "oauth_callback";
        private const string OAuthVersionKey = "oauth_version";
        private const string OAuthSignatureMethodKey = "oauth_signature_method";
        private const string OAuthSignatureKey = "oauth_signature";
        private const string OAuthTimestampKey = "oauth_timestamp";
        private const string OAuthNonceKey = "oauth_nonce";
        private const string OAuthTokenKey = "oauth_token";
        private const string OAuthTokenSecretKey = "oauth_token_secret";

        private const string HMACSHA1SignatureType = "HMAC-SHA1";
        private const string PlainTextSignatureType = "PLAINTEXT";
        private const string RSASHA1SignatureType = "RSA-SHA1";

        private const string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
        #endregion Constants

        /// <summary>
        /// Creates a full url for the api. (e.g. "https://mychurchcode.staging.fellowshiponeapi.com/v1/WeblinkUser/Login")
        /// </summary>
        /// <param name="baseAPIUrl"></param>
        /// <param name="apiVersion"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string CreateAPIUrl(string baseAPIUrl, string apiVersion, string partialUrl)
        {
            return string.Format("http://{0}.{1}/{2}/{3}", this.ChurchCode, baseAPIUrl, apiVersion, partialUrl);
        }

        /// <summary>
        /// Step #1:  Get an Unauthenticated request token
        /// </summary>
        /// <returns></returns>
        public Token GetRequestToken()
        {
            Token requestToken = null;
            Uri url = new Uri(RequestUrl);
            string nonce = GenerateNonce();
            string timestamp = GenerateTimeStamp();
            string normalizedUrl = string.Empty;
            string normalizedReqParms = string.Empty;
            string signatureBase = string.Empty;
            // First we generate a signature string. This incorporates multi-layered security goodness to make sure the url we're about to send cannot be used by anyone else,
            //  cannot be used more than once, and cannot be used outside of a specific time period.
            string sig = GenerateSignature(url, ConsumerKey, ConsumerSecret, null, null, "GET", timestamp, nonce, out normalizedUrl, out normalizedReqParms, out signatureBase);

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                // Everything gets stamped into a header (including the url)
                string authHeader = BuildOAuthHeader(ConsumerKey, nonce, sig, "HMAC-SHA1", timestamp, "");
                request.Headers.Add("Authorization", authHeader);
                request.ContentType = "appliation/xml";
                request.Method = "GET";

                // Execute the webRequest
                WebResponse webResponse = request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                // Read the response into a string
                string results = sr.ReadToEnd().Trim();

                // Parse out the request token
                string[] tokeninfo = results.Split('&');
                requestToken = new Token();
                requestToken.Value = tokeninfo[0].Replace("oauth_token=", "");
                requestToken.Secret = tokeninfo[1].Replace("oauth_token_secret=", "");
            }
            catch (WebException we)
            {
                HttpWebResponse wr = (HttpWebResponse)we.Response;
                StringBuilder error = new StringBuilder();
                error.Append(we.Message).Append(" <br/>Reason: ").Append(wr.StatusDescription).Append("<br><br>");

                if (wr.Headers["oauth_signature_base_debug"] != null)
                {
                    error.Append("<br><br>").Append(signatureBase);
                    error.Append("<br><br>").Append(wr.Headers["oauth_signature_base_debug"].ToString());
                }

                if (wr.Headers["oauth_signature_debug"] != null)
                {
                    error.Append("<br><br>").Append(sig);
                    error.Append("<br><br>").Append(wr.Headers["oauth_signature_debug"].ToString());
                }
                throw new Exception(error.ToString());
            }

            return requestToken;
        }

        /// <summary>
        /// Step #2: Authenticate the request token. This method builds a url to send the user off to, so they can login with their FT login.  After logging in, the FT API
        ///  will redirect them to the callbackUrl supplied.  We send the requestToken in the url.  When our callback url is called, it will contain a querystring parm
        ///  with the authorized request token (if the user logged in successfully).
        /// </summary>
        /// <param name="token"></param>
        /// <param name="callbackUrl"></param>
        /// <returns></returns>
        public string RequestUserAuth(string token, string callbackUrl)
        {
            var builder = new UriBuilder(UserAuthorizeUrl); // We've hardcoded to use WebLink login in our url constant
            var collection = new NameValueCollection();
            var queryParameters = new NameValueCollection();

            if (builder.Query != null)
            {
                collection.Add(System.Web.HttpUtility.ParseQueryString(builder.Query));
            }

            if (queryParameters != null)
                collection.Add(queryParameters);

            collection["oauth_token"] = token;

            if (!string.IsNullOrEmpty(callbackUrl))
            {
                collection["oauth_callback"] = callbackUrl;
            }

            builder.Query = "";

            return builder.Uri + "?" + FormatQueryString(collection);

        }

        /// <summary>
        /// Step #3:  Trade the authorized request token for an Access Token
        /// </summary>
        /// <param name="requestToken"></param>
        /// <param name="personUrl"></param>
        /// <returns></returns>
        public Token GetAccessToken(Token requestToken, out string personUrl)
        {
            Token accessToken = null;
            Uri url = new Uri(AccessUrl);
            string nonce = GenerateNonce();
            string timestamp = GenerateTimeStamp();
            string normalizedUrl = string.Empty;
            string normalizedReqParms = string.Empty;
            string signatureBase = string.Empty;
            string sig = GenerateSignature(url, ConsumerKey, ConsumerSecret, requestToken.Value, requestToken.Secret, "GET", timestamp, nonce, out normalizedUrl, out normalizedReqParms, out signatureBase);
            personUrl = string.Empty;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            string authHeader = BuildOAuthHeader(ConsumerKey, nonce, sig, HMACSHA1SignatureType, timestamp, requestToken.Value);
            request.Headers.Add("Authorization", authHeader);
            request.ContentType = "appliation/xml";
            request.Method = "GET";

            try
            {
                WebResponse webResponse = request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());

                if (webResponse.Headers["Content-Location"] != null)
                {
                    personUrl = webResponse.Headers["Content-Location"].ToString();
                }

                string results = sr.ReadToEnd().Trim();

                string[] tokeninfo = results.Split('&');
                accessToken = new Token();
                accessToken.Value = tokeninfo[0].Replace("oauth_token=", "");
                accessToken.Secret = tokeninfo[1].Replace("oauth_token_secret=", "");
            }
            catch (WebException we)
            {
                HttpWebResponse wr = (HttpWebResponse)we.Response;
                StringBuilder error = new StringBuilder();
                error.Append(we.Message).Append(" <br/>Reason: ").Append(wr.StatusDescription).Append("<br><br>");

                if (wr.Headers["oauth_signature_base_debug"] != null)
                {
                    error.Append("<br><br>").Append(signatureBase);
                    error.Append("<br><br>").Append(wr.Headers["oauth_signature_base_debug"].ToString());
                }

                if (wr.Headers["oauth_signature_debug"] != null)
                {
                    error.Append("<br><br>").Append(sig);
                    error.Append("<br><br>").Append(wr.Headers["oauth_signature_debug"].ToString());
                }
                throw new Exception(error.ToString());
            }
            return accessToken;
        }

        public HttpWebRequest CreateWebRequestFromPartialUrl(string partialUrl, Token accessToken, HttpRequestMethod httpRequestMethod)
        {
            string fullUrl = CreateAPIUrl(BaseAPIUrl, ApiVersion, partialUrl);
            return CreateWebRequest(fullUrl, accessToken, httpRequestMethod);
        }

        public HttpWebRequest CreateWebRequest(string fullUrl, Token accessToken, HttpRequestMethod httpRequestMethod)
        {
            HttpWebRequest webRequest;

            Uri uri = new Uri(fullUrl);
            string nonce = GenerateNonce();
            string timestamp = GenerateTimeStamp();
            string normalizedUrl = string.Empty;
            string normalizedReqParms = string.Empty;
            string signatureBase = string.Empty;
            string httpMethod = httpRequestMethod.ToString();
            string sig = GenerateSignature(uri, ConsumerKey, ConsumerSecret, accessToken.Value, accessToken.Secret, httpMethod, timestamp, nonce, out normalizedUrl, out normalizedReqParms, out signatureBase);

            webRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
            string authHeader = BuildOAuthHeader(ConsumerKey, nonce, sig, HMACSHA1SignatureType, timestamp, accessToken.Value);
            webRequest.Headers.Add("Authorization", authHeader);
            webRequest.Accept = "application/xml";
            webRequest.Method = httpMethod;

            return webRequest;
        }

        // This is only used for first or second party usage.  We're using Third party - we have F1 generate the login page and accept the credentials.
        //public string BuildCredentials(string username, string password) {
        //    string credentials = username + " " + password;
        //    ASCIIEncoding a = new ASCIIEncoding();
        //    byte[] authBytes = a.GetBytes(credentials);
        //    credentials = Convert.ToBase64String(authBytes, 0, authBytes.Length);

        //    return credentials;
        //}

        /// <summary>
        /// Generate the signature base that is used to produce the signature
        /// </summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>        
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="signatureType">The signature type. To use the default values use <see cref="OAuthBase.SignatureTypes">OAuthBase.SignatureTypes</see>.</param>
        /// <returns>The signature base</returns>
        public string GenerateSignatureBase(Uri url, string consumerKey, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, string signatureType, out string normalizedUrl, out string normalizedRequestParameters)
        {
            if (token == null)
            {
                token = string.Empty;
            }

            if (tokenSecret == null)
            {
                tokenSecret = string.Empty;
            }

            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentNullException("httpMethod");
            }

            if (string.IsNullOrEmpty(signatureType))
            {
                throw new ArgumentNullException("signatureType");
            }

            normalizedUrl = null;
            normalizedRequestParameters = null;

            List<QueryParameter> parameters = GetQueryParameters(url.Query);
            parameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));
            parameters.Add(new QueryParameter(OAuthNonceKey, nonce));
            parameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
            parameters.Add(new QueryParameter(OAuthSignatureMethodKey, signatureType));
            parameters.Add(new QueryParameter(OAuthConsumerKeyKey, consumerKey));

            if (!string.IsNullOrEmpty(token))
            {
                parameters.Add(new QueryParameter(OAuthTokenKey, token));
            }

            parameters.Sort(new QueryParameterComparer());

            normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
            {
                normalizedUrl += ":" + url.Port;
            }
            normalizedUrl += url.AbsolutePath;
            ////normalizedUrl = normalizedUrl.Replace("http", "https");
            normalizedRequestParameters = NormalizeRequestParameters(parameters);

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
            signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        /// <summary>
        /// Generate the signature value based on the given signature base and hash algorithm
        /// </summary>
        /// <param name="signatureBase">The signature based as produced by the GenerateSignatureBase method or by any other means</param>
        /// <param name="hash">The hash algorithm used to perform the hashing. If the hashing algorithm requires initialization or a key it should be set prior to calling this method</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }

        /// <summary>
        /// Generates a signature using the HMAC-SHA1 algorithm
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out string normalizedRequestParameters, out string signatureBase)
        {
            return GenerateSignature(url, consumerKey, consumerSecret, token, tokenSecret, httpMethod, timeStamp, nonce, SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedRequestParameters, out signatureBase);
        }

        /// <summary>
        /// Generates a signature using the specified signatureType 
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="signatureType">The type of signature to use</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, SignatureTypes signatureType, out string normalizedUrl, out string normalizedRequestParameters, out string signatureBase)
        {
            normalizedUrl = null;
            normalizedRequestParameters = null;
            signatureBase = string.Empty;

            switch (signatureType)
            {
                case SignatureTypes.PLAINTEXT:
                    return System.Web.HttpUtility.UrlEncode(string.Format("{0}&{1}", consumerSecret, tokenSecret));
                case SignatureTypes.HMACSHA1:
                    signatureBase = GenerateSignatureBase(url, consumerKey, token, tokenSecret, httpMethod, timeStamp, nonce, HMACSHA1SignatureType, out normalizedUrl, out normalizedRequestParameters);
                    HMACSHA1 hmacsha1 = new HMACSHA1();
                    hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)));
                    return GenerateSignatureUsingHash(signatureBase, hmacsha1);
                case SignatureTypes.RSASHA1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
            }
        }

        /// <summary>
        /// Generate the timestamp for the signature        
        /// </summary>
        /// <returns></returns>
        public string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            //TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan now = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);

            return Convert.ToInt64(now.TotalSeconds).ToString();
        }

        /// <summary>
        /// Generate a nonce
        /// </summary>
        /// <returns></returns>
        public string GenerateNonce()
        {
            return Guid.NewGuid().ToString();
        }

        #region Private methods
        /// <summary>
        /// Helper function to compute a hash value
        /// </summary>
        /// <param name="hashAlgorithm">The hashing algoirhtm used. If that algorithm needs some initialization, like HMAC and its derivatives, they should be initialized prior to passing it to this function</param>
        /// <param name="data">The data to hash</param>
        /// <returns>a Base64 string of the hash value</returns>
        private string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }

            byte[] dataBuffer = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Internal function to cut out all non oauth query string parameters (all parameters not begining with "oauth_")
        /// </summary>
        /// <param name="parameters">The query string part of the Url</param>
        /// <returns>A list of QueryParameter each containing the parameter name and value</returns>
        private List<QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }

            List<QueryParameter> result = new List<QueryParameter>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
        /// </summary>
        /// <param name="value">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        private string UrlEncode(string value)
        {
            StringBuilder result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (UnreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Normalizes the request parameters according to the spec
        /// </summary>
        /// <param name="parameters">The list of parameters already sorted</param>
        /// <returns>a string representing the normalized parameters</returns>
        private string NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            QueryParameter p = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }

        private string BuildOAuthHeader(string consumerKey, string nonce, string signature, string signatureMethod, string timestamp, string token)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("OAuth oauth_consumer_key=").Append(System.Web.HttpUtility.UrlEncode(consumerKey)).Append(",");
            sb.Append("oauth_nonce=").Append(nonce).Append(",");
            sb.Append("oauth_signature=").Append(System.Web.HttpUtility.UrlEncode(signature)).Append(",");
            sb.Append("oauth_signature_method=").Append(signatureMethod).Append(",");
            sb.Append("oauth_timestamp=").Append(timestamp).Append(",");
            sb.Append("oauth_token=").Append(token).Append(",");
            sb.Append("oauth_version=").Append("1.0");
            return sb.ToString();
        }

        /// <summary>
        /// Formats a set of query parameters, as per query string encoding.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string FormatQueryString(NameValueCollection parameters)
        {
            var builder = new StringBuilder();

            if (parameters != null)
            {
                foreach (string key in parameters.Keys)
                {
                    if (builder.Length > 0) builder.Append("&");
                    builder.Append(key).Append("=");
                    builder.Append(UrlEncode(parameters[key]));
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Takes an http method, url and a set of parameters and formats them as a signature base as per the OAuth core spec.
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string FormatParameters(string httpMethod, Uri url, List<QueryParameter> parameters)
        {
            string normalizedRequestParameters = NormalizeRequestParameters(parameters);

            var signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());

            signatureBase.AppendFormat("{0}&", UrlEncode(NormalizeUri(url)));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        /// <summary>
        /// Normalizes a Url according to the OAuth specification (this ensures http or https on a default port is not displayed
        /// with the :XXX following the host in the url).
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private string NormalizeUri(Uri uri)
        {
            string normalizedUrl = string.Format("{0}://{1}", uri.Scheme, uri.Host);

            if (!((uri.Scheme == "http" && uri.Port == 80) ||
                  (uri.Scheme == "https" && uri.Port == 443)))
            {
                normalizedUrl += ":" + uri.Port;
            }

            return normalizedUrl + ((uri.AbsolutePath == "/") ? "" : uri.AbsolutePath);
        }
        #endregion Private methods

        #region Internal classes
        /// <summary>
        /// Provides a predefined set of algorithms that are supported officially by the protocol
        /// </summary>
        public enum SignatureTypes
        {
            HMACSHA1,
            PLAINTEXT,
            RSASHA1
        }

        /// <summary>
        /// Provides an internal structure to sort the query parameter
        /// </summary>
        private class QueryParameter
        {
            private string name = null;
            private string value = null;

            public QueryParameter(string name, string value)
            {
                this.name = name;
                this.value = value;
            }

            public string Name
            {
                get { return name; }
            }

            public string Value
            {
                get { return value; }
            }
        }

        /// <summary>
        /// Comparer class used to perform the sorting of the query parameters
        /// </summary>
        private class QueryParameterComparer : IComparer<QueryParameter>
        {

            #region IComparer<QueryParameter> Members

            public int Compare(QueryParameter x, QueryParameter y)
            {
                if (x.Name == y.Name)
                {
                    return string.Compare(x.Value, y.Value, StringComparison.Ordinal);
                }
                else
                {
                    return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                }
            }

            #endregion
        }
        #endregion Internal classes
    }

    public class Token
    {
        public string Value;
        public string Secret;
    }

}
