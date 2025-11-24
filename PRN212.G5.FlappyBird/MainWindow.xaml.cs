using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using FlappyBird.Business.Services;
using FlappyBird.Business.Models;
using FlappyBird.Data.Repositories;
using PRN212.G5.FlappyBird.Helpers;

namespace PRN212.G5.FlappyBird.Views
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer gameTimer = new();
        private readonly DispatcherTimer birdAnimTimer = new();
        private readonly DispatcherTimer dayNightTimer = new();
        private readonly string playerName = "Player";


        // Bird frame management đã được chuyển vào BirdFrameManager
        private readonly BirdFrameManager birdFrameManager = new();

        // Services để quản lý business logic
        private readonly StageService stageService = new();
        private readonly BirdService birdService = new();
        private readonly GameService gameService = new();
        private readonly GameRepo gameRepo = new();

        // UI Wrappers để map Business models với UI elements
        private sealed class PipePairUI
        {
            public PipePairUI(Image top, Image bottom, PipePairState state)
            {
                Top = top;
                Bottom = bottom;
                State = state;
            }

            public Image Top { get; }
            public Image Bottom { get; }
            public PipePairState State { get; }
        }

        private sealed class NoTouchUI
        {
            public NoTouchUI(Image image, NoTouchState state)
            {
                Image = image;
                State = state;
            }

            public Image Image { get; }
            public NoTouchState State { get; }
        }

        private sealed class GateUI
        {
            public GateUI(Ellipse gate, GateState state)
            {
                Gate = gate;
                State = state;
            }

            public Ellipse Gate { get; }
            public GateState State { get; }
        }

        private readonly List<PipePairUI> pipePairs = new();
        private readonly List<Image> clouds = new(); // UI elements cho clouds
        private readonly List<NoTouchUI> noTouchObstacles = new();
        private readonly List<GateUI> gates = new();

        private const double DefaultPipeSpeed = 5;
        private const double MinPipeSpeed = 3;
        private const double MaxPipeSpeed = 10;

        private double pipeSpeed = DefaultPipeSpeed;
        private double selectedPipeSpeed = DefaultPipeSpeed;

        private readonly MediaPlayer sfxJump = new();
        private readonly MediaPlayer sfxPoint = new();
        private readonly MediaPlayer sfxFail = new();

        // UI Helpers
        private readonly GameUIRenderer uiRenderer;
        private readonly AnimationHelper animationHelper;

        // Animation constants đã được chuyển vào BirdService

        public MainWindow(double initialPipeSpeed)
        {
            InitializeComponent();

            selectedPipeSpeed = Math.Clamp(initialPipeSpeed, MinPipeSpeed, MaxPipeSpeed);
            pipeSpeed = selectedPipeSpeed;

            // Khởi tạo helpers
            uiRenderer = new GameUIRenderer(stageService, GameCanvas);
            animationHelper = new AnimationHelper(
                stageService, GameCanvas, DayLayer, NightLayer, DayGround, NightGround, FlappyBird);

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;

            birdAnimTimer.Interval = TimeSpan.FromMilliseconds(100);
            birdAnimTimer.Tick += BirdAnimTick;

            // Tắt timer tự động chuyển đổi ngày/đêm, chỉ chuyển khi vào cổng
            // dayNightTimer.Interval = TimeSpan.FromMinutes(0.50);
            // dayNightTimer.Tick += (_, __) => SmoothToggleDayNight();

            int initialHighScore = gameRepo.LoadHighScore();
            gameService.UpdateHighScore(initialHighScore);
            Title = $"Flappy Bird - {playerName}";

            birdFrameManager.LoadAllBirdFrames();
            birdFrameManager.UseBirdFramesForTheme(false, birdService, FlappyBird);

            Loaded += MainWindow_OnLoaded;
        }
        
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        // AssetHelper, SoundHelper, BirdFrameManager đã được tách ra thành các helper classes

        private void StartGame()
        {
            ResetStageToDay();

            // Reset services
            gameService.StartGame();
            birdService.Reset();
            stageService.Reset();

            GameOverPanel.Visibility = Visibility.Collapsed;

            ScoreText.Visibility = Visibility.Visible;
            HighScoreText.Visibility = Visibility.Visible;

            Canvas.SetLeft(FlappyBird, birdService.BirdState.X);
            Canvas.SetTop(FlappyBird, birdService.BirdState.Y);
            Panel.SetZIndex(FlappyBird, 5);
            
            pipeSpeed = selectedPipeSpeed;
            ScoreText.Text = "Score: 0";
            HighScoreText.Text = $"High Score: {gameService.HighScore}";

            ClearDynamicObjects();
            
            // Create clouds using StageService
            stageService.CreateClouds(4);
            CreateCloudsUI();
            CreateInitialPipes(4);
            
            birdFrameManager.UseBirdFramesForTheme(false, birdService, FlappyBird);

            gameTimer.Start();
            birdAnimTimer.Start();
            dayNightTimer.Start();
        }

        private void ResetStageToDay(bool animate = false)
        {
            animationHelper.ResetStageToDay(animate,
                () => UpdateDynamicAssets(false),
                () => birdFrameManager.UseBirdFramesForTheme(false, birdService, FlappyBird));
        }

        // LoadHighScore và SaveHighScore đã được chuyển vào GameRepo (Data layer)

        private void EndGame()
        {
            if (gameService.IsGameOver) return;
            
            gameService.EndGame();
            birdService.SetDead();

            gameTimer.Stop();
            dayNightTimer.Stop();

            if (gameService.Score > gameService.HighScore)
            {
                gameService.UpdateHighScore(gameService.Score);
                gameRepo.SaveHighScore(gameService.Score);
                if (HighScoreText != null) HighScoreText.Text = $"High Score: {gameService.HighScore}";
            }

            GoScoreValue.Text = gameService.Score.ToString();
            GoBestScoreValue.Text = gameService.HighScore.ToString();
            GameOverPanel.Visibility = Visibility.Visible;

            // Switch to death animation
            SoundHelper.PlaySfx(sfxFail, "Fail.mp3", 0.7);
            
            // Animate bird falling down with rotation
            animationHelper.AnimateBirdDeath(birdService, stageService.GetCanvasHeight());
        }

        private void SmoothToggleDayNight()
        {
            animationHelper.SmoothToggleDayNight(
                (night) => UpdateDynamicAssets(night),
                (night) => birdFrameManager.UseBirdFramesForTheme(night, birdService, FlappyBird));
        }

        private void UpdateDynamicAssets(bool night)
        {
            var pipePairsForUpdate = pipePairs.Select(p => (p.Top, p.Bottom)).ToList();
            uiRenderer.UpdateDynamicAssets(night, pipePairsForUpdate, clouds);
        }

        private void ClearDynamicObjects()
        {
            foreach (var pair in pipePairs)
            {
                GameCanvas.Children.Remove(pair.Top);
                GameCanvas.Children.Remove(pair.Bottom);
            }
            foreach (var cloud in clouds) GameCanvas.Children.Remove(cloud);
            foreach (var state in noTouchObstacles) 
            {
                if (state.Image != null && GameCanvas.Children.Contains(state.Image))
                    GameCanvas.Children.Remove(state.Image);
            }
            foreach (var gate in gates)
            {
                if (gate.Gate != null && GameCanvas.Children.Contains(gate.Gate))
                    GameCanvas.Children.Remove(gate.Gate);
            }
            pipePairs.Clear();
            clouds.Clear();
            noTouchObstacles.Clear();
            gates.Clear();
        }

        private void CreateCloudsUI()
        {
            clouds.Clear();
            clouds.AddRange(uiRenderer.CreateCloudsUI());
        }

        private void SpawnNoTouchGroup(int count, double startX)
        {
            // Sử dụng StageService để spawn NoTouch
            int beforeCount = stageService.NoTouchObstacles.Count;
            stageService.SpawnNoTouchGroup(count, startX);
            
            // Tạo UI elements cho các NoTouch mới
            for (int i = beforeCount; i < stageService.NoTouchObstacles.Count; i++)
            {
                var state = stageService.NoTouchObstacles[i];
                try
                {
                    var obstacle = uiRenderer.CreateNoTouchUI(state);
                    if (obstacle != null)
                    {
                        noTouchObstacles.Add(new NoTouchUI(obstacle, state));
                    }
                }
                catch
                {
                    // Error creating NoTouch
                }
            }
            
            // Kiểm tra xem có gate mới được tạo không
            if (stageService.Gates.Count > gates.Count)
            {
                var newGateState = stageService.Gates[stageService.Gates.Count - 1];
                CreateGateUI(newGateState);
            }
        }
        
        private void CreateGateUI(GateState gateState)
        {
            var gate = uiRenderer.CreateGateUI(gateState);
            gates.Add(new GateUI(gate, gateState));
        }

        private void CreateInitialPipes(int count)
        {
            // Tạo pipes trực tiếp như code cũ, nhưng sử dụng StageService để quản lý state
            for (int i = 0; i < count; i++)
            {
                double leftPos = stageService.GetFirstPipeStartLeft() + (i * stageService.GetPipeSpacing());
                CreatePipePair(leftPos);
            }
        }

        private void CreatePipePair(double leftPos)
        {
            var pairState = stageService.CreatePipePair(leftPos, gameService.Score);
            CreatePipeUI(pairState);
        }

        private void CreatePipeUI(PipePairState pairState)
        {
            var (top, bottom) = uiRenderer.CreatePipeUI(pairState);
            pipePairs.Add(new PipePairUI(top, bottom, pairState));
        }

        // Methods RandomizePipe và RandomizePipeAnimationOnly đã được chuyển vào StageService

        private void ApplyPipeGeometry(PipePairState pairState, Image top, Image bottom)
        {
            uiRenderer.ApplyPipeGeometry(pairState, top, bottom);
        }

        private void ApplyPipeAnimation(PipePairUI pairUI)
        {
            var pair = pairUI.State;
            
            // Use StageService to apply animation logic (pair.X đã được update bởi UpdatePipePositions)
            stageService.ApplyPipeAnimation(pair);
            
            // Update UI based on state
            ApplyPipeGeometry(pair, pairUI.Top, pairUI.Bottom);
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            gameService.UpdateFrame();
            if (!gameService.IsPlaying || gameService.IsGameOver) return;

            // Cập nhật chim sử dụng BirdService
            birdService.Update();
            var birdState = birdService.BirdState;
            Canvas.SetTop(FlappyBird, Canvas.GetTop(FlappyBird) + birdState.Speed);

            // Cập nhật góc xoay của chim
            var rt = FlappyBird.RenderTransform as RotateTransform;
            if (rt != null)
            {
                rt.Angle = birdState.Rotation;
            }

            double speed = pipeSpeed + gameService.Score * 0.1;

            // Cập nhật vị trí mây sử dụng StageService
            stageService.UpdateCloudPositions();
            for (int i = 0; i < clouds.Count && i < stageService.Clouds.Count; i++)
            {
                var cloudState = stageService.Clouds[i];
                Canvas.SetLeft(clouds[i], cloudState.X);
                Canvas.SetTop(clouds[i], cloudState.Y);
            }

            // Cập nhật vị trí pipes sử dụng StageService
            stageService.UpdatePipePositions(speed);

            // Xử lý pipes (luôn hiển thị, không ẩn)
            for (int i = 0; i < pipePairs.Count; i++)
            {
                var pairUI = pipePairs[i];
                var pair = pairUI.State;
                var top = pairUI.Top;
                var bottom = pairUI.Bottom;

                // Update X position từ StageService (không lấy từ UI)
                // pair.X đã được update bởi stageService.UpdatePipePositions(speed)

                // Xử lý pipes trong group (non-leader sẽ follow leader)
                if (pair.GroupId != -1 && !pair.IsGroupLeader)
                {
                    // Pipes trong group di chuyển cùng leader, giữ khoảng cách GroupPipeSpacing
                    var leader = pipePairs.FirstOrDefault(p => p.State.GroupId == pair.GroupId && p.State.IsGroupLeader);
                    if (leader != null)
                    {
                        double leaderX = leader.State.X;
                        double offset = pair.GroupIndex * stageService.GetGroupPipeSpacing();
                        Canvas.SetLeft(top, leaderX + offset);
                        Canvas.SetLeft(bottom, leaderX + offset);
                        pair.X = leaderX + offset;
                    }
                    else
                    {
                        // Leader không tồn tại (đã bị xóa), xóa pipe này luôn
                        if (GameCanvas.Children.Contains(top))
                            GameCanvas.Children.Remove(top);
                        if (GameCanvas.Children.Contains(bottom))
                            GameCanvas.Children.Remove(bottom);
                        stageService.PipePairs.Remove(pair);
                        pipePairs.RemoveAt(i);
                        i--; // Giảm index vì đã xóa phần tử
                        continue;
                    }
                    
                    // Kiểm tra nếu non-leader pipe ra khỏi màn hình (theo leader)
                    // KHÔNG xóa ngay, để leader xử lý recycle cho cả group
                    // Non-leader pipes sẽ được recycle cùng với leader khi leader ra khỏi màn hình
                    
                    // Kiểm tra collision với group pipes (non-leader)
                    if (gameService.GraceTicksRemaining <= 0 &&
                        (FlappyBird.CollidesWith(top) || FlappyBird.CollidesWith(bottom)))
                    {
                        EndGame();
                        return;
                    }
                    
                    continue; // Bỏ qua phần xử lý khác, nhưng đã check collision rồi
                }

                ApplyPipeAnimation(pairUI);

                // Update UI position from StageService
                // pair.X đã được update bởi stageService.UpdatePipePositions(speed)
                Canvas.SetLeft(top, pair.X);
                Canvas.SetLeft(bottom, pair.X);

                if (pair.X < -stageService.GetPipeWidth())
                {
                    // Nếu là leader của group, recycle tất cả pipes trong group cùng lúc
                    if (pair.GroupId != -1 && pair.IsGroupLeader)
                    {
                        // Lấy tất cả pipes trong group (bao gồm cả leader)
                        var groupPipes = pipePairs.Where(p => p.State.GroupId == pair.GroupId).OrderBy(p => p.State.GroupIndex).ToList();
                        
                        // Xóa tất cả pipes trong group khỏi StageService trước khi tính farthestRight
                        stageService.RemoveGroupPipes(pair.GroupId);
                        
                        // Tính farthestRight từ pipes còn lại (không tính pipes trong group này)
                        double groupFarthestRight = 0;
                        var groupRemainingPipes = pipePairs.Where(p => p.State.GroupId != pair.GroupId && p.State.X >= -stageService.GetPipeWidth()).ToList();
                        if (groupRemainingPipes.Count > 0)
                        {
                            groupFarthestRight = groupRemainingPipes.Max(p => p.State.X);
                        }
                        if (groupFarthestRight <= 0)
                        {
                            groupFarthestRight = stageService.GetFirstPipeStartLeft();
                        }
                        
                        double groupFarthestNoTouchX = stageService.GetFarthestNoTouchX();
                        
                        double newLeaderX;
                        if (groupFarthestNoTouchX > 0)
                        {
                            newLeaderX = Math.Max(groupFarthestNoTouchX + 500, groupFarthestRight + stageService.GetPipeSpacing());
                    }
                    else
                    {
                            newLeaderX = groupFarthestRight + stageService.GetPipeSpacing();
                            if (newLeaderX < 1000)
                            {
                                newLeaderX = 1000;
                            }
                        }
                        
                        // Recycle tất cả pipes trong group với vị trí mới
                        for (int g = 0; g < groupPipes.Count; g++)
                        {
                            var groupPipe = groupPipes[g];
                            double newGroupPipeX = newLeaderX + (g * stageService.GetGroupPipeSpacing());
                            
                            // Cập nhật vị trí cho tất cả pipes trong group
                            groupPipe.State.X = newGroupPipeX;
                            Canvas.SetLeft(groupPipe.Top, newGroupPipeX);
                            Canvas.SetLeft(groupPipe.Bottom, newGroupPipeX);
                            
                            // Thêm lại vào StageService
                            stageService.PipePairs.Add(groupPipe.State);
                        }
                        
                        // Set lại pair.X cho leader (để code tiếp theo không xử lý lại)
                        pair.X = newLeaderX;
                        
                        // Bỏ qua phần recycle phía dưới vì đã xử lý cả group rồi
                        gameService.IncrementScore();
                        stageService.OnPipePassed();
                        ScoreText.Text = $"Score: {gameService.Score}";
                        SoundHelper.PlaySfx(sfxPoint, "Point.mp3", 0.6);
                        
                        // Kiểm tra xem có cần spawn NoTouch không
                        if (stageService.ShouldSpawnNoTouch(stageService.GetTotalPipesPassed(), out int groupNoTouchCount, out int groupSpawnAt))
                        {
                            if (groupSpawnAt > 0)
                            {
                                SpawnNoTouchGroup(groupNoTouchCount, newLeaderX + stageService.GetPipeSpacing());
                            }
                        }
                        
                        continue; // Bỏ qua phần recycle phía dưới
                    }
                    
                    // Xóa pipe này khỏi StageService trước khi tính farthestRight (cho pipe đơn)
                    stageService.PipePairs.Remove(pair);
                    
                    // Tính farthestRight từ UI pipes còn lại (loại trừ pipe hiện tại)
                    double farthestRight = 0;
                    var remainingPipes = pipePairs.Where(p => p != pairUI && p.State.X >= -stageService.GetPipeWidth()).ToList();
                    if (remainingPipes.Count > 0)
                    {
                        farthestRight = remainingPipes.Max(p => p.State.X);
                    }
                    if (farthestRight <= 0)
                    {
                        farthestRight = stageService.GetFirstPipeStartLeft();
                    }
                    
                    double farthestNoTouchX = stageService.GetFarthestNoTouchX();

                    double newX;
                    if (farthestNoTouchX > 0)
                    {
                        // Có NoTouch, đặt pipe cách NoTouch nửa màn hình (500px)
                        newX = Math.Max(farthestNoTouchX + 500, farthestRight + stageService.GetPipeSpacing());
                    }
                    else
                    {
                        // Không có NoTouch, đặt bình thường - đảm bảo pipe xuất hiện từ bên phải màn hình
                        newX = farthestRight + stageService.GetPipeSpacing();
                        // Đảm bảo newX không nhỏ hơn 1000 (bên phải màn hình)
                        if (newX < 1000)
                        {
                            newX = 1000;
                        }
                    }

                    // Tạo lại pipe với vị trí mới
                    pair.X = newX;
                    Canvas.SetLeft(top, newX);
                    Canvas.SetLeft(bottom, newX);
                    
                    // Thêm lại vào StageService
                    stageService.PipePairs.Add(pair);

                    // Kiểm tra xem có tạo group pipes không
                    bool shouldCreateGroup = stageService.ShouldCreateGroup(gameService.Score) && pair.GroupId == -1;
                    
                    if (shouldCreateGroup)
                    {
                        int groupSize = stageService.GetGroupSize(gameService.Score);
                        int currentGroupId = stageService.GetNextGroupId();
                        
                        // TẤT CẢ group pipes đều dùng pattern cầu thang (staircase)
                        // Chọn loại: tĩnh hoặc có animation (sử dụng StageService)
                        bool isAnimatedGroup = stageService.ShouldUseAnimatedGroup();
                        
                        // Sử dụng StageService để generate group heights
                        List<double> groupHeights = stageService.GenerateGroupHeights(groupSize, isAnimatedGroup, out bool ascending);
                        
                        // Nếu không tạo được ít nhất 2 pipes, không tạo group (quay lại pipe bình thường)
                        if (groupHeights.Count < 2)
                        {
                            pair.GroupId = -1;
                            pair.IsGroupLeader = false;
                            stageService.RandomizePipe(pair, gameService.Score);
                            uiRenderer.ApplyPipeGeometry(pair, top, bottom);
                            continue; // Bỏ qua phần tạo group, tiếp tục với pipe bình thường
                        }
                        
                        // Cập nhật groupSize thực tế
                        groupSize = groupHeights.Count;
                        
                        // Pipe hiện tại là leader của group
                        pair.GroupId = currentGroupId;
                        pair.IsGroupLeader = true;
                        pair.GroupIndex = 0;
                        pair.BaseTopHeight = groupHeights[0];
                        pair.CurrentTopHeight = groupHeights[0];
                        
                        // Set height trước (không thay đổi)
                        int minTopHeight = 100;
                        int minBottomHeight = 100;
                        pair.MinTopHeight = minTopHeight;
                        pair.MinBottomHeight = minBottomHeight;
                        uiRenderer.ApplyPipeGeometry(pair, top, bottom);
                        
                        if (isAnimatedGroup)
                        {
                            // Animated group: Chỉ random animation properties, KHÔNG thay đổi height
                            stageService.RandomizePipeAnimationOnly(pair, gameService.Score);
                        }
                        else
                        {
                            // Static group: Không có animation
                            pair.EnableVerticalAnimation = false;
                            pair.IsMoving = false;
                            pair.IsOscillating = false;
                            pair.HasTargetMovement = false;
                            pair.IsJumpPattern = false;
                        }
                        
                        // Tạo các pipes còn lại trong group (chỉ tạo đúng số pipes đã tính được)
                        for (int g = 1; g < groupHeights.Count; g++)
                        {
                            double groupPipeX = newX + (g * stageService.GetGroupPipeSpacing());
                            
                            // Tạo PipePairState trong StageService
                            var groupPairState = new PipePairState
                            {
                                X = groupPipeX,
                                GroupId = currentGroupId,
                                IsGroupLeader = false,
                                GroupIndex = g,
                                BaseTopHeight = groupHeights[g],
                                CurrentTopHeight = groupHeights[g],
                                MinTopHeight = minTopHeight,
                                MinBottomHeight = minBottomHeight
                            };
                            
                            stageService.PipePairs.Add(groupPairState);
                            
                            // Tạo UI sử dụng GameUIRenderer
                            var (groupTop, groupBottom) = uiRenderer.CreatePipeUI(groupPairState);
                            // Cập nhật lại X position vì CreatePipeUI đã set X từ pairState.X
                            Canvas.SetLeft(groupTop, groupPipeX);
                            Canvas.SetLeft(groupBottom, groupPipeX);
                            groupPairState.X = groupPipeX;
                            
                            if (isAnimatedGroup)
                            {
                                // Cầu thang animated: Chỉ random animation properties, KHÔNG thay đổi height
                                stageService.RandomizePipeAnimationOnly(groupPairState, gameService.Score);
                            }
                            else
                            {
                                // Cầu thang tĩnh: Không có animation
                                groupPairState.EnableVerticalAnimation = false;
                                groupPairState.IsMoving = false;
                                groupPairState.IsOscillating = false;
                                groupPairState.HasTargetMovement = false;
                                groupPairState.IsJumpPattern = false;
                            }
                            
                            pipePairs.Add(new PipePairUI(groupTop, groupBottom, groupPairState));
                        }
                    }
                    else
                    {
                        // Pipe bình thường
                        pair.GroupId = -1;
                        pair.IsGroupLeader = false;
                        stageService.RandomizePipe(pair, gameService.Score);
                        uiRenderer.ApplyPipeGeometry(pair, top, bottom);
                    }

                    gameService.IncrementScore();
                    stageService.OnPipePassed();
                    ScoreText.Text = $"Score: {gameService.Score}";
                    SoundHelper.PlaySfx(sfxPoint, "Point.mp3", 0.6);
                    
                    // Kiểm tra xem có cần spawn NoTouch không (sử dụng StageService)
                    if (stageService.ShouldSpawnNoTouch(stageService.GetTotalPipesPassed(), out int noTouchCount, out int spawnAt))
                    {
                        if (spawnAt > 0)
                        {
                            // Spawn NoTouch ngay sau pipe vừa recycle
                            SpawnNoTouchGroup(noTouchCount, newX + stageService.GetPipeSpacing());
                        }
                    }
                }

                // Kiểm tra collision với pipes
                if (gameService.GraceTicksRemaining <= 0 &&
                    (FlappyBird.CollidesWith(top) || FlappyBird.CollidesWith(bottom)))
                {
                    EndGame();
                    return;
                }
            }

            // Cập nhật vị trí NoTouch sử dụng StageService
            stageService.UpdateNoTouchPositions(speed);
            
            // Đồng bộ UI với StageService states
            for (int i = noTouchObstacles.Count - 1; i >= 0; i--)
            {
                if (i < 0 || i >= noTouchObstacles.Count) break;
                var noTouchUI = noTouchObstacles[i];
                var state = noTouchUI.State;
                
                if (noTouchUI.Image == null || !GameCanvas.Children.Contains(noTouchUI.Image))
                {
                        noTouchObstacles.RemoveAt(i);
                    continue;
                }

                if (double.IsNaN(state.X) || state.X < -100)
                {
                    if (GameCanvas.Children.Contains(noTouchUI.Image))
                        GameCanvas.Children.Remove(noTouchUI.Image);
                    noTouchObstacles.RemoveAt(i);
                    continue;
                }

                // Update UI position từ state
                Canvas.SetLeft(noTouchUI.Image, state.X);
                Canvas.SetTop(noTouchUI.Image, state.CurrentY);

                // Kiểm tra collision với NoTouch
                if (gameService.GraceTicksRemaining <= 0 && FlappyBird.CollidesWith(noTouchUI.Image))
                {
                    EndGame();
                    return;
                }
            }
            
            // Xóa NoTouch ra khỏi màn hình từ StageService
            stageService.RemoveOffscreenNoTouch();
            
            // Cập nhật vị trí Gate sử dụng StageService
            stageService.UpdateGatePositions(speed);
            
            // Đồng bộ UI với StageService states
            for (int i = gates.Count - 1; i >= 0; i--)
            {
                if (i < 0 || i >= gates.Count) break;
                var gateUI = gates[i];
                var state = gateUI.State;
                
                if (gateUI.Gate == null || !GameCanvas.Children.Contains(gateUI.Gate))
                {
                        gates.RemoveAt(i);
                    continue;
                }

                if (double.IsNaN(state.X) || state.X < -150)
                {
                    if (GameCanvas.Children.Contains(gateUI.Gate))
                        GameCanvas.Children.Remove(gateUI.Gate);
                    gates.RemoveAt(i);
                    continue;
                }

                // Update UI position từ state
                Canvas.SetLeft(gateUI.Gate, state.X);
                Canvas.SetTop(gateUI.Gate, state.Y);

                // Kiểm tra collision với chim
                if (!state.IsActivated && gameService.GraceTicksRemaining <= 0)
                {
                    double birdX = Canvas.GetLeft(FlappyBird);
                    double distance = Math.Abs(state.X - birdX);
                    
                    if (distance < 150 && (gameService.FrameCount % 5 == 0))
                    {
                        if (FlappyBird.CollidesWith(gateUI.Gate))
                        {
                            state.IsActivated = true;
                            SmoothToggleDayNight();
                            gateUI.Gate.Opacity = 1.0;
                        }
                    }
                }
            }
            
            // Xóa Gates ra khỏi màn hình từ StageService
            stageService.RemoveOffscreenGates();

            double currentBirdTop = Canvas.GetTop(FlappyBird);

            if (currentBirdTop < 0)
            {
                Canvas.SetTop(FlappyBird, 0);
                birdService.BirdState.Speed = 0;
            }

            // Kiểm tra collision với ground
            double groundLevel = stageService.GetCanvasHeight() - FlappyBird.Height;
            if (currentBirdTop > groundLevel)
            {
                Canvas.SetTop(FlappyBird, groundLevel);
                birdService.BirdState.Speed = 0;
                EndGame();
            }
        }

        // UpdateBirdAnimationState và UpdateBirdRotation đã được chuyển vào BirdService

        private void BirdAnimTick(object? sender, EventArgs e)
        {
            var birdState = birdService.BirdState;
            BitmapImage[] frames;

            // Select frames based on animation state
            switch (birdState.AnimationState)
            {
                case BirdAnimationState.Flying:
                    frames = birdFrameManager.CurrentFlyFrames;
                    break;
                case BirdAnimationState.Falling:
                    frames = birdFrameManager.CurrentFallFrames;
                    break;
                case BirdAnimationState.Dead:
                    frames = birdFrameManager.CurrentDeathFrames;
                    break;
                default:
                    frames = birdFrameManager.CurrentFlyFrames;
                    break;
            }

            if (frames == null || frames.Length == 0) return;

            // Advance to next frame
            birdState.FrameIndex = (birdState.FrameIndex + 1) % frames.Length;

            // Update bird sprite
            if (frames[birdState.FrameIndex] != null)
                FlappyBird.Source = frames[birdState.FrameIndex];
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameService.IsPlaying || gameService.IsGameOver) return;

            if (e.Key == Key.Space)
            {
                birdService.Jump();
                SoundHelper.PlaySfx(sfxJump, "Jump.mp3", 0.5);
            }
            else if (e.Key == Key.N)
            {
                SmoothToggleDayNight();
            }
        }

        private void BtnReplay_Click(object sender, RoutedEventArgs e)
        {
            // Stop all timers and animations
            StopGameLoops();
            
            // Clear all bird animations
            FlappyBird.BeginAnimation(Canvas.TopProperty, null);
            var rt = FlappyBird.RenderTransform as RotateTransform;
            if (rt != null)
            {
                rt.BeginAnimation(RotateTransform.AngleProperty, null);
            }
            
            // Hide game over panel
            GameOverPanel.Visibility = Visibility.Collapsed;

            // Reset to day theme without animation for immediate response
            ResetStageToDay(false);
            
            // Start new game
            StartGame();
        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            StopGameLoops();

            var loginWindow = new LoginWindow(selectedPipeSpeed);

            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();

            Close();
        }

        private void StopGameLoops()
        {
            gameTimer.Stop();
            birdAnimTimer.Stop();
            dayNightTimer.Stop();
        }
    }

    // BirdAnimationState đã được chuyển vào FlappyBird.Business.Models.BirdState
    
    public static class CollisionHelper
    {
        public static bool CollidesWith(this FrameworkElement a, FrameworkElement b)
        {
            if (a == null || b == null) return false;

            double aLeft = Canvas.GetLeft(a);
            double aTop = Canvas.GetTop(a);
            double bLeft = Canvas.GetLeft(b);
            double bTop = Canvas.GetTop(b);

            double aWidth = double.IsNaN(a.Width) ? a.ActualWidth : a.Width;
            double aHeight = double.IsNaN(a.Height) ? a.ActualHeight : a.Height;
            double bWidth = double.IsNaN(b.Width) ? b.ActualWidth : b.Width;
            double bHeight = double.IsNaN(b.Height) ? b.ActualHeight : b.Height;

            Rect rectA = new(aLeft, aTop, Math.Max(1, aWidth), Math.Max(1, aHeight));
            Rect rectB = new(bLeft, bTop, Math.Max(1, bWidth), Math.Max(1, bHeight));

            return rectA.IntersectsWith(rectB);
        }
    }
}