using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

namespace DiscordMultiTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "DCMTool v1.0.1 (by giantpreston)";

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
                @"                                                 (version 1.0.1 by giantpreston)"
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

            // Batch Token Validation
            string filePath = "tokens.txt";
            if (File.Exists(filePath))
            {
                var tokens = File.ReadAllLines(filePath);
                var validTokens = new StringBuilder();

                foreach (var token in tokens)
                {
                    if (string.IsNullOrWhiteSpace(token)) continue;
                    string result = await ValidateSingleToken(token.Trim());
                    if (result == "Good token")
                    {
                        validTokens.AppendLine(token.Trim());
                    }
                }

                if (validTokens.Length > 0)
                {
                    File.WriteAllText("valid_tokens.txt", validTokens.ToString());
                    Console.WriteLine("Valid tokens have been written to 'valid_tokens.txt'");
                }
                else
                {
                    Console.WriteLine("No valid tokens found.");
                }
            }
            else
            {
                Console.WriteLine("No 'tokens.txt' file found in the current directory.");
            }
        }

        private static async Task<string> ValidateSingleToken(string token)
        {
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
                        return "Good token";
                    case 401:
                        return "Bad token";
                    case 403:
                        return "Locked token";
                    case 429:
                        return "Rate limited, slow down!";
                    default:
                        return $"Unknown error: {statusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Error occurred: {ex.Message}";
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
                    Console.WriteLine($"Email: {root.GetProperty("email").GetString()}");
                }
                else
                {
                    Console.WriteLine("\nFailed to fetch user info. Please ensure the token is valid.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError occurred: {ex.Message}");
            }
        }
    }
}
