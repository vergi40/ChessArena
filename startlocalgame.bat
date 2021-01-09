Rem Run local game on local browser
Rem Launches 2 chess AI instances and a local test server.
Rem After this launches node server and browser frontend

cd Clients//vergiBlue
@REM .//startlocalservergame.bat

@REM Return home
cd ..//..//

npm start Web//frontend
npm start Web//backend