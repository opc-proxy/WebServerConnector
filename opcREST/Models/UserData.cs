using LiteDB;
using System;

namespace opcRESTconnector {

    /// <summary>
    /// Class that stores user session data
    /// </summary>
    public class UserData{
        [BsonId]
        public string userName {get;set;}
        public Password password {get;set;}
        public AuthRoles role {get;set;}
        public DateTime activity_expiry {get;set;}
        public DateTime write_expiry {get; set;}

        /// <summary>
        /// Constructor for LiteDB
        /// </summary>
        [BsonCtor]
        public UserData(){
            userName = "Anonymous";
            password = new Password();
            role = AuthRoles.Undefined;
            write_expiry = DateTime.UtcNow;
            activity_expiry = DateTime.UtcNow;
        }
        /// <summary>
        /// Quick constructor used for session
        /// </summary>
        /// <param name="user_name"></param>
        public UserData(string user_name){
            userName = user_name;
            password = new Password();
            role = AuthRoles.Undefined;
            write_expiry = DateTime.UtcNow;
            activity_expiry = DateTime.UtcNow;
        }
        /// <summary>
        /// Creates an Active user with expired password
        /// </summary>
        /// <param name="user_name"></param>
        /// <param name="pwd"></param>
        /// <param name="Role"></param>
        /// <param name="expiry_days"></param>
        public UserData(string user_name , string pwd , AuthRoles Role, double expiry_days ){
            userName = user_name;
            password = new Password(pwd, 0);
            role = Role;
            write_expiry = DateTime.UtcNow;
            activity_expiry = DateTime.UtcNow.AddDays(expiry_days);
        }

        public bool isActive(){
            if(activity_expiry.ToUniversalTime().Ticks < DateTime.UtcNow.Ticks) return false;
            if(!password.IsActive()) return false;
            return true;
        }

        public bool AllowWrite(TimeSpan duration){
            if(!isActive()) return false;

            if(role == AuthRoles.Writer || role == AuthRoles.Admin){
                write_expiry = DateTime.UtcNow.Add(duration);
                return true;
            } 
            return false;
        }
        public bool hasWriteRights(){
            if(!isActive()) return false;
            return (write_expiry.ToUniversalTime().Ticks  > DateTime.UtcNow.Ticks);
        }

        public bool hasReadRights(){
            if(!isActive()) return false;
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
        public string hashedValue {get;set;}
        public DateTime expiry { get ; set ; }
        public DateTime expiryUCT 
        {
            get { return this.expiry.ToUniversalTime(); }
        }
        [BsonCtor]
        public Password(){
            hashedValue = null; 
            expiry = DateTime.UtcNow;
        }
        public Password(string pwd , double duration_hours ){
            hashedValue = hash(pwd); 
            expiry = DateTime.UtcNow.Add(TimeSpan.FromHours(duration_hours)) ;
        }
        
        public bool IsActive(){
            return expiryUCT.Ticks > DateTime.UtcNow.Ticks;
        }
        public string  update_password(string old_pwd, string new_pwd, double duration_hours){
            if(String.IsNullOrEmpty(old_pwd) || String.IsNullOrEmpty(new_pwd)) return null;
            if(String.IsNullOrEmpty(hashedValue)) return null;
            if(BCrypt.Net.BCrypt.Verify(old_pwd,hashedValue))
            {
                hashedValue = hash(new_pwd);
                expiry = DateTime.UtcNow.Add(TimeSpan.FromHours(duration_hours)) ;
                return hashedValue;
            }
            else return null;
        }
        public bool isValid(string pwd){
            if(String.IsNullOrEmpty(pwd) || String.IsNullOrEmpty(hashedValue) ) return false;
            if(expiryUCT.Ticks < DateTime.UtcNow.Ticks) return false;
            return BCrypt.Net.BCrypt.Verify(pwd,hashedValue);
        }
        public string hash(string pwd){
            if(String.IsNullOrEmpty(pwd)) return null;
            else  return BCrypt.Net.BCrypt.HashPassword(pwd);
        }

        
    }
}