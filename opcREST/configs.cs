

namespace opcRESTconnector
{
    /// <summary>
    /// Wrapper class for the confs to respect JSON hierarchy
    /// </summary>
    public class RESTconfigsWrapper{

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

        public RESTconfigs(){
            host = "localhost";
            port = "8082";
            https = false;
            urlPrefix = "";
        }

    }
    


}