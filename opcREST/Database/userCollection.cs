using LiteDB;
using System;

namespace opcRESTconnector.Data
{
    public class userCollection : IDBcollection<UserData>
    {
        public userCollection(LiteDatabase db, bool recoveryMode) : base("users",db) {
            
            ensureAdminUserExist(recoveryMode);
        }
        
        private void ensureAdminUserExist(bool recoveryMode){
            var admin = _collection.FindOne(x => x.userName == "admin");
            // create Admin user if not exist
            if(admin == null) {
                admin = new UserData("admin","admin",AuthRoles.Admin, 1 );
                admin.activity_expiry =  DateTime.MaxValue; // admin is always active, but password expires
                _collection.Insert(admin);
            }
            // Reset password if in recovery mode
            else if(recoveryMode){
                admin.password = new Password("admin",0);
                _collection.Update(admin);
            }
        }

        public override void ensureIndex(){ /* username is PK, nothing else to index here */ }
        public override bool Delete(UserData data)
        {
            return _collection.Delete(data.userName);
        }

        public override UserData Get(string user_name)
        {
            UserData user = _collection.FindOne(x => x.userName == user_name); 
            return user ?? new UserData();
        }

        public override BsonValue Insert(UserData data)
        {
            try{
                var o = _collection.Insert(data);
                return o;
            }
            catch{
                return BsonValue.Null;
            }
        }

        public override void Upsert(UserData data)
        {
            _collection.Upsert(data);
        }
        public override bool Update(UserData data){
            return _collection.Update(data);
        }
        
    }
}