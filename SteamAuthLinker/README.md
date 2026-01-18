# Steam Mobile Authenticator Linker

A complete, user-friendly command-line tool to link a Steam Mobile Authenticator to your account on macOS.

## What This Does

This program implements the complete workflow from the [SteamAuth README](../README.md):

1. **Login to Steam** - Authenticates using your Steam credentials
2. **Link Authenticator** - Adds a mobile authenticator to your account
3. **Save Data** - Saves your authenticator secrets to a `.maFile`
4. **Finalize** - Completes the linking process using an SMS code
5. **Generate Codes** - Load saved `.maFile` and generate Steam Guard codes

## Features

- ✅ Complete interactive workflow
- ✅ Automatic file saving (prevents account lockout)
- ✅ Phone number support (if required)
- ✅ Email confirmation handling
- ✅ SMS code verification
- ✅ Live code generation from saved files
- ✅ Beautiful terminal UI with progress indicators
- ✅ Error handling and helpful messages

## Requirements

- macOS
- .NET SDK 10.0+ (already installed if you followed the main setup)
- A Steam account
- Access to your email and phone for verification

## Installation

The program is already set up! All dependencies are installed.

## Usage

### Run the Program

```bash
cd /Users/asgreen/Personal/SteamAuth/SteamAuthLinker
dotnet run
```

### Main Menu Options

**Option 1: Link a new mobile authenticator**
- Follow the interactive prompts
- Enter your Steam username and password
- Provide phone number if your account doesn't have one
- Confirm email if prompted
- Enter SMS code to finalize
- Your authenticator data will be saved to `[username].maFile`

**Option 2: Generate codes from existing .maFile**
- Enter the path to your saved `.maFile`
- Watch live Steam Guard codes update every 30 seconds
- Press Ctrl+C to stop

**Option 3: Exit**

### Example Session

```
╔══════════════════════════════════════════════════════════╗
║   Steam Mobile Authenticator Linker                     ║
║   Link a mobile authenticator to your Steam account     ║
╚══════════════════════════════════════════════════════════╝

=== Main Menu ===
1. Link a new mobile authenticator to an account
2. Generate codes from an existing .maFile
3. Exit

Choose an option: 1

╔══════════════════════════════════════════════════════════╗
║   STEP 1: Login to Steam                                 ║
╚══════════════════════════════════════════════════════════╝

Steam Username: myusername
Steam Password: ********

→ Connecting to Steam...
✓ Connected to Steam

→ Authenticating...
→ Waiting for authentication...
✓ Authentication successful!
✓ Logged in as Steam ID: 76561198012345678

[... continues through the full workflow ...]
```

## Important Notes

⚠️ **CRITICAL**: Your `.maFile` contains sensitive data that can generate Steam Guard codes. 

- **Keep it secure** - Store it safely, like a password
- **Back it up** - If you lose this file, you may be locked out of your account
- **Save the Revocation Code** - You'll need this to remove the authenticator
- **Never share it** - Anyone with this file can generate your Steam Guard codes

## What Gets Saved

The `.maFile` contains:
- `shared_secret` - Used to generate Steam Guard codes
- `identity_secret` - Used to confirm trades/market listings
- `revocation_code` - Used to remove the authenticator
- `account_name` - Your Steam username
- `device_id` - Virtual device identifier
- Other session data

## Troubleshooting

**"Account already has an authenticator linked"**
- You need to remove the existing authenticator first using your Steam Mobile App or the revocation code

**"Account requires a phone number"**
- Restart the program and provide your phone number when prompted
- Format: `+1 5551234567` (include country code)

**"Failed to finalize authenticator: BadSMSCode"**
- Double-check the SMS code from your phone
- The code expires after a short time - request a new one if needed

**"Must confirm email"**
- Check your email for a confirmation link from Steam
- Click the link, then press Enter in the program to continue

## Generating Codes Later

Once you've linked your authenticator, you can generate codes anytime:

```bash
cd /Users/asgreen/Personal/SteamAuth/SteamAuthLinker
dotnet run
# Choose option 2
# Enter path to your .maFile
```

## Technical Details

This program uses:
- **SteamAuth** - Core authenticator functionality
- **SteamKit2** - Steam network protocol and authentication
- **Newtonsoft.Json** - JSON serialization

The workflow matches the official [SteamAuth README](../README.md) exactly.

## Credits

Built on top of the [SteamAuth](https://github.com/geel9/SteamAuth) library.
