# How to run and build locally

- Make sure you have .NET 6 SDK or above
  - `dotnet --version` should show `6.x.yyy`
  - if not, go get it: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
- Clone the repo
- `cd` into the root of the repo
- Run release build `dotnet run --project ./LD51 -c Release`
- Run debug build `dotnet run --project ./LD51 -- --skipSnapshot` (yes, that many dashes, spaced exactly like that)
