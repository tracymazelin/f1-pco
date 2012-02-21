using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace F1toPCO.Util {
    public class PrivateConsts {
        //Staging
        public static string f1ConsumerKey = ConfigurationManager.AppSettings["f1ConsumerKey"];
        public static string f1ConsumerSecret = ConfigurationManager.AppSettings["f1ConsumerSecret"];

        public static string pcoConsumerKey = ConfigurationManager.AppSettings["pcoConsumerKey"];
        public static string pcoConsumerSecret = ConfigurationManager.AppSettings["pcoConsumerSecret"];
    }
}