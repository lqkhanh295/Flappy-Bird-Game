using FlappyBird.Business.Models;

namespace FlappyBird.Business.Services
{
    public class BirdService
    {
        private const double FlapThreshold = -3; // Ngưỡng tốc độ để chuyển sang animation bay
        private const double FallThreshold = 2; // Ngưỡng tốc độ để chuyển sang animation rơi
        private const double MaxUpRotation = -30; // Góc xoay tối đa khi bay lên
        private const double MaxDownRotation = 90; // Góc xoay tối đa khi rơi xuống
        private const double Gravity = 1.0; // Trọng lực mỗi frame
        private const double JumpStrength = -10.0; // Lực nhảy

        public BirdState BirdState { get; private set; } = new();

        public void Reset()
        {
            BirdState = new BirdState
            {
                Speed = 0,
                Rotation = 0,
                AnimationState = BirdAnimationState.Flying,
                FrameIndex = 0,
                X = 70,
                Y = 247
            };
        }

        public void Jump()
        {
            BirdState.Speed = JumpStrength;
            BirdState.AnimationState = BirdAnimationState.Flying;
            BirdState.FrameIndex = 0;
        }

        public void Update()
        {
            // Áp dụng trọng lực
            BirdState.Speed += Gravity;
            
            // Cập nhật trạng thái animation dựa trên tốc độ
            UpdateAnimationState();
            
            // Cập nhật góc xoay dựa trên tốc độ
            UpdateRotation();
        }

        private void UpdateAnimationState()
        {
            if (BirdState.AnimationState == BirdAnimationState.Dead)
                return;

            // Xác định trạng thái animation dựa trên tốc độ của chim
            if (BirdState.Speed < FlapThreshold)
            {
                // Chim đang bay lên - hiển thị animation bay
                if (BirdState.AnimationState != BirdAnimationState.Flying)
                {
                    BirdState.AnimationState = BirdAnimationState.Flying;
                    BirdState.FrameIndex = 0;
                }
            }
            else if (BirdState.Speed > FallThreshold)
            {
                // Chim đang rơi - hiển thị animation rơi
                if (BirdState.AnimationState != BirdAnimationState.Falling)
                {
                    BirdState.AnimationState = BirdAnimationState.Falling;
                    BirdState.FrameIndex = 0;
                }
            }
        }

        private void UpdateRotation()
        {
            if (BirdState.AnimationState == BirdAnimationState.Dead)
                return;

            // Tính toán góc xoay dựa trên tốc độ của chim
            // Tốc độ âm (bay lên) = xoay lên
            // Tốc độ dương (rơi) = xoay xuống
            double targetRotation = System.Math.Clamp(BirdState.Speed * 3, MaxUpRotation, MaxDownRotation);
            
            // Chuyển đổi góc xoay mượt mà
            BirdState.Rotation += (targetRotation - BirdState.Rotation) * 0.2;
        }

        public void SetDead()
        {
            BirdState.AnimationState = BirdAnimationState.Dead;
        }
    }
}

