namespace opcRESTconnector
{
    public class reCATPCHAresp{
        public bool success {get; set;}
        public string challenge_ts {get; set;}
        public string hostname {get; set;}
        // error codes name problem

        public reCATPCHAresp(){
            success = false;
            challenge_ts = "";
            hostname = "";
        }
    }
}