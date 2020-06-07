using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UEGP3CA
{
    [RequireComponent(typeof(Camera))]
    public class FreeLookCamera : MonoBehaviour
    {
        [SerializeField]
        protected float speed, rotationSpeed;
        [SerializeField]
        protected string forwardAxis, sideAxis;
        [SerializeField]
        protected string rotationXAxis, rotationYAxis;

        private void Update()
        {
            //get input
            float rotX = Input.GetAxis(rotationXAxis);
            float rotY = Input.GetAxis(rotationYAxis);

            float zMove = Input.GetAxis(forwardAxis);
            float xMove = Input.GetAxis(sideAxis);

            //Rotate the camera.
            var curSpeed = rotationSpeed * Time.deltaTime;
            transform.Rotate(new Vector3(rotX * curSpeed, 0), Space.Self);
            transform.Rotate(new Vector3(0, rotY * curSpeed), Space.World);

            //Move
            var dir = transform.right * xMove + transform.forward * zMove;
            dir = Vector3.ClampMagnitude(dir, 1);
            var delta = dir * speed * Time.deltaTime;
            transform.position += delta;
        }
    }
}