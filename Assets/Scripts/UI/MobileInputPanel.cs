using UnityEngine;

public class MobileInputPanel : MonoBehaviour
{
    private void Start()
    {
        if (!Application.isMobilePlatform)
            gameObject.SetActive(false);
    }
}
