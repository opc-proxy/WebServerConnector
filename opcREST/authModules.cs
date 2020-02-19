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


    public class CustomBaseAthentication : BasicAuthenticationModule
    {
        public CustomBaseAthentication(RESTconfigs conf) : base("/", "Access to site")
        {
            foreach(var user_item in conf.userAuth)
            {
                if(user_item.Count == 3){
                    this.WithAccount(user_item[0],user_item[1]);
                }
                else{
                    throw new Exception("Malformed user item: " + user_item[0]);
                }
            }
            
        }
    }

    public class AuthorizationModule : WebModuleBase
    {
        Dictionary<string, AuthRoles> user_roles;

        public AuthorizationModule(RESTconfigs conf)
            : base("/")
        {
            user_roles = new Dictionary<string, AuthRoles>();
            if(!conf.enableBasicAuth) return;

            // filling the dictionary with defined roles and users
            foreach(var user_item in conf.userAuth)
            {
                var role = AuthRoles.Undefined;

                if(user_item.Count == 3){
                    switch (user_item[2].ToLower()){
                        case "r" :
                            role = AuthRoles.Reader;
                            break;
                        case "w":
                            role = AuthRoles.Writer;
                            break;
                        case "a": 
                            role = AuthRoles.Admin;
                            break;
                        default:
                            role = AuthRoles.Undefined;
                            Console.WriteLine("Warning: Undefined user role");
                            break;
                    }

                    user_roles.Add(user_item[0], role);
                }
                else{
                    throw new Exception("Malformed user item: " + user_item[0]);
                }
            }
        }

        public override bool IsFinalHandler => false;

        protected override Task OnRequestAsync(IHttpContext context)
        {
            return Task.Run(()=>{
                if( context.User!= null && context.User.Identity.IsAuthenticated ) {
                    var role = AuthRoles.Undefined;
                    user_roles.TryGetValue(context.User.Identity.Name, out role);
                    context.Items.Add("Role",role);
                }
                else context.Items.Add("Role",AuthRoles.Undefined);
            });
        }
    }
}
