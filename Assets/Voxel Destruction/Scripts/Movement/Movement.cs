using UnityEngine;
using UnityEngine.SceneManagement;

namespace VoxelDestruction
{
    public class Movement : MonoBehaviour
    {
        public static Movement instance;

        [Header("Important Objects")]
        
        Camera mainCam;

        Rigidbody rb;

        Transform look;
        Transform head;
        Transform groundcheck;

        public Transform cam;

        [Header("Stats")]

        public float speed = 100f;
        public float jumpforce = 50f;
        public float airDrag;
        float normalDrag;

        [Space]

        public float mousesensitivity = 10f;

        [Header("Ground")]

        public bool grounded;
        public LayerMask groundlayer;
        public float groundcheckrange = 3f;
        public float groundheckradius;
        Vector3 groundNormal;

        private bool jump;
        private float vertical;
        private float horizontal;
        
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
        }
        
        private void Start()
        {
            Time.timeScale = 1;
            
            rb = GetComponent<Rigidbody>();

            look = transform.GetChild(0);
            head = transform.GetChild(1);
            groundcheck = transform.GetChild(2);

            groundNormal = Vector3.zero;
            normalDrag = rb.drag;

            readytojump = true;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            mainCam = cam.GetComponent<Camera>();

            colls = new Collider[20];
        }

        private void Update()
        {
            jump = Input.GetKey(KeyCode.Space);
            vertical = Input.GetAxisRaw("Vertical");
            horizontal = Input.GetAxisRaw("Horizontal");

            //if (Input.GetKeyDown(KeyCode.Escape))
            //    SceneManager.LoadScene(0);

            GroundCheck();
            Look();
        }

        private Collider[] colls;
        void GroundCheck()
        {
            float hits = Physics.OverlapSphereNonAlloc(groundcheck.position, groundheckradius, colls, groundlayer, QueryTriggerInteraction.Ignore);

            if (hits > 0)
            {
                CancelInvoke(nameof(ResetGround));
                grounded = true;
                groundNormal = Physics.Raycast(groundcheck.position, -groundcheck.transform.up, out RaycastHit hit, groundcheckrange, groundlayer) ? hit.normal : transform.up;
                rb.drag = normalDrag;
            }
            else if (grounded && !IsInvoking(nameof(ResetGround)))
            {
                Invoke(nameof(ResetGround), 0.2f);
            }
        }

        private void ResetGround()
        {
            grounded = false;
            groundNormal = Vector3.zero;
            rb.drag = airDrag;
        }

        float xRotation = 0f;
        float desiredX;

        void Look()
        {
            float mouseX = Input.GetAxis("Mouse X") * mousesensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mousesensitivity;

            desiredX = cam.localRotation.eulerAngles.y + mouseX;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90, 90);

            cam.localRotation = Quaternion.Euler(xRotation, desiredX, cam.localRotation.eulerAngles.z);
            look.localRotation = Quaternion.Euler(0, desiredX, 0f);
        }

        private void FixedUpdate()
        {
            if (readytojump && jump && grounded)
                 Jump();

            if (horizontal == 0 && vertical == 0)
                return;

            float multi;

            if (grounded)
            {
                multi = 1f;
            }
            else
            {
                multi = 0.9f;
            }
            
            if (groundNormal != Vector3.zero)
            {
                rb.AddForce(Vector3.Cross(look.right, groundNormal) * (vertical * speed * Time.fixedDeltaTime * multi), ForceMode.Impulse);
                rb.AddForce(Vector3.Cross(look.forward, groundNormal) * (-horizontal * speed * Time.fixedDeltaTime * multi), ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(look.forward * (vertical * speed * Time.fixedDeltaTime * multi), ForceMode.Impulse);
                rb.AddForce(look.right * (horizontal * speed * Time.fixedDeltaTime * multi), ForceMode.Impulse);
            }
        }

        private bool IsWall(Vector3 v, Vector3 dir)
        {
            float angle = Vector3.Angle(dir, v);
            return angle < 35;
        }

        private bool readytojump;
        void Jump()
        {
            if (rb.velocity.y < 0)
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            
            if (groundNormal != Vector3.zero)
            {
                rb.AddForce(transform.up * jumpforce / 2 * Time.fixedDeltaTime, ForceMode.Impulse);
                rb.AddForce(groundNormal * jumpforce / 2 * Time.fixedDeltaTime, ForceMode.Impulse);
            }
            else
                rb.AddForce(transform.up * (jumpforce * Time.fixedDeltaTime), ForceMode.Impulse);

            readytojump = false;
            grounded = false;
            groundNormal = Vector3.zero;
            Invoke(nameof(ResetJump), 0.05f);
        }

        void ResetJump()
        {
            readytojump = true;
        }

        public float GetFallSpeed()
        {
            return rb.velocity.y;
        }
        
        public Vector2 FindVelRelativeToLook() {
            float lookAngle = look.transform.eulerAngles.y;
            float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

            float u = Mathf.DeltaAngle(lookAngle, moveAngle);
            float v = 90 - u;

            float magnitue = rb.velocity.magnitude;
            float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
            float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
            
            return new Vector2(xMag, yMag);
        }
    }   
}
