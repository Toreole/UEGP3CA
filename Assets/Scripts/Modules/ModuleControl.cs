using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        StasisObject lastObject;

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.F))
                DoStasis();
            if(Input.GetKeyDown(KeyCode.R) && lastObject)
            {
                lastObject.SetStasis(false);
                lastObject = null;
            }
        }

        void DoStasis()
        {
            if(Physics.Raycast(pivot.position, pivot.forward, out RaycastHit hit, range, mask))
            {
                if(hit.transform.TryGetComponent<StasisObject>(out StasisObject so))
                {
                    if(lastObject)
                        lastObject.SetStasis(false);
                    so.SetStasis(true);
                    Debug.Log("Set Stasis", so);
                    lastObject = so;
                }
            }
        }

    }
}