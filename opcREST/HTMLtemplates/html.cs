
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

        const string admin_suers = @"
        <!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""ie=edge"">
    <script type=""text/javascript"" src=""/_js/htmlescape.js""></script>
    <script type=""text/javascript"" src=""/utils.js"" ></script>
    <!--<script type=""text/javascript"" src=""/_js/admin_utils.js"" ></script>-->
    <link rel=""stylesheet"" type=""text/css"" href=""/_css/milligram.css"">
    <link rel=""stylesheet"" type=""text/css"" href=""/_css/layout.css"">
    <title>Test</title>
</head>
<body style=""margin:0"">
    <nav class=""navbar""> 
        <strong style=""color: yellow;""> OPC-Proxy Admin-Pages </strong> 
        <strong style=""flex:1; display: flex; justify-content: center; align-items: center;""> 
            <a style=""color: #ffffff;"" href=""bla"">User-Managment</a> 
            <div style=""flex: 0.1;""></div>
            <a style=""color: #ffffff"" href=""bla"">Nodes</a> 
            <div style=""flex: 0.1;""></div>
            <a style=""color: #ffffff;"" href=""bla"">IP-Banning</a> 
            <div style=""flex: 0.1;""></div>
            <a style=""color: #ffffff;"" href=""bla"">Logs</a> 
        </strong> 
        <div > Hello user!</div> 
    </nav>
    <nav id=""error-bar"" class=""errordisplay""> 
        <strong class=""selfcenter"" style=""margin-right: 1rem;""><a>Error:</a> </strong><strong class=""selfcenter"" id=""error-title"" ></strong>
    </nav>
    <div >
    
    <div class=""view-container"">
        <div class=""flexcolumn"">

    <div class=""user-table"" >
            <table style=""width: 100%;"" >
                <thead class=""tableheader"">
                    <tr>
                        <th class=""tableheader"">User Name</th>
                        <th class=""tableheader"">Full Name</th>
                        <th class=""tableheader"">Role</th>
                        <th class=""tableheader"">Status</th>
                    </tr>
                </thead>
                <tbody id=""usr_table""></tbody>
            </table>
        </div>
        <button id=""create-usr-btn"" > + Add user</button>
        </div>
        
        <div class=""flexcolumn details"" id=""details"">
        </div>
    </div>
</div>
<div class=""nofication-bkg""  id=""notification-bkg"" style=""background-color:rgba(0, 0, 0, 0.7);"">
    <div class=""box notification"" id=""box-notification"">
    </div>
    <button id=""cancel-btn"" style=""width: fit-content; margin-top: 1rem;"">Cancel</button>
</div>
    <script>
            var usr_table = document.querySelector(""#usr_table"");
            updateUsrTable(usr_table);
            
            var add_usr_btn = document.querySelector(""#create-usr-btn"");
            add_usr_btn.onclick = addUser;

            var notify_box = document.querySelector(""#notification-bkg"");

            var cancel_btn = document.querySelector(""#cancel-btn"");
            cancel_btn.onclick = ()=>{ 
                notify_box.removeAttribute(""show"");
                updateUsrTable(usr_table);
                doSelection(""__none__"", usr_table);
                let details = document.querySelector(""#details"");
                details.innerHTML = '';
            }

    </script>
</body>
</html>
        ";


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
