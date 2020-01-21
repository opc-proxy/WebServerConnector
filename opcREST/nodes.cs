using System.Collections.Generic;
using System.Threading.Tasks;
using Swan;
using Swan.Cryptography;

namespace opcRESTconnector  
{
    public class NodeValue
    {
        public string Name {get;set;}
        public string Type {get;set;}
        public string Value {get;set;}
        public string Timestamp {get;set;}

        public NodeValue(){
            Name = "none";
            Type = "none";
            Value = "none";
            Timestamp = "none";
        }
    }

    public class ReadResponse{
        public List<NodeValue> Nodes {get;set;}
        public string ErrorMessage {get;set;}
        public bool IsError {get;set;}

        public ReadResponse(){
            Nodes = new List<NodeValue>{};
            ErrorMessage = "none";
            IsError = false;
        }
    }

    public class WriteResponse{
        public string ErrorMessage {get;set;}
        public bool IsError {get;set;}
        public WriteResponse(){
            ErrorMessage = "none";
            IsError = false;
        }
    }

}