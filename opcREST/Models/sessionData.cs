using System;
using LiteDB;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        }
        public sessionData(UserData input_user , double expiry_days ){
            user = input_user;
            expiry = DateTime.UtcNow.AddDays(expiry_days);
            last_seen = DateTime.UtcNow;
            write_expiry = DateTime.UtcNow;
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

    }
}