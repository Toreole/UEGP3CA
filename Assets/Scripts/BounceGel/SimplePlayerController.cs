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
        [SerializeField, Range(0f, 1f)]
        protected float airControl;
        [SerializeField]
        protected string jumpInput = "Jump", crouchButton = "Crouch";
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
        [SerializeField]
        protected LayerMask gelMask;

        private Vector3 startPos;

        float yVel;
        float currentXRot = 0f;

        bool isGrounded;
        Vector3 previousXZVelocity;

        private void Start() 
        {
            startPos = transform.position;
        }

        //This should be kept simple for now, just move and jump
        void Update()
        {
            Rotate();
            Move();
            if(Input.GetKeyDown(KeyCode.R))
                transform.position = startPos;
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
            var movement = new Vector3(0, 0);
            var currentMovement = GetMoveInputDir() * speed;
            //1. Check for grounded
            if(Physics.SphereCast(transform.position, halfHeight, Vector3.down, out RaycastHit hit, 0.1f, groundMask))
            {
                isGrounded = true;
                //XZ Movement
                
                movement += currentMovement;
                previousXZVelocity = movement;
                movement.y = yVel;

                //jumping
                if(Input.GetButtonDown(jumpInput))
                {
                    yVel = jumpSpeed;
                    movement.y = yVel;
                    isGrounded = false;
                    Debug.Log("Jump!");
                }
            } 
            else 
            {
                //just left the ground on this frame.
                if(isGrounded)
                {
                    //reset the yVel on the first frame?
                    yVel = Mathf.Clamp(yVel, 0, 100);
                }
                isGrounded = false;
                yVel += Physics.gravity.y * Time.deltaTime;
                movement = Vector3.RotateTowards(previousXZVelocity, currentMovement, Mathf.PI * airControl, 0.1f);
                previousXZVelocity = movement;
                movement.y = yVel;
            }
            
            controller.Move(movement * Time.deltaTime);
        }

        Vector3 GetMoveInputDir()
        {
            float xInput = Input.GetAxis(moveInputX);
            float zInput = Input.GetAxis(moveInputZ);

            var dir = transform.forward * zInput + transform.right * xInput;
            return Vector3.ClampMagnitude(dir, 1f);
        }

        IEnumerator Delay(System.Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action();
        }
    }
}