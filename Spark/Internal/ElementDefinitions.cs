using System.Collections.Generic;
using Spark.Core;
using Spark.Core.Configs;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Stores predefined element visual configurations.
    /// </summary>
    internal static class ElementDefinitions
    {
        private static readonly Dictionary<Element, ElementalEffectDefinition> Definitions = new Dictionary<Element, ElementalEffectDefinition>();

        static ElementDefinitions()
        {
            // None/Neutral
            Definitions[Element.None] = new ElementalEffectDefinition
            {
                PrimaryColor = new Color(1f, 1f, 1f),
                SecondaryColor = new Color(0.8f, 0.8f, 0.8f),
                EmissionRate = 20f,
                ParticleLifetime = 0.5f,
                ParticleSize = 0.05f,
                GravityModifier = 0f,
                NoiseStrength = 0.1f,
                LightColor = Color.white,
                LightIntensity = 0.3f
            };

            // Fire
            Definitions[Element.Fire] = new ElementalEffectDefinition
            {
                PrimaryColor = new Color(1f, 0.7f, 0.2f),
                SecondaryColor = new Color(1f, 0.3f, 0.05f),
                EmissionRate = 45f,
                ParticleLifetime = 0.6f,
                ParticleSize = 0.08f,
                GravityModifier = -0.2f, // Rises
                NoiseStrength = 0.15f,
                NoiseFrequency = 2f,
                LightColor = new Color(1f, 0.6f, 0.2f),
                LightIntensity = 0.6f,
                ImpactSound = "fire_hit",
                AmbientSound = "fire_loop"
            };

            // Frost
            Definitions[Element.Frost] = new ElementalEffectDefinition
            {
                PrimaryColor = new Color(0.7f, 0.9f, 1f),
                SecondaryColor = new Color(0.4f, 0.7f, 1f),
                EmissionRate = 35f,
                ParticleLifetime = 1.0f,
                ParticleSize = 0.06f,
                GravityModifier = 0.1f, // Falls slowly
                NoiseStrength = 0.08f,
                NoiseFrequency = 1f,
                LightColor = new Color(0.5f, 0.7f, 1f),
                LightIntensity = 0.4f,
                ImpactSound = "frost_hit",
                AmbientSound = "frost_loop"
            };

            // Lightning
            Definitions[Element.Lightning] = new ElementalEffectDefinition
            {
                PrimaryColor = new Color(0.9f, 0.95f, 1f),
                SecondaryColor = new Color(0.5f, 0.7f, 1f),
                EmissionRate = 70f,
                ParticleLifetime = 0.25f,
                ParticleSize = 0.04f,
                GravityModifier = 0f,
                NoiseStrength = 0.4f,
                NoiseFrequency = 10f, // Erratic
                LightColor = new Color(0.8f, 0.9f, 1f),
                LightIntensity = 0.8f,
                ImpactSound = "lightning_strike",
                AmbientSound = "lightning_hum"
            };

            // Poison
            Definitions[Element.Poison] = new ElementalEffectDefinition
            {
                PrimaryColor = new Color(0.3f, 0.9f, 0.2f),
                SecondaryColor = new Color(0.2f, 0.6f, 0.1f),
                EmissionRate = 25f,
                ParticleLifetime = 1.2f,
                ParticleSize = 0.15f, // Larger, cloud-like
                GravityModifier = -0.03f, // Slight rise
                NoiseStrength = 0.1f,
                NoiseFrequency = 0.5f,
                LightColor = new Color(0.3f, 0.8f, 0.2f),
                LightIntensity = 0.3f,
                ImpactSound = "poison_splash",
                AmbientSound = null
            };

            // Spirit
            Definitions[Element.Spirit] = new ElementalEffectDefinition
            {
                PrimaryColor = new Color(0.95f, 0.9f, 1f),
                SecondaryColor = new Color(0.7f, 0.5f, 0.9f),
                EmissionRate = 35f,
                ParticleLifetime = 0.9f,
                ParticleSize = 0.07f,
                GravityModifier = -0.08f, // Floats up
                NoiseStrength = 0.1f,
                NoiseFrequency = 1f,
                LightColor = new Color(0.9f, 0.85f, 1f),
                LightIntensity = 0.5f,
                ImpactSound = "spirit_hit",
                AmbientSound = null
            };

            // Shadow
            Definitions[Element.Shadow] = new ElementalEffectDefinition
            {
                PrimaryColor = new Color(0.2f, 0.1f, 0.3f),
                SecondaryColor = new Color(0.1f, 0.05f, 0.15f),
                EmissionRate = 30f,
                ParticleLifetime = 0.8f,
                ParticleSize = 0.1f,
                GravityModifier = 0.05f,
                NoiseStrength = 0.2f,
                NoiseFrequency = 1.5f,
                LightColor = new Color(0.3f, 0.1f, 0.4f),
                LightIntensity = 0.2f,
                ImpactSound = "shadow_hit",
                AmbientSound = null
            };

            // Arcane
            Definitions[Element.Arcane] = new ElementalEffectDefinition
            {
                PrimaryColor = new Color(0.6f, 0.3f, 1f),
                SecondaryColor = new Color(0.9f, 0.6f, 1f),
                EmissionRate = 40f,
                ParticleLifetime = 0.7f,
                ParticleSize = 0.05f,
                GravityModifier = 0f,
                NoiseStrength = 0.12f,
                NoiseFrequency = 2f,
                LightColor = new Color(0.6f, 0.4f, 1f),
                LightIntensity = 0.5f,
                ImpactSound = "arcane_hit",
                AmbientSound = null
            };
        }

        /// <summary>
        /// Gets the definition for an element.
        /// </summary>
        public static ElementalEffectDefinition Get(Element element)
        {
            if (Definitions.TryGetValue(element, out var def))
                return def;

            return Definitions[Element.None];
        }
    }
}
