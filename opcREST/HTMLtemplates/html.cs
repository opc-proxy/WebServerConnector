
using System;
using System.Linq;

namespace opcRESTconnector{

    public class HTMLtemplates{
        public static string loginPage(string token, string message, string user="", string recaptchaClientKey =""){
            Utils.HTMLescape(ref message);
            Utils.HTMLescape(ref user);
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <meta http-equiv='X-UA-Compatible' content='ie=edge'>
                <link rel='icon' href='https://avatars0.githubusercontent.com/u/52571081?s=200&v=4'>
                <style>{CSStemplates.layout}</style>
                {(
                    recaptchaClientKey != "" ?
                    "<script src='https://www.google.com/recaptcha/api.js' async defer></script>" :
                    ""
                )}
                <title>login</title>
                {CSStemplates.normalize}
            </head>
            <body>
                <div class='container'>
                    <div class='filler'> </div>
                    <h3 style='align-self:center;'>OPC-Proxy <strong> <a>Login</a></strong></h3>
                    <div class='box'>
                        <form class='flexcolumn' ref='loginForm' 
                        action='{Routes.login}' 
                        method='post'>
                        <h6 style='color:rebeccapurple;'>{message}</h6>
                            <label>Username</label>
                            <input type='text' placeholder='username' name='user' value='{user}'>
                            <label>Password</label>
                            <input type='password' placeholder='password' name='pw'>
                            <div class='g-recaptcha' data-sitekey='{recaptchaClientKey}' style='width:304px;height:78px;'></div>
                            <input type='hidden' value='{token}' name='_csrf'>
                            <input type='submit' value='Submit' style='align-self: flex-end; margin-top:3rem;'>
                        </form>
                    </div>
                <div class='filler'> </div>
            </div>
            </body>
            </html>
            ";
        }

        public static string forbidden(string redirectTo){
            Utils.HTMLescape(ref redirectTo);
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
                <style>{CSStemplates.layout}</style>
                <title>Unauthorized</title>
            </head>
            <body >
                <div class='flexcolumn' style='margin-top:4rem;'>
                <h1 class='selfcenter'>NOT Authorized... </h1> <h3 class='selfcenter'>Redirecting...</h3>
                </div>
            </body>
            </html>
            ";
        }

    
        public static string updatePW(string token, string user, string error = ""){
            Utils.HTMLescape(ref user);
            Utils.HTMLescape(ref error);
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <meta http-equiv='X-UA-Compatible' content='ie=edge'>
                    <link rel='icon' href='https://avatars0.githubusercontent.com/u/52571081?s=200&v=4'>
                    <title>login</title>
                    <style>{CSStemplates.layout}</style>
                    {CSStemplates.normalize}
                </head>
                <body>
                <div class='container'>
                    <div class='filler'> </div>
                    <h3 style='align-self:center;'>Password update required for user: <strong style='margin-left:1rem;'><a>{user}</strong></a></h3>
                    <div class='box'>
                    <form class='flexcolumn' ref='loginForm' 
                    action='{Routes.update_pw}' 
                    method='post'>
                        <h6 style='color:rebeccapurple'>{error}</h6>
                        <label> Current password:</label>
                        <input type='password' placeholder='old password' name='old_pw'>
                        <label> New password:</label>
                        <input type='password' placeholder='new password' name='new_pw'>
                        <input type='submit' value='Submit' style='align-self: flex-end; margin-top:2rem;'>
                        <input type='hidden' value='{token}' name='_csrf'>
                    </form>
                    </div>
                    <div class='filler'> </div>
                </div>
                </body>
                </html>
                ";
        }

        public static string writeAccess(string token, string user, string referrer="", string error =""){
            Utils.HTMLescape(ref user);
            Utils.HTMLescape(ref referrer);
            Utils.HTMLescape(ref error);
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
                    <style>{CSStemplates.layout}</style>
                </head>
                <body>
                <div class='container'>
                    <div class='filler'> </div>
                    <h3 style='align-self:center;'>Request Write Access for user: <strong style='margin-left:1rem;'><a>{user}</strong></a></h3>
                    <div class='box'>
                    <form class='flexcolumn' ref='loginForm' 
                    action='{Routes.write_access}' 
                    method='post'>
                        <h6 style='color:rebeccapurple'>{error}</h6>
                        <label>Password</label> 
                        <input type='password' placeholder='password' name='pw'>
                        <input type='submit' value='Submit' style='align-self: flex-end; margin-top:2rem;'>
                        <input type='hidden' value='{token}' name='_csrf'>
                        <input type='hidden' value='{referrer}' name='_referrer'>
                    </form>
                    </div>
                    <div class='filler'> </div>
                </div>
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
