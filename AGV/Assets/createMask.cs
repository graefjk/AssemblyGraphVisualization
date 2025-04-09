using SimpleWebBrowser;
using UnityEngine;
using UnityEngine.UI;

public class createMask : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    RawImage image;
    void Start()
    {
        image = GameObject.Find("Browser2D").GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        //Sprite.Create(image.texture., new Rect(0,0,1000,1000), Vector2.zero);
    }
}
