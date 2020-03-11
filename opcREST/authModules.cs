using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using Swan.Logging;
using EmbedIO.Authentication;
using System;
using OpcProxyCore;
using System.Collections.Generic;

using System.Security.Principal;
using EmbedIO.Utilities;


namespace opcRESTconnector {

    public enum AuthRoles{
        Reader,
        Writer,
        Admin,
        Undefined
    }

    public class EnforceAuth : WebModuleBase
    {
        public EnforceAuth() : base("/") {}
        public override bool IsFinalHandler => false;

        protected override Task OnRequestAsync(IHttpContext context)
        {
            object usr = null;
            var session = context.Session;
            session.TryGetValue("user",out usr);
            Console.WriteLine("Auth Module: sid " + session.Id);
            Console.WriteLine("Auth Module: Empty " + session.IsEmpty);
            Console.WriteLine("Auth Module: Count " + session.Count);
            Console.WriteLine("Auth Module: User " + session.ContainsKey("user"));
            Console.WriteLine("Auth Module: User1 " + usr);
            if(String.IsNullOrEmpty(session.Id) || session.IsEmpty ) { 
                context.Response.StatusCode = 403;
                context.SetHandled();
                return context.SendStringAsync(HTMLtemplates.forbidden("/admin/login/"),"text/html",Encoding.UTF8);
                //throw HttpException.Forbidden("Unauthorized Access");
                //throw HttpException.Redirect("/admin/login/",401);
            }
            else return Task.CompletedTask;
        }
    }
}
