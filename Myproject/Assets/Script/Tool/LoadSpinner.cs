using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoadSpinner : MonoBehaviour
{
    [SerializeField] private Image _loadingBar = null;
    [SerializeField] private Transform _loadingBarTransform = null;

    private float LoadingDuration = 3f;
    private float rotateDuration = 1.5f;

    public void Initialize()
    {
        RotateLoading();

        StopLoading();
    }

    public void StartLoading()
    {
        Debug.LogError("StartLoading");

        _loadingBar.DOPlay();
        _loadingBarTransform.DOPlay();
    }

    public void StopLoading()
    {
        Debug.LogError("StopLoading");

        _loadingBar.DOPause();
        _loadingBarTransform.DOPause();
    }

    private void RotateLoading()
    {
        _loadingBar.fillAmount = 0f;
        _loadingBar.DOFillAmount(1f, LoadingDuration).SetEase(DG.Tweening.Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        _loadingBarTransform.DORotate(new Vector3(0f, 0f, -360f), rotateDuration, RotateMode.FastBeyond360).SetEase(DG.Tweening.Ease.Linear).SetLoops(-1);
    }
}
