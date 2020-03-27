using LiteDB;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;


namespace opcRESTconnector {

    public enum UsrStatusCodes
    {
        Success,
        ExpiredUsr,
        WrongPW,
        ExpiredPW,
        NotAuthorized,
        PasswordExist,
    }

    /// <summary>
    /// Class that stores user session data
    /// </summary>
    public class UserData{
        [BsonId]
        public string userName {get;set;}
        public Password password {get;set;}
        public AuthRoles role {get;set;}
        public string email {get;set;}
        public string fullName {get;set;}
        public DateTime activity_expiry {get;set;}

        /// <summary>
        /// Constructor for LiteDB
        /// </summary>
        [BsonCtor]
        public UserData(){
            _init();
        }
        /// <summary>
        /// Quick constructor used for session
        /// </summary>
        /// <param name="user_name"></param>
        public UserData(string user_name){
            _init();
            userName = user_name;
        }
        /// <summary>
        /// Creates an Active user with expired password
        /// </summary>
        /// <param name="user_name"></param>
        /// <param name="pwd"></param>
        /// <param name="Role"></param>
        /// <param name="expiry_days"></param>
        public UserData(string user_name, string pwd , AuthRoles Role, double expiry_days ){
            userName = user_name;
            password = new Password(pwd, 0);
            role = Role;
            activity_expiry = DateTime.UtcNow.AddDays(expiry_days);
            email="";
            fullName = "";
        }

        private void _init(){
            userName = "Anonymous";
            password = new Password();
            role = AuthRoles.Undefined;
            activity_expiry = DateTime.UtcNow;
            email ="";
            fullName = "";
        }

        public bool isActive(){
            return (activity_expiry.ToUniversalTime().Ticks > DateTime.UtcNow.Ticks);
        }


        public override string ToString(){
            var out_ = $@"
            User Name : {this.userName}
            Role      : {this.role}
            ";
            return out_;
        }

        public bool isAnonymous(){
            return userName == "Anonymous";
        }

        public static string GeneratePW(){
            byte[] token = new byte[8];
            var rnd = new RNGCryptoServiceProvider();
            rnd.GetBytes(token);
            return  Convert.ToBase64String(token);
        }
    }

    public class Password{
        public string hashedValue {get;set;}
        public DateTime expiry { get ; set ; }
        public List<string> old_passwords {get; set;}
        public DateTime expiryUCT 
        {
            get { return this.expiry.ToUniversalTime(); }
        }
        [BsonCtor]
        public Password(){
            hashedValue = null; 
            expiry = DateTime.UtcNow;
            old_passwords = new List<string>{};
        }
        public Password(string pwd , double duration_hours ){
            hashedValue = hash(pwd); 
            expiry = DateTime.UtcNow.Add(TimeSpan.FromHours(duration_hours)) ;
            old_passwords = new List<string>{};
        }
        
        public bool isActive(){
            return expiryUCT.Ticks > DateTime.UtcNow.Ticks;
        }
        public UsrStatusCodes  update_password(string old_pwd, string new_pwd, double duration_hours){
            if(String.IsNullOrEmpty(old_pwd) || String.IsNullOrEmpty(new_pwd)) return UsrStatusCodes.WrongPW;
            if(String.IsNullOrEmpty(hashedValue)) return UsrStatusCodes.NotAuthorized;
            if(BCrypt.Net.BCrypt.Verify(old_pwd,hashedValue))
            {
                foreach (var pw in old_passwords)
                {
                    if(BCrypt.Net.BCrypt.Verify(new_pwd,pw)) return UsrStatusCodes.PasswordExist;
                }
                if(BCrypt.Net.BCrypt.Verify(new_pwd,hashedValue)) return UsrStatusCodes.PasswordExist;
                old_passwords.Add(hashedValue);
                hashedValue = hash(new_pwd);
                expiry = DateTime.UtcNow.Add(TimeSpan.FromHours(duration_hours)) ;
                return UsrStatusCodes.Success;
            }
            else return UsrStatusCodes.WrongPW;
        }
        public bool isValid(string pwd){
            if(String.IsNullOrEmpty(pwd) || String.IsNullOrEmpty(hashedValue) ) return false;
            return BCrypt.Net.BCrypt.Verify(pwd,hashedValue);
        }
        public string hash(string pwd){
            if(String.IsNullOrEmpty(pwd)) return null;
            else  return BCrypt.Net.BCrypt.HashPassword(pwd);
        }

        
    }

    public class UserForm {
        public string userName {get;set;}
        public string role {get;set;}
        public string email {get;set;}
        public string fullName {get;set;}
        public double duration_days {get;set;}

        public UserForm(){
            userName = email = role = fullName = "";
            duration_days = 0;
        }

        public AuthRoles getRole(){
            switch(role){
                case "admin" :
                    return AuthRoles.Admin;
                case "writer" :
                    return AuthRoles.Writer;
                case "reader" :
                    return AuthRoles.Reader;
                default :
                    return AuthRoles.Undefined;
            }
        }
        public override string ToString(){
            return $@"
            username : {userName}
            role     : {role}
            email    : {email}
            full name: {fullName}
            expiry   : {duration_days}
            ";
        }
    }

    public class UserResponse:httpErrorData {
        public string userName {get;set;}
        public string role {get;set;}
        public string email {get;set;}
        public string fullName {get;set;}
        public string activity_expiry {get;set;}
        public string password_expiry {get;set;}
        public string temporary_pw {get;set;}
        public bool isSend {get;set;}
        public UserResponse(){
            userName = role = email = fullName = activity_expiry = password_expiry = temporary_pw = "";
            IsError = false;
            ErrorMessage = "";
            isSend = false;
        }
    }
    
}