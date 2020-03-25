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
    public  class AdminController : WebApiController{
        CSRF_utils _csrf;
        SecureSessionManager session_manager;     

        RESTconfigs _conf;   

        public AdminController(SecureSessionManager ssm, CSRF_utils csrf, RESTconfigs conf){
            _csrf = csrf;
            session_manager = ssm;
            _conf = conf;
        }


        [Route(HttpVerbs.Get,BaseRoutes.write_access)]
        public Task write_access(string override_referer="", string error = ""){

            var token = _csrf.setCSRFcookie(HttpContext);
            var referer = HttpContext.Request.Headers["Referer"] ?? "/";
            if(!String.IsNullOrEmpty(override_referer)) referer = override_referer;
            
            UserData _user = (((sessionData) HttpContext.Session["session"])?.user);
            string user_name = (_user != null) ? _user.userName : "Anonymous";
            return HttpContext.SendStringAsync(HTMLtemplates.writeAccess(token,user_name,referer, error),"text/html",Encoding.UTF8); 
        }

        [Route(HttpVerbs.Post, BaseRoutes.write_access)]
        public Task check_write_access(){

            // has a valid CSRF token
            if(!_csrf.validateCSRFtoken(HttpContext))   return  AuthUtils.sendForbiddenTemplate(HttpContext);

            var _data = HttpContext.GetRequestFormDataAsync();
            _data.Wait();
            var data = _data.Result;

            string pw = data.Get("pw") ;
            string referer = data.Get("_referrer") ?? "/";
            if(string.IsNullOrEmpty(pw)) return write_access(referer,"Invalid Password");
            
            var _session = (sessionData) HttpContext.Session["session"];
            if(_session == null) return  AuthUtils.sendForbiddenTemplate(HttpContext,referer); 

            var status = _session.AllowWrite(pw,_conf.writeExpiryMinutes);
            if(status == UsrStatusCodes.WrongPW )  return write_access(referer,"Invalid Password");
            else if( status != UsrStatusCodes.Success ) return  AuthUtils.sendForbiddenTemplate(HttpContext,referer); 
            
            // change session permission in DB
            session_manager.store.sessions.Update(_session);
            
            return Utils.HttpRedirect(HttpContext,referer);
        }


    }
    
}