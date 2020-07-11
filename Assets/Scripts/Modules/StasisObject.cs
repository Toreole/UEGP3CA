using UnityEngine;
using UEGP3CA;
using System.Collections;

namespace UEGP3CA.Modules
{
    [RequireComponent(typeof(Rigidbody))]
    public class StasisObject : MonoBehaviour
    {
        [SerializeField]
        protected Rigidbody body;
        [SerializeField]
        protected new Renderer renderer;
        [SerializeField, ColorUsage(false, true)]
        protected Color stasisColor;
        [SerializeField]
        protected LineRenderer previewLine;


        new Collider collider;
        Vector3 lastVelocity;
        bool stasis;
        Material material;

        private void Start()
        {
            material = renderer.material;
            collider = GetComponent<Collider>();
        }

        public void SetStasis(bool activeStasis)
        {
            stasis = activeStasis;
            if(activeStasis)
            {
                lastVelocity = body.velocity;
                previewLine.SetPosition(0, collider.ClosestPoint(transform.position + body.velocity));
            }
            else 
                body.velocity = lastVelocity;
            body.isKinematic = activeStasis;
            previewLine.enabled = activeStasis;
            previewLine.SetPosition(1, transform.position + lastVelocity);

            material.SetColor(Utility.MainColorProperty, stasis? stasisColor : Color.white);
        }

        public void AddImpulse(Vector3 impulse)
        {
            if(stasis)
            {
                lastVelocity += impulse / body.mass;
                UpdateLine();
            }
            else 
                body.AddForce(impulse, ForceMode.Impulse);
        }
        public void AddVelocity(Vector3 delta)
        {
            if(stasis)
            {
                lastVelocity += delta;
                UpdateLine();
            }
            else 
                body.AddForce(delta, ForceMode.VelocityChange);
        }

        private void UpdateLine()
        {
            var normVel = lastVelocity.normalized;
            var closest = collider.ClosestPoint(transform.position + normVel);
            previewLine.SetPosition(1, closest + normVel);
            previewLine.SetPosition(0, closest);
        }
    }
}