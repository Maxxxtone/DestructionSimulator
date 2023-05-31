using UnityEngine;
using Random = UnityEngine.Random;

namespace VoxelDestruction
{
    public class Demo_Weapon : MonoBehaviour
    {
        [Header("Sway")] 
        
        public float posOffset;

        public float posSmooth;

        [Header("General")] 
        
        public Camera mainCamera;

        public Camera overlayCamera;

        public bool isHitable;
        public float fireCycle;

        public float hitStrength;

        [Space] 
        
        public float hitOffset;
        public float hitRadius;

        public LayerMask hitLayer;

        [Header("Throwing")] 
        
        public bool isThrowable;

        public GameObject throwingPrefab;

        public float throwingForce;
        public float throwingTorque;
        
        //Private
        private Animator ani;

        private float nextTimeToFire;

        private Vector3 defaultPos;
        private Vector3 currentVel;

        private bool exists;
        
        private void Start()
        {
            if (GetComponent<Animator>())
                ani = GetComponent<Animator>();

            nextTimeToFire = Time.time + fireCycle;

            defaultPos = transform.localPosition;
            exists = true;
        }

        private void Update()
        {
            if (!exists)
                return;
            
            Vector3 offset = Movement.instance.FindVelRelativeToLook() * posOffset;
            float fallspeed = Movement.instance.GetFallSpeed() * posOffset;
            
            Vector3 desiredPos;
            desiredPos = defaultPos - new Vector3(offset.x, fallspeed, offset.y);
            
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, desiredPos, ref currentVel, posSmooth);

            if (isHitable && Time.time > nextTimeToFire && Input.GetMouseButton(0))
            {
                nextTimeToFire = Time.time + fireCycle;

                Hit();
            }

            if (isThrowable && Input.GetMouseButtonDown(1))
            {
                Vector3 p1 = overlayCamera.WorldToViewportPoint(transform.position);
                Vector3 pos = mainCamera.ViewportToWorldPoint(p1);
             
                GameObject prefab = Instantiate(throwingPrefab, pos, transform.rotation);
                
                prefab.GetComponent<Rigidbody>().AddForce(mainCamera.transform.forward * throwingForce, ForceMode.VelocityChange);
                prefab.GetComponent<Rigidbody>().AddTorque(Random.insideUnitSphere * throwingTorque, ForceMode.VelocityChange);

                MeshRenderer[] r = GetComponentsInChildren<MeshRenderer>();
                foreach (var c in r)
                    c.enabled = false;

                exists = false;
                
                Invoke(nameof(Activate), 3f);
            }
        }

        private void Activate()
        {
            MeshRenderer[] r = GetComponentsInChildren<MeshRenderer>(true);
            foreach (var c in r)
                c.enabled = true;
            
            exists = true;
        }
        
        private void Hit()
        {
            ani.SetTrigger("Hit");

            Collider[] hits = Physics.OverlapSphere(mainCamera.transform.position + mainCamera.transform.forward * hitOffset, hitRadius, hitLayer);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.root.GetComponent<VoxelObject>())
                {
                    hits[i].transform.root.GetComponent<VoxelObject>().AddDestruction(hitStrength, 
                        mainCamera.transform.position + mainCamera.transform.forward * hitOffset, 
                        -mainCamera.transform.forward);
                } 
            }
        }
    }
}