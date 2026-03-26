namespace MazeQuest;

public static class ScoreManager
{
    private static readonly string ScoreFile = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "scores.dat");

    
    
    
    public static void SaveScore(ScoreEntry entry)
    {
        try
        {
            File.AppendAllText(ScoreFile, entry.ToString() + Environment.NewLine);
        }
        catch
        {
            
        }
    }

    
    
    
    public static List<ScoreEntry> GetLeaderboard(int topN = 10)
    {
        var entries = LoadScores();

        
        entries = Algorithms.MergeSort(entries);

        return entries.Count > topN ? entries.GetRange(0, topN) : entries;
    }

    
    
    
    public static int GetRank(int score)
    {
        var entries = LoadScores();
        entries = Algorithms.MergeSort(entries);

        
        var sortedScores = new List<int>();
        foreach (var e in entries)
            sortedScores.Add(e.Score);

        
        return Algorithms.BinarySearchRank(sortedScores, score) + 1; 
    }

    
    
    
    private static List<ScoreEntry> LoadScores()
    {
        var entries = new List<ScoreEntry>();

        if (!File.Exists(ScoreFile))
            return entries;

        try
        {
            var lines = File.ReadAllLines(ScoreFile);
            foreach (var line in lines)
            {
                var entry = ScoreEntry.Parse(line);
                if (entry != null)
                    entries.Add(entry);
            }
        }
        catch
        {
            
        }

        return entries;
    }

    
    
    
    public static void ShowLeaderboard()
    {
        Console.Clear();
        var entries = GetLeaderboard();

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine();
        Console.WriteLine("    ╔══════════════════════════════════════════════════════════════╗");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("    ║                    L E A D E R B O A R D                    ║");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("    ╠══════════════════════════════════════════════════════════════╣");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("    ║  Rank │ Name             │  Score  │ Level │ Date           ║");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("    ╠══════════════════════════════════════════════════════════════╣");

        if (entries.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    ║           No scores recorded yet.                          ║");
        }
        else
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                ConsoleColor rankColor = i switch
                {
                    0 => ConsoleColor.Yellow,
                    1 => ConsoleColor.White,
                    2 => ConsoleColor.DarkYellow,
                    _ => ConsoleColor.DarkGray
                };

                string medal = i switch
                {
                    0 => "♛",
                    1 => "♕",
                    2 => "♜",
                    _ => " "
                };

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("    ║  ");
                Console.ForegroundColor = rankColor;
                Console.Write($"{medal}{i + 1,2}");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("  │ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{e.Name,-16}");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(" │ ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{e.Score,7}");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(" │   ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{e.Level}");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("   │ ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{e.Date:yyyy-MM-dd}");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("     ║");
                Console.WriteLine();
            }
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("    ╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("    Press any key to return to menu...");
        Console.ResetColor();
        Console.ReadKey(true);
    }
}
