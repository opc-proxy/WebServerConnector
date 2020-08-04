using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using EmbedIO.Utilities;
using Newtonsoft.Json.Linq;
using NLog;
using System.Net.Http;

namespace opcRESTconnector {

    public enum AuthRoles{
        Reader,
        Writer,
        Admin,
        Undefined
    }

    
    public class EnsureActiveUser : WebModuleBase
    {
        public static NLog.Logger logger = null;
        public EnsureActiveUser() : base("/") {
            logger = LogManager.GetLogger(this.GetType().Name);
        }
        public override bool IsFinalHandler => false;

        protected override Task OnRequestAsync(IHttpContext context)
        {
            return AuthUtils.EnsureActiveUser(context) ;
        }
    }

    public class EnsureApiCsrf : WebModuleBase
    {
        public static NLog.Logger logger = null;
        RESTconfigs _conf;
        public EnsureApiCsrf(string root, RESTconfigs config) : base(root)
        {
            logger = LogManager.GetLogger(this.GetType().Name);
            _conf = config;
        }
        public override bool IsFinalHandler => false;

        protected override Task OnRequestAsync(IHttpContext context)
        {
            if(context.Request.HttpVerb != HttpVerbs.Post) return Task.CompletedTask;
            
            string csrf_token = context.Request.Headers["X-API-Token"] ?? "none";
            if(_conf.enableCookieAuth)
            {
                var session = (sessionData) context.Session?["session"];
                if(session == null) throw HttpException.Forbidden();
                if( session.csrf_token == csrf_token) return Task.CompletedTask;
                else {
                    logger.Warn("Api Token auth failed. ");
                    logger.Debug("token in session: " + session.csrf_token);
                    logger.Debug("token in X-API-Token Header: " + csrf_token);
                    throw HttpException.Forbidden();
                }
            }
            else 
            {
                if(_conf.GetEnvVars().apiKey == "" )        return Task.CompletedTask;;
                if(_conf.GetEnvVars().apiKey == csrf_token) return Task.CompletedTask;
            }
            logger.Warn("Api Token auth failed. ");
            logger.Debug("token in Env Var: " +_conf.GetEnvVars().apiKey );
            logger.Debug("token in X-API-Token Header: " + csrf_token);
            throw HttpException.Forbidden();
        }
    }

    public class AuthUtils {
        
        public static Task EnsureActiveUser(IHttpContext context){
            if( string.IsNullOrEmpty(context.Session.Id) ) return AuthUtils.sendForbiddenTemplate(context);
            // if Id is not empty then "session" is also filled (no need to tryGet)
            var session = (sessionData) context.Session["session"];
            // case session expired but not cleared yet (default is 30 sec)
            if(session?.expiryUTC.Ticks < DateTime.UtcNow.Ticks) return AuthUtils.sendForbiddenTemplate(context);
            var user = session?.user;
            if(user == null) return AuthUtils.sendForbiddenTemplate(context);
            if( !user.isActive() ) return AuthUtils.sendForbiddenTemplate(context);
            if( !user.password.isActive() ) return Utils.HttpRedirect(context, Routes.update_pw);
            return Task.CompletedTask;
        }
        public static Task sendForbiddenTemplate(IHttpContext context, string redirectURL = Routes.login){
            context.Response.StatusCode = 403;
            context.SetHandled();
            return context.SendStringAsync(HTMLtemplates.forbidden(redirectURL),"text/html",Encoding.UTF8);
        }

        public static bool isAdmin(IHttpContext ctx, ref ErrorData err){
            if( ((sessionData)ctx.Session["session"]).user?.role != AuthRoles.Admin)
            {
                ctx.Response.StatusCode = 403; // Forbidden;
                err.Success = false;
                err.ErrorCode = ErrorCodes.NotAdmin;
                return false;
            }
            return true;
        }

        public static bool isAdmin(IHttpContext ctx){
            if( ((sessionData)ctx.Session["session"]).user?.role != AuthRoles.Admin)
            {
                ctx.Response.StatusCode = 403; // Forbidden;
                return false;
            }
            return true;
        }

        public static bool isValidUserData(IHttpContext ctx, UserForm data, ref ErrorData err){

            if(data == null ) 
            { 
                ctx.Response.StatusCode = 400;
                err.ErrorCode = ErrorCodes.BadData; return false; 
            }
            if(String.IsNullOrEmpty(data.userName)) err.ErrorCode = ErrorCodes.BadUsrName;
            else if(!Utils.isEmail(data.email)) err.ErrorCode = ErrorCodes.BadEmail;
            else if(data.getRole() == AuthRoles.Undefined) err.ErrorCode = ErrorCodes.BadRole;

            if(err.ErrorCode != "")  {
                ctx.Response.StatusCode = 400;
                return false;
            }
            else return true;
            
        }

        public static ErrorData errorResponse(IHttpContext ctx, string error, int statuscode){
            ctx.Response.StatusCode = statuscode;
            var err = new ErrorData();
            err.ErrorCode = error;
            err.Success = false;
            return err;
        }

        public static async Task<bool> reCAPTCHA_isValid(string recaptcha, string serverKey){
            var query = new Dictionary<string, string>
            {
                { "secret", serverKey },
                { "response", recaptcha}
            };
            var http = new HttpClient();
            var Req = new FormUrlEncodedContent(query);
            var response = await http.PostAsync("https://www.google.com/recaptcha/api/siteverify", Req);
            var body = await response.Content.ReadAsStringAsync() ;
            try{
                var result = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<reCATPCHAresp>();
                return result.success;
            }
            catch{
                return false;
            }
        }
    }

    /// <summary>
    /// Class to handle the generation and validation of CSRF tokens.
    /// The secret key used to sign tokens is randomly generated at startup.
    /// </summary>
    public class CSRF_utils {
        AntiCSRF.AntiCSRF csrf_gen;
        string salt;
        string secret;
        public static NLog.Logger logger = null;
        RNGCryptoServiceProvider rnd;
        public CSRF_utils(){
            
            logger = LogManager.GetLogger(this.GetType().Name);
            var csrf_conf = new AntiCSRF.Config.AntiCSRFConfig(){ expiryInSeconds = 180 };
            csrf_gen = new AntiCSRF.AntiCSRF(csrf_conf);
            salt = "XYfday567D";
            byte[] token = new byte[32];
            rnd = new RNGCryptoServiceProvider();
            rnd.GetBytes(token);
            secret = Convert.ToBase64String(token);
        }

        public string generateRandomToken()
        {
            byte[] token = new byte[64];
            rnd.GetBytes(token);
            return  Convert.ToBase64String(token);
        }

        /// <summary>
        /// Generate a signed token from a random number (not encrypted), note that all users have same "salt".
        /// </summary>
        /// <param name="context"></param>
        public string setCSRFcookie(IHttpContext context){
            var token = csrf_gen.GenerateToken(salt,secret);
            // logger.Debug("Set CSRF: " + token);
            var cookie = new System.Net.Cookie("_csrf",token + "; SameSite=Strict");
            cookie.HttpOnly = true;
            cookie.Expires = DateTime.Now.AddMinutes(3) ;
            context.Response.SetCookie(cookie);
            return token;
        }

        public bool validateCSRFtoken(IHttpContext context){
            
            CSRFdata data = new CSRFdata();

            if(context.Request.ContentType.ToLower().Contains("application/json")){
                var _data = context.GetRequestDataAsync<CSRFdata>();
                _data.Wait();
                data = _data.Result;
            }
            else if( context.Request.ContentType.ToLower().Contains("application/x-www-form-urlencoded")){
                var _data = context.GetRequestFormDataAsync();
                _data.Wait();
                var __data = _data.Result;
                if(!__data.ContainsKey("_csrf")) return false;
                data._csrf = __data["_csrf"];
            }
            else return false;
                       
           return _validateCSRFtoken(context,data);
        }

        public bool _validateCSRFtoken(IHttpContext context, CSRFdata data){
            System.Net.Cookie req_cookie = null;
            foreach (var c in context.Request.Cookies)
            {
                if(c.Name == "_csrf") {
                    req_cookie = c;
                    logger.Debug("found CSRF "+ c.Value);
                }
            } 
            System.Net.Cookie resp_cookie = new System.Net.Cookie();
            if(req_cookie == null) return false;
            if(!csrf_gen.ValidateToken(req_cookie.Value,secret,salt)) return false;
            logger.Debug("CSRF token Valid");

            //if( !data.ContainsKey("_csrf") ) return false;
            if( String.IsNullOrEmpty(data._csrf) ) return false;
            logger.Debug("CSRF token present in data");
            
            //if( req_cookie.Value != data.Get("_csrf") ) return false;
            if( req_cookie.Value != data._csrf ) return false;
            logger.Debug("CSRF token and Cookie are same");

            return true;
        }

    }

    public  class CSRFdata {
        public string _csrf {get;set;}
        public CSRFdata(){
            _csrf = "";
        }
    }
}
