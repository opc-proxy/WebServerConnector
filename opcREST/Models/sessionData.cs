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
        }
        public sessionData(UserData input_user , double expiry_days ){
            user = input_user;
            expiry = DateTime.UtcNow.AddDays(expiry_days);
            last_seen = DateTime.UtcNow;
        }
    }
}