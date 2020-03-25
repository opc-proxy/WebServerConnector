namespace opcRESTconnector
{
    

/// <summary>
/// Class for returning errors data similar to API
/// </summary>
public class httpErrorData {
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public httpErrorData(){
        IsError = true;
        ErrorMessage = "";
    }
}


}