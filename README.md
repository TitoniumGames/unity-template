# Game Template Unity Package

A comprehensive Unity game development template that provides essential systems for building games quickly and efficiently.

## 🚀 Quick Start

### Install via Git URL

1. Open Unity Package Manager (Window > Package Manager)
2. Click the **+** button
3. Select **Add package from git URL**
4. Enter: `https://github.com/TitoniumGames/unity-template.git?path=/Assets/GameTemplate`
5. Click **Add**

### Install via Package Manager (if published)

1. Open Unity Package Manager
2. Search for "Game Template"
3. Click **Install**

## 📦 Package Contents

### Core Systems

- **GameManager**: Central game state management with Application Settings integration
- **LevelManager**: Level loading and management
- **PlayerData**: Player data persistence with integrated Currency System
- **LevelData**: Level configuration system
- **GameEvents**: Event-driven architecture
- **ApplicationSettings**: Configurable game settings (FPS, currency options)
- **CurrencySystem**: Multi-currency management (Coin, Gem, Diamond)

### Event System

- **EventManager**: Centralized event management
- **IEvent & IEventListener**: Event interface system
- **MonoBehaviourEventListener**: Easy event listening for MonoBehaviours
- **CurrencyEvent**: Currency change events (add, spend, convert)
- **CurrencyEventListener**: Listen to specific currency events

### Editor Tools

- **Delete Example Folder**: Clean up after installation
- **Documentation Access**: Quick access to README files
- **About Dialog**: Package information

## 🛠️ Usage

### Basic Setup

```csharp
// Initialize the game manager (automatically loads ApplicationSettings)
GameManager.Instance.Initialize();

// Subscribe to events
EventManager.Instance.Subscribe<GameStartEvent>(OnGameStart);

// Load a level
LevelManager.Instance.LoadLevel("Level1");
```

### Currency System

```csharp
// Get player currency
var currency = GameManager.Instance.PlayerData.GetCurrency();

// Add currency
GameManager.Instance.PlayerData.AddCoin(100);
GameManager.Instance.PlayerData.AddGem(10);

// Spend currency
bool success = GameManager.Instance.PlayerData.SpendCoin(50);

// Convert currency
bool converted = GameManager.Instance.PlayerData.ConvertCoinToGem(100);

// Check if can afford
bool canAfford = GameManager.Instance.PlayerData.CanAfford(100, 5, 0);
```

### Application Settings

```csharp
// Create ApplicationSettings asset in Unity Editor
// Set target FPS, enable/disable currencies, set conversion ratios
// GameManager will automatically load and apply these settings
```

## 📚 Documentation

- [Core Systems](Assets/GameTemplate/Core/README.md)
- [Event System](Assets/GameTemplate/EventSystem/README.md)
- [Package README](Assets/GameTemplate/README.md)

## 🎯 Features

- ✅ Game state management
- ✅ Level loading and management
- ✅ Player data persistence
- ✅ Event-driven architecture
- ✅ **Application Settings system (FPS, currency configuration)**
- ✅ **Multi-currency system (Coin, Gem, Diamond)**
- ✅ **Currency conversion and management**
- ✅ **Currency events and listeners**
- ✅ Editor tools for package management
- ✅ Comprehensive documentation
- ✅ Sample project included

## 🔧 Requirements

- Unity 2022.3 or higher
- TextMeshPro package

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](Assets/GameTemplate/LICENSE) file for details.

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 Changelog

See [CHANGELOG.md](Assets/GameTemplate/CHANGELOG.md) for version history.

## 🆘 Support

If you encounter any issues or have questions, please:

1. Check the documentation
2. Search existing issues
3. Create a new issue with detailed information

---

**Created by Kane** - A Unity game development template for building better games faster.
