using System.Collections.Generic;
using LiteDB;
using Newtonsoft.Json;
using System;

namespace opcRESTconnector.Data
{
    public class sessionCollection : IDBcollection<sessionData>{
        public sessionCollection(LiteDatabase db) : base("sessions",db){ }
        public override void ensureIndex()
        {
            _collection.EnsureIndex("user");
            _collection.EnsureIndex("expiry");
        }

        public override bool Delete(sessionData data)
        {
            return _collection.Delete(data.Id);
        }
        public bool DeletFromId(string id){
            if(String.IsNullOrEmpty(id)) return false;
            var obi = new ObjectId(id);
            return _collection.Delete(obi);
        }
        public override sessionData Get(string id)
        {
            if(String.IsNullOrEmpty(id)) return null;
            ObjectId oid = new ObjectId(id);
            return Get(oid);
        }

        public sessionData Get(ObjectId oid)
        {
            return _collection.Include( x => x.user).FindById(oid);
        }

        public override BsonValue Insert(sessionData data)
        {
            if( String.IsNullOrEmpty(data?.user?.userName)) return null;
            try{
                var b = _collection.Insert(data);
                return data.Id.ToString();
            }
            catch {
                return BsonValue.Null;
            }
        }
        /// <summary>
        /// Update values of the session, User cannot be updated.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Update(sessionData data)
        {
            if(data?.Id == null || data?.user?.userName == null) return false;
            var local_session = Get(data.Id);
            // User Constraint: user cannot be updated
            if(local_session.user.userName != data.user.userName) return false;
            return _collection.Update(data);
        }
        /// <summary>
        /// Get the session and if exists it updates last seen
        /// </summary>
        /// <param name="s_Id"></param>
        /// <returns></returns>
        public sessionData GetAndUpdateLastSeen(string s_Id){
            var s = Get(s_Id);
            if(s == null) return null;
            s.last_seen = DateTime.UtcNow;
            if(_collection.Update(s)) return s;
            else return null;
        }

        public override void Upsert(sessionData data)
        {
            _collection.Upsert(data);
        }

        public void PurgeExpired(){
            _collection.DeleteMany( x => (x.expiry.Ticks < DateTime.UtcNow.Ticks));
        }
    }
}