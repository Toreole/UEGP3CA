using UnityEngine;

namespace UEGP3CA
{
    public static class Utility
    {
        public static int EmissionProperty = Shader.PropertyToID("_EmissionColor");
        public static int MainColorProperty = Shader.PropertyToID("_Color");

        public static bool TryGetComponent<T>(this Component self, out T component) where T : Component
        {
            component = self.GetComponent<T>();
            return component;
        }public static bool TryGetComponent<T>(this GameObject self, out T component) where T : Component
        {
            component = self.GetComponent<T>();
            return component;
        }
    }
}