using InstantGamesBridge;
using System.Collections;
using System.Collections.Generic;
using InstantGamesBridge.Modules.Advertisement;
using UnityEngine;

public class RewardAd : MonoBehaviour
{
    public System.Action RewardGet;
    public System.Action RewardClose;
    //[SerializeField] private SoundState _soundState;
    public void Reward()
    {
        Debug.Log("In Reward()");

        bool isSuccess = false;
        Bridge.advertisement.ShowRewarded(success =>
        {
            if (success)
            {
                // Success
                Debug.Log("Bridge.advertisement.ShowRewarded success: " + success);
                isSuccess = true;
            }
            else
            {
                // Error
                Debug.Log("Bridge.advertisement.ShowRewarded success: " + success);
            }
        });

        Bridge.advertisement.interstitialStateChanged += state => { Debug.Log($"Interstitial state: {state}"); };
        Bridge.advertisement.rewardedStateChanged += state => {
            if (state == RewardedState.Rewarded)
            {
                //AudioListener.volume = 1;
               // _soundState.SoundDisabled = false;
                RewardGet?.Invoke();
            }
            else if (state == RewardedState.Opened)
            {
                //AudioListener.volume = 0;
                //_soundState.SoundDisabled = true;
            }
            else
            {
               // _soundState.SoundDisabled = false;
            }
            Debug.Log($"Rewarded state: {state}");
        };

        //return isSuccess;
    }
}
