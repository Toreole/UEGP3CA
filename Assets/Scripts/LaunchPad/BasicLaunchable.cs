using UnityEngine;

namespace UEGP3CA
{
    [RequireComponent(typeof(Rigidbody))]
    public class BasicLaunchable : MonoBehaviour, ILaunchable
    {
        Rigidbody body;

        private void Start() {
            body = GetComponent<Rigidbody>();    
        }

        public void Launch(Vector3 vel)
        {
            body.velocity = vel;
        }
    }
}