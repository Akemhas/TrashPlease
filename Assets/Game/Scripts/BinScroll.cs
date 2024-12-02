using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using static UnityEditor.PlayerSettings;

public class BinScroll : MonoBehaviour
{
    public Image left;
    public Image center;
    public Image right;
    public Tween tween;


    private int pos; //-1 = left, 0 = center, 1 = right
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        centerDot();
    }

    public void swipeLeft()
    {
        if (pos == 0)
        {
            leftDot();
            return;
        }
        if (pos == 1)
        {
            centerDot();
            return;
        }
        moveBack();
    }

    public void swipeRight()
    {
        if (pos == -1)
        {
            centerDot();
            return;
        }
        if (pos == 0)
        {
            rightDot();
            return;
        }
        moveBack();
    }
    public void moveBack()
    {
        if (pos == -1)
        {
            leftDot();
            return;
        }
        if (pos == 0)
        {
            centerDot();
            return;
        }
        if(pos == 1)
        {
            rightDot();
            return;
        }
    }

    public void leftDot()
    {
        pos = -1;
        left.color = Color.white;
        center.color = Color.gray;
        right.color = Color.gray;
        tween = Tween.LocalPositionX(transform, endValue: 6f, 1f, Ease.OutCubic);
    }

    public void centerDot()
    {
        pos = 0;
        left.color = Color.gray;
        center.color = Color.white;
        right.color = Color.gray;
        tween = Tween.LocalPositionX(transform, endValue: 0f, 1f, Ease.OutCubic);
    }

    public void rightDot()
    {
        pos = 1;
        left.color = Color.gray;
        center.color = Color.gray;
        right.color = Color.white;
        tween = Tween.LocalPositionX(transform, endValue: -6f, 1f, Ease.OutCubic);
    }
}
