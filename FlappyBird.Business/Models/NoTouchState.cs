namespace FlappyBird.Business.Models
{
    public class NoTouchState
    {
        public double X { get; set; }
        public double BaseY { get; set; }
        public double CurrentY { get; set; }
        public bool IsOscillating { get; set; }
        public double OscillationAmplitude { get; set; }
        public double OscillationPhase { get; set; }
        public double OscillationSpeed { get; set; }
        public double SpawnX { get; set; }
    }
}

