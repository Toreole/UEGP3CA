using UnityEngine;

namespace UEGP3CA.Shouts
{
    /// <summary>
    /// This is the FusRoDah shout.
    /// </summary>
    [System.Serializable]
    public class ShoutData
    {
        [System.Serializable]
        public struct ShoutWord
        {
            public string textWord;
            public float cooldown;
            public float castTime;
            public float strength;
        }

        [SerializeField]
        protected ShoutWord[] words;

        //optimally this would be set by the editor to avoid doing this at runtime but whatever.
        public float FullCastTime => words[words.Length - 1].castTime;

        public ShoutWord this[int index] => words[index];
        public int Size => words.Length;

    }
}