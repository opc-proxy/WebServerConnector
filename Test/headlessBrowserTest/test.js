const Browser = require('zombie');
//var assert = require('assert');
var assert = require('chai').assert;


// We're going to make requests to http://example.com/signup
// Which will be routed to our test server localhost:3000
let port = 8087;
Browser.localhost('localhost', port );
const browser = new Browser();

describe('No Auth? You shall not pass',()=>{
  it('Forbid GET POST /',forbid("/"));
  it('Forbid GET POST /any',forbid("/any"));
  it('Forbid GET POST /admin',forbid("/admin"));
  it('Forbid GET POST /admin/write_access',forbid("/admin/write_access"));
  it('Forbid GET POST /api',forbid("/api"));
  it('Forbid GET POST /api/REST',forbid("/api/REST"));
  it('Forbid GET POST /api/JSON',forbid("/api/JSON"));
  it('Forbid GET POST /api/JSON/read',forbid("/api/JSON/read"));
  it('Forbid GET POST /api/JSON/write',forbid("/api/JSON/write"));
  it('Forbid GET POST /api/REST/MyVariable',forbid("/api/REST/MyVariable"));
});

describe('Login page', function() {

  before(function() {
    return browser.visit('/admin/login');
  });

  it('Is CSRF protected', checkCSRF);

  describe('Form submission', async function() {

    it('Fail with wrong credetial', async function() {
      browser.fill('user', 'pino');
      browser.fill('pw', '13');
      await browser.pressButton('Submit');
      browser.assert.success();
      browser.assert.url({ pathname: '/admin/login' },"Still in login");
      let c = browser.getCookie("_opcSession");
      assert.isNull(c,"No session cookie");
    });


    it('Success with right credetial', async function() {
      browser.fill('user', 'pino');
      browser.fill('pw', '123');
      await browser.pressButton('Submit');
      browser.assert.success();
      browser.assert.url({ pathname: '/' },"Redirected");
      let c = browser.getCookie("_opcSession");
      assert.isNotNull(c,"Has session cookie");
    });

  });
});

describe('Write Access',()=>{
  
  it('Without permission Can read, not write',async ()=>{
    await browser.visit("/api/REST/MyVariable");
    browser.assert.success();
    let resp = await postData('/api/JSON/read',{names:["MyVariable"]});
    assert.equal(resp.status,200,"Read JSON");

    resp = await post('/api/REST/MyVariable',"value=9");
    assert.equal(resp.status,403,"POST FORM");
    resp = await postData('/api/JSON/write',{"name":"MyVariable", value:8});
    assert.equal(resp.status,403,"POST JSON");
  });

  it('Is CSRF protected', async ()=>{
    await  browser.visit('/admin/write_access');
    checkCSRF();
  });

  it('Grant access with Redirect',async ()=>{
      browser.referer = "http://localhost:"+port.toString()+"/api/REST/MyVariable";
      await  browser.visit('/admin/write_access');
      browser.fill('pw', '123');
      await browser.pressButton('Submit');
      browser.assert.success();
      browser.assert.url({ pathname: '/api/REST/MyVariable' },"Redirected Correctly"); 
      
  });

  it('Can Write', async ()=>{
    let resp = await post('/api/REST/MyVariable',"value=7");
    assert.equal(resp.status,200,"POST FORM");
    resp = await postData('/api/JSON/write',{"name":"MyVariable", value:8});
    assert.equal(resp.status,200,"POST JSON");
  });

  it('Write Rights expire', async()=>{
    await sleep(1000);
    let resp = await post('/api/REST/MyVariable',"value=7");
    assert.equal(resp.status,403,"POST");
    resp = await postData('/api/JSON/write',{"name":"MyVariable", value:8});
    assert.equal(resp.status,403,"POST");
  });

});

describe('Logout',()=>{
  let session = "";
  it('Removes cookie', async ()=>{
    session = browser.getCookie("_opcSession");
    await  browser.visit('/admin/logout');
    browser.assert.success();
    browser.assert.url({ pathname: '/admin/login' },"Redirected Correctly"); 
    let cookie = browser.getCookie("_opcSession");
    assert.equal(cookie,"", "Session cookie is empty");
  });

  it('Delete session', async ()=>{
    browser.setCookie("_opcSession", session);
    try{
      await browser.visit('/');
      browser.assert.status(403,"Forbidden?")
    }
    catch{
      browser.assert.status(403,"Forbidden")
    }
  });

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
    let res = await post(location);
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

async function post(location, data=""){
  return await browser.fetch("http://localhost:"+port.toString()+location, {
       method : 'POST' ,
       headers: {
         'Content-Type': 'application/x-www-form-urlencoded',
      },
      body : data  
      });
}

async function postData(location,data){
  return await browser.fetch("http://localhost:"+port.toString()+location, {
       method : 'POST', 
       headers: {
        'Content-Type': 'application/json'
       },
       body: JSON.stringify(data)
      });
}
function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}
