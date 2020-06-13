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
        [SerializeField]
        protected Transform cam;
        [SerializeField]
        protected string rotInputX = "Mouse Y", rotInputY = "Mouse X";
        [SerializeField]
        protected float rotationSpeed = 90f;
        [SerializeField]
        protected Vector2 rotXMinMax;

        float yVel;
        float currentXRot = 0f;

        Vector3 previousVelXZPlane;

        //This should be kept simple for now, just move and jump
        void Update()
        {
            Rotate();
            Move();
        }

        void Rotate()
        {
            float xIn = -Input.GetAxis(rotInputX);
            float yIn = Input.GetAxis(rotInputY);

            float cRot = rotationSpeed * Time.deltaTime;

            transform.Rotate(0, cRot * yIn, 0);

            //rotate camera
            //currentXRot => cam.eulerAngles.x essentially (80 = down, -80 = up)
            //rotXMinMax.x = -80
            //rotXMinMax.y = 80;
            var delta = Mathf.Clamp(cRot * xIn, rotXMinMax.x - currentXRot, rotXMinMax.y - currentXRot);
            currentXRot += delta;
            cam.Rotate(delta, 0, 0);
        }

        void Move()
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