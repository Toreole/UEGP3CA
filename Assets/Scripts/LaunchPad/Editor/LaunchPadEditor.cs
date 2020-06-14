using UnityEngine;
using UnityEditor;
using UEGP3CA;

namespace UEGP3CA.Edit
{
    [CustomEditor(typeof(LaunchPad))]
    public class LaunchPadEditor : Editor 
    {
        private void OnSceneGUI() 
        {
            var pad = target as LaunchPad;    
        }
    }
}