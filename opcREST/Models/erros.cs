using System;
using System.Collections.Generic;

namespace opcRESTconnector
{

/// <summary>
/// Class for returning errors data 
/// </summary>
public class ErrorData {
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public List<string> ErrorCodes {get;set;}
    public ErrorData(){
        Success = false;
        ErrorMessage = "";
        ErrorCodes = new List<string>();
    }
}


}