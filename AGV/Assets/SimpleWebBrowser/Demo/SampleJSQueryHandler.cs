using UnityEngine;
using SimpleWebBrowser;
using MessageLibrary;

public class SampleJSQueryHandler : MonoBehaviour
{

    public WebBrowser2D MainBrowser;

    void Start()
    {
        MainBrowser = GameObject.Find("Browser2D").GetComponent<WebBrowser2D>();
        MainBrowser.OnShowDialog += MainBrowser_OnJSQuery;
    }

    private void MainBrowser_OnJSQuery(string message, string prompt, DialogEventType type)
    {
        Debug.Log("Javascript query:" + message);
        MainBrowser.RespondToJSQuery("My response: OK");
        MainBrowser.RunJavaScript("document.getElementById('test').innerHTML = 'LOL'");
        MainBrowser.RunJavaScript("document.getElementsByTagName('body')[0].style.height = '100px'");
    }
}
