using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UEGP3CA
{
    public class LaunchPad : MonoBehaviour
    {
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("launchVelocity")]
        protected Vector3 launchVelocityA;
        [SerializeField]
        protected Vector3 launchVelocityB;

        [SerializeField]
        protected Transform landingZoneA, landingZoneB;
        [SerializeField]
        protected Animator animator;
        [SerializeField, HideInInspector]
        protected Quaternion primaryRotation, secondaryRotation;
        [SerializeField]
        protected float rotationTime;
        [SerializeField]
        protected bool startActive = true, canRotate = false;
        [SerializeField]
        protected Transform rotationPivot;


        public Transform LandingZoneA => landingZoneA;
        public Transform LandingZoneB => landingZoneB;
        public Transform Pivot => rotationPivot;
        
        readonly int triggerID = Animator.StringToHash("Launch");
        public bool CanRotate => canRotate;
        public bool IsSecondary {get => isSecondary; set => isSecondary = value;}

        bool isSecondary;
        bool activeLaunchpad;
        bool isRotating;

        private void Start() 
        {
            landingZoneA.gameObject.SetActive(!isSecondary);
            landingZoneB.gameObject.SetActive(isSecondary);
            activeLaunchpad = startActive;
        }

        public void ToggleRotation()
        {
            if(!isRotating && canRotate)
                StartCoroutine(Rotate());
        }

        IEnumerator Rotate()
        {
            isRotating = true;
            bool activeBefore = activeLaunchpad;
            activeLaunchpad = false;
            Quaternion goal = isSecondary? primaryRotation : secondaryRotation;
            Quaternion start = rotationPivot.rotation;
            for(float t = 0; t < rotationTime; t += Time.deltaTime)
            {
                rotationPivot.rotation = Quaternion.Lerp(start, goal, t/rotationTime);
                print(t.ToString("0.00"));
                yield return null;
            }
            isSecondary = !isSecondary;
            activeLaunchpad = activeBefore;
            isRotating = false;
            landingZoneA.gameObject.SetActive(!isSecondary);
            landingZoneB.gameObject.SetActive(isSecondary);
        }

        private void OnTriggerEnter(Collider other) 
        {
            if(activeLaunchpad)
            {
                ILaunchable launchable = other.GetComponent<ILaunchable>();
                if(launchable != null)
                {
                    launchable.Launch(rotationPivot.TransformVector(isSecondary? launchVelocityB : launchVelocityA));
                    animator.SetTrigger(triggerID);
                    print("test");
                }
            }
        }

        private void OnValidate() 
        {
            if(!landingZoneA)
                landingZoneA = transform.GetChild(0);
            if(!landingZoneB)
                landingZoneB = Instantiate(landingZoneA.gameObject, Vector3.zero, Quaternion.identity, transform).transform;
        }
    }
}