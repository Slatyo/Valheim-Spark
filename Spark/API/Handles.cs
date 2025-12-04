using UnityEngine;

namespace Spark.API
{
    /// <summary>
    /// Handle to a spawned visual effect.
    /// </summary>
    public class SparkEffectHandle
    {
        internal GameObject EffectObject { get; set; }
        internal string EffectId { get; set; }

        /// <summary>Whether the effect is still active.</summary>
        public bool IsActive => EffectObject != null;

        /// <summary>Destroys the effect.</summary>
        public void Destroy()
        {
            if (EffectObject != null)
            {
                Object.Destroy(EffectObject);
                EffectObject = null;
            }
        }

        /// <summary>Sets effect intensity.</summary>
        public void SetIntensity(float intensity)
        {
            // TODO: Implement intensity adjustment
        }
    }

    /// <summary>
    /// Handle to a continuous beam effect.
    /// </summary>
    public class SparkBeamHandle
    {
        internal GameObject BeamObject { get; set; }
        internal LineRenderer LineRenderer { get; set; }

        /// <summary>Whether the beam is still active.</summary>
        public bool IsActive => BeamObject != null;

        /// <summary>Updates the beam target position.</summary>
        public void UpdateTarget(Vector3 newTarget)
        {
            if (LineRenderer != null)
            {
                LineRenderer.SetPosition(1, newTarget);
            }
        }

        /// <summary>Updates both origin and target.</summary>
        public void UpdatePositions(Vector3 origin, Vector3 target)
        {
            if (LineRenderer != null)
            {
                LineRenderer.SetPosition(0, origin);
                LineRenderer.SetPosition(1, target);
            }
        }

        /// <summary>Destroys the beam.</summary>
        public void Destroy()
        {
            if (BeamObject != null)
            {
                Object.Destroy(BeamObject);
                BeamObject = null;
                LineRenderer = null;
            }
        }
    }

    /// <summary>
    /// Handle to an environmental zone.
    /// </summary>
    public class SparkZoneHandle
    {
        internal GameObject ZoneObject { get; set; }
        internal string ZoneId { get; set; }
        internal float Duration { get; set; }
        internal float ElapsedTime { get; set; }

        /// <summary>Whether the zone is still active.</summary>
        public bool IsActive => ZoneObject != null;

        /// <summary>Gets remaining duration (0 if permanent).</summary>
        public float RemainingDuration => Duration > 0 ? Mathf.Max(0, Duration - ElapsedTime) : -1;

        /// <summary>Destroys the zone.</summary>
        public void Destroy()
        {
            if (ZoneObject != null)
            {
                Object.Destroy(ZoneObject);
                ZoneObject = null;
            }
        }

        /// <summary>Sets zone intensity.</summary>
        public void SetIntensity(float intensity)
        {
            // TODO: Implement intensity adjustment
        }
    }

    /// <summary>
    /// Handle to an audio source.
    /// </summary>
    public class SparkAudioHandle
    {
        internal AudioSource Source { get; set; }
        internal string SoundId { get; set; }

        /// <summary>Whether the audio is still playing.</summary>
        public bool IsPlaying => Source != null && Source.isPlaying;

        /// <summary>Stops the audio.</summary>
        public void Stop()
        {
            if (Source != null)
            {
                Source.Stop();
            }
        }

        /// <summary>Sets volume.</summary>
        public void SetVolume(float volume)
        {
            if (Source != null)
            {
                Source.volume = Mathf.Clamp01(volume);
            }
        }

        /// <summary>Sets pitch.</summary>
        public void SetPitch(float pitch)
        {
            if (Source != null)
            {
                Source.pitch = pitch;
            }
        }
    }
}
