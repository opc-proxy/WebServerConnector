using System.Threading.Tasks;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using EmbedIO;
using System;
using OpcProxyCore;
using System.Net;
using System.Net.Mail;


namespace opcRESTconnector
{
    public class Utils{
       
        /// <summary>
        /// Wrapper for HttpContext.Redirect() Status code 303
        /// </summary>
        /// <returns></returns>
        public static Task HttpRedirect(IHttpContext ctx, string url){
            ctx.Redirect(url,303);
            ctx.SetHandled();
            return Task.CompletedTask;
        }

        public static void HTMLescape(ref string input){
            input = WebUtility.HtmlEncode(input);
        }
        public static string HTMLescape(string input){
            return WebUtility.HtmlEncode(input);
        }

        public static bool isEmail(string email){
            if( String.IsNullOrEmpty(email) ) return false;

            try{
                MailAddress m = new MailAddress(email);
                return true;
            }
            catch{
                return false;
            }
        }   
    }

    public sealed class nodeRESTController : WebApiController
    {
        serviceManager _service;
        RESTconfigs _conf;
        public nodeRESTController(serviceManager manager, RESTconfigs conf){
            _service = manager;
            _conf = conf;
        }



        [Route(HttpVerbs.Get, "/{node_name}")]
        public  async Task<ReadResponse> GetNode(string node_name){
            
            var _session = (sessionData) HttpContext.Session?["session"];
            if(_conf.enableCookieAuth && (_session == null || !_session.hasReadRights()))
                throw HttpException.Forbidden();

           try
           {
                string[] names = new string[1]{ node_name };
                var values = await _service.readValueFromCache(names);
                var response = new ReadResponse(values[0]);
                return response;
           }
            catch
            {
                throw HttpException.BadRequest();
            }
        }

        
        [Route(HttpVerbs.Post, "/{node_name}")]
         public async Task<WriteResponse> PostData(string node_name) 
        {
            var _session = (sessionData) HttpContext.Session?["session"];
            // Check if Authorized
            if(_conf.enableCookieAuth && (_session == null || !_session.hasWriteRights() ))
                throw HttpException.Forbidden();
            
            var data = await HttpContext.GetRequestFormDataAsync();

            if(!data.ContainsKey("value")) 
                throw HttpException.BadRequest();
                    
            var value = data.Get("value");

            try
            {
                var writeResp = await _service.writeToOPCserver(new string[]{node_name}, new object[]{value});
                var response = new WriteResponse(writeResp[0]);
                return response;
            }
            catch
            {
                throw HttpException.BadRequest();
            }
        }


    }

   
}