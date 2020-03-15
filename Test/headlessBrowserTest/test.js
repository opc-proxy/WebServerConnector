const Browser = require('zombie');
var assert = require('assert');

// We're going to make requests to http://example.com/signup
// Which will be routed to our test server localhost:3000
Browser.localhost('localhost',8087 );

describe('User visits signup page', function() {

  const browser = new Browser();

  before(function() {
    return browser.visit('/admin/login');
  });

  describe('submits form', function() {

    before(function() {
      browser.fill('user', 'pino');
      browser.fill('pw', '123');
      return browser.pressButton('Submit');
    });

    it('should be successful', function() {
      browser.assert.success();
    });

//    it('should see welcome page', function() {
//      browser.assert.text('title', 'Welcome To Brains Depot');
//    });
  });

});
