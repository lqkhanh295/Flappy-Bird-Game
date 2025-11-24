using System;
using System.Collections.Generic;
using FlappyBird.Business.Models;

namespace FlappyBird.Business.Services
{
    public class StageService
    {
        private readonly Random rnd = new();
        
        // Constants
        private const double GroupPipeSpacing = 90;
        private const double PipeSpacing = 260;
        private const double FirstPipeStartLeft = 1100;
        private const int CanvasHeight = 500;
        private const int PipeWidth = 80;
        private const double Gap = 180;
        private const double CloudSpeed = 2;
        private const double BirdX = 70; // Vị trí X của chim

        // State collections
        public List<PipePairState> PipePairs { get; private set; } = new();
        public List<NoTouchState> NoTouchObstacles { get; private set; } = new();
        public List<GateState> Gates { get; private set; } = new();
        public List<CloudState> Clouds { get; private set; } = new();
        
        // Internal state
        private int nextGroupId = 0;
        private int totalPipesPassed = 0;
        private int nextNoTouchSpawnAt = -1;
        private int lastSpawnedPhase = -1;
        private int noTouchSpawnCount = 0;
        private bool isNight = false;
        
        // Day/Night state
        public bool IsNight => isNight;

        public void Reset()
        {
            PipePairs.Clear();
            NoTouchObstacles.Clear();
            Gates.Clear();
            Clouds.Clear();
            nextGroupId = 0;
            totalPipesPassed = 0;
            nextNoTouchSpawnAt = -1;
            lastSpawnedPhase = -1;
            noTouchSpawnCount = 0;
            isNight = false;
        }
        
        /// <summary>
        /// Toggle day/night mode. Returns the new night state.
        /// </summary>
        public bool ToggleDayNight()
        {
            isNight = !isNight;
            return isNight;
        }
        
        /// <summary>
        /// Reset to day mode.
        /// </summary>
        public void ResetToDay()
        {
            isNight = false;
        }
        
        /// <summary>
        /// Set night mode explicitly.
        /// </summary>
        public void SetNightMode(bool night)
        {
            isNight = night;
        }

        public void CreateInitialPipes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreatePipePair(FirstPipeStartLeft + i * PipeSpacing, 0);
            }
        }

        public PipePairState CreatePipePair(double leftPos, int score)
        {
            var pairState = new PipePairState
            {
                X = leftPos,
                GroupId = -1,
                IsGroupLeader = false,
                GroupIndex = 0
            };
            
            RandomizePipe(pairState, score);
            PipePairs.Add(pairState);
            return pairState;
        }

        public void RandomizePipe(PipePairState pair, int score)
        {
            int minTopHeight = 100;
            int minBottomHeight = 100;
            int maxTopHeight = CanvasHeight - (int)Gap - minBottomHeight;
            
            double baseTopHeight = rnd.Next(minTopHeight, maxTopHeight + 1);

            // Pattern variations
            double patternRoll = rnd.NextDouble();
            if (patternRoll < 0.15)
            {
                baseTopHeight = minTopHeight + rnd.Next(0, Math.Max(1, 40));
            }
            else if (patternRoll < 0.30)
            {
                baseTopHeight = maxTopHeight - rnd.Next(0, Math.Max(1, 40));
            }

            pair.BaseTopHeight = baseTopHeight;
            pair.CurrentTopHeight = baseTopHeight;

            // Animation logic based on score
            bool enableAnimation = false;
            double animationChance = 0.0;
            
            if (score >= 40)
                animationChance = 0.80;
            else if (score >= 30)
                animationChance = 0.80;
            else if (score >= 20)
                animationChance = 0.65;
            else if (score >= 10)
                animationChance = 0.50;
            
            enableAnimation = rnd.NextDouble() < animationChance;
            
            if (enableAnimation)
            {
                double maxAmplitude = Math.Max(0, Math.Min(baseTopHeight - minTopHeight, maxTopHeight - baseTopHeight));
                
                double baseAmplitude = score >= 40 ? 140 : score >= 30 ? 130 : score >= 20 ? 120 : 100;
                double amplitudeRange = score >= 40 ? 70 : score >= 30 ? 65 : score >= 20 ? 60 : 50;
                double desiredAmplitude = baseAmplitude + rnd.NextDouble() * amplitudeRange;
                double amplitude = maxAmplitude > 0 ? Math.Min(desiredAmplitude, maxAmplitude) : 0;
                
                double oscillationChance = 0.0;
                if (score >= 40)
                    oscillationChance = 0.80;
                else if (score >= 30)
                    oscillationChance = 0.80;
                else if (score >= 20)
                    oscillationChance = 0.65;
                else if (score >= 10)
                    oscillationChance = 0.50;
                
                bool useOscillation = rnd.NextDouble() < oscillationChance;
                bool useTarget = !useOscillation || (score >= 20 && rnd.NextDouble() < 0.4);
                
                bool useJumpPattern = false;
                if (score >= 20 && !useOscillation && useTarget)
                {
                    useJumpPattern = rnd.NextDouble() < 0.3;
                }
                
                int delayFrames = 0;
                if (score >= 20 && rnd.NextDouble() < 0.25)
                {
                    delayFrames = rnd.Next(10, 40);
                }
                pair.AnimationDelayFrames = delayFrames;
                pair.AnimationFrameCount = 0;
                
                if (useOscillation && amplitude > 20)
                {
                    pair.IsOscillating = true;
                    pair.AnimationAmplitude = amplitude;
                    pair.AnimationPhase = rnd.NextDouble() * Math.PI * 2;
                    double oscSpeed = score >= 40 ? 0.05 + rnd.NextDouble() * 0.02 : 
                                     score >= 30 ? 0.04 + rnd.NextDouble() * 0.02 :
                                     score >= 20 ? 0.04 + rnd.NextDouble() * 0.02 : 0.03 + rnd.NextDouble() * 0.02;
                    pair.AnimationSpeed = oscSpeed;
                    pair.EnableVerticalAnimation = true;
                    pair.IsMoving = false;
                    pair.HasTargetMovement = useTarget && score >= 20;
                    
                    if (pair.HasTargetMovement)
                    {
                        double moveAmplitude = amplitude * 1.8;
                        moveAmplitude = Math.Min(moveAmplitude, maxAmplitude);
                        bool moveUp = rnd.NextDouble() < 0.5;
                        double targetOffset = moveUp ? -moveAmplitude : moveAmplitude;
                        pair.TargetTopHeight = Math.Clamp(baseTopHeight + targetOffset, minTopHeight, maxTopHeight);
                        pair.IsMoving = true;
                        double baseSpeed = 0.5;
                        double speedMultiplier = 1.0 + (score * 0.01);
                        double moveSpeed = (baseSpeed + rnd.NextDouble() * 0.25) * Math.Min(speedMultiplier, 1.4);
                        pair.TargetMovementSpeed = moveSpeed;
                        pair.TargetStopX = BirdX + PipeSpacing * 1.0;
                    }
                }
                else if (useTarget)
                {
                    pair.IsOscillating = false;
                    pair.HasTargetMovement = false;
                    
                    if (useJumpPattern)
                    {
                        pair.IsJumpPattern = true;
                        double jumpAmplitude = Math.Min(amplitude * 2.0, maxAmplitude);
                        bool jumpUp = rnd.NextDouble() < 0.5;
                        double jumpOffset = jumpUp ? -jumpAmplitude : jumpAmplitude;
                        double jumpHeight = Math.Clamp(baseTopHeight + jumpOffset, minTopHeight, maxTopHeight);
                        pair.JumpTargetHeight = jumpHeight;
                        pair.TargetTopHeight = jumpHeight;
                        pair.TargetMovementStage = 1;
                        double jumpSpeed = 1.5 + rnd.NextDouble() * 0.8;
                        pair.AnimationSpeed = jumpSpeed;
                        pair.EnableVerticalAnimation = true;
                        pair.IsMoving = true;
                        pair.TargetStopX = BirdX + PipeSpacing * 1.0;
                    }
                    else
                    {
                        pair.IsJumpPattern = false;
                        double moveAmplitude = Math.Min(amplitude * 2.5, maxAmplitude);
                        bool moveUp = rnd.NextDouble() < 0.5;
                        double targetOffset = moveUp ? -moveAmplitude : moveAmplitude;
                        double targetHeight = Math.Clamp(baseTopHeight + targetOffset, minTopHeight, maxTopHeight);
                        pair.TargetTopHeight = targetHeight;
                        pair.TargetMovementStage = 1;
                        double baseSpeed = 0.7;
                        double speedMultiplier = 1.0 + (score * 0.01);
                        double moveSpeed = (baseSpeed + rnd.NextDouble() * 0.3) * Math.Min(speedMultiplier, 1.5);
                        pair.AnimationSpeed = moveSpeed;
                        pair.EnableVerticalAnimation = true;
                        pair.IsMoving = true;
                        pair.TargetStopX = BirdX + PipeSpacing * 1.0;
                    }
                }
            }
            else
            {
                pair.TargetTopHeight = baseTopHeight;
                pair.AnimationSpeed = 0;
                pair.EnableVerticalAnimation = false;
                pair.IsMoving = false;
                pair.IsOscillating = false;
                pair.HasTargetMovement = false;
                pair.IsJumpPattern = false;
                pair.FirstTargetHeight = baseTopHeight;
                pair.SecondTargetHeight = baseTopHeight;
                pair.TargetMovementStage = 0;
                pair.AnimationDelayFrames = 0;
                pair.AnimationFrameCount = 0;
            }

            pair.MinTopHeight = minTopHeight;
            pair.MinBottomHeight = minBottomHeight;
        }

        public void RandomizePipeAnimationOnly(PipePairState pair, int score)
        {
            double baseTopHeight = pair.BaseTopHeight;
            int minTopHeight = (int)pair.MinTopHeight;
            int minBottomHeight = (int)pair.MinBottomHeight;
            int maxTopHeight = CanvasHeight - (int)Gap - minBottomHeight;
            
            bool enableAnimation = false;
            double animationChance = 0.0;
            
            if (score >= 40)
                animationChance = 0.80;
            else if (score >= 30)
                animationChance = 0.80;
            else if (score >= 20)
                animationChance = 0.65;
            else if (score >= 10)
                animationChance = 0.50;
            
            enableAnimation = rnd.NextDouble() < animationChance;
            
            if (enableAnimation)
            {
                double maxAmplitude = Math.Max(0, Math.Min(baseTopHeight - minTopHeight, maxTopHeight - baseTopHeight));
                
                double baseAmplitude = score >= 40 ? 140 : score >= 30 ? 130 : score >= 20 ? 120 : 100;
                double amplitudeRange = score >= 40 ? 70 : score >= 30 ? 65 : score >= 20 ? 60 : 50;
                double desiredAmplitude = baseAmplitude + rnd.NextDouble() * amplitudeRange;
                double amplitude = maxAmplitude > 0 ? Math.Min(desiredAmplitude, maxAmplitude) : 0;
                
                double oscillationChance = 0.0;
                if (score >= 40)
                    oscillationChance = 0.80;
                else if (score >= 30)
                    oscillationChance = 0.80;
                else if (score >= 20)
                    oscillationChance = 0.65;
                else if (score >= 10)
                    oscillationChance = 0.50;
                
                bool useOscillation = rnd.NextDouble() < oscillationChance;
                bool useTarget = !useOscillation || (score >= 20 && rnd.NextDouble() < 0.4);
                
                bool useJumpPattern = false;
                if (score >= 20 && !useOscillation && useTarget)
                {
                    useJumpPattern = rnd.NextDouble() < 0.3;
                }
                
                int delayFrames = 0;
                if (score >= 20 && rnd.NextDouble() < 0.25)
                {
                    delayFrames = rnd.Next(10, 40);
                }
                pair.AnimationDelayFrames = delayFrames;
                pair.AnimationFrameCount = 0;
                
                if (useOscillation && amplitude > 20)
                {
                    pair.IsOscillating = true;
                    pair.AnimationAmplitude = amplitude;
                    pair.AnimationPhase = rnd.NextDouble() * Math.PI * 2;
                    double oscSpeed = score >= 40 ? 0.05 + rnd.NextDouble() * 0.02 : 
                                     score >= 30 ? 0.04 + rnd.NextDouble() * 0.02 :
                                     score >= 20 ? 0.04 + rnd.NextDouble() * 0.02 : 0.03 + rnd.NextDouble() * 0.02;
                    pair.AnimationSpeed = oscSpeed;
                    pair.EnableVerticalAnimation = true;
                    pair.IsMoving = false;
                    pair.HasTargetMovement = useTarget && score >= 20;
                    
                    if (pair.HasTargetMovement)
                    {
                        double moveAmplitude = amplitude * 1.8;
                        moveAmplitude = Math.Min(moveAmplitude, maxAmplitude);
                        bool moveUp = rnd.NextDouble() < 0.5;
                        double targetOffset = moveUp ? -moveAmplitude : moveAmplitude;
                        pair.TargetTopHeight = Math.Clamp(baseTopHeight + targetOffset, minTopHeight, maxTopHeight);
                        pair.IsMoving = true;
                        double baseSpeed = 0.5;
                        double speedMultiplier = 1.0 + (score * 0.01);
                        double moveSpeed = (baseSpeed + rnd.NextDouble() * 0.25) * Math.Min(speedMultiplier, 1.4);
                        pair.TargetMovementSpeed = moveSpeed;
                        pair.TargetStopX = BirdX + PipeSpacing * 1.0;
                    }
                }
                else if (useTarget)
                {
                    pair.IsOscillating = false;
                    pair.HasTargetMovement = false;
                    
                    if (useJumpPattern)
                    {
                        pair.IsJumpPattern = true;
                        double jumpAmplitude = Math.Min(amplitude * 2.0, maxAmplitude);
                        bool jumpUp = rnd.NextDouble() < 0.5;
                        double jumpOffset = jumpUp ? -jumpAmplitude : jumpAmplitude;
                        double jumpHeight = Math.Clamp(baseTopHeight + jumpOffset, minTopHeight, maxTopHeight);
                        pair.JumpTargetHeight = jumpHeight;
                        pair.TargetTopHeight = jumpHeight;
                        pair.TargetMovementStage = 1;
                        double baseSpeed = 2.0;
                        double speedMultiplier = 1.0 + (score * 0.01);
                        double jumpSpeed = (baseSpeed + rnd.NextDouble() * 0.5) * Math.Min(speedMultiplier, 1.5);
                        pair.TargetMovementSpeed = jumpSpeed;
                        pair.EnableVerticalAnimation = true;
                        pair.IsMoving = true;
                    }
                    else
                    {
                        pair.IsJumpPattern = false;
                        double moveAmplitude = amplitude * 2.5;
                        moveAmplitude = Math.Min(moveAmplitude, maxAmplitude);
                        bool moveUp = rnd.NextDouble() < 0.5;
                        double targetOffset = moveUp ? -moveAmplitude : moveAmplitude;
                        pair.TargetTopHeight = Math.Clamp(baseTopHeight + targetOffset, minTopHeight, maxTopHeight);
                        pair.IsMoving = true;
                        double baseSpeed = 0.5;
                        double speedMultiplier = 1.0 + (score * 0.01);
                        double moveSpeed = (baseSpeed + rnd.NextDouble() * 0.25) * Math.Min(speedMultiplier, 1.4);
                        pair.TargetMovementSpeed = moveSpeed;
                        pair.TargetStopX = BirdX + PipeSpacing * 1.0;
                        pair.EnableVerticalAnimation = true;
                        pair.HasTargetMovement = true;
                    }
                }
                else
                {
                    pair.EnableVerticalAnimation = false;
                    pair.IsOscillating = false;
                    pair.IsMoving = false;
                    pair.HasTargetMovement = false;
                    pair.IsJumpPattern = false;
                }
            }
            else
            {
                pair.EnableVerticalAnimation = false;
                pair.IsOscillating = false;
                pair.IsMoving = false;
                pair.HasTargetMovement = false;
                pair.IsJumpPattern = false;
                pair.AnimationDelayFrames = 0;
                pair.AnimationFrameCount = 0;
            }
        }

        public void ApplyPipeAnimation(PipePairState pair)
        {
            if (!pair.EnableVerticalAnimation)
                return;

            if (pair.AnimationFrameCount < pair.AnimationDelayFrames)
            {
                pair.AnimationFrameCount++;
                return;
            }

            double targetOffset = 0;

            if (pair.IsOscillating)
            {
                pair.AnimationPhase += pair.AnimationSpeed;
                if (pair.AnimationPhase > Math.PI * 2)
                {
                    pair.AnimationPhase -= Math.PI * 2;
                }
                
                double oscOffset = Math.Sin(pair.AnimationPhase) * pair.AnimationAmplitude;
                targetOffset = oscOffset;
                
                if (pair.HasTargetMovement && pair.IsMoving)
                {
                    if (pair.X <= pair.TargetStopX)
                    {
                        pair.IsMoving = false;
                        pair.BaseTopHeight = pair.CurrentTopHeight - oscOffset;
                    }
                    else
                    {
                        double distance = pair.TargetTopHeight - pair.BaseTopHeight;
                        double moveSpeed = pair.HasTargetMovement ? pair.TargetMovementSpeed : pair.AnimationSpeed;
                        double moveStep = moveSpeed * Math.Sign(distance);
                        
                        if (Math.Abs(distance) <= Math.Abs(moveStep))
                        {
                            pair.BaseTopHeight = pair.TargetTopHeight;
                            pair.IsMoving = false;
                        }
                        else
                        {
                            pair.BaseTopHeight += moveStep;
                        }
                    }
                }
                
                pair.CurrentTopHeight = pair.BaseTopHeight + targetOffset;
            }
            else if (pair.IsMoving)
            {
                if (pair.X <= pair.TargetStopX)
                {
                    pair.IsMoving = false;
                    pair.TargetMovementStage = 3;
                }
                else
                {
                    if (pair.IsJumpPattern)
                    {
                        double distance = pair.JumpTargetHeight - pair.CurrentTopHeight;
                        double moveStep = pair.AnimationSpeed * Math.Sign(distance);
                        
                        if (Math.Abs(distance) <= Math.Abs(moveStep))
                        {
                            pair.CurrentTopHeight = pair.JumpTargetHeight;
                            pair.IsMoving = false;
                            pair.TargetMovementStage = 3;
                        }
                        else
                        {
                            pair.CurrentTopHeight += moveStep;
                        }
                    }
                    else
                    {
                        double distance = pair.TargetTopHeight - pair.CurrentTopHeight;
                        double moveStep = pair.AnimationSpeed * Math.Sign(distance);
                        
                        if (Math.Abs(distance) <= Math.Abs(moveStep))
                        {
                            pair.CurrentTopHeight = pair.TargetTopHeight;
                            pair.IsMoving = false;
                            pair.TargetMovementStage = 3;
                        }
                        else
                        {
                            pair.CurrentTopHeight += moveStep;
                        }
                    }
                }
            }
        }

        public void UpdatePipePositions(double speed)
        {
            foreach (var pair in PipePairs)
            {
                pair.X -= speed;
            }
        }

        public void SpawnNoTouchGroup(int count, double startX)
        {
            for (int i = 0; i < count; i++)
            {
                double spawnX = startX + (i * 150);
                double spawnY = rnd.Next(80, CanvasHeight - 120);
                
                var state = new NoTouchState
                {
                    X = spawnX,
                    BaseY = spawnY,
                    CurrentY = spawnY,
                    IsOscillating = rnd.NextDouble() < 0.6,
                    OscillationAmplitude = rnd.Next(25, 50),
                    OscillationPhase = rnd.NextDouble() * Math.PI * 2,
                    OscillationSpeed = 0.03 + rnd.NextDouble() * 0.02,
                    SpawnX = spawnX
                };
                
                NoTouchObstacles.Add(state);
            }
            
            noTouchSpawnCount++;
            
            if (noTouchSpawnCount >= 2)
            {
                CreateGate(startX + (count * 150) + 200);
                noTouchSpawnCount = 0;
            }
        }

        public void CreateGate(double gateX)
        {
            var gateState = new GateState
            {
                X = gateX,
                Y = (CanvasHeight - 80) / 2,
                SpawnX = gateX,
                IsActivated = false
            };
            
            Gates.Add(gateState);
        }

        public void UpdateNoTouchPositions(double speed)
        {
            foreach (var noTouch in NoTouchObstacles)
            {
                noTouch.X -= speed;

                if (noTouch.IsOscillating)
                {
                    noTouch.OscillationPhase += noTouch.OscillationSpeed;
                    noTouch.CurrentY = noTouch.BaseY + Math.Sin(noTouch.OscillationPhase) * noTouch.OscillationAmplitude;
                }
            }
        }

        public void UpdateGatePositions(double speed)
        {
            foreach (var gate in Gates)
            {
                gate.X -= speed;
            }
        }

        public bool ShouldSpawnNoTouch(int totalPipesPassed, out int noTouchCount, out int spawnAt)
        {
            spawnAt = -1;
            noTouchCount = 0;
            
            if (totalPipesPassed > 10)
            {
                int currentPhase = (totalPipesPassed - 1) / 10;
                int phaseStartPipe = currentPhase * 10 + 1;
                int phaseEndPipe = (currentPhase + 1) * 10;
                noTouchCount = Math.Min(9, currentPhase);
                
                if (noTouchCount > 0 && currentPhase != lastSpawnedPhase && nextNoTouchSpawnAt == -1)
                {
                    nextNoTouchSpawnAt = rnd.Next(phaseStartPipe, phaseEndPipe + 1);
                }
                
                if (totalPipesPassed >= nextNoTouchSpawnAt && nextNoTouchSpawnAt > 0 && currentPhase != lastSpawnedPhase)
                {
                    spawnAt = nextNoTouchSpawnAt;
                    lastSpawnedPhase = currentPhase;
                    nextNoTouchSpawnAt = -1;
                    return true;
                }
            }
            
            return false;
        }

        public void OnPipePassed()
        {
            totalPipesPassed++;
        }

        public int GetTotalPipesPassed() => totalPipesPassed;

        public bool ShouldCreateGroup(int score)
        {
            double groupChance = score >= 50 ? 0.65 : (score >= 15 ? 0.45 : 0.0);
            return score >= 15 && rnd.NextDouble() < groupChance;
        }

        public int GetGroupSize(int score)
        {
            if (score >= 15 && score <= 40)
            {
                return rnd.Next(2, 4);
            }
            else if (score > 40)
            {
                return 4;
            }
            return 1;
        }

        /// <summary>
        /// Xác định xem group pipes có nên dùng animation hay không
        /// </summary>
        public bool ShouldUseAnimatedGroup()
        {
            return rnd.NextDouble() < 0.5; // 50% tĩnh, 50% animated
        }

        public List<double> GenerateGroupHeights(int groupSize, bool isAnimatedGroup, out bool ascending)
        {
            ascending = rnd.NextDouble() < 0.5;
            double stepSize = isAnimatedGroup ? 20 : 50;
            
            int minTopHeight = 100;
            int minBottomHeight = 100;
            int maxTopHeight = CanvasHeight - (int)Gap - minBottomHeight;
            List<double> groupHeights = new List<double>();
            
            double currentTopHeight;
            if (ascending)
            {
                currentTopHeight = minTopHeight + rnd.Next(50, 150);
                groupHeights.Add(currentTopHeight);
                
                for (int g = 1; g < groupSize; g++)
                {
                    double bottomOfPrevious = currentTopHeight + Gap;
                    double nextBottom = bottomOfPrevious + stepSize;
                    double nextTopHeight = nextBottom - Gap;
                    
                    if (nextTopHeight < minTopHeight + 50 || nextBottom > CanvasHeight - minBottomHeight)
                    {
                        break;
                    }
                    
                    nextTopHeight = Math.Clamp(nextTopHeight, minTopHeight + 50, CanvasHeight - Gap - minBottomHeight);
                    groupHeights.Add(nextTopHeight);
                    currentTopHeight = nextTopHeight;
                }
            }
            else
            {
                double maxBottom = CanvasHeight - minBottomHeight;
                double firstBottom = maxBottom - rnd.Next(0, 100);
                currentTopHeight = firstBottom - Gap;
                currentTopHeight = Math.Clamp(currentTopHeight, minTopHeight + 50, CanvasHeight - Gap - minBottomHeight);
                groupHeights.Add(currentTopHeight);
                
                for (int g = 1; g < groupSize; g++)
                {
                    double bottomOfPrevious = currentTopHeight + Gap;
                    double nextBottom = bottomOfPrevious - stepSize;
                    double nextTopHeight = nextBottom - Gap;
                    
                    if (nextTopHeight < minTopHeight + 50 || nextBottom < Gap + minTopHeight + 50)
                    {
                        break;
                    }
                    
                    nextTopHeight = Math.Clamp(nextTopHeight, minTopHeight + 50, CanvasHeight - Gap - minBottomHeight);
                    groupHeights.Add(nextTopHeight);
                    currentTopHeight = nextTopHeight;
                }
            }
            
            return groupHeights;
        }

        public int GetNextGroupId()
        {
            return nextGroupId++;
        }

        public void RemoveOffscreenPipes()
        {
            PipePairs.RemoveAll(p => p.X < -PipeWidth);
        }

        public void RemoveOffscreenNoTouch()
        {
            NoTouchObstacles.RemoveAll(n => n.X < -100);
        }

        public void RemoveOffscreenGates()
        {
            Gates.RemoveAll(g => g.X < -150);
        }

        public double GetFarthestPipeX()
        {
            double farthest = double.MinValue;
            foreach (var pair in PipePairs)
            {
                if (pair.X > farthest)
                    farthest = pair.X;
            }
            return farthest == double.MinValue ? FirstPipeStartLeft : farthest;
        }

        public double GetFarthestNoTouchX()
        {
            double farthest = -1;
            foreach (var noTouch in NoTouchObstacles)
            {
                if (noTouch.X > farthest)
                    farthest = noTouch.X;
            }
            return farthest;
        }

        public void RemoveGroupPipes(int groupId)
        {
            PipePairs.RemoveAll(p => p.GroupId == groupId);
        }

        public double GetPipeSpacing() => PipeSpacing;
        public double GetGroupPipeSpacing() => GroupPipeSpacing;
        public double GetFirstPipeStartLeft() => FirstPipeStartLeft;
        public int GetPipeWidth() => PipeWidth;
        public double GetGap() => Gap;
        public int GetCanvasHeight() => CanvasHeight;
        public double GetCloudSpeed() => CloudSpeed;

        public void CreateClouds(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var cloud = new CloudState
                {
                    X = 200 + i * 250,
                    Y = rnd.Next(20, 150),
                    Width = rnd.Next(110, 180),
                    Height = rnd.Next(50, 90)
                };
                Clouds.Add(cloud);
            }
        }

        public void UpdateCloudPositions()
        {
            foreach (var cloud in Clouds)
            {
                cloud.X -= CloudSpeed;
                if (cloud.X < -150)
                {
                    // Reset cloud về bên phải màn hình khi ra khỏi màn hình
                    cloud.X = 1000 + rnd.Next(0, 200);
                    cloud.Y = rnd.Next(20, 150);
                }
            }
        }
    }
}

