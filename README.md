# Flappy Bird Game

Một game Flappy Bird được phát triển bằng C# WPF với kiến trúc 3 lớp (3-Layer Architecture),

## Tính năng

### Gameplay
- **Điều khiển đơn giản**: Nhấn phím Space hoặc click chuột để điều khiển chim bay
- **Hệ thống điểm số**: Ghi điểm khi vượt qua các ống nước
- **Lưu điểm cao**: Tự động lưu và hiển thị điểm cao nhất
- **Animation mượt mà**: Chim có animation bay và rơi tự nhiên

### Giao diện
- **Chế độ Day/Night**: Tự động chuyển đổi giữa chế độ ngày và đêm
- **Nhiều skin**: Hỗ trợ chọn skin cho chim và môi trường
- **Giao diện đẹp mắt**: Thiết kế UI hiện đại với các nút và hình ảnh tùy chỉnh
- **Màn hình đăng nhập**: Có video background và giao diện đăng nhập

### Cài đặt
- **Điều chỉnh tốc độ**: Tùy chỉnh tốc độ di chuyển của ống nước
- **Điều chỉnh âm lượng**: Tùy chỉnh âm lượng nhạc nền và hiệu ứng âm thanh
- **Lưu cài đặt**: Tự động lưu các cài đặt của người chơi

### Âm thanh
- **Nhạc nền**: Có nhạc nền khi chơi game
- **Hiệu ứng âm thanh**: 
  - Âm thanh khi nhảy (Jump)
  - Âm thanh khi ghi điểm (Point)
  - Âm thanh khi thua (Fail)

## Công nghệ sử dụng

- **.NET 9.0**: Framework chính
- **WPF (Windows Presentation Foundation)**: Xây dựng giao diện người dùng
- **C#**: Ngôn ngữ lập trình
- **3-Layer Architecture**: 
  - **Presentation Layer**: `PRN212.G5.FlappyBird` - Giao diện và UI
  - **Business Layer**: `FlappyBird.Business` - Logic nghiệp vụ
  - **Data Layer**: `FlappyBird.Data` - Xử lý dữ liệu


## Cấu trúc dự án

```
FlappyBird/
├── PRN212.G5.FlappyBird/          # Presentation Layer
│   ├── Assets/                    # Hình ảnh, âm thanh, video
│   ├── Helpers/                   # Các helper classes
│   │   ├── AnimationHelper.cs
│   │   ├── AssetHelper.cs
│   │   ├── BirdFrameManager.cs
│   │   ├── GameUIRenderer.cs
│   │   └── SoundHelper.cs
│   ├── Views/                     # Các cửa sổ
│   │   └── SettingsWindow.xaml
│   ├── LoginWindow.xaml           # Màn hình đăng nhập
│   ├── MainWindow.xaml            # Màn hình game chính
│   └── SelectSkin.xaml            # Màn hình chọn skin
│
├── FlappyBird.Business/           # Business Layer
│   ├── Models/                    # Các model
│   │   ├── BirdState.cs
│   │   ├── CloudState.cs
│   │   ├── GateState.cs
│   │   ├── NoTouchState.cs
│   │   └── PipePairState.cs
│   └── Services/                  # Các service
│       ├── BirdService.cs
│       ├── GameService.cs
│       └── StageService.cs
│
├── FlappyBird.Data/               # Data Layer
│   └── Repositories/
│       └── GameRepo.cs            # Repository xử lý dữ liệu
│
├── FlappyBirdGame.sln            # Solution file
└── README.md                      # File này
```
<img width="986" height="603" alt="image" src="https://github.com/user-attachments/assets/c6b11cd9-66ff-4e92-962a-41b26f9eaff4" />
<img width="736" height="543" alt="image" src="https://github.com/user-attachments/assets/4889bb38-e831-4f78-a0bc-95180b1f7f4e" />
<img width="406" height="593" alt="image" src="https://github.com/user-attachments/assets/c6f2fae5-181c-4be2-bab0-7a5b98a2ddcc" />
<img width="986" height="603" alt="image" src="https://github.com/user-attachments/assets/e6cbedc6-b94f-4673-8c06-299adab48ff6" />
<img width="986" height="603" alt="image" src="https://github.com/user-attachments/assets/457b8d49-0d63-46a6-94e7-07fec5480c40" />








