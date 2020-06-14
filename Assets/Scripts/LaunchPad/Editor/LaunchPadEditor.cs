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
            DrawArc(transform.position, launchSpeed);
        }

        void DrawArc(Vector3 start, Vector3 startTangent)
        {
            //helpers.
            float g = Mathf.Abs(Physics.gravity.y);
            Vector3 xzSpeed = new Vector3(startTangent.x, 0, startTangent.z);

            //1. calculate flight time.
            float halfT = startTangent.y / g;

            //2. figure out highest point. 
            Vector3 highPoint = start;
            highPoint.y += Mathf.Pow(startTangent.y, 2) / (2*g);
            highPoint += xzSpeed * halfT;

            //3. get the endpoint.
            Vector3 endPoint = start + xzSpeed * (2*halfT);

            float distance = Vector3.Distance(start, endPoint);
            float height = highPoint.y - Vector3.Lerp(start, endPoint, 0.5f).y;
            float arcLength = GetArcLength(distance, height);

            //draw line segments every 0.5 units
            Vector3 lineStart = transform.position;
            //the "velocity" at which the curve is being drawn.
            Vector3 velocity = startTangent;

            //i like cyan :)
            Handles.color = Color.cyan;
            for(int i = 0; i < arcLength * 1; i++)
            {
                //speed in units / s
                var speed = velocity.magnitude;
                //time delta to do 0.5 units of movement
                var dt = 1f / speed;

                //the next line point.
                Vector3 lineEnd = lineStart + velocity * dt;

                //Draw this segment.
                Handles.DrawLine(lineStart, lineEnd);

                //apply gravity.
                velocity.y += Physics.gravity.y * dt;
                //move the line start.
                lineStart = lineEnd;
            }
        }

        //ARC LENGTH OF PARABOLA
        //     1                      b²     4*a + SQR(b² + 16 * a²)
        // L = - SQR(b² + 16 * a²) + --- ln( ---------------------- )
        //     2                     8*a               b        
        //b = distance from start to end
        //a = "height"/ vertical distance from midpoint to highest point.
        float GetArcLength(float distance, float height)
        {
            float bSquare = Mathf.Pow(distance, 2.0f); //b²
            float aSquare = Mathf.Pow(height, 2.0f); // a²
            float bSq16aSq = Mathf.Sqrt(bSquare + 16.0f * aSquare); // = SQR(b² + 16 * a²)

            double addA = 0.5D * bSq16aSq;
            double addB = (bSquare /  (8.0 * height)) * Mathf.Log((4.0f*height + bSq16aSq) / distance);

            return (float)(addA + addB);
        }
    }
}