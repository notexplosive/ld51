# How to run and build locally

- Install .NET 6 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
- Clone the repo
- `cd` into the root of the repo
- Run release build `dotnet run --project ./LD51 -c Release`
- Run debug build `dotnet run --project ./LD51 -- --skipSnapshot` (yes, that many dashes, spaced exactly like that)
