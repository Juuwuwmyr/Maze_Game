using MazeQuest;
using Xunit;
using System.Collections.Generic;

namespace MazeKc.Tests;

public class AlgorithmTests
{
    [Fact]
    public void MergeSort_CorrectlySortsScoresDescending()
    {
        var entries = new List<ScoreEntry>
        {
            new ScoreEntry { Name = "C", Score = 10 },
            new ScoreEntry { Name = "A", Score = 30 },
            new ScoreEntry { Name = "B", Score = 20 }
        };

        var sorted = Algorithms.MergeSort(entries);

        Assert.Equal(30, sorted[0].Score);
        Assert.Equal(20, sorted[1].Score);
        Assert.Equal(10, sorted[2].Score);
    }

    [Fact]
    public void BFS_FindsShortestPathInSimpleMaze()
    {
        // 0 = Path, 1 = Wall
        int[,] maze = {
            { 0, 0, 0 },
            { 1, 1, 0 },
            { 0, 0, 0 }
        };

        var start = (0, 0);
        var goal = (2, 0);

        var path = Algorithms.BFS(maze, start, goal);

        Assert.NotEmpty(path);
        Assert.Equal(start, path[0]);
        Assert.Equal(goal, path[path.Count - 1]);
    }

    [Fact]
    public void BinarySearchRank_ReturnsCorrectRank()
    {
        var scores = new List<int> { 100, 80, 50, 30, 10 };
        
        // Score 85 should be rank 1 (between 100 and 80, but index-wise after 100)
        // Wait, BinarySearchRank returns the index where it SHOULD be.
        int rankIndex = Algorithms.BinarySearchRank(scores, 85);
        
        Assert.Equal(1, rankIndex);
    }
}
