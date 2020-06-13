using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UEGP3CA
{
    [RequireComponent(typeof(CharacterController))]
    public class SimplePlayerController : MonoBehaviour
    {
        [SerializeField]
        protected CharacterController controller;
        [SerializeField]
        protected float speed = 2.0f;
        [SerializeField]
        protected string moveInputX = "Horizontal", moveInputZ = "Vertical";
        [SerializeField]
        protected string jumpInput = "Jump";
        [SerializeField]
        protected float jumpSpeed = 3f;
        [SerializeField]
        protected float halfHeight = 1f;
        [SerializeField]
        protected LayerMask groundMask;

        float yVel;

        Vector3 previousVelXZPlane;

        //This should be kept simple for now, just move and jump
        void Update()
        {
            var movement = new Vector3(0, yVel);
            //1. Check for grounded
            if(Physics.SphereCast(transform.position, 0.1f, Vector3.down, out RaycastHit hit, halfHeight, groundMask))
            {
                //XZ Movement
                float xInput = Input.GetAxis(moveInputX);
                float zInput = Input.GetAxis(moveInputZ);

                var dir = transform.forward * zInput + transform.right * xInput;
                dir = Vector3.ClampMagnitude(dir, 1f);

                movement += dir * speed;
                previousVelXZPlane = movement;

                //jumping
                if(Input.GetButtonDown(jumpInput))
                {
                    yVel = jumpSpeed;
                }
            } 
            else 
            {
                movement += previousVelXZPlane;
                movement.y = yVel;
                yVel += Physics.gravity.y * Time.deltaTime;
            }

            controller.Move(movement * Time.deltaTime);
        }
    }
}