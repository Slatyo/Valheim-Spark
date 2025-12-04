# Spark - Shared VFX & Audio Effects Framework

A modular visual and audio effects library for the Valheim mod ecosystem.

## Features

- **Elemental Effects**: Fire and Lightning with specialized particle systems (more coming soon)
- **Automatic Bounds Detection**: Effects automatically scale and position based on target geometry
- **Holster Persistence**: Item-based effects survive weapon holster/unholster cycles
- **Target Type Adaptation**: Effects adjust intensity based on target type (weapon, armor, creature, etc.)
- **Creature Auras**: Presets for Enraged, Frozen, Elite, Boss
- **Impact Effects**: One-shot hits and explosions with camera shake
- **Status Visuals**: Burning, Freezing, Poisoned, Blessed, Cursed effects
- **Procedural Effects**: Lightning bolts, beams
- **Environmental Zones**: Poison clouds, fire fields, frost zones
- **Audio System**: Spatial audio with pooling and category-based volume control
- **Performance**: Object pooling, LOD system, configurable limits

## For Mod Developers

Add Spark as a dependency and use the static API classes:

```csharp
using Spark.API;
using Spark.Core;

// === RECOMMENDED: Item-based effects (persist through holster/unholster) ===
// Use this for weapons, shields, and equipment
var weapon = player.GetCurrentWeapon();
SparkEffect.AttachToItem(weapon, Element.Fire, intensity: 1.0f);
SparkEffect.AttachToItem(weapon, Element.Lightning, intensity: 1.5f);

// Remove tracked effect
SparkEffect.RemoveFromItem(weapon);

// Check if item has effect
bool hasEffect = SparkEffect.HasItemEffect(weapon);

// === Direct GameObject attachment (for static objects) ===
// Use for armor, buildings, world items that don't get recreated
IEffectController controller = SparkEffect.AttachElemental(gameObject, Element.Fire, intensity: 1.0f);

// Specific effect types
FireEffectController fire = SparkEffect.AttachFire(gameObject, intensity: 1.0f);
LightningEffectController lightning = SparkEffect.AttachLightning(gameObject, intensity: 1.0f);

// Custom lightning configuration
var customLightning = SparkEffect.AttachLightning(gameObject,
    zapIntervalMin: 0.5f,
    zapIntervalMax: 2.0f,
    chainChance: 0.3f);

// Remove effects
SparkEffect.RemoveAll(gameObject);
SparkEffect.HasEffects(gameObject);

// Get calculated bounds (useful for custom effects)
SparkBounds bounds = SparkEffect.GetBounds(gameObject);

// === Creature Auras ===
SparkAura.AttachElite(creature);
SparkAura.AttachBoss(creature);
SparkAura.AttachEnraged(creature);
SparkAura.AttachFrozen(creature);

// === Impacts ===
SparkImpact.SpawnElemental(position, Element.Fire, scale: 1.5f);
SparkImpact.Explosion(position, new ExplosionConfig {
    Element = Element.Fire,
    Radius = 5f,
    CameraShake = true
});

// === Status Effects ===
SparkStatus.AttachBurning(character);
SparkStatus.AttachFreezing(character);
SparkStatus.AttachPoisoned(character);
SparkStatus.AttachBlessed(character);
SparkStatus.AttachCursed(character);
SparkStatus.RemoveAll(character);

// === Procedural Effects ===
SparkProcedural.LightningBolt(startPos, endPos, new LightningConfig {
    Segments = 12,
    JaggedAmount = 0.15f,
    Branches = 3,
    Duration = 0.3f
});

// === Audio ===
SparkAudio.Play("fire_impact", position);
```

## Elements

Currently implemented with specialized controllers:
- `Element.Fire` - Dynamic flames with velocity-responsive particles, embers, flickering light
- `Element.Lightning` - Erratic lightning bolts with chain lightning, ground strikes

Planned (currently fall back to Fire):
- `Element.Frost`
- `Element.Poison`
- `Element.Spirit`
- `Element.Shadow`
- `Element.Arcane`

## Bounds System

Spark automatically detects target geometry and type:

```csharp
SparkBounds bounds = SparkEffect.GetBounds(gameObject);

bounds.Center;      // Local-space center
bounds.Size;        // Dimensions
bounds.Length;      // Longest dimension
bounds.LengthAxis;  // Primary axis (0=X, 1=Y, 2=Z)
bounds.TargetType;  // Weapon, Tool, Shield, Armor, Helmet, Character, Creature, etc.
```

Effects automatically adapt based on `TargetType`:
- Weapons: Full intensity
- Armor/Helmet: Reduced intensity
- Creatures: Increased intensity

## Configuration

All settings adjustable via BepInEx config:
- Quality multiplier for particle counts
- LOD distances for performance scaling
- Maximum particle systems and lights
- Audio volume per category

## Debug Commands

All commands use Munin command framework. Open console (F5) and type:

```
munin spark item <element> [intensity]   - Attach to weapon (persists through holster)
munin spark attach <element> [intensity] - Attach to weapon visual (debug)
munin spark gear <element> <slot>        - Attach to gear slot
munin spark impact [element]             - Spawn impact effect
munin spark explosion [element] [radius] - Spawn explosion
munin spark bolt                         - Lightning bolt from sky
munin spark aura <preset>                - Aura on nearest creature
munin spark status <status>              - Status on player
munin spark bounds                       - Show weapon bounds
munin spark textures                     - List loaded textures
munin spark clear                        - Remove all effects
munin spark help                         - List all commands
```

## Part of the Mod Ecosystem

Spark is the shared VFX/SFX layer used by:
- **Enchanting** - Weapon enchantment effects
- **Loot** - Legendary item effects
- **Denizen** - Creature visual effects
- **Rift** - Dungeon environment effects

## Source Code

[GitHub](https://github.com/Slatyo/Valheim-Spark)
