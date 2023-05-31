using System.Collections;
using UnityEngine;

namespace VoxelDestruction
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController instance;

        public Transform head;
        bool shaking;

        public float smooth;
        bool inSmoothMode;
        Coroutine currentShake;

        private Vector3 offset;

        public GameObject weapons;
        public GameObject inventory;
        
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
        }

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            shaking = false;
            inSmoothMode = false;
        }

        private void LateUpdate()
        {
            if (!shaking && !inSmoothMode)
                transform.position = head.position;

            if (inSmoothMode && !shaking)
            {
                transform.position = Vector3.Lerp(transform.position, head.position, Time.deltaTime * smooth);
            }
        }

        public void StartSmooth ()
        {
            inSmoothMode = true;
        }

        public void StopSmooth()
        {
            inSmoothMode = false;
        }

        public void Shake (float duration, float magnitude)
        {
            if (currentShake != null)
                return;

            currentShake = StartCoroutine(_shake(duration, magnitude));
        }

        public IEnumerator _shake (float duration, float magnitude)
        {
            shaking = true;
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.position = head.position + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;

                yield return null;
            }

            transform.position = head.position;

            shaking = false;

            currentShake = null;
        }
    }   
}
