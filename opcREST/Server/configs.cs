using System;

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
        public bool enableREST {get; set;}
        public bool enableJSON {get; set;}
        public double sessionExpiryHours{get; set;}
        public double writeExpiryMinutes{get; set;}
        public string staticFilesPath {get; set;}
        public bool serverLog {get; set;}
        public string appStoreFileName{get; set;}
        public bool recoveryMode{get;set;}
        public string sendGridEmail {get;set;}

        private EnvVars _envVars;

        public EnvVars GetEnvVars(){
            return _envVars;
        }

        public bool isRecaptchaEnabled(){
            return (_envVars.recaptchaClientKey != "" && _envVars.recaptchaServerKey !="");
        }

        public RESTconfigs(){
            staticFilesPath = "./public/";
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
            appStoreFileName = "webserver.data.db";
            recoveryMode = false;
            sendGridEmail="admin@gmail.com";
            _envVars = new EnvVars();
        }

    }
    
    public class EnvVars{
        public string recaptchaClientKey;
        public string recaptchaServerKey;
        public string sendGridAPIkey;
        public string apiKey;

        public EnvVars()
        {
            recaptchaClientKey = Environment.GetEnvironmentVariable("OPC_WEBSERVER_RECAPTCHA_C") ?? "";
            recaptchaServerKey = Environment.GetEnvironmentVariable("OPC_WEBSERVER_RECAPTCHA_S") ?? "";
            sendGridAPIkey = Environment.GetEnvironmentVariable("OPC_WEBSERVER_SENDGRID") ?? "";
            apiKey = Environment.GetEnvironmentVariable("OPC_WEBSERVER_APIKEY") ?? "";
        }
    }


}