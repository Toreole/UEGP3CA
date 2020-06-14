using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UEGP3CA
{
    public class LaunchPad : MonoBehaviour
    {
        [SerializeField]
        protected Vector3 launchVelocity;
        [SerializeField]
        protected Transform landingZone;

        public Transform LandingZone => landingZone;
        private Vector3 worldSpaceLaunch;

        private void Start() 
        {
            worldSpaceLaunch = transform.TransformVector(launchVelocity);
        }

        private void OnTriggerEnter(Collider other) 
        {
            ILaunchable launchable = other.GetComponent<ILaunchable>();
            if(launchable != null)
                launchable.Launch(worldSpaceLaunch);
        }
    }
}