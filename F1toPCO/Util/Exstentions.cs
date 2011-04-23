using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace F1toPCO.Util {
    public static class Exstentions {
        public static string ToPublicUrl(this UrlHelper urlHelper, Uri relativeUri) {

            var httpContext = urlHelper.RequestContext.HttpContext;

            var uriBuilder = new UriBuilder {
                Host = httpContext.Request.Url.Host,
                Path = "/",
                Port = 80,
                Scheme = "http",
            };

            if (httpContext.Request.IsLocal) {
                uriBuilder.Path = "/F1toPCO/";
                uriBuilder.Port = httpContext.Request.Url.Port;
            }

            return new Uri(uriBuilder.Uri, relativeUri).AbsoluteUri;
        }
    }
}