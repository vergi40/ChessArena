rem Change active directory to script directory
cd /D "%~dp0"
cd ..

echo "Build projects"
dotnet build --verbosity quiet

rem TODO kill the process in test end
CALL startserver.bat
TIMEOUT /T 1

dotnet run --no-build --project vergiBlueConsole -- chessarena --gamemode 8 --playername pingtester