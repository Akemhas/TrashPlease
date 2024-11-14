using UnityEngine;
using UnityEngine.UI;

public class animateImage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Image img;
    SpriteRenderer spriteRenderer;
    void Start()
    {
        img = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        img.sprite = spriteRenderer.sprite;
    }
}
