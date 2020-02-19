using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using Swan.Logging;
using EmbedIO.Authentication;
using System;
using OpcProxyCore;
using opcRESTconnector.Session;

namespace opcRESTconnector {

    public class HTTPServerBuilder {


        /// <summary>
        /// 
        /// </summary>
        //// <param name="url"></param>
        /// <returns></returns>
        public static WebServer CreateWebServer (RESTconfigs conf, serviceManager manager) {
            
            //SecureSessionManager soap = new SecureSessionManager();

            string url = conf.https?"https":"http" + "://" + conf.host + ":" + conf.port + "/" ;
            if(conf.urlPrefix != "") url = url + conf.urlPrefix;

            var server = new WebServer ( o => o.WithUrlPrefix(url).WithMode(HttpListenerMode.EmbedIO));

            server.WithWebApi("/admin", m => m.WithController<logonLogoffController>(()=>{return new logonLogoffController(conf, url);}));

            // BASIC AUTHENTICATION
            CustomBaseAthentication authentication = new CustomBaseAthentication(conf);
            if(conf.enableBasicAuth) server.WithModule(authentication);
            
            // AUTHORIZZATION
            AuthorizationModule authorizzation = new AuthorizationModule(conf);
            server.WithModule(authorizzation);

            /*if(conf.enableBasicAuth) server.WithAction("/logout",HttpVerb.Any, async (ctx)=>{ 
                if(passed) {
                    passed = false;
                    ctx.Redirect("/");
                }
                else {
                    passed = true;
                    ctx.Response.StatusCode = 401;
                    ctx.Response.Headers.Add("WWW-Authenticate", "Basic realm=Access to site");
                    await ctx.SendStringAsync("<script>setTimeout(()=>{window.location = '/'},3000)</script>","text/html",Encoding.UTF8);
                }

            });*/

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