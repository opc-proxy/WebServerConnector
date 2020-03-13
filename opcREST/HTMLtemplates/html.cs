

namespace opcRESTconnector{

    public class HTMLtemplates{
        public static string loginPage(string token, string url, string message, string user=""){
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
                <h1>{message}</h1>
                <form ref='loginForm' 
                  action='{url}' 
                  method='post'>
                    <input type='text' placeholder='username' name='user' value='{user}'>
                    <input type='password' placeholder='password' name='pw'>
                    <input type='submit' value='Submit'>
                    <input type='hidden' value='{token}' name='_csrf'>
                </form>
            </body>
            </html>
            ";
        }

        public static string forbidden(string redirectTo){
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <meta http-equiv='X-UA-Compatible' content='ie=edge'>
                <meta http-equiv='refresh' content='1; url={redirectTo}'>
                <title>Unauthorized</title>
            </head>
            <body>
                <h1>NOT Authorized... </h1> <h3>Redirecting...</h3>
            </body>
            </html>
            ";
        }

    

        public static string writeAccess(string token, string url, string user){
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
                    <h1>Request Write Access for user: {user}</h1>
                    <form ref='loginForm' 
                    action='{url}' 
                    method='post'>
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
