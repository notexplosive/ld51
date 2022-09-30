dotnet publish .\LD51\ -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained
del .\LD51\bin\Release\net6.0\win-x64\publish\*.pdb
butler push .\LD51\bin\Release\net6.0\win-x64\publish notexplosive/ld51:windows