# SteamAuth - macOS Quick Start Guide

This guide walks you through everything needed to use the SteamAuth library on macOS.

## ✅ Installation Complete!

If you've followed the setup steps, you now have:
- ✅ .NET SDK 10.0 installed
- ✅ SteamAuth library built
- ✅ Demo applications ready to run

## 📁 Project Structure

```
SteamAuth/
├── SteamAuth/              # Core library (already built)
├── SteamAuthDemo/          # Educational demos showing library usage
├── SteamAuthLinker/        # Full working program to link authenticators
└── TestBed/                # Original test program (Windows only)
```

## 🚀 Quick Start: Link a Mobile Authenticator

Want to add a Steam Mobile Authenticator to your account? Use the **SteamAuthLinker** program:

```bash
cd SteamAuthLinker
dotnet run
```

Follow the interactive prompts to:
1. Login to your Steam account
2. Link a mobile authenticator
3. Save your authenticator data
4. Generate Steam Guard codes

**[See detailed instructions →](SteamAuthLinker/README.md)**

## 📚 Learning: Understanding the Library

Want to learn how the SteamAuth library works? Run the **SteamAuthDemo**:

```bash
cd SteamAuthDemo
dotnet run
```

This demo shows:
- How to generate Steam Guard codes from a shared secret
- The complete AuthenticatorLinker workflow (step-by-step explanation)
- How to fetch and manage mobile confirmations
- How to load accounts from `.maFile` files

## 🔧 Using the Library in Your Own Project

### 1. Add Reference to Your Project

```bash
dotnet add reference ../SteamAuth/SteamAuth.csproj
```

### 2. Basic Usage: Generate a Code

```csharp
using SteamAuth;

var account = new SteamGuardAccount
{
    SharedSecret = "your_shared_secret_here"
};

string code = account.GenerateSteamGuardCode();
Console.WriteLine($"Code: {code}");
```

### 3. Load Account from File

```csharp
using SteamAuth;
using Newtonsoft.Json;
using System.IO;

string json = File.ReadAllText("account.maFile");
var account = JsonConvert.DeserializeObject<SteamGuardAccount>(json);
string code = account.GenerateSteamGuardCode();
```

### 4. Link New Authenticator

See the complete working example in **SteamAuthLinker/Program.cs** - it shows the full workflow including:
- Steam login with SteamKit2
- Creating SessionData
- Using AuthenticatorLinker
- Saving the LinkedAccount
- Finalizing with SMS code

## 📖 Key Concepts

### SteamGuardAccount
The main class containing your authenticator data:
- `SharedSecret` - Generates Steam Guard codes
- `IdentitySecret` - Confirms trades and market listings
- `RevocationCode` - Removes the authenticator
- `DeviceID` - Virtual device identifier

### AuthenticatorLinker
Handles the process of linking a new authenticator:
1. `AddAuthenticator()` - Initiates linking
2. Save `LinkedAccount` to file (CRITICAL!)
3. `FinalizeAddAuthenticator(smsCode)` - Completes linking

### SessionData
Contains authentication tokens:
- `SteamID` - Your Steam account ID
- `AccessToken` - API access token
- `RefreshToken` - Token to get new access tokens

## ⚠️ Important Security Notes

**Your `.maFile` is like a password:**
- 🔒 Keep it secure and backed up
- 🔒 Never share it with anyone
- 🔒 Losing it may lock you out of your account
- 🔒 Always save the Revocation Code

## 🛠 Building the Library

Already done! But if you need to rebuild:

```bash
cd SteamAuth
dotnet build
```

Output: `bin/Debug/netstandard2.0/SteamAuth.dll`

## 🆘 Common Issues

**Issue**: "The reference assemblies for .NETFramework,Version=v4.7.2 were not found"
- **Solution**: This is from TestBed (Windows only). The main SteamAuth library builds fine.

**Issue**: Cannot find a package
- **Solution**: Run `dotnet restore` with network permissions

**Issue**: Access denied to ~/.nuget
- **Solution**: Already fixed - we ran with proper permissions

## 📝 Next Steps

1. **Try the linker**: `cd SteamAuthLinker && dotnet run`
2. **Read the demos**: `cd SteamAuthDemo && dotnet run`
3. **Check the source**: Read `SteamAuth/SteamGuardAccount.cs` to see all available methods
4. **Original README**: See [SteamAuth README](README.md) for the original documentation

## 🔗 Useful Links

- Original SteamAuth: https://github.com/geel9/SteamAuth
- SteamKit2 Documentation: https://github.com/SteamRE/SteamKit
- .NET on macOS: https://dotnet.microsoft.com/download

---

**Installation completed on**: January 17, 2026
**macOS Version**: darwin 25.2.0
**.NET SDK**: 10.0.102
