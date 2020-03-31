using System;

namespace opcRESTconnector
{
    public class jsTEmplates{
    public const string htmlescape =@"
            /*!
 * escape-html
 * Copyright(c) 2012-2013 TJ Holowaychuk
 * Copyright(c) 2015 Andreas Lubbe
 * Copyright(c) 2015 Tiancheng 'Timothy' Gu
 * MIT Licensed
 */

'use strict'

/**
 * Module variables.
 * @private
 */

var matchHtmlRegExp = /[""'&<>]/


function escapeHtml (string) {
  var str = '' + string
  var match = matchHtmlRegExp.exec(str)

  if (!match) {
    return str
  }

  var escape
  var html = ''
  var index = 0
  var lastIndex = 0

  for (index = match.index; index < str.length; index++) {
    switch (str.charCodeAt(index)) {
      case 34: // ""
        escape = '&quot;'
        break
      case 38: // &
        escape = '&amp;'
        break
      case 39: // '
        escape = '&#39;'
        break
      case 60: // <
        escape = '&lt;'
        break
      case 62: // >
        escape = '&gt;'
        break
      default:
        continue
    }

    if (lastIndex !== index) {
      html += str.substring(lastIndex, index)
    }

    lastIndex = index + 1
    html += escape
  }

  return lastIndex !== index
    ? html + str.substring(lastIndex, index)
    : html
}
            ";


    public const string admin_utils=@"
        var users = [];
var selected_usr = ""__none__"";


async function postDataJson( url, data ) {

    const response = await fetch(url, {
        method: 'POST', 
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data) 
    });
    var resp = await response.json();
    if(response.ok) 
        return resp;
    else {
        if(resp) return resp;
        else return { Success:false, ErrorMessage:""HTTP Error! Status code: "" + response.status };
    }
}

async function getUsers(){
    const resp = await fetch('/admin/users/read');
    if(resp.ok) return await resp.json();
    else showError(""Unauthorized Access"");
}

async function getUserSessions(username){
    const resp = await fetch('/admin/users/' + username + '/sessions');
    if(resp.ok) return await resp.json();
    else showError(""Unauthorized Access"");
}

function showError(txt){
    let errorTitle = document.querySelector(""#error-title"");
    let errorBar = document.querySelector(""#error-bar"");
    errorTitle.innerText = txt;
    errorBar.setAttribute(""show"","""");
    setTimeout(()=>{errorBar.removeAttribute(""show"");},4000);
}

async function updateUsrTable(usr_table){
    
    usr_table.innerHTML = ``;
    users = await getUsers();
    users.forEach( u => { usr_table.appendChild(buildTableElement(u,usr_table)) });
}

function doSelection(index, usr_table){
    selected_usr = index;
    for(let node of usr_table.childNodes){
        if(node.id === index) node.setAttribute(""selected"","""");
        else node.removeAttribute(""selected"");
    }
}
function buildTableElement(data, usr_table){
    let row = document.createElement('tr');
    let col_usr_name = document.createElement('td');
    let col_usr_role = document.createElement('td');
    col_usr_role.classList.add(""center"");
    let col_full_name = document.createElement('td');
    col_full_name.classList.add(""center"");
    let col_status = document.createElement('td');
    col_status.classList.add(""center"");

    col_usr_name.innerText = data.userName;
    col_usr_role.innerText = data.role;
    col_full_name.innerText = data.fullName;
    col_status.innerText = data.status;


    row.appendChild(col_usr_name);
    row.appendChild(col_full_name);
    row.appendChild(col_usr_role);
    row.appendChild(col_status);
    
    // the additional escape is not needed, but... you know...
    row.id = escapeHtml(data.userName);

    row.classList.add(""usr_entry"");
    row.onclick=async function (e){
        doSelection(data.userName,usr_table);
        let session_data = await getUserSessions(selected_usr);
        if(session_data.Success) fillDetailView(session_data.sessions);
    }

    return row;
}


function getSelectedUser(){
    if(selected_usr === ""__none__"") return null;
    return users.find((usr)=> usr.userName === selected_usr);
}

function fillDetailView(sessions){
    let details = document.querySelector(""#details"");
    details.innerHTML = """";
    let usr = getSelectedUser();
    if(!usr) return;

    details.appendChild(buildDetailItem(""User Name"", usr.userName));
    details.appendChild(buildDetailItem(""Full Name"", usr.fullName));
    details.appendChild(buildDetailItem(""E-mail"", usr.email));
    details.appendChild(buildDetailItem(""Role"", usr.role));
    details.appendChild(buildDetailItem(""Activity Expiry [UTC]"", usr.activity_expiry));
    details.appendChild(buildDetailItem(""Password Expiry [UTC]"", usr.password_expiry));
    let sess_title = document.createElement(""strong"");
    sess_title.style = ""align-self:center;"";
    sess_title.innerText = ""Sessions"";
    details.appendChild(sess_title);

    details.appendChild(buildSessionTable(sessions));

    details.appendChild(build_action_bar());
}

function buildDetailItem(el_name, el_data){
    let div = document.createElement(""div"");
    div.classList.add(""detail-item"");
    let strong = document.createElement(""strong"");
    strong.innerText = el_name;
    div.appendChild(strong);
    let span = document.createElement(""span"");
    span.innerText = el_data;
    div.appendChild(span);
    return div;
}

function buildSessionTable(sessions){
    let table = document.createElement(""table"");
    table.classList.add(""session-table"");
    table.innerHTML = `
    <thead >
        <tr>
            <th class=""center"">Last Seen [UTC]</th>
            <th class=""center"">User Agent</th>
            <th class=""center"">IP</th>
            <th class=""center"">Session Expiry [UTC]</th>
        </tr>
    </thead>
    `;

    sessions.forEach( s => {
        table.appendChild(buildSessionItem(s));
    });

    return table;
}

function buildSessionItem(session){
    let row = document.createElement(""tr"");
    let last_seen = document.createElement(""td"");
    last_seen.classList.add(""center"");
    let user_agent = document.createElement(""td"");
    user_agent.classList.add(""center"");
    let ip = document.createElement(""td"");
    ip.classList.add(""center"");
    let expiry = document.createElement(""td"");
    expiry.classList.add(""center"");
    let a_usr_s = document.createElement(""a"");
    a_usr_s.classList.add(""pointer"");
    let d_usr_s = document.createElement(""div"");
    d_usr_s.classList.add(""full-user-agent"");

    last_seen.innerText = session.last_seen;
    ip.innerText = session.ip;
    expiry.innerText = session.expiry;

    a_usr_s.innerText = session.user_agent.substring(0,7) + ""...""
    // for phone support
    a_usr_s.onclick = ()=>{d_usr_s.setAttribute(""show"",""""); setTimeout(()=>{d_usr_s.removeAttribute(""show"");}, 3000)}
    user_agent.onmouseover = (e)=>{d_usr_s.setAttribute(""show"","""");}
    user_agent.onmouseout = (e) => { d_usr_s.removeAttribute(""show"");}
    d_usr_s.innerText = session.user_agent;
    user_agent.appendChild(a_usr_s);
    user_agent.appendChild(d_usr_s);

    row.appendChild(last_seen);
    row.appendChild(user_agent);
    row.appendChild(ip);
    row.appendChild(expiry);

    return row;
}

function addUser(){
    set_return_btn(""cancel"");
    build_notify_container(add_user_notify_box());
}

function build_notify_container(fill){
    var notify_container = document.querySelector(""#notification-bkg"");
    var notify_box = document.querySelector(""#box-notification"");
 
    notify_box.innerHTML = '';
    notify_box.appendChild(fill);
    notify_container.setAttribute(""show"","""");
}

function add_user_notify_box(){
    let form = document.createElement(""form"");
    form.innerHTML = `
        <h6 style=""color:red;"" id=""error-display""></h6>
        <h4><strong>Create User:</strong></h4>
        <label>User Name</label>
        <input type=""text"" id=""userName"" placeholder=""username"">
        <label>E-mail</label>
        <input type=""text"" id=""email"" placeholder=""e-mail"">
        <label>Full Name</label>
        <input type=""text"" id=""fullname"" placeholder=""full name"">
        <label>Role</label>
        <select id=""role"" style=""width:10rem; -moz-appearance:menulist; -webkit-appearance:menulist; appearance:menulist;"">
            <option value=""reader"">Reader</option>
            <option value=""writer"">Writer</option>
            <option value=""admin"">Admin</option>
        </select>
        <label>User Activity in Days</label>
        <input style=""width:10rem; display:block;"" type=""number"" id=""duration_days"" min=""0"" max=""9999"" value=""100"">
        <input type=""submit"" id=""submit"" value=""Add User"">
    `;
    var _err = form.querySelector(""#error-display"");
    var _username = form.querySelector(""#userName"");
    var _role = form.querySelector(""#role"");
    var _fullname = form.querySelector(""#fullname"");
    var _email = form.querySelector(""#email"");
    var _duration = form.querySelector(""#duration_days"");
    var btn = form.querySelector(""#submit"");

    form.onsubmit = async (e)=>{
        e.preventDefault();
        
        var data = {
            userName : _username.value,
            role :_role.value,
            fullName : _fullname.value,
            email : _email.value,
            duration_days : Number.parseFloat(_duration.value)
        }
        console.log(data);
        btn.style=""display:none;""
        let spinner = build_spinner();
        spinner.style = ""width:10%; height:10%;""
        form.appendChild(spinner);
        let resp = await postDataJson(""/admin/users/create"", data)
        form.removeChild(spinner);
        btn.style=""display:block;""

        console.log(resp);
        
        if(resp.Success){
            let u_name = escapeHtml(resp.user.userName);
            let pw = escapeHtml(resp.temporary_pw)
            form.innerHTML = `
            <h6><strong> User '<a>${u_name}</a>' Successfully created </strong></h6>
            ${
                (resp.isSend) ? 
                '<h6>E-mail sent to user.</h6>' :
                '<h6>An E-mail to user could not be sent.</h6>'+
                '<h6>Please contact user with one-time password: <strong><a>'
                + pw +'</a></strong></h6>' 
            }
            `;
            set_return_btn(""ok"");
        }
        else{
            _err.innerText = resp.ErrorMessage;
        }

    }

    return form;

}

function build_action_bar(){

    let div = document.createElement(""div"");
    div.style = ""display:flex; justify-content:space-evenly; align-items:center; flex-wrap:wrap; margin-top:3rem;"";
    div.appendChild(build_update_btn());
    div.appendChild(build_reset_pw());
    div.appendChild(build_remove_btn());

    return div;
}

function build_spinner(){
    let spinner = document.createElement(""div"");
    spinner.classList.add(""loader"");
    spinner.style = ""margin:0 auto;""
    return spinner;
}
function build_reset_pw(){
    let btn = document.createElement(""button"");
    btn.innerText = ""Reset Password"";
    btn.onclick = async ()=>{
        
        set_return_btn(""ok"");
        build_notify_container(build_spinner());

        let resp = await postDataJson('/admin/users/'+selected_usr+'/reset_pw',{});
        let h4 = document.createElement(""h4"");
        console.log(resp);
        if(resp.Success) {
            h4.innerHTML = `Password reset successfully for user <a>${escapeHtml(selected_usr)}</a>.<br>
            ${
                resp.isSend ? ""E-mail sent to user."" :
                `E-mail could not be sent. <br> Please contact the user with his one time password: <strong>${escapeHtml(resp.temporary_pw)}</strong>`
            }
            `;
        }
        else {
            if(resp.ErrorCodes && resp.ErrorCodes[0])
                h4.innerText = ""An Error occurred during password reset: "" + escapeHtml(resp.ErrorCodes[0]);
            else
                h4.innerText = ""An Error occurred during password reset: "" + escapeHtml(resp.ErrorMessage);

        }
        build_notify_container(h4);
    }

    return btn;
}

function build_remove_btn(){
    let btn = document.createElement(""button"");
    btn.innerText = ""Delete User"";
    btn.onclick = ()=>{
        set_return_btn(""cancel"");
        build_notify_container(build_remove_view());
    }

    return btn;
}

function build_remove_view(){
    let div = document.createElement(""div"");
    let h4 = document.createElement(""h4"");
    h4.innerHTML = `User <strong><a>${escapeHtml(selected_usr)}</a> </strong> will be permanetly removed from the system, all data will be lost.`
    let btn = document.createElement(""button"");
    btn.innerText = ""Continue"";
    btn.style = ""margin:0 auto; display:block;"";
    btn.onclick = async ()=>{
        let resp = await postDataJson('/admin/users/'+selected_usr+'/delete', {});
        if(resp.Success){
            btn.style=""display:none"";
            h4.innerHTML = `User <strong><a>${escapeHtml(selected_usr)}</a> </strong> has been delete.`;
            set_return_btn(""ok"");
        }
        else{
            btn.style=""display:none"";
            h4.innerHTML = `An error occurred while deleting the user: <a>${escapeHtml(resp.ErrorCodes[0])}</a>`;
            set_return_btn(""ok"");
        }
    }

    div.appendChild(h4);
    div.appendChild(btn);
    return div;
}

function build_update_btn(){
    let btn = document.createElement(""button"");
    btn.innerText = ""Edit User"";
    btn.onclick = ()=>{
        set_return_btn(""cancel"");
        build_notify_container(update_user_view());
    }
    return btn;
}

function update_user_view(){
    let form = document.createElement(""form"");
    let usr = getSelectedUser();

    form.innerHTML = `
        <h6 style=""color:red;"" id=""error-display""></h6>
        <h4><strong>Updating User: <a>${escapeHtml(usr.userName)}</a> </strong></h4>
        <label>E-mail</label>
        <input type=""text"" id=""email"">
        <label>Full Name</label>
        <input type=""text"" id=""fullname"" >
        <label>Role</label>
        <select id=""role"" style=""width:10rem; -moz-appearance:menulist; -webkit-appearance:menulist; appearance:menulist;"">
            <option value=""reader"">Reader</option>
            <option value=""writer"">Writer</option>
            <option value=""admin"">Admin</option>
        </select>
        <label>Extend User Activity by Number of Days</label>
        <input style=""width:10rem;"" type=""number"" id=""duration_days"" min=""-1"" max=""9999""> 
        <h6 style=""display:inline;"">If set to -1, disable the user </h6> <br>
        <input type=""submit"" id=""submit"" value=""Update User"">
    `;
    let _err = form.querySelector(""#error-display"");
    let _role = form.querySelector(""#role"");
    let _fullname = form.querySelector(""#fullname"");
    let _email = form.querySelector(""#email"");
    let _duration = form.querySelector(""#duration_days"");
    let btn = form.querySelector(""#submit"");

    _role.value = escapeHtml(usr.role).toLowerCase();
    _fullname.value = escapeHtml(usr.fullName);
    _email.value = escapeHtml(usr.email);
    _duration.value = 0 ;
    
    form.onsubmit = async (e)=>{
        e.preventDefault();
        
        let data = {
            userName : usr.userName,
            role :_role.value,
            fullName : _fullname.value,
            email : _email.value,
            duration_days : Number.parseFloat(_duration.value)
        }
        console.log(data);

        let resp = await postDataJson(""/admin/users/""+ usr.userName+""/update"", data)
        console.log(resp);
        
        if(resp.Success){
            form.innerHTML = `
            <h6><strong> User '<a>${escapeHtml(usr.userName)}</a>' Successfully Updated </strong></h6>
            `;
            set_return_btn(""ok"");
        }
        else{
            _err.innerText = resp.ErrorMessage;
        }
    }

    return form;
}

function set_return_btn(val){
    var cancel_btn = document.querySelector(""#cancel-btn"");
    cancel_btn.innerText = val;

}
        ";
        }
    }
