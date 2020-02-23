using System.Threading.Tasks;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using EmbedIO;
using System;
using OpcProxyCore;
using Newtonsoft.Json.Linq;



namespace opcRESTconnector
{
    
 // FIXME refactor these two controllers to use common functions, they do mostly same stuff
    // will be more readable
    public sealed class nodeJSONController : WebApiController
    {
        serviceManager _service;
        RESTconfigs _conf;
        public nodeJSONController(serviceManager manager, RESTconfigs conf){
            _service = manager;
            _conf = conf;
        }



        [Route(HttpVerb.Post, "/read")]
        public async Task<ReadResponse> GetNodes(){
            
            // Check if Authorized
            var role = (AuthRoles)(HttpContext.Items["Role"] ?? AuthRoles.Undefined);
            if(_conf.enableBasicAuth && role == AuthRoles.Undefined) 
                throw HttpException.Forbidden();
            
            if( !HttpContext.Request.ContentType.Contains("application/json")) {
                throw HttpException.BadRequest();
            }

           
            string body = await HttpContext.GetRequestBodyAsStringAsync();
            ReadRequest data;
            try{
                data = JObject.Parse(body).ToObject<ReadRequest>();
            }
            catch(Exception){
                throw HttpException.BadRequest();
            }

            // this is a bit too stringent with JSON parsing and fails also if one represent string with '' instead of ""
            //var data = await HttpContext.GetRequestDataAsync<ReadRequest>();

            // validity check
            if (_conf.enableAPIkey && data.apiKey != _conf.apyKey)
                throw HttpException.Forbidden();

            ReadStatusCode status;
            var values = _service.readValueFromCache(data.names.ToArray(), out status);
            if (status != ReadStatusCode.Ok) HttpContext.Response.StatusCode = 404;
            return Utils.packReadNodes(values, status);

        }

        
        [Route(HttpVerb.Post, "/write")]
         public async Task<WriteResponse> PostData() 
        {

            // Check if Authorized
            var role = (AuthRoles)(HttpContext.Items["Role"] ?? AuthRoles.Undefined);
            if(_conf.enableBasicAuth && role != AuthRoles.Writer && role != AuthRoles.Admin ) 
                throw HttpException.Forbidden();
            
            if( !HttpContext.Request.ContentType.Contains("application/json")) 
                throw HttpException.BadRequest();

            WriteRequest data;
            string body = await HttpContext.GetRequestBodyAsStringAsync();
            try{
                data = JObject.Parse(body).ToObject<WriteRequest>();
            }
            catch(Exception){
                throw HttpException.BadRequest();
            }

            // validity check
            if(_conf.enableAPIkey &&  data.apiKey != _conf.apyKey )  
                throw HttpException.Forbidden();

            if(data.value == null || data.name == "") 
                throw HttpException.BadRequest();
                    
            var status = await _service.writeToOPCserver(data.name, data.value);
            
            WriteResponse r = new WriteResponse();
            r.IsError = (Opc.Ua.StatusCode.IsBad(status[0]));
            r.ErrorMessage = (r.IsError) ? "Not Found" : "none";

            if(r.IsError) HttpContext.Response.StatusCode = 404;

            return r;
        }


    }

    }