using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Files;
using EmbedIO.WebApi;
using System;
using System.Text;
using System.IO;
using OpcProxyCore;
using opcRESTconnector.Session;
using opcRESTconnector.Data;

namespace opcRESTconnector {

    public class HTTPServerBuilder {


        /// <summary>
        /// 
        /// </summary>
        //// <param name="url"></param>
        /// <returns></returns>
        public static WebServer CreateWebServer (RESTconfigs conf, serviceManager manager, DataStore app_store) {
            var logger = NLog.LogManager.GetLogger("WebServer");

            string url = buildHostURL(conf);
            var server = new WebServer ( o => o.WithUrlPrefix(url).WithMode(HttpListenerMode.EmbedIO));
            
            if(conf.recoveryMode) logger.Warn("Running in recovery mode");
    
            // AUTHENTICATION
            if(conf.enableCookieAuth) {
                // COOKIE BASED
                SecureSessionManager cookieAuth = new SecureSessionManager(conf, app_store);
                var csrf = new CSRF_utils();
                server.WithSessionManager(cookieAuth);
                server.WithWebApi(BaseRoutes.auth, m => m.WithController<LoginController>(()=>{return new LoginController(cookieAuth,csrf,conf);}));
                server.WithModule(new EnsureActiveUser());
                server.WithWebApi(BaseRoutes.admin, m => m.WithController<AdminController>(()=>{return new AdminController(app_store,csrf,conf);}));
            }
            else {
                DummySessionManager baseSession = new DummySessionManager(conf);
                server.WithSessionManager(baseSession);
            }
            
            // API routes
            server.WithModule(new EnsureApiCsrf(BaseRoutes.api, conf)); // token protection for api
            if(conf.enableREST) 
                server.WithWebApi (Routes.rest, m => m.WithController<nodeRESTController> (()=>{return new nodeRESTController(manager,conf);}));
            if(conf.enableJSON)
                server.WithWebApi (Routes.json, m => m.WithController<nodeJSONController> (()=>{return new nodeJSONController(manager,conf);}));
            
            server.WithWebApi (BaseRoutes.internal_css, m => m.WithController<internalCssController>());
            server.WithWebApi (BaseRoutes.internal_js, m => m.WithController<internalJSController>());
            
            // STATIC Files
            if(conf.enableStaticFiles) server.WithStaticFolder("/",conf.staticFilesPath,false);
            
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

        public static string buildHostURL(RESTconfigs conf){
            var url =  conf.https?"https":"http" + "://" + conf.host + ":" + conf.port + "/" ;
            if(conf.urlPrefix != "") url = url + conf.urlPrefix;
            return url;
        }
        private static async Task customHttpErrorCallback (IHttpContext ctx, IHttpException ex) {
            ctx.Response.StatusCode = ex.StatusCode;

            if(ctx.Request.RawUrl.Contains("api")){
                switch (ex.StatusCode) {
                    case 401:
                        await ctx.SendDataAsync (new ErrorData() { ErrorCode = ErrorCodes.Unauthorized });
                        break;
                    case 400:
                        await ctx.SendDataAsync (new ErrorData() { ErrorCode = ErrorCodes.BadRequest });
                        break;
                    case 403:
                        await ctx.SendDataAsync (new ErrorData() { ErrorCode = ErrorCodes.Forbidden });
                        break;
                    case 404:
                        await ctx.SendDataAsync (new ErrorData() { ErrorCode = ErrorCodes.NotFound });
                        break;
                    case 405:
                        await ctx.SendDataAsync (new ErrorData() { ErrorCode = ErrorCodes.NotAllowed });
                        break;
                    case 500:
                        await ctx.SendDataAsync (new ErrorData() { ErrorCode = ErrorCodes.ServerError });
                        break;
                    default:
                        await ctx.SendDataAsync (new ErrorData() { ErrorCode = ErrorCodes.Uknown });
                        break;
                }
            }
            else await ctx.SendStandardHtmlAsync(ex.StatusCode);
        }
        
    }

}