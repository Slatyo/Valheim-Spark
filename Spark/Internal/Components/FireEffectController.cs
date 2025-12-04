using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Creates dynamic fire/flame effects with velocity-responsive particles.
    /// Uses world-space simulation for trailing flames and velocity inheritance.
    /// Ported from Enchanting mod.
    /// </summary>
    public class FireEffectController : MonoBehaviour, IEffectController, ITargetTypeAdapter
    {
        // Fire configuration (from ElementalVFXManager)
        private static readonly Color PrimaryColor = new Color(1f, 0.7f, 0.2f, 0.9f);      // Bright orange-yellow
        private static readonly Color SecondaryColor = new Color(1f, 0.3f, 0.05f, 0.7f);   // Dark red
        private static readonly Color FadeColor = new Color(0.5f, 0.1f, 0.02f, 0f);        // Dark brown fade

        private const float BaseEmissionRate = 45f;
        private const float BaseParticleSize = 0.1f;
        private const float BaseParticleLifetime = 0.6f;
        private const float BaseStartSpeed = 0.1f;
        private const float GravityModifier = -0.2f;  // Flames rise
        private const float NoiseStrength = 0.15f;
        private const float NoiseFrequency = 3f;
        private const float BaseLightIntensity = 0.6f;
        private const float BaseLightRange = 1.2f;

        private ParticleSystem _mainParticles;
        private ParticleSystem _accentParticles;
        private Light _effectLight;

        private SparkBounds _bounds;
        private float _intensity = 1f;
        private bool _isInitialized;

        public bool IsActive => _isInitialized && _mainParticles != null && _mainParticles.isPlaying;

        /// <summary>
        /// Initialize with bounds (IEffectController interface).
        /// </summary>
        public void Initialize(SparkBounds bounds)
        {
            _bounds = bounds;

            CreateMainParticles();
            CreateAccentParticles();
            CreateLight();

            _isInitialized = true;
        }

        /// <summary>
        /// Legacy Initialize for compatibility.
        /// </summary>
        public void Initialize(float sourceLength, Vector3 sourceCenter, int lengthAxis)
        {
            _bounds = new SparkBounds
            {
                Center = sourceCenter,
                LengthAxis = lengthAxis,
                Size = GetDefaultSize(sourceLength, lengthAxis)
            };

            CreateMainParticles();
            CreateAccentParticles();
            CreateLight();

            _isInitialized = true;
        }

        private Vector3 GetDefaultSize(float length, int axis)
        {
            float thin = 0.05f;
            return axis switch
            {
                0 => new Vector3(length, thin, thin),
                2 => new Vector3(thin, thin, length),
                _ => new Vector3(thin, length, thin)
            };
        }

        /// <summary>
        /// Set effect intensity.
        /// </summary>
        public void SetIntensity(float intensity)
        {
            _intensity = Mathf.Max(0.1f, intensity);
            ApplyIntensity();
        }

        /// <summary>
        /// Configure for legacy compatibility.
        /// </summary>
        public void Configure(Color? primaryColor = null, Color? secondaryColor = null, float intensity = 1f)
        {
            _intensity = Mathf.Max(0.1f, intensity);
            ApplyIntensity();
        }

        /// <summary>
        /// Adapt effect based on target type (ITargetTypeAdapter).
        /// </summary>
        public void AdaptToTargetType(BoundsTargetType targetType)
        {
            float intensityMultiplier = targetType switch
            {
                BoundsTargetType.Weapon => 1f,
                BoundsTargetType.Tool => 0.8f,
                BoundsTargetType.Shield => 0.7f,
                BoundsTargetType.Armor => 0.4f,
                BoundsTargetType.Helmet => 0.3f,
                BoundsTargetType.Cape => 0.5f,
                BoundsTargetType.Character => 1.2f,
                BoundsTargetType.Creature => 1.5f,
                BoundsTargetType.Item => 0.6f,
                BoundsTargetType.Piece => 0.8f,
                _ => 1f
            };

            _intensity *= intensityMultiplier;
            ApplyIntensity();
        }

        private void ApplyIntensity()
        {
            if (_mainParticles != null)
            {
                var emission = _mainParticles.emission;
                emission.rateOverTime = BaseEmissionRate * _intensity;

                var main = _mainParticles.main;
                main.maxParticles = (int)(200 * _intensity);
            }

            if (_accentParticles != null)
            {
                var emission = _accentParticles.emission;
                emission.rateOverTime = BaseEmissionRate * 0.4f * _intensity;

                var main = _accentParticles.main;
                main.maxParticles = (int)(80 * _intensity);
            }

            if (_effectLight != null)
            {
                _effectLight.intensity = BaseLightIntensity * _intensity;
            }
        }

        private void CreateMainParticles()
        {
            var particleObj = new GameObject("FireParticles");
            particleObj.transform.SetParent(transform, false);
            particleObj.transform.localPosition = _bounds.Center;

            _mainParticles = particleObj.AddComponent<ParticleSystem>();
            var psRenderer = particleObj.GetComponent<ParticleSystemRenderer>();

            // Main module - world space for trailing
            var main = _mainParticles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(BaseParticleLifetime * 0.8f, BaseParticleLifetime * 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(BaseStartSpeed * 0.5f, BaseStartSpeed * 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(BaseParticleSize * 0.6f, BaseParticleSize * 1.4f);
            main.startColor = PrimaryColor;
            main.gravityModifier = GravityModifier;
            main.maxParticles = (int)(200 * _intensity);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            // Inherit velocity from weapon movement (30%)
            var inheritVelocity = _mainParticles.inheritVelocity;
            inheritVelocity.enabled = true;
            inheritVelocity.mode = ParticleSystemInheritVelocityMode.Initial;
            inheritVelocity.curveMultiplier = 0.3f;

            // Emission
            var emission = _mainParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = BaseEmissionRate * _intensity;

            // Shape - Box along weapon
            var shape = _mainParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.position = Vector3.zero;
            shape.scale = _bounds.GetParticleShapeScale();

            // Color over lifetime - orange to red to dark
            var colorOverLifetime = _mainParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(PrimaryColor, 0f),
                    new GradientColorKey(SecondaryColor, 0.5f),
                    new GradientColorKey(FadeColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.2f, 0f),
                    new GradientAlphaKey(PrimaryColor.a, 0.15f),
                    new GradientAlphaKey(SecondaryColor.a, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Size over lifetime - grows then shrinks
            var sizeOverLifetime = _mainParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.3f),
                new Keyframe(0.2f, 1f),
                new Keyframe(0.7f, 0.7f),
                new Keyframe(1f, 0f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Rotation for fire flickering
            var rotationOverLifetime = _mainParticles.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);

            // Noise for organic movement
            var noise = _mainParticles.noise;
            noise.enabled = true;
            noise.strength = NoiseStrength;
            noise.frequency = NoiseFrequency;
            noise.scrollSpeed = 1.5f;
            noise.damping = true;
            noise.quality = ParticleSystemNoiseQuality.Medium;

            // Velocity - fire rises
            var velocityOverLifetime = _mainParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);

            // Renderer - use fire_flame texture
            psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            psRenderer.sortMode = ParticleSystemSortMode.Distance;
            psRenderer.minParticleSize = 0.01f;
            psRenderer.maxParticleSize = 0.5f;

            var material = TextureLoader.GetMaterial("fire_flame", PrimaryColor);
            psRenderer.material = material ?? ShaderUtils.CreateParticleMaterial(PrimaryColor);

            _mainParticles.Play();
        }

        private void CreateAccentParticles()
        {
            var particleObj = new GameObject("FireEmbers");
            particleObj.transform.SetParent(transform, false);
            particleObj.transform.localPosition = _bounds.Center;

            _accentParticles = particleObj.AddComponent<ParticleSystem>();
            var psRenderer = particleObj.GetComponent<ParticleSystemRenderer>();

            // Smaller faster particles (embers/sparks)
            var main = _accentParticles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(BaseParticleLifetime * 0.4f, BaseParticleLifetime * 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(BaseStartSpeed * 0.8f, BaseStartSpeed * 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(BaseParticleSize * 0.2f, BaseParticleSize * 0.5f);
            main.startColor = Color.white;  // Start white, fade to primary
            main.gravityModifier = GravityModifier * 0.5f;
            main.maxParticles = (int)(80 * _intensity);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            // Higher velocity inheritance for embers
            var inheritVelocity = _accentParticles.inheritVelocity;
            inheritVelocity.enabled = true;
            inheritVelocity.mode = ParticleSystemInheritVelocityMode.Initial;
            inheritVelocity.curveMultiplier = 0.5f;

            // Lower emission rate
            var emission = _accentParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = BaseEmissionRate * 0.4f * _intensity;

            // Shape
            var shape = _accentParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.position = Vector3.zero;
            shape.scale = _bounds.GetParticleShapeScale();

            // Color - white to orange, brighter fade
            var colorOverLifetime = _accentParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(PrimaryColor, 0.3f),
                    new GradientColorKey(FadeColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.2f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Quick size shrink
            var sizeOverLifetime = _accentParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.3f, 0.6f),
                new Keyframe(1f, 0f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // More erratic noise for sparkle
            var noise = _accentParticles.noise;
            noise.enabled = true;
            noise.strength = NoiseStrength * 1.5f;
            noise.frequency = NoiseFrequency * 1.5f;
            noise.damping = true;

            // Renderer - use fire_ember texture
            psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            psRenderer.sortMode = ParticleSystemSortMode.Distance;
            psRenderer.minParticleSize = 0.005f;
            psRenderer.maxParticleSize = 0.2f;

            var material = TextureLoader.GetMaterial("fire_ember", PrimaryColor);
            psRenderer.material = material ?? ShaderUtils.CreateParticleMaterial(PrimaryColor);

            _accentParticles.Play();
        }

        private void CreateLight()
        {
            var lightObj = new GameObject("FireLight");
            lightObj.transform.SetParent(transform, false);
            lightObj.transform.localPosition = _bounds.Center;

            _effectLight = lightObj.AddComponent<Light>();
            _effectLight.type = LightType.Point;
            _effectLight.color = PrimaryColor;
            _effectLight.intensity = BaseLightIntensity * _intensity;
            _effectLight.range = BaseLightRange;
            _effectLight.shadows = LightShadows.None;
        }

        private void Update()
        {
            if (!_isInitialized || _effectLight == null) return;

            // Flicker the light
            float flicker = 1f + (Mathf.PerlinNoise(Time.time * 8f, 0f) - 0.5f) * 0.3f;
            _effectLight.intensity = BaseLightIntensity * _intensity * flicker;
        }

        private void OnDestroy()
        {
            if (_mainParticles != null)
                Destroy(_mainParticles.gameObject);
            if (_accentParticles != null)
                Destroy(_accentParticles.gameObject);
            if (_effectLight != null)
                Destroy(_effectLight.gameObject);
        }
    }
}
