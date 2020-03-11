using System;
using Xunit;
using Newtonsoft.Json.Linq;
using opcRESTconnector.Session;
using opcRESTconnector;

namespace UserDataStore
{

    public class UserDataTest{
        [Fact]
        public void Initialize()
        {
            var empty_u = new UserData();
            Assert.Equal("", empty_u.userName);
            Assert.Equal(AuthRoles.Undefined, empty_u.role);
            Assert.False( empty_u.hasWriteRights());
            Assert.Equal("", empty_u.password.admin_default);
            Assert.True( empty_u.password.expiry.Ticks < DateTime.UtcNow.Ticks);
            Assert.Equal("", empty_u.password.currentHash);
            Assert.False(empty_u.password.isValid(""));
        }

        [Fact]
        public void Methods()
        {
            var user = new UserData{
                userName = "pino",
                role = AuthRoles.Reader,
                password = new Password("123",10)
            };

            Assert.False(user.hasWriteRights());
            user.AllowWrite(new TimeSpan(10,0,0,0));
            Assert.False(user.hasWriteRights());
            user.role = AuthRoles.Writer;
            user.AllowWrite(new TimeSpan(10,0,0,0));
            Assert.True(user.hasWriteRights());
            Assert.True(user.password.isValid("123"));
        }
    }
    public class UserStoreTest{
        public RESTconfigs default_conf;
        public UserStoreTest(){
            string data = @"
                {
                    'RESTapi': {
                         userAuth : [
                            ['pino','123','R'],
                            ['gino','123','W'],
                            ['paul','123','A'],
                            ['john','123','J']
                        ]
                    }

            }";
            default_conf = JObject.Parse(data).ToObject<RESTconfigsWrapper>().RESTapi;
        }
        [Fact]
        public void Initialize()
        {
            var store = new UserStore(default_conf);
            Assert.NotNull(store);
            var user = store.GetUser("pino");
            Assert.NotNull(user);
            Assert.Equal("123",user.password.admin_default);
            Assert.Equal(AuthRoles.Reader, user.role);
            Assert.False(user.hasWriteRights());
        }
        [Fact]
        public void Methods()
        {
            var store = new UserStore(default_conf);
            var pino = store.GetUser("pino");
            var gino = store.GetUser("gino");
            var john = store.GetUser("john");
            var none = store.GetUser("none");

            Assert.False(pino.hasWriteRights());
            Assert.False(gino.hasWriteRights());
            Assert.False(john.hasWriteRights());
            Assert.False(none.hasWriteRights());

            Assert.True(pino.password.isValid("123"));
            Assert.True(gino.password.isValid("123"));
            Assert.True(john.password.isValid("123"));
            Assert.False(none.password.isValid("123"));

            Assert.False(pino.AllowWrite(TimeSpan.FromHours(1)));
            Assert.True(gino.AllowWrite(TimeSpan.FromHours(1)));
            Assert.False(john.AllowWrite(TimeSpan.FromHours(1)));
            Assert.False(none.AllowWrite(TimeSpan.FromHours(1)));
        }
    }

}