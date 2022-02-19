echo "Launching server"
Rem start cmd.exe /c "%~dp0TestServer\bin\Debug\net5.0\TestServer.exe"
start cmd /k dotnet run --no-build --project vergiBlue.Tests\TestServer