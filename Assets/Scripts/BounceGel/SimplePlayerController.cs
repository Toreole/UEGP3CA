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
        [SerializeField]
        protected string blueGelTag;

        float yVel;
        float currentXRot = 0f;

        Vector3 previousVelXZPlane, previousVelocity;
        bool isOnBouncyGel = false;
        bool isGrounded;
        bool stick = false;

        //This should be kept simple for now, just move and jump
        void Update()
        {
            Rotate();
            Move();
            CheckForGel();
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
            //1. Check for grounded
            if(Physics.SphereCast(transform.position, halfHeight, Vector3.down, out RaycastHit hit, 0.1f, groundMask))
            {
                if(!isGrounded)
                {
                    //if it wasnt grounded before, do this:
                    StartCoroutine(Delay(
                        () => { stick = true; }
                    , 0.15f));
                }
                isGrounded = true;
                //XZ Movement
                float xInput = Input.GetAxis(moveInputX);
                float zInput = Input.GetAxis(moveInputZ);

                var dir = transform.forward * zInput + transform.right * xInput;
                dir = Vector3.ClampMagnitude(dir, 1f);

                movement += dir * speed;
                previousVelXZPlane = movement;

                if(stick)
                {
                    yVel = Mathf.Clamp(yVel, -0.5f, 0.0f);
                }
                movement.y = yVel;

                //jumping
                if(Input.GetButtonDown(jumpInput))
                {
                    yVel = isOnBouncyGel? 2f * jumpSpeed: jumpSpeed;
                    movement.y = yVel;
                    stick = false;
                }
            } 
            else 
            {
                isGrounded = false;
                stick = false;
                movement += previousVelXZPlane;
                yVel += Physics.gravity.y * Time.deltaTime;
                movement.y = yVel;
            }
            
            previousVelocity = movement;
            controller.Move(movement * Time.deltaTime);
        }

        void CheckForGel()
        {
            //One combined raycast towards the velocity direction.
            if(previousVelocity.sqrMagnitude > 0.2) //shouldnt be tiny or 0
            {
                if(Physics.Raycast(transform.position, previousVelocity.normalized, out RaycastHit hit, halfHeight + 0.3f, gelMask))
                {
                    if(Input.GetButton(crouchButton) && hit.normal.y > 0.8)
                    {
                        return;
                    }
                    var angle = Vector3.Angle(-previousVelocity, hit.normal) * Mathf.Deg2Rad;

                    //the direction the player should now go in.
                    var newDir = Vector3.Reflect(previousVelocity.normalized, hit.normal);
                    previousVelocity = previousVelocity.magnitude * newDir;

                    previousVelXZPlane = new Vector3(previousVelocity.x, 0, previousVelocity.z);
                    yVel = previousVelocity.y;
                }
            }
            //Raycast down
            if(isGrounded)
                isOnBouncyGel = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, halfHeight+0.1f, gelMask);
            }

        IEnumerator Delay(System.Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action();
        }
    }
}