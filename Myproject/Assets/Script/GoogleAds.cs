using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class GoogleAds : MonoBehaviour
{
    [SerializeField] private string _adResurrectionId = "ca-app-pub-3940256099942544/5224354917";
    [SerializeField] private string _adGameMenuId = "ca-app-pub-3940256099942544/6300978111";

    private RewardedAd _rewardedAd;
    private BannerView _bannerView;

    public void Initialize()
    {
        MobileAds.Initialize((InitializationStatus initStatus) => { });
    }

    public void ShowRewardedAd_Resurrection(Action onResultCallback)
    {
        LoadRewardedAd();

        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));

                onResultCallback?.Invoke();
                _rewardedAd.Destroy();
            });
        }
    }

    private void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(_adResurrectionId, adRequest,(RewardedAd ad, LoadAdError error) =>
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load an ad " + "with error : " + error);

                return;
            }

            Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
            _rewardedAd = ad;
        });
    }
}
