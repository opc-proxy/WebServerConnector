using System;
using Xunit;
using opcRESTconnector;
using OpcProxyCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Test
{
    public class runserver:IDisposable
    {
        public runserver(){


        }

        public void Dispose(){

        }

        [Fact]
        public void Test()
        {

/*            var url = "http://localhost:8082";
            // Our web server is disposable.
            using (var server = HTTPServerBuilder.CreateWebServer(url))
            {
                // Once we've registered our modules and configured them, we call the RunAsync() method.
                server.RunAsync();

                Console.ReadKey(true);
            }
            */
            var json = JObject.Parse(@"
                {
                    'opcServerURL':'opc.tcp://localhost:4840/freeopcua/server/',
                    'loggerConfig' :{
                        'loglevel' :'info'
                    },

                    'nodesLoader' : {
                        'targetIdentifier' : 'browseName',
                        'whiteList':['MyVariable']

                    }
                }
            ");

            serviceManager s = new serviceManager(json);

            opcREST rest = new opcREST();
            s.addConnector(rest);
            s.run();

        }
    }
}
