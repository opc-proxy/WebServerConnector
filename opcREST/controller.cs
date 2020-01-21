using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using EmbedIO;
using System;

//using Unosquare.Tubular;

namespace opcRESTconnector
{
    
    public sealed class nodeRESTController : WebApiController
    {
        public string t;
        public nodeRESTController(string txt){
            t = txt;
        }
                
        [Route(HttpVerbs.Get, "/people/{id?}")]
        public async Task<Person> GetPeople(int id)
            => (await Person.GetDataAsync().ConfigureAwait(false)).FirstOrDefault(x => x.Id == id)
            ?? throw HttpException.NotFound();

        
        [Route(HttpVerbs.Post, "/write")]
         public async Task<string> PostData() 
        {
            var data = await HttpContext.GetRequestFormDataAsync();	
            
            if(data.ContainsKey("name") && data.ContainsKey("apiKey")){
                var name = data.Get("name");
                var api_key = data.Get("apiKey");
                Console.WriteLine("ciao ----> " + t);
                return "key : " + api_key + " name " + name;
            }
            else throw HttpException.BadRequest();
        }


    }
}