const express = require('express');
const { DefaultAzureCredential } = require('@azure/identity');
// const sql = require('mssql');
const app = express();
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

const { AzureLogger, setLogLevel } = require("@azure/logger");

setLogLevel("info");

// override logging to output to console.log (default location is stderr)
AzureLogger.log = (...args) => {
  console.log("[%s]: %s",new Date().toISOString(),...args);
};


app.locals.credential = new DefaultAzureCredential();

const configureSQL = async token => {
  const config = {
    authentication: {
      type: 'azure-active-directory-access-token',
      options: {
        token: token
      }
    },
    server: process.env.DB_SERVER,
    database: process.env.DB_NAME,
    options: {
      encrypt: true,
      enableArithAbort: true
    },
    pool: {
      max: 10,
      min: 0,
      idleTimeoutMillis: 30000
    }
  };

  return config;
};

app.get('/', async (req, res) => {
  try {
    if (req.app.locals.tokenObj.expiresOnTimestamp < Date.now()) {
     
      //logger.log(`token is ${app.locals.tokenObj.token}...`);
    }

    req.app.locals.tokenObj = await req.app.locals.credential.getToken(
      'https://ossrdbms-aad.database.windows.net/.default'
    );
    
    let msg = `${app.locals.tokenObj.token} <br/>`;
    msg += `expiresOnTimestamp :${new Date(req.app.locals.tokenObj.expiresOnTimestamp).toISOString()} <br/>`;
    msg += `now : ${new Date().toISOString()}`;
    res.send(msg);
  } catch (err) {
    console.log(err);
    res.status(500).send(err);
  }
});

(async function () {
  app.locals.tokenObj = await app.locals.credential.getToken(
    'https://ossrdbms-aad.database.windows.net/.default'
  );

  
  var server = app.listen(8000, () => {
    var host = server.address().address;  
    var port = server.address().port;  
    console.log('Example app listening at http://%s:%s', host, port);  
  });
})();