using UnityEngine;

public class ChangeAnimation : MonoBehaviour
{
    [SerializeField] private Vector3 _initScale;
    [SerializeField] private Vector3 _finalScale = new Vector3(200,200,200);
    private void OnEnable()
    {
        transform.localScale = _initScale;  
    }
    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _finalScale, .03f);
    }
    private void OnDisable()
    {
        transform.localScale = _initScale;
    }
}
