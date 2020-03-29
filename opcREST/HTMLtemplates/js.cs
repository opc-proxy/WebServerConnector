using System;

namespace opcRESTconnector
{
    public class jsTEmplates{

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
    return await response.json();
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
    let col_full_name = document.createElement('td');
    let col_status = document.createElement('td');

    col_usr_name.innerText = data.userName;
    col_usr_name.style=""width:25%;""
    
    col_usr_role.innerText = data.role;
    col_usr_role.style=""width:25%;""

    col_full_name.innerText = data.fullName;
    col_full_name.style=""width:25%;""

    col_status.innerText = data.status;
    col_status.style=""width:25%;""


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

        ";
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
        }
    }
