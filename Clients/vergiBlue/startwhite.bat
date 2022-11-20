echo "Launching white player client"
start cmd /k dotnet run --no-build --project vergiBlueConsole -- chessarena --gamemode 1 --playername white
TIMEOUT /T 1 