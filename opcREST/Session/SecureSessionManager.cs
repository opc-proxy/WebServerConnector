using System;
using System.Linq;
using System.Net;
using EmbedIO.Utilities;
using EmbedIO.Sessions;
using EmbedIO;
using Jose;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace opcRESTconnector.Session
{
    class SecureSessionManager : LSManagerCopy {
        private byte[] secret;
        
        public SecureSessionManager(){
            secret = new byte[32];
            var rnd = new RNGCryptoServiceProvider();
            rnd.GetNonZeroBytes(secret);
            Console.WriteLine("Secret 0 " + secret[0].ToString());

            CookieHttpOnly = true;
            CookieName = "_opcSession";
            CookiePath = "/" ;
        }

        /// <summary>
        /// It does not actually CREATE any session, it retrieves the session if cookie is 
        /// present and validates it, add a dummy Anonimous session otherwise. It does not add any cookie.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public new ISession Create(IHttpContext context){

            var id = GetSessionId(context);

            SimpleSession session;
            lock (_sessions)
            {
                if (!string.IsNullOrEmpty(id) && _sessions.TryGetValue(id, out session))
                {
                    session.BeginUse();
                }
                else session = new SimpleSession("",TimeSpan.Zero);
            }
            return session;
        }

        public new string GetSessionId(IHttpContext context){
        
            string cookieValue =  context.Request.Cookies.FirstOrDefault(IsSessionCookie)?.Value.Trim() ?? string.Empty;    
            return AuthenticateCookie(cookieValue);
        }

        public void RegisterSession(IHttpContext context){
            
            string id = UniqueIdGenerator.GetNext();
            lock (_sessions) {    
                SimpleSession session = new SimpleSession(id, SessionDuration);
                _sessions.TryAdd(id, session);
            }
            
            var cookie = createSecureCookie(id);
            context.Request.Cookies.Add(cookie);
            context.Response.Cookies.Add(cookie);
        }
        public string AuthenticateCookie(string cookieValue){
            return "";
        }
        public Cookie createSecureCookie(string id){

            Cookie c = BuildSessionCookie(id);
            var payload = new Dictionary<string, object>()
            {
                { "sub", c.Value },
                { "exp", c.Expires.Ticks }
            };
            string token = Jose.JWT.Encode(payload, secret, JweAlgorithm.A256GCMKW, JweEncryption.A256CBC_HS512);

            c.Value = token + "; SameSite=Strict";
            
            return c;
        }
    }
}