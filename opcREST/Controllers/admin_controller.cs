using System.Collections.Generic;
using System.Threading.Tasks;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using EmbedIO;
using System;
using Newtonsoft.Json.Linq;
using System.Text;
using opcRESTconnector.Data;
using System.Net.Http;
using System.Threading;
using opcRESTconnector;
using OpcProxyCore;
using Newtonsoft.Json;
using System.Net.Http.Headers ;
using System.Net;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace opcRESTconnector{
    public  class AdminController : WebApiController{
        CSRF_utils _csrf;
        DataStore store;     

        RESTconfigs _conf;   

        public AdminController(DataStore data_store, CSRF_utils csrf, RESTconfigs conf){
            _csrf = csrf;
            store = data_store;
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
            store.sessions.Update(_session);
            
            return Utils.HttpRedirect(HttpContext,referer);
        }

        [Route(HttpVerbs.Post, BaseRoutes.create_user)]
        public async Task createUser(){
            // has a valid CSRF token
            // if(!_csrf.validateCSRFtoken(HttpContext))   return  AuthUtils.sendForbiddenTemplate(HttpContext);

            if( ((sessionData)HttpContext.Session["session"]).user?.role != AuthRoles.Admin)  
                { await AuthUtils.sendForbiddenTemplate(HttpContext); return; }

            var _data = HttpContext.GetRequestDataAsync<UserForm>();
            _data.Wait();
            var data = _data.Result;
            Console.WriteLine(data.ToString());
            // check if data are correct
            if(String.IsNullOrEmpty(data.userName) || !Utils.isEmail(data.email) || data.getRole() == AuthRoles.Undefined) 
                {await HttpContext.SendDataAsync(new httpErrorData {ErrorMessage = "Bad Data"}); return;}

            string pw = UserData.GeneratePW();
            var db_user = new UserData(data.userName, pw, data.getRole(), data.duration_days);
            db_user.fullName = data.fullName;
            db_user.email = data.email;

            if( store.users.Insert(db_user).IsNull ) {await HttpContext.SendDataAsync(new httpErrorData {ErrorMessage = "User Exist"}); return;}

            string output = JsonConvert.SerializeObject(db_user);

            Console.WriteLine(output);
            
            var isMailSend = await sendMail(db_user,pw);
            
            var usr_resp = new UserResponse()
            {
                userName = db_user.userName,
                fullName = db_user.fullName,
                email = db_user.email,
                activity_expiry = db_user.activity_expiry.ToUniversalTime().ToString(),
                password_expiry = db_user.password.expiryUCT.ToString(),
                role = db_user.role.ToString(),
                temporary_pw = pw,
                isSend = isMailSend
            };
            await HttpContext.SendDataAsync(usr_resp);

        }

        public async Task<bool> sendMail(UserData user, string pw){

            Console.WriteLine("Sending e-mail");

            var apiKey = _conf.sendGridAPIkey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("pan.manfredini@gmail.com", "Admin");
            var subject = "Registered user";
            var to = new EmailAddress(user.email, user.fullName);
            var plainTextContent = $@"
                Hello {user.fullName},

                A user has been created for you!
                
                User Name                {user.userName}
                One Time Password        {pw}
                Role                     {user.role.ToString()}
                Valid Until              {user.activity_expiry.ToUniversalTime().ToString()}

                Please login at {HTTPServerBuilder.buildHostURL(_conf)+Routes.login.Substring(1)}

                Best regards,

                    Admin team.
            ";
            var htmlContent = $@"
                Hello {user.fullName}, <br>
                <br>
                A user has been created for you! <br>
                <br>
                <table style='width:35rem;'>
                <tr><td>User Name</td>  <td><strong>{user.userName}</strong></td> 
                <tr><td>One Time Password</td>  <td><strong>{pw}</strong></td> 
                <tr><td>Role</td>  <td><strong>{user.role.ToString()}</strong></td> 
                <tr><td>Valid Until</td>  <td><strong>{user.activity_expiry.ToUniversalTime().ToString()}</strong></td> 
                </table>
                <br>
                Please login at <a href='{HTTPServerBuilder.buildHostURL(_conf)+Routes.login.Substring(1)}'> {HTTPServerBuilder.buildHostURL(_conf)+Routes.login.Substring(1)}</a>
                <br>
                <br>
                Best regards,
                <br>
                    Admin team.
            ";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response.StatusCode == HttpStatusCode.Accepted;
        }


    }
    
}