

namespace opcRESTconnector
{
    
    class opcRESTconnector{
        public RESTconfigs _conf;
        opcRESTconnector(){
            _conf = new RESTconfigs ();
            string url = _conf.https?"https":"http" + "://" + _conf.host + ":" + _conf.port + "/" ;
            if(_conf.urlPrefix != "") url = url + _conf.urlPrefix;

        }
    }
}