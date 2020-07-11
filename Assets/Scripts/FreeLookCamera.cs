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
        [SerializeField]
        protected bool useUnscaledTime = true;

        private void Start() 
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            //get input
            float rotX = Input.GetAxis(rotationXAxis);
            float rotY = Input.GetAxis(rotationYAxis);

            float zMove = Input.GetAxis(forwardAxis);
            float xMove = Input.GetAxis(sideAxis);

            float deltaTime = useUnscaledTime? Time.unscaledDeltaTime : Time.deltaTime;

            //Rotate the camera.
            var curSpeed = rotationSpeed * deltaTime;
            transform.Rotate(new Vector3(rotX * curSpeed, 0), Space.Self);
            transform.Rotate(new Vector3(0, rotY * curSpeed), Space.World);

            //Move
            var dir = transform.right * xMove + transform.forward * zMove;
            dir = Vector3.ClampMagnitude(dir, 1);
            var delta = dir * speed * deltaTime;
            transform.position += delta;
        }
    }
}