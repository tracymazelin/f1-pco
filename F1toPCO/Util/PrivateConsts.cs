using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace F1toPCO.Util {
    public class PrivateConsts {
        //public const string f1AuthorizeUrl = "https://{0}.staging.fellowshiponeapi.com/v1/PortalUser/Login?oauth_token={1}&oauth_callback={2}";
        public const string f1AuthorizeUrl = "http://{0}.fellowshiponeapi.local/v1/PortalUser/Login?oauth_token={1}&oauth_callback={2}";
        public const string f1CalBack = "http://localhost/F1toPCO/Home/CallBack?provider=f1";
        public const string pcoAuthorizeUrl = "https://www.planningcenteronline.com/oauth/authorize?oauth_token={0}";
        public const string pcoCallback = "http://localhost/F1toPCO/Home/CallBack?provider=pco";
    }
}