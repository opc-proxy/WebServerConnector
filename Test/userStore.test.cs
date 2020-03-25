using System;
using Xunit;
using Newtonsoft.Json.Linq;
using opcRESTconnector.Session;
using opcRESTconnector;
using opcRESTconnector.Data;

namespace UserDataStore
{

    public class UserDataTest{
        [Fact]
        public void Initialize()
        {
            var empty_u = new UserData();
            Assert.Equal("Anonymous", empty_u.userName);
            Assert.Equal(AuthRoles.Undefined, empty_u.role);
            Assert.True( empty_u.password.expiry.Ticks < DateTime.UtcNow.Ticks);
            Assert.Null(empty_u.password.hashedValue);
            Assert.False(empty_u.password.isValid(""));
        }

        [Fact]
        public void Methods()
        {
            var user = new UserData("pino","123",AuthRoles.Reader, 1);
            var _s =  new sessionData(user,1);
            Assert.True(user.isActive());
            Assert.False(user.password.isActive());
            // user password is expired, but still can validate
            Assert.True(user.password.isValid("123"));
            user.password.update_password("123","1234",1);
            Assert.True(user.password.isActive());
            Assert.True(user.isActive());
            Assert.False(_s.hasWriteRights());
            _s.AllowWrite("1234",1);
            Assert.False(_s.hasWriteRights());
            user.role = AuthRoles.Writer;
            _s.AllowWrite("1234",1);
            Assert.True(_s.hasWriteRights());
            Assert.False(user.password.isValid("123"));
            Assert.True(user.password.isValid("1234"));
        }
    }
    public class DataStoreTest{
        public RESTconfigs default_conf;
        DataStore store;
        string session_id;
        public DataStoreTest(){
            
            default_conf = new RESTconfigs();

            store = new DataStore(default_conf);
            var pino = new UserData {
                userName = "pino",
                password = new Password("123",1),
                role = AuthRoles.Reader,
                activity_expiry = DateTime.UtcNow.AddDays(1)
            };
            var gino = new UserData {
                userName = "gino",
                password = new Password("123",1),
                role = AuthRoles.Writer,
                activity_expiry = DateTime.UtcNow.AddDays(1)
            };

            var session = new sessionData(pino,1);
            store.users.Upsert(pino);
            store.users.Upsert(gino);
            session_id = store.sessions.Insert(session)?.AsString;
            string session_id2 = store.sessions.Insert(session);
            Assert.Null(session_id2);
        }
        
        [Fact]
        public void Initialize()
        {
            Assert.NotNull(store);
            var user = store.users.Get("pino");
            var anonymous = store.users.Get("john");
            Assert.NotNull(user);
            Assert.NotNull(anonymous);
            Assert.Equal("Anonymous",anonymous.userName);
            Assert.Equal(AuthRoles.Reader, user.role);

            // Session
            var s = store.sessions.Get(session_id);
            Assert.NotNull(s);
            Assert.Equal("pino",s.user.userName);
            Assert.False(s.hasWriteRights());
            
            // Quick session
            var session = new sessionData(user, 1);
            string s_name = store.sessions.Insert(session);
            // does not modify gino's props
            var gino = store.users.Get("pino");
            Assert.True(gino.password.isValid("123"));
            
            var s2 = store.sessions.Get(s_name);
            s2.expiry = DateTime.UtcNow.AddYears(1);
            s2.user.role = AuthRoles.Undefined;
            store.sessions.Update(s2);
            // does not modify gino's props
            // it should not is a NoSQL... But you never know
            var gino2 = store.users.Get("pino");
            Assert.Equal(AuthRoles.Reader, gino2.role);
            Assert.NotNull(store.sessions.GetAndUpdateLastSeen(session_id));
        }
        [Fact]
        public void Methods()
        {
            var pino = store.users.Get("pino");
            var s_pino = new sessionData(pino,1);
            var gino = store.users.Get("gino");
            var s_gino = new sessionData(gino,1);
            var none = store.users.Get("none");
            var s_none = new sessionData(none,1);

            Assert.False(s_pino.hasWriteRights());
            Assert.False(s_gino.hasWriteRights());
            Assert.False(s_none.hasWriteRights());

            Assert.True(pino.password.isValid("123"));
            Assert.True(gino.password.isValid("123"));
            Assert.False(none.password.isValid("123"));

            Assert.Equal( UsrStatusCodes.NotAuthorized,s_pino.AllowWrite("123",1));
            Assert.Equal(UsrStatusCodes.Success, s_gino.AllowWrite("123",1));
            Assert.Equal(UsrStatusCodes.ExpiredUsr, s_none.AllowWrite("123",1));
        }
    }

}