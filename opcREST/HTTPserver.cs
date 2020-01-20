using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.WebApi;
using EmbedIO.Actions;
using Swan.Logging;
using System.Text;
namespace opcRESTconnector
{

    public class HTTPServer
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
       /* static void init(string[] args)
        {
            var url = "http://localhost:9696/";
            if (args.Length > 0)
                url = args[0];

            // Our web server is disposable.
            using (var server = CreateWebServer(url))
            {
                // Once we've registered our modules and configured them, we call the RunAsync() method.
                server.RunAsync();

                var browser = new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }
                };
                browser.Start();
                // Wait for any key to be pressed before disposing of our web server.
                // In a service, we'd manage the lifecycle of our web server using
                // something like a BackgroundWorker or a ManualResetEvent.
                Console.ReadKey(true);
            }
        }
	*/
	// Create and configure our web server.
        public static WebServer CreateWebServer(string url)
        {
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
		        // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithWebApi("/REST", m => m
                    .WithController<PeopleController>())
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => { ctx.Response.StatusCode = 404;  return ctx.SendDataAsync(new { Error = "Error" }); }));

            // exception handler
            server.HandleHttpException(customHttpErrorCallback);
            
            // Logging handler
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }

        private static async Task customHttpErrorCallback(IHttpContext ctx, IHttpException ex){
            
                ctx.Response.StatusCode = ex.StatusCode;

                switch (ex.StatusCode)
                {
                    case 400:
                        await  ctx.SendDataAsync(new httpErrorData(){Error = "Bad Request"});
                        break;
                    case 403:
                        await  ctx.SendDataAsync(new httpErrorData(){Error = "Forbidden"});
                        break;
                    case 404:
                        await  ctx.SendDataAsync(new httpErrorData(){Error = "Not Found"});
                        break;
                    case 405:
                        await  ctx.SendDataAsync(new httpErrorData(){Error = "Not Allowed"});
                        break;
                    default:
                        await  ctx.SendDataAsync(new httpErrorData(){Error = "Uknown Exception"});
                        break;
                }

        }
    }

    /// <summary>
    /// Class for returning errors data 
    /// </summary>
    public class httpErrorData {
        public string Error {get; set;}
    }
}