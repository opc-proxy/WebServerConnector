using System.Collections.Generic;
using System.Threading.Tasks;
using Swan;
using Swan.Cryptography;
using System;

namespace opcRESTconnector  
{
    public class NodeValue
    {
        public string Name {get;set;}
        public string Type {get;set;}
        public object Value {get;set;}
        public string Timestamp {get;set;}

        public NodeValue(){
            Name = "none";
            Type = "none";
            Value = null;
            Timestamp = "none";
        }
    }

    public class ReadResponse :ErrorData{
        public List<NodeValue> Nodes {get;set;}
        public ReadResponse(){
            Nodes = new List<NodeValue>{};
            ErrorMessage = "none";
            Success = true;
        }
    }

    public class WriteResponse : ErrorData{
        public WriteResponse(){
            ErrorMessage = "none";
            Success = true;
        }
    }

    public class ReadRequest{
        public List<String> names {get;set;}
        public string apiKey {get;set;}

        public ReadRequest(){
            names = new List<string>{};
            apiKey = "";
        }
    }

    public class WriteRequest{
        public string name {get;set;}
        public string apiKey {get;set;}
        public object value {get; set;}

        public WriteRequest(){
            name = "";
            apiKey = "";
            value = null;
        }
    }

}