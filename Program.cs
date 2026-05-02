using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text;

// Returns error
bool LoseStamina(ref short stamina, short to_lose)
{
    short last_stamina = stamina;
    stamina -= to_lose;

    if (stamina < 0)
    {
        stamina = last_stamina;
        return true;
    }
    return false;
}

// Returns error
bool AddStamina(ref short stamina, short to_add)
{
    short last_stamina = stamina;
    stamina += to_add;
    if (stamina > 100)
    {
        stamina = last_stamina;
        return true;
    }
    return false;
}

void SaveGame(string file_name, string file_dir, dynamic save_data)
{
    if (!Directory.Exists(file_dir))
    {
        Directory.CreateDirectory(file_dir);
    }

    string file_path = $"{file_dir}/{file_name}";

    StreamWriter save_file_writer = new StreamWriter(file_path);

    // For formathing 
    var options = new JsonSerializerOptions { WriteIndented = true };
    string json_data = JsonSerializer.Serialize(save_data, options);

    save_file_writer.Write(json_data);

    save_file_writer.Close();
}

void LoadSave(string file_path, ref short stamina, ref bool has_key)
{
    StreamReader save_data_reader = new StreamReader(file_path);
    string json_data = save_data_reader.ReadToEnd();

    JsonElement json_data_element = JsonSerializer.Deserialize<JsonElement>(json_data);
    
    stamina = json_data_element.GetProperty("stamina").GetInt16();
    has_key = json_data_element.GetProperty("hasKey").GetBoolean();
}


// 
// Settings
//

const string save_dir = "./Saves";
const string prompt = "cmd> ";
const bool dev_mode = false;

short stamina = 100;
bool has_key = false;

if (args.Length == 1)
{
    string saved_file = args[0];
    string file_path = $"{save_dir}/{saved_file}";
    if (!File.Exists(file_path))
    {
        Console.WriteLine($"The save file '{saved_file}' dosen't exist in '{save_dir}' directory");
        goto game_start;
    }

    LoadSave(file_path, ref stamina, ref has_key);
}


game_start:

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
        if (has_key)
        {
            Console.WriteLine("You have the key.");
            continue;
        }

        Console.WriteLine("You see something in a bush.");
        Console.WriteLine("You look in the bush and find the key.");

        has_key = true;
        Console.WriteLine("You put the key in your pocket.");
    }
    else if (user_input == "open gate")
    {
        Console.WriteLine("You try to open the gate.");
        if (!has_key)
        {
            Console.WriteLine("You don't have the key.");
            Console.WriteLine("But you try anyway.");
            Console.WriteLine("The door won't open.");

            if (LoseStamina(ref stamina, 20))
            {
                Console.WriteLine("You have 0 stamina.");
                Console.WriteLine("You die!");
                break;
            }

            Console.WriteLine("You lose 20% of your stamina.");
            continue;
        }
        else if (stamina != 100)
        {
            Console.WriteLine("You dont have stamina.");
            Console.WriteLine("But you try anyway");

            if (LoseStamina(ref stamina, 15))
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
        if (AddStamina(ref stamina, 20))
        {
            Console.WriteLine("You have 100% stamina.");
            continue;
        }
        Console.WriteLine("You recover 20% stamina.");
    }
    else if (Regex.IsMatch(user_input, @"^save \w+\.json$"))
    {
        Console.WriteLine("Saving data...");
        string file_name = user_input.Split(" ")[1];

        // Save date
        var save_data = new
        {
            stamina = stamina,
            hasKey = has_key
        };

        SaveGame(file_name, save_dir, save_data);
        Console.WriteLine("Data Saved..");
    }
    else if (Regex.IsMatch(user_input, @"^load \w+\.json$"))
    {
        Console.WriteLine("Loading data...");
        string file_name = user_input.Split(" ")[1];
        string file_path = $"{save_dir}/{file_name}";

        if (!File.Exists(file_path))
        {
            Console.WriteLine($"The save file '{file_name}' dosen't exist in '{save_dir}' directory");
            continue;
        }

        LoadSave(file_path, ref stamina, ref has_key);
        Console.WriteLine("Data Loaded..");
    }
    else if (user_input == "stats")
    {
        if (dev_mode)
        {
            Console.WriteLine("Stats: ");
            Console.WriteLine($"\tStamina: {stamina}%");
            Console.WriteLine($"\tHas Key: {has_key}");
        }
        else
        {
            Console.WriteLine("You are not an dev!");
        }
    }
}

