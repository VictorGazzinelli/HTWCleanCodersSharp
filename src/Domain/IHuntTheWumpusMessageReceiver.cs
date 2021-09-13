namespace HTWGame.Domain
{
    public interface IHuntTheWumpusMessageReceiver
    {
        void NoPassage();
        void HearBats();
        void HearPit();
        void SmellWumpus();
        void Passage(Direction direction);
        void NoArrows();
        void ArrowShot();
        void PlayerShootsSelfInBack();
        void PlayerKillsWumpus();
        void PlayerShootsWall();
        void ArrowsFound(int arrowsFound);
        void FellInPit();
        void PlayerMovesToWumpus();
        void WumpusMovesToPlayer();
        void BatsTransport();
    }
}