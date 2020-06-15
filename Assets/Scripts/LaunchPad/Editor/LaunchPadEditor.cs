using UnityEngine;
using UnityEditor;
using UEGP3CA;

namespace UEGP3CA.Edit
{
    [CustomEditor(typeof(LaunchPad))]
    public class LaunchPadEditor : Editor 
    {
        //stuff of the launchpad.
        SerializedProperty vectorProperty;

        //temp storage for preview.
        Vector3 landingZone;
        Transform transform;
        float g;
        bool recalculate = true;

        private void OnEnable() 
        {
            vectorProperty = serializedObject.FindProperty("launchVelocity");
            transform = (target as MonoBehaviour).transform;
            g = Mathf.Abs(Physics.gravity.y);
            recalculate = true;
        }

        private void OnSceneGUI() 
        {
            //i like cyan :)
            Handles.color = Color.cyan;

            //change the launch setting.
            Vector3 worldPosition = Handles.DoPositionHandle(transform.TransformPoint(vectorProperty.vector3Value), transform.rotation);
            Vector3 result = transform.InverseTransformPoint(worldPosition);
            DrawArc(transform.position, result);
            //Helper tangent line
            Handles.DrawLine(transform.position, worldPosition);
            if(recalculate = result != vectorProperty.vector3Value)
            {
                //Recalculate the landing point.
                landingZone = FindLandingZone(transform.position, transform.TransformVector(result));
            }
            result.x = 0;
            //Handles.DrawWireCube(result, Vector3.one);

            vectorProperty.vector3Value = result;
            serializedObject.ApplyModifiedProperties();
            //draw the arc.
            result = transform.TransformVector(result);
        }


        float arcLength = 0;
        float height;
        float distance;

        void DrawArc(Vector3 start, Vector3 startTangent)
        {
            if(recalculate || Mathf.Approximately(arcLength, 0))
            {
                //helpers.
                Vector3 xzSpeed = new Vector3(startTangent.x, 0, startTangent.z);

                //1. calculate flight time.
                float halfT = startTangent.y / g;

                //2. figure out highest point. 
                Vector3 highPoint = start;
                highPoint.y += Mathf.Pow(startTangent.y, 2) / (2*g);
                highPoint += xzSpeed * halfT;

                distance = Vector3.Distance(start, landingZone);
                height = highPoint.y - Vector3.Lerp(start, landingZone, 0.5f).y;
                arcLength = GetArcLength(distance, height);
            }

            //draw line segments every 0.5 units
            Vector3 lineStart = transform.position;
            //the "velocity" at which the curve is being drawn.
            Vector3 velocity = startTangent;

            for(int i = 0; i < arcLength; i++)
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

        const float step = 1f;
        const int maxStepCount = 64;
        //Start location and start velocity have to be in world-space.
        Vector3 FindLandingZone(Vector3 startLocation, Vector3 startVelocity)
        {
            //caches
            Vector3 pos = startLocation;
            Vector3 velocity = startVelocity;
            float speed = velocity.magnitude;
            Vector3 direction = startVelocity / speed;

            for(int i = 0; i < maxStepCount; i++)
            {
                //Debug.DrawRay(pos, direction, Color.red, 2);
                //step.
                if(Physics.Raycast(pos, direction, out RaycastHit hit, step, int.MaxValue, QueryTriggerInteraction.Ignore))
                {
                    //Hit an object, this must be the end.
                    var landing = (target as LaunchPad).LandingZone;
                    landing.position = hit.point;
                    landing.forward = hit.normal;
                    
                    return hit.point;
                }
                //deltaTime until next step
                float dt = step / speed;
                //offset
                pos += direction * step;
                //accelerate with gravity
                velocity.y -= g * dt;
                //set speed and direction.
                speed = velocity.magnitude;
                direction = velocity / speed;
            }
            return Vector3.zero;
        }
    }
}