using System.Text.RegularExpressions;
using System.Text.Json;

// 
// Settings
//

const string save_dir = "./Saves";
const string prompt = "cmd> ";
const bool dev_mode = false;

PlayerProprietes player_proprietes = new PlayerProprietes(100, false);
SaveManager save_manager = new SaveManager(player_proprietes, new JsonSerializerOptions{ WriteIndented = true });

if (args.Length == 1)
{
    string saved_file = args[0];
    string file_path = $"{save_dir}/{saved_file}";
    if (!save_manager.LoadSave(file_path))
    {
        Console.WriteLine($"The save file '{saved_file}' dosen't exist in '{save_dir}' directory");
        return;
    }
}

Console.WriteLine("Security Game");

// player needs 100% stamina and a key
// if player types search it finds the key
// if dosent have the key or stamina too low then it losses stamina

Console.WriteLine("You are in a forest and you see a gate.");
Console.WriteLine("On that door you see an sign.");
Console.WriteLine("The sign says: 'You need 100% stamina and a key'.");
Console.WriteLine("You have 100% stamina but you dont have an key.");
Console.WriteLine("What do you do?");

string user_input;

Console.Write("\n");
Console.WriteLine("Type 'exit' to exit.");

while (true)
{
    Console.Write(prompt);
    user_input = Console.ReadLine()!;

    if (user_input == "exit")
    {
        return;
    }
    else if (user_input == "search" )
    {
        if (player_proprietes.HasItem())
        {
            Console.WriteLine("You have the key.");
            continue;
        }

        Console.WriteLine("You see something in a bush.");
        Console.WriteLine("You look in the bush and find the key.");

        player_proprietes.PickUpItem();
        Console.WriteLine("You put the key in your pocket.");
    }
    else if (user_input == "open gate")
    {
        Console.WriteLine("You try to open the gate.");
        if (!player_proprietes.HasItems())
        {
            Console.WriteLine("You don't have the key.");
            Console.WriteLine("But you try anyway.");
            Console.WriteLine("The door won't open.");

            if (player_proprietes.LoseStamina(20))
            {
                Console.WriteLine("You have 0 stamina.");
                Console.WriteLine("You die!");
                break;
            }

            Console.WriteLine("You lose 20% of your stamina.");
            continue;
        }
        else if (player_proprietes.stamina != 100)
        {
            Console.WriteLine("You dont have stamina.");
            Console.WriteLine("But you try anyway");

            if (player_proprietes.LoseStamina(15))
            {
                Console.WriteLine("You have 0 stamina.");
                Console.WriteLine("You die!");
                break;
            }

            Console.WriteLine("You lose 15% stamina.");
            continue;
        }

        Console.WriteLine("You use the key.");
        Console.WriteLine("The gate opens.");
        Console.WriteLine("Game over!");
        break;
    }
    else if (user_input == "rest")
    {
        Console.WriteLine("You rest.");
        if (player_proprietes.AddStamina(20))
        {
            Console.WriteLine("You have 100% stamina.");
            continue;
        }
        Console.WriteLine("You recover 20% stamina.");
    }
    else if (Regex.IsMatch(user_input, @"^save \w+\.json"))
    {
        Console.WriteLine("Saving data...");
        string file_name = user_input.Split(" ")[1];

        // Save date
        var save_data = new
        {
            stamina = player_proprietes.stamina,
            hasKey = player_proprietes.has_key,
        };

        save_manager.SaveGame(file_name, save_dir);
        Console.WriteLine("Data Saved..");
    }
    else if (Regex.IsMatch(user_input, @"^load \w+\.json"))
    {
        Console.WriteLine("Loading data...");
        string file_name = user_input.Split(" ")[1];
        string file_path = $"{save_dir}/{file_name}";

        if (!save_manager.LoadSave(file_path))
        {
            Console.WriteLine($"The file '{file_name}' dosen't exist in '{save_dir}' directory");
            continue;
        }

        Console.WriteLine("Data Loaded..");
    }
    else if (user_input == "stats")
    {
        if (dev_mode)
        {
            Console.WriteLine("Stats: ");
            Console.WriteLine($"\tStamina: {player_proprietes.stamina}%");
            Console.WriteLine($"\tHas Key: {player_proprietes.has_key}");
        }
        else
        {
            Console.WriteLine("You are not an dev!");
        }
    }
}


class PlayerProprietes
{
    short _stamina;
    bool _has_key;

    public short stamina
    {
        get
        {
            return _stamina;
        }
        set
        {
            _stamina = value;
        }
    }

    public bool has_key
    {
        get
        {
            return _has_key;
        }
        set
        {
            _has_key = value;
        }
    }

    public PlayerProprietes(short stamina, bool has_key)
    {
        this._stamina = stamina;
        this._has_key = has_key;
    }

    public bool LoseStamina(short to_lose)
    {
        short last_stamina = this._stamina;
        _stamina -= to_lose;

        if (_stamina <= 0)
        {
            _stamina = last_stamina;
            return true;
        }
        return false;
    }

    public bool AddStamina(short to_add)
    {
        short last_stamina = _stamina;
        _stamina += to_add;
        if (_stamina > 100)
        {
            _stamina = last_stamina;
            return true;
        }
        return false;
    }

    public void PickUpItem() // for now a key
    {
        _has_key = true;
    }

    public bool HasItem() // Todo: for one item
    {
        return _has_key;
    }

    public bool HasItems() // for multiple item
    {
        return _has_key;
    }

};


class SaveManager
{
    PlayerProprietes _save_data;
    JsonSerializerOptions _data_options;


    public SaveManager(PlayerProprietes player_data, JsonSerializerOptions? data_options)
    {
        _save_data = player_data;
        if (data_options != null)
            _data_options = data_options!;
    }

    public void SaveGame(string file_name, string file_dir)
    {
        if (!Directory.Exists(file_dir))
        {
            Directory.CreateDirectory(file_dir);
        }

        string file_path = $"{file_dir}/{file_name}";

        // For formathing 
        if (_data_options == null)
            _data_options = new JsonSerializerOptions();

        string json_data = JsonSerializer.Serialize(_save_data, _data_options);

        File.WriteAllText(file_path, json_data);
    }

    public bool LoadSave(string file_path)
    {
        if (!File.Exists(file_path))
        {
            return false;
        }
        string json_data = File.ReadAllText(file_path);

        if (_data_options == null)
            _data_options = new JsonSerializerOptions();

        PlayerProprietes player_data = JsonSerializer.Deserialize<PlayerProprietes>(json_data, _data_options)!;
        if (player_data == null)
        {
            return false;
        }

        _save_data = player_data;
        return true;
    }
}