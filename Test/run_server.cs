using System;
using Xunit;
using opcRESTconnector;

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

            var url = "http://localhost:8080";
            // Our web server is disposable.
            using (var server = HTTPServerBuilder.CreateWebServer(url))
            {
                // Once we've registered our modules and configured them, we call the RunAsync() method.
                server.RunAsync();

                Console.ReadKey(true);
            }

        }
    }
}
