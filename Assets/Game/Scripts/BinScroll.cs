using System;
using UnityEngine;
using PrimeTween;

public class BinScroll : MonoBehaviour
{
    public Tween Tween;

    private int _pos; //-1 = left, 0 = center, 1 = right

    private void Awake()
    {
        UIManager.Instance.SwipeButtonClicked += OnSwipeButtonClicked;
    }

    private void OnSwipeButtonClicked(int posIndex)
    {
        Swipe(posIndex);
    }

    private void Start() => CenterDot();

    public void Swipe(int posIndex)
    {
        switch (posIndex)
        {
            case 0:
                CenterDot();
                return;
            case 1:
                RightDot();
                return;
        }

        UIManager.Instance.ChangeSwipeIndicatorVisual(_pos);
    }

    public void MoveBack()
    {
        Swipe(_pos);
    }

    private void CenterDot()
    {
        _pos = 0;
        Tween = Tween.LocalPositionX(transform, endValue: 0f, 1f, Ease.OutCubic);
        UIManager.Instance.ChangeSwipeIndicatorVisual(_pos);
    }

    private void RightDot()
    {
        _pos = 1;
        Tween = Tween.LocalPositionX(transform, endValue: -6f, 1f, Ease.OutCubic);
        UIManager.Instance.ChangeSwipeIndicatorVisual(_pos);
    }
}