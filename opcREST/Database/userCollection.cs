using LiteDB;
using System;
using System.Collections.Generic;

namespace opcRESTconnector.Data
{
    public class userCollection : IDBcollection<UserData>
    {
        public userCollection(LiteDatabase db, bool recoveryMode) : base("users",db) {
            
            ensureAdminUserExist(recoveryMode);
        }
        
        private void ensureAdminUserExist(bool recoveryMode){
            // create Admin user if Collection is empty
            if(_collection.Count() == 0) {
                _collection.Insert(createNewAdminUser());
            }
            // Reset password if in recovery mode
            if(recoveryMode){
                var admin = _collection.FindOne(x => x.userName == "admin");
                if(admin == null) admin = createNewAdminUser();
                admin.password = new Password("admin",0);  // case forgot PW, ahi ahi!
                admin.activity_expiry = DateTime.MaxValue; // case deactivated by mistake
                _collection.Upsert(admin);
            }
        }

        public UserData createNewAdminUser(){
            var admin = new UserData("admin","admin",AuthRoles.Admin, 1 );
            admin.activity_expiry =  DateTime.MaxValue; // admin is always active, but password expires
            return admin;
        }

        public override void ensureIndex(){ /* username is PK, nothing else to index here */ }
        public override bool Delete(UserData data)
        {
            // LiteDB should be thread safe, but somehow in a test I did seems not, thread just fail misteriously
            // this only happens with ~10000 thread per second...
            // So here I manually lock, maybe there is something I'm missing... FIXME.
            lock(_db){
                return _collection.Delete(data.userName);
            }
        }

        public override UserData Get(string user_name)
        {
            lock(_db){
                UserData user = _collection.FindOne(x => x.userName == user_name); 
                return user ?? new UserData();
            }
        }

        public IEnumerable<UserData> GetAll(){
            lock(_db){
                return _collection.FindAll();
            }
        }

        public override BsonValue Insert(UserData data)
        {
            lock(_db){
                try{
                    var o = _collection.Insert(data);
                    return o;
                }
                catch{
                    return BsonValue.Null;
                }
            }
        }

        public override void Upsert(UserData data)
        {
            lock(_db){
                _collection.Upsert(data);
            }
        }
        public override bool Update(UserData data){
            lock(_db){
                return _collection.Update(data);
            }
        }
        
    }
}