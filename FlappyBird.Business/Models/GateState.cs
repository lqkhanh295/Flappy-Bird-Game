namespace FlappyBird.Business.Models
{
    public class GateState
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double SpawnX { get; set; }
        public bool IsActivated { get; set; } = false;
    }
}

