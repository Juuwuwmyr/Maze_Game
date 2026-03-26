namespace MazeQuest;

public class Enemy
{
    public string Name { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Damage { get; set; }
    public int Level { get; set; }
    public bool IsAlive => Health > 0;
    public int MoveCounter { get; set; } = 0;

    
    public int MoveFrequency { get; set; } = 2;

    public Enemy(string name, int row, int col, int health, int damage, int level)
    {
        Name = name;
        Row = row;
        Col = col;
        Health = health;
        MaxHealth = health;
        Damage = damage;
        Level = level;
        MoveFrequency = Math.Max(1, 3 - (level / 2));
    }

    
    
    
    
    public bool MoveTowardPlayer(int[,] maze, int playerRow, int playerCol)
    {
        if (!IsAlive) return false;

        MoveCounter++;
        if (MoveCounter < MoveFrequency) return false;
        MoveCounter = 0;

        
        
        int originalCell = maze[Row, Col];

        var path = Algorithms.BFS(maze, (Row, Col), (playerRow, playerCol));

        if (path.Count >= 2)
        {
            var nextPos = path[1]; 

            
            if (nextPos.row == playerRow && nextPos.col == playerCol)
                return true; 

            
            if (maze[nextPos.row, nextPos.col] == (int)CellType.Path)
            {
                maze[Row, Col] = (int)CellType.Path;
                Row = nextPos.row;
                Col = nextPos.col;
                maze[Row, Col] = (int)CellType.Enemy;
            }
        }

        return false;
    }

    
    
    
    public bool TakeDamage(int amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            return true;
        }
        return false;
    }

    
    
    
    public string GetSymbol() => Name switch
    {
        "Goblin" => "👽",
        "Skeleton" => "💀",
        "Slime" => "🦠",
        "Spider" => "🕷️",
        "Ghost" => "👻",
        "Orc" => "👹",
        "Wraith" => "🧟",
        _ => "💀"
    };

    public ConsoleColor GetColor() => Name switch
    {
        "Goblin" => ConsoleColor.Green,
        "Skeleton" => ConsoleColor.White,
        "Slime" => ConsoleColor.DarkGreen,
        "Spider" => ConsoleColor.DarkYellow,
        "Ghost" => ConsoleColor.Cyan,
        "Orc" => ConsoleColor.DarkRed,
        "Wraith" => ConsoleColor.Magenta,
        _ => ConsoleColor.Red
    };
}
