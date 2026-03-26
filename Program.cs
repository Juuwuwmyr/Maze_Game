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
        int selectedIndex = 0;

        while (running)
        {
            SoundManager.PlayMenuMusic();
            Renderer.RenderTitleScreen(selectedIndex);

            var keyInfo = Console.ReadKey(true);
            var key = keyInfo.Key;

            if (key == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex - 1 + 4) % 4;
                SoundManager.PlayMenuSelect();
            }
            else if (key == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex + 1) % 4;
                SoundManager.PlayMenuSelect();
            }
            else if (key == ConsoleKey.Enter)
            {
                SoundManager.PlayMenuSelect();
                switch (selectedIndex)
                {
                    case 0: // New Game
                        StartNewGame();
                        break;
                    case 1: // Leaderboard
                        ScoreManager.ShowLeaderboard();
                        break;
                    case 2: // How To Play
                        Renderer.RenderHowToPlay();
                        break;
                    case 3: // Exit
                        running = false;
                        break;
                }
            }
            else if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1) { selectedIndex = 0; StartNewGame(); }
            else if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2) { selectedIndex = 1; ScoreManager.ShowLeaderboard(); }
            else if (key == ConsoleKey.D3 || key == ConsoleKey.NumPad3) { selectedIndex = 2; Renderer.RenderHowToPlay(); }
            else if (key == ConsoleKey.D4 || key == ConsoleKey.NumPad4 || key == ConsoleKey.Escape) { running = false; }
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
        Renderer.RenderNameEntry(state);

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

        
        var entry = new ScoreEntry
        {
            Name = state.PlayerName,
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
