namespace Template.Maze
{
    public interface IMazeSaveService
    {
        bool IsDirty { get; }

        MazeSessionState PrepareMazeForEntry(MazeConfig config);
        MazeSessionState PrepareNewMaze(MazeConfig config);
        bool IsCoinCollected(string coinId);
        void MarkCoinCollected(string coinId);
        bool TryMarkMazeCompleted();
        void SaveMaze();
        void LoadMaze();
        void DeleteMaze();
    }
}
