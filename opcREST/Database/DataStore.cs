using System;
using System.Collections.Generic;
using LiteDB;

namespace opcRESTconnector.Data{

    public class DataStore{
        public LiteDatabase db;
        public userCollection users;
        public sessionCollection sessions;

        public RESTconfigs _conf;

        public DataStore(RESTconfigs config){
            
            _conf = config;
            db    = new LiteDatabase(@config.appStoreFileName);

            users = new userCollection(db,config.recoveryMode);
            
            sessions = new sessionCollection(db);
        }       
    }
}