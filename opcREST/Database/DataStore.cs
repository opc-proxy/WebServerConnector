using System;
using System.Collections.Generic;
using LiteDB;

namespace opcRESTconnector.Data{

    public class DataStore : IDisposable{
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

        public void Dispose()
        {
            // Needed because sessions cookies are secret-key dependent,
            // secret-key is generated randomly at startup, so at each restart
            // all sessions are invalid.
            // In case implement secret-key from env-variable this can be changed.
            db?.DropCollection("sessions");
            db?.Rebuild();
            db?.Dispose();
        }
    }
}