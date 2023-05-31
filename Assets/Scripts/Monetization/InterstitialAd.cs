using InstantGamesBridge;
using InstantGamesBridge.Modules.Advertisement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterstitialAd : MonoBehaviour
{
    //[SerializeField] private SoundState _soundState;
    [SerializeField] private bool _showOnStart;
    private void Start()
    {
        if (_showOnStart)
            Show();
    }
    public void Show()
    {
        Debug.Log("In Ad()");
        bool isSuccess = false;

        /* -- -- -- Interstitial -- -- -- */
        // Необязательный параметр, игнорировать ли минимальную задержку
        bool ignoreDelay = false; // По умолчанию = false

        // Одинаково для всех платформ
        Bridge.advertisement.ShowInterstitial(
            ignoreDelay,
            success =>
            {
                if (success)
                {
                    // Success
                    Debug.Log("Bridge.advertisement.ShowInterstitial success: " + success);
                    isSuccess = true;
                }
                else
                {
                    // Error
                    Debug.Log("Bridge.advertisement.ShowInterstitial success: " + success);
                }
            });
        Bridge.advertisement.interstitialStateChanged += state => {
            if (state == InterstitialState.Opened)
            {
                //AudioListener.volume = 0.0f;
               // _soundState.SoundDisabled = true;
            }
            else
            {
                //AudioListener.volume = 1.0f;
                //_soundState.SoundDisabled = false;
            }
        };
    }
}
