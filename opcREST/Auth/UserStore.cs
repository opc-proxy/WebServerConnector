using System;
using System.Collections.Generic;

namespace opcRESTconnector.Session{

    public class UserStore{
        public Dictionary<string, UserData> users;
        public UserStore(RESTconfigs config){
            users = new Dictionary<string, UserData>();

            // filling the dictionary with defined roles and users
            foreach(var user_item in config.userAuth)
            {
                var Role = AuthRoles.Undefined;
                if(user_item.Count == 3){
                    switch (user_item[2].ToLower()){
                        case "r" :
                            Role = AuthRoles.Reader;
                            break;
                        case "w":
                            Role = AuthRoles.Writer;
                            break;
                        case "a": 
                            Role = AuthRoles.Admin;
                            break;
                        default:
                            Role = AuthRoles.Undefined;
                            break;
                    }

                    var temp_user = new UserData(){ 
                        userName = user_item[0],
                        password = new Password(user_item[1],config.sessionExpiryHours),
                        role = Role
                    };
                    users.Add(user_item[0], temp_user);
                }
                else{
                    throw new Exception("Malformed user item: " + user_item[0]);
                }
            }
        }
        public UserData GetUser(string user_name){
            UserData user;
            if(users.TryGetValue(user_name,out user)){
                return user;
            }
            else return new UserData();
        }
    }
}