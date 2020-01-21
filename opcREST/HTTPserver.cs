using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using Swan.Logging;
namespace opcRESTconnector {

    public class HTTPServerBuilder {

        /// <summary>
        /// 
        /// </summary>
        //// <param name="url"></param>
        /// <returns></returns>
        public static WebServer CreateWebServer (string url) {
            
            var server = new WebServer (
                o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO)
                    )
            .WithLocalSessionManager()
            //.WithWebApi ("/REST", m => m.WithController<nodeRESTController> ());
            .WithWebApi ("/REST", m => m.WithController<nodeRESTController> (()=>{return new nodeRESTController("bellissimo!");}));

            // exception handler
            server.HandleHttpException (customHttpErrorCallback);

            // Logging handler
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }

        private static async Task customHttpErrorCallback (IHttpContext ctx, IHttpException ex) {

            ctx.Response.StatusCode = ex.StatusCode;

            switch (ex.StatusCode) {
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
                default:
                    await ctx.SendDataAsync (new httpErrorData () { Error = "Uknown Exception" });
                    break;
            }

        }
    }

    /// <summary>
    /// Class for returning errors data 
    /// </summary>
    public class httpErrorData {
        public string Error { get; set; }
    }
}