using Spark.Core;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Returns an effect to the pool after a delay.
    /// </summary>
    internal class PoolReturner : MonoBehaviour
    {
        private Element _element;
        private float _delay;
        private float _elapsed;

        public void Initialize(Element element, float delay)
        {
            _element = element;
            _delay = delay;
            _elapsed = 0f;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _delay)
            {
                EffectPool.Return(_element, gameObject);
                Destroy(this); // Remove this component
            }
        }
    }
}
