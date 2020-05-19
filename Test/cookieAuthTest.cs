using System;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using opcRESTconnector;
using OpcProxyCore;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using LiteDB;

namespace Test
{
    public class cookieAuthTest{
        serviceManager s;
        opcREST rest;
        public cookieAuthTest(){
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
                        port : 8087,
                        urlPrefix : '',
                        'serverLog' : false,
                        'enableCookieAuth' : true,
                        'writeExpiryMinutes' : 0.015,
                        'appStoreFileName':'cookifile.data',
                        'enableStaticFiles' : true,
                    }
                }
            ");

            s = new serviceManager(json);

            rest = new opcREST();
            s.addConnector(rest);
            
            Task.Run( () => s.run() );

            Console.WriteLine("Warming up...");
            Thread.Sleep(500);
            
            var pino = new UserData("pino","123",AuthRoles.Writer, 1);
            var gino = new UserData("gino","123",AuthRoles.Reader, 1);
            rest.app_store.users.Upsert(pino);
            rest.app_store.users.Upsert(gino);

            Thread.Sleep(500);
            Console.WriteLine("Start Test...");
            
        }

        [Fact]
        public void test1()
        {   

            var process = new Process {
              StartInfo = new ProcessStartInfo {
                    FileName = "../../../headlessBrowserTest/node_modules/mocha/bin/mocha",
                    Arguments = "../../../headlessBrowserTest/test.js",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }

            };
            process.StartInfo.UseShellExecute = false;
            process.OutputDataReceived += (sender, args) => Console.WriteLine( args.Data);
            process.ErrorDataReceived += (sender, args) => Console.WriteLine( args.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            Console.WriteLine("done");
            rest.app_store.db.DropCollection("users");
            rest.app_store.db.Rebuild();
            
        }
    }
}