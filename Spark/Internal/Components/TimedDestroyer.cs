using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Destroys a GameObject after a delay.
    /// </summary>
    internal class TimedDestroyer : MonoBehaviour
    {
        private float _delay;
        private float _elapsed;

        public void Initialize(float delay)
        {
            _delay = delay;
            _elapsed = 0f;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _delay)
            {
                Destroy(gameObject);
            }
        }
    }
}
