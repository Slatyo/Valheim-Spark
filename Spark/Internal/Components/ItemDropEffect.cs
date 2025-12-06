using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// 3D particle-based effect for dropped items with rarity.
    /// Creates floating particles, ground glow, and pulsing light.
    /// </summary>
    internal class ItemDropEffect : MonoBehaviour
    {
        private Color _color;
        private int _rarity;
        private float _time;

        private ParticleSystem _floatingParticles;
        private ParticleSystem _groundParticles;
        private Light _light;
        private GameObject _groundRing;
        private Material _ringMaterial;

        // Rarity scaling
        private float _baseScale;
        private float _particleRate;
        private float _lightIntensity;
        private float _lightRange;

        public void Initialize(Color color, int rarity)
        {
            _color = color;
            _rarity = rarity;

            // Scale based on rarity (1=Uncommon, 2=Rare, 3=Epic, 4=Legendary)
            _baseScale = 0.3f + (rarity * 0.15f);      // 0.45 to 0.9
            _particleRate = 3f + (rarity * 3f);        // 6 to 15 particles/sec
            _lightIntensity = 0.3f + (rarity * 0.3f);  // 0.6 to 1.5
            _lightRange = 1f + (rarity * 0.5f);        // 1.5 to 3

            CreateFloatingParticles();
            CreateGroundRing();
            CreateLight();

            // Epic and Legendary get extra ground particles
            if (rarity >= 3)
            {
                CreateGroundParticles();
            }
        }

        private void CreateFloatingParticles()
        {
            var go = new GameObject("FloatingParticles");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, 0.3f, 0);

            _floatingParticles = go.AddComponent<ParticleSystem>();
            var main = _floatingParticles.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 0.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.startColor = new ParticleSystem.MinMaxGradient(_color, new Color(_color.r, _color.g, _color.b, 0.5f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 30;
            main.gravityModifier = -0.1f; // Float upward slightly

            var emission = _floatingParticles.emission;
            emission.rateOverTime = _particleRate;

            var shape = _floatingParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = _baseScale * 0.5f;

            var colorOverLifetime = _floatingParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(_color, 0f), new GradientColorKey(_color, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.8f, 0.3f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = _floatingParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f)
            ));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = ShaderUtils.CreateParticleMaterial(_color);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            _floatingParticles.Play();
        }

        private void CreateGroundParticles()
        {
            var go = new GameObject("GroundParticles");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;

            _groundParticles = go.AddComponent<ParticleSystem>();
            var main = _groundParticles.main;
            main.startLifetime = 0.8f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
            main.startColor = _color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 20;
            main.gravityModifier = -0.3f; // Rise up

            var emission = _groundParticles.emission;
            emission.rateOverTime = _rarity >= 4 ? 8f : 4f;

            var shape = _groundParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = _baseScale * 0.6f;
            shape.rotation = new Vector3(-90f, 0f, 0f); // Horizontal circle

            var colorOverLifetime = _groundParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(_color, 0f), new GradientColorKey(_color, 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = ShaderUtils.CreateParticleMaterial(_color);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            _groundParticles.Play();
        }

        private void CreateGroundRing()
        {
            _groundRing = new GameObject("GroundRing");
            _groundRing.transform.SetParent(transform, false);
            _groundRing.transform.localPosition = new Vector3(0, 0.01f, 0);
            _groundRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            _groundRing.transform.localScale = new Vector3(_baseScale, _baseScale, 1f);

            var meshFilter = _groundRing.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateRingMesh(32, 0.8f, 1f);

            var renderer = _groundRing.AddComponent<MeshRenderer>();
            _ringMaterial = ShaderUtils.CreateParticleMaterial(new Color(_color.r, _color.g, _color.b, 0.4f));
            renderer.material = _ringMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void CreateLight()
        {
            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0, 0.3f, 0);

            _light = lightGo.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = _color;
            _light.intensity = _lightIntensity;
            _light.range = _lightRange;
            _light.shadows = LightShadows.None;
        }

        private void Update()
        {
            _time += Time.deltaTime;

            // Pulse the light
            if (_light != null)
            {
                float pulse = 1f + 0.2f * Mathf.Sin(_time * 2f);
                _light.intensity = _lightIntensity * pulse;
            }

            // Pulse the ground ring
            if (_ringMaterial != null)
            {
                float alpha = 0.3f + 0.15f * Mathf.Sin(_time * 1.5f);
                _ringMaterial.color = new Color(_color.r, _color.g, _color.b, alpha);

                // Subtle scale pulse
                float scale = _baseScale * (1f + 0.05f * Mathf.Sin(_time * 2f));
                _groundRing.transform.localScale = new Vector3(scale, scale, 1f);
            }

            // Slow rotation for ground ring (Legendary only)
            if (_rarity >= 4 && _groundRing != null)
            {
                _groundRing.transform.Rotate(0f, 0f, 15f * Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (_ringMaterial != null)
                Destroy(_ringMaterial);
        }

        private static Mesh CreateRingMesh(int segments, float innerRadius, float outerRadius)
        {
            var mesh = new Mesh();
            int vertexCount = segments * 2;
            var vertices = new Vector3[vertexCount];
            var uv = new Vector2[vertexCount];
            var triangles = new int[segments * 6];

            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                vertices[i * 2] = new Vector3(cos * innerRadius, sin * innerRadius, 0);
                vertices[i * 2 + 1] = new Vector3(cos * outerRadius, sin * outerRadius, 0);

                uv[i * 2] = new Vector2((float)i / segments, 0);
                uv[i * 2 + 1] = new Vector2((float)i / segments, 1);

                int nextI = (i + 1) % segments;
                int triIndex = i * 6;
                triangles[triIndex] = i * 2;
                triangles[triIndex + 1] = nextI * 2;
                triangles[triIndex + 2] = i * 2 + 1;
                triangles[triIndex + 3] = nextI * 2;
                triangles[triIndex + 4] = nextI * 2 + 1;
                triangles[triIndex + 5] = i * 2 + 1;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
