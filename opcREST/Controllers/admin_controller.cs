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
        CSRF_utils _csrf;
        SecureSessionManager session_manager;        

        public logonLogoffController(SecureSessionManager ssm, CSRF_utils csrf){
            _csrf = csrf;
            session_manager = ssm;
        }

        [Route(HttpVerbs.Get,"/login")]
        public async Task logon(string message="", string user=""){
            var token = _csrf.setCSRFcookie(HttpContext);
            await HttpContext.SendStringAsync(HTMLtemplates.loginPage(token,"/admin/login", message, user),"text/html",Encoding.UTF8);
        }
        
        
        [Route(HttpVerbs.Post, "/login")]
        public async Task check_logon(){

            if(!_csrf.validateCSRFtoken(HttpContext))  { await AuthUtils.sendForbiddenTemplate(HttpContext); return; }
            var data = await HttpContext.GetRequestFormDataAsync();

            string user = data.Get("user") ;
            string pw = data.Get("pw") ;
            
            // validate password
            var _user = session_manager.userStore.GetUser(user);
            
            if( !_user.password.isValid(pw) ) {        
                // set some session cookie for error display
                //HttpContext.Redirect("/admin/login/");
                await logon("username or password invalid", user);
                return;
            }

            // delete the current session if any
            session_manager.Delete(HttpContext,"");

            var current_session = session_manager.RegisterSession(HttpContext);
            Console.WriteLine("pass0");
            current_session["user"] = _user;
            Console.WriteLine(_user.ToString());
            HttpContext.Redirect("/");
        }
        
        [Route(HttpVerbs.Get, "/logout")]
        public Task logout(){
            
           var session =  session_manager.RemoveSession(HttpContext);
           if(String.IsNullOrEmpty(session.Id)){
                return AuthUtils.sendForbiddenTemplate(HttpContext);
           }

           throw HttpException.Redirect("/admin/login/",303);
        }

        [Route(HttpVerbs.Get,"/write_access")]
        public Task write_access(){
            if( string.IsNullOrEmpty(HttpContext.Session.Id) ) return AuthUtils.sendForbiddenTemplate(HttpContext);
            var token = _csrf.setCSRFcookie(HttpContext);
            var referrer = HttpContext.Request.Headers.Keys ;
            UserData _user = (UserData) HttpContext.Session["user"];
            string user_name = (_user != null) ? _user.userName : "Anonymous";
            return HttpContext.SendStringAsync(HTMLtemplates.writeAccess(token,"/admin/write_access",user_name),"text/html",Encoding.UTF8); 
        }

        [Route(HttpVerbs.Post,"/write_access")]
        public Task check_write_access(){
            // has an authenticated session 
            if( string.IsNullOrEmpty(HttpContext.Session.Id) ) { return  AuthUtils.sendForbiddenTemplate(HttpContext); }
            // has a valid CSRF token
            if(!_csrf.validateCSRFtoken(HttpContext))   return  AuthUtils.sendForbiddenTemplate(HttpContext);

            var _data = HttpContext.GetRequestFormDataAsync();
            _data.Wait();
            var data = _data.Result;

            string pw = data.Get("pw") ;
            string referrer = data.Get("referrer") ;
            Console.WriteLine("write access pw : " + pw);
            if(string.IsNullOrEmpty(pw)) throw HttpException.Redirect("/admin/write_access",303);
            
            UserData _user = (UserData) HttpContext.Session["user"];
            if(!_user.password.isValid(pw) )  throw HttpException.Redirect("/admin/write_access",303);

            if(!_user.AllowWrite(TimeSpan.FromMinutes(30)))  return  AuthUtils.sendForbiddenTemplate(HttpContext); 
            throw HttpException.Redirect("/");
        }

        [Route(HttpVerbs.Any, "/{data}", true)]
         public Task forbid(){
            if( string.IsNullOrEmpty(HttpContext.Session.Id) ){
                return AuthUtils.sendForbiddenTemplate(HttpContext);
            }
            else throw HttpException.NotFound();
         }
        
        [Route(HttpVerbs.Any, "/")]
         public Task forbid2(){
            if( string.IsNullOrEmpty(HttpContext.Session.Id) ){
                return AuthUtils.sendForbiddenTemplate(HttpContext);
            }
            else throw HttpException.NotFound();
         }
    }


        
    
}