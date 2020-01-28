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
            Assert.Equal( 1, json.Nodes.Count);
            Assert.Equal( "MyVariable", json.Nodes[0].Name);
            Assert.True( json.Nodes[0].Value != "");
            Assert.True( json.Nodes[0].Value != "none");
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
    }
}
