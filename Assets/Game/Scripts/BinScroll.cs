using UnityEngine;
using PrimeTween;

public class BinScroll : MonoBehaviour
{
    public Tween Tween;

    private int _pos; //-1 = left, 0 = center, 1 = right

    private void OnEnable()
    {
        UIManager.Instance.SwipeButtonClicked += OnSwipeButtonClicked;
    }

    private void OnDisable()
    {
        UIManager.Instance.SwipeButtonClicked -= OnSwipeButtonClicked;
    }

    private void OnSwipeButtonClicked(int posIndex)
    {
        Swipe(posIndex);
    }

    private void Start() => CenterDot();

    public void SwipeDir(int dir)
    {
        Swipe(_pos + dir);
    }

    public void Swipe(int posIndex)
    {
        switch (posIndex)
        {
            case -1:
                LeftDot();
                return;
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

    private void LeftDot()
    {
        _pos = -1;
        UIManager.Instance.ChangeSwipeIndicatorVisual(_pos);
        if (Mathf.Approximately(transform.position.x, 6f)) return;
        Tween = Tween.LocalPositionX(transform, endValue: 6f, 1f, Ease.OutCubic);
    }

    private void CenterDot()
    {
        _pos = 0;
        UIManager.Instance.ChangeSwipeIndicatorVisual(_pos);
        if (Mathf.Approximately(transform.position.x, 0f)) return;
        Tween = Tween.LocalPositionX(transform, endValue: 0f, 1f, Ease.OutCubic);
    }

    private void RightDot()
    {
        _pos = 1;
        UIManager.Instance.ChangeSwipeIndicatorVisual(_pos);
        if (Mathf.Approximately(transform.position.x, -6f)) return;
        Tween = Tween.LocalPositionX(transform, endValue: -6f, 1f, Ease.OutCubic);
    }
}