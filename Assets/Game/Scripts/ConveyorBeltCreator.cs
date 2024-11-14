using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using NUnit.Framework.Internal;

public class ConveyorBeltCreator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject belt;
    public GameObject scanner;
    private RectTransform rectTransform;
    private float rectWidth;
    private float beltWidth;
    private float beltScaleX;
    public List<GameObject> beltList;
    public GameObject scannerBelt;
    void Start()
    {
        beltWidth = belt.GetComponent<RectTransform>().rect.width;
        rectTransform = GetComponent<RectTransform>();
        rectWidth = rectTransform.rect.width;
        beltScaleX = belt.transform.localScale.x;
        createConveyorBelt();
    }

    private void createConveyorBelt()
    {
        foreach (GameObject b in beltList)
        {
            Destroy(b);
        }

        Debug.Log(rectWidth);

        float currentPosX = -100;

        scannerBelt = Instantiate(belt);
        scannerBelt.transform.SetParent(gameObject.transform, false);
        scannerBelt.transform.SetSiblingIndex(0);
        scannerBelt.GetComponent<RectTransform>().anchoredPosition = new Vector3(currentPosX, 88, 0);
        beltList.Add(scannerBelt);

        GameObject lastBelt = Instantiate(belt);
        lastBelt.transform.SetParent(gameObject.transform, false);
        lastBelt.transform.SetSiblingIndex(0);
        lastBelt.GetComponent<RectTransform>().anchoredPosition = new Vector3(currentPosX + beltWidth * beltScaleX, 88, 0);
        beltList.Add(lastBelt);

        while (currentPosX > -rectWidth + (beltWidth*beltScaleX/2))
        {
            currentPosX = currentPosX - beltWidth * beltScaleX;
            GameObject b = Instantiate(belt);
            b.transform.SetParent(gameObject.transform, false);
            b.transform.SetSiblingIndex(0);
            b.GetComponent<RectTransform>().anchoredPosition = new Vector3(currentPosX, 88, 0);
            beltList.Add(b);
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnGUI()
    {
        if (GUILayout.Button("Add Bin"))
        {

        }
        if (GUILayout.Button("Next Bin"))
        {

        }
        if (GUILayout.Button("Generate Conveyor Belt"))
        {
            rectWidth = rectTransform.rect.width;
            createConveyorBelt();
        }
    }
}
