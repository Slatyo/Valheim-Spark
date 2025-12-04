using Spark.API;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Updates zone duration and auto-destroys when expired.
    /// </summary>
    internal class ZoneUpdater : MonoBehaviour
    {
        private SparkZoneHandle _handle;

        public void Initialize(SparkZoneHandle handle)
        {
            _handle = handle;
        }

        private void Update()
        {
            if (_handle == null) return;

            _handle.ElapsedTime += Time.deltaTime;

            if (_handle.Duration > 0 && _handle.ElapsedTime >= _handle.Duration)
            {
                _handle.Destroy();
            }
        }
    }
}
