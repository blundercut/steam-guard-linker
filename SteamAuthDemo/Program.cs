using System;
using System.IO;
using Newtonsoft.Json;
using SteamAuth;

namespace SteamAuthDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SteamAuth Library Demo ===");
            Console.WriteLine("This demo shows the complete workflow described in the README\n");

            ShowMenu();
        }

        static void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- Choose a demo ---");
                Console.WriteLine("1. Generate Steam Guard Code (if you already have a shared secret)");
                Console.WriteLine("2. Show AuthenticatorLinker workflow (linking a new authenticator)");
                Console.WriteLine("3. Fetch and manage mobile confirmations");
                Console.WriteLine("4. Load account from .maFile and generate code");
                Console.WriteLine("5. Exit");
                Console.Write("\nEnter choice: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        DemoGenerateCode();
                        break;
                    case "2":
                        DemoAuthenticatorLinker();
                        break;
                    case "3":
                        DemoConfirmations();
                        break;
                    case "4":
                        DemoLoadFromFile();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Try again.");
                        break;
                }
            }
        }

        /// <summary>
        /// Demo 1: Generate Steam Guard codes from a shared secret
        /// README: "To generate login codes if you already have a Shared Secret, simply instantiate 
        /// a SteamGuardAccount and set its SharedSecret. Then call SteamGuardAccount.GenerateSteamGuardCode()."
        /// </summary>
        static void DemoGenerateCode()
        {
            Console.WriteLine("=== Demo 1: Generate Steam Guard Code ===");
            Console.WriteLine("This demonstrates generating a Steam Guard code from a shared secret.\n");

            // Example shared secret (this is just a demo - not a real one)
            string exampleSharedSecret = "cnOgv/KdpLoP6Nbh0GMkXkPXALQ=";

            // Step 1: Instantiate a SteamGuardAccount
            var account = new SteamGuardAccount
            {
                SharedSecret = exampleSharedSecret
            };

            // Step 2: Call GenerateSteamGuardCode()
            string code = account.GenerateSteamGuardCode();

            Console.WriteLine($"Generated Code: {code}");
            Console.WriteLine("(Note: This is a demo code generated from a demo secret)\n");

            // Show multiple codes to demonstrate time-based changes
            Console.WriteLine("Codes refresh every 30 seconds. Generating 3 codes:");
            for (int i = 1; i <= 3; i++)
            {
                code = account.GenerateSteamGuardCode();
                long steamTime = TimeAligner.GetSteamTime();
                long timeRemaining = 30 - (steamTime % 30);
                Console.WriteLine($"  {i}. Code: {code} (valid for {timeRemaining} more seconds)");
                
                if (i < 3)
                    System.Threading.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Demo 2: Show the AuthenticatorLinker workflow
        /// README: "To add a mobile authenticator to a user, instantiate a UserLogin instance which 
        /// will allow you to login to the account. After logging in, instantiate an AuthenticatorLinker 
        /// and use AuthenticatorLinker.AddAuthenticator() and AuthenticatorLinker.FinalizeAddAuthenticator() 
        /// to link a new authenticator."
        /// </summary>
        static void DemoAuthenticatorLinker()
        {
            Console.WriteLine("=== Demo 2: AuthenticatorLinker Workflow ===");
            Console.WriteLine("This demonstrates the complete process of linking a mobile authenticator.\n");

            Console.WriteLine("STEP-BY-STEP PROCESS:");
            Console.WriteLine("=====================\n");

            Console.WriteLine("Step 1: Login to Steam Account");
            Console.WriteLine("-------");
            Console.WriteLine("Note: The README mentions 'UserLogin' but this is outdated.");
            Console.WriteLine("The current implementation uses SteamKit2 for authentication.");
            Console.WriteLine("You need to:");
            Console.WriteLine("  - Use SteamKit2 to authenticate");
            Console.WriteLine("  - Create a SessionData object with AccessToken and RefreshToken");
            Console.WriteLine("  - See TestBed/Program.cs for a working example\n");
            Console.WriteLine("Code example:");
            Console.WriteLine(@"
  // Using SteamKit2 (separate library)
  var sessionData = new SessionData()
  {
      SteamID = authSession.SteamID.ConvertToUInt64(),
      AccessToken = pollResponse.AccessToken,
      RefreshToken = pollResponse.RefreshToken,
  };
");

            Console.WriteLine("\nStep 2: Instantiate AuthenticatorLinker");
            Console.WriteLine("-------");
            Console.WriteLine("Code example:");
            Console.WriteLine(@"
  AuthenticatorLinker linker = new AuthenticatorLinker(sessionData);
  
  // Optional: Add phone number if account doesn't have one
  linker.PhoneNumber = ""+1 1234567890"";
  linker.PhoneCountryCode = ""1"";  // Optional, defaults to account country
");

            Console.WriteLine("\nStep 3: Call AddAuthenticator()");
            Console.WriteLine("-------");
            Console.WriteLine("Code example:");
            Console.WriteLine(@"
  var result = await linker.AddAuthenticator();
  
  // Possible results:
  // - LinkResult.AwaitingFinalization: Success! Continue to next step
  // - LinkResult.MustProvidePhoneNumber: Need to add phone number
  // - LinkResult.AuthenticatorPresent: Account already has authenticator
  // - LinkResult.GeneralFailure: Something went wrong
");

            Console.WriteLine("\nStep 4: CRITICAL - Save LinkedAccount JSON");
            Console.WriteLine("-------");
            Console.WriteLine("⚠️  WARNING: This is the most important step!");
            Console.WriteLine("If you don't save this data, you will be locked out of your account!\n");
            Console.WriteLine("Code example:");
            Console.WriteLine(@"
  // Save the LinkedAccount to a file IMMEDIATELY after AddAuthenticator()
  string json = JsonConvert.SerializeObject(linker.LinkedAccount, Formatting.Indented);
  File.WriteAllText(""account.maFile"", json);
  
  Console.WriteLine(""SAVED! Keep this file safe!"");
");

            Console.WriteLine("\nStep 5: Call FinalizeAddAuthenticator()");
            Console.WriteLine("-------");
            Console.WriteLine("Code example:");
            Console.WriteLine(@"
  Console.WriteLine(""Enter SMS code sent to your phone: "");
  string smsCode = Console.ReadLine();
  
  var finalizeResult = await linker.FinalizeAddAuthenticator(smsCode);
  
  if (finalizeResult == FinalizeResult.Success)
  {
      Console.WriteLine(""Authenticator successfully linked!"");
      // Now you can use linker.LinkedAccount to generate codes
  }
");

            Console.WriteLine("\nStep 6: Generate Codes");
            Console.WriteLine("-------");
            Console.WriteLine("Code example:");
            Console.WriteLine(@"
  // Load your saved account data
  string json = File.ReadAllText(""account.maFile"");
  var account = JsonConvert.DeserializeObject<SteamGuardAccount>(json);
  
  // Generate a code
  string code = account.GenerateSteamGuardCode();
  Console.WriteLine(""Your code: "" + code);
");

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("For a complete working example, see: TestBed/Program.cs");
            Console.WriteLine(new string('=', 60));
        }

        /// <summary>
        /// Demo 3: Fetch and manage mobile confirmations
        /// README: "To fetch mobile confirmations, call SteamGuardAccount.FetchConfirmations(). 
        /// You can then call SteamGuardAccount.AcceptConfirmation and SteamGuardAccount.DenyConfirmation."
        /// </summary>
        static void DemoConfirmations()
        {
            Console.WriteLine("=== Demo 3: Mobile Confirmations ===");
            Console.WriteLine("This demonstrates how to fetch and manage mobile confirmations.\n");

            Console.WriteLine("STEP-BY-STEP PROCESS:");
            Console.WriteLine("=====================\n");

            Console.WriteLine("Step 1: Load your SteamGuardAccount");
            Console.WriteLine("-------");
            Console.WriteLine("Code example:");
            Console.WriteLine(@"
  // Load from saved .maFile
  string json = File.ReadAllText(""account.maFile"");
  var account = JsonConvert.DeserializeObject<SteamGuardAccount>(json);
");

            Console.WriteLine("\nStep 2: Fetch Confirmations");
            Console.WriteLine("-------");
            Console.WriteLine("Code example:");
            Console.WriteLine(@"
  var confirmations = await account.FetchConfirmations();
  
  if (confirmations == null || confirmations.Length == 0)
  {
      Console.WriteLine(""No confirmations pending."");
      return;
  }
  
  foreach (var conf in confirmations)
  {
      Console.WriteLine($""ID: {conf.ID}"");
      Console.WriteLine($""Type: {conf.ConfType}"");
      Console.WriteLine($""Creator: {conf.Creator}"");
      Console.WriteLine($""Description: {conf.Description}"");
  }
");

            Console.WriteLine("\nStep 3: Accept or Deny Confirmations");
            Console.WriteLine("-------");
            Console.WriteLine("Code example:");
            Console.WriteLine(@"
  // Accept a confirmation
  bool acceptSuccess = await account.AcceptConfirmation(confirmations[0]);
  if (acceptSuccess)
      Console.WriteLine(""Confirmation accepted!"");
  
  // Or deny a confirmation
  bool denySuccess = await account.DenyConfirmation(confirmations[0]);
  if (denySuccess)
      Console.WriteLine(""Confirmation denied!"");
");

            Console.WriteLine("\nStep 4: Accept Multiple Confirmations at Once");
            Console.WriteLine("-------");
            Console.WriteLine("Code example:");
            Console.WriteLine(@"
  bool success = await account.AcceptMultipleConfirmations(confirmations);
  if (success)
      Console.WriteLine(""All confirmations accepted!"");
");

            Console.WriteLine("\nNote: Confirmations are used for:");
            Console.WriteLine("  - Trade offers");
            Console.WriteLine("  - Market listings");
            Console.WriteLine("  - Account changes");
        }

        /// <summary>
        /// Demo 4: Load account from .maFile and generate code
        /// </summary>
        static void DemoLoadFromFile()
        {
            Console.WriteLine("=== Demo 4: Load Account from .maFile ===");
            Console.WriteLine("This demonstrates loading a saved authenticator account.\n");

            // Create a demo .maFile for demonstration
            var demoAccount = new SteamGuardAccount
            {
                AccountName = "demo_user",
                SharedSecret = "cnOgv/KdpLoP6Nbh0GMkXkPXALQ=",
                IdentitySecret = "identitySecretExample==",
                RevocationCode = "R12345",
                DeviceID = "android:demo-device-id",
                SerialNumber = "1234567890"
            };

            string demoFileName = "demo_account.maFile";

            try
            {
                // Save demo file
                string json = JsonConvert.SerializeObject(demoAccount, Formatting.Indented);
                File.WriteAllText(demoFileName, json);
                Console.WriteLine($"Created demo file: {demoFileName}");
                Console.WriteLine($"\nFile contents:\n{json}\n");

                // Load it back
                Console.WriteLine("Loading account from file...");
                string loadedJson = File.ReadAllText(demoFileName);
                var loadedAccount = JsonConvert.DeserializeObject<SteamGuardAccount>(loadedJson);

                Console.WriteLine($"✓ Successfully loaded account: {loadedAccount.AccountName}");
                Console.WriteLine($"✓ Revocation Code: {loadedAccount.RevocationCode}");
                Console.WriteLine($"✓ Device ID: {loadedAccount.DeviceID}");

                // Generate a code
                string code = loadedAccount.GenerateSteamGuardCode();
                Console.WriteLine($"\n✓ Generated Code: {code}");

                Console.WriteLine("\n⚠️  IMPORTANT: In a real scenario:");
                Console.WriteLine("  1. Keep your .maFile secure and backed up");
                Console.WriteLine("  2. Never share your SharedSecret or IdentitySecret");
                Console.WriteLine("  3. Save your RevocationCode - you'll need it to remove the authenticator");

                // Clean up demo file
                Console.WriteLine($"\nCleaning up demo file: {demoFileName}");
                File.Delete(demoFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
