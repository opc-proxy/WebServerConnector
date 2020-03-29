using System.Collections.Generic;
using System.Threading.Tasks;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using EmbedIO;
using System;
using Newtonsoft.Json.Linq;
using System.Text;
using opcRESTconnector.Session;
using System.Net.Http;
using System.Threading;
using opcRESTconnector;
using OpcProxyCore;
using Newtonsoft.Json;
using System.Net.Http.Headers ;
using System.Net;

namespace opcRESTconnector{
    public  class internalJSController : WebApiController{

        [Route(HttpVerbs.Get, "/htmlescape.js")]
        public async Task htmlescape(){
            await HttpContext.SendStringAsync( jsTEmplates.htmlescape,"text/javascript",Encoding.UTF8);
        }
        [Route(HttpVerbs.Get, "/admin_utils.js")]
        public async Task admin_utils(){
            await HttpContext.SendStringAsync( jsTEmplates.admin_utils,"text/javascript",Encoding.UTF8);
        }
    }
}