dotnet publish .\Hangman\ -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained
del .\Hangman\bin\Release\net6.0\win-x64\publish\*.pdb
butler push .\Hangman\bin\Release\net6.0\win-x64\publish notexplosive/ld51:windows