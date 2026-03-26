namespace MazeQuest;

public static class Player
{
    private static readonly Random Rng = new();

    
    
    
    public static bool ProcessInput(ConsoleKey key, GameState state)
    {
        switch (key)
        {
            case ConsoleKey.W:
            case ConsoleKey.UpArrow:
                return TryMove(state, -1, 0);

            case ConsoleKey.S:
            case ConsoleKey.DownArrow:
                return TryMove(state, 1, 0);

            case ConsoleKey.A:
            case ConsoleKey.LeftArrow:
                return TryMove(state, 0, -1);

            case ConsoleKey.D:
            case ConsoleKey.RightArrow:
                return TryMove(state, 0, 1);

            case ConsoleKey.U: 
                return UndoMove(state);

            case ConsoleKey.H: 
                return UseHealthPotion(state);

            case ConsoleKey.Q: 
                state.IsRunning = false;
                return true;

            default:
                return false;
        }
    }

    
    
    
    private static bool TryMove(GameState state, int dr, int dc)
    {
        int newRow = state.PlayerRow + dr;
        int newCol = state.PlayerCol + dc;

        
        if (newRow < 0 || newRow >= state.MazeRows || newCol < 0 || newCol >= state.MazeCols)
            return false;

        int cell = state.Maze[newRow, newCol];

        
        if (cell == (int)CellType.Wall)
            return false;

        
        state.MoveHistory.Push((state.PlayerRow, state.PlayerCol));

        
        switch ((CellType)cell)
        {
            case CellType.Path:
                SoundManager.PlayMove();
                MoveTo(state, newRow, newCol);
                break;

            case CellType.Exit:
                SoundManager.PlayLevelComplete();
                MoveTo(state, newRow, newCol);
                HandleExit(state);
                break;

            case CellType.Enemy:
                HandleCombat(state, newRow, newCol);
                break;

            case CellType.HealthPotion:
                SoundManager.PlayItemPickup();
                state.AddItem("HealthPotion");
                state.SetMessage("» Found a ❤️ Health Potion! (+1)", ConsoleColor.Green);
                state.Score += 10;
                MoveTo(state, newRow, newCol);
                break;

            case CellType.Sword:
                SoundManager.PlayItemPickup();
                state.AddItem("Sword");
                state.SetMessage("» Found a ⚔️ Sword! (+1) Attack power increased!", ConsoleColor.Yellow);
                state.Score += 15;
                MoveTo(state, newRow, newCol);
                break;

            case CellType.Shield:
                SoundManager.PlayItemPickup();
                state.AddItem("Shield");
                state.SetMessage("» Found a 🛡️ Shield! (+1) Defense increased!", ConsoleColor.Cyan);
                state.Score += 15;
                MoveTo(state, newRow, newCol);
                break;

            case CellType.Key:
                SoundManager.PlayItemPickup();
                state.AddItem("Key");
                state.SetMessage("» Found a 🗝️ Key! (+1)", ConsoleColor.DarkYellow);
                state.Score += 20;
                MoveTo(state, newRow, newCol);
                break;

            case CellType.Trap:
                SoundManager.PlayTrap();
                int trapDmg = 10 + (state.CurrentLevel * 5);
                int shieldReduction = state.GetItemCount("Shield") * 3;
                trapDmg = Math.Max(5, trapDmg - shieldReduction);
                state.Health -= trapDmg;
                state.SetMessage($"» Stepped on a trap! -{trapDmg} HP!", ConsoleColor.Red);
                MoveTo(state, newRow, newCol);
                if (state.Health <= 0)
                    HandleDeath(state);
                break;
        }

        
        ProcessEnemyTurns(state);

        state.TickMessage();
        return true;
    }

    private static void MoveTo(GameState state, int newRow, int newCol)
    {
        state.Maze[state.PlayerRow, state.PlayerCol] = (int)CellType.Path;
        state.PlayerRow = newRow;
        state.PlayerCol = newCol;
        state.Maze[newRow, newCol] = (int)CellType.Player;
        state.Score += 1;
    }

    
    
    
    
    
    
    
    private static bool UndoMove(GameState state)
    {
        if (state.MoveHistory.Count == 0)
        {
            state.SetMessage("» No moves to undo!", ConsoleColor.DarkGray);
            return false;
        }

        var (prevRow, prevCol) = state.MoveHistory.Pop();

        
        if (state.Maze[prevRow, prevCol] == (int)CellType.Path)
        {
            SoundManager.PlayUndo();
            state.Maze[state.PlayerRow, state.PlayerCol] = (int)CellType.Path;
            state.PlayerRow = prevRow;
            state.PlayerCol = prevCol;
            state.Maze[prevRow, prevCol] = (int)CellType.Player;
            state.Score = Math.Max(0, state.Score - 1);
            state.SetMessage("» Move undone! (Backtracked)", ConsoleColor.DarkYellow);
            return true;
        }

        state.SetMessage("» Cannot undo — path blocked!", ConsoleColor.DarkGray);
        return false;
    }

    
    
    
    private static void HandleCombat(GameState state, int enemyRow, int enemyCol)
    {
        
        Enemy? enemy = null;
        foreach (var e in state.Enemies)
        {
            if (e.Row == enemyRow && e.Col == enemyCol && e.IsAlive)
            {
                enemy = e;
                break;
            }
        }

        if (enemy == null)
        {
            MoveTo(state, enemyRow, enemyCol);
            return;
        }

        
        SoundManager.PlayAttack();
        int playerDmg = 15 + (state.GetItemCount("Sword") * 10);
        bool killed = enemy.TakeDamage(playerDmg);

        if (killed)
        {
            SoundManager.PlayEnemyKill();
            state.SetMessage($"» Defeated the {enemy.Name}! +{20 + enemy.Level * 5} points!", ConsoleColor.Green);
            state.Score += 20 + enemy.Level * 5;
            state.TotalEnemiesDefeated++;
            MoveTo(state, enemyRow, enemyCol);
        }
        else
        {
            
            int enemyDmg = enemy.Damage;
            int shieldReduction = state.GetItemCount("Shield") * 5;
            enemyDmg = Math.Max(3, enemyDmg - shieldReduction);
            state.Health -= enemyDmg;
            SoundManager.PlayHurt();

            state.SetMessage(
                $"» Fighting {enemy.Name}! You dealt {playerDmg} dmg. " +
                $"Enemy hit you for {enemyDmg} dmg! (Enemy HP: {enemy.Health}/{enemy.MaxHealth})",
                ConsoleColor.DarkYellow);

            
            state.MoveHistory.Pop(); 

            if (state.Health <= 0)
                HandleDeath(state);
        }
    }

    
    
    
    private static bool UseHealthPotion(GameState state)
    {
        if (state.UseItem("HealthPotion"))
        {
            SoundManager.PlayHeal();
            int healAmount = 40;
            state.Health = Math.Min(state.MaxHealth, state.Health + healAmount);
            state.SetMessage($"» Used ❤️ Health Potion! +{healAmount} HP", ConsoleColor.Green);
            return true;
        }

        state.SetMessage("» No health potions left!", ConsoleColor.Red);
        return false;
    }

    
    
    
    private static void HandleExit(GameState state)
    {
        state.Score += 100 * state.CurrentLevel;

        if (state.CurrentLevel >= GameState.MaxLevel)
        {
            state.HasWon = true;
            state.IsGameOver = true;
        }
        else
        {
            state.SetMessage($"» Level {state.CurrentLevel} complete! Score +{100 * state.CurrentLevel}", ConsoleColor.Green);
        }
    }

    
    
    
    private static void HandleDeath(GameState state)
    {
        state.Lives--;
        if (state.Lives <= 0)
        {
            state.IsGameOver = true;
            state.SetMessage("» You have been defeated...", ConsoleColor.DarkRed);
        }
        else
        {
            state.Health = state.MaxHealth;
            state.SetMessage($"» You died! Lives remaining: {state.Lives}", ConsoleColor.Red);

            
            state.Maze[state.PlayerRow, state.PlayerCol] = (int)CellType.Path;
            state.PlayerRow = 1;
            state.PlayerCol = 1;
            state.Maze[1, 1] = (int)CellType.Player;
            state.MoveHistory.Clear();
        }
    }

    
    
    
    
    
    
    
    private static void ProcessEnemyTurns(GameState state)
    {
        
        state.EnemyActionQueue.Clear();
        foreach (var enemy in state.Enemies)
        {
            if (enemy.IsAlive)
                state.EnemyActionQueue.Enqueue(enemy);
        }

        
        while (state.EnemyActionQueue.Count > 0)
        {
            var enemy = state.EnemyActionQueue.Dequeue();

            bool attacked = enemy.MoveTowardPlayer(state.Maze, state.PlayerRow, state.PlayerCol);

            if (attacked)
            {
                int dmg = enemy.Damage;
                int shieldReduction = state.GetItemCount("Shield") * 5;
                dmg = Math.Max(3, dmg - shieldReduction);
                state.Health -= dmg;
                SoundManager.PlayHurt();
                state.SetMessage($"» {enemy.Name} attacked you! -{dmg} HP!", ConsoleColor.Red);

                if (state.Health <= 0)
                {
                    HandleDeath(state);
                    break;
                }
            }
        }
    }
}
