<img width="984" height="601" alt="Screenshot 2025-11-24 192818" src="https://github.com/user-attachments/assets/888a2e28-2549-4f60-bf59-301bbaddcb5b" /><img width="978" height="598" alt="Screenshot 2025-11-24 193620" src="https://github.com/user-attachments/assets/07fa87aa-74f3-47ae-9832-4e995ebc2fe9" /><img width="982" height="598" alt="Screenshot 2025-11-24 192738" src="https://github.com/user-attachments/assets/f25e4cf7-1ac9-4626-b299-452113bee036" /># Flappy Bird Game

A feature-rich Flappy Bird clone built with **WPF (.NET)** using a clean **3-layer architecture**. Developed as a group project for the PRN212 course.

## Features

- **Classic Flappy Bird gameplay** -- tap Space to flap, avoid pipes and obstacles
- **Dynamic difficulty scaling** -- speed increases, animated pipes appear, and new obstacle types spawn as your score climbs
- **Day/Night cycle** -- smooth theme transitions triggered by in-game gates or the N key
- **3 bird skins** -- Yellow (Classic), Green, and Purple, each with unique fly/fall sprites for both themes
- **Animated pipes** -- oscillating, target-movement, and jump-pattern animations that intensify with score
- **Group pipes (Staircase)** -- clusters of 2-4 pipes arranged in ascending/descending staircase formations
- **NoTouch obstacles** -- bouncing hazards that spawn in increasing numbers after 10 pipes
- **Gates** -- collectible portals that toggle the day/night theme
- **Persistent high score** -- saved locally to file
- **Background music and sound effects** -- BGM with looping, jump/point/fail SFX
- **Video background** -- animated login screen with looping MP4
- **Configurable pipe speed** -- adjustable from 3 to 10 via Settings

## Architecture

The project follows a **3-layer architecture** with clear separation of concerns:

```
FlappyBirdGame.sln
|
+-- PRN212.G5.FlappyBird/        # Presentation Layer (WPF)
|   +-- LoginWindow.xaml/.cs      # Login screen + BGM + video background
|   +-- MainWindow.xaml/.cs       # Core game window & game loop
|   +-- SelectSkin.xaml/.cs       # Bird skin selection UI
|   +-- Views/
|   |   +-- SettingsWindow.xaml/.cs  # Speed & volume settings
|   +-- Helpers/
|   |   +-- AnimationHelper.cs    # Day/night transitions, death animation
|   |   +-- AssetHelper.cs        # Asset path & bitmap loading utilities
|   |   +-- BirdFrameManager.cs   # Bird sprite frame management per skin/theme
|   |   +-- GameUIRenderer.cs     # Creates & updates pipe/cloud/obstacle UI elements
|   |   +-- SoundHelper.cs        # Sound effect playback
|   +-- Assets/                   # Images, audio, video resources
|
+-- FlappyBird.Business/          # Business Logic Layer
|   +-- Models/
|   |   +-- BirdState.cs          # Bird position, speed, rotation, animation state
|   |   +-- CloudState.cs         # Cloud position and size
|   |   +-- GateState.cs          # Gate position and activation status
|   |   +-- NoTouchState.cs       # NoTouch obstacle position and oscillation
|   |   +-- PipePairState.cs      # Pipe pair geometry, animation, and group info
|   +-- Services/
|       +-- BirdService.cs        # Bird physics (gravity, jump, rotation)
|       +-- GameService.cs        # Score, high score, game state, grace period
|       +-- StageService.cs       # Pipe generation, difficulty scaling, obstacles
|
+-- FlappyBird.Data/              # Data Access Layer
    +-- Repositories/
        +-- GameRepo.cs           # High score file persistence
```

## How It Works

### Game Loop (20ms tick)

1. **Bird physics** -- gravity pulls the bird down each frame; Space key applies upward force
2. **Pipe movement** -- all pipes scroll left at `baseSpeed + score * 0.1`
3. **Pipe recycling** -- when a pipe exits the left edge, it is repositioned to the right with new random properties
4. **Collision detection** -- AABB (Axis-Aligned Bounding Box) checks against pipes, obstacles, ground, and ceiling
5. **Score** -- incremented each time a pipe is recycled past the bird

### Difficulty Progression

| Score | Pipe Animation | Oscillation | Jump Pattern | Group Pipes | NoTouch |
|-------|---------------|-------------|-------------|-------------|---------|
| 0-9 | None | - | - | - | - |
| 10-14 | 50% chance | 50% | - | - | Phase spawning |
| 15-19 | 50% chance | 50% | - | 45% chance | Phase spawning |
| 20-29 | 65% chance | 65% | 30% | 45% chance | Phase spawning |
| 30-39 | 80% chance | 80% | 30% | 45% chance | Phase spawning |
| 40+ | 80% chance | 80% | 30% | 65% chance | Phase spawning |

### Key Constants

| Parameter | Value |
|-----------|-------|
| Canvas Height | 500px |
| Pipe Width | 80px |
| Gap between pipes | 180px |
| Pipe Spacing | 260px |
| Group Pipe Spacing | 90px |
| Gravity | 1.0 per frame |
| Jump Strength | -10.0 |
| Grace Period | 60 frames |

## Getting Started

### Prerequisites

- .NET 6.0+ SDK (https://dotnet.microsoft.com/download)
- Windows OS (WPF requirement)

### Run the Game

```bash
# Clone the repository
git clone <repo-url>
cd FlappyBird

# Build and run
dotnet run --project PRN212.G5.FlappyBird
```

## Controls

| Key | Action |
|-----|--------|
| Space | Flap / Jump |
| N | Toggle Day/Night |
| Replay Button | Restart after Game Over |
| Back Button | Return to Login screen |

## Assets

| File | Purpose |
|------|---------|
| birdfly-1/2/3.png | Bird fly sprites (3 skins) |
| birdfall-1/2/3.png | Bird fall sprites (3 skins) |
| Pipe-day/night.png | Pipe sprites for each theme |
| DayStage/NightStage.jpg | Background images |
| DayGround/NightGround.jpg | Ground images |
| Cloud-Day/Night.png | Cloud sprites |
| NoTouch.png | NoTouch obstacle sprite |
| BGM.mp3 | Background music |
| Jump/Point/Fail.mp3 | Sound effects |
| LoginWindowBG.mp4 | Login screen video background |

<img width="982" height="598" alt="Screenshot 2025-11-24 192738" src="https://github.com/user-attachments/assets/599a4355-f6ab-4e2e-b1c2-5ff79ed0d86d" />
<img width="736" height="543" alt="Screenshot 2025-11-24 201006" src="https://github.com/user-attachments/assets/53c52317-17d1-4aba-b3d4-4a8d0c04409d" />
<img width="406" height="593" alt="Screenshot 2025-11-24 201020" src="https://github.com/user-attachments/assets/77b0c3b7-b815-448d-91ff-bcc63b4d342f" />
<img width="986" height="603" alt="Screenshot 2025-11-24 201042" src="https://github.com/user-attachments/assets/5dbd1512-a97d-4de3-b2f5-ff3eaa6b0ff9" />
<img width="986" height="603" alt="Screenshot 2025-11-24 201050" src="https://github.com/user-attachments/assets/e50bf061-6c43-4670-bd16-feb88b9f16d2" />


