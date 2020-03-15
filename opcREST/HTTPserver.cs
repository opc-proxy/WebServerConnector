using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Files;
using EmbedIO.WebApi;
using System;
using System.IO;
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
            var logger = NLog.LogManager.GetLogger("WebServer");

            string url = buildHostURL(conf);
            var server = new WebServer ( o => o.WithUrlPrefix(url).WithMode(HttpListenerMode.EmbedIO));

            // string passwordHash = BCrypt.Net.BCrypt.HashPassword("mypasss");
            // Console.WriteLine("Bcrypt " + passwordHash);

            // AUTHENTICATION
            if(conf.enableCookieAuth) {
                // COOKIE BASED
                SecureSessionManager cookieAuth = new SecureSessionManager(conf);
                var csrf = new CSRF_utils();
                server.WithSessionManager(cookieAuth);
                server.WithWebApi(BaseRoutes.admin, m => m.WithController<logonLogoffController>(()=>{return new logonLogoffController(cookieAuth,csrf,conf);}));
                server.WithModule(new EnforceAuth());
            }
            else {
                DummySessionManager baseSession = new DummySessionManager(conf);
                server.WithSessionManager(baseSession);
            }
            
            // API routes
            if(conf.enableREST) 
                server.WithWebApi (Routes.rest, m => m.WithController<nodeRESTController> (()=>{return new nodeRESTController(manager,conf);}));
            if(conf.enableJSON)
                server.WithWebApi (Routes.json, m => m.WithController<nodeJSONController> (()=>{return new nodeJSONController(manager,conf);}));
            
            // STATIC Files
            if(conf.enableStaticFiles) server.WithStaticFolder("/",conf.staticFilesPath,true);
            
            // exception handler
            server.HandleHttpException (customHttpErrorCallback);

            // Logging handler
            if(!conf.serverLog) { 
                server.StateChanged += (s, e) => {
                    if(e.NewState == WebServerState.Listening) logger.Info("Listening at " + url );
                    else logger.Info("Is " + e.NewState.ToString() );
                };
            }
            return server;
        }

        private static string buildHostURL(RESTconfigs conf){
            var url =  conf.https?"https":"http" + "://" + conf.host + ":" + conf.port + "/" ;
            if(conf.urlPrefix != "") url = url + conf.urlPrefix;
            return url;
        }
        private static async Task customHttpErrorCallback (IHttpContext ctx, IHttpException ex) {

            ctx.Response.StatusCode = ex.StatusCode;

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