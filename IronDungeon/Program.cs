using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IronDungeon.API;
using Newtonsoft.Json;

namespace IronDungeon
{
    internal class Program
    {
        // From: https://github.com/Eigenbahn/ai-dungeon-cli/blob/master/ai_dungeon_cli/opening-utf8.txt
        private const string Splash = @"
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

Retry: Premium users can hit this button to retry the last action to generate a new response.Very helpful when you want to explore the multiverse.";

        private const string Help5 = @"Tips

- Try using new words often, the AI gets more creative with variety.
- Remember to start a ""do "" input with a verb, ex: Attack the orc
- Use the undo command to delete your last input along with the AI's response.
- Long sentences for actions are no problem! Get creative!
- Want more story to generate ? Just press enter without typing in an action.
- Use the remember command or the pin button to edit the story context that the AI always remembers.
- Use the alter command to directly change the AI's response to your input if you want to make some changes to it.
- For best results, use second person. For example, ""You fly to the moon"" instead of ""I fly to the moon"".";

        private static readonly List<string> HelpList = new List<string> { Help1, Help2, Help3, Help4, Help5 };
        private const string CustomPrompt = "Enter a prompt that describes who you are and the first couple sentences of where you start out ex:\n'You are a knight in the kingdom of Larion. You are hunting the evil dragon who has been terrorizing the kingdom. You enter the forest searching for the dragon and see'";
        private const string ConfigFile = "config.json";
        private readonly static string[] LoginOptions = { "Sign up / Register", "Use an Email and password", "Use a token", "Exit" };
        private readonly static string[] MenuOptions = { "Create a new game", "Continue a game", "Edit the configuration", "Help", "Exit" };
        private readonly static string[] ConfigOptions = { "Edit the token", "Slow typing Animation (Current: ", "Logout", "Exit to menu" };
        private static Config UserConfig;
        private static AIDungeon API;
        private static bool HasToken = false;
        private static bool Exit = false;
        private static bool ExitToMenu = false;
        private static readonly Random rng = new Random();

        //static void Main() //string[] args
        //{
        //    MainAsync().GetAwaiter().GetResult();
        //}

        private static async Task Main()
        {
            Console.Write(Splash);
            if (File.Exists(ConfigFile))
            {
                LoadConfig();
            }
            Thread.Sleep(5000);
            if (!HasToken)
            {
                Login();
            }
            Console.Clear();

            while (!Exit)
            {
                int Option = OptionSelection("Enter an option:", MenuOptions, true);
                switch (Option)
                {
                    case 1:
                        await CreateNewGameAsync();
                        break;

                    case 2:
                        await ContinueGameAsync();
                        break;

                    case 3:
                        EditConfig();
                        break;

                    case 4:
                        Help();
                        break;

                    case 5:
                        Exit = true;
                        break;
                }
            }
            Console.Write("\n\nBye bye!");
            Thread.Sleep(4000);
        }

        private static async Task CreateNewGameAsync()
        {
            Console.WriteLine("\nLoading...");

            var ModeList = await API.GetScenarioAsync(AIDungeon.AllScenarios);
            if (!IsValidResponse(ModeList))
            {
                return;
            }
            SortedList<string, uint> Modes = new SortedList<string, uint>();
            foreach (var Mode in ModeList.Data.Content.Options)
            {
                Modes.Add(Mode.Title, uint.Parse(Mode.Id.Substring(9))); // "scenario:xxxxxx"
            }
            var TempModes = new List<string>(Modes.Keys) { "Exit to menu" };
            int ModeOption = OptionSelection("Select a setting...", TempModes);
            if (ModeOption == TempModes.Count)
            {
                // Exit
                return;
            }
            ModeOption--;

            string TextToShow;
            uint ID;
            if (Modes.Keys[ModeOption] != "custom")
            {
                var CharacterList = await API.GetScenarioAsync(Modes.Values[ModeOption]);
                if (!IsValidResponse(CharacterList))
                {
                    return;
                }
                SortedList<string, uint> Characters = new SortedList<string, uint>();
                foreach (var Character in CharacterList.Data.Content.Options)
                {
                    Characters.Add(Character.Title, uint.Parse(Character.Id.Substring(9))); // "scenario:xxxxxx"
                }
                var TempCharacters = new List<string>(Characters.Keys) { "Exit to menu" };
                int CharacterOption = OptionSelection("Select a character...", TempCharacters);
                if (CharacterOption == TempCharacters.Count)
                {
                    // Exit
                    return;
                }
                CharacterOption--;

                string Name;
                while (true)
                {
                    Console.Write("\n\nEnter the character name: ");
                    Name = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(Name))
                    {
                        break;
                    }
                    Console.WriteLine("You must specify a name.");
                }

                Console.WriteLine($"Creating a new adventure with the mode: {Modes.Keys[ModeOption]}, and the character: {Characters.Keys[CharacterOption]}...");

                var Scenario = await API.GetScenarioAsync(Characters.Values[CharacterOption]);
                if (!IsValidResponse(Scenario))
                {
                    return;
                }
                var Result = await API.CreateAdventureAsync(Characters.Values[CharacterOption], Scenario.Data.Content.Prompt.Replace("${character.name}", Name));
                if (!IsValidResponse(Scenario))
                {
                    return;
                }
                if (Result.Data.AdventureInfo == null)
                {
                    Console.Write("Seems that the access token is invalid.\nPlease edit the token in the menu/Edit config, or try logging out and logging in.\n\nPress any key to continue...");
                    Console.ReadKey();
                    return;
                }
                ID = uint.Parse(Result.Data.AdventureInfo.ContentId); // "adventure:xxxxxxx"
                var History = Result.Data.AdventureInfo.HistoryList[Result.Data.AdventureInfo.HistoryList.Count - 1];
                TextToShow = History.Input + History.Output;
            }
            else
            {
                string CustomText;
                Console.WriteLine(CustomPrompt + '\n');
                while (true)
                {
                    Console.Write("Prompt: ");
                    CustomText = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(CustomText))
                    {
                        Console.WriteLine("You must enter a text.");
                    }
                    else if (CustomText.Length > 140)
                    {
                        Console.WriteLine("Text length must be lower than 140.");
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine("Creating a new adventure with the custom prompt...");
                var Adventure = await API.CreateAdventureAsync(Modes.Values[ModeOption]);
                if (!IsValidResponse(Adventure))
                {
                    return;
                }
                var Action = await API.RunActionAsync(uint.Parse(Adventure.Data.AdventureInfo.ContentId), ActionType.Progress, InputType.None, CustomText);
                if (!IsValidResponse(Action))
                {
                    return;
                }
                ID = uint.Parse(Action.Data.UserAction.Id.Substring(10)); // "adventure:xxxxxxx"
                var History = Action.Data.UserAction.HistoryList[Action.Data.UserAction.HistoryList.Count - 1];
                TextToShow = History.Input + History.Output;
            }
            Console.Clear();
            await ProcessAdventureAsync(ID, TextToShow);
        }

        private static async Task ContinueGameAsync()
        {
            Console.WriteLine("\nLoading adventure list...");
            // I don't know if this works but I'll leave it here anyways.
            await API.RefreshAdventureListAsync();
            var Response = await API.GetAdventureListAsync();
            if (!IsValidResponse(Response))
            {
                return;
            }
            if (Response.Data.User == null)
            {
                Console.Write("Seems that the access token is invalid.\nPlease edit the token in the menu/Edit config, or try logging out and logging in.\n\nPress any key to continue...");
                Console.ReadKey();
                return;
            }
            var AdventureList = Response.Data.User.ContentList;
            if (AdventureList.Count == 0)
            {
                Console.Write("You do not have any adventures.\nPress any key to continue...");
                Console.ReadKey();
                return;
            }
            List<string> Options = new List<string>();
            foreach (var Adventure in AdventureList)
            {
                Options.Add($"{Adventure.Title} (ID: {Adventure.ContentId})");
            }
            Options.Add("Exit to menu");
            int Option = OptionSelection("Enter an option:", Options);
            if (Option == Options.Count)
            {
                // Exit
                return;
            }
            Console.WriteLine("\nLoading adventure...");
            Option--;
            uint ID = uint.Parse(AdventureList[Option].ContentId);
            var AdventureInfo = await API.GetAdventureAsync(ID);
            if (!IsValidResponse(AdventureInfo))
            {
                return;
            }
            string Story = "";
            var HistoryList = AdventureInfo.Data.Content.HistoryList;
            if (HistoryList.Count == 0)
            {
                Console.WriteLine("The adventure is empty, strange...");
                Console.Write("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            var LastHistory = HistoryList[HistoryList.Count - 1];
            HistoryList.RemoveAt(HistoryList.Count - 1);
            foreach (var History in HistoryList)
            {
                Story += $"{History.Input}{History.Output}\n";
            }
            Console.Clear();
            Console.Write(Story);
            Thread.Sleep(5000);
            await ProcessAdventureAsync(ID, LastHistory.Input + LastHistory.Output);
        }

        private static void EditConfig()
        {
            bool Exit = false;
            while (!Exit)
            {
                List<string> Options = new List<string>(ConfigOptions)
                {
                    [1] = $"Slow typing Animation (Current: {UserConfig.SlowTyping})"
                };
                int Option = OptionSelection("Enter an option:", Options);
                switch (Option)
                {
                    case 1:
                        Console.Write("\nEnter the new token: ");
                        UserConfig.Token = Console.ReadLine();
                        API = new AIDungeon(UserConfig.Token);
                        SaveConfig();
                        break;

                    case 2:
                        UserConfig.SlowTyping = !UserConfig.SlowTyping;
                        SaveConfig();
                        break;

                    case 3:
                        DeleteConfigFile();
                        HasToken = false;
                        Login();
                        Exit = true;
                        break;

                    case 4:
                        Exit = true;
                        break;
                }
            }
        }

        private static void DeleteConfigFile()
        {
            // Try to delete the config file
            if (File.Exists(ConfigFile))
            {
                try
                {
                    File.Delete(ConfigFile);
                }
                catch
                {
                }
            }
        }

        private static void Login()
        {
            while (!HasToken)
            {
                string Email;
                string Password;
                int LoginOption = OptionSelection("Enter an option to log in:", LoginOptions);
                switch (LoginOption)
                {
                    case 1:
                        Console.Write("\nEmail: ");
                        Email = Console.ReadLine();
                        Console.Write("Username: ");
                        string Username = Console.ReadLine();
                        Console.Write("Password: ");
                        Password = Console.ReadLine();
                        Console.WriteLine("Loading...");
                        API = new AIDungeon(Email, Username, Password);
                        if (API.Token == null)
                        {
                            Console.WriteLine("Invalid register info.");
                        }
                        else
                        {
                            Console.WriteLine("Registered Successfully. You should check your E-Mail to verify your account.");
                            HasToken = true;
                        }
                        Thread.Sleep(5000);
                        break;

                    case 2:
                        Console.Write("\nEmail: ");
                        Email = Console.ReadLine();
                        Console.Write("Password: ");
                        Password = Console.ReadLine();
                        Console.WriteLine("Logging in...");
                        API = new AIDungeon(Email, Password);
                        if (API.Token == null)
                        {
                            Console.WriteLine("Invalid credentials.");
                            Thread.Sleep(5000);
                        }
                        else
                        {
                            Console.WriteLine("Successfully logged in.");
                            HasToken = true;
                        }
                        Thread.Sleep(5000);
                        break;

                    case 3:
                        Console.Write("\nToken: ");
                        string Token = Console.ReadLine();
                        API = new AIDungeon(Token);
                        HasToken = true;
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

        private static void Help()
        {
            Paginate(HelpList);
        }

        private static async Task ProcessAdventureAsync(uint AdventureID, string InitialPrompt)
        {
            ExitToMenu = false;
            bool Init = true;
            ActionResponse Response;
            ActionType CurrentActionType = ActionType.Continue;
            InputType CurrentInputType = InputType.None;
            string Text = "";
            while (!ExitToMenu)
            {
                if (Init)
                {
                    TypeLine(InitialPrompt + "\n\n" +
                        "Quick help: Use \"Do\", \"Say\" or \"Story\" at the start of the text to do an action, say something, or make progress on the story.\n" +
                        "Enter \"quit\" to exit.\n\n");
                    Init = false;
                }
                else
                {
                    Response = await API.RunActionAsync(AdventureID, CurrentActionType, CurrentInputType, Text);
                    if (!IsValidResponse(Response))
                    {
                        ExitToMenu = true;
                        continue;
                    }
                    var LastHistory = Response.Data.UserAction.HistoryList[Response.Data.UserAction.HistoryList.Count - 1];
                    TypeLine($"{LastHistory.Input}\n{LastHistory.Output}\n");
                }
                CurrentActionType = ActionType.Continue;
                CurrentInputType = InputType.None;
                Console.Write("> ");
                Text = Console.ReadLine();
                if (Text.ToLowerInvariant() == "quit")
                {
                    ExitToMenu = true;
                    continue;
                }
                if (!string.IsNullOrEmpty(Text))
                {
                    CurrentActionType = ActionType.Progress;
                    // Split the text in two, the first part should be the input type, and the second part, the text.
                    string[] SplitText = Text.Split(' ', 2);
                    if (SplitText.Length == 1 || !Enum.TryParse(SplitText[0], true, out CurrentInputType))
                    {
                        // If there isn't two parts or the parse fails, keep the text and set the input type to Do (the default).
                        CurrentInputType = InputType.Do;
                    }
                    else
                    {
                        // else, CurrentInputType will be one of the valid input types
                        // and text will be the second part (the rest) of the split text.
                        Text = SplitText[1];
                    }
                }
                //TypeLine("Generating story...");
            }
        }

        private static bool IsValidResponse(IResponse Response)
        {
            if (Response == null)
            {
                Console.Write("\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }
            if (Response.Errors != null)
            {
                Console.Write($"En error occurred: {Response.Errors[0].Message}\nPress any key to continue...");
                Console.ReadKey();
                return false;
            }
            return true;
        }

        private static int OptionSelection(string Prompt, IList<string> Options, bool ClearScreen = true)
        {
            if (ClearScreen)
            {
                Console.Clear();
            }
            Console.Write(Splash);
            string Text = $"{Prompt}\n\n";
            for (int i = 0; i < Options.Count; i++)
            {
                Text += $"{i + 1}. {Options[i]}\n";
            }
            Console.WriteLine(Text);
            Console.Write("Option: ");
            while (true)
            {
                int Option;
                if (Options.Count > 9)
                {
                    string Line = Console.ReadLine();
                    if (int.TryParse(Line, NumberStyles.Integer, CultureInfo.InvariantCulture, out Option) && Option >= 1 && Option <= Options.Count)
                    {
                        return Option;
                    }
                    DeleteLastLine();
                    Console.SetCursorPosition(8 + Line.Length, Console.CursorTop - 1);
                    for (int i = 0; i < Line.Length; i++)
                    {
                        DeleteLastChar();
                    }
                }
                else
                {
                    Option = Console.ReadKey().KeyChar - '0';
                    if (Option >= 1 && Option <= Options.Count)
                    {
                        return Option;
                    }
                    DeleteLastChar();
                }
            }
        }

        private static void Paginate(IList<string> List)
        {
            int Index = 0;
            while (true)
            {
                Console.Clear();
                Console.Write(Splash);
                Console.WriteLine(List[Index]);

                Console.WriteLine($"\nAI Dungeon Help - Page {Index + 1} of {List.Count}");
                Console.WriteLine("\nUse the Right and Left key to change the page.");
                Console.Write("Use the Esc key to exit the help.");
                while (true)
                {
                    var Input = Console.ReadKey().Key;
                    if (Input == ConsoleKey.LeftArrow && Index != 0)
                    {
                        Index--;
                        break;
                    }
                    else if (Input == ConsoleKey.RightArrow && Index != List.Count - 1)
                    {
                        Index++;
                        break;
                    }
                    else if (Input == ConsoleKey.Escape)
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

        private static void Type(string line, int minDelay = 25, int maxDelay = 50)
        {
            if (UserConfig.SlowTyping)
            {
                foreach (char character in line)
                {
                    Console.Write(character);
                    Thread.Sleep(rng.Next(minDelay, maxDelay));
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

        private static void LoadConfig()
        {
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
                Console.Write("\nAn error occurred while saving the config: " + e.Message + "\nThe config will not be saved on exit.\nPress any key to continue...");
                Console.ReadKey();
            }
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
    }
}