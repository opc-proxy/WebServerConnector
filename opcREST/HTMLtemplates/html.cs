
using System;
using System.Linq;

namespace opcRESTconnector{

    public class HTMLtemplates{
        public static string loginPage(string token, string message, string user="", string recaptchaClientKey =""){
            Utils.HTTPescape(ref message);
            Utils.HTTPescape(ref user);
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <meta http-equiv='X-UA-Compatible' content='ie=edge'>
                <link rel='icon' href='https://avatars0.githubusercontent.com/u/52571081?s=200&v=4'>
                {(
                    recaptchaClientKey != "" ?
                    "<script src='https://www.google.com/recaptcha/api.js' async defer></script>" :
                    ""
                )}
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
                    <div class='g-recaptcha' data-sitekey='{recaptchaClientKey}'></div>
                    <input type='submit' value='Submit'>
                    <input type='hidden' value='{token}' name='_csrf'>
                </form>
                <a href='/admin/write_access'> write access </a>
            </body>
            </html>
            ";
        }

        public static string forbidden(string redirectTo){
            Utils.HTTPescape(ref redirectTo);
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <meta http-equiv='X-UA-Compatible' content='ie=edge'>
                <meta http-equiv='refresh' content='1; url={redirectTo}'>
                <link rel='icon' href='https://avatars0.githubusercontent.com/u/52571081?s=200&v=4'>
                {CSStemplates.normalize}
                <title>Unauthorized</title>
            </head>
            <body>
                <h1>NOT Authorized... </h1> <h3>Redirecting...</h3>
            </body>
            </html>
            ";
        }

    
        public static string updatePW(string token, string user, string error = ""){
            Utils.HTTPescape(ref user);
            Utils.HTTPescape(ref error);
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
                    <h1>Password update required for user: {user}</h1>
                    <h3>{error}</h3>
                    <form ref='loginForm' 
                    action='{Routes.update_pw}' 
                    method='post'>
                        <label> Current password:</label>
                        <input type='password' placeholder='old password' name='old_pw'>
                        <label> New password:</label>
                        <input type='password' placeholder='new password' name='new_pw'>
                        <input type='submit' value='Submit'>
                        <input type='hidden' value='{token}' name='_csrf'>
                    </form>
                </body>
                </html>
                ";
        }

        public static string writeAccess(string token, string user, string referrer="", string error =""){
            Utils.HTTPescape(ref user);
            Utils.HTTPescape(ref referrer);
            Utils.HTTPescape(ref error);
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


/*      // test of syntax
  public static string test(bool b){
            return $@"
                { (b ? 
                    "ciao" 
                  : "bau"
                )}
                {
                    string.Join("\n", 
                        Enumerable.Range(1, 4).Select(num => "<div> " + num + " </div>").ToArray()
                    )
                }
            ";
        }*/

    }

}
