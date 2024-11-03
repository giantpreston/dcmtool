using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace DiscordMultiTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "DCMTool v1.0.0 (by giantpreston)";

            while (true)
            {
                Console.Clear();
                DisplayMenu();
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ValidateToken();
                        break;
                    case "2":
                        await SendMessageToWebhook(); 
                        break;
                    case "3":
                        await DeleteWebhook();
                        break;
                    case "4":
                        await GetTokenInfo();
                        break;
                    case "5":
                        Console.WriteLine("Exiting the program...");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress any key to return to the main menu...");
                Console.ReadKey();
            }
        }

        private static void DisplayMenu()
        {
            ConsoleColor[] gradientColors = {
                ConsoleColor.Cyan,
                ConsoleColor.Cyan,
                ConsoleColor.Cyan,
                ConsoleColor.Cyan,
                ConsoleColor.Blue,
                ConsoleColor.Blue,
                ConsoleColor.DarkBlue,
                ConsoleColor.DarkBlue,
                ConsoleColor.DarkBlue,
                ConsoleColor.DarkBlue
            };

            string[] asciiArt = new string[] {
                @" ________  ________  _____ ______   _________  ________  ________  ___          ",
                @"|\   ___ \|\   ____\|\   _ \  _   \|\___   ___\\   __  \|\   __  \|\  \         ",
                @"\ \  \_|\ \ \  \___|\ \  \\\__\ \  \|___ \  \_\ \  \|\  \ \  \|\  \ \  \        ",
                @" \ \  \ \\ \ \  \    \ \  \\|__| \  \   \ \  \ \ \  \\\  \ \  \\\  \ \  \       ",
                @"  \ \  \_\\ \ \  \____\ \  \    \ \  \   \ \  \ \ \  \\\  \ \  \\\  \ \  \____  ",
                @"   \ \_______\ \_______\ \__\    \ \__\   \ \__\ \ \_______\ \_______\ \_______\",
                @"    \|_______|\|_______|\|__|     \|__|    \|__|  \|_______|\|_______|\|_______|",
                @"                                                                                ",
                @"                                                                                ",
                @"                                                 (version 1.0.0 by giantpreston)"
            };

            for (int i = 0; i < asciiArt.Length; i++)
            {
                Console.ForegroundColor = gradientColors[i % gradientColors.Length];
                Console.WriteLine(asciiArt[i]);
            }

            Console.ResetColor(); 
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==============================");
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Validate Token");
            Console.WriteLine("2. Send Message to Webhook");
            Console.WriteLine("3. Delete Webhook");
            Console.WriteLine("4. Get Token Info");
            Console.WriteLine("5. Exit");
            Console.Write("Please enter your choice: ");
            Console.ResetColor(); 
        }

        private static async Task ValidateToken()
        {
            Console.Clear();
            Console.WriteLine("==== Token Validation ====");
            Console.Write("Enter your Discord user token: ");
            var token = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("\nNo token provided.");
                return;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", token);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.41 Safari/537.36");

            try
            {
                var response = await client.GetAsync("https://discord.com/api/v9/users/@me/library");
                var statusCode = (int)response.StatusCode;

                switch (statusCode)
                {
                    case 200:
                        Console.WriteLine("\nGood token");
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case 401:
                        Console.WriteLine("\nBad token");
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case 403:
                        Console.WriteLine("\nLocked token");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case 429:
                        Console.WriteLine("\nRate limited, slow down!");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    default:
                        Console.WriteLine($"\nUnknown error: {statusCode}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred while validating the token: {ex.Message}");
            }
            finally
            {
                Console.ResetColor(); 
            }
        }

        private static async Task SendMessageToWebhook()
        {
            Console.Clear();
            Console.WriteLine("==== Send Message via Webhook ====");
            Console.Write("Enter the webhook URL: ");
            var webhookUrl = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(webhookUrl) || !webhookUrl.StartsWith("https://discord.com/api/webhooks/"))
            {
                Console.WriteLine("\nInvalid URL. Please provide a valid Discord webhook URL.");
                return;
            }

            Console.Write("Enter the message content: ");
            var messageContent = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                Console.WriteLine("\nInvalid message content. Please provide a valid message.");
                return;
            }

            try
            {
                await SendMs(messageContent, webhookUrl); 
                Console.WriteLine("Message sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nFailed to send message: {ex.Message}");
            }
        }

        private static async Task SendMs(string message, string webhook)
        {
            using var client = new HttpClient();
            string payload = "{\"content\": \"" + message + "\"}";
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(webhook, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send message: {response.StatusCode}");
            }
        }

        private static async Task DeleteWebhook()
        {
            Console.Clear();
            Console.WriteLine("==== Delete Webhook ====");
            Console.Write("Enter the webhook URL to delete: ");
            var webhookUrl = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(webhookUrl) || !webhookUrl.StartsWith("https://discord.com/api/webhooks/"))
            {
                Console.WriteLine("\nInvalid URL. Please provide a valid Discord webhook URL.");
                return;
            }

            using var client = new HttpClient();
            var response = await client.DeleteAsync(webhookUrl);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("\nWebhook deleted successfully.");
            }
            else
            {
                Console.WriteLine($"\nFailed to delete webhook: {response.StatusCode}");
            }
        }

private static async Task GetTokenInfo()
{
    Console.Clear();
    Console.WriteLine("==== Token Information ====");
    Console.Write("Enter your Discord user token: ");
    var token = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(token))
    {
        Console.WriteLine("\nNo token provided.");
        return;
    }

    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", token);

    try
    {
        var response = await client.GetAsync("https://discord.com/api/v9/users/@me");
        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
        // Only uncomment these if you need the Raw JSON Response sent by the Discord API for debugging.
            // Console.WriteLine("\nRaw JSON Response:");
            // Console.WriteLine(jsonResponse);

            // Parse the JSON directly
            using var jsonDoc = JsonDocument.Parse(jsonResponse);
            var root = jsonDoc.RootElement;

            Console.WriteLine("\n==== User Information ====");
            Console.WriteLine($"ID: {root.GetProperty("id").GetString()}");
            Console.WriteLine($"Username: {root.GetProperty("username").GetString()}");
            Console.WriteLine($"Global Name: {root.GetProperty("global_name").GetString()}");
            Console.WriteLine($"Discriminator: {root.GetProperty("discriminator").GetString()}");
            Console.WriteLine($"Avatar Hash: {root.GetProperty("avatar").GetString()}");
            Console.WriteLine($"Banner Hash: {root.GetProperty("banner").GetString()}");
            Console.WriteLine($"Accent Color: {(root.TryGetProperty("accent_color", out var accentColor) ? $"#{accentColor.GetInt32():X6}" : "N/A")}");
            Console.WriteLine($"Banner Color: {root.GetProperty("banner_color").GetString()}");
            Console.WriteLine($"MFA Enabled: {root.GetProperty("mfa_enabled").GetBoolean()}");
            Console.WriteLine($"Locale: {root.GetProperty("locale").GetString()}");
            Console.WriteLine($"Premium Type: {root.GetProperty("premium_type").GetInt32()}");
            Console.WriteLine($"Email: {root.GetProperty("email").GetString()}");
            Console.WriteLine($"Verified: {root.GetProperty("verified").GetBoolean()}");
            Console.WriteLine($"Phone: {root.GetProperty("phone").GetString()}");
            Console.WriteLine($"NSFW Allowed: {root.GetProperty("nsfw_allowed").GetBoolean()}");
            Console.WriteLine($"Bio: {root.GetProperty("bio").GetString()}");
            Console.WriteLine($"Authenticator Types: {string.Join(", ", root.GetProperty("authenticator_types").EnumerateArray().Select(x => x.GetInt32().ToString()))}");
        }
        else
        {
            Console.WriteLine($"\nFailed to retrieve token info: {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nAn error occurred while retrieving token info: {ex.Message}");
    }
}
    }
}
