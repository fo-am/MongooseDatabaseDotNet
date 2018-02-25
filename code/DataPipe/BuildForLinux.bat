del /s /q "linuxDeploy"

dotnet  publish -r linux-arm -c Release --force -o "../linuxDeploy"

del "%~dp0\linuxDeploy\appsettings.json"

PAUSE