

namespace opcRESTconnector{

    public class HTMLtemplates{
        public static string loginPage(string token, string message, string user=""){
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <meta http-equiv='X-UA-Compatible' content='ie=edge'>
                <link rel='icon' href='https://avatars0.githubusercontent.com/u/52571081?s=200&v=4'>
                <title>login</title>
                {CSStemplates.normalize}
            </head>
            <body>
                <h1>{message}</h1>
                <form ref='loginForm' 
                  action='{Routes.login}' 
                  method='post'>
                    <input type='text' placeholder='username' name='user' value='{user}'>
                    <input type='password' placeholder='password' name='pw'>
                    <input type='submit' value='Submit'>
                    <input type='hidden' value='{token}' name='_csrf'>
                </form>
                <a href='/admin/write_access'> write access </a>
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
                <link rel='icon' href='https://avatars0.githubusercontent.com/u/52571081?s=200&v=4'>
                <title>Unauthorized</title>
            </head>
            <body>
                <h1>NOT Authorized... </h1> <h3>Redirecting...</h3>
            </body>
            </html>
            ";
        }

    

        public static string writeAccess(string token, string user, string referrer="", string error =""){
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <meta http-equiv='X-UA-Compatible' content='ie=edge'>
                    <link rel='icon' href='https://avatars0.githubusercontent.com/u/52571081?s=200&v=4'>
                    <title>login</title>
                    {CSStemplates.normalize}
                </head>
                <body>
                    <h1>Request Write Access for user: {user}</h1>
                    <h3>{error}</h3>
                    <form ref='loginForm' 
                    action='{Routes.write_access}' 
                    method='post'>
                        <input type='password' placeholder='password' name='pw'>
                        <input type='submit' value='Submit'>
                        <input type='hidden' value='{token}' name='_csrf'>
                        <input type='hidden' value='{referrer}' name='_referrer'>
                    </form>
                </body>
                </html>
                ";
        }
    }

}
