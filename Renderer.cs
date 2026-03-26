namespace MazeQuest;

public static class Renderer
{
    private const int FogRadius = 5;
    private const int HudWidth = 30;

    
    
    
    
    public static void RenderGame(GameState state)
    {
        Console.CursorVisible = false;
        Console.SetCursorPosition(0, 0);

        int rows = state.MazeRows;
        int cols = state.MazeCols;

        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("  ╔");
        for (int c = 0; c < cols; c++) Console.Write("══");
        Console.Write("╗");
        PrintHudLine(state, -2);
        Console.WriteLine();

        Console.Write("  ║");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write( $"  MAZE QUEST — Level {state.CurrentLevel}");
        int titleLen = $"  MAZE QUEST — Level {state.CurrentLevel}".Length;
        for (int i = titleLen; i < cols * 2; i++) Console.Write(' ');
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("║");
        PrintHudLine(state, -1);
        Console.WriteLine();

        Console.Write("  ╠");
        for (int c = 0; c < cols; c++) Console.Write("══");
        Console.Write("╣");
        PrintHudLine(state, 0);
        Console.WriteLine();

        
        for (int r = 0; r < rows; r++)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("  ║");

            for (int c = 0; c < cols; c++)
            {
                
                int dist = Math.Abs(r - state.PlayerRow) + Math.Abs(c - state.PlayerCol);
                if (dist > FogRadius)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("░░");
                    continue;
                }

                float fogFade = dist > FogRadius - 2 ? 0.5f : 1f;
                CellType cell = (CellType)state.Maze[r, c];

                switch (cell)
                {
                    case CellType.Wall:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("🧱");
                        break;

                    case CellType.Path:
                        Console.ForegroundColor = fogFade < 1 ? ConsoleColor.DarkGray : ConsoleColor.DarkGray;
                        Console.Write("  ");
                        break;

                    case CellType.Player:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("🤴🏻");
                        break;

                    case CellType.Exit:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("▓▓");
                        break;

                    case CellType.Enemy:
                        Enemy? enemy = FindEnemyAt(state, r, c);
                        if (enemy != null)
                        {
                            Console.ForegroundColor = enemy.GetColor();
                            string sym = enemy.GetSymbol();
                            Console.Write(sym);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("EE");
                        }
                        break;

                    case CellType.HealthPotion:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("❤️");
                        break;

                    case CellType.Sword:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("⚔️");
                        break;

                    case CellType.Shield:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("🛡️");
                        break;

                    case CellType.Key:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("🗝️");
                        break;

                    case CellType.Trap:
                        
                        if (dist <= 2)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write("^^");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.Write("  ");
                        }
                        break;
                }
            }

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("║");
            PrintHudLine(state, r + 1);
            Console.WriteLine();
        }
 
        
        for (int r = rows + 1; r <= 23; r++)
        {
            Console.Write(new string(' ', cols * 2 + 4));
            PrintHudLine(state, r);
            Console.WriteLine();
        }

        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("  ╚");
        for (int c = 0; c < cols; c++) Console.Write("══");
        Console.Write("╝");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();

        
        Console.Write("  ");
        if (!string.IsNullOrEmpty(state.StatusMessage))
        {
            Console.ForegroundColor = state.StatusColor;
            Console.Write(state.StatusMessage.PadRight(cols * 2 + HudWidth));
        }
        else
        {
            Console.Write(new string(' ', cols * 2 + HudWidth));
        }
        Console.WriteLine();

        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"  [WASD/Arrows] Move  [U] Undo  [H] Heal  [Q] Quit");
        Console.Write(new string(' ', 30));
        Console.WriteLine();

        Console.ResetColor();
    }

    
    
    
    private static void PrintHudLine(GameState state, int lineIndex)
    {
        Console.Write("  ");
        switch (lineIndex)
        {
            case -2:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("╔══════════════════════════╗");
                break;
            case -1:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("     PLAYER STATUS        ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 0:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("╠══════════════════════════╣");
                break;
            case 1:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($" ❤️ Lives: {state.Lives,-15} ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 2:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" HP: ");
                PrintHealthBar(state.Health, state.MaxHealth, 17);
                Console.Write("  ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 3:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"      {state.Health,3}/{state.MaxHealth,-3} HP          ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 4:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($" Score: {state.Score,-18} ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 5:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("╠══════════════════════════╣");
                break;
            case 6:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("      INVENTORY           ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 7:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("╠══════════════════════════╣");
                break;
            case 8:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($" ⚔️ Swords:  {state.GetItemCount("Sword"),-12} ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 9:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($" 🛡️ Shields: {state.GetItemCount("Shield"),-12} ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 10:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($" ❤️ Potions: {state.GetItemCount("HealthPotion"),-12} ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 11:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($" 🗝️ Keys:    {state.GetItemCount("Key"),-12} ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 12:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("╠══════════════════════════╣");
                break;
            case 13:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("       LEGEND             ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 14:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("╠══════════════════════════╣");
                break;
            case 15:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(" 🤴🏻 = You (Player)       ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 16:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" 🧱 = Wall                ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 17:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" ▓▓ = Exit                ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 18:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" 💀 = Enemy               ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 19:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(" ❤️ = Health Potion       ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 20:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(" ⚔️ = Sword                ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 21:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(" 🛡️ = Shield               ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 22:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(" 🗝️ = Key                  ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");
                break;
            case 23:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("╚══════════════════════════╝");
                break;
            default:
                Console.Write(new string(' ', 28));
                break;
        }
    }

    
    
    
    private static void PrintHealthBar(int health, int maxHealth, int barWidth)
    {
        int filled = (int)((float)health / maxHealth * barWidth);
        filled = Math.Clamp(filled, 0, barWidth);

        ConsoleColor barColor = health > maxHealth * 0.6 ? ConsoleColor.Green :
                                health > maxHealth * 0.3 ? ConsoleColor.Yellow :
                                ConsoleColor.Red;

        Console.ForegroundColor = barColor;
        Console.Write("[");
        for (int i = 0; i < barWidth; i++)
        {
            Console.Write(i < filled ? "█" : " ");
        }
        Console.Write("]");
    }

    
    
    
    private static Enemy? FindEnemyAt(GameState state, int r, int c)
    {
        
        foreach (var e in state.Enemies)
        {
            if (e.Row == r && e.Col == c && e.IsAlive)
                return e;
        }
        return null;
    }

    
    
    
    public static void RenderTitleScreen()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine();
        Console.WriteLine(@"    ╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine(@"    ║                                                                  ║");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"    ║   ███╗   ███╗ █████╗ ███████╗███████╗     ██████╗ ██╗   ██╗      ║");
        Console.WriteLine(@"    ║   ████╗ ████║██╔══██╗╚══███╔╝██╔════╝    ██╔═══██╗██║   ██║      ║");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(@"    ║   ██╔████╔██║███████║  ███╔╝ █████╗      ██║   ██║██║   ██║      ║");
        Console.WriteLine(@"    ║   ██║╚██╔╝██║██╔══██║ ███╔╝  ██╔══╝      ██║▄▄ ██║██║   ██║      ║");
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine(@"    ║   ██║ ╚═╝ ██║██║  ██║███████╗███████╗    ╚██████╔╝╚██████╔╝      ║");
        Console.WriteLine(@"    ║   ╚═╝     ╚═╝╚═╝  ╚═╝╚══════╝╚══════╝     ╚══▀▀═╝  ╚═════╝       ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(@"    ║                                                                  ║");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(@"    ║          ╔═══════════════════════════════════╗                   ║");
        Console.WriteLine(@"    ║          ║   E S C A P E   T H E   M A Z E   ║                   ║");
        Console.WriteLine(@"    ║          ╚═══════════════════════════════════╝                   ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(@"    ║                                                                  ║");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(@"    ║         A Data Structures & Algorithms Project                   ║");
        Console.WriteLine(@"    ║                 Colegio De Naujan © 2026                         ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(@"    ║                                                                  ║");
        Console.WriteLine(@"    ╚══════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("          ┌─────────────────────────────────────┐");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("          │         [1]  NEW GAME               │");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("          │         [2]  LEADERBOARD            │");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("          │         [3]  HOW TO PLAY            │");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("          │         [4]  EXIT                   │");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("          └─────────────────────────────────────┘");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("          Select an option: ");
        Console.ResetColor();
    }

    
    
    
    public static void RenderHowToPlay()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine();
        Console.WriteLine("    ╔══════════════════════════════════════════════════════╗");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("    ║              H O W   T O   P L A Y                   ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("    ╠══════════════════════════════════════════════════════╣");
        Console.WriteLine("    ║                                                      ║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("    ║   OBJECTIVE                                          ║");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("    ║   Navigate the maze and reach the exit (▓▓).         ║");
        Console.WriteLine("    ║   Survive 5 levels to win!                           ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("    ║                                                      ║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("    ║   CONTROLS                                           ║");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("    ║   W/↑ = Up        S/↓ = Down                         ║");
        Console.WriteLine("    ║   A/← = Left      D/→ = Right                        ║");
        Console.WriteLine("    ║   U = Undo Move   H = Use Health Potion              ║");
        Console.WriteLine("    ║   Q = Quit Game                                      ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("    ║                                                      ║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("    ║   ITEMS                                              ║");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("    ║   ❤️");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(" = Health Potion (restores 40 HP)               ║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("    ║   ⚔️");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(" = Sword (increases attack damage)              ║");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("    ║   🛡️");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(" = Shield (reduces damage taken)                ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("    ║                                                      ║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("    ║   ENEMIES                                            ║");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("    ║   Walk into enemies to fight them.                   ║");
        Console.WriteLine("    ║   Enemies move toward you each turn!                 ║");
        Console.WriteLine("    ║   Collect swords to deal more damage.                ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("    ║                                                      ║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("    ║   FOG OF WAR                                         ║");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("    ║   You can only see nearby areas. Beware!             ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("    ║                                                      ║");
        Console.WriteLine("    ╚══════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("    Press any key to return to menu...");
        Console.ResetColor();
        Console.ReadKey(true);
    }

    
    
    
    public static void RenderGameOver(GameState state)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine();
        Console.WriteLine(@"    ╔══════════════════════════════════════════════════════╗");
        Console.WriteLine(@"    ║                                                      ║");
        Console.WriteLine(@"    ║         ██████╗  █████╗ ███╗   ███╗███████╗          ║");
        Console.WriteLine(@"    ║        ██╔════╝ ██╔══██╗████╗ ████║██╔════╝          ║");
        Console.WriteLine(@"    ║        ██║  ███╗███████║██╔████╔██║█████╗            ║");
        Console.WriteLine(@"    ║        ██║   ██║██╔══██║██║╚██╔╝██║██╔══╝            ║");
        Console.WriteLine(@"    ║        ╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗          ║");
        Console.WriteLine(@"    ║         ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝          ║");
        Console.WriteLine(@"    ║                                                      ║");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(@"    ║         ██████╗ ██╗   ██╗███████╗██████╗             ║");
        Console.WriteLine(@"    ║        ██╔═══██╗██║   ██║██╔════╝██╔══██╗            ║");
        Console.WriteLine(@"    ║        ██║   ██║██║   ██║█████╗  ██████╔╝            ║");
        Console.WriteLine(@"    ║        ██║   ██║╚██╗ ██╔╝██╔══╝  ██╔══██╗            ║");
        Console.WriteLine(@"    ║        ╚██████╔╝ ╚████╔╝ ███████╗██║  ██║            ║");
        Console.WriteLine(@"    ║         ╚═════╝   ╚═══╝  ╚══════╝╚═╝  ╚═╝            ║");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(@"    ║                                                      ║");
        Console.WriteLine(@"    ╚══════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"              Final Score: {state.Score}");
        Console.WriteLine($"              Level Reached: {state.CurrentLevel}");
        Console.WriteLine($"              Enemies Defeated: {state.TotalEnemiesDefeated}");
        Console.WriteLine($"              Items Collected: {state.TotalItemsCollected}");
        Console.WriteLine();
        Console.ResetColor();
    }

    
    
    
    public static void RenderVictory(GameState state)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine();
        Console.WriteLine(@"    ╔══════════════════════════════════════════════════════╗");
        Console.WriteLine(@"    ║                                                      ║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"    ║    ██╗   ██╗██╗ ██████╗████████╗ ██████╗ ██████╗ ██╗ ║");
        Console.WriteLine(@"    ║    ██║   ██║██║██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗╚██╗║");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(@"    ║    ██║   ██║██║██║        ██║   ██║   ██║██████╔╝ ██║║");
        Console.WriteLine(@"    ║    ╚██╗ ██╔╝██║██║        ██║   ██║   ██║██╔══██╗ ██║║");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"    ║     ╚████╔╝ ██║╚██████╗   ██║   ╚██████╔╝██║  ██║██╔╝║");
        Console.WriteLine(@"    ║      ╚═══╝  ╚═╝ ╚═════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝╚═╝ ║");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(@"    ║                                                      ║");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(@"    ║       You conquered all 5 levels of the maze!        ║");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(@"    ║                                                      ║");
        Console.WriteLine(@"    ╚══════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"              Final Score: {state.Score}");
        Console.WriteLine($"              Enemies Defeated: {state.TotalEnemiesDefeated}");
        Console.WriteLine($"              Items Collected: {state.TotalItemsCollected}");
        Console.WriteLine();
        Console.ResetColor();
    }

    
    
    
    public static void RenderLevelTransition(int level)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine();
        Console.WriteLine("    ╔══════════════════════════════════════════╗");
        Console.WriteLine("    ║                                          ║");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("    ║           ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"L E V E L   {level}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("                ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("    ║                                          ║");

        string[] difficulty = { "Beginner", "Apprentice", "Warrior", "Champion", "Legendary" };
        string diff = difficulty[Math.Clamp(level - 1, 0, difficulty.Length - 1)];
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write($"    ║          Difficulty: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{diff,-16}");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("   ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("    ║                                          ║");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("    ║       Get ready to enter the maze...     ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("    ║                                          ║");
        Console.WriteLine("    ╚══════════════════════════════════════════╝");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("    Press any key to start...");
        Console.ResetColor();
        Console.ReadKey(true);
    }
}
