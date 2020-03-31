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
using System.Net.Http;
using System.Threading;
using opcRESTconnector;
using OpcProxyCore;
using Newtonsoft.Json;
using System.Net.Http.Headers ;
using System.Net;


namespace opcRESTconnector{
    public  class LoginController : WebApiController{
        CSRF_utils _csrf;
        SecureSessionManager session_manager;     

        RESTconfigs _conf;   

        public LoginController(SecureSessionManager ssm, CSRF_utils csrf, RESTconfigs conf){
            _csrf = csrf;
            session_manager = ssm;
            _conf = conf;
        }

        [Route(HttpVerbs.Get, BaseRoutes.login)]
        public async Task logon(string message="", string user=""){
            var token = _csrf.setCSRFcookie(HttpContext);
            var htmlTemplate = HTMLtemplates.loginPage(token, message, user, _conf.recaptchaClientKey);
            await HttpContext.SendStringAsync( htmlTemplate,"text/html",Encoding.UTF8);
        }
        
        
        [Route(HttpVerbs.Post, BaseRoutes.login)]
        public async Task check_logon(){

            if(!_csrf.validateCSRFtoken(HttpContext))  { await AuthUtils.sendForbiddenTemplate(HttpContext); return; }
            var data = await HttpContext.GetRequestFormDataAsync();

            string user = data.Get("user") ?? "_anonymous_";
            string pw = data.Get("pw") ?? "invalid_pwd";
            string reCAPTCHA = data.Get("g-recaptcha-response") ?? "invalid_recaptcha";

            // validate reCAPTCHA if enabled
            if(_conf.isRecaptchaEnabled()){
               var isValid =  await AuthUtils.reCAPTCHA_isValid(reCAPTCHA, _conf.recaptchaServerKey);
               if(!isValid) { await logon("reCAPTCHA invalid", user); return; }
            }
            
            // validate password
            var _user = session_manager.store.users.Get(user);
            Console.WriteLine("Active User -1");
            if(!_user.isActive())  { await AuthUtils.sendForbiddenTemplate(HttpContext); return;}
            Console.WriteLine("Active User");
            if( !_user.password.isValid(pw) ) {        
                await logon("username or password invalid", user);
                return;
            }
            
            // delete the current session if any
            session_manager.Delete(HttpContext,"");

            var current_session = session_manager.RegisterSession(HttpContext,_user);
            HttpContext.Redirect("/",303);
        }

        
        
        [Route(HttpVerbs.Get, BaseRoutes.logout)]
        public Task logout(){
            
           var session =  session_manager.RemoveSession(HttpContext);
           if(String.IsNullOrEmpty(session)){
                return AuthUtils.sendForbiddenTemplate(HttpContext);
           }
           return Utils.HttpRedirect(HttpContext,Routes.login);

        }

        [Route(HttpVerbs.Get, BaseRoutes.update_pw)]
        public Task update_pw(string error){
            if( string.IsNullOrEmpty(HttpContext.Session.Id) ) return AuthUtils.sendForbiddenTemplate(HttpContext);
            var token = _csrf.setCSRFcookie(HttpContext);

            UserData _user = (((sessionData) HttpContext.Session["session"])?.user);
            string user_name = (_user != null) ? _user.userName : "Anonymous";
            if(!_user.isActive()) { return AuthUtils.sendForbiddenTemplate(HttpContext);}

            return HttpContext.SendStringAsync(HTMLtemplates.updatePW(token,user_name,error),"text/html",Encoding.UTF8); 
        }

        [Route(HttpVerbs.Post, BaseRoutes.update_pw)]
        public async Task update_pw_post(){
            // has an authenticated session 
            if( string.IsNullOrEmpty(HttpContext.Session.Id) ) { await AuthUtils.sendForbiddenTemplate(HttpContext); return ;}
            // has a valid CSRF token
            if(!_csrf.validateCSRFtoken(HttpContext)) { await AuthUtils.sendForbiddenTemplate(HttpContext); return;}
            
            var data = await HttpContext.GetRequestFormDataAsync();
            string old_pw = data.Get("old_pw") ;
            string new_pw = data.Get("new_pw") ;
            
            UserData user = (((sessionData) HttpContext.Session["session"])?.user);
            if(user == null) { await AuthUtils.sendForbiddenTemplate(HttpContext); return;}

            if(!user.isActive()) { await AuthUtils.sendForbiddenTemplate(HttpContext); return;}
            var status = user.password.update_password(old_pw,new_pw, _conf.sessionExpiryHours);
            if( status == UsrStatusCodes.WrongPW ) { await update_pw("Invalid Password"); return;}
            if( status == UsrStatusCodes.PasswordExist ) { await update_pw("Password Exist"); return;}
            if( status != UsrStatusCodes.Success ) { await AuthUtils.sendForbiddenTemplate(HttpContext); return; }
            
            session_manager.store.users.Update(user);

            await Utils.HttpRedirect(HttpContext, "/");
            return ;
        }


        [Route(HttpVerbs.Any, "/{data}", true)]
         public Task forbid(){
            return forbid2();
         }
        
        [Route(HttpVerbs.Any, "/")]
         public Task forbid2(){
            if( string.IsNullOrEmpty(HttpContext.Session.Id) ) return AuthUtils.sendForbiddenTemplate(HttpContext);
            // if Id is not empty then "session" is also filled (no need to tryGet)
            var session = (sessionData) HttpContext.Session["session"];
            var user = session?.user;
            if(user == null) return AuthUtils.sendForbiddenTemplate(HttpContext);
            if( !user.isActive() ) return AuthUtils.sendForbiddenTemplate(HttpContext);
            if( !user.password.isActive() ) return Utils.HttpRedirect(HttpContext, Routes.update_pw);
            throw HttpException.NotFound();
         }
    }


}
