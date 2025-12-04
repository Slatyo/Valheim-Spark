using System.Collections;
using System.Collections.Generic;
using Spark.Core.Configs;
using UnityEngine;

namespace Spark.Internal
{
    /// <summary>
    /// Handles chain lightning coroutine.
    /// </summary>
    internal class ChainLightningCoroutine : MonoBehaviour
    {
        public void StartChain(Vector3 origin, List<Vector3> targets, ChainConfig config)
        {
            StartCoroutine(ChainRoutine(origin, targets, config));
        }

        private IEnumerator ChainRoutine(Vector3 origin, List<Vector3> targets, ChainConfig config)
        {
            Vector3 currentPos = origin;
            int chainsCreated = 0;
            var boltConfig = config.BoltConfig ?? new LightningConfig();

            foreach (var target in targets)
            {
                if (chainsCreated >= config.MaxChains)
                    break;

                ProceduralEffects.CreateLightningBolt(currentPos, target, boltConfig);
                currentPos = target;
                chainsCreated++;

                yield return new WaitForSeconds(config.ChainDelay);
            }

            // Auto-destroy this component's GameObject after chain completes
            yield return new WaitForSeconds(boltConfig.Duration);
            Destroy(gameObject);
        }
    }
}
