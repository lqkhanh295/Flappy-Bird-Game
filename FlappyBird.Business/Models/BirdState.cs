namespace FlappyBird.Business.Models
{
    public class BirdState
    {
        public double Speed { get; set; }
        public double Rotation { get; set; }
        public BirdAnimationState AnimationState { get; set; } = BirdAnimationState.Flying;
        public int FrameIndex { get; set; } = 0;
        public double X { get; set; } = 70;
        public double Y { get; set; } = 247;
    }

    public enum BirdAnimationState
    {
        Flying,     // Wings flapping, bird going up
        Falling,    // Wings down, bird falling
        Dead        // Game over state
    }
}

