using Microsoft.AspNetCore.DataProtection;

namespace BookingApi.Commons
{

    /// <summary>
    /// 
    /// A copy of the common methods defined in the main folder. Actually only decrypt methods is copied
    /// </summary>
    public class CommonMethods
    {

        public static string Decrypt(string payload, string secret)
        {
            // Get the path to %LOCALAPPDATA%\myapp-keys
            var destFolder = Path.Combine(
                Environment.GetEnvironmentVariable("LOCALAPPDATA")??"",
                "myapp-keys");

            // Instantiate the data protection system at this folder
            var dataProtectionProvider = DataProtectionProvider.Create(
                new DirectoryInfo(destFolder));

            var protector = dataProtectionProvider.CreateProtector(secret);

            payload = protector.Unprotect(payload);

            Console.WriteLine("Unprotected: " + payload + "\n\n");

            return payload;
        }
    }
}
