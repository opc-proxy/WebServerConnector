using System.Collections.Generic;
using System.Threading.Tasks;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using EmbedIO;
using System;
using Newtonsoft.Json.Linq;
using System.Text;
using opcRESTconnector.Session;


namespace opcRESTconnector{
    public  class logonLogoffController : WebApiController{
        string _url;
        AntiCSRF.AntiCSRF csrf_gen;
        RESTconfigs _conf;
        Dictionary<string, string> users;

        SecureSessionManager session_manager;        

        public logonLogoffController(RESTconfigs conf, string url, SecureSessionManager ssm){
            _conf = conf;
            _url = url;

            var csrf_conf = new AntiCSRF.Config.AntiCSRFConfig(){ expiryInSeconds = 180 };
            csrf_gen = new AntiCSRF.AntiCSRF(csrf_conf);
            users = new Dictionary<string, string>();
            if(conf.enableCookieAuth) loadUsers();

            session_manager = ssm;
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

        [Route(HttpVerb.Get,"/login")]
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
            Console.WriteLine("CSRF token Valid");

            if( !data.ContainsKey("_csrf") ) throw HttpException.Forbidden();
            Console.WriteLine("CSRF token present in data");
            
            if( req_cookie.Value != data.Get("_csrf") ) throw HttpException.Forbidden();
            Console.WriteLine("CSRF token and Cookie are same");

            string user = data.Get("user") ;
            string pw = data.Get("pw") ;
            
            Console.WriteLine("Very good " + user + " " + pw);
            
            // validate password
            if( !validate(user,pw) ) {        
                // set some session cookie for error display
                HttpContext.Redirect("/admin/login/");
                return;
            }

            // delete the current session if any
            session_manager.Delete(HttpContext);

            var current_session = session_manager.RegisterSession(HttpContext);
            Console.WriteLine("pass0");
            current_session["user"] = new UserData(user);
            Console.WriteLine("pass1");
            HttpContext.Redirect("/");
        }
        
        [Route(HttpVerb.Get, "/logout")]
        public Task logout(){
            
           var session =  session_manager.RemoveSession(HttpContext);
           if(String.IsNullOrEmpty(session.Id)){
                HttpContext.Response.StatusCode = 403;
                HttpContext.SetHandled();
                return HttpContext.SendStringAsync(HTMLtemplates.forbidden(),"text/html",Encoding.UTF8);
           }

           throw HttpException.Redirect("/admin/login/",303);
        }


        [Route(HttpVerb.Any, "/{data}", true)]
         public Task forbid(){
            if( string.IsNullOrEmpty(HttpContext.Session.Id) ){
                HttpContext.Response.StatusCode = 403;
                HttpContext.SetHandled();
                return HttpContext.SendStringAsync(HTMLtemplates.forbidden(),"text/html",Encoding.UTF8);
            }
            else throw HttpException.NotFound();
         }
    }
        
    
}