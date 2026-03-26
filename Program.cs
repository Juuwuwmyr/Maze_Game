namespace MazeQuest;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "Maze Quest — A DSA Adventure";

        try
        {
            
            if (OperatingSystem.IsWindows())
            {
                Console.WindowWidth = Math.Max(Console.WindowWidth, 140);
                Console.WindowHeight = Math.Max(Console.WindowHeight, 45);
                Console.BufferWidth = Math.Max(Console.BufferWidth, 140);
                Console.BufferHeight = Math.Max(Console.BufferHeight, 100);
            }
        }
        catch {  }

        
        SoundManager.Initialize();

        bool running = true;
        while (running)
        {
            SoundManager.PlayMenuMusic();
            Renderer.RenderTitleScreen();

            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    SoundManager.PlayMenuSelect();
                    StartNewGame();
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    SoundManager.PlayMenuSelect();
                    ScoreManager.ShowLeaderboard();
                    break;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    SoundManager.PlayMenuSelect();
                    Renderer.RenderHowToPlay();
                    break;

                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                case ConsoleKey.Escape:
                    running = false;
                    break;
            }
        }

        SoundManager.Shutdown();
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("  Thank you for playing Maze Quest!");
        Console.WriteLine("  A project by the BSIS students of Colegio De Naujan © 2026");
        Console.WriteLine();
        Console.ResetColor();
    }

    
    
    
    static void StartNewGame()
    {
        var state = new GameState();

        while (!state.IsGameOver && state.IsRunning)
        {
            
            Renderer.RenderLevelTransition(state.CurrentLevel);

            
            var maze = MazeGenerator.Generate(state.CurrentLevel,
                out var playerStart, out var exitPos, out var enemies);

            state.Maze = maze;
            state.PlayerRow = playerStart.r;
            state.PlayerCol = playerStart.c;
            state.ExitRow = exitPos.r;
            state.ExitCol = exitPos.c;
            state.Enemies = enemies;
            state.MoveHistory.Clear();
            state.EnemyActionQueue.Clear();

            
            state.Maze[state.PlayerRow, state.PlayerCol] = (int)CellType.Player;

            state.SetMessage($"» Level {state.CurrentLevel} — Find the exit! Watch for enemies!", ConsoleColor.Cyan);

            
            SoundManager.PlayGameMusic();
            Console.Clear();

            
            bool levelComplete = false;
            while (!state.IsGameOver && state.IsRunning && !levelComplete)
            {
                Renderer.RenderGame(state);

                
                var key = Console.ReadKey(true).Key;
                Player.ProcessInput(key, state);

                
                if (state.HasWon || (!state.IsGameOver && state.CurrentLevel > 0 &&
                    state.Maze[state.ExitRow, state.ExitCol] == (int)CellType.Player))
                {
                    if (state.HasWon)
                    {
                        levelComplete = true;
                    }
                    else if (state.CurrentLevel < GameState.MaxLevel)
                    {
                        SoundManager.PlayLevelComplete();
                        state.NextLevel();
                        levelComplete = true; 
                    }
                }
            }
        }

        if (!state.IsRunning)
            return;

        
        SoundManager.StopMusic();
        if (state.HasWon)
        {
            SoundManager.PlayVictory();
            Renderer.RenderVictory(state);
        }
        else
        {
            SoundManager.PlayGameOver();
            Renderer.RenderGameOver(state);
        }

        
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("    Enter your name for the leaderboard: ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        string name = Console.ReadLine()?.Trim() ?? "Player";
        if (string.IsNullOrEmpty(name)) name = "Player";
        if (name.Length > 16) name = name[..16];

        var entry = new ScoreEntry
        {
            Name = name,
            Score = state.Score,
            Level = state.CurrentLevel,
            Date = DateTime.Now
        };

        ScoreManager.SaveScore(entry);

        
        int rank = ScoreManager.GetRank(state.Score);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n    Your rank: #{rank}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("\n    Press any key to continue...");
        Console.ResetColor();
        Console.ReadKey(true);
    }
}
