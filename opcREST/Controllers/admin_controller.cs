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

        RESTconfigs _conf;   

        public logonLogoffController(SecureSessionManager ssm, CSRF_utils csrf, RESTconfigs conf){
            _csrf = csrf;
            session_manager = ssm;
            _conf = conf;
        }

        [Route(HttpVerbs.Get, BaseRoutes.login)]
        public async Task logon(string message="", string user=""){
            var token = _csrf.setCSRFcookie(HttpContext);
            await HttpContext.SendStringAsync(HTMLtemplates.loginPage(token, message, user),"text/html",Encoding.UTF8);
        }
        
        
        [Route(HttpVerbs.Post, BaseRoutes.login)]
        public async Task check_logon(){

            if(!_csrf.validateCSRFtoken(HttpContext))  { await AuthUtils.sendForbiddenTemplate(HttpContext); return; }
            var data = await HttpContext.GetRequestFormDataAsync();

            string user = data.Get("user") ?? "_anonymous_";
            string pw = data.Get("pw") ?? "invalid_pwd";
            
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
            current_session["user"] = _user;
            HttpContext.Redirect("/",303);
        }
        
        [Route(HttpVerbs.Get, BaseRoutes.logout)]
        public Task logout(){
            
           var session =  session_manager.RemoveSession(HttpContext);
           if(String.IsNullOrEmpty(session.Id)){
                return AuthUtils.sendForbiddenTemplate(HttpContext);
           }
           return Utils.HttpRedirect(HttpContext,Routes.login);

        }

        [Route(HttpVerbs.Get,BaseRoutes.write_access)]
        public Task write_access(string override_referer="", string error = ""){
            if( string.IsNullOrEmpty(HttpContext.Session.Id) ) return AuthUtils.sendForbiddenTemplate(HttpContext);
            var token = _csrf.setCSRFcookie(HttpContext);
            var referer = HttpContext.Request.Headers["Referer"] ?? "/";
            if(!String.IsNullOrEmpty(override_referer)) referer = override_referer;
            
            UserData _user = (UserData) HttpContext.Session["user"];
            string user_name = (_user != null) ? _user.userName : "Anonymous";
            return HttpContext.SendStringAsync(HTMLtemplates.writeAccess(token,user_name,referer, error),"text/html",Encoding.UTF8); 
        }

        [Route(HttpVerbs.Post, BaseRoutes.write_access)]
        public Task check_write_access(){
            // has an authenticated session 
            if( string.IsNullOrEmpty(HttpContext.Session.Id) ) { return  AuthUtils.sendForbiddenTemplate(HttpContext); }
            // has a valid CSRF token
            if(!_csrf.validateCSRFtoken(HttpContext))   return  AuthUtils.sendForbiddenTemplate(HttpContext);

            var _data = HttpContext.GetRequestFormDataAsync();
            _data.Wait();
            var data = _data.Result;

            string pw = data.Get("pw") ;
            string referer = data.Get("_referrer") ?? "/";
            if(string.IsNullOrEmpty(pw)) return write_access(referer,"Invalid Password");
            
            UserData _user = (UserData) HttpContext.Session["user"];
            if(!_user.password.isValid(pw) )  return write_access(referer,"Invalid Password");

            if(!_user.AllowWrite(TimeSpan.FromMinutes(_conf.writeExpiryMinutes)))  return  AuthUtils.sendForbiddenTemplate(HttpContext,referer); 
            
            return Utils.HttpRedirect(HttpContext,referer);
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