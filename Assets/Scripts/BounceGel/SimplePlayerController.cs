using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UEGP3CA
{
    [RequireComponent(typeof(CharacterController))]
    public class SimplePlayerController : MonoBehaviour, ILaunchable
    {
        [SerializeField]
        protected CharacterController controller;
        [SerializeField]
        protected float speed = 2.0f;
        [SerializeField]
        protected string moveInputX = "Horizontal", moveInputZ = "Vertical";
        [SerializeField, Range(0f, 2f)]
        protected float airControl = 1f;
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

        //runtime cache and helper vars.
        private Vector3 startPos;
        float yVel = 0f;
        float currentXRot = 0f;

        bool isGrounded;
        bool isBouncyGround = false;
        Vector3 previousXZVelocity;
        Vector3 previousVelocity;

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

            //rotate camera to a given limit.
            var delta = Mathf.Clamp(cRot * xIn, rotXMinMax.x - currentXRot, rotXMinMax.y - currentXRot);
            currentXRot += delta;
            cam.Rotate(delta, 0, 0);
        }

        void Move()
        {
            var movement = Vector3.zero;
            var currentMovement = GetMoveInputDir() * speed;
            //1. Check for grounded
            if(Physics.SphereCast(transform.position, halfHeight, Vector3.down, out RaycastHit hit, 0.1f, groundMask))
            {
                //Check if the ground is bouncy.
                isBouncyGround = Physics.CheckSphere(hit.point, 0.15f, gelMask);
                //if just entering ground, and its bouncy, reflect, only do this if not crouching.
                if(!isGrounded && isBouncyGround && !Input.GetButton(crouchButton))
                {
                    ReflectOff(hit.normal);
                    isGrounded = true;
                }
                else 
                {
                    //set grounded.
                    isGrounded = true;
                    //set movement to be the input.
                    movement = currentMovement;
                    previousXZVelocity = movement;
                    movement.y = yVel;

                    //jump when the button is pressed.
                    if(Input.GetButtonDown(jumpInput))
                    {
                        //jump higher when standing on bouncy ground.
                        yVel = isBouncyGround ? jumpSpeed * 2f : jumpSpeed;
                        movement.y = yVel;
                        isGrounded = false;
                    }
                }
            } 
            else 
            {
                //just left the ground on this frame.
                if(isGrounded)
                {
                    //clamp yVelocity upon leaving the ground. shouldnt be negative.
                    yVel = Mathf.Clamp(yVel, 0, 100);
                }
                //set grounded
                isGrounded = false;
                //accelerate on the vertical axis
                yVel += Physics.gravity.y * Time.deltaTime;
                //rotate movement towards the input, max angle = airControl * pi / second
                movement = Vector3.RotateTowards(previousXZVelocity, currentMovement, Mathf.PI * airControl * Time.deltaTime, 0.01f);
                //set the previous XZ Velocity
                previousXZVelocity = movement;
                //set the movement.y to actually do things.
                movement.y = yVel;
                
                //raycast into the moving direction, if there is gel, reflect / bounce off it.
                if(previousXZVelocity.sqrMagnitude > 0.25f)
                {
                    if(Physics.Raycast(transform.position, previousXZVelocity.normalized, out RaycastHit gelHit, halfHeight,  gelMask))
                    {
                        ReflectOff(gelHit.normal);
                    }
                }
            }
            
            //set the previous velocity, and move the player using it.
            previousVelocity = movement;
            controller.Move(movement * Time.deltaTime);

            //Local helper function to make the code a little nicer.
            void ReflectOff(Vector3 normal)
            {
                Debug.Log("Reflect");
                //Reflect the Vector off the normal
                movement = Vector3.Reflect(movement, normal);
                //Set the cache vars.
                yVel = movement.y;
                previousXZVelocity = new Vector3(movement.x, 0, movement.z);
                previousVelocity = movement;
            }
        }

        Vector3 GetMoveInputDir()
        {
            float xInput = Input.GetAxis(moveInputX);
            float zInput = Input.GetAxis(moveInputZ);

            var dir = transform.forward * zInput + transform.right * xInput;
            return Vector3.ClampMagnitude(dir, 1f);
        }

        public void Launch(Vector3 launchVelocity)
        {
            previousVelocity = launchVelocity;
            previousXZVelocity = new Vector3(launchVelocity.x, 0, launchVelocity.z);
            yVel = launchVelocity.y;
            isGrounded = false;
        }
    }
}