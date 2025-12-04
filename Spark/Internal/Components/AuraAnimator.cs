using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Animates aura effects (pulse, rotation).
    /// </summary>
    internal class AuraAnimator : MonoBehaviour
    {
        private bool _pulse;
        private float _pulseSpeed;
        private bool _rotate;
        private float _rotationSpeed;
        private float _time;
        private ParticleSystem _particleSystem;
        private float _baseEmission;

        public void Initialize(bool pulse, float pulseSpeed, bool rotate, float rotationSpeed)
        {
            _pulse = pulse;
            _pulseSpeed = pulseSpeed;
            _rotate = rotate;
            _rotationSpeed = rotationSpeed;

            _particleSystem = GetComponent<ParticleSystem>();
            if (_particleSystem != null)
            {
                _baseEmission = _particleSystem.emission.rateOverTime.constant;
            }
        }

        private void Update()
        {
            if (_rotate)
            {
                transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
            }

            if (_pulse && _particleSystem != null)
            {
                _time += Time.deltaTime * _pulseSpeed;
                float pulse = (Mathf.Sin(_time * Mathf.PI * 2) + 1) / 2;
                pulse = Mathf.Lerp(0.5f, 1.5f, pulse);

                var emission = _particleSystem.emission;
                emission.rateOverTime = _baseEmission * pulse;
            }
        }
    }
}
