# Spark - Shared VFX & Audio Effects Framework

A modular visual and audio effects library for the Valheim mod ecosystem. Provides elemental particles, weapon glows, creature auras, impact effects, and audio management with pooling and LOD support.

## Features

- **Elemental Effects**: Fire, Frost, Lightning, Poison, Spirit, Shadow, Arcane
- **Weapon Effects**: Glow, trails, full elemental enchantments
- **Creature Auras**: Ring, sphere, pillar, ground effects with presets (Enraged, Frozen, Elite, Boss)
- **Impact Effects**: One-shot hits and explosions
- **Status Visuals**: Persistent buffs/debuffs (Burning, Freezing, Poisoned, etc.)
- **Procedural Effects**: Lightning bolts, chain lightning, beams, ground cracks
- **Environmental Zones**: Poison clouds, fire fields, frost zones
- **Audio System**: Spatial audio with pooling and category-based volume control
- **Performance**: Object pooling, LOD system, configurable limits

## Installation

1. Install [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)
2. Install [Jotunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/)
3. Download and extract to `BepInEx/plugins/`

## Usage (For Mod Developers)

### Add Dependency

```csharp
[BepInDependency("com.spark.valheim")]
```

### Elemental Effects

```csharp
using Spark.API;
using Spark.Core;

// Attach fire effect to a GameObject
var handle = SparkVFX.AttachElemental(weaponVisual, Element.Fire, new ElementalConfig
{
    Intensity = 1.0f,
    Scale = 1.0f,
    TrailEnabled = true
});

// Remove effects
SparkVFX.RemoveElemental(weaponVisual);
```

### Weapon Effects

```csharp
// Add glow
SparkWeapon.AddGlow(weapon, new GlowConfig
{
    Color = Color.red,
    Intensity = 0.8f,
    PulseSpeed = 1f
});

// Add trail
SparkWeapon.AddTrail(weapon, new TrailConfig
{
    Color = Color.blue,
    Width = 0.1f,
    Duration = 0.3f
});

// Full elemental weapon effect
SparkWeapon.AddElementalEffect(weapon, Element.Lightning, new WeaponEffectConfig
{
    ParticleIntensity = 1.0f,
    GlowIntensity = 0.5f,
    TrailEnabled = true,
    LightEnabled = true
});
```

### Creature Auras

```csharp
// Preset auras
SparkAura.AttachEnraged(creature);
SparkAura.AttachElite(creature);
SparkAura.AttachBoss(creature);

// Custom aura
var auraId = SparkAura.Attach(creature, new AuraConfig
{
    Type = AuraType.Ring,
    Color = Color.red,
    Radius = 2f,
    Pulse = true
});

// Remove
SparkAura.Remove(creature, auraId);
```

### Impact Effects

```csharp
// Preset impacts
SparkImpact.Spawn(position, ImpactType.FireHit);
SparkImpact.Spawn(position, ImpactType.LightningStrike);

// Explosion
SparkImpact.Explosion(position, new ExplosionConfig
{
    Element = Element.Fire,
    Radius = 5f,
    CameraShake = true
});
```

### Status Effects

```csharp
// Preset statuses
SparkStatus.AttachBurning(character);
SparkStatus.AttachFreezing(character);

// Custom status
var statusId = SparkStatus.Attach(character, new StatusConfig
{
    Type = StatusType.Poisoned,
    Intensity = 1f,
    AttachPoint = AttachPoint.Body
});
```

### Procedural Effects

```csharp
// Lightning bolt
SparkProcedural.LightningBolt(start, end, new LightningConfig
{
    Segments = 10,
    JaggedAmount = 0.1f,
    Branches = 2
});

// Chain lightning
SparkProcedural.ChainLightning(origin, targets, new ChainConfig
{
    MaxChains = 5,
    ChainDelay = 0.1f
});

// Continuous beam
var beam = SparkProcedural.CreateBeam(origin, target, new BeamConfig
{
    Element = Element.Arcane,
    Width = 0.1f
});
beam.UpdateTarget(newTarget);
beam.Destroy();
```

### Audio

```csharp
// Play at position
SparkAudio.Play("fire_impact", position, new AudioConfig
{
    Volume = 0.8f,
    Category = SoundCategory.Impact
});

// Play attached (follows object)
var handle = SparkAudio.PlayAttached("burning_loop", gameObject, loop: true);
handle.Stop();

// UI sounds (2D)
SparkAudio.PlayUI("legendary_drop");

// Volume control
SparkAudio.SetCategoryVolume(SoundCategory.Impact, 0.8f);
```

### Settings

```csharp
// Runtime settings
SparkSettings.QualityMultiplier = 0.5f; // Reduce for performance
SparkSettings.MaxActiveParticleSystems = 30;
SparkSettings.EffectsEnabled = false; // Disable all effects
SparkSettings.AudioEnabled = false; // Disable all audio
```

## Configuration

Edit `BepInEx/config/com.spark.valheim.cfg`:

```ini
[Quality]
QualityMultiplier = 1.0
MaxParticleSystems = 50
MaxParticlesPerSystem = 200
MaxLights = 20

[LOD]
FullQualityDistance = 15
ReducedDistance = 30
MinimalDistance = 50
CullDistance = 100

[Audio]
MasterVolume = 1.0
Enabled = true

[General]
EffectsEnabled = true
```

## Dependencies

- Jotunn (required)

## For Other Mods

Spark is designed as a base layer for the mod ecosystem:
- **Enchanting**: Uses Spark for weapon enchantment visuals
- **Rift**: Uses Spark for boss/elite auras and dungeon effects
- **Denizen**: Uses Spark for creature state indicators
- **Loot**: Uses Spark for drop rarity effects

## Source Code

[GitHub Repository](https://github.com/Slatyo/Valheim-Spark)

## License

MIT License
