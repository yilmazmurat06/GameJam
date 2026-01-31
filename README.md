# 🎭 Mask Dungeon

> **A 2D Top-Down Psychological Thriller Dungeon Game**  
> *Built for a 36-hour Game Jam*

---

## 🎮 About

A psychological thriller where you wake up in your bedroom only to discover your wife wearing a terrifying mask. Follow her into the dungeon below, where masks hold the power to transform your abilities and emotions. Battle through enemies and uncover the truth.

## 🕹️ Controls

| Key | Action |
|-----|--------|
| **WASD** / **Arrow Keys** | 8-directional movement |
| **Space** / **Left Click** | Attack |
| **E** | Interact with objects/NPCs |

## 🎭 Mask System

Collect and equip different masks to gain unique abilities:

| Mask | Effect |
|------|--------|
| **None** | Normal movement |
| **Rage** | Slower but heavier (knockback resistance) |
| **Sadness** | Time moves slower |
| **Joy** | Faster and lighter |

## ⚔️ Combat System

### Weapons
- **Melee Weapons** - Sword, axe attacks with hitbox detection
- **Ranged Weapons** - Projectile-based with optional ammo

### Enemies
- **FSM-based AI** - Idle → Chase → Attack → Death
- **Detection System** - Enemies chase when player enters range
- **Health & Damage** - Full combat with knockback and invincibility frames

## 🏗️ Architecture

Built with a clean, modular FSM (Finite State Machine) architecture:

```
Assets/Scripts/
├── Core/
│   ├── GameState.cs          # Game state enum
│   ├── GameManager.cs        # Singleton state manager
│   └── GameEvents.cs         # Static event hub
├── Player/
│   ├── PlayerController.cs   # FSM controller + mask/combat
│   ├── PlayerInputHandler.cs # 8-dir input + attack
│   └── States/
│       ├── IPlayerState.cs
│       ├── PlayerIdleState.cs
│       ├── PlayerMoveState.cs
│       ├── PlayerAttackState.cs
│       └── PlayerFrozenState.cs
├── Combat/
│   ├── IDamageable.cs        # Damage interface
│   ├── Health.cs             # Health component
│   └── DamageInfo.cs         # Damage data struct
├── Weapons/
│   ├── WeaponBase.cs         # Abstract weapon
│   ├── MeleeWeapon.cs        # Melee attacks
│   ├── RangedWeapon.cs       # Ranged attacks
│   └── Projectile.cs         # Projectile behavior
├── Enemies/
│   ├── EnemyBase.cs          # Base AI controller
│   ├── IEnemyState.cs        # State interface
│   └── States/
│       ├── EnemyIdleState.cs
│       ├── EnemyChaseState.cs
│       ├── EnemyAttackState.cs
│       └── EnemyDeathState.cs
├── Interaction/
│   ├── IInteractable.cs
│   ├── InteractionTrigger.cs
│   ├── WifeNPC.cs
│   └── GenericInteractable.cs
└── Camera/
    ├── CameraConfinerSwitcher.cs
    └── CameraZoneTrigger.cs
```

## 🛠️ Tech Stack

- **Unity 2022+** with URP (Universal Render Pipeline)
- **Cinemachine** for camera management
- **New Input System** with legacy fallback

## 🚀 Getting Started

1. Clone the repository
2. Open `gamejam/` folder in Unity Hub
3. Open `Assets/Scenes/SampleScene.unity`
4. **Important Setup:**
   - Create layers: `Player`, `Enemy`
   - Assign Player layer to player object
   - Assign Enemy layer to enemy objects
5. Press **Play** to test

## 📝 License

Made with ❤️ for Game Jam