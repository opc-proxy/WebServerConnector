using System.Threading.Tasks;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using EmbedIO;
using System;
using OpcProxyCore;
using Newtonsoft.Json.Linq;
using opcRESTconnector.Session;
using System.Collections.Generic;

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



        [Route(HttpVerbs.Post, "/read")]
        public async Task<List<ReadResponse>> GetNodes(){
            
            // Check if Authorized
            var _session = (sessionData) HttpContext.Session?["session"];
            if(_conf.enableCookieAuth && (_session == null || !_session.hasReadRights()))
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

            try{
                var values = await _service.readValueFromCache(data.names.ToArray());
                var response = new List<ReadResponse>();
                values.ForEach(val => response.Add(new ReadResponse(val)) );
                return response;
            }    
            catch{
                throw HttpException.Forbidden();
            }
        }

        
        [Route(HttpVerbs.Post, "/write")]
         public async Task<List<WriteResponse>> PostData() 
        {
            var _session = (sessionData) HttpContext.Session?["session"];
            // Check if Authorized
            if(_conf.enableCookieAuth && (_session == null || !_session.hasWriteRights() ))
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

            if(data.values.Count == 0 || data.names.Count != data.values.Count ) 
                throw HttpException.BadRequest();
            try
            {
                var writeResp = await _service.writeToOPCserver(data.names.ToArray(), data.values.ToArray());
                var response = new List<WriteResponse>();
                writeResp.ForEach( w => response.Add(new WriteResponse(w)) );
                return response;
            }
            catch
            {
                throw HttpException.BadRequest();
            }
        }


    }

    }