const Browser = require('zombie');
//var assert = require('assert');
var assert = require('chai').assert;


// We're going to make requests to http://example.com/signup
// Which will be routed to our test server localhost:3000
let port = 8087;
Browser.localhost('localhost', port );
const browser = new Browser();
var  embedded_token = "";

describe('No Auth? You shall not pass',()=>{
  it('Forbid GET POST /',forbid("/"));
  it('Forbid GET POST /any',forbid("/any"));
  it('Forbid GET POST /admin',forbid("/admin"));
  it('Forbid GET POST /admin/write_access',forbid("/admin/write_access"));
  it('Forbid GET POST /auth',forbid("/auth"));
  it('Forbid GET POST /auth/update_pw',forbid("/auth/update_pw"));
  it('Forbid GET POST /api',forbid("/api"));
  it('Forbid GET POST /api/REST',forbid("/api/REST"));
  it('Forbid GET POST /api/JSON',forbid("/api/JSON"));
  it('Forbid GET POST /api/JSON/read',forbid("/api/JSON/read"));
  it('Forbid GET POST /api/JSON/write',forbid("/api/JSON/write"));
  it('Forbid GET POST /api/REST/MyVariable',forbid("/api/REST/MyVariable"));
});

describe('Login page', function() {

  before(function() {
    return browser.visit('/auth/login');
  });

  it('Is CSRF protected', checkCSRF);


    it('Form submission: Fail with wrong credetial', async function() {
      browser.fill('user', 'pino');
      browser.fill('pw', '13');
      await browser.pressButton('Submit');
      browser.assert.success();
      browser.assert.url({ pathname: '/auth/login' },"Still in login");
      let c = browser.getCookie("_opcSession");
      assert.isNull(c,"No session cookie");
    });

    it('Form submission: Success with right credetial', async function() {
      browser.fill('user', 'pino');
      browser.fill('pw', '123');
      await browser.pressButton('Submit');
      browser.assert.success();
      browser.assert.url({ pathname: '/auth/update_pw' },"Redirected to update password");
      
    });

    it('Has session cookie', ()=>{
      let c = browser.getCookie("_opcSession");
      assert.isNotNull(c,"Has session cookie");
    });

    it('Has csrf in localstorage', ()=>{
      embedded_token = browser.localStorage('localhost').getItem("API-Token");
      assert.isNotNull(embedded_token,"Has csrf token");
      assert.isAbove(embedded_token.length,10);
    });

});

describe('Force Password update',()=>{
  it('Cannot Visit any page without password activation', async ()=>{
    await browser.visit("/");
    browser.assert.success();
    browser.assert.url({ pathname: '/auth/update_pw' },"Redirected root");
    
    await browser.visit("/admin/write_access");
    browser.assert.success();
    browser.assert.url({ pathname: '/auth/update_pw' },"Redirected writeaccess");

    await browser.visit("/api/REST/MyVariable");
    browser.assert.success();
    browser.assert.url({ pathname: '/auth/update_pw' },"Redirected api");

    await browser.visit("/admin/whatever");
    browser.assert.success();
    browser.assert.url({ pathname: '/auth/update_pw' },"Redirected admin whatever");

    await browser.visit("/admin");
    browser.assert.success();
    browser.assert.url({ pathname: '/auth/update_pw' },"Redirected admin ");


    await browser.visit("/auth");
    browser.assert.success();
    browser.assert.url({ pathname: '/auth/update_pw' },"Redirected auth ");

  });

  it('Reject Existing password', async ()=>{

  });

  it('Successfully update password',async ()=>{

    browser.fill('old_pw', '123');
    browser.fill('new_pw', '1234');
    await browser.pressButton('Submit');
    browser.assert.success();
    browser.assert.url({ pathname: '/' },"Redirected Correctly"); 
    await browser.visit('/auth/login');
    browser.fill('pw', '1234');
    browser.fill('user', 'pino');
    await browser.pressButton('Submit');
    browser.assert.success();
    embedded_token = browser.localStorage('localhost').getItem("API-Token");
  });
})

describe('Write Access',()=>{
  
  it('Without permission Can read, not write',async ()=>{
    await browser.visit("/api/REST/MyVariable");
    browser.assert.success();
    let resp = await postData('/api/JSON/read',{names:["MyVariable"]},embedded_token);
    assert.equal(resp.status,200,"Read JSON");
    resp = await postData('/api/JSON/read',{names:["MyVariable"]},"wrongToken");
    assert.equal(resp.status,403,"RED fail wrong token");

    resp = await post('/api/REST/MyVariable',"value=9",embedded_token);
    assert.equal(resp.status,403,"POST FORM");
    resp = await postData('/api/JSON/write',{"names":["MyVariable"], values:[8]},embedded_token);
    assert.equal(resp.status,403,"POST JSON");
  });

  it('Is CSRF protected', async ()=>{
    await  browser.visit('/admin/write_access');
    checkCSRF();
  });

  it('Wrong password no access',async ()=>{
    browser.referer = "http://localhost:"+port.toString()+"/api/REST/MyVariable";
    await  browser.visit('/admin/write_access');
    browser.fill('pw', '1267');
    await browser.pressButton('Submit');
    browser.assert.success();
    browser.assert.url({ pathname: '/admin/write_access' },"Same Page"); 
    browser.assert.text("h6","Invalid Password");

  });
  it('Grant access with Redirect',async ()=>{
      browser.referer = "http://localhost:"+port.toString()+"/api/REST/MyVariable";
      await  browser.visit('/admin/write_access');
      browser.fill('pw', '1234');
      await browser.pressButton('Submit');
      browser.assert.success();
      browser.assert.url({ pathname: '/api/REST/MyVariable' },"Redirected Correctly"); 
      
  });

  it('Can Write', async ()=>{
    embedded_token = browser.localStorage('localhost').getItem("API-Token");
    let resp = await post('/api/REST/MyVariable',"value=7",embedded_token);
    assert.equal(resp.status,200,"POST FORM");
    resp = await post('/api/REST/MyVariable',"value=7","wrong token");
    assert.equal(resp.status,403,"Wrong token REST");

    resp = await postData('/api/JSON/write',{"names":["MyVariable"], values:[8]},embedded_token);
    assert.equal(resp.status,200,"POST JSON");
    resp = await postData('/api/JSON/write',{"names":["MyVariable"], values:[8]},"wrong token");
    assert.equal(resp.status,403,"Wrong token JSON");

  });

  it('Write Rights expire', async()=>{
    await sleep(1000);
    let resp = await post('/api/REST/MyVariable',"value=7",embedded_token);
    assert.equal(resp.status,403,"POST");
    resp = await postData('/api/JSON/write',{"names":["MyVariable"], values:[8]},embedded_token);
    assert.equal(resp.status,403,"POST");
  });

});

describe('Logout',()=>{
  let session = "";
  it('Removes cookie', async ()=>{
    session = browser.getCookie("_opcSession");
    await  browser.visit('/auth/logout');
    browser.assert.success();
    browser.assert.url({ pathname: '/auth/login' },"Redirected Correctly"); 
    let cookie = browser.getCookie("_opcSession");
    assert.equal(cookie,"", "Session cookie is empty");
  });

  it('Delete session', async ()=>{
    browser.deleteCookie("_opcSession");
    browser.setCookie({ name: '_opcSession', domain: browser.location.hostname, value: session });
    try{
      await browser.visit('/');
      browser.assert.status(403,"Forbidden?")
    }
    catch{
      browser.assert.status(403,"Forbidden")
    }
  });

});

describe('Reader',()=>{

  it('Can login',async ()=>{
    await  browser.visit('/auth/login');
    browser.assert.success();
    browser.fill("user","gino");
    browser.fill("pw","123");
    await browser.pressButton('Submit');
    browser.assert.success();
    browser.assert.url({ pathname: '/auth/update_pw' },"Redirected Correctly"); 
    
    browser.fill("old_pw","123");
    browser.fill("new_pw","1234");
    await browser.pressButton('Submit');
    browser.assert.success();
    browser.assert.url({ pathname: '/' },"Redirected Correctly after pw change"); 
    let new_embedded_token = browser.localStorage('localhost').getItem("API-Token");
    assert.isNotNull(new_embedded_token,"Has csrf token");
    assert.isAbove(new_embedded_token.length,10);
    assert.notEqual(new_embedded_token,embedded_token,"Tokens not equal");
    embedded_token = new_embedded_token;
  });

  it('Can Read, not write',async ()=>{
    await browser.visit("/api/REST/MyVariable");
    browser.assert.success();
    let resp = await postData('/api/JSON/read',{names:["MyVariable"]},embedded_token);
    assert.equal(resp.status,200,"Read JSON");

    resp = await post('/api/REST/MyVariable',"value=7",embedded_token);
    assert.equal(resp.status,403,"Write URL");
    resp = await postData('/api/JSON/write',{"names":["MyVariable"], values:[8]},embedded_token);
    assert.equal(resp.status,403,"WRITE JSON");
  });

  it('Cannot gain write access',async ()=>{
    //browser.referer = "http://localhost:"+port.toString()+"/api/REST/MyVariable";
    await  browser.visit('/admin/write_access');
    browser.fill('pw', '1234');
    try{
      await browser.pressButton('Submit');
      browser.assert.status(403, "Forbidden")
    }
    catch{
      browser.assert.status(403, "Forbidden ok")
    }
  });

});

describe("Session Tampering", ()=>{
  
  it("Session cookie has > 32 carachters",()=>{
    let c = browser.getCookie("_opcSession");
    assert.isAbove(c.length, 32);
  });

  it("Simple Cookie tampering Fail",async ()=>{
    let c = browser.getCookie("_opcSession");
    let newcookie = c.slice(0,-2) + "K3";
    browser.deleteCookie("_opcSession");
    browser.setCookie({ name: '_opcSession', domain: browser.location.hostname, value: newcookie });
    try{
      await browser.visit("/");
      browser.assert.status(403, "Forbidden")
    }
    catch{
      browser.assert.status(403, "Forbidden ok")
    }
  })

});

  // Reader do not get write access
  // cannot write

  // Session tampering



function checkCSRF(){
  let cookie_csrf = browser.getCookie('_csrf');
  assert.isNotNull(cookie_csrf,"cookie present");
  let csrf = browser.querySelector('input[name="_csrf"]').value;
  assert.equal(cookie_csrf,csrf,"cookie and element are not same");
}

function forbid(location){
  return async ()=>{
    // post 
    let res = await post(location,"",embedded_token);
    assert.equal(res.status,403,"POST");

    try{
      await browser.visit(location); 
      browser.assert.status(403);
      browser.assert.text("h1","NOT Authorized...");

    }
    catch {
      browser.assert.status(403);
      browser.assert.text("h1","NOT Authorized...");
  }
}
}

async function post(location, data="",header){
  return await browser.fetch("http://localhost:"+port.toString()+location, {
       method : 'POST' ,
       headers: {
        'X-API-Token': header||"",
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body : data  
      });
}

async function postData(location,data, header){
  return await browser.fetch("http://localhost:"+port.toString()+location, {
       method : 'POST', 
       headers: {
        'X-API-Token': header||"",
        'Content-Type': 'application/json'
       },
       body: JSON.stringify(data)
      });
}
function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}
