using UnityEngine;

namespace UEGP3CA
{
    public static class Utility
    {
        public static int EmissionProperty = Shader.PropertyToID("_EmissionColor");
        public static int MainColorProperty = Shader.PropertyToID("_Color");

        //Because 2018.4 doesnt have this yet.
        public static bool TryGetComponent<T>(this Component self, out T component) where T : class
        {
            component = self.GetComponent<T>();
            return component != null;
        }
    }
}