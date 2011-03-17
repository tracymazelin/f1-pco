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
    public class Token
    {
        public string Value;
        public string Secret;
        public string Verifier;
    }
}
