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
    public  class internalCssController : WebApiController{

        [Route(HttpVerbs.Get, "/milligram.css")]
        public async Task milligram(){
            await HttpContext.SendStringAsync( CSStemplates.milligram,"text/css",Encoding.UTF8);
        }
        [Route(HttpVerbs.Get, "/layout.css")]
        public async Task layout(){
            await HttpContext.SendStringAsync( CSStemplates.layout,"text/css",Encoding.UTF8);
        }
    }
}