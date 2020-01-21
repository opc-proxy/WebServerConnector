

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
        public bool enableBasicAuth {get; set;}
        public bool enableStaticFiles {get; set;}
        public bool enableAPIkey {get; set;}

        public string apyKey {get; set;}
        public string basicAuthPassword {get; set;}
        public string staticFilesPath {get; set;}


        public RESTconfigs(){
            apyKey = "";
            basicAuthPassword = "";
            staticFilesPath = "./public/";
            enableAPIkey = false;
            enableStaticFiles = false;
            enableBasicAuth = false;
            host = "localhost";
            port = "8082";
            https = false;
            urlPrefix = "";
        }

    }
    


}