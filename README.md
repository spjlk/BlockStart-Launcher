# BlockStart Launcher

A small WPF launcher for Windows 10 and Windows 11.

## Features
- Dark Minecraft-inspired WPF interface
- Java Edition: official Microsoft/Xbox sign-in through CmlLib, release version list, download/install and launch
- Java maximum RAM setting (2–16 GB)
- Bedrock Edition: attempts the registered `minecraft:` protocol, then opens the Microsoft Store listing

## Requirements
- Windows 10 or 11, 64-bit
- .NET 8 SDK (for building)
- An internet connection for NuGet restore and Minecraft downloads
- A Microsoft account that owns Minecraft: Java Edition to play Java
- Minecraft for Windows (Bedrock) installed from Microsoft Store/Xbox app to play Bedrock

## Build in Visual Studio
1. Install Visual Studio 2022 with the **.NET desktop development** workload.
2. Open `BlockStartLauncher.csproj`.
3. Let NuGet restore packages.
4. Choose `Release` and `x64`, then Build > Build Solution.
5. Run the generated `bin\Release\net8.0-windows\BlockStartLauncher.exe`.

## Build from PowerShell
Install the .NET 8 SDK, open PowerShell in this folder, then run:

```powershell
dotnet restore
dotnet build -c Release
```

To publish a self-contained 64-bit Windows folder (does not require the user to install the .NET Desktop Runtime):

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

Output is under:
`bin\Release\net8.0-windows\win-x64\publish\`

You can distribute the entire publish folder. To make a single-file executable:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Notes and limitations
- Java authentication uses the CmlLib Microsoft login flow. You must own Java Edition on the Microsoft account used.
- This starter keeps the login session in memory only; it does not persist refresh tokens or implement a remember-me feature.
- Version list currently shows release versions, not snapshots or mod-loader profiles.
- RAM allocation applies only to Java Edition. Do not allocate all system RAM; leave memory for Windows and other applications.
- Bedrock is managed by the official Microsoft Store/Xbox installation. Third-party launchers cannot generally choose Bedrock game versions or set its JVM RAM (Bedrock is not Java).
- This project is a starter implementation. Build it on your own PC with the current .NET SDK and test against your installed Minecraft editions.
- CmlLib.Core and CmlLib.Core.Auth.Microsoft are third-party open-source dependencies; review their licenses and release notes before redistributing.
