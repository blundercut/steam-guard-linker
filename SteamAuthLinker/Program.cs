using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamAuth;
using SteamKit2;
using SteamKit2.Authentication;
using SteamKit2.Internal;

namespace SteamAuthLinker
{
    /// <summary>
    /// Steam Mobile Authenticator Linker
    /// This program walks you through the complete process of linking a mobile authenticator to your Steam account.
    /// Based on the workflow described in the SteamAuth README.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Steam Mobile Authenticator Linker                     ║");
            Console.WriteLine("║   Link a mobile authenticator to your Steam account     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("\n=== Main Menu ===");
                Console.WriteLine("1. Link a new mobile authenticator to an account");
                Console.WriteLine("2. Generate codes from an existing .maFile");
                Console.WriteLine("3. Exit");
                Console.Write("\nChoose an option: ");

                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        await LinkNewAuthenticator();
                        break;
                    case "2":
                        GenerateCodesFromFile();
                        break;
                    case "3":
                        Console.WriteLine("\nGoodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        /// <summary>
        /// Main workflow: Link a new mobile authenticator to a Steam account
        /// </summary>
        static async Task LinkNewAuthenticator()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   STEP 1: Login to Steam                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            // Get credentials
            Console.Write("\nSteam Username: ");
            string username = Console.ReadLine();

            Console.Write("Steam Password: ");
            string password = ReadPassword();
            Console.WriteLine();

            // Connect to Steam using SteamKit2
            Console.WriteLine("\n→ Connecting to Steam...");
            SteamClient steamClient = new SteamClient();
            steamClient.Connect();

            // Wait for connection
            while (!steamClient.IsConnected)
            {
                await Task.Delay(500);
            }
            Console.WriteLine("✓ Connected to Steam");

            // Authenticate
            Console.WriteLine("\n→ Authenticating...");
            CredentialsAuthSession authSession;
            try
            {
                authSession = await steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(new AuthSessionDetails
                {
                    Username = username,
                    Password = password,
                    IsPersistentSession = false,
                    PlatformType = EAuthTokenPlatformType.k_EAuthTokenPlatformType_MobileApp,
                    ClientOSType = EOSType.Android9,
                    Authenticator = new UserConsoleAuthenticator(),
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Login failed: {ex.Message}");
                return;
            }

            // Wait for authentication to complete
            Console.WriteLine("→ Waiting for authentication...");
            var pollResponse = await authSession.PollingWaitForResultAsync();
            Console.WriteLine("✓ Authentication successful!");

            // Create session data
            SessionData sessionData = new SessionData()
            {
                SteamID = authSession.SteamID.ConvertToUInt64(),
                AccessToken = pollResponse.AccessToken,
                RefreshToken = pollResponse.RefreshToken,
            };

            Console.WriteLine($"✓ Logged in as Steam ID: {sessionData.SteamID}");

            // Disconnect from Steam (we don't need the connection anymore)
            steamClient.Disconnect();

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   STEP 2: Link Mobile Authenticator                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            // Create AuthenticatorLinker
            AuthenticatorLinker linker = new AuthenticatorLinker(sessionData);

            // Check if phone number is needed
            Console.WriteLine("\nDoes your account already have a phone number linked?");
            Console.Write("If not, enter your phone number (+1 5551234567), otherwise press Enter: ");
            string phoneNumber = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                linker.PhoneNumber = phoneNumber;
                Console.WriteLine($"✓ Phone number set: {phoneNumber}");
            }

            // Try to add authenticator
            Console.WriteLine("\n→ Adding authenticator to account...");
            
            int tries = 0;
            AuthenticatorLinker.LinkResult result = AuthenticatorLinker.LinkResult.GeneralFailure;
            
            while (tries < 5)
            {
                tries++;

                result = await linker.AddAuthenticator();

                if (result == AuthenticatorLinker.LinkResult.MustConfirmEmail)
                {
                    Console.WriteLine($"\n⚠  You must confirm your email address: {linker.ConfirmationEmailAddress}");
                    Console.WriteLine("   Check your email and click the confirmation link.");
                    Console.Write("   Press Enter when done...");
                    Console.ReadLine();
                    Console.WriteLine("→ Retrying...");
                    continue;
                }

                if (result == AuthenticatorLinker.LinkResult.MustProvidePhoneNumber)
                {
                    Console.WriteLine("\n✗ Your account requires a phone number.");
                    Console.WriteLine("   Please run this program again and provide a phone number.");
                    return;
                }

                if (result == AuthenticatorLinker.LinkResult.AuthenticatorPresent)
                {
                    Console.WriteLine("\n✗ Your account already has an authenticator linked.");
                    Console.WriteLine("   You must remove it before adding a new one.");
                    return;
                }

                if (result != AuthenticatorLinker.LinkResult.AwaitingFinalization)
                {
                    Console.WriteLine($"\n✗ Failed to add authenticator: {result}");
                    return;
                }

                // Success! Break out of the loop
                break;
            }

            if (result != AuthenticatorLinker.LinkResult.AwaitingFinalization)
            {
                Console.WriteLine("\n✗ Failed to add authenticator after multiple attempts.");
                return;
            }

            Console.WriteLine("✓ Authenticator added successfully!");

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   CRITICAL: Save Your Authenticator Data                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            // Save the LinkedAccount IMMEDIATELY (as per README warning)
            string fileName = $"{linker.LinkedAccount.AccountName}.maFile";
            string jsonData = JsonConvert.SerializeObject(linker.LinkedAccount, Formatting.Indented);

            try
            {
                File.WriteAllText(fileName, jsonData);
                Console.WriteLine($"\n✓ Authenticator data saved to: {fileName}");
                Console.WriteLine($"✓ Revocation Code: {linker.LinkedAccount.RevocationCode}");
                Console.WriteLine("\n⚠  IMPORTANT: Keep this file and revocation code safe!");
                Console.WriteLine("   You will need them to generate codes and remove the authenticator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ ERROR saving authenticator file: {ex.Message}");
                Console.WriteLine("\n⚠  FOR SECURITY, the authenticator will NOT be finalized.");
                Console.WriteLine("   Without this file, you will be locked out of your account!");
                return;
            }

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   STEP 3: Finalize Authenticator                         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.WriteLine("\nSteam has sent an SMS code to your phone.");
            
            tries = 0;
            while (tries < 5)
            {
                tries++;

                Console.Write($"\nEnter SMS code (attempt {tries}/5): ");
                string smsCode = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(smsCode))
                {
                    Console.WriteLine("✗ SMS code cannot be empty.");
                    continue;
                }

                Console.WriteLine("→ Finalizing authenticator...");
                var finalizeResult = await linker.FinalizeAddAuthenticator(smsCode);

                if (finalizeResult == AuthenticatorLinker.FinalizeResult.Success)
                {
                    Console.WriteLine("\n✓✓✓ SUCCESS! ✓✓✓");
                    Console.WriteLine("Your mobile authenticator has been successfully linked!");
                    
                    // Update the saved file with the finalized status
                    jsonData = JsonConvert.SerializeObject(linker.LinkedAccount, Formatting.Indented);
                    File.WriteAllText(fileName, jsonData);

                    Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
                    Console.WriteLine("║   Your Authenticator is Ready!                           ║");
                    Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
                    Console.WriteLine($"\nFile saved: {fileName}");
                    Console.WriteLine($"Revocation Code: {linker.LinkedAccount.RevocationCode}");
                    
                    // Generate a test code
                    string testCode = linker.LinkedAccount.GenerateSteamGuardCode();
                    Console.WriteLine($"\nYour current Steam Guard code: {testCode}");
                    
                    Console.WriteLine("\n⚠  IMPORTANT REMINDERS:");
                    Console.WriteLine("   1. Keep your .maFile backed up safely");
                    Console.WriteLine("   2. Save your Revocation Code - you need it to remove the authenticator");
                    Console.WriteLine("   3. You can now generate codes using option 2 in the main menu");
                    
                    return;
                }
                else if (finalizeResult == AuthenticatorLinker.FinalizeResult.BadSMSCode)
                {
                    Console.WriteLine("✗ Invalid SMS code. Please try again.");
                    continue;
                }
                else
                {
                    Console.WriteLine($"✗ Failed to finalize: {finalizeResult}");
                    continue;
                }
            }

            Console.WriteLine("\n✗ Failed to finalize authenticator after multiple attempts.");
            Console.WriteLine("   Your authenticator data has been saved, but is not yet active.");
            Console.WriteLine("   You may need to try again later or contact Steam support.");
        }

        /// <summary>
        /// Generate codes from a saved .maFile
        /// </summary>
        static void GenerateCodesFromFile()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Generate Steam Guard Codes                             ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.Write("\nEnter the path to your .maFile: ");
            string filePath = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("✗ No file path provided.");
                return;
            }

            // Remove quotes if user dragged file into terminal
            filePath = filePath.Trim('"', '\'');

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"✗ File not found: {filePath}");
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var account = JsonConvert.DeserializeObject<SteamGuardAccount>(json);

                if (account == null || string.IsNullOrEmpty(account.SharedSecret))
                {
                    Console.WriteLine("✗ Invalid .maFile format or missing SharedSecret.");
                    return;
                }

                Console.WriteLine($"\n✓ Loaded account: {account.AccountName}");
                Console.WriteLine($"  Device ID: {account.DeviceID}");
                Console.WriteLine($"  Revocation Code: {account.RevocationCode}");
                Console.WriteLine($"  Fully Enrolled: {account.FullyEnrolled}");

                Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
                Console.WriteLine("║   Live Steam Guard Codes                                 ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
                Console.WriteLine("\nGenerating codes... (Press Ctrl+C to stop)\n");

                string lastCode = "";
                while (true)
                {
                    string code = account.GenerateSteamGuardCode();
                    long steamTime = TimeAligner.GetSteamTime();
                    long secondsRemaining = 30 - (steamTime % 30);

                    // Only print when code changes or every second
                    if (code != lastCode)
                    {
                        Console.WriteLine($"\n  Code: {code}");
                        lastCode = code;
                    }

                    // Progress bar
                    int barLength = 30;
                    int filled = (int)secondsRemaining;
                    string bar = new string('█', filled) + new string('░', barLength - filled);
                    
                    Console.Write($"\r  [{bar}] {secondsRemaining}s ");
                    
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (JsonException)
            {
                Console.WriteLine("✗ Invalid JSON format in .maFile");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Read password from console without echoing characters
        /// </summary>
        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);

            return password;
        }
    }

    /// <summary>
    /// Simple authenticator that prompts for 2FA codes via console
    /// Implements SteamKit2's IAuthenticator interface
    /// </summary>
    public class UserConsoleAuthenticator : IAuthenticator
    {
        public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
        {
            if (previousCodeWasIncorrect)
            {
                Console.WriteLine("✗ The previous code was incorrect.");
            }

            Console.Write("Enter the code from your Steam Mobile App: ");
            string code = Console.ReadLine()?.Trim() ?? "";
            return Task.FromResult(code);
        }

        public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect)
        {
            if (previousCodeWasIncorrect)
            {
                Console.WriteLine("✗ The previous code was incorrect.");
            }

            Console.WriteLine($"A code has been sent to your email: {email}");
            Console.Write("Enter the email code: ");
            string code = Console.ReadLine()?.Trim() ?? "";
            return Task.FromResult(code);
        }

        public Task<bool> AcceptDeviceConfirmationAsync()
        {
            Console.WriteLine("Please confirm this login in your Steam Mobile App.");
            Console.Write("Press Enter once you've confirmed...");
            Console.ReadLine();
            return Task.FromResult(true);
        }
    }
}
