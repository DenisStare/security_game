using System.Text.RegularExpressions;
using System.Text.Json;

// 
// Settings
//

const string save_dir = "./Saves";
const string prompt = "cmd> ";

PlayerStats player = new PlayerStats();
SaveManager save_manager = new SaveManager(player, new JsonSerializerOptions{ WriteIndented = true });

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
        if (player.HasItem("key"))
        {
            Console.WriteLine("You have the key.");
            continue;
        }

        Console.WriteLine("You see something in a bush.");
        Console.WriteLine("You look in the bush and find the key.");

        player.PickUpItem("hey");
        Console.WriteLine("You put the key in your pocket.");
    }
    else if (user_input == "open gate")
    {
        Console.WriteLine("You try to open the gate.");
        if (!player.HasItem("key"))
        {
            Console.WriteLine("You don't have the key.");
            Console.WriteLine("But you try anyway.");
            Console.WriteLine("The door won't open.");

            player.LoseStamina(20);

            Console.WriteLine("You lose 20% of your stamina.");
            continue;
        }
        else if (player.stamina != 100)
        {
            Console.WriteLine("You dont have stamina.");
            Console.WriteLine("But you try anyway");

            player.LoseStamina(15);

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
        if (player.stamina == 100)
        {
            Console.WriteLine("You have 100% stamina");
            continue;
        }

        player.AddStamina(20);

        Console.WriteLine("You recover 20% stamina.");
    }
    else if (Regex.IsMatch(user_input, @"^save \w+\.json"))
    {
        Console.WriteLine("Saving data...");
        string file_name = user_input.Split(" ")[1];

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
        Console.WriteLine("Stats: ");
        Console.WriteLine($"\tStamina: {player.stamina}%");
        Console.WriteLine($"\tHas Key: {player.items}");
    }
}


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
    PlayerStats _save_data;
    JsonSerializerOptions _data_options;


    public SaveManager(PlayerStats player_data, JsonSerializerOptions? data_options)
    {
        _save_data = player_data;
        if (data_options != null)
            _data_options = data_options;
        else
            _data_options = new JsonSerializerOptions();
    }

    public void SaveGame(string file_name, string file_dir)
    {
        if (!Directory.Exists(file_dir))
        {
            Directory.CreateDirectory(file_dir);
        }

        string file_path = $"{file_dir}/{file_name}";

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

        PlayerStats player_data = JsonSerializer.Deserialize<PlayerStats>(json_data, _data_options)!;
        if (player_data == null)
        {
            return false;
        }

        _save_data = player_data;
        return true;
    }
}
