using UnityEngine;
using UnityEditor;
using UEGP3CA;

namespace UEGP3CA.Edit
{
    [CustomEditor(typeof(LaunchPad))]
    public class LaunchPadEditor : Editor 
    {
        SerializedProperty vectorProperty;
        Transform transform;
        private void OnEnable() 
        {
            vectorProperty = serializedObject.FindProperty("launchVelocity");
            transform = (target as MonoBehaviour).transform;
        }

        private void OnSceneGUI() 
        {
            //change the launch setting.
            Vector3 launchSpeed = Handles.DoPositionHandle(transform.position + vectorProperty.vector3Value, transform.rotation) - transform.position;
            
            //Handles.DrawLines(Vector3[])
            vectorProperty.vector3Value = launchSpeed;
            serializedObject.ApplyModifiedProperties();
            //helpers.
            float g = Mathf.Abs(Physics.gravity.y);
            Vector3 xzSpeed = new Vector3(launchSpeed.x, 0, launchSpeed.z);
            //1. calculate flight time.
            float halfT = launchSpeed.y / g;
            //2. figure out highest point. 
            Vector3 highPoint = transform.position;
            highPoint.y += Mathf.Pow(launchSpeed.y, 2) / (2*g);
            highPoint += xzSpeed * halfT;
            //3. get the endpoint.
            Vector3 endPoint = transform.position + xzSpeed * (2*halfT);
            //4. tangents.
            var tanL = launchSpeed.magnitude / Mathf.PI;
            var startTan = launchSpeed.normalized * tanL;
            var midTan = xzSpeed.normalized * tanL;
            var endTan = startTan;
                endTan.y *= -1f;
            //draw a preview of how it flies.
            Handles.DrawBezier(transform.position, highPoint, transform.position + startTan, highPoint - midTan, Color.cyan, Texture2D.whiteTexture, 1);
            Handles.DrawBezier(highPoint, endPoint, highPoint+midTan, endPoint-endTan, Color.cyan, Texture2D.whiteTexture, 1);

        }
    }
}