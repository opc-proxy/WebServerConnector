using System;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using opcRESTconnector;
using OpcProxyCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers ;
using System.Net;

namespace Test
{
    public class startServer2:IDisposable
    {
        public serviceManager s;
        public HttpClient client;

        public startServer2(){
            
            var json = JObject.Parse(@"
                {
                    'opcServerURL':'opc.tcp://localhost:4840/freeopcua/server/',
                    'loggerConfig' :{
                        'loglevel' :'info'
                    },

                    'nodesLoader' : {
                        'targetIdentifier' : 'browseName',
                        'whiteList':['MyVariable']

                    },

                    'RESTapi': {
                        port : 8076,
                        urlPrefix : '',
                        enableBasicAuth : true,
                        basicAuth : [
                            ['pino','123','R'],
                            ['gino','123','W'],
                            ['paul','1234','A'],
                            ['john','1234','J']
                        ],
                        enableStaticFiles : false,
                        apyKey : 'pippo',
                        enableAPIkey : false
                    }
                }
            ");

            s = new serviceManager(json);

            opcREST rest = new opcREST();
            s.addConnector(rest);
            
            Task.Run( () => s.run() );
            client = new HttpClient();

            Console.WriteLine("Warming up...");
            Thread.Sleep(1000);
            Console.WriteLine("Start Test...");

        }

        public void Dispose(){
            s.cancellationToken.Cancel();
            Console.WriteLine("closing...");
            Thread.Sleep(1000);
            Console.WriteLine("closed...");
        }
    }

    public class AuthTests:IClassFixture<startServer2>{
        public HttpClient http;
        public AuthTests(startServer2 s){
            this.http = s.client;
        }

        [Fact]
        public async void Authentication()
        {   
            // Not setting auth header give Unauthorized
            var http2 = new HttpClient();
            var response = await http2.GetAsync("http://localhost:8076/api/REST/MyVariable");
            Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode );

            // Good call is success
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( 
                "Basic", Convert.ToBase64String( System.Text.ASCIIEncoding.ASCII.GetBytes("pino:123"))
            );
            response = await http.GetAsync("http://localhost:8076/api/REST/MyVariable");
            Assert.True(response.IsSuccessStatusCode);

            // bad user auth dont pass at any depth 
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( 
                "Basic", Convert.ToBase64String( System.Text.ASCIIEncoding.ASCII.GetBytes("pin:123"))
            );
            response = await http.GetAsync("http://localhost:8076/api/REST/MyVariable");
            Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode );
            response = await http.GetAsync("http://localhost:8076/api/REST/riable");
            Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode );
            response = await http.GetAsync("http://localhost:8076/api/JSON/riable");
            Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode );
            response = await http.GetAsync("http://localhost:8076/api/");
            Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode );
            response = await http.GetAsync("http://localhost:8076/");
            Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode );
        }

        [Fact]
        public async void AuthorizzationREAD()
        {
            // Read Role can read REST
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( 
                "Basic", Convert.ToBase64String( System.Text.ASCIIEncoding.ASCII.GetBytes("pino:123"))
            );
            var response = await http.GetAsync("http://localhost:8076/api/REST/MyVariable");
            Assert.True(response.IsSuccessStatusCode);


            // Read Role CANNOT WRITE REST
            var query = new Dictionary<string, string>{
                { "value", "10" }
            };

            var goodReq = new FormUrlEncodedContent(query);
            response = await http.PostAsync("http://localhost:8076/api/REST/MyVariable", goodReq);
            var body = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<httpErrorData>();
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("Forbidden", body.Error);

            // Read Role CAN READ JSON
            var Req = new StringContent("{names:['MyVariable']}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8076/api/JSON/read", Req);
            Assert.True(response.IsSuccessStatusCode);

            // ROLE READ CANNOT WRITE JSON
            Req = new StringContent("{name:'MyVariable', value:32}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8076/api/JSON/write", Req);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            
        }

        [Fact]
        public async void AuthorizationWRITE()
        {
            // ROLE WRITE can READ
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( 
                "Basic", Convert.ToBase64String( System.Text.ASCIIEncoding.ASCII.GetBytes("gino:123"))
            );
            var response = await http.GetAsync("http://localhost:8076/api/REST/MyVariable");
            Assert.True(response.IsSuccessStatusCode);

            // ROLE WRITE CAN WRITE
            var query = new Dictionary<string, string>{
                { "value", "10" }
            };

            var goodReq = new FormUrlEncodedContent(query);
            response = await http.PostAsync("http://localhost:8076/api/REST/MyVariable", goodReq);
            Assert.True(response.IsSuccessStatusCode);

            // WRITE Role CAN READ JSON
            var Req = new StringContent("{names:['MyVariable']}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8076/api/JSON/read", Req);
            Assert.True(response.IsSuccessStatusCode);

            // ROLE WRITE CANNOT WRITE JSON
            Req = new StringContent("{name:'MyVariable', value:32}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8076/api/JSON/write", Req);
            Assert.True(response.IsSuccessStatusCode);
            
        }
        [Fact]
        public async void AuthAdmin()
        {
            // ROLE ADMIN can READ
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( 
                "Basic", Convert.ToBase64String( System.Text.ASCIIEncoding.ASCII.GetBytes("paul:1234"))
            );
            var response = await http.GetAsync("http://localhost:8076/api/REST/MyVariable");
            Assert.True(response.IsSuccessStatusCode);

            // ROLE ADMIN CAN WRITE
            var query = new Dictionary<string, string>{
                { "value", "10" }
            };

            var goodReq = new FormUrlEncodedContent(query);
            response = await http.PostAsync("http://localhost:8076/api/REST/MyVariable", goodReq);
            Assert.True(response.IsSuccessStatusCode);

            // ADMIN Role CAN READ JSON
            var Req = new StringContent("{names:['MyVariable']}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8076/api/JSON/read", Req);
            Assert.True(response.IsSuccessStatusCode);

            // ROLE ADMIN CANNOT WRITE JSON
            Req = new StringContent("{name:'MyVariable', value:32}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8076/api/JSON/write", Req);
            Assert.True(response.IsSuccessStatusCode);
            
        }

        [Fact]
        public async void YouShallNotPass()
        {
            // ROLE Undefined can READ
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( 
                "Basic", Convert.ToBase64String( System.Text.ASCIIEncoding.ASCII.GetBytes("john:1234"))
            );
            var response = await http.GetAsync("http://localhost:8076/api/REST/MyVariable");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);


            // ROLE UNDEFINED  CAN WRITE
            var query = new Dictionary<string, string>{
                { "value", "10" }
            };

            var goodReq = new FormUrlEncodedContent(query);
            response = await http.PostAsync("http://localhost:8076/api/REST/MyVariable", goodReq);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            // UNDEFINED Role CAN READ JSON
            var Req = new StringContent("{names:['MyVariable']}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8076/api/JSON/read", Req);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            // ROLE UNDEFINED CANNOT WRITE JSON
            Req = new StringContent("{name:'MyVariable', value:32}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8076/api/JSON/write", Req);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            
        }
    }
}