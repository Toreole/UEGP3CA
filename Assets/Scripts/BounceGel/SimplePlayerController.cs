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
                    var inDir = previousVelocity.normalized;
                        Debug.DrawRay(hit.point - inDir, inDir, Color.cyan, 5);
                    movement = Vector3.Reflect(previousVelocity, hit.normal);
                        Debug.DrawRay(hit.point, hit.normal, Color.green, 5);
                        Debug.DrawRay(hit.point, movement.normalized, Color.magenta, 5);
                    //set caches.
                    SetCacheVars();
                }
                else 
                {
                    isGrounded = true;
                    movement += currentMovement;
                    previousXZVelocity = movement;
                    movement.y = yVel;

                    //jumping
                    if(Input.GetButtonDown(jumpInput))
                    {
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
                    //reset the yVel on the first frame?
                    yVel = Mathf.Clamp(yVel, 0, 100);
                }
                isGrounded = false;
                yVel += Physics.gravity.y * Time.deltaTime;
                movement = Vector3.RotateTowards(previousXZVelocity, currentMovement, Mathf.PI * airControl * Time.deltaTime, 0.01f);
                previousXZVelocity = movement;
                movement.y = yVel;
                //raycast into the moving direction, if there is gel, reflect / bounce off it.
                if(previousXZVelocity.sqrMagnitude > 0.25f)
                {
                    if(Physics.Raycast(transform.position, previousXZVelocity.normalized, out RaycastHit gelHit, halfHeight,  gelMask))
                    {
                        Debug.DrawRay(gelHit.point, gelHit.normal, Color.red, 5);
                        movement = Vector3.Reflect(movement, gelHit.normal);
                        //set caches.
                        SetCacheVars();
                    }
                }
            }
            
            previousVelocity = movement;
            controller.Move(movement * Time.deltaTime);

            void SetCacheVars()
            {
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

        IEnumerator Delay(System.Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action();
        }
    }
}