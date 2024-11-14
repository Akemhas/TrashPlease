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

        float currentPosX = -100 + beltWidth * beltScaleX;

        while (currentPosX > -rectWidth - (beltWidth*beltScaleX/2))
        {
            
            GameObject b = Instantiate(belt);
            b.transform.SetParent(gameObject.transform, false);
            b.transform.SetSiblingIndex(0);
            b.GetComponent<RectTransform>().anchoredPosition = new Vector3(currentPosX, 88, 0);
            beltList.Add(b);
            currentPosX = currentPosX - beltWidth * beltScaleX;
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
