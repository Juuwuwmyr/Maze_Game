namespace MazeQuest;

public static class Algorithms
{
    
    
    
    
    
    
    
    
    public static List<(int row, int col)> BFS(int[,] maze, (int row, int col) start, (int row, int col) goal)
    {
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);
        bool[,] visited = new bool[rows, cols];
        var parent = new Dictionary<(int, int), (int, int)?>();
        var queue = new Queue<(int row, int col)>();

        queue.Enqueue(start);
        visited[start.row, start.col] = true;
        parent[start] = null;

        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == goal)
            {
                
                var path = new List<(int, int)>();
                (int, int)? node = goal;
                while (node != null)
                {
                    path.Add(node.Value);
                    node = parent[node.Value];
                }
                path.Reverse();
                return path;
            }

            for (int i = 0; i < 4; i++)
            {
                int nr = current.row + dr[i];
                int nc = current.col + dc[i];

                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols &&
                    !visited[nr, nc] && maze[nr, nc] != (int)CellType.Wall)
                {
                    visited[nr, nc] = true;
                    parent[(nr, nc)] = current;
                    queue.Enqueue((nr, nc));
                }
            }
        }

        return new List<(int, int)>(); 
    }

    
    
    
    
    
    
    
    
    public static (int row, int col) LinearSearch(int[,] maze, int targetType)
    {
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (maze[r, c] == targetType)
                    return (r, c);
            }
        }
        return (-1, -1);
    }

    
    
    
    public static List<(int row, int col)> LinearSearchAll(int[,] maze, int targetType)
    {
        var results = new List<(int, int)>();
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (maze[r, c] == targetType)
                    results.Add((r, c));
            }
        }
        return results;
    }

    
    
    
    
    
    
    
    
    
    public static int BinarySearchRank(List<int> sortedScores, int targetScore)
    {
        int low = 0;
        int high = sortedScores.Count - 1;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (sortedScores[mid] == targetScore)
                return mid;
            else if (sortedScores[mid] > targetScore)
                low = mid + 1; 
            else
                high = mid - 1;
        }
        return low; 
    }

    
    
    
    
    
    
    
    public static List<ScoreEntry> MergeSort(List<ScoreEntry> entries)
    {
        if (entries.Count <= 1)
            return new List<ScoreEntry>(entries);

        int mid = entries.Count / 2;
        var left = MergeSort(entries.GetRange(0, mid));
        var right = MergeSort(entries.GetRange(mid, entries.Count - mid));

        return Merge(left, right);
    }

    private static List<ScoreEntry> Merge(List<ScoreEntry> left, List<ScoreEntry> right)
    {
        var result = new List<ScoreEntry>();
        int i = 0, j = 0;

        while (i < left.Count && j < right.Count)
        {
            if (left[i].Score >= right[j].Score)
                result.Add(left[i++]);
            else
                result.Add(right[j++]);
        }

        while (i < left.Count) result.Add(left[i++]);
        while (j < right.Count) result.Add(right[j++]);

        return result;
    }

    
    
    
    
    
    
    
    
    public static bool RecursiveSolve(int[,] maze, (int row, int col) current,
        (int row, int col) goal, bool[,] visited)
    {
        
        if (current == goal)
            return true;

        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);

        if (current.row < 0 || current.row >= rows ||
            current.col < 0 || current.col >= cols)
            return false;

        if (visited[current.row, current.col] ||
            maze[current.row, current.col] == (int)CellType.Wall)
            return false;

        
        visited[current.row, current.col] = true;

        
        if (RecursiveSolve(maze, (current.row - 1, current.col), goal, visited)) return true;
        if (RecursiveSolve(maze, (current.row + 1, current.col), goal, visited)) return true;
        if (RecursiveSolve(maze, (current.row, current.col - 1), goal, visited)) return true;
        if (RecursiveSolve(maze, (current.row, current.col + 1), goal, visited)) return true;

        return false;
    }
}
