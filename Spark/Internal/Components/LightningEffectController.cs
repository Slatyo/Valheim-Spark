using System.Collections.Generic;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Creates erratic lightning bolt effects that zap from a point to ground or targets.
    /// Uses LineRenderers for lightning arcs with procedural jagged paths.
    /// Ported from Enchanting mod with improvements.
    /// </summary>
    public class LightningEffectController : MonoBehaviour, IEffectController, ITargetTypeAdapter
    {
        // Lightning bolt settings - rare and impactful
        private const float MinZapInterval = 1.5f;
        private const float MaxZapInterval = 5f;
        private const float BoltDuration = 0.15f;
        private const float BoltWidth = 0.012f;
        private const int SegmentsPerBolt = 10;
        private const float MaxGroundDistance = 1.5f;
        private const float MinGroundDistance = 0.3f;

        // Anchor distances for chained lightning
        private const float MinAnchorDist = 0.4f;
        private const float MaxAnchorDist = 0.8f;

        // Multiple bolt support
        private const int MaxActiveBolts = 1;
        private const float ChainLightningChance = 0.2f;

        // Visual settings
        private static readonly Color BoltColorCore = new Color(0.65f, 0.75f, 1f, 0.9f);
        private static readonly Color BoltColorGlow = new Color(0.25f, 0.4f, 0.75f, 0.4f);

        private readonly List<LightningBolt> _bolts = new List<LightningBolt>();
        private float _nextZapTime;
        private Transform _sourceTransform;
        private float _sourceLength;
        private Vector3 _sourceCenter;
        private int _lengthAxis;
        private Light _flashLight;
        private Material _boltMaterial;
        private bool _isInitialized;

        // Configuration
        private float _zapIntervalMin = MinZapInterval;
        private float _zapIntervalMax = MaxZapInterval;
        private float _chainChance = ChainLightningChance;
        private Color _coreColor = BoltColorCore;
        private Color _glowColor = BoltColorGlow;
        private float _intensity = 1f;
        private SparkBounds _bounds;

        public bool IsActive => _isInitialized && _bolts.Count > 0;

        private class LightningBolt
        {
            public LineRenderer LineRenderer;
            public LineRenderer GlowRenderer;
            public float SpawnTime;
            public Light PointLight;
        }

        /// <summary>
        /// Initialize with bounds (IEffectController interface).
        /// </summary>
        public void Initialize(SparkBounds bounds)
        {
            _bounds = bounds;
            _sourceTransform = transform.parent ?? transform;
            _sourceLength = bounds.Length;
            _sourceCenter = bounds.Center;
            _lengthAxis = bounds.LengthAxis;
            _nextZapTime = Time.time + Random.Range(0.1f, 0.3f);

            CreateBoltMaterial();
            CreateFlashLight();
            _isInitialized = true;
        }

        /// <summary>
        /// Legacy Initialize for weapon/object-attached lightning.
        /// </summary>
        public void Initialize(Transform source, float length, Vector3 center, int lengthAxis)
        {
            _sourceTransform = source;
            _sourceLength = length;
            _sourceCenter = center;
            _lengthAxis = lengthAxis;
            _nextZapTime = Time.time + Random.Range(0.1f, 0.3f);

            CreateBoltMaterial();
            CreateFlashLight();
            _isInitialized = true;
        }

        /// <summary>
        /// Initialize for position-based lightning (no source transform).
        /// </summary>
        public void Initialize(Vector3 center)
        {
            _sourceTransform = transform;
            _sourceLength = 0.5f;
            _sourceCenter = center;
            _lengthAxis = 1; // Y-axis
            _nextZapTime = Time.time + Random.Range(0.1f, 0.3f);

            CreateBoltMaterial();
            CreateFlashLight();
            _isInitialized = true;
        }

        /// <summary>
        /// Set effect intensity (IEffectController interface).
        /// Higher intensity = faster zaps, more chain chance.
        /// </summary>
        public void SetIntensity(float intensity)
        {
            _intensity = Mathf.Max(0.1f, intensity);
            _zapIntervalMin = Mathf.Max(0.05f, MinZapInterval / _intensity);
            _zapIntervalMax = Mathf.Max(0.1f, MaxZapInterval / _intensity);
            _chainChance = Mathf.Clamp01(ChainLightningChance * _intensity);
        }

        /// <summary>
        /// Adapt effect based on target type (ITargetTypeAdapter).
        /// </summary>
        public void AdaptToTargetType(BoundsTargetType targetType)
        {
            float intensityMultiplier = targetType switch
            {
                BoundsTargetType.Weapon => 1f,
                BoundsTargetType.Tool => 0.7f,
                BoundsTargetType.Shield => 0.6f,
                BoundsTargetType.Armor => 0.3f,
                BoundsTargetType.Helmet => 0.2f,
                BoundsTargetType.Cape => 0.4f,
                BoundsTargetType.Character => 1.2f,
                BoundsTargetType.Creature => 1.5f,
                BoundsTargetType.Item => 0.5f,
                BoundsTargetType.Piece => 0.6f,
                _ => 1f
            };

            SetIntensity(_intensity * intensityMultiplier);
        }

        /// <summary>
        /// Configure lightning appearance and behavior.
        /// </summary>
        public void Configure(float zapIntervalMin = MinZapInterval, float zapIntervalMax = MaxZapInterval,
            float chainChance = ChainLightningChance, Color? coreColor = null, Color? glowColor = null)
        {
            _zapIntervalMin = zapIntervalMin;
            _zapIntervalMax = zapIntervalMax;
            _chainChance = chainChance;
            _coreColor = coreColor ?? BoltColorCore;
            _glowColor = glowColor ?? BoltColorGlow;

            // Update material color if already created
            if (_boltMaterial != null)
            {
                _boltMaterial.SetColor("_Color", _coreColor);
                _boltMaterial.SetColor("_TintColor", _coreColor);
            }
        }

        /// <summary>
        /// Force spawn a lightning bolt immediately.
        /// </summary>
        public void ForceSpawn()
        {
            if (_isInitialized)
                SpawnLightningBolt();
        }

        private void CreateBoltMaterial()
        {
            _boltMaterial = ShaderUtils.CreateParticleMaterial(_coreColor);
        }

        private void CreateFlashLight()
        {
            var lightObj = new GameObject("LightningFlash");
            lightObj.transform.SetParent(transform, false);
            lightObj.transform.localPosition = _sourceCenter;

            _flashLight = lightObj.AddComponent<Light>();
            _flashLight.type = LightType.Point;
            _flashLight.color = new Color(0.5f, 0.6f, 0.9f);
            _flashLight.intensity = 0f;
            _flashLight.range = 2.5f;
            _flashLight.shadows = LightShadows.None;
        }

        private void Update()
        {
            if (!_isInitialized) return;

            CleanupExpiredBolts();

            if (Time.time >= _nextZapTime)
            {
                SpawnLightningBolt();
                _nextZapTime = Time.time + Random.Range(_zapIntervalMin, _zapIntervalMax);
            }

            UpdateFlashLight();
        }

        private void SpawnLightningBolt()
        {
            if (_sourceTransform == null || _boltMaterial == null)
                return;

            Vector3 startLocal = GetRandomSourcePoint();
            Vector3 startWorld = _sourceTransform.TransformPoint(startLocal);

            // Chance for chained lightning through anchors
            if (Random.value < _chainChance)
            {
                SpawnChainedLightning(startWorld);
                return;
            }

            // Simple bolt straight down to ground
            Vector3 endWorld = FindGroundPoint(startWorld);
            if (endWorld == Vector3.zero)
                return;

            var bolt = CreateBolt(startWorld, endWorld);
            if (bolt != null)
            {
                _bolts.Add(bolt);
            }
        }

        private void SpawnChainedLightning(Vector3 startWorld)
        {
            // Arc 1: source to nearby random anchor
            Vector3 anchor1 = GetNearbyAnchor(startWorld);
            var bolt1 = CreateBolt(startWorld, anchor1);
            if (bolt1 != null)
                _bolts.Add(bolt1);

            // 50% chance for second anchor, otherwise go straight to ground
            if (Random.value < 0.5f)
            {
                Vector3 anchor2 = GetNearbyAnchor(anchor1);
                var bolt2 = CreateBolt(anchor1, anchor2);
                if (bolt2 != null)
                    _bolts.Add(bolt2);

                Vector3 ground = FindGroundPoint(anchor2);
                var bolt3 = CreateBolt(anchor2, ground);
                if (bolt3 != null)
                    _bolts.Add(bolt3);
            }
            else
            {
                Vector3 ground = FindGroundPoint(anchor1);
                var bolt2 = CreateBolt(anchor1, ground);
                if (bolt2 != null)
                    _bolts.Add(bolt2);
            }
        }

        private Vector3 GetNearbyAnchor(Vector3 fromPoint)
        {
            Vector3 direction = Random.onUnitSphere;
            float dist = Random.Range(MinAnchorDist, MaxAnchorDist);
            return fromPoint + direction * dist;
        }

        private Vector3 GetRandomSourcePoint()
        {
            float t = Random.Range(-0.5f, 0.5f);
            Vector3 offset = Vector3.zero;

            switch (_lengthAxis)
            {
                case 0: offset = new Vector3(t * _sourceLength, 0, 0); break;
                case 2: offset = new Vector3(0, 0, t * _sourceLength); break;
                default: offset = new Vector3(0, t * _sourceLength, 0); break;
            }

            return _sourceCenter + offset;
        }

        private Vector3 FindGroundPoint(Vector3 startWorld)
        {
            Vector3 direction = Vector3.down;

            // Raycast to find ground
            if (Physics.Raycast(startWorld, direction, out RaycastHit hit, MaxGroundDistance,
                LayerMask.GetMask("terrain", "Default", "static_solid", "piece")))
            {
                if (hit.distance >= MinGroundDistance)
                {
                    return hit.point;
                }
            }

            // Fallback: straight down
            float dist = Random.Range(MinGroundDistance, MaxGroundDistance * 0.7f);
            Vector3 endPoint = startWorld + Vector3.down * dist;

            if (endPoint.y >= startWorld.y)
                endPoint.y = startWorld.y - MinGroundDistance;

            return endPoint;
        }

        private LightningBolt CreateBolt(Vector3 start, Vector3 end)
        {
            // Validate positions
            if (float.IsNaN(start.x) || float.IsNaN(end.x) ||
                float.IsInfinity(start.x) || float.IsInfinity(end.x))
            {
                return null;
            }

            float dist = Vector3.Distance(start, end);
            if (dist > MaxGroundDistance)
            {
                Vector3 dir = (end - start).normalized;
                if (float.IsNaN(dir.x))
                    return null;
                end = start + dir * MaxGroundDistance;
                dist = MaxGroundDistance;
            }

            if (dist < 0.05f)
                return null;

            var boltObj = new GameObject("LightningBolt");
            boltObj.transform.SetParent(transform, false);

            // Main bolt line
            var lr = boltObj.AddComponent<LineRenderer>();
            lr.material = _boltMaterial;
            lr.startColor = _coreColor;
            lr.endColor = _coreColor;
            lr.startWidth = BoltWidth;
            lr.endWidth = BoltWidth * 0.3f;
            lr.positionCount = SegmentsPerBolt;
            lr.useWorldSpace = true;
            lr.alignment = LineAlignment.View;

            GenerateJaggedPath(lr, start, end);

            // Create glow effect (wider, more transparent)
            var glowObj = new GameObject("BoltGlow");
            glowObj.transform.SetParent(boltObj.transform, false);

            var glowLr = glowObj.AddComponent<LineRenderer>();
            glowLr.material = _boltMaterial;
            glowLr.startColor = _glowColor;
            glowLr.endColor = _glowColor;
            glowLr.startWidth = BoltWidth * 4f;
            glowLr.endWidth = BoltWidth * 1.5f;
            glowLr.positionCount = SegmentsPerBolt;
            glowLr.useWorldSpace = true;
            glowLr.alignment = LineAlignment.View;

            // Copy positions to glow
            Vector3[] positions = new Vector3[SegmentsPerBolt];
            lr.GetPositions(positions);
            glowLr.SetPositions(positions);

            // Create impact light at ground
            var impactLightObj = new GameObject("ImpactLight");
            impactLightObj.transform.position = end;
            impactLightObj.transform.SetParent(boltObj.transform, false);

            var impactLight = impactLightObj.AddComponent<Light>();
            impactLight.type = LightType.Point;
            impactLight.color = new Color(0.4f, 0.5f, 0.85f);
            impactLight.intensity = 1.5f;
            impactLight.range = 1.2f;
            impactLight.shadows = LightShadows.None;

            return new LightningBolt
            {
                LineRenderer = lr,
                GlowRenderer = glowLr,
                SpawnTime = Time.time,
                PointLight = impactLight
            };
        }

        private void GenerateJaggedPath(LineRenderer lr, Vector3 start, Vector3 end)
        {
            Vector3[] positions = new Vector3[SegmentsPerBolt];
            positions[0] = start;
            positions[SegmentsPerBolt - 1] = end;

            Vector3 direction = end - start;
            float length = direction.magnitude;

            if (length < 0.01f || float.IsNaN(length) || float.IsInfinity(length))
            {
                for (int i = 0; i < SegmentsPerBolt; i++)
                    positions[i] = start;
                lr.SetPositions(positions);
                return;
            }

            length = Mathf.Min(length, MaxGroundDistance);

            // Minimum jag - proportionally larger for visibility
            float minJag = 0.05f;
            float jagAmount = Mathf.Max(minJag, length * 0.2f);

            for (int i = 1; i < SegmentsPerBolt - 1; i++)
            {
                float t = i / (float)(SegmentsPerBolt - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);

                // Less jagged at the ends
                float jagScale = 0.3f + 0.7f * (1f - Mathf.Abs(t - 0.5f) * 2f);
                float jag = jagAmount * jagScale;

                // Offset in random world directions
                Vector3 offset = new Vector3(
                    Random.Range(-jag, jag),
                    Random.Range(-jag, jag),
                    Random.Range(-jag, jag)
                );

                positions[i] = basePos + offset;

                if (float.IsNaN(positions[i].x))
                    positions[i] = basePos;
            }

            lr.SetPositions(positions);
        }

        private void CleanupExpiredBolts()
        {
            for (int i = _bolts.Count - 1; i >= 0; i--)
            {
                var bolt = _bolts[i];
                if (Time.time - bolt.SpawnTime >= BoltDuration)
                {
                    if (bolt.LineRenderer != null)
                    {
                        Destroy(bolt.LineRenderer.gameObject);
                    }
                    _bolts.RemoveAt(i);
                }
            }
        }

        private void UpdateFlashLight()
        {
            if (_flashLight == null) return;
            float targetIntensity = _bolts.Count > 0 ? 1.5f * _bolts.Count : 0f;
            _flashLight.intensity = Mathf.Lerp(_flashLight.intensity, targetIntensity, Time.deltaTime * 30f);
        }

        private void OnDestroy()
        {
            foreach (var bolt in _bolts)
            {
                if (bolt.LineRenderer != null)
                {
                    Destroy(bolt.LineRenderer.gameObject);
                }
            }
            _bolts.Clear();

            if (_boltMaterial != null)
            {
                Destroy(_boltMaterial);
            }
        }
    }
}
