using System;
using System.Collections.Generic;

namespace opcRESTconnector
{

/// <summary>
/// Class for returning errors data 
/// </summary>
public class ErrorData {
    public bool Success { get; set; }
    public string ErrorMessage { get; set;
        /* FIXME
        get {
            string message = "";;
            foreach (var err in ErrorCodes)
            {
                message += err +". ";
            }
            return message;
        }*/
    }
    public List<string> ErrorCodes {get;set;}
    public ErrorData(){
        Success = false;
        ErrorCodes = new List<string>();
    }

}

public class ErrorCodes{
    public const string NotAdmin = "NOT-ADMIN";
    public const string BadPW = "BAD-PW";
    public const string BadData = "BAD-DATA";
    public const string BadEmail = "BAD-EMAIL";
    public const string BadUsrName = "BAD-USER-NAME";
    public const string UsrNotExist = "USER-NOT-EXIST";
    public const string UsrNotActive = "USER-NOT-ACTIVE";
    public const string UsrExist = "USER-EXIST";
    public const string BadName = "BAD-FULL-NAME";
    public const string BadRole = "BAD-ROLE";
    public const string DBerror = "DB-ERROR";

}

}