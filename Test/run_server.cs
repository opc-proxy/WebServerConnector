using System;
using Xunit;
using opcRESTconnector;

namespace Test
{
    public class runserver
    {
        public runserver(){
        }

        [Fact]
        public void Test()
        {


            var url = "http://localhost:9696/";
            // Our web server is disposable.
            using (var server = HTTPServer.CreateWebServer(url))
            {
                // Once we've registered our modules and configured them, we call the RunAsync() method.
                server.RunAsync();

                Console.ReadKey(true);
            }

        }
    }
}
