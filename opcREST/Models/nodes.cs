using System.Collections.Generic;
using System.Threading.Tasks;
using Swan;
using Swan.Cryptography;
using System;
using OpcProxyCore;
using Opc.Ua;

namespace opcRESTconnector  
{
    public class NodeBasicResp : ErrorData
    {
        public string Name {get;set;}
        public object Value {get;set;}

        public NodeBasicResp():base(){
            Name = "none";
            Value = null;
        }
    }

    public class ReadResponse : NodeBasicResp{
        public string Type {get;set;}
        public string Timestamp {get;set;}
        public double Timestamp_ms {get;set;}
        public ReadResponse() :base(){
            ErrorCode = "";
            Success = true;
            Type = "";
            Timestamp = DateTime.UtcNow.ToString();
            Timestamp_ms = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
        public ReadResponse( ReadVarResponse r)
        {
            if(r == null) return;
            Name = Utils.HTMLescape(r.name);
            Success = r.success;
            ErrorCode = buildErrorcode(r.statusCode);

            if(r.success) 
            {
                Value = (r.value?.GetType() ==  typeof(String)) ? Utils.HTMLescape((string)r?.value) : r.value;
                Type  = (r.systemType != "") ? r.systemType?.Substring(7).ToLower() : "";
                Timestamp_ms = r.timestampUTC_ms;
                Timestamp = r.timestamp.ToUniversalTime().ToString();
                ErrorCode = buildErrorcode(r.statusCode);
            }
            else 
            {
                Value = null;
                Type = "";
                Timestamp_ms = 0;
                Timestamp = "none";
            }
        }

        public static string buildErrorcode( uint code )
        {
            if(StatusCode.IsGood(code)) return "";

            switch(code) {
                case (StatusCodes.BadNoEntryExists) :
                    return ErrorCodes.VarNotExist;
                case (StatusCodes.BadTypeMismatch) :
                    return ErrorCodes.BadValue;
                default :
                    return ErrorCodes.Uknown;
            }
        }
    }

    public class WriteResponse : NodeBasicResp{
        public WriteResponse(){
            ErrorCode = "";
            Success = false;
        }
        public WriteResponse( WriteVarResponse r)
        {
            if(r == null) return;
            Name = Utils.HTMLescape(r.name);
            Success = r.success;
            ErrorCode = ReadResponse.buildErrorcode(r.statusCode);

            if(r.success) 
            {
                Value = (r.value?.GetType() ==  typeof(String)) ? Utils.HTMLescape((string)r.value) : r.value;
            }
            else Value = null;
        }
    }

    public class ReadRequest{
        public List<string> names {get;set;}
        public string apiKey {get;set;}

        public ReadRequest(){
            names = new List<string>{};
            apiKey = "";
        }
    }

    public class WriteRequest{
        public List<string> names {get;set;}
        public string apiKey {get;set;}
        public List<object> values {get; set;}

        public WriteRequest(){
            names = new List<string>();
            apiKey = "";
            values = new List<object>();
        }
    }

}