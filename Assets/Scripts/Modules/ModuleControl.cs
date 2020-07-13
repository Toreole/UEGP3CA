using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UEGP3CA.Modules
{
    ///<summary>
    /// This is the stasis module from legend of zelda: botw
    ///</summary>
    public class ModuleControl : MonoBehaviour
    {
        [SerializeField]
        protected float range = 10;
        [SerializeField]
        protected Transform pivot;
        [SerializeField]
        protected LayerMask mask;

        [SerializeField]
        protected Image slomoOverlay;

        float sqrRange;
        bool inFullSlomo = false;
        StasisObject lastObject;

        private void Awake() 
        {
            sqrRange = range * range;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.F) && !inFullSlomo)
                DoStasis();
            //if an object is in stasis, release it if necessary.
            if(lastObject)
            {
                //square magnitude check for distance because its faster than magnitude.
                if(Input.GetKeyDown(KeyCode.R) || (transform.position - lastObject.transform.position).sqrMagnitude > sqrRange)
                {
                    lastObject.SetStasis(false);
                    lastObject = null;
                }
                //test adding a force to the object in stasis.
                if(Input.GetMouseButtonDown(1))
                {
                    //test
                    if(Physics.Raycast(pivot.position, pivot.forward, out RaycastHit hit, range, mask))
                        {
                            if(hit.transform.TryGetComponent<StasisObject>(out StasisObject so))
                            {
                                if(lastObject == so)
                                {
                                    so.AddImpulse(pivot.forward);
                                }
                            }
                        }
                }
            }
        }

        void DoStasis()
        {
            var slomoRoutine = StartCoroutine(EnterSloMo());
            StartCoroutine(CheckForStasisObject(slomoRoutine));
        }

        IEnumerator EnterSloMo()
        {
            var col = slomoOverlay.color;
            for(float t = 0; t < 0.4f; t += Time.unscaledDeltaTime)
            {
                col.a = t/ 0.4f;
                slomoOverlay.color = col;
                Time.timeScale = Mathf.Lerp(1f, 0.3f, col.a);
                yield return null;
            }
            inFullSlomo = true;
        }

        IEnumerator CheckForStasisObject(Coroutine slomoRoutine)
        {
            float startTime = Time.unscaledTime;
            while(Time.unscaledTime - startTime < 5) //max 5 seconds
            {
                if(Input.GetMouseButtonDown(0))
                {
                    if(Physics.Raycast(pivot.position, pivot.forward, out RaycastHit hit, range, mask))
                    {
                        if(hit.transform.TryGetComponent<StasisObject>(out StasisObject so))
                        {
                            if(lastObject)
                                lastObject.SetStasis(false);
                            so.SetStasis(true);
                            //Debug.Log("Set Stasis", so);
                            lastObject = so;

                            break;
                        }
                    }
                }
                yield return null;
            }
            if(!inFullSlomo)
                StopCoroutine(slomoRoutine);
            inFullSlomo = false;
            var c = slomoOverlay.color;
            c.a = 0;
            slomoOverlay.color = c;
            Time.timeScale = 1f;
        }

    }
}