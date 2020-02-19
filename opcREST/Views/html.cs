

namespace opcRESTconnector{

    public class HTMLtemplates{
        public static string loginPage(string token, string url){
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <meta http-equiv='X-UA-Compatible' content='ie=edge'>
                <title>login</title>
            </head>
            <body>
                <form ref='loginForm' 
                  action='{url}' 
                  method='post'>
                    <input type='text' placeholder='username' name='user'>
                    <input type='password' placeholder='password' name='pw'>
                    <input type='submit' value='Submit'>
                    <input type='hidden' value='{token}' name='_csrf'>
                </form>
            </body>
            </html>
            ";
        }

    }

}