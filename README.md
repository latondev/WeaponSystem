# WeaponSystem - Unity Weapon System

A comprehensive, modular weapon system for Unity with ScriptableObject-based configuration.

## Features

- **Modular System**: Separate subsystems for Aim, Ammo, Audio, Muzzle, and Recoil
- **Multiple Fire Modes**: Single, Auto, Burst, and Spread firing modes
- **Projectile System**: Support for Normal, Ballistic, and Homing projectiles
- **Config-Based Design**: ScriptableObject configs for easy weapon creation
- **Editor Tools**: Custom Editor Window with 2D/3D live preview
- **Auto-Targeting**: Automatic enemy detection and targeting
- **Damage System**: IDamageable and IStructure interfaces for hit detection
- **Code Generation**: Built-in tool to generate all system files

## Requirements

- Unity 6000.0 or higher
- .NET Framework 4.7.1

## Structure

```
WeaponSystem/
├── Runtime/               # Runtime code
│   ├── Weapon.cs                  # Main weapon component
│   ├── WeaponTargeting.cs         # Auto-targeting system
│   ├── Configs/                   # ScriptableObject configs
│   │   ├── WeaponConfig.cs
│   │   ├── AmmoConfig.cs
│   │   ├── AudioConfig.cs
│   │   ├── FireModeConfig.cs
│   │   ├── MuzzleConfig.cs
│   │   ├── ProjectileConfig.cs
│   │   ├── RecoilConfig.cs
│   │   └── AimConfig.cs
│   ├── FireModes/                 # Fire mode implementations
│   │   ├── IWeaponFireMode.cs
│   │   ├── AutoFireMode.cs
│   │   ├── BurstFireMode.cs
│   │   ├── SingleFireMode.cs
│   │   └── SpreadFireMode.cs
│   ├── Systems/                   # Weapon subsystems
│   │   ├── AimSystem.cs
│   │   ├── AmmoSystem.cs
│   │   ├── AudioSystem.cs
│   │   ├── MuzzleSystem.cs
│   │   └── RecoilSystem.cs
│   └── Projectiles/               # Projectile logic
│       ├── Projectile.cs
│       └── IDamageable.cs
├── Editor/                # Editor-only code
│   ├── WeaponConfigEditorWindow.cs  # Config editor with preview
│   ├── WeaponConfigInspector.cs     # Custom inspector
│   └── WeaponSystemGenerator.cs     # Code generation tool
├── Tests/                 # Test assemblies
│   ├── Editor/
│   └── Runtime/
└── Samples~/              # Sample assets (excluded from package)
    ├── Ak.asset
    └── pngegg 1.prefab
```

## Usage

### Creating a Weapon

1. **Create Config**:
   - Right-click in Project window → Create → Weapon System → Weapon Config
   - Configure fire mode, muzzle, projectile, ammo, recoil, audio, and aim settings

2. **Create Projectile Prefab**:
   - Create a GameObject
   - Add `Projectile` component
   - Assign to `ProjectileConfig.projectilePrefab`

3. **Setup Weapon**:
   - Create a GameObject
   - Add `Weapon` component
   - Assign your `WeaponConfig`
   - Optionally add `WeaponTargeting` component

### Editor Tools

#### Weapon Config Editor
- Open via **Tools > Weapon Config Editor**
- Live 2D/3D preview with projectile simulation
- Tabbed interface for different configuration sections
- Auto-fire and time-scale controls

#### Code Generator
- Open via **Tools > Generate Weapon System**
- Generates all system files in specified folder
- Useful for quick project setup

### Fire Modes

| Mode | Description |
|------|-------------|
| Single | One shot per click |
| Auto | Continuous firing while held |
| Burst | Multiple shots per click |
| Spread | Multiple projectiles at once |

### Projectile Types

| Type | Description |
|------|-------------|
| Normal | Straight-line movement |
| Ballistic | Gravity-affected arc |
| Homing | Tracks target automatically |

### Targeting Modes

| Mode | Description |
|------|-------------|
| Manual | Set target manually via code |
| NearestEnemy | Auto-detect nearest enemy |
| NearestInCrosshair | Nearest to aim direction |
| MousePosition | Target mouse position |

## Key Classes

- **Weapon**: Main component (`Runtime/Weapon.cs:6`)
- **WeaponConfig**: Central configuration ScriptableObject (`Runtime/Configs/WeaponConfig.cs`)
- **WeaponConfigEditorWindow**: Editor tool (`Editor/WeaponConfigEditorWindow.cs:10`)
- **WeaponTargeting**: Auto-targeting component (`Runtime/WeaponTargeting.cs:5`)
- **IWeaponFireMode**: Fire mode interface (`Runtime/FireModes/IWeaponFireMode.cs:3`)

## Data Models

### WeaponConfig
```csharp
- weaponName (string)
- weaponIcon (Sprite)
- fireModeConfig (FireModeConfig)
- muzzleConfigs (MuzzleConfig[])
- projectileConfig (ProjectileConfig)
- ammoConfig (AmmoConfig)
- recoilConfig (RecoilConfig)
- audioConfig (AudioConfig)
- aimConfig (AimConfig)
```

### ProjectileConfig
```csharp
- projectilePrefab (GameObject)
- type (ProjectileType)
- speed (float)
- unlimitedLifetime (bool)
- lifetime (float)
- destroyOnDistanceTraveled (bool)
- maxDistance (float)
- gravityModifier (float)
- followStrength (float)
- damage (int)
- damageToStructure (int)
- impactEffect (GameObject)
```

## API Examples

### Programmatic Firing
```csharp
var weapon = GetComponent<Weapon>();

// Set fire input programmatically
weapon.SetFireInput(isHolding, wasPressed);

// Fire immediately
weapon.TryFire();

// Reload
weapon.TryReload();

// Add ammo
weapon.AddAmmo(50);

// Set projectile target for homing
weapon.SetProjectileTarget(targetTransform);
```

### Weapon Events
```csharp
var weapon = GetComponent<Weapon>();

weapon.onShoot.AddListener(() => {
    // Handle shot fired
});

weapon.onReloadStart.AddListener(() => {
    // Handle reload start
});

weapon.onReloadEnd.AddListener(() => {
    // Handle reload complete
});
```

### Custom Targeting
```csharp
var targeting = GetComponent<WeaponTargeting>();

// Set manual target
targeting.SetManualTarget(enemyTransform);

// Get current target
Transform currentTarget = targeting.GetCurrentTarget();
```

## Extending the System

### Adding Custom Fire Mode
1. Implement `IWeaponFireMode` interface
2. Handle `Update()`, `CanFire()`, `OnFireStarted()`, and `Fire()`
3. Add new enum value to `FireMode`

### Adding Custom Projectile Type
1. Add new value to `ProjectileType` enum
2. Extend `Projectile.UpdateMovement()` with switch case
3. Add configuration fields to `ProjectileConfig`

## Assembly Definitions

- **WeaponSystem.Runtime**: All runtime code
- **WeaponSystem.Editor**: Editor-only code
- **WeaponSystem.Tests.Runtime**: Runtime tests
- **WeaponSystem.Tests.Editor**: Editor tests

## Changelog

### [1.0.0] - 2026-02-23
- Initial release
- Modular weapon system with ScriptableObject configs
- Four fire modes: Single, Auto, Burst, Spread
- Three projectile types: Normal, Ballistic, Homing
- Editor Window with 2D/3D preview
- Auto-targeting system
- Code generation tool

## Author

latondev

## License

See package.json
