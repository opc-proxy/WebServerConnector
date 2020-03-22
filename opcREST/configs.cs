using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
namespace opcRESTconnector
{
    /// <summary>
    /// Wrapper class for the confs to respect JSON hierarchy
    /// </summary>
    public class RESTconfigsWrapper{
        public RESTconfigs RESTapi {get; set;}
        public RESTconfigsWrapper(){
            RESTapi = new RESTconfigs();
        }
    }

    public class RESTconfigs{

        public string host {get;set;}
        /// <summary>
        /// port on which to listen. default is 8082.
        /// </summary>
        public string port {get;set;}
        /// <summary>
        /// Either true for "https", default false
        /// </summary>
        public bool https {get;set;}

        /// <summary>
        /// URL of your endpoint plus a prefix. Example: "test" would get you to "http://localhost:port/test/" as your root.
        /// </summary>
        public string urlPrefix {get; set;}
        public bool enableCookieAuth {get; set;}
        public bool enableStaticFiles {get; set;}
        public bool enableAPIkey {get; set;}
        public bool enableREST {get; set;}
        public bool enableJSON {get; set;}
        public double sessionExpiryHours{get; set;}
        public double writeExpiryMinutes{get; set;}
        public string apyKey {get; set;}
        public List<List<string>> userAuth {get; set;}
        public string staticFilesPath {get; set;}
        public bool serverLog {get; set;}
        public string recaptchaClientKey{get; set;}
        public string recaptchaServerKey{get; set;}
        public string appStoreFileName{get; set;}
        public bool recoveryMode{get;set;}

        public bool isRecaptchaEnabled(){
            return (recaptchaClientKey != "" && recaptchaServerKey !="");
        }

        public RESTconfigs(){
            apyKey = "";
            userAuth = new List<List<string>>(){};
            staticFilesPath = "./public/";
            enableAPIkey = false;
            enableStaticFiles = false;
            enableCookieAuth = false;
            enableREST = true;
            enableJSON = true;
            host = "localhost";
            port = "8082";
            https = false;
            urlPrefix = "";
            sessionExpiryHours = 720;
            writeExpiryMinutes = 30;
            serverLog = true;
            recaptchaClientKey = "";
            recaptchaServerKey = "";
            appStoreFileName = "webserver.data.db";
            recoveryMode = false;
        }

    }
    


}