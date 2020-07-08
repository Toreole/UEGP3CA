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

        Vector3 lastVelocity;
        bool stasis;
        Material material;

        private IEnumerator Start()
        {
            material = renderer.material;
                SetStasis(true);
            yield return new WaitForSeconds(2);
                SetStasis(false);
        }

        public void SetStasis(bool activeStasis)
        {
            stasis = activeStasis;
            if(activeStasis)
                lastVelocity = body.velocity;
            else 
                body.velocity = lastVelocity;
            body.isKinematic = !body.isKinematic;

            material.SetColor(Utility.MainColorProperty, stasis? stasisColor : Color.white);
        }
    }
}