param([string]$Dir = $(throw "Parameter missing: -Dir Directory to output"))
dotnet publish "./Logic.Server/Logic.Server.csproj" -c Release -r win-x64 --self-contained true -o $Dir
dotnet publish "./Logic.Client/Logic.Client.csproj" -c Release -r win-x64 --self-contained true -o $Dir
dotnet publish "../communication/Agent/Communication.Agent.csproj" -c Release -r win-x64 --self-contained true -o $Dir -f netcoreapp3.0