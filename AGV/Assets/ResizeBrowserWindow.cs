using SimpleWebBrowser;
using UnityEngine;

public class ResizeBrowserWindow : MonoBehaviour
{
    public WebBrowser2D MainBrowser;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MainBrowser = GameObject.Find("Browser2D").GetComponent<WebBrowser2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**
    bool resizedOnce = false;
    private void OnRectTransformDimensionsChange()
    {
        if (resizedOnce)
        {
            Debug.Log($"Window dimensions changed to {Screen.width}x{Screen.height}");
            MainBrowser.RunJavaScript("height = '" + 1000 + "'px;");
            MainBrowser.RunJavaScript("width = 1000px;");
            MainBrowser.RunJavaScript("reload();");
        }
        resizedOnce = true;
    }
    **/
}
