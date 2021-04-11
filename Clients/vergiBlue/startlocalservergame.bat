echo "Build projects"
dotnet build

Rem CALL waits for script to finish. START doesn't block current script
Rem It doesn't matter here which one to use, because start-scripts invoke external console
CALL startserver.bat
TIMEOUT /T 1

CALL startwhite.bat
TIMEOUT /T 1

CALL startblack.bat