using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using Swan.Logging;
using EmbedIO.Authentication;
using System;
using OpcProxyCore;

namespace opcRESTconnector {

    public class HTTPServerBuilder {

        /// <summary>
        /// 
        /// </summary>
        //// <param name="url"></param>
        /// <returns></returns>
        public static WebServer CreateWebServer (RESTconfigs conf, serviceManager manager) {
            
            string url = conf.https?"https":"http" + "://" + conf.host + ":" + conf.port + "/" ;
            if(conf.urlPrefix != "") url = url + conf.urlPrefix;

            var server = new WebServer ( o => o.WithUrlPrefix(url).WithMode(HttpListenerMode.EmbedIO));
            
            // BASIC AUTH
            BasicAuthenticationModule auth = new BasicAuthenticationModule("/","Access to site");
            auth.WithAccount("",conf.basicAuthPassword);
            if(conf.enableBasicAuth) server.WithModule(auth);

            // API routes
            if(conf.enableREST) 
                server.WithWebApi ("/api/REST", m => m.WithController<nodeRESTController> (()=>{return new nodeRESTController(manager,conf);}));
            if(conf.enableJSON)
                server.WithWebApi ("/api/JSON", m => m.WithController<nodeJSONController> (()=>{return new nodeJSONController(manager,conf);}));
            
            // STATIC Files
            if(conf.enableStaticFiles) server.WithStaticFolder("/",conf.staticFilesPath,false);
            
            // exception handler
            server.HandleHttpException (customHttpErrorCallback);

            // Logging handler
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }


        private static async Task customHttpErrorCallback (IHttpContext ctx, IHttpException ex) {

            ctx.Response.StatusCode = ex.StatusCode;
            if(ex.StatusCode == 401) ctx.Response.Headers.Add("WWW-Authenticate", "Basic realm=Access to site");

            foreach (var item in ctx.Route.Keys){
                Console.WriteLine(item);

            }
            if(ctx.Request.RawUrl.Contains("api")){
                switch (ex.StatusCode) {
                    case 401:
                        await ctx.SendDataAsync (new httpErrorData () { Error = "Unauthorized" });
                        break;
                    case 400:
                        await ctx.SendDataAsync (new httpErrorData () { Error = "Bad Request" });
                        break;
                    case 403:
                        await ctx.SendDataAsync (new httpErrorData () { Error = "Forbidden" });
                        break;
                    case 404:
                        await ctx.SendDataAsync (new httpErrorData () { Error = "Not Found" });
                        break;
                    case 405:
                        await ctx.SendDataAsync (new httpErrorData () { Error = "Not Allowed" });
                        break;
                    case 500:
                        await ctx.SendDataAsync (new httpErrorData () { Error = "Internal Server Error" });
                        break;
                    default:
                        await ctx.SendDataAsync (new httpErrorData () { Error = "Uknown Exception" });
                        break;
                }
            }
            else await ctx.SendStandardHtmlAsync(ex.StatusCode);
        }
    }

    /// <summary>
    /// Class for returning errors data 
    /// </summary>
    public class httpErrorData {
        public string Error { get; set; }
    }
}