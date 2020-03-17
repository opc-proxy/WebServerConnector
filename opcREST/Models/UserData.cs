using System;

namespace opcRESTconnector.Session {

    /// <summary>
    /// Class that stores user session data
    /// </summary>
    public class UserData{
        public string userName {get;set;}
        public Password password {get;set;}
        public AuthRoles role {get;set;}
        private DateTime write_expiry {get; set;}

        public UserData(string user_name = "Anonymous"){
            userName = user_name;
            password = new Password();
            role = AuthRoles.Undefined;
            write_expiry = DateTime.UtcNow;
        }

        public bool AllowWrite(TimeSpan duration){
            if(role == AuthRoles.Writer || role == AuthRoles.Admin){
                write_expiry = DateTime.UtcNow.Add(duration);
                return true;
            } 
            return false;
        }
        public bool hasWriteRights(){
            if(write_expiry.Ticks  > DateTime.UtcNow.Ticks) return true;
            return false;
        }

        public bool hasReadRights(){
            return role != AuthRoles.Undefined;
        }

        public override string ToString(){
            var out_ = $@"
            User Name : {this.userName}
            Role      : {this.role}
            Write Permission : ";
            out_ += hasWriteRights() ? "Yes" : "No";
            return out_;
        }
    }

    public class Password{
        public string admin_default {get; set;}
        public string currentHash {get;set;}
        public DateTime expiry;
        private bool _pwd_is_updated;
        public string update_password(string pwd){
            _pwd_is_updated = true;
            return pwd;
        }
        public bool isValid(string pwd){
            if(admin_default == "") return false;
            if(expiry.Ticks < DateTime.UtcNow.Ticks) return false;
            if(_pwd_is_updated) return hash(pwd) == currentHash ;
            return pwd == admin_default;
        }
        public string hash(string pwd){
            // fixme apply hashing
            return pwd;
        }
        public Password(string pwd_default = "", double duration_hours = 0){
            admin_default = pwd_default;
            currentHash = "";
            expiry = DateTime.UtcNow.Add(TimeSpan.FromHours(duration_hours)) ;
            _pwd_is_updated = false;
        }
    }
}