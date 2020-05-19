using System;
using LiteDB;
using System.Collections.Generic;

namespace opcRESTconnector
{
    public class sessionData{
        [BsonId]
        public ObjectId Id {get; set;}
        
        [BsonRef("users")]
        public UserData user {get;set;}
        public DateTime expiry {get;set;}
        public DateTime last_seen {get;set;}
        public DateTime write_expiry {get; set;}
        public string user_agent {get;set;}
        public string ip {get;set;}
        public string csrf_token {get;set;}

        public DateTime expiryUTC{
            get {
                return this.expiry.ToUniversalTime();
            }
        }

        [BsonCtor]
        public sessionData(){
            user = new UserData("Anonymous");
            expiry = DateTime.UtcNow;
            last_seen = DateTime.UtcNow;
            write_expiry = DateTime.UtcNow;
            user_agent = "";
            ip = "";
            csrf_token = "";
        }
        public sessionData(UserData input_user , double expiry_days, string agent, string _ip ){
            user = input_user;
            expiry = DateTime.UtcNow.AddDays(expiry_days);
            last_seen = DateTime.UtcNow;
            write_expiry = DateTime.UtcNow;
            ip = _ip;
            user_agent = agent;
            csrf_token = "";
        }

        public UsrStatusCodes AllowWrite(string pw, double duration_minutes){
            if(!user.isActive()) return UsrStatusCodes.ExpiredUsr;
            if(!user.password.isActive()) return UsrStatusCodes.ExpiredPW;
            if(!user.password.isValid(pw)) return UsrStatusCodes.WrongPW;

            if(user.role == AuthRoles.Writer || user.role == AuthRoles.Admin){
                write_expiry = DateTime.UtcNow.AddMinutes(duration_minutes);
                return UsrStatusCodes.Success;
            } 
            return UsrStatusCodes.NotAuthorized;
        }
        public bool hasWriteRights(){
            if(!user.isActive()) return false;
            if(!user.password.isActive()) return false;
            return (write_expiry.ToUniversalTime().Ticks  > DateTime.UtcNow.Ticks);
        }

        public bool hasReadRights(){
            if(!user.isActive()) return false;
            if(!user.password.isActive()) return false;
            return user.role != AuthRoles.Undefined;
        }

        public bool isValidToken(string token)
        {
            if(!hasReadRights()) return false;
            return (csrf_token == token) ;
        }
    }

    public class SessionResponse {
        public string userName {get;set;}
        public string expiry {get;set;}
        public string last_seen {get;set;}
        public string user_agent {get;set;}
        public string ip {get;set;}
        
        public SessionResponse(sessionData s){
            userName = Utils.HTMLescape(s.user.userName);
            expiry = UserResponse.niceDate(s.expiryUTC);
            last_seen = niceDateNow(s.last_seen);
            user_agent = Utils.HTMLescape(s.user_agent);
            ip = Utils.HTMLescape(s.ip);
        }
        public static string niceDateNow(DateTime d){
            if( ( DateTime.UtcNow - d.ToUniversalTime()).CompareTo(TimeSpan.FromMinutes(5)) <= 0 ) return "Now";
            string t = d.ToUniversalTime().ToLongDateString();
            var s = new List<string>(t.Split(','));
            s.RemoveAt(0);
            s.Add(d.ToUniversalTime().ToShortTimeString());
            return string.Join(" ",s);
        }
    }
    public class SessionGetResponse : ErrorData{
        public List<SessionResponse> sessions {get;set;}
        public SessionGetResponse(List<SessionResponse> resp =null){
            sessions = resp ?? new List<SessionResponse>();
            Success = true;
        }
    }
}