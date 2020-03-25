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
                        userAuth : [
                            ['pino','123','W'],
                            ['gino','123','R'],
                            ['paul','1234','A'],
                        ],
                        enableStaticFiles : true,
                        apyKey : 'pippo',
                        enableAPIkey : false
                    }
                }
            ");

            s = new serviceManager(json);

            opcREST rest = new opcREST();
            s.addConnector(rest);
            
            Task.Run( () => s.run() );

            Console.WriteLine("Warming up...");
            Thread.Sleep(1000);
            Console.WriteLine("Start Test...");
            
        }

        [Fact]
        public void test1()
        {   
            //var res_cookie = adminController.Response.Cookies;
            //Assert.Empty(res_cookie);
            //adminController.PreProcessRequest();
            //adminController.HttpContext = new IHttpContext();
            //await adminController.logon();
            //Assert.NotEmpty(res_cookie);
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
            
        }
    }
}