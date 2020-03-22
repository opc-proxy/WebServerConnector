using System;
using System.Threading;
using EmbedIO;
using EmbedIO.Sessions;
using opcRESTconnector.Data;

namespace opcRESTconnector.Session{

    /// <summary>
    /// Dummy Session that allows the user to read and write at all times
    /// </summary>
    public class DummySessionManager : ISessionManager
    {
        DataStore users;
        public DummySessionManager(RESTconfigs conf){
            users = new DataStore(conf);
        }
        public ISession Create(IHttpContext context)
        {
            SimpleSession return_session = new SimpleSession();
            UserData _user = new UserData();
            _user.role = AuthRoles.Writer;
            _user.AllowWrite(TimeSpan.FromMinutes(3));
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