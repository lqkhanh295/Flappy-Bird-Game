namespace FlappyBird.Business.Models
{
    public class PipePairState
    {
        public double X { get; set; } // Vị trí X của pipe
        public double BaseTopHeight { get; set; }
        public double CurrentTopHeight { get; set; }
        public double TargetTopHeight { get; set; }
        public double MinTopHeight { get; set; }
        public double MinBottomHeight { get; set; }
        public double AnimationSpeed { get; set; }
        public double TargetMovementSpeed { get; set; }
        public bool EnableVerticalAnimation { get; set; }
        public bool IsMoving { get; set; }
        public bool IsOscillating { get; set; }
        public bool HasTargetMovement { get; set; }
        public bool IsJumpPattern { get; set; }
        public double AnimationPhase { get; set; }
        public double AnimationAmplitude { get; set; }
        public double TargetStopX { get; set; }
        public double FirstTargetHeight { get; set; }
        public double SecondTargetHeight { get; set; }
        public double JumpTargetHeight { get; set; }
        public int TargetMovementStage { get; set; }
        public int AnimationDelayFrames { get; set; }
        public int AnimationFrameCount { get; set; }
        public int GroupId { get; set; } = -1;
        public bool IsGroupLeader { get; set; } = false;
        public int GroupIndex { get; set; } = 0;
    }
}

