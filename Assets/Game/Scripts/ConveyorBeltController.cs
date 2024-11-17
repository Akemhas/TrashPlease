using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem.XR;

public class ConveyorBeltController : Singleton<ConveyorBeltController>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject belt;
    [SerializeField] private float startPosX;
    public BinController binController;
    private RectTransform rectTransform;
    private float rectWidth;
    private float beltWidth;
    private float beltScaleX;
    public List<GameObject> beltList;
    private static Array trashTypes = Enum.GetValues(typeof(TrashSortType));
    public List<TrashSortType> binQueueList;
    public Queue<TrashSortType> binQueue = new Queue<TrashSortType>();
    public List<TrashSortType> activeBinQueueList;
    public Queue<TrashSortType> activeBinQueue = new Queue<TrashSortType>();
    void Start()
    {
        beltWidth = belt.GetComponent<RectTransform>().rect.width;
        rectTransform = GetComponent<RectTransform>();
        rectWidth = rectTransform.rect.width;
        beltScaleX = belt.transform.localScale.x;
        createConveyorBelt();
    }

    public void CreatingBin()
    {
        binController.StartCreatingBin(activeBinQueue.Dequeue());
        //activeBinQueue.Dequeue();
        activeBinQueueList = activeBinQueue.ToList();
    }

    public TrashSortType getNextTrashType()
    {
        TrashSortType nextType = binQueue.Dequeue();
        binQueueList = binQueue.ToList();
        activeBinQueue.Enqueue(nextType);
        activeBinQueueList = activeBinQueue.ToList();
        return nextType;
    }

    public float getBeltWidth()
    {
        return beltWidth * beltScaleX;
    }
    private void createConveyorBelt()
    {
        foreach (GameObject b in beltList)
        {
            Destroy(b);
        }
        beltList.Clear();

        float currentPosX = startPosX;// + beltWidth * beltScaleX;

        while (currentPosX > -rectWidth - (beltWidth*beltScaleX/2))
        {
            GameObject b = Instantiate(belt);
            b.transform.SetParent(gameObject.transform, false);
            b.transform.SetSiblingIndex(0);
            b.GetComponent<RectTransform>().anchoredPosition = new Vector3(currentPosX, 88, 0);
            if (beltList.Count > 0)
            {
                beltList.Last().GetComponent<ConveyorBelt>().previousBelt = b;
                b.GetComponent<ConveyorBelt>().nextBelt = beltList.Last();
                b.GetComponent<ConveyorBelt>().controller = this;
            }
            beltList.Add(b);
            currentPosX = currentPosX - beltWidth * beltScaleX;
        }
    }


    private void OnGUI()
    {
        float resX = (float)(Screen.width) / 400.0f;
        float resY = (float)(Screen.height) / 800.0f;
        //Set matrix
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(resX, resY, 1));
        if (GUILayout.Button("Add Bin"))
        {
            binQueue.Enqueue((TrashSortType)trashTypes.GetValue(UnityEngine.Random.Range(0,trashTypes.Length)));
            binQueueList = binQueue.ToList();
            beltList.Last().GetComponent<ConveyorBelt>().RequestBin();
        }
        if (GUILayout.Button("Next Bin"))
        {
            Debug.Log("NEXT BIN");
            ConveyorBelt b = beltList.First().GetComponent<ConveyorBelt>();
            Destroy(b.currentBin);
            b.currentBin = null;
        }
        if (GUILayout.Button("Generate Conveyor Belt"))
        {
            rectWidth = rectTransform.rect.width;
            createConveyorBelt();
        }
    }
}
