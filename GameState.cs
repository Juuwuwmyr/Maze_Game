namespace MazeQuest;

public class GameState
{
    
    public int Lives { get; set; } = 3;
    public int MaxHealth { get; set; } = 100;
    public int Health { get; set; } = 100;
    public int Score { get; set; } = 0;
    public int CurrentLevel { get; set; } = 1;
    public const int MaxLevel = 5;

    
    public int PlayerRow { get; set; }
    public int PlayerCol { get; set; }

    
    public int[,] Maze { get; set; } = new int[0, 0];
    public int MazeRows => Maze.GetLength(0);
    public int MazeCols => Maze.GetLength(1);

    
    public int ExitRow { get; set; }
    public int ExitCol { get; set; }

    
    public List<Enemy> Enemies { get; set; } = new();

    
    public Stack<(int row, int col)> MoveHistory { get; set; } = new();

    
    public Queue<Enemy> EnemyActionQueue { get; set; } = new();

    
    
    
    
    public Dictionary<string, int> Inventory { get; set; } = new()
    {
        { "Sword", 0 },
        { "Shield", 0 },
        { "HealthPotion", 1 },
        { "Key", 0 }
    };

    
    public bool IsGameOver { get; set; } = false;
    public bool HasWon { get; set; } = false;
    public bool IsRunning { get; set; } = true;
    public int TotalEnemiesDefeated { get; set; } = 0;
    public int TotalItemsCollected { get; set; } = 0;

    
    public string StatusMessage { get; set; } = "";
    public ConsoleColor StatusColor { get; set; } = ConsoleColor.White;
    public int StatusMessageTimer { get; set; } = 0;

    
    
    
    public void Reset()
    {
        Lives = 3;
        Health = 100;
        MaxHealth = 100;
        Score = 0;
        CurrentLevel = 1;
        IsGameOver = false;
        HasWon = false;
        IsRunning = true;
        TotalEnemiesDefeated = 0;
        TotalItemsCollected = 0;
        MoveHistory.Clear();
        EnemyActionQueue.Clear();
        Enemies.Clear();
        Inventory = new Dictionary<string, int>
        {
            { "Sword", 0 },
            { "Shield", 0 },
            { "HealthPotion", 1 },
            { "Key", 0 }
        };
        StatusMessage = "";
        StatusMessageTimer = 0;
    }

    
    
    
    public void NextLevel()
    {
        CurrentLevel++;
        Health = MaxHealth;
        MoveHistory.Clear();
        EnemyActionQueue.Clear();
        Enemies.Clear();
        StatusMessage = "";
        StatusMessageTimer = 0;
    }

    
    
    
    public void AddItem(string item, int count = 1)
    {
        if (Inventory.ContainsKey(item))
            Inventory[item] += count;
        else
            Inventory[item] = count;
        TotalItemsCollected += count;
    }

    
    
    
    public bool UseItem(string item)
    {
        if (Inventory.ContainsKey(item) && Inventory[item] > 0)
        {
            Inventory[item]--;
            return true;
        }
        return false;
    }

    
    
    
    public int GetItemCount(string item)
    {
        return Inventory.ContainsKey(item) ? Inventory[item] : 0;
    }

    public void SetMessage(string msg, ConsoleColor color, int duration = 8)
    {
        StatusMessage = msg;
        StatusColor = color;
        StatusMessageTimer = duration;
    }

    public void TickMessage()
    {
        if (StatusMessageTimer > 0)
        {
            StatusMessageTimer--;
            if (StatusMessageTimer == 0)
                StatusMessage = "";
        }
    }
}

public enum CellType
{
    Path = 0,
    Wall = 1,
    Player = 2,
    Exit = 3,
    Enemy = 4,
    HealthPotion = 5,
    Sword = 6,
    Shield = 7,
    Key = 8,
    Trap = 9
}

public class ScoreEntry
{
    public string Name { get; set; } = "Player";
    public int Score { get; set; }
    public int Level { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;

    public override string ToString()
    {
        return $"{Name}|{Score}|{Level}|{Date:yyyy-MM-dd HH:mm}";
    }

    public static ScoreEntry? Parse(string line)
    {
        var parts = line.Split('|');
        if (parts.Length >= 4 && int.TryParse(parts[1], out int score) &&
            int.TryParse(parts[2], out int level))
        {
            return new ScoreEntry
            {
                Name = parts[0],
                Score = score,
                Level = level,
                Date = DateTime.TryParse(parts[3], out var d) ? d : DateTime.Now
            };
        }
        return null;
    }
}
