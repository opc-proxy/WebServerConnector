using System;
using LiteDB;

namespace opcRESTconnector.Data
{
    public abstract class IDBcollection<T>{
        public ILiteCollection<T> _collection;
        public IDBcollection(string collectionName,LiteDatabase db){
            _collection = db.GetCollection<T>(collectionName);

            ensureIndex();
        }

        public abstract void  ensureIndex() ;
        public abstract BsonValue Insert(T data) ;
        public abstract void  Upsert(T data) ;
        public abstract bool  Update(T data) ;
        public abstract bool  Delete(T data) ;
        public abstract T Get(string index);

    }
}