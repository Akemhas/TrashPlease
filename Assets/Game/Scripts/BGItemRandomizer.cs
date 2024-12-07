using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BGItemRandomizer : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private RectTransform _parent;
    [SerializeField] private List<Graphic> _randomImages;

    [Button]
    private void GetGraphics()
    {
        _randomImages.Clear();
        var children = _parent.GetComponentsInChildren<Graphic>();
        foreach (var child in children)
        {
            if (child.rectTransform == _parent) continue;
            _randomImages.Add(child);
        }
    }
    [Button]
    private void EvenlyDistributePositions()
    {
        Undo.RecordObject(this, "Evenly Distribute Positions");

        int totalItems = _randomImages.Count;
        if (totalItems == 0)
            return;

        // Calculate the number of rows and columns based on the number of items
        int columns = Mathf.CeilToInt(Mathf.Sqrt(totalItems));
        int rows = Mathf.CeilToInt((float)totalItems / columns);

        // Calculate cell size based on parent's rect
        float cellWidth = _parent.rect.width / columns;
        float cellHeight = _parent.rect.height / rows;

        for (int i = 0; i < totalItems; i++)
        {
            var randomImage = _randomImages[i];
            var rt = randomImage.transform as RectTransform;

            // Calculate row and column position for the current item
            int row = i / columns;
            int column = i % columns;

            // Calculate the anchored position within the parent's rect
            float xPos = -_parent.rect.width / 2 + (column + 0.5f) * cellWidth;
            float yPos = _parent.rect.height / 2 - (row + 0.5f) * cellHeight;

            rt.anchoredPosition = new Vector2(xPos, yPos);
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    [Button]
    private void RandomizePositions()
    {
        Undo.RecordObject(this, "Randomize Position");
        foreach (var randomImage in _randomImages)
        {
            var rt = randomImage.transform as RectTransform;
            rt.anchoredPosition = new Vector2(Random.Range(-_parent.rect.width / 2, _parent.rect.width / 2), Random.Range(-_parent.rect.height / 2, _parent.rect.height / 2));
        }
    }

    [Button]
    private void EvenlyRandomizePositions(int divisions)
    {
        Undo.RecordObject(this, "Evenly Randomize Position");

        float cellWidth = _parent.rect.width / divisions;
        float cellHeight = _parent.rect.height / divisions;

        List<Vector2> availableCells = new List<Vector2>();
        for (int x = 0; x < divisions; x++)
        {
            for (int y = 0; y < divisions; y++)
            {
                availableCells.Add(new Vector2(x * cellWidth, y * cellHeight));
            }
        }

        foreach (var randomImage in _randomImages)
        {
            if (availableCells.Count == 0)
            {
                Debug.LogWarning("Not enough cells for all randomImages to be placed.");
                break;
            }

            // Choose a random cell from the available cells
            int randomIndex = Random.Range(0, availableCells.Count);
            Vector2 cellCenter = availableCells[randomIndex];
            availableCells.RemoveAt(randomIndex);

            // Randomize the position slightly within the cell
            float xOffset = Random.Range(-cellWidth / 2, cellWidth / 2);
            float yOffset = Random.Range(-cellHeight / 2, cellHeight / 2);
            Vector2 randomPosition = new Vector2(
                cellCenter.x - _parent.rect.width / 2 + xOffset,
                cellCenter.y - _parent.rect.height / 2 + yOffset
            );

            var rt = randomImage.transform as RectTransform;
            rt.anchoredPosition = randomPosition;
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    [Button]
    private void RandomizeColors()
    {
        Undo.RecordObject(this, "Randomize Colors");
        foreach (var randomImage in _randomImages)
        {
            randomImage.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    [Button]
    private void RandomizeRotations()
    {
        Undo.RecordObject(this, "Randomize Rotations");
        foreach (var randomImage in _randomImages)
        {
            randomImage.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
#endif
}