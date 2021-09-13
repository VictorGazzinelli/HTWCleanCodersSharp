namespace HTWGame.Domain
{
    public interface IHuntTheWumpus
    {
        string GetPlayerCavern();
        void AddBatCavern(string cavern);
        void AddPitCavern(string cavern);
        string GetWumpusCavern();
        int GetQuiver();
        int GetArrowsInCavern(string cavern);
        void ConnectCavern(string from, string to, Direction direction);
        string FindDestination(string cavern, Direction direction);
        ICommand MakeRestCommand();
        ICommand MakeShootCommand(Direction direction);
        ICommand MakeMoveCommand(Direction direction);
        void SetPlayerCavern(string playerCavern);
        void SetWumpusCavern(string wumpusCavern);
        void SetQuiver(int arrows);
        void SetArrowsIn(string cavern, int arrows);
    }
}