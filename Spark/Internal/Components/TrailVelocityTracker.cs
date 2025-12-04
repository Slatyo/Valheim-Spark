using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Tracks velocity and enables/disables trail based on movement speed.
    /// </summary>
    internal class TrailVelocityTracker : MonoBehaviour
    {
        private TrailRenderer _trail;
        private float _minVelocity;
        private Vector3 _lastPosition;
        private float _currentVelocity;

        public void Initialize(TrailRenderer trail, float minVelocity)
        {
            _trail = trail;
            _minVelocity = minVelocity;
            _lastPosition = transform.position;
        }

        private void Update()
        {
            if (_trail == null) return;

            // Calculate velocity
            Vector3 currentPosition = transform.position;
            _currentVelocity = (currentPosition - _lastPosition).magnitude / Time.deltaTime;
            _lastPosition = currentPosition;

            // Enable/disable trail based on velocity
            _trail.emitting = _currentVelocity >= _minVelocity;
        }
    }
}
