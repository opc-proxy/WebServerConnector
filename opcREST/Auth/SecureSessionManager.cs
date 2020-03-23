using System;
using System.Linq;
using System.Net;
using EmbedIO.Utilities;
using EmbedIO.Sessions;
using EmbedIO;
using Jose;
using System.Security.Cryptography;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using opcRESTconnector.Data;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace opcRESTconnector.Session
{
    public class SecureSessionManager : LSManagerCopy {
        private byte[] secret;
        public static NLog.Logger logger = null;

        public DataStore store;

        private RESTconfigs _conf;
        
        public SecureSessionManager( RESTconfigs conf, DataStore app_store){

            secret = new byte[32];
            var rnd = new RNGCryptoServiceProvider();
            rnd.GetNonZeroBytes(secret);

            CookieHttpOnly = true;
            CookieName = "_opcSession";
            CookiePath = "/" ;
            CookieDuration = TimeSpan.FromHours(conf.sessionExpiryHours);

            logger = LogManager.GetLogger(this.GetType().Name);

            store = app_store;
            _conf = conf;
        }
        
        /// <summary>
        /// It does not actually CREATE any session, it retrieves the session if cookie is 
        /// present and validates it, add a dummy Anonimous session otherwise. It does not add any cookie.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ISession Create(IHttpContext context){
            logger.Debug("entered in create");
            var id = GetSessionId(context);

            SimpleSession session = new SimpleSession();
            lock (store)
            {
                if (string.IsNullOrEmpty(id)) return session;
                var db_session = store.sessions.GetAndUpdateLastSeen(id);
                if(db_session == null) return session;
                
                logger.Debug("Session Found");
                session = new SimpleSession(db_session);
                session.BeginUse();
            }
            return session;
        }

        public override string GetSessionId(IHttpContext context){
        
            string cookieValue =  context.Request.Cookies.FirstOrDefault(IsSessionCookie)?.Value.Trim() ?? string.Empty;    
            return AuthenticateCookie(cookieValue);
        }

        public SimpleSession RegisterSession(IHttpContext context, UserData user){
            logger.Debug("Registering Session");
            var db_session = new sessionData(user, _conf.sessionExpiryHours / 24);
            string id = store.sessions.Insert(db_session);
            if(String.IsNullOrEmpty(id)) return new SimpleSession();
            
            var session =  new SimpleSession(db_session);
            var cookie = createSecureCookie(id);
            context.Request.Cookies.Add(cookie);
            context.Response.Cookies.Add(cookie);
            return session;
        }
        public string AuthenticateCookie(string cookieValue){
            logger.Debug("Authenticating cookie value " + cookieValue);
            jwtPayload j = null;
            try{
                string jwt = JWT.Decode(cookieValue,secret,JweAlgorithm.A256KW, JweEncryption.A256CBC_HS512);
                j = JObject.Parse(jwt).ToObject<jwtPayload>();
                if(j.exp < DateTime.UtcNow.Ticks ) throw new Exception("token expired");
            }
            catch(Exception e) {
                logger.Debug("Auth failed: "+ e.Message);
                j = new jwtPayload();
            }
            return j.sub;
        }
        public Cookie createSecureCookie(string id){
            logger.Debug("Creating cookie");
            Cookie c = BuildSessionCookie(id);
            var payload = new jwtPayload();
            payload.exp = c.Expires.Ticks;
            payload.sub = c.Value;
            string token = Jose.JWT.Encode(payload.ToString(), secret, JweAlgorithm.A256KW, JweEncryption.A256CBC_HS512);
            logger.Debug("jwt created : " + token);
            c.Value = token + "; SameSite=Strict";
            
            return c;
        }

        public override void Delete(IHttpContext context, string x)
        {
            RemoveSession(context);
        }

        public string RemoveSession(IHttpContext context){

            var id = GetSessionId(context);

            if( store.sessions.DeletFromId(id) ) {
                logger.Debug("Session Removed : " + id );
            }
            else {
                logger.Debug("Session Not Found : " + id );
            }
            context.Request.Cookies.Add(BuildSessionCookie(string.Empty));
            context.Response.Cookies.Add(BuildSessionCookie(string.Empty));
            return id;
        }

        public override void Start(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        store.sessions.PurgeExpired();
                        await Task.Delay(PurgeInterval, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            }, cancellationToken);
        }
    }

    public class jwtPayload{
        public string sub {get;set;}
        public long exp {get;set;}

        public new string ToString(){
            return JObject.FromObject(this).ToString();
        }
        public jwtPayload(){
            sub = String.Empty;
            exp = DateTime.UtcNow.Ticks - 10;
        }
    }
}