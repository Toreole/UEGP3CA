using UnityEngine;
using UnityEditor;
using UEGP3CA;

namespace UEGP3CA.Edit
{
    [CustomEditor(typeof(LaunchPad))]
    public class LaunchPadEditor : Editor 
    {
        //stuff of the launchpad.
        SerializedProperty vectorAProperty;
        SerializedProperty vectorBProperty;
        SerializedProperty seconRotationProperty;
        SerializedProperty primRotationProperty;

        //temp storage for preview.
        Vector3 landingZone;
        Transform pivot;
        LaunchPad pad;
        Transform transform;
        float g;
        bool recalculate = true;
        bool manualOverride = false;

        private void OnEnable() 
        {
            vectorAProperty = serializedObject.FindProperty("launchVelocityA");
            vectorBProperty = serializedObject.FindProperty("launchVelocityB");
            seconRotationProperty = serializedObject.FindProperty("secondaryRotation");
            primRotationProperty = serializedObject.FindProperty("primaryRotation");
            
            pad = target as LaunchPad;
            transform = pad.transform;
            pivot = pad.Pivot;
            g = Mathf.Abs(Physics.gravity.y);
            recalculate = true;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            manualOverride = EditorGUILayout.Toggle("Manual rotation", manualOverride);
            if(manualOverride)
            {
                seconRotationProperty.quaternionValue = Quaternion.Euler(EditorGUILayout.Vector3Field("secondary", seconRotationProperty.quaternionValue.eulerAngles));
                primRotationProperty.quaternionValue = Quaternion.Euler(EditorGUILayout.Vector3Field("primary", primRotationProperty.quaternionValue.eulerAngles));
                pivot.rotation = pad.IsSecondary? seconRotationProperty.quaternionValue : primRotationProperty.quaternionValue;
            }

            if(pad.CanRotate)
            {
                if(Application.isPlaying)
                {
                    if(GUILayout.Button("Rotate"))
                        pad.ToggleRotation();
                }
                else 
                {
                    if(GUILayout.Button($"Set {(pad.IsSecondary? "Secondary": "Primary")} rotation"))
                    {
                        if(pad.IsSecondary)
                            seconRotationProperty.quaternionValue = pivot.rotation;
                        else
                            primRotationProperty.quaternionValue = pivot.rotation;
                    }
                    if(GUILayout.Button("Toggle active rotation"))
                    {
                        recalculate = true;
                        //inverse secondary
                        pad.IsSecondary = !pad.IsSecondary;
                        pivot.rotation = pad.IsSecondary? seconRotationProperty.quaternionValue : primRotationProperty.quaternionValue;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI() 
        {
            //stop the scene GUI when in playmode, it fucks shit up.
            if(Application.isPlaying)
                return;
            //clear the scene view and re-draw it for better visibility.
            Camera cam = SceneView.GetAllSceneCameras()[0];
            Handles.ClearCamera(new Rect(0, 0, 1000, 1000), cam);
            cam.Render();

            //if the pad is able to rotate and the inspector isnt in manual mode, show the pivot rotation handle.
            if(pad.CanRotate && !manualOverride)
            {
                //secondary direction
                Handles.color = Color.magenta;
                var seconDir = (!pad.IsSecondary? seconRotationProperty : primRotationProperty).quaternionValue * (!pad.IsSecondary ? vectorBProperty.vector3Value : vectorAProperty.vector3Value);
                Handles.DrawLine(transform.position, transform.position + seconDir);
                Handles.Label(transform.position + seconDir, (pad.IsSecondary ? "Primary" : "Secondary"));

                var rotation = Handles.RotationHandle(pivot.rotation, transform.position + pivot.up * 2f);
                if(pad.IsSecondary)
                    seconRotationProperty.quaternionValue = rotation;
                else 
                    primRotationProperty.quaternionValue = rotation;
                pivot.rotation = rotation;
            }

            //i like cyan :)
            Handles.color = Color.cyan;

            //the active vector property to edit.
            var activeVector = pad.IsSecondary? vectorBProperty : vectorAProperty;

            //change the launch setting.
            Vector3 worldPosition = Handles.DoPositionHandle(pivot.TransformPoint(activeVector.vector3Value), pivot.rotation);
            Vector3 result = pivot.InverseTransformPoint(worldPosition);
            Vector3 direction = pivot.TransformVector(result);
            //Draw the arc.
            DrawArc(transform.position, direction);
            //Helper tangent line
            Handles.DrawLine(transform.position, worldPosition);
            if(recalculate || result != activeVector.vector3Value)
            {
                //Recalculate the landing point.
                landingZone = FindLandingZone(transform.position, direction);
            }
            result.x = 0;
            //Handles.DrawWireCube(result, Vector3.one);
            activeVector.vector3Value = result;

            serializedObject.ApplyModifiedProperties();
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
        //     1                      b²       4*a + SQR(b² + 16 * a²)
        // L = - SQR(b² + 16 * a²) + --- logn( ---------------------- )
        //     2                     8*a                 b        
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
                    var landing = pad.IsSecondary? pad.LandingZoneB : pad.LandingZoneA;
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