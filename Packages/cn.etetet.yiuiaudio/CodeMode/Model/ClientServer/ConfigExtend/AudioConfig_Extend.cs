using System.Collections.Generic;

namespace ET
{
    public partial class AudioConfig : IAudioConfig
    {
        public string GetSoundKey()
        {
            return this.GroupId;
        }

        public List<string> GetAudioClipNames()
        {
            return this.AudioNames;
        }

        public int GetEmitterCount()
        {
            return RandomGenerator.RandomNumber(this.PlayNumMin, this.PlayNumMax + 1);
        }

        public float GetDistanceMin()
        {
            return this.DistanceMin;
        }

        public float GetDistanceMax()
        {
            return this.DistanceMax;
        }

        public float GetIntervalTimeMin()
        {
            return this.IntervalTimeMin;
        }

        public float GetIntervalTimeMax()
        {
            return this.IntervalTimeMax;
        }

        public float GetNextTimeMin()
        {
            return this.NextTimeMin;
        }

        public float GetNextTimeMax()
        {
            return this.NextTimeMax;
        }

        public float GetSpatialBlend()
        {
            return this.SpatialBlend;
        }

        public float GetVolume()
        {
            return this.Volume;
        }
    }
}
