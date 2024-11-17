using PrimeTween;
using System.ComponentModel;
using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using System.Linq;
using System.Xml.Serialization;

public class ConveyorBelt : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Image img;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    public GameObject previousBelt;
    public GameObject nextBelt;
    public bool animate;
    public GameObject currentBin;
    public ConveyorBeltController controller;
    public bool locked = false;
    void Start()
    {
        img = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    private string BinAddress(TrashSortType sortType) => "BinSide_" + sortType;
    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        RequestBin();
    }
    public void RequestBin()
    {
        if (locked)
        {
            return;
        }

        if (previousBelt != null)
        {
            previousBelt.GetComponent<ConveyorBelt>().TryPassingBin();
            return;
        }
        //queue is empty
        if (controller.binQueue.Count == 0)
        {
            return;
        }

        if (currentBin != null)
        {
            Debug.Log("belt already has a bin!");
            return;
        }
        locked = true;

        Addressables.InstantiateAsync(BinAddress(controller.getNextTrashType())).Completed += LoadComplete;
    }

    public void moveBins()
    {
        
    }


    private void LoadComplete(AsyncOperationHandle<GameObject> handle)
    {
        animate = true;
        currentBin = handle.Result;
        currentBin.transform.SetParent(transform.parent.transform, false);
        currentBin.transform.SetSiblingIndex(controller.beltList.Count);
        currentBin.GetComponent<RectTransform>().anchoredPosition = new Vector3(GetComponent<RectTransform>().anchoredPosition.x - controller.getBeltWidth(), 175, 0);
        Tween.UIAnchoredPositionX(currentBin.GetComponent<RectTransform>(), GetComponent<RectTransform>().anchoredPosition.x, 1f, Ease.Linear)
            .OnComplete(() =>
            {
                TryPassingBin();
            });
    }

    private void TryPassingBin()
    {
        animate = false;
        if (currentBin == null)
        {
            return;
        }
        
        //if its last belt cant pass
        if (nextBelt == null)
        {
            return;
        }
        if (nextBelt.GetComponent<ConveyorBelt>().currentBin == null)
        {
            animate = true;
            nextBelt.GetComponent<ConveyorBelt>().animate = true;
            Tween.UIAnchoredPositionX(currentBin.GetComponent<RectTransform>(), GetComponent<RectTransform>().anchoredPosition.x + controller.getBeltWidth() / 2, 0.5f, Ease.Linear)
            .OnComplete(() =>
            {
                nextBelt.GetComponent<ConveyorBelt>().currentBin = currentBin;
                currentBin = null;
                animate = false;
                locked = false;
                nextBelt.GetComponent<ConveyorBelt>().AcceptBin();
                if(previousBelt == null)
                {
                    Invoke("RequestBin", 0.05f);
                }
            });
            //entering scanner
            if (nextBelt.GetComponent<ConveyorBelt>().nextBelt == null)
            {
                controller.CreatingBin();
            }
        }
        else
        {
            Invoke("TryPassingBin", 0.3f);
        }
    }

    private void AcceptBin()
    {
        animate = true;
        if (currentBin == null)
        {
            return; 
        }
        Tween.UIAnchoredPositionX(currentBin.GetComponent<RectTransform>(), GetComponent<RectTransform>().anchoredPosition.x, 0.5f, Ease.Linear)
        .OnComplete(() =>
        {
            animate = false;
            TryPassingBin();
        });
    }
        

    // Update is called once per frame
    void Update()
    {
        
        img.sprite = spriteRenderer.sprite;
        if (animate)
        {
            animator.speed = 1;
        }
        else
        {
            animator.speed = 0;
        }
    }
}
