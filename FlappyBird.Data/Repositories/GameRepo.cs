using System;
using System.IO;

namespace FlappyBird.Data.Repositories
{
    public class GameRepo
    {
        private readonly string filePath = "highscore.txt";

        public int LoadHighScore()
        {
            try
            {
                if (File.Exists(filePath) &&
                    int.TryParse(File.ReadAllText(filePath), out int score) &&
                    score >= 0)
                {
                    return score;
                }
            }
            catch
            {
                // ignore and fallback to zero
            }
            return 0;
        }

        public void SaveHighScore(int score)
        {
            try
            {
                File.WriteAllText(filePath, Math.Max(0, score).ToString());
            }
            catch
            {
                // ignore persistence errors
            }
        }
    }
}
