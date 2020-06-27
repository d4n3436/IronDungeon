using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IronDungeon.API;
using Newtonsoft.Json;

namespace IronDungeon
{
    internal class Program
    {
        // From: https://github.com/Eigenbahn/ai-dungeon-cli/blob/master/ai_dungeon_cli/opening-utf8.txt
        private const string Opening = @"
 ▄▄▄       ██▓   ▓█████▄  █    ██  ███▄    █   ▄████ ▓█████  ▒█████   ███▄    █
▒████▄    ▓██▒   ▒██▀ ██▌ ██  ▓██▒ ██ ▀█   █  ██▒ ▀█▒▓█   ▀ ▒██▒  ██▒ ██ ▀█   █
▒██  ▀█▄  ▒██▒   ░██   █▌▓██  ▒██░▓██  ▀█ ██▒▒██░▄▄▄░▒███   ▒██░  ██▒▓██  ▀█ ██▒
░██▄▄▄▄██ ░██░   ░▓█▄   ▌▓▓█  ░██░▓██▒  ▐▌██▒░▓█  ██▓▒▓█  ▄ ▒██   ██░▓██▒  ▐▌██▒
 ▓█   ▓██▒░██░   ░▒████▓ ▒▒█████▓ ▒██░   ▓██░░▒▓███▀▒░▒████▒░ ████▓▒░▒██░   ▓██░
 ▒▒   ▓▒█░░▓      ▒▒▓  ▒ ░▒▓▒ ▒ ▒ ░ ▒░   ▒ ▒  ░▒   ▒ ░░ ▒░ ░░ ▒░▒░▒░ ░ ▒░   ▒ ▒
  ▒   ▒▒ ░ ▒ ░    ░ ▒  ▒ ░░▒░ ░ ░ ░ ░░   ░ ▒░  ░   ░  ░ ░  ░  ░ ▒ ▒░ ░ ░░   ░ ▒░
  ░   ▒    ▒ ░    ░ ░  ░  ░░░ ░ ░    ░   ░ ░ ░ ░   ░    ░   ░ ░ ░ ▒     ░   ░ ░
      ░  ░ ░        ░       ░              ░       ░    ░  ░    ░ ░           ░
                  ░

";

        private const string Help1 = @"About AI Dungeon

AI Dungeon is a first of its kind AI generated text adventure.
Using a 1.5B parameter machine learning model called GPT-2, AI Dungeon generates the story and results of your actions as you play in this virtual world.
Unlike virtually every other game in existence, you are not limited by the imagination of the developer in what you can do.
Anything you can express in language can be your action and the AI dungeon master will decide how the world responds to your actions.";

        private const string Help2 = @"Things You Should Know

- Anything is possible. Literally anything. You can type any action you think of and the AI will try to generate a story off of it. It's not always perfect but you'll be surprised at what's possible.
- AI is hard. This is a game that's unlike anything you've ever played before. It uses cutting edge AI tech to generate the responses. That being said there may be weird quirks that you notice.
  We're always working on improving the AI and keeping up with the research, but ultimately AI Dungeon will get better as we get new AI research advancements.
- There are a lot of ways you can improve your adventure when the AI struggles. Use features like undo and alter to help keep the AI on track. Read more below to learn about how these different commands work.";

        private const string Help3 = @"Input Options

DO: An action you want to take in the story. Always start with an action word, ex. ""Search for the hidden treasure.""
SAY: You can use this input setting to speak, ex. ""Leave them alone!""
STORY: Use this input setting to stop the AI from putting ""You"" in front of your input.";

        private const string Help4 = @"Commands

Undo: Undo the last action. This is helpful if you get a nonsensical response or the game starts looping. Just undo and try something else.
Redo: Did you undo on accident? Just hit the redo button to get it back.
Alter: Edits the last response from the AI. This is helpful if you get a nonsensical response and want to fix what the AI generated.
Remember: Edits the memory context. The context is fed into the model at each step so the AI will always have that information ""pinned"".
Retry: Retries the last action and generates a new response. Very helpful when you want to explore the multiverse.";

        private const string Help5 = @"Tips

- Try using new words often, the AI gets more creative with variety.
- Remember to start a ""do"" input with a verb, ex: Attack the orc
- Use the undo command to delete your last input along with the AI's response.
- Long sentences for actions are no problem! Get creative!
- Want more story to generate? Just press enter without typing in an action.
- Use the remember command or the pin button to edit the story context that the AI always remembers.
- Use the alter command to directly change the AI's response to your input if you want to make some changes to it.
- For best results, use second person. For example, ""You fly to the moon"" instead of ""I fly to the moon"".";

        private static readonly string[] HelpList = new string[] { Help1, Help2, Help3, Help4, Help5 };
        private const string CustomPrompt = "Enter a prompt that describes who you are and the first couple sentences of where you start out ex:\n'You are a knight in the kingdom of Larion. You are hunting the evil dragon who has been terrorizing the kingdom. You enter the forest searching for the dragon and see'";
        private const string ConfigFile = "config.json";
        private readonly static string[] LoginOptions = { "Sign up / Register", "Use an Email and password", "Use a token", "Exit" };
        private readonly static string[] MenuOptions = { "Create a new game", "Continue a game", "Edit the configuration", "Help", "About", "Exit" };
        private readonly static string[] ConfigOptions = { "Edit the token", "Slow typing Animation (Current: ", "Logout", "Return to the menu" };
        private static Config UserConfig;
        private static AIDungeon API;
        private static bool HasToken = false;
        private static bool Exit = false;
        private static readonly Random Rng = new Random();

        //static void Main() //string[] args
        //{
        //    MainAsync().GetAwaiter().GetResult();
        //}

        private static async Task Main()
        {
            Console.Clear();
            Console.Write(Opening);

            LoadConfig();

            await Task.Delay(5000);

            bool isFirst;
            if (!HasToken)
            {
                isFirst = true;
                await LoginAsync();
                Console.Clear();
            }
            else
            {
                DeleteLastLine();
                isFirst = false;
            }

            while (!Exit)
            {
                int Option = OptionSelection("Enter an option:", MenuOptions, isFirst, isFirst);
                isFirst = true;
                switch (Option)
                {
                    case 1:
                        await CreateNewGameAsync();
                        break;

                    case 2:
                        await ContinueGameAsync();
                        break;

                    case 3:
                        await EditConfigAsync();
                        break;

                    case 4:
                        DisplayHelp();
                        break;

                    case 5:
                        About();
                        break;

                    case 6:
                        Exit = true;
                        break;
                }
            }
            Console.Write("\nBye bye!");
            await Task.Delay(4000);
        }

        private static async Task LoginAsync()
        {
            while (!HasToken)
            {
                string email;
                string password;
                int loginOption = OptionSelection("Enter an option to log in:", LoginOptions);
                switch (loginOption)
                {
                    case 1:
                        Console.Write("\nEmail: ");
                        email = Console.ReadLine();
                        Console.Write("Username: ");
                        string username = Console.ReadLine();
                        Console.Write("Password: ");
                        password = ReadPassword();
                        Console.WriteLine("\nLoading...");
                        API = new AIDungeon();
                        try
                        {
                            var response = await API.RegisterAsync(email, username, password);
                            if (response.Errors != null)
                            {
                                Console.Write($"En error occurred: {response.Errors[0].Message}");
                            }
                            else
                            {
                                API.Token = response.Data.CreateAccount.AccessToken;
                                Console.Write("Registered Successfully. You should check your E-Mail to verify your account.");
                                HasToken = true;
                            }
                        }
                        catch (HttpRequestException e)
                        {
                            Console.WriteLine($"An error occurred: {e.Message}");
                        }
                        await Task.Delay(5000);
                        break;

                    case 2:
                        Console.Write("\nEmail: ");
                        email = Console.ReadLine();
                        Console.Write("Password: ");
                        password = ReadPassword();
                        Console.WriteLine("\nLogging in...");
                        API = new AIDungeon();
                        LoginResponse loginResponse;
                        try
                        {
                            loginResponse = await API.LoginAsync(email, password);
                        }
                        catch (HttpRequestException e)
                        {
                            Console.WriteLine($"An error occurred: {e.Message}");
                        }
                        await Task.Delay(5000);
                        break;

                    case 3:
                        string token;
                        while (true)
                        {
                            Console.Write("\nEnter a token (Enter 'r' to return): ");
                            token = Console.ReadLine()?.Trim();
                            if (string.IsNullOrWhiteSpace(token))
                            {
                                Console.WriteLine("You must enter a token.");
                            }
                            else if (token.ToLowerInvariant() == "r")
                            {
                                break;
                            }
                            else if (!Guid.TryParse(token, out _))
                            {
                                Console.WriteLine("Invalid token.");
                            }
                            else
                            {
                                API = new AIDungeon(token);
                                HasToken = true;
                                break;
                            }
                        }
                        break;

                    case 4:
                        Environment.Exit(0);
                        break;
                }

                // Create config file
                if (HasToken)
                {
                    UserConfig = new Config(API.Token, true);
                    SaveConfig();
                }
            }
        }

        private static async Task CreateNewGameAsync()
        {
            Console.WriteLine("\nLoading...");
            ScenarioResponse modeList = null;
            try
            {
                modeList = await API.GetScenarioAsync(AIDungeon.AllScenarios);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
            if (!IsValidResponse(modeList))
            {
                return;
            }

            SortedList<string, uint> modes = new SortedList<string, uint>();
            foreach (var mode in modeList.Data.Content.Options)
            {
                modes.Add(mode.Title, uint.Parse(mode.Id.Substring(9), CultureInfo.InvariantCulture)); // "scenario:xxxxxx"
            }

            var tempModes = new List<string>(modes.Keys) { "Exit to menu" };
            int modeOption = OptionSelection("Select a setting...", tempModes);
            if (modeOption == tempModes.Count)
            {
                // Exit
                return;
            }
            modeOption--;

            List<History> historyList;
            uint id;
            if (modes.Keys[modeOption] != "custom")
            {
                ScenarioResponse characterList = null;
                try
                {
                    characterList = await API.GetScenarioAsync(modes.Values[modeOption]);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"An error occurred: {e.Message}");
                }
                if (!IsValidResponse(characterList))
                {
                    return;
                }

                SortedList<string, uint> characters = new SortedList<string, uint>();
                foreach (var character in characterList.Data.Content.Options)
                {
                    characters.Add(character.Title, uint.Parse(character.Id.Substring(9), CultureInfo.InvariantCulture)); // "scenario:xxxxxx"
                }

                var tempCharacters = new List<string>(characters.Keys) { "Exit to menu" };
                int characterOption = OptionSelection("Select a character...", tempCharacters);
                if (characterOption == tempCharacters.Count)
                {
                    // Exit
                    return;
                }

                characterOption--;

                string name;
                while (true)
                {
                    Console.Write("\n\nEnter the character name: ");
                    name = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        break;
                    }
                    Console.WriteLine("You must specify a name.");
                }

                Console.WriteLine($"Creating a new adventure with the mode: {modes.Keys[modeOption]}, and the character: {characters.Keys[characterOption]}...");

                ScenarioResponse scenario = null;
                try
                {
                    scenario = await API.GetScenarioAsync(characters.Values[characterOption]);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"An error occurred: {e.Message}");
                }
                if (!IsValidResponse(scenario))
                {
                    return;
                }

                CreationResponse response = null;
                try
                {
                    response = await API.CreateAdventureAsync(characters.Values[characterOption], scenario.Data.Content.Prompt.Replace("${character.name}", name));
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"An error occurred: {e.Message}");
                }
                if (!IsValidResponse(response))
                {
                    return;
                }
                if (response.Data.AdventureInfo == null)
                {
                    Console.Write("Seems that the access token is invalid.\nPlease edit the token in the menu/Edit config, or try logging out and logging in.\n");
                    Console.Write("\nPress any key to continue...");
                    Console.ReadKey();
                    return;
                }

                id = uint.Parse(response.Data.AdventureInfo.ContentId, CultureInfo.InvariantCulture); // "adventure:xxxxxxx"
                historyList = response.Data.AdventureInfo.HistoryList; // result.Data.AdventureInfo.HistoryList.Count - 1
            }
            else
            {
                string customText;
                Console.WriteLine($"\n{CustomPrompt}\n");
                while (true)
                {
                    Console.Write("Prompt: ");
                    customText = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(customText))
                    {
                        Console.WriteLine("You must enter a text.");
                    }
                    else if (customText.Length > 140)
                    {
                        Console.WriteLine($"Text length must be lower than 140. (Current: {customText.Length})");
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine("Creating a new adventure with the custom prompt...");

                CreationResponse adventure = null;
                try
                {
                    adventure = await API.CreateAdventureAsync(modes.Values[modeOption]);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"An error occurred: {e.Message}");
                }
                if (!IsValidResponse(adventure))
                {
                    return;
                }

                ActionResponse action = null;
                AdventureInfoResponse response = null;
                try
                {
                    action = await API.RunActionAsync(uint.Parse(adventure.Data.AdventureInfo.ContentId, CultureInfo.InvariantCulture), ActionType.Describe, customText);

                    // wait a few seconds to generate the story
                    await Task.Delay(6000);
                    response = await API.GetAdventureAsync(uint.Parse(adventure.Data.AdventureInfo.ContentId, CultureInfo.InvariantCulture));
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"An error occurred: {e.Message}");
                }
                if (!IsValidResponse(action))
                {
                    return;
                }
                if (!IsValidResponse(response))
                {
                    return;
                }

                id = uint.Parse(response.Data.Content.Id.Substring(10), CultureInfo.InvariantCulture); // "adventure:xxxxxxx"
                historyList = response.Data.Content.HistoryList; // Action.Data.UserAction.HistoryList.Count - 1
            }

            await ProcessAdventureAsync(id, historyList);
        }

        private static async Task ContinueGameAsync()
        {
            Console.Write("\nLoading adventure list...");

            // I don't know if this works but I'll leave it here anyways.
            await API.RefreshAdventureListAsync();

            AdventureListResponse response = null;
            try
            {
                response = await API.GetAdventureListAsync();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
            if (!IsValidResponse(response))
            {
                return;
            }
            if (response.Data.User == null)
            {
                Console.WriteLine("Seems that the access token is invalid.");
                Console.WriteLine("Please edit the token in the menu/Edit config, or try logging out and logging in.");
                Console.Write("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var adventureList = response.Data.User.ContentList;
            if (adventureList.Count == 0)
            {
                Console.WriteLine("You do not have any adventures.");
                Console.Write("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            List<string> options = new List<string>();
            foreach (var adventure in adventureList)
            {
                options.Add($"{adventure.Title} (ID: {adventure.ContentId})");
            }
            options.Add("Exit to menu");
            int option = OptionSelection("Enter an option:", options);
            if (option == options.Count)
            {
                // Exit
                return;
            }
            Console.WriteLine("\nLoading adventure...");
            option--;
            uint id = uint.Parse(adventureList[option].ContentId, CultureInfo.InvariantCulture);

            AdventureInfoResponse adventureInfo = null;
            try
            {
                adventureInfo = await API.GetAdventureAsync(id);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
            if (!IsValidResponse(adventureInfo))
            {
                return;
            }

            var historyList = adventureInfo.Data.Content.HistoryList;
            await ProcessAdventureAsync(id, historyList);
        }

        private static async Task ProcessAdventureAsync(uint adventureId, List<History> historyList)
        {
            ActionResponse actionResponse;
            AdventureInfoResponse adventureResponse;
            ActionType action;
            string text;
            uint lastActionId = 0;

            // Seems that in a custom adventure the last 2 history will always be empty, so we need to filter that.
            historyList.RemoveAll(x => string.IsNullOrEmpty(x.Text));

            // This should prevent any errors
            if (historyList.Count == 0)
            {
                Console.WriteLine("The adventure is empty, strange...");
                Console.Write("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            string lastHistory = historyList[historyList.Count - 1].Text;

            Console.Clear();
            if (historyList.Count > 1)
            {
                string previousHistory = "";
                historyList.RemoveAt(historyList.Count - 1);
                foreach (var history in historyList)
                {
                    previousHistory += history.Text;
                }
                Console.Write(previousHistory);
                await Task.Delay(5000);
            }

            TypeLine(lastHistory.Trim() + "\n");

            TypeLine("Quick help: Use \"Do\", \"Say\" or \"Describe\" at the start of your input to do an action, say something, or make progress on the story.");

            TypeLine("Other commands:\n");
            TypeLine("\"Undo\": Undo the last action.");
            TypeLine("\"Redo\": Redo the last action.");
            TypeLine("\"Alter\": Alter the last action.");
            TypeLine("\"Remember\": Edit the memory context.");
            TypeLine("\"Retry\": Retry the last action and generate a new response.\n");
            TypeLine("Undo, Redo and Retry do not require any input.\n");
            TypeLine("See the help in the menu for more info.");

            TypeLine("Enter \"quit\" or \"exit\" to exit.");

            while (true)
            {
                Console.Write("\n> ");
                text = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    action = ActionType.Continue;
                }
                else
                {
                    if (text.ToLowerInvariant() == "quit" || text.ToLowerInvariant() == "exit")
                    {
                        break;
                    }

                    // Split the text in two, the first part should be the input type, and the second part, the text.
                    string[] splitText = text.Split(' ', 2);
                    if (Enum.TryParse(splitText[0], true, out action))
                    {
                        // if only the command is passed
                        if (splitText.Length == 1)
                        {
                            // if the command requires an input
                            if (action == ActionType.Remember || action == ActionType.Alter)
                            {
                                Console.WriteLine("You must enter a text when using Remember / Alter commands.");
                                continue;
                            }
                        }
                        else
                        {
                            // else, actionType will be one of the valid input types
                            // and text will be the second part (the rest) of the split text.
                            text = splitText[1];
                        }
                    }
                    else
                    {
                        // if the parse fails, keep the text and set the input type to Do (the default).
                        action = ActionType.Do;
                    }
                }
                //TypeLine("Generating story...");

                if (action == ActionType.Alter)
                {
                    lastActionId = uint.Parse(historyList[historyList.Count - 1].Id, CultureInfo.InvariantCulture);
                }
                actionResponse = null;
                adventureResponse = null;
                try
                {
                    actionResponse = await API.RunActionAsync(adventureId, action, text, lastActionId);

                    // wait a few seconds to generate the story
                    await Task.Delay(6000);
                    adventureResponse = await API.GetAdventureAsync(adventureId);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"An error occurred: {e.Message}");
                }
                if (!IsValidResponse(actionResponse))
                {
                    break;
                }
                if (!IsValidResponse(adventureResponse))
                {
                    break;
                }

                historyList = adventureResponse.Data.Content.HistoryList;

                string textToShow = historyList[historyList.Count - 1].Text;
                if (action != ActionType.Continue && action != ActionType.Undo && action != ActionType.Redo && action != ActionType.Alter)
                {
                    textToShow = historyList[historyList.Count - 2].Text + textToShow;
                }
                TypeLine(textToShow);
            }
        }

        private static async Task EditConfigAsync()
        {
            bool exit = false;
            while (!exit)
            {
                List<string> options = new List<string>(ConfigOptions)
                {
                    [1] = $"Slow typing Animation (Current: {UserConfig.SlowTyping})"
                };
                int option = OptionSelection("Enter an option:", options);
                switch (option)
                {
                    case 1:
                        string token;
                        while (true)
                        {
                            Console.Write("\nEnter the new token (Enter 'r' to return): ");
                            token = Console.ReadLine()?.Trim();
                            if (string.IsNullOrWhiteSpace(token))
                            {
                                Console.WriteLine("You must enter a token.");
                            }
                            else if (token.ToLowerInvariant() == "r")
                            {
                                break;
                            }
                            else if (!Guid.TryParse(token, out _))
                            {
                                Console.WriteLine("Invalid token.");
                            }
                            else
                            {
                                UserConfig.Token = token;
                                API = new AIDungeon(UserConfig.Token);
                                SaveConfig();
                                break;
                            }
                        }
                        break;

                    case 2:
                        UserConfig.SlowTyping = !UserConfig.SlowTyping;
                        SaveConfig();
                        break;

                    case 3:
                        DeleteConfig();
                        HasToken = false;
                        await LoginAsync();
                        exit = true;
                        break;

                    case 4:
                        exit = true;
                        break;
                }
            }
        }

        private static void LoadConfig()
        {
            if (!File.Exists(ConfigFile))
            {
                return;
            }
            try
            {
                string json = File.ReadAllText(ConfigFile);
                UserConfig = JsonConvert.DeserializeObject<Config>(json);
                API = new AIDungeon(UserConfig.Token);
                HasToken = true;
                Console.Write("Configuration loaded.");
            }
            catch
            {
                Console.Write("Unable to load the config.");
            }
        }

        private static void SaveConfig()
        {
            try
            {
                File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(UserConfig));
            }
            catch (IOException e)
            {
                Console.WriteLine($"\nAn error occurred while saving the config: {e.Message}");
                Console.WriteLine("The config will not be saved on exit.");
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nAn error occurred: {e.Message}");
                Console.WriteLine("The config will not be saved on exit.");
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private static void DeleteConfig()
        {
            // Try to delete the config file
            if (!File.Exists(ConfigFile))
            {
                return;
            }
            try
            {
                File.Delete(ConfigFile);
            }
            catch
            {
            }
        }

        private static bool IsValidResponse(IResponse response)
        {
            if (response == null)
            {
                Console.Write("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }
            if (response.Errors != null)
            {
                Console.WriteLine($"En error occurred: {response.Errors[0].Message}");
                Console.Write("Press any key to continue...");
                Console.ReadKey();
                return false;
            }
            return true;
        }

        private static int OptionSelection(string prompt, IList<string> options, bool clearScreen = true, bool printOpening = true)
        {
            if (clearScreen)
            {
                Console.Clear();
            }
            if (printOpening)
            {
                Console.Write(Opening);
            }
            string text = $"{prompt}\n\n";
            for (int i = 0; i < options.Count; i++)
            {
                text += $"{i + 1}. {options[i]}\n";
            }
            Console.WriteLine(text);
            Console.Write("Option: ");
            while (true)
            {
                string line = Console.ReadLine();
                if (int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out int option))
                {
                    if (option >= 1 && option <= options.Count)
                    {
                        return option;
                    }
                }
                DeleteLastLine();
                Console.SetCursorPosition(8 + line.Length, Console.CursorTop - 1);
                for (int i = 0; i < line.Length; i++)
                {
                    DeleteLastChar();
                }
                /*
                if (options.Count > 9)
                {
                    // code above
                }
                else
                {
                    option = Console.ReadKey().KeyChar - '0';
                    if (option >= 1 && option <= options.Count)
                    {
                        return option;
                    }
                    DeleteLastChar();
                }
                */
            }
        }

        private static void DisplayHelp()
        {
            Paginate(HelpList);
        }

        private static void Paginate(string[] list)
        {
            int index = 0;
            while (true)
            {
                Console.Clear();
                Console.Write(Opening);
                Console.WriteLine(list[index]);

                Console.WriteLine($"\nAI Dungeon Help - Page {index + 1} of {list.Length}");
                Console.WriteLine("\nUse the Right and Left key to change the page.");
                Console.Write("Use the Esc key to exit the help.");
                while (true)
                {
                    var input = Console.ReadKey().Key;
                    if (input == ConsoleKey.LeftArrow && index != 0)
                    {
                        index--;
                        break;
                    }
                    else if (input == ConsoleKey.RightArrow && index != list.Length - 1)
                    {
                        index++;
                        break;
                    }
                    else if (input == ConsoleKey.Escape)
                    {
                        return;
                    }
                    else
                    {
                        DeleteLastChar();
                    }
                }
            }
        }

        private static void About()
        {
            Console.Clear();
            Console.Write(Opening);

            TypeLine("IronDungeon - An AI Dungeon CLI client and API.\n");
            TypeLine("Made by d4n (d4n#9385), for personal use only.\n");
            TypeLine("Web app API by aidungeon.io\n");
            Type("Press any key to continue...");
            Console.ReadKey();
        }

        private static void Type(string line, int minDelay = 25, int maxDelay = 50)
        {
            if (UserConfig.SlowTyping)
            {
                foreach (char character in line)
                {
                    Console.Write(character);
                    Thread.Sleep(Rng.Next(minDelay, maxDelay));
                }
            }
            else
            {
                Console.Write(line);
            }
        }

        private static void TypeLine(string line, int minDelay = 25, int maxDelay = 50)
        {
            Type(line, minDelay, maxDelay);
            Console.WriteLine();
        }

        private static void DeleteLastChar()
        {
            Console.Write("\b \b");
        }

        private static void DeleteLastLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        private static string ReadPassword()
        {
            string password = string.Empty;
            while (true)
            {
                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return password;
                }
                password += keyInfo.KeyChar;
            }
        }
    }
}