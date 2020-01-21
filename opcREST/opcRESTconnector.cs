
using System.Threading;
using Newtonsoft.Json.Linq;
using OpcProxyClient;
using OpcProxyCore;
using EmbedIO;

namespace opcRESTconnector
{
    
    public class opcREST : IOPCconnect{
        public RESTconfigs _conf;
        public serviceManager manager;

        public WebServer server;

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
            _conf = config.ToObject<RESTconfigsWrapper>().RESTapi;
            server = HTTPServerBuilder.CreateWebServer(_conf,manager);
            server.RunAsync(cts.Token);
        }

        public void clean()
        {
            server.Dispose();
        }
    }
}