using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using EmbedIO;
using System;
using OpcProxyCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace opcRESTconnector
{
    public class Utils{
        public static ReadResponse packReadNodes(dbVariableValue[] values, ReadStatusCode status){

            ReadResponse r = new ReadResponse();

            foreach(var variable in values){
                NodeValue val = new NodeValue();
                val.Name = variable.name;
                val.Type = variable.systemType.Substring(7).ToLower();
                val.Value = variable.value;
                val.Timestamp = variable.timestamp.ToUniversalTime().ToString("o");
                r.Nodes.Add(val);
            }
            r.ErrorMessage = ( status == ReadStatusCode.Ok) ? "none" : "Not Found";
            r.IsError = ( status != ReadStatusCode.Ok);

            return r;
        }
    }

    public sealed class logonLogoffController : WebApiController{
        string _url;
        AntiCSRF.AntiCSRF csrf_gen;
        RESTconfigs _conf;
        Dictionary<string, string> users;

        public logonLogoffController(RESTconfigs conf, string url){
            _conf = conf;
            _url = url;

            var csrf_conf = new AntiCSRF.Config.AntiCSRFConfig(){ expiryInSeconds = 180 };
            csrf_gen = new AntiCSRF.AntiCSRF(csrf_conf);
            users = new Dictionary<string, string>();
            if(conf.enableCookieAuth) loadUsers();

        }

        public bool validate(string user, string password){
            if(!users.ContainsKey(user)) return false;
            string actualPW = "";
            users.TryGetValue(user, out actualPW);
            if(password != actualPW) return false;
            return true;
        }

        public void loadUsers(){
            foreach(var user_item in _conf.userAuth)
            {
                if(user_item.Count == 3){
                    users.Add(user_item[0],user_item[1]);
                }
                else{
                    throw new Exception("Malformed user item: " + user_item[0]);
                }
            }
        }

        [Route(HttpVerb.Get, "/login")]
        public async Task logon(){
            var token = csrf_gen.GenerateToken("salt","whatever");
            Console.WriteLine(token);
            var cookie = new System.Net.Cookie("_csrf",token + "; SameSite=Strict");
            cookie.HttpOnly = true;
            cookie.Expires = DateTime.Now.AddMinutes(3) ;
            HttpContext.Response.SetCookie(cookie);
            await HttpContext.SendStringAsync(HTMLtemplates.loginPage(token,_url +"admin/login"),"text/html",Encoding.UTF8);
        }
        
        
        [Route(HttpVerb.Post, "/login")]
        public async Task check_logon(){

            var data = await HttpContext.GetRequestFormDataAsync();
            System.Net.Cookie req_cookie = null;
            foreach (var c in HttpContext.Request.Cookies)
            {
                if(c.Name == "_csrf") {
                    req_cookie = c;
                    Console.WriteLine("found cookie "+ c.Value);
                }
            } 
            System.Net.Cookie resp_cookie = new System.Net.Cookie();

            if(req_cookie == null) {
                // probaly the cookie has expired
                // set some session cookie for error display
                HttpContext.Redirect("/admin/login/");
                return;
            }
            
            if(!csrf_gen.ValidateToken(req_cookie.Value,"whatever","salt")) throw HttpException.Forbidden();

            if( !data.ContainsKey("_csrf") ) throw HttpException.Forbidden();
            
            if( req_cookie.Value != data.Get("_csrf") ) throw HttpException.Forbidden();

            string user = data.Get("user") ;
            string pw = data.Get("pw") ;
            
            Console.WriteLine("Very good " + user + " " + pw);
            
            // validate password
            if( !validate(user,pw) ) {        
                // set some session cookie for error display
                HttpContext.Redirect("/admin/login/");
                return;
            }

            resp_cookie.Name = "JWTtoken";
            resp_cookie.Value = "token" + "; SameSite=Strict";
            resp_cookie.HttpOnly = true;
            resp_cookie.Path = "/";
            HttpContext.Response.SetCookie(resp_cookie);
            Console.WriteLine("pass");
            HttpContext.Redirect("/");
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



        [Route(HttpVerb.Get, "/{node_name}")]
        public  ReadResponse GetNode(string node_name){
           
            // Check if Authorized
            var role = (AuthRoles)(HttpContext.Items["Role"] ?? AuthRoles.Undefined);
            if(_conf.enableBasicAuth && role == AuthRoles.Undefined) 
                throw HttpException.Forbidden();
            
           try{
                List<string> names = new List<string>{ node_name };
                ReadStatusCode status;
                var values =  _service.readValueFromCache(names.ToArray(),out status);
                if(status != ReadStatusCode.Ok) HttpContext.Response.StatusCode = 404;
                return Utils.packReadNodes(values,status);
           }
            catch(Exception ex){
                Console.WriteLine(ex.Message);
                throw HttpException.BadRequest();
            }
        }

        
        [Route(HttpVerb.Post, "/{node_name}")]
         public async Task<WriteResponse> PostData(string node_name) 
        {
              // Check if Authorized
            var role = (AuthRoles)(HttpContext.Items["Role"] ?? AuthRoles.Undefined);
            if(_conf.enableBasicAuth && role != AuthRoles.Writer && role != AuthRoles.Admin ) 
                throw HttpException.Forbidden();
            
            var data = await HttpContext.GetRequestFormDataAsync();
            // validity check
            if(_conf.enableAPIkey && ( !data.ContainsKey("apiKey") || data.Get("apiKey") != _conf.apyKey ))  
                throw HttpException.Forbidden();

            if(!data.ContainsKey("value")) 
                throw HttpException.BadRequest();
                    
            var value = data.Get("value");
            var status = await _service.writeToOPCserver(node_name, value);
            
            WriteResponse r = new WriteResponse();
            r.IsError = (Opc.Ua.StatusCode.IsBad(status[0]));
            r.ErrorMessage = (r.IsError) ? "Not Found" : "none";

            if(r.IsError) HttpContext.Response.StatusCode = 404;

            return r;
        }


    }

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