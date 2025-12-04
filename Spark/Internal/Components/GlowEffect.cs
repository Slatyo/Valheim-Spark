using Spark.Core.Configs;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Component for pulsing glow effects.
    /// </summary>
    internal class GlowEffect : MonoBehaviour
    {
        private GlowConfig _config;
        private Renderer _renderer;
        private Material _material;
        private float _time;
        private Color _baseColor;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        public void Initialize(GlowConfig config)
        {
            _config = config;
            _baseColor = config.Color;

            // Create a simple mesh for the glow
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateGlowMesh();

            _renderer = gameObject.AddComponent<MeshRenderer>();
            _material = ShaderUtils.CreateParticleMaterial(new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.3f));
            if (_material != null)
            {
                _material.EnableKeyword("_EMISSION");
                _material.SetColor(EmissionColor, _baseColor * config.Intensity);
                _renderer.material = _material;
            }
        }

        private void Update()
        {
            if (_config == null || _material == null) return;

            if (_config.PulseSpeed > 0)
            {
                _time += Time.deltaTime * _config.PulseSpeed;
                float pulse = Mathf.Lerp(_config.PulseMinIntensity, _config.Intensity, (Mathf.Sin(_time * Mathf.PI * 2) + 1) / 2);
                _material.SetColor(EmissionColor, _baseColor * pulse);
            }
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }

        private static Mesh CreateGlowMesh()
        {
            // Simple quad mesh for the glow
            var mesh = new Mesh();
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0)
            };
            mesh.uv = new[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
