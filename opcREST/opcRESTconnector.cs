using Swan.Logging;
using System.Threading;
using Newtonsoft.Json.Linq;
using OpcProxyClient;
using OpcProxyCore;
using EmbedIO;
using System;
using NLog;

namespace opcRESTconnector
{
    
    public class opcREST : IOPCconnect{
        public RESTconfigs _conf;
        public serviceManager manager;

        public WebServer server;
        public static NLog.Logger logger = null;

        public opcREST(){
            logger = LogManager.GetLogger(this.GetType().Name);
        }

        public void OnNotification(object emitter, MonItemNotificationArgs items)
        {
            // Do nothing here
        }

        public void setServiceManager(serviceManager serv)
        {
            manager = serv;    
        }

        public void init(JObject config, CancellationTokenSource cts)
        {
            try{        
                _conf = config.ToObject<RESTconfigsWrapper>().RESTapi;
                if(!_conf.serverLog) {
                    try { Swan.Logging.Logger.UnregisterLogger<ConsoleLogger>();  }
                    catch { /* in case of multiple instance they seems to share Swan logging and this throws */ }
                }
                server = HTTPServerBuilder.CreateWebServer(_conf,manager);
                
                // mainly used in case of port already in use
                server.StateChanged += (s, e) => {
                    if(e.NewState == WebServerState.Stopped) {
                        // logger.Fatal("Webserver stopped: probably port " + _conf.port + " is already in use");
                        cts.Cancel();
                    }
                };

                server.RunAsync(cts.Token);

                // Bug in SWAN: https://github.com/unosquare/swan/issues/107
                Console.CursorVisible = true;
            }
            catch(Exception ex){
                logger.Error(ex.Message);
                cts.Cancel();
                // Bug in SWAN: https://github.com/unosquare/swan/issues/107
                Console.CursorVisible = true;
            }
        }


        public void clean()
        {
            server?.Dispose();
        }

        
    }
}