using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using TMPro;

public class UiControl : MonoBehaviour
{
    // ÂüÁ¶ : https://m.blog.naver.com/dooya-log/221320177107

    public void Initialize()
    {
        this.gameObject.SetActive(true);
    }

    public void Move(RectTransform tr, Vector3 targetPosition, float duration, float delay = 0, Ease ease = Ease.Linear, Action onResultCallback = null)
    {
        tr.DOMove(targetPosition, duration).SetEase((DG.Tweening.Ease)ease).SetDelay(delay).OnComplete(() => 
        {
            onResultCallback?.Invoke();
        });
    }

    public void Move_X(RectTransform tr, float targetPosition_X, float duration, float delay = 0, Ease ease = Ease.Linear, Action onResultCallback = null)
    {
        tr.DOMoveX(targetPosition_X, duration).SetEase((DG.Tweening.Ease)ease).SetDelay(delay).OnComplete(() =>
        {
            onResultCallback?.Invoke();
        });
    }

    public void Move_Y(RectTransform tr, float targetPosition_Y, float duration, float delay = 0, Ease ease = Ease.Linear, Action onResultCallback = null)
    {
        tr.DOMoveY(targetPosition_Y, duration).SetEase((DG.Tweening.Ease)ease).SetDelay(delay).OnComplete(() =>
        {
            onResultCallback?.Invoke();
        });
    }

    public void Move_Local(RectTransform tr, Vector3 targetPosition, float duration, float delay = 0, Ease ease = Ease.Linear, Action onResultCallback = null)
    {
        tr.DOLocalMove(targetPosition, duration).SetEase((DG.Tweening.Ease)ease).SetDelay(delay).OnComplete(() =>
        {
            onResultCallback?.Invoke();
        });
    }

    public void Move_Local_X(RectTransform tr, float targetPosition_X, float duration, float delay = 0, Ease ease = Ease.Linear, Action onResultCallback = null)
    {
        tr.DOLocalMoveX(targetPosition_X, duration).SetEase((DG.Tweening.Ease)ease).SetDelay(delay).OnComplete(() =>
        {
            onResultCallback?.Invoke();
        });
    }

    public void Move_Local_Y(RectTransform tr, float targetPosition_Y, float duration, float delay = 0, Ease ease = Ease.Linear, Action onResultCallback = null)
    {
        tr.DOLocalMoveY(targetPosition_Y, duration).SetEase((DG.Tweening.Ease)ease).SetDelay(delay).OnComplete(() =>
        {
            onResultCallback?.Invoke();
        });
    }

    public void Move_Anchor_X(RectTransform tr, float targetPositon_X, float duration, float delay = 0, Ease ease = Ease.Linear, Action onResultCallback = null)
    {
        tr.DOAnchorPosX(targetPositon_X, duration).SetEase((DG.Tweening.Ease)ease).SetDelay(delay).OnComplete(() =>
        {
            onResultCallback?.Invoke();
        });
    }

    public void Move_Anchor_Y(RectTransform tr, float targetPositon_Y, float duration, float delay = 0, Ease ease = Ease.Linear, Action onResultCallback = null)
    {
        tr.DOAnchorPosY(targetPositon_Y, duration).SetEase((DG.Tweening.Ease)ease).SetDelay(delay).OnComplete(() =>
        {
            onResultCallback?.Invoke();
        });
    }
}
