using System.Text.RegularExpressions;
using System.Text.Json;

using Actions = System.Collections.Generic.List<PlayerAction>;

// 
// Settings
//

const string save_dir = "./Saves";
const string prompt = "cmd> ";

var actions = new Actions
{
new PlayerAction("search for key", (p, _) =>
    {
        if (p.HasItem("key"))
        {
            Console.WriteLine("You have the key.");
            return;
        }

        Console.WriteLine("You see something in a bush.");
        Console.WriteLine("You look in the bush and find the key.");

        p.PickUpItem("key");
        Console.WriteLine("You put the key in your pocket.");
    }, 
    user_input => user_input == "search"
),

new PlayerAction("open gate", (p, _) => {
    Console.WriteLine("You try to open the gate.");
    if (!p.HasItem("key"))
    {
        Console.WriteLine("You don't have the key.");
        Console.WriteLine("But you try anyway.");
        Console.WriteLine("The door won't open.");

        p.LoseStamina(20);

        Console.WriteLine("You lose 20% of your stamina.");
        return;
    }
    else if (p.stamina != 100)
    {
        Console.WriteLine("You dont have stamina.");
        Console.WriteLine("But you try anyway");

        p.LoseStamina(15);

        Console.WriteLine("You lose 15% stamina.");
        return;
    }

    Console.WriteLine("You use the key.");
    Console.WriteLine("The gate opens.");
    Console.WriteLine("Game over!");
    return;
}, 
user_input => user_input == "open gate" 
),

new PlayerAction("exit", (p, _) =>
{
    Environment.Exit(0);
}, user_input => user_input == "exit"
),

new PlayerAction("rest", (p, _) =>
{
    Console.WriteLine("You rest.");
    // TODO BUG: check if it has 100 stamina.
    if (p.stamina == 100)
    {
        Console.WriteLine("You have 100% stamina");
        return;
    }

    p.AddStamina(20);

    Console.WriteLine("You recover 20% stamina.");
}, user_input => user_input == "rest"
)

};



var settings = new Settings(args, save_dir, prompt);
var game = new GameManager(actions);

game.Run(settings);

// void Save(string input, Settings settings)
// {
//     Console.WriteLine("Saving data...");
//     string[] parts = input.Split(" ");
//     if (parts.Length > 2)
//     {
//         Console.WriteLine("Please put the path.");
//     }
//     string file_name = parts[1];

//     _save_manager.SaveGame(ref _player, file_name, settings.save_dir);
//     Console.WriteLine("Data Saved..");
// }

// void Load(string input, Settings settings)
// {
//     Console.WriteLine("Loading data...");
//     string[] parts = input.Split(" ");
//     if (parts.Length > 2)
//     {
//         Console.WriteLine("Please put the path.");
//         return;
//     }
//     string file_name = parts[1];

//     string file_path = $"{settings.save_dir}/{file_name}";

//     if (!_save_manager.LoadSave(ref _player, file_path))
//     {
//         Console.WriteLine($"The file '{file_name}' dosen't exist in '{settings.save_dir}' directory");
//         return;
//     }

//     Console.WriteLine("Data Loaded..");
// }

// void Stats(PlayerStats player)
// {
//     Console.WriteLine("Stats: ");
//     Console.WriteLine($"\tStamina: {player.stamina}%");

//     if (player.items.Count == 0)
//     {
//         Console.WriteLine("\tYou dont have items!");
//         return;
//     }

//     Console.WriteLine("\tItems: ");
//     foreach (string item in player.items)
//     {
//         Console.WriteLine("\t\t" + item);
//     }
// }

class PlayerStats
{
    public short stamina = 100;
    public List<string> items = new();

    public short LoseStamina(short to_lose)
    {
        this.stamina = (short)Math.Max(stamina - to_lose, 0);
        if (this.stamina == 0)
        {
            GameOver();
            return -1;
        }

        return this.stamina;
    }

    public short AddStamina(short to_add)
    {
        this.stamina = (short)Math.Min(stamina + to_add, 100);
        return this.stamina;
    }

    public void PickUpItem(string new_item)
    {
        this.items.Add(new_item);
    }

    public bool HasItem(string item) // Todo: for one item
    {
        if (this.items.Contains(item))
        {
            return true;
        }
        return false;
    }

    public bool HasItems(string[] items)
    {
        foreach (string item in items)
        {
            if (!this.HasItem(item))
            {
                return false;
            }
        }
        return true;
    }

    public void GameOver()
    {
        Console.WriteLine("You have 0% stamina. You die.");
        Environment.Exit(3);
    }
};


class SaveManager
{
    JsonSerializerOptions _data_options;

    public SaveManager()
    {
        _data_options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true};
    }

    public SaveManager(JsonSerializerOptions data_options)
    {

        _data_options = data_options;
    }

    public void SaveGame(ref PlayerStats to_save, string file_name, string file_dir)
    {
        if (!Directory.Exists(file_dir))
        {
            Directory.CreateDirectory(file_dir);
        }

        string file_path = $"{file_dir}/{file_name}";

        string json_data = JsonSerializer.Serialize(to_save, _data_options);

        File.WriteAllText(file_path, json_data);
    }

    public bool LoadSave(ref PlayerStats to_load, string file_path)
    {
        if (!File.Exists(file_path))
        {
            return false;
        }
        string json_data = File.ReadAllText(file_path);

        PlayerStats player_data = JsonSerializer.Deserialize<PlayerStats>(json_data, _data_options)!;
        if (player_data == null)
        {
            return false;
        }

        to_load = player_data;
        return true;
    }
}

class Settings
{
    public string save_dir = "";
    public string[] args = [];

    public string prompt = "";

    public Settings(string[] args, string save_directory, string prompt)
    {
        this.args = args;
        this.prompt = prompt;
        save_dir = save_directory;
    }
}
class GameManager
{
    string _user_input = "";
    PlayerStats _player = new PlayerStats();
    SaveManager _save_manager = new SaveManager();

    Actions _actions;

    public GameManager(Actions actions, PlayerStats? player = null, SaveManager? save_manager = null)
    {
        if (player != null) _player = player;
        if (save_manager != null) _save_manager = save_manager;

        _actions = actions;
    }

    public void Run(Settings game_settings)
    {
        CheckForSaveFiles(game_settings);
        PlayIntro(_player);

        while (true)
        {
            try 
            {
                _user_input = GetUserInput(game_settings.prompt);
            }
            catch (Exception _)
            {
                Console.WriteLine("Go and type something player!");
                continue;
            }

            foreach (PlayerAction action in _actions)
            {
                action.Execute(_player, _user_input, game_settings);
            }
            // if (_user_input == "exit")
            // {
            //     Exit();
            // }
            // else if (_user_input == "search" )
            // {
            //     Search(_player);
            // }
            // else if (_user_input == "open gate")
            // {
            //     OpenGate();
            // }
            // else if (_user_input == "rest")
            // {
            //     Rest(_player);
            // }
            // else if (Regex.IsMatch(_user_input, @"^save \w+\.json"))
            // {
            //     Save(_user_input, game_settings);
            // }
            // else if (Regex.IsMatch(_user_input, @"^load \w+\.json"))
            // {
            //     Load(_user_input, game_settings);
            // }
            // else if (_user_input == "stats")
            // {
            //     Stats(_player);
            // }
        }
    }

    string GetUserInput(string prompt)
    {
        Console.Write(prompt);
        string user_input = Console.ReadLine()!;

        if (user_input == null)
        {
            user_input = "";
            throw new Exception("Invalid input!");
        }
        return user_input.Trim().ToLower();
    }

    void PlayIntro(PlayerStats player)
    {
        Console.WriteLine("Security Game");

        Console.WriteLine("You are in a forest and you see a gate.");
        Console.WriteLine("On that door you see an sign.");
        Console.WriteLine("The sign says: 'You need 100% stamina and a key'.");
        Console.WriteLine($"You have {player.stamina} stamina but you dont have an key.");
        Console.WriteLine("What do you do?");

        Console.Write("\n");
        Console.WriteLine("Type 'exit' to exit.");
    }

    void CheckForSaveFiles(Settings info)
    {
        if (info.args.Length == 1)
        {
            string saved_file = info.args[0];
            string file_path = $"{info.save_dir}/{saved_file}";
            if (!_save_manager.LoadSave(ref _player, file_path))
            {
                Console.WriteLine($"The save file '{saved_file}' dosen't exist in '{info.save_dir}' directory");
                return;
            }
        }
    }
}

class PlayerAction
{
    string _action_name;
    Action<PlayerStats, object[]?> _action;
    Predicate<string> _condition;

    public PlayerAction(string name, Action<PlayerStats, object[]?> action, Predicate<string> condition)
    {
        _action_name = name;
        _action = action;  
        _condition = condition;
    }

    public void Execute(PlayerStats player_info, string input, params object[] optional_paramaters)
    {
        if (_condition.Invoke(input) != true) return;
        _action.Invoke(player_info, optional_paramaters);
    }
}