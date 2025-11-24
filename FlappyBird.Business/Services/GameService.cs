namespace FlappyBird.Business.Services
{
    public class GameService
    {
        public int Score { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public bool IsGameOver { get; private set; } = false;
        public bool IsPlaying { get; private set; } = false;
        public int FrameCount { get; private set; } = 0; // Đếm frame để tối ưu collision detection
        public int GraceTicksRemaining { get; private set; } = 0;

        private const int StartGraceTicks = 60;

        public void Reset(int initialHighScore = 0)
        {
            Score = 0;
            HighScore = initialHighScore;
            IsGameOver = false;
            IsPlaying = false;
            FrameCount = 0;
            GraceTicksRemaining = StartGraceTicks;
        }

        public void StartGame()
        {
            IsPlaying = true;
            IsGameOver = false;
            Score = 0;
            FrameCount = 0;
            GraceTicksRemaining = StartGraceTicks;
        }

        public void EndGame()
        {
            IsGameOver = true;
            IsPlaying = false;
        }

        public void IncrementScore()
        {
            Score++;
        }

        public void UpdateFrame()
        {
            FrameCount++; // Tăng frame counter để tối ưu collision detection
            if (GraceTicksRemaining > 0)
                GraceTicksRemaining--;
        }

        public void UpdateHighScore(int newHighScore)
        {
            if (newHighScore > HighScore)
            {
                HighScore = newHighScore;
            }
        }
    }
}

