using System;
using System.Threading;
using EmbedIO;
using EmbedIO.Sessions;

namespace opcRESTconnector.Session{

    public class DummySessionManager : ISessionManager
    {
        UserStore users;
        public DummySessionManager(RESTconfigs conf){
            users = new UserStore(conf);
        }
        public ISession Create(IHttpContext context)
        {
            SimpleSession return_session = new SimpleSession();
            UserData _user = new UserData();
            if( context.User!= null && context.User.Identity.IsAuthenticated ) {
                    _user = users.GetUser(context.User.Identity.Name);
            }
            return_session["user"] = _user;
            return_session.BeginUse();
            return return_session;        
        }

        public void Delete(IHttpContext context, String x)
        {
            //throw new NotImplementedException();
        }

        public void OnContextClose(IHttpContext context)
        {
            //throw new NotImplementedException();
        }

        public void Start(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
        }
    }
}