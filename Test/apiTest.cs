using System;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using opcRESTconnector;
using OpcProxyCore;
//using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

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
                        'loglevel' :'fatal'
                    },

                    'nodesLoader' : {
                        'targetIdentifier' : 'browseName',
                        'whiteList':['MyVariable']

                    },

                    'RESTapi': {
                        serverLog:false,
                        port : 8089,
                        urlPrefix : '',
                        enableStaticFiles : false,
                        apiTokenProtection : true,
                        envVars: {
                            recaptchaClientKey : 'ciao'
                        }
                    }
                }
            ");
            
            Environment.SetEnvironmentVariable("OPC_WEBSERVER_APIKEY","1234567");

            s = new serviceManager(json);

            opcREST rest = new opcREST();
            s.addConnector(rest);
            
            Task.Run( () => s.run() );
            client = new HttpClient();

            Console.WriteLine("Warming up...");
            Thread.Sleep(1000);
            Console.WriteLine("API key " + rest._conf.GetEnvVars().apiKey);
            Console.WriteLine("OPC_WEBSERVER_SENDGRID " + rest._conf.GetEnvVars().sendGridAPIkey);
            Console.WriteLine("OPC_WEBSERVER_SENDGRID " + Environment.GetEnvironmentVariable("OPC_WEBSERVER_SENDGRID"));
            Console.WriteLine("Start Test...");
            Assert.Equal("",rest._conf.GetEnvVars().recaptchaClientKey);
            Assert.Equal("1234567",rest._conf.GetEnvVars().apiKey);

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
        public serviceManager service;
        public APItests(startServer s){
            this.http = s.client;
            this.service = s.s;
        }

        [Fact]
        public async void StaticFiles()
        {
            // Should not provide static files
            var errorResponse = await http.GetAsync("http://localhost:8089/");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, errorResponse.StatusCode);

            errorResponse = await http.GetAsync("http://localhost:8089/index.html");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, errorResponse.StatusCode);

            errorResponse = await http.GetAsync("http://localhost:8089/API");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, errorResponse.StatusCode);

            errorResponse = await http.GetAsync("http://localhost:8089/api");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, errorResponse.StatusCode);

        }

        [Fact]
        public async void restGET()
        {
            var response = await http.GetAsync("http://localhost:8089/api/REST/MyVariable");
            var errorResponse = await http.GetAsync("http://localhost:8089/api/REST/MyVaria");
            
            // Variable exist is success
            Assert.True(response.IsSuccessStatusCode);
            string body = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(body).ToObject<ReadResponse>();
            Assert.True(json.Success);
            Assert.Equal( "MyVariable", json.Name);
            Assert.True( json.Value != null);
            Assert.True( json.Type == "double");
            Assert.Equal("", json.ErrorCode);
            Assert.True(json.Success);
            
            // Return not found if variable does not exist
            Assert.Equal(System.Net.HttpStatusCode.OK, errorResponse.StatusCode);
            body = await errorResponse.Content.ReadAsStringAsync();
            json = JObject.Parse(body).ToObject<ReadResponse>();
            Assert.False(json.Success);
            Assert.Null(json.Value);
            Assert.True( json.Type == "");
            Assert.Equal(ErrorCodes.VarNotExist, json.ErrorCode);
            
        }

        [Fact]
        public async void restPOST()
        {
            var query = new Dictionary<string, string>{
                { "value", "10" },
            };

            var goodReq = new FormUrlEncodedContent(query);
            goodReq.Headers.Add("X-API-Token", "1234567");
            var response = await http.PostAsync("http://localhost:8089/api/REST/MyVariable", goodReq);
            var body = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<WriteResponse>();

            // If variable exist and api key provided then success
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(body.Success);
            Assert.Equal("", body.ErrorCode);

            // Variable does not exist
            response = await http.PostAsync("http://localhost:8089/api/REST/MyVarable", goodReq);
            body = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<WriteResponse>();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.False(body.Success);
            Assert.Equal(ErrorCodes.VarNotExist, body.ErrorCode);

            // no apikey or wrong key -> not authorized
            var query2 = new Dictionary<string, string>{
                { "value", "10" },
            };
            var badReq = new FormUrlEncodedContent(query2);
            badReq.Headers.Add("X-API-Token", "123456ew7");
            response = await http.PostAsync("http://localhost:8089/api/REST/MyVariable", badReq);
            var body2 = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<ErrorData>();
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(ErrorCodes.Forbidden, body2.ErrorCode);

            // Bad variable value
            var query3 = new Dictionary<string, string>{
                { "value", "abc" },
            };
            var badReq3 = new FormUrlEncodedContent(query3);
            badReq3.Headers.Add("X-API-Token", "1234567");
            response = await http.PostAsync("http://localhost:8089/api/REST/MyVariable", badReq3);
            var body3 = JObject.Parse(await response.Content.ReadAsStringAsync()).ToObject<WriteResponse>();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ErrorCodes.BadValue, body3.ErrorCode);
        }

        [Fact]
        public async void jsonGET(){
            
            // Success if all is good
            var  Req = new StringContent("{names:['MyVariable']}", Encoding.UTF8, "application/json"); 
            Req.Headers.Add("X-API-Token","1234567");
            var response = await http.PostAsync("http://localhost:8089/api/JSON/read", Req);
            //var js_string = "'"+(await response.Content.ReadAsStringAsync())+"'";
            //var js_string = "[{\"Type\": \"double\",\"Timestamp\": \"4\\/21\\/2020 3:21:36 PM\",\"Timestamp_ms\": 1587482496470,\"Name\": \"MyVariable\",\"Value\": 23.80000000000009,\"Success\": true,\"ErrorCode\": \"\"}]";
            Assert.True(response.IsSuccessStatusCode);
            //var resp = JObject.Parse(js_string).ToObject<List<ReadResponse>>();
            var resp = JsonSerializer.Deserialize<List<ReadResponse>>(await response.Content.ReadAsStringAsync());
            Assert.Single(resp);
            Assert.Equal("MyVariable", resp[0].Name);
            Assert.True(resp[0].Value != null);

            // one var not found but still returning the other
            Req = new StringContent("{names:['MyVariable','notExist']}", Encoding.UTF8, "application/json");
            Req.Headers.Add("X-API-Token","1234567");
            response = await http.PostAsync("http://localhost:8089/api/JSON/read", Req);
            
            Assert.True(response.IsSuccessStatusCode);
            resp = JsonSerializer.Deserialize<List<ReadResponse>>(await response.Content.ReadAsStringAsync());
            Assert.Equal(2, resp.Count);
            Assert.Equal("MyVariable", resp[0].Name);
            Assert.Equal("notExist", resp[1].Name);
            Assert.Equal(ErrorCodes.VarNotExist, resp[1].ErrorCode);

            // fail if apiKey not provided
            Req = new StringContent("{names:['MyVariable']}", Encoding.UTF8, "application/json");
            Req.Headers.Add("X-API-Token","123456fd7");
            response = await http.PostAsync("http://localhost:8089/api/JSON/read", Req);

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
            var resp2 = JsonSerializer.Deserialize<ErrorData>(await response.Content.ReadAsStringAsync());
            Assert.Equal(ErrorCodes.Forbidden, resp2.ErrorCode);
        }

        [Fact]
        public async void jsonPOST()
        {
            // Success if all is good
            var  Req = new StringContent("{names:['MyVariable'], values:[32]}", Encoding.UTF8, "application/json");
            Req.Headers.Add("X-API-Token","1234567");
            var response = await http.PostAsync("http://localhost:8089/api/JSON/write", Req);
            var resp = JsonSerializer.Deserialize<List<WriteResponse>>(await response.Content.ReadAsStringAsync());

            Assert.True(response.IsSuccessStatusCode);
            Assert.True(resp[0].Success);
            Assert.Equal("", resp[0].ErrorCode);

            // not found if var does not exist
            Req = new StringContent("{names:['MyVaable'], values:[32]}", Encoding.UTF8, "application/json");
            Req.Headers.Add("X-API-Token","1234567");
            response = await http.PostAsync("http://localhost:8089/api/JSON/write", Req);
            Assert.True(response.IsSuccessStatusCode);
            resp = JsonSerializer.Deserialize<List<WriteResponse>>(await response.Content.ReadAsStringAsync());
            Assert.Equal(ErrorCodes.VarNotExist, resp[0].ErrorCode);

            // bad req if value not provided
            Req = new StringContent("{names:['MyVariable']}", Encoding.UTF8, "application/json");
            Req.Headers.Add("X-API-Token","1234567");
            response = await http.PostAsync("http://localhost:8089/api/JSON/write", Req);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

            // Error if value not right type
            Req = new StringContent("{names:['MyVariable'],values:['pollo']}", Encoding.UTF8, "application/json");
            Req.Headers.Add("X-API-Token","1234567");
            response = await http.PostAsync("http://localhost:8089/api/JSON/write", Req);
            resp = JsonSerializer.Deserialize<List<WriteResponse>>(await response.Content.ReadAsStringAsync());
            Assert.Equal(ErrorCodes.BadValue, resp[0].ErrorCode);
            Assert.False(resp[0].Success);

            // Forbidden if wrong apiKey
            Req = new StringContent("{names:['MyVariable'], values:['7']}", Encoding.UTF8, "application/json");
            Req.Headers.Add("X-API-Token","123456ew7");
            response = await http.PostAsync("http://localhost:8089/api/JSON/write", Req);
            var resp2 = JsonSerializer.Deserialize<ErrorData>(await response.Content.ReadAsStringAsync());
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
            Assert.True(resp2.ErrorCode == ErrorCodes.Forbidden);

        }
    }
}
