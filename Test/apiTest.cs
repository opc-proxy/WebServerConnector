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

namespace Test
{
    public class startServer:IDisposable
    {
        public serviceManager s;
        public HttpClient client;

        public startServer(){
            
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
                        port : 8087,
                        urlPrefix : '',
                        enableBasicAuth : false,
                        enableStaticFiles : false,
                        apyKey : 'pippo',
                        enableAPIkey : true
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

    public class APItests:IClassFixture<startServer>{
        public HttpClient http;
        public APItests(startServer s){
            this.http = s.client;
        }

        [Fact]
        public async void StaticFiles()
        {
            // Should not provide static files
            var errorResponse = await http.GetAsync("http://localhost:8087/");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, errorResponse.StatusCode);

            errorResponse = await http.GetAsync("http://localhost:8087/index.html");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, errorResponse.StatusCode);

            errorResponse = await http.GetAsync("http://localhost:8087/API");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, errorResponse.StatusCode);

            errorResponse = await http.GetAsync("http://localhost:8087/api");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, errorResponse.StatusCode);

        }

        [Fact]
        public async void restGET()
        {
            var response = await http.GetAsync("http://localhost:8087/api/REST/MyVariable");
            var errorResponse = await http.GetAsync("http://localhost:8087/api/REST/MyVaria");
            
            // Variable exist is success
            Assert.True(response.IsSuccessStatusCode);
            string body = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(body).ToObject<ReadResponse>();
            Assert.False(json.IsError);
            Assert.Single(json.Nodes);
            Assert.Equal( "MyVariable", json.Nodes[0].Name);
            Assert.True( json.Nodes[0].Value != null);
            Assert.True( json.Nodes[0].Type == "double");
            Assert.Equal("none", json.ErrorMessage);
            Assert.False(json.IsError);
            
            // Return not found if variable does not exist
            Assert.Equal(System.Net.HttpStatusCode.NotFound, errorResponse.StatusCode);
            body = await errorResponse.Content.ReadAsStringAsync();
            json = JObject.Parse(body).ToObject<ReadResponse>();
            Assert.True(json.IsError);
            Assert.Empty(json.Nodes);
            Assert.Equal("Not Found", json.ErrorMessage);
            
        }

        [Fact]
        public async void restPOST()
        {
            var query = new Dictionary<string, string>{
                { "value", "10" },
                { "apiKey", "pippo" }
            };

            var goodReq = new FormUrlEncodedContent(query);
            var response = await http.PostAsync("http://localhost:8087/api/REST/MyVariable", goodReq);
            var body = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<WriteResponse>();

            // If variable exist and api key provided then success
            Assert.True(response.IsSuccessStatusCode);
            Assert.False(body.IsError);
            Assert.Equal("none", body.ErrorMessage);

            // Variable does not exist
            response = await http.PostAsync("http://localhost:8087/api/REST/MyVarable", goodReq);
            body = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<WriteResponse>();
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.True(body.IsError);
            Assert.Equal("Not Found", body.ErrorMessage);

            // no apikey or wrong key -> not authorized
            var query2 = new Dictionary<string, string>{
                { "value", "10" },
                { "apiKey", "popo" }
            };
            var badReq = new FormUrlEncodedContent(query2);
            response = await http.PostAsync("http://localhost:8087/api/REST/MyVariable", badReq);
            var body2 = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<httpErrorData>();
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("Forbidden", body2.Error);

            // Bad variable value  --- here return not found, there is an issue about it already
            var query3 = new Dictionary<string, string>{
                { "value", "abc" },
                { "apiKey", "pippo" }
            };
            var badReq3 = new FormUrlEncodedContent(query3);
            response = await http.PostAsync("http://localhost:8087/api/REST/MyVariable", badReq3);
            var body3 = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<WriteResponse>();
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("Not Found", body3.ErrorMessage);
        }

        [Fact]
        public async void jsonGET(){
            
            // Success if all is good
            var  Req = new StringContent("{names:['MyVariable'], apiKey:'pippo'}", Encoding.UTF8, "application/json");
            var response = await http.PostAsync("http://localhost:8087/api/JSON/read", Req);

            Assert.True(response.IsSuccessStatusCode);
            ReadResponse resp = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<ReadResponse>();
            Assert.Single(resp.Nodes);
            Assert.Equal("MyVariable", resp.Nodes[0].Name);
            Assert.True(resp.Nodes[0].Value != null);

            // one var not found but still returning the other
            Req = new StringContent("{names:['MyVariable','notExist'], apiKey:'pippo'}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8087/api/JSON/read", Req);
            
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            resp = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<ReadResponse>();
            Assert.Single(resp.Nodes);
            Assert.Equal("MyVariable", resp.Nodes[0].Name);

            // fail if apiKey not provided
            Req = new StringContent("{names:['MyVariable'], apiKey:'pipo'}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8087/api/JSON/read", Req);

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
            var resp2 = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<httpErrorData>();
            Assert.Equal("Forbidden", resp2.Error);
        }

        [Fact]
        public async void jsonPOST()
        {
            // Success if all is good
            var  Req = new StringContent("{name:'MyVariable', apiKey:'pippo', value:32}", Encoding.UTF8, "application/json");
            var response = await http.PostAsync("http://localhost:8087/api/JSON/write", Req);
            var resp = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<WriteResponse>();

            Assert.True(response.IsSuccessStatusCode);
            Assert.False(resp.IsError);
            Assert.Equal("none", resp.ErrorMessage);

            // not found if var does not exist
            Req = new StringContent("{name:'MyVaable', apiKey:'pippo', value:32}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8087/api/JSON/write", Req);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

            // bad req if value not provided
            Req = new StringContent("{name:'MyVariable', apiKey:'pippo'}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8087/api/JSON/write", Req);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

            // Error if value not right type
            Req = new StringContent("{name:'MyVariable', apiKey:'pippo', value:'pollo'}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8087/api/JSON/write", Req);
            resp = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<WriteResponse>();
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.True(resp.IsError);

            // Forbidden if wrong apiKey
            Req = new StringContent("{name:'MyVariable', apiKey:'pppo', value:'7'}", Encoding.UTF8, "application/json");
            response = await http.PostAsync("http://localhost:8087/api/JSON/write", Req);
            var resp2 = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<httpErrorData>();
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
            Assert.True(resp2.Error == "Forbidden");

        }
    }
}
