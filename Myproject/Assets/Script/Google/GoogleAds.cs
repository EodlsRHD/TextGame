using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class GoogleAds : MonoBehaviour
{
    [SerializeField] private string _adResurrectionId = "ca-app-pub-3940256099942544/5224354917";
    [SerializeField] private string _adGameMenuId = "ca-app-pub-3940256099942544/6300978111";
    [SerializeField] private string _adPopupId = "ca-app-pub-3940256099942544/6300978111";

    private RewardedAd _rewardedAd = null;
    private BannerView _gameMenuBannerView = null;
    private BannerView _popupBannerView = null;

    public void Initialize()
    {
        MobileAds.Initialize((InitializationStatus initStatus) => 
        {
            CreateGameMenuBannerView();
            CreatePopupBannerView();
        });
    }

    #region reward Resurrection

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

    #endregion

    #region Benner Gamemenu

    private void CreateGameMenuBannerView()
    {
        if (_gameMenuBannerView != null)
        {
            DestroyGameMenuAd();
        }

        _gameMenuBannerView = new BannerView(_adGameMenuId, AdSize.MediumRectangle, AdPosition.Top);

        _gameMenuBannerView.LoadAd(new AdRequest());
        _gameMenuBannerView.Hide();
    }

    public void ShowGameMenuAd()
    {
        if (_gameMenuBannerView == null)
        {
            CreateGameMenuBannerView();

            return;
        }

        _gameMenuBannerView.Show();
    }

    public void HideGameMenuAd()
    {
        if(_gameMenuBannerView == null)
        {
            CreateGameMenuBannerView();

            return;
        }

        _gameMenuBannerView.Hide();
    }

    private void DestroyGameMenuAd()
    {
        if (_gameMenuBannerView != null)
        {
            _gameMenuBannerView.Destroy();
            _gameMenuBannerView = null;
        }
    }

    #endregion

    #region Benner Gamemenu

    private void CreatePopupBannerView()
    {
        if (_popupBannerView != null)
        {
            DestroyPopupAd();
        }

        AdSize adaptiveSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        _popupBannerView = new BannerView(_adPopupId, adaptiveSize, AdPosition.Bottom);

        _popupBannerView.LoadAd(new AdRequest());
        _popupBannerView.Hide();
    }

    public void ShowPopupAd()
    {
        if (_popupBannerView == null)
        {
            CreatePopupBannerView();

            return;
        }

        _popupBannerView.Show();
    }

    public void HidePopupAd()
    {
        if (_popupBannerView == null)
        {
            CreatePopupBannerView();

            return;
        }

        _popupBannerView.Hide();
    }

    private void DestroyPopupAd()
    {
        if (_popupBannerView != null)
        {
            _popupBannerView.Destroy();
            _popupBannerView = null;
        }
    }

    #endregion
}
