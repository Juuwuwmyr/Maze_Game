namespace MazeQuest;

public static class MazeGenerator
{
    private static readonly Random Rng = new();

    
    
    
    private static readonly (int rows, int cols, int enemies, int potions, int swords, int shields, int traps)[] LevelConfig =
    {
        (15, 25, 2,  3, 1, 1, 2),   
        (17, 29, 3,  3, 1, 1, 3),   
        (21, 35, 4,  2, 2, 1, 4),   
        (25, 41, 5,  2, 2, 1, 5),   
        (29, 47, 7,  2, 2, 2, 6),   
    };

    
    
    
    public static int[,] Generate(int level, out (int r, int c) playerStart, out (int r, int c) exitPos,
        out List<Enemy> enemies)
    {
        int idx = Math.Clamp(level - 1, 0, LevelConfig.Length - 1);
        var config = LevelConfig[idx];

        
        int rows = config.rows | 1;
        int cols = config.cols | 1;

        int[,] maze;
        bool solvable;
        (int r, int c) start, exit;

        
        do
        {
            maze = new int[rows, cols];

            
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    maze[r, c] = (int)CellType.Wall;

            
            var visited = new bool[rows, cols];
            RecursiveCarve(maze, visited, 1, 1, rows, cols);

            
            start = (1, 1);
            exit = (rows - 2, cols - 2);
            maze[start.r, start.c] = (int)CellType.Path;
            maze[exit.r, exit.c] = (int)CellType.Exit;

            
            var solveVisited = new bool[rows, cols];
            solvable = Algorithms.RecursiveSolve(maze, start, exit, solveVisited);
        }
        while (!solvable);

        playerStart = start;
        exitPos = exit;

        
        PlaceItems(maze, config.potions, CellType.HealthPotion, start, exit);
        PlaceItems(maze, config.swords, CellType.Sword, start, exit);
        PlaceItems(maze, config.shields, CellType.Shield, start, exit);
        PlaceItems(maze, config.traps, CellType.Trap, start, exit);

        
        enemies = PlaceEnemies(maze, config.enemies, level, start, exit);

        return maze;
    }

    
    
    
    
    private static void RecursiveCarve(int[,] maze, bool[,] visited,
        int row, int col, int rows, int cols)
    {
        visited[row, col] = true;
        maze[row, col] = (int)CellType.Path;

        
        var directions = new (int dr, int dc)[] { (-2, 0), (2, 0), (0, -2), (0, 2) };
        Shuffle(directions);

        foreach (var (dr, dc) in directions)
        {
            int nr = row + dr;
            int nc = col + dc;

            if (nr > 0 && nr < rows - 1 && nc > 0 && nc < cols - 1 && !visited[nr, nc])
            {
                
                maze[row + dr / 2, col + dc / 2] = (int)CellType.Path;
                RecursiveCarve(maze, visited, nr, nc, rows, cols);
            }
        }
    }

    private static void PlaceItems(int[,] maze, int count, CellType type,
        (int r, int c) start, (int r, int c) exit)
    {
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);
        int placed = 0;
        int attempts = 0;

        while (placed < count && attempts < 500)
        {
            int r = Rng.Next(1, rows - 1);
            int c = Rng.Next(1, cols - 1);
            attempts++;

            if (maze[r, c] == (int)CellType.Path && (r, c) != start && (r, c) != exit)
            {
                maze[r, c] = (int)type;
                placed++;
            }
        }
    }

    private static List<Enemy> PlaceEnemies(int[,] maze, int count, int level,
        (int r, int c) start, (int r, int c) exit)
    {
        var enemies = new List<Enemy>();
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);
        int placed = 0;
        int attempts = 0;

        string[] names = { "Goblin", "Skeleton", "Slime", "Spider", "Ghost", "Orc", "Wraith" };

        while (placed < count && attempts < 500)
        {
            int r = Rng.Next(3, rows - 3);
            int c = Rng.Next(3, cols - 3);
            attempts++;

            
            int dist = Math.Abs(r - start.r) + Math.Abs(c - start.c);
            if (maze[r, c] == (int)CellType.Path && dist > 6)
            {
                maze[r, c] = (int)CellType.Enemy;
                int hp = 20 + (level * 10) + Rng.Next(0, 10);
                int dmg = 5 + (level * 3) + Rng.Next(0, 5);
                string name = names[Rng.Next(names.Length)];
                enemies.Add(new Enemy(name, r, c, hp, dmg, level));
                placed++;
            }
        }

        return enemies;
    }

    private static void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
