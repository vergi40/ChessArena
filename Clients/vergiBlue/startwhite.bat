echo "Launching white player client"
start cmd /k dotnet run --no-build --project vergiBlue -- --gamemode 1 --playername white
TIMEOUT /T 1 