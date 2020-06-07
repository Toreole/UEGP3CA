using UnityEngine;

namespace UEGP3CA.Shouts
{
    /// <summary>
    /// This is the FusRoDah shout.
    /// </summary>
    [System.Serializable]
    public class ShoutData
    {
        //the words
        public string[] words = new string[3];
        //the cooldown for each step.
        public float[] cooldown = new float[3];
        //the time to actually "cast" it.
        [Tooltip("castTime[0] should always be 0 since its an instant cast.")]
        public float[] castTime = new float[3];
        //the strength of the knockback
        public float[] strength = new float[3];

        //optimally this would be set by the editor to avoid doing this at runtime but whatever.
        public float FullCastTime
        {
            get
            {
                float sum = 0;
                foreach (float x in castTime)
                    sum += x;
                return sum;
            }
        }

        public float GetCastTimeAdditive(int index)
        {
            float t = 0f;
            for(int i = 0; i < index; i++)
            {
                t += castTime[i];
            }
            return t;
        }
    }
}