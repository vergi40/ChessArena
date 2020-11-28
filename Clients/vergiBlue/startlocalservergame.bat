echo "Build projects"
dotnet build

echo "Launching server"
Rem start cmd.exe /c "%~dp0TestServer\bin\Debug\net5.0\TestServer.exe"
start cmd /k dotnet run --no-build --project TestServer
TIMEOUT /T 1 

echo "Launching white player client"
start cmd /k dotnet run --no-build --project vergiBlue -- --gamemode 1 --playername white
TIMEOUT /T 1 

echo "Launching black player client"
start cmd /k dotnet run --no-build --project vergiBlue -- --gamemode 1 --playername black