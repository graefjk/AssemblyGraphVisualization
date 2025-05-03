using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AGV;
using SimpleWebBrowser;
using UnityEngine;
using UnityEngine.UI;
using static DottetLine;

public class DottetLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public MeshRenderer meshRenderer;
    WebBrowser2D MainBrowser;
    RawImage ui;
    Outline outline;
    AGVManager importer;

    [System.Serializable]
    public struct LineData
    {
        public string partName;
        public float start;
        public float end;
        public string axis;
        public bool reverse;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public string id;
        public List<string> assembledParts;
    }
    public LineData lineData = new LineData();
    public bool selected = false;
    public bool assembled = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        MainBrowser = GameObject.Find("Browser2D").GetComponent<WebBrowser2D>();
        ui = GameObject.Find("Browser2D").GetComponent<RawImage>();
        outline = GetComponent<Outline>();
        importer = GameObject.Find("AGVManager").GetComponent<AGVManager>();
        if (lineData.partName == null)
        {
            lineData.partName = gameObject.name;
            lineData.start = 10f;
            lineData.end = 10f;
            lineData.axis = "none";
            lineData.id = Guid.NewGuid() + "";
        }
    }

    public void setLine(string axis, bool reverse, float start, float end)
    {
        lineData.start = start;
        lineData.end = end;
        lineData.axis = axis;
        lineData.reverse = reverse;
        Vector3 startPosition = meshRenderer.bounds.center;
        Vector3 endPosition = meshRenderer.bounds.center;
        switch (axis)
        {
            case "x":
            case "X":
                startPosition -= transform.right * start;
                endPosition += transform.right * end;
                break;
            case "y":
            case "Y":
                startPosition -= transform.up * start;
                endPosition += transform.up * end;
                break;
            case "z":
            case "Z":
                startPosition -= transform.forward * start;
                endPosition += transform.forward * end;
                break;
            case "none":
                lineRenderer.enabled = false;
                return;
        }
        if (reverse)
        {
            lineRenderer.SetPosition(1, transform.InverseTransformPoint(startPosition));
            lineRenderer.SetPosition(0, transform.InverseTransformPoint(endPosition));
        }
        else { 
            lineRenderer.SetPosition(0, transform.InverseTransformPoint(startPosition));
            lineRenderer.SetPosition(1, transform.InverseTransformPoint(endPosition)); 
        }
        lineRenderer.enabled = true;
    }

    void OnMouseOver()
    {
        if (!isUI() && Input.GetKeyDown(KeyCode.Mouse2))
        {
            Debug.Log("selectPartWithName " + gameObject.name);
            MainBrowser.RunJavaScript("formatPartDescription(false,'" + gameObject.name + "');");
            switch (lineData.axis)
            {
                case "x":
                case "X":
                    MainBrowser.RunJavaScript("xCheckBox.checked = true; yCheckBox.checked = false; zCheckBox.checked = false;");
                    break;
                case "y":
                case "Y":
                    MainBrowser.RunJavaScript("xCheckBox.checked = false; yCheckBox.checked = true; zCheckBox.checked = false;");
                    break;
                case "z":
                case "Z":
                    MainBrowser.RunJavaScript("xCheckBox.checked = false; yCheckBox.checked = false; zCheckBox.checked = true;");
                    break;
                case "":
                case "none":
                    MainBrowser.RunJavaScript("xCheckBox.checked = false; yCheckBox.checked = false; zCheckBox.checked = false;");
                    return;
            }
            MainBrowser.RunJavaScript("lineStart.value = " + lineData.start.ToString(CultureInfo.InvariantCulture) + "; lineEnd.value = " + lineData.end.ToString(CultureInfo.InvariantCulture) + ";");
        }
    }

    private void OnMouseDown()
    {
        if (!isUI() && !Input.GetKey(KeyCode.LeftShift))
        {
            selected = !selected;
            outline.enabled = selected;
            if (selected)
            {
                importer.selectedExtraParts.Add(this);
            }
            else
            {
                importer.selectedExtraParts.Remove(this);
            }
        }
    }

    public LineData getLineData()
    {
        lineData.position = transform.localPosition;
        lineData.rotation = transform.localRotation;
        lineData.scale = transform.localScale;
        return lineData;
    }

    public bool isUI()
    {
        Vector2 screenCords = MainBrowser.GetScreenCoords();
        return (((Texture2D)ui.texture).GetPixel((int)screenCords.x, (int)screenCords.y).a > 0);
    }
}
