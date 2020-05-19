
namespace opcRESTconnector
{

/// <summary>
/// Class for returning errors data 
/// </summary>
public class ErrorData {
    public bool Success { get; set; }
    public string ErrorCode {get;set;}
    public ErrorData(){
        Success = false;
        ErrorCode = "";
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
    public const string Unauthorized = "NOT-AUTHORIZED";
    public const string BadRequest = "BAD-REQUEST";
    public const string Forbidden = "FORBIDDEN";
    public const string  NotFound = "NOT-FOUND";
    public const string  NotAllowed = "NOT-ALLOWED";
    public const string  ServerError = "SERVER-ERROR";
    public const string  Uknown = "UKNOWN";
    public const string VarNotExist  = "VAR-NOT-EXIST";
    public const string  BadValue = "BAD-VALUE";

}

}