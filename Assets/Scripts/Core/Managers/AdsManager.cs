using System;
using UnityCommunity.UnitySingleton;
using UnityEngine;
#if UNITY_ANDROID
using GoogleMobileAds.Api;
#endif

public class AdsManager : PersistentMonoSingleton<AdsManager>
{
#if UNITY_ANDROID
    private const string RewardedAdUnitId = "ca-app-pub-4717936789267161/3672102644";

    private RewardedAd rewardedAd;
    private bool isInitialized = false;

    public override void InitializeSingleton()
    {
        base.InitializeSingleton();
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize(status =>
        {
            isInitialized = true;
            Debug.Log("[AdsManager] AdMob 초기화 완료");
            LoadRewardedAd();
        });
    }

    public void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        RewardedAd.Load(RewardedAdUnitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogWarning($"[AdsManager] 리워드 광고 로드 실패: {error}");
                return;
            }
            rewardedAd = ad;
            Debug.Log("[AdsManager] 리워드 광고 로드 완료");
        });
    }

    public bool IsRewardedAdReady => rewardedAd != null;

    public void ShowRewardedAd(Action onRewarded, Action onFailed = null)
    {
        if (!IsRewardedAdReady)
        {
            Debug.LogWarning("[AdsManager] 광고가 준비되지 않았습니다.");
            onFailed?.Invoke();
            return;
        }

        bool rewarded = false;
        var ad = rewardedAd;

        void OnClosed()
        {
            ad.OnAdFullScreenContentClosed -= OnClosed;
            ad.OnAdFullScreenContentFailed -= OnFailed;
            if (rewarded) onRewarded?.Invoke();
            LoadRewardedAd();
        }

        void OnFailed(AdError error)
        {
            ad.OnAdFullScreenContentClosed -= OnClosed;
            ad.OnAdFullScreenContentFailed -= OnFailed;
            Debug.LogWarning($"[AdsManager] 광고 표시 실패: {error}");
            onFailed?.Invoke();
            LoadRewardedAd();
        }

        ad.OnAdFullScreenContentClosed += OnClosed;
        ad.OnAdFullScreenContentFailed += OnFailed;

        ad.Show(reward =>
        {
            Debug.Log("[AdsManager] 리워드 지급 완료");
            rewarded = true;
        });
    }
#else
    public bool IsRewardedAdReady => false;

    public void ShowRewardedAd(Action onRewarded, Action onFailed = null)
    {
        Debug.Log("[AdsManager] 광고 미지원 플랫폼");
        onFailed?.Invoke();
    }
#endif
}
