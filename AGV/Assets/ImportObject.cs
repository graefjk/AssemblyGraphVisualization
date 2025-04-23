using UnityEngine;
using AsImpL;
using System.IO;
using System.IO.Compression;
using QuikGraph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using NumSharp;
using NumSharp.Utilities;
using SimpleWebBrowser;
using SFB;
using MessageLibrary;
using System.Globalization;

namespace AGV
{
    public class ImportObject : MonoBehaviour
    {
        [SerializeField]
        public string zipFile;
        protected ImportOptions importOptions = new ImportOptions();
        public List<string> timeLine = new List<string>();
        public int timeLinePosition = -1;

        ObjectImporter objImporter;
        GameObject assembly;
        GameObject parts;
        GameObject finished;
        GameObject extraParts;
        public GameObject activePart;
        [SerializeField]
        public bool reverse = false;
        public bool pause = false;
        [SerializeField] public float partTableLenght;
        public float spacing;
        public string directory;

        AdjacencyGraph<string, STaggedEdge<string, string[]>> graph = new AdjacencyGraph<string, STaggedEdge<string, string[]>>();
        int childCount = 0;
        public WebBrowser2D MainBrowser;
        public bool previewPartsThatCannotBeAssembledRightNow = false;
        public Material dottetLineMaterial;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            MainBrowser = GameObject.Find("Browser2D").GetComponent<WebBrowser2D>();
            MainBrowser.OnShowDialog += MainBrowser_OnJSQuery;
            assembly = transform.Find("Assembly").gameObject;
            parts = transform.Find("Parts").gameObject;
            finished = transform.Find("Finished").gameObject;
            extraParts = transform.Find("ExtraParts").gameObject;
            standardShader = Shader.Find("Universal Render Pipeline/Lit");
            //importOptions.litDiffuse = true;
            importOptions.zUp = false;
            importOptions.convertToDoubleSided = true;
            //importZIP(zipFile);
        }

        public void mouseClick(string name)
        {
            Debug.Log(name);
            MainBrowser.RunJavaScript("document.getElementById('playPause').innerHTML = &#xf04c;");
            pause = false;
            if (UnityEngine.Input.GetKey(KeyCode.LeftShift)) // maybe add && currentVertex.Contains('"'+name+'"')
            {
                selectPart(name);
            }
            else if (removableParts.Contains(name))
            {
                disassemblePart(name);
            }
            else
            {
                assemblePart(name);
            }
        }

        private void selectPart(string name)
        {
            Debug.Log("Select Part " + name);
            MainBrowser.RunJavaScript("toggleCheckBox(" + name + ")");
        }


        private void OnValidate()
        {
            Debug.Log("partTableLenght changed to: " + partTableLenght);
            if (parts != null)
            {
                placePartsOnTable(partTableLenght);
            }
        }

        public float maxY = 0;
        public float xSpacing = 0;
        void placePartsOnTable(float length)
        {
            float maxExtent = Math.Max(assemblyBounds.extents.x, Math.Max(assemblyBounds.extents.y, assemblyBounds.extents.z));
            finished.transform.position = -assemblyBounds.center + new Vector3(partTableLenght / 2, assemblyBounds.extents.magnitude, 0);
            assembly.transform.position = -assemblyBounds.center + new Vector3(partTableLenght / 2, assemblyBounds.extents.magnitude, -assemblyBounds.extents.z - assemblyBounds.extents.magnitude - spacing);
            parts.transform.position = new Vector3(0, 0, -3 * assemblyBounds.extents.z - 2 * assemblyBounds.extents.magnitude - 2 * spacing);

            assembly.GetComponent<RotateAssembly>().centerPosition = assembly.transform.position + assemblyBounds.center;
            assembly.GetComponent<RotateAssembly>().initialPosition = assembly.transform.position;


            float xPosition = 0;
            int xIndex = 0;
            int yIndex = 0;
            SortedSet<Transform> partsList = new SortedSet<Transform>(Comparer<Transform>.Create((a, b) => (a.GetComponent<Renderer>().bounds.extents.y >= b.GetComponent<Renderer>().bounds.extents.y ? 1 : -1)));

            for (int i = 0; i < parts.transform.childCount; i++)
            {
                partsList.Add(parts.transform.GetChild(i));
            }

            foreach (Transform part in partsList)
            {
                Bounds bounds = part.GetComponent<Renderer>().bounds;
                Debug.Log(part.name + " " + bounds);
                Debug.Log(part.name + " " + xPosition + " " + partTableLenght + " " + bounds + " " + part.transform.position + " " + (part.transform.position.z - bounds.center.z + bounds.extents.z));
                if (xPosition + bounds.extents.x > partTableLenght)
                {
                    //Debug.Log(part.name);
                    xPosition = 0;
                    xIndex = 0;
                    yIndex++;
                }
                else if (xPosition != 0)
                {
                    xPosition += bounds.extents.x;
                    xIndex++;
                }
                part.transform.localPosition = new Vector3(xIndex*xSpacing+xPosition - bounds.center.x + part.transform.position.x, (1 * (bounds.extents.z - bounds.center.z + part.transform.position.z) - (2 * maxY * yIndex + yIndex)), part.transform.position.y - bounds.center.y + bounds.extents.y);

                //part.position = new Vector3(0,0,0);
                xPosition += bounds.extents.x + 0.1f;
            }
        }

        private void disassemblePart(String name)
        {
            string sourceVertex = currentVertex.Replace('"' + name + '"', "").Replace(",,", ",").Replace("[,", "[").Replace(",]", "]");
            graph.TryGetEdge(sourceVertex, currentVertex, out outEdge);
            Debug.Log("source: " + sourceVertex + " target: " + currentVertex + " " + outEdge);
            currentVertex = sourceVertex;
            addToTimeLine(currentVertex);
            reverse = true;
            playEdgeTransition(outEdge);
            if (sourceVertex == "[]")
            {
                assembly.transform.Find(name).gameObject.SetActive(false);
            }

        }

        public string currentVertex = "[]";
        public bool isAssembled(string partName)
        {
            return currentVertex.Contains('"' + partName + '"');
        }

        public bool canBeAssembled(string partName)
        {
            return addableParts.Contains(partName);
        }

        IEnumerable<STaggedEdge<string, string[]>> edgeList;
        Shader standardShader;
        public Bounds assemblyBounds;

        public Bounds getAssemblyBounds()
        {
            return assemblyBounds;
        }

        private void ObjImporter_ImportingComplete()
        {
            assembly.transform.localPosition = Vector3.zero;
            assembly.transform.localRotation = Quaternion.identity;
            parts.transform.localPosition = Vector3.zero;
            parts.transform.localRotation = Quaternion.identity;
            finished.transform.localPosition = Vector3.zero;
            finished.transform.localRotation = Quaternion.identity;

            timeLine = new List<string>();
            timeLinePosition = -1;
            addToTimeLine("[]");
            timeLinePosition = 0;
            currentVertex = "[]";

            //GameObject.Find("New Game Object").AddComponent<MeshCollider>();
            assemblyBounds = new Bounds();
            Debug.Log("BOUNDS: " + assemblyBounds);
            //Dom.Element partsList= document.getElementById("parts-list");
            //partsList.innerHTML = "";
            MainBrowser.RunJavaScript("document.getElementById('parts-list').innerHTML=''");
            string partsListHTML = "";
            for (int i = 0; i < parts.transform.childCount; i++)
            {
                Transform child = parts.transform.GetChild(i);
                //add part to UI list
                //partsList.innerHTML += "<input type='checkbox' name='" + child.name + "' checked =\"0\" value=true class='partCheckBox'>&nbsp;<div id='" + child.name + "' class='part' onmouseover='showBorder(this)' onmouseout='hideBorder(this)'>" + child.name + "</div>";
                //MainBrowser.RunJavaScript("document.getElementById('parts-list').innerHTML += <input type='checkbox' name='" + child.name + "' checked ='0' value=true class='partCheckBox'>&nbsp;<div id='" + child.name + "' class='part' onmouseover='showBorder(this)' onmouseout='hideBorder(this)'>" + child.name + "</div>;");
                partsListHTML += "<input type='checkbox' name='" + child.name + "' checked='0' class='partCheckBox' onclick='checkBoxClicked()'>&nbsp;<div id='" + child.name + "' class='part' style='background-color: rgb(255, 141, 30)' onclick='clickPart(this)' onauxclick='selectPart(this)' onmouseover='showBorder(this)' onmouseout='hideBorder(this)'>" + child.name + "</div><div style='width:100%'></div>";

                child.localScale = new Vector3(1, 1, -1);
                child.AddComponent<MeshCollider>();
                Outline outline = child.AddComponent<Outline>();
                outline.OutlineColor = Color.cyan;
                outline.enabled = false;
                child.AddComponent<OnMouseClick>();
                Transform finishedCopy = Instantiate(child, finished.transform);
                finishedCopy.name = child.name;
                Transform copy = Instantiate(child, assembly.transform);
                copy.name = child.name;

                //copy.AddComponent<OnMouseClickDisassemble>();
                assemblyBounds.Encapsulate(copy.gameObject.GetComponent<Renderer>().bounds);
                copy.gameObject.SetActive(false);

                //copy.localScale = new Vector3(1, 1, -1);
                Renderer renderer = child.GetComponent<Renderer>();
                renderer.material.shader = standardShader;
                renderer.material.color = orange;

                if (maxY < renderer.bounds.extents.z)
                {
                    maxY = renderer.bounds.extents.z;
                }
            }
            Debug.Log(partsListHTML);
            MainBrowser.RunJavaScript("document.getElementById('parts-list').innerHTML=\"" + partsListHTML + '"');

            for (int i = 0; i < parts.transform.childCount; i++)
            {
                Transform child = parts.transform.GetChild(i);
                OnMouseClick onMouseClick = child.GetComponent<OnMouseClick>();

                //om.Element childElement = document.getElementById(child.name);
                //childElement.onclick += mouseClick;
                //childElement.onmouseover = onMouseClick.onMouseEnter;
                //childElement.onmouseout = onMouseClick.onMouseExit;
            }

            Debug.Log(assemblyBounds);

            graph.TryGetOutEdges("[]", out edgeList);
            addableParts = new List<string>();
            foreach (STaggedEdge<string, string[]> edge in edgeList)
            {
                parts.transform.Find(edge.Tag[0] + "").GetComponent<Renderer>().material.color = green;
                MainBrowser.RunJavaScript("document.getElementById('" + edge.Tag[0] + "').style.backgroundColor = '" + htmlGreen + "'");
                addableParts.Add(edge.Tag[0] + "");
                Debug.Log(edge);
            }

            //import instructions
            if (File.Exists(Path.Combine(directory, "instructions.json")))
            {
                using (StreamReader r = new StreamReader(Path.Combine(directory, "instructions.json")))
                {
                    string json = r.ReadToEnd().Trim();
                    MainBrowser.RunJavaScript("importJsonInstructions('" + json + "');");
                    MainBrowser.RunJavaScript("prompt('log', 'instructions: ' + instructions);");
                }
            }
            MainBrowser.RunJavaScript("setPartsBorderColors();");
            placePartsOnTable(partTableLenght);

            for (int i = 0; i < finished.transform.childCount; i++)
            {
                finished.transform.GetChild(i).GetComponent<Renderer>().material.shader = standardShader;
            }

            for (int i = 0; i < assembly.transform.childCount; i++)
            {
                assembly.transform.GetChild(i).GetComponent<Renderer>().material.shader = standardShader;
            }
        }

        public void mouseEnterPart(string partName)
        {
            parts.transform.Find(partName).GetComponent<OnMouseClick>().OnMouseEnterDontCheckUI();
        }

        public void mouseExitPart(string partName)
        {
            parts.transform.Find(partName).GetComponent<OnMouseClick>().OnMouseExit();
        }

        void assemblePart(string partName)
        {
            graph.TryGetOutEdges(currentVertex, out edgeList);
            foreach (STaggedEdge<string, string[]> edge in edgeList)
            {
                if ("" + edge.Tag[0] == partName)
                {
                    Debug.Log("1: " + edge);
                    currentVertex = edge.Target;
                    addToTimeLine(currentVertex);
                    reverse = false;
                    playEdgeTransition(edge);
                }
            }
        }

        void addToTimeLine(string Vertex)
        {
            if (timeLinePosition != timeLine.Count - 1)
            {
                Debug.Log((timeLinePosition + 1) + " " + (timeLine.Count - timeLinePosition - 1));
                timeLine.RemoveRange(timeLinePosition + 1, timeLine.Count - timeLinePosition - 1);
            }
            timeLine.Add(Vertex);
            timeLinePosition++;
        }

        HashSet<char> removeChars = new HashSet<char> { '"', '[', ']' };
        List<string> removableParts = new List<string>();
        public List<string> addableParts = new List<string>();
        STaggedEdge<string, string[]> outEdge = new STaggedEdge<string, string[]>();

        Color dodgerBlue = new Color(0.118f, 0.565f, 1f, 1f);
        Color red = new Color(1f, 0.118f, 0.125f, 1f);
        string htmlRed = "rgb(255, 30, 32)";
        Color orange = new Color(1f, 0.553f, 0.118f, 1f);
        string htmlOrange = "rgb(255, 141, 30)";
        Color yellow = new Color(1f, 0.996f, 0.118f, 1f);
        string htmlYellow = "rgb(255, 254, 30)";
        Color green = new Color(0.565f, 1f, 0.118f, 1f);
        string htmlGreen = "rgb(144, 255, 30)";
        public List<string> assembledParts = new List<string>();

        Outline activePartOutline;
        Outline finishedOutline;
        Outline partOutline;

        private void playEdgeTransition(STaggedEdge<string, string[]> edge)
        {
            MainBrowser.RunJavaScript("document.getElementById('playPause').innerHTML = &#xf04c;");
            pause = false;

            for (int i = 0; i < parts.transform.childCount; i++)
            {
                string partName = parts.transform.GetChild(i).name;
                parts.transform.Find(partName).GetComponent<Renderer>().material.color = orange;
                MainBrowser.RunJavaScript("document.getElementById(" + partName + ").style.backgroundColor = '" + htmlOrange + "'");
            }
            Debug.Log("current Vertex: " + currentVertex + " " + currentVertex.Split(','));
            string[] currentParts = currentVertex.ReplaceMultiple(removeChars, ' ').Replace(" ", "").Split(',');
            this.assembledParts = currentParts.ToList();
            for (int i = 0; i < assembly.transform.childCount; i++)
            {
                GameObject child = assembly.transform.GetChild(i).gameObject;
                if (!currentParts.Contains(child.name))
                {
                    child.SetActive(false);
                }
            }

            removableParts = new List<string>();
            foreach (String s in currentParts)
            {
                List<String> inList = currentParts.ToList();
                inList.Remove(s);
                string inVertex = "[";
                for (int i = 0; i < inList.Count - 1; i++)
                {
                    inVertex += '"' + inList[i] + "\",";
                }
                if (inList.Count > 0)
                {
                    inVertex += '"' + inList[inList.Count - 1] + '"';
                }
                inVertex += "]";
                Debug.Log("InVertex: " + inVertex);
                Renderer renderer = parts.transform.Find(s).GetComponent<Renderer>();
                renderer.material.color = Color.gray;
                if (s != "")
                {
                    MainBrowser.RunJavaScript("document.getElementById('" + s + "').style.backgroundColor = 'gray'");
                }
                if (graph.TryGetEdge(inVertex, currentVertex, out outEdge))
                {
                    renderer.material.color = dodgerBlue;
                    MainBrowser.RunJavaScript("document.getElementById('" + s + "').style.backgroundColor = 'dodgerblue'");
                    removableParts.Add(s);
                }
            }
            graph.TryGetOutEdges(currentVertex, out edgeList);
            addableParts = new List<string>();
            foreach (STaggedEdge<string, string[]> edgeItem in edgeList)
            {
                Debug.Log(edgeItem + " " + edgeItem.Tag[0] + " " + edgeItem.Tag[1] + " document.getElementById('" + edgeItem.Tag[0] + "').style.backgroundColor = '" + htmlGreen + "'");
                parts.transform.Find(edgeItem.Tag[0] + "").GetComponent<Renderer>().material.color = green;
                MainBrowser.RunJavaScript("document.getElementById('" + edgeItem.Tag[0] + "').style.backgroundColor = '" + htmlGreen + "'");
                addableParts.Add(edgeItem.Tag[0] + "");
            }
            foreach (string s in edge.Source.Split(','))
            {
                if (s == "[]")
                {
                    continue;
                }
                Debug.Log(s);
                string id = s.Replace("[", "").Replace("]", "").Replace("\"","");
                GameObject part = assembly.transform.Find(id).gameObject;
                part.transform.localPosition = new Vector3(0, 0, 0);
                part.transform.localRotation = Quaternion.identity;
                part.GetComponent<Renderer>().material.color = Color.white;
                Outline outline = part.GetComponent<Outline>();
                outline.OutlineColor = Color.cyan;
                outline.enabled = false;
            }
            GameObject activePart = assembly.transform.Find(edge.Tag[0] + "").gameObject;
            if (this.activePart != null)
            {
                activePartOutline = this.activePart.GetComponent<Outline>();
                activePartOutline.OutlineColor = Color.cyan;
                activePartOutline.enabled = false;

                finishedOutline = finished.transform.Find(this.activePart.name).GetComponent<Outline>();
                finishedOutline.OutlineColor = Color.cyan;
                finishedOutline.enabled = false;

                partOutline = parts.transform.Find(this.activePart.name).GetComponent<Outline>();
                partOutline.OutlineColor = Color.cyan;
                partOutline.enabled = false;
            }
            this.activePart = activePart;
            if (!reverse)
            {
                activePartOutline = activePart.GetComponent<Outline>();
                activePartOutline.OutlineColor = yellow;
                activePartOutline.enabled = true;

                finishedOutline = finished.transform.Find(this.activePart.name).GetComponent<Outline>();
                finishedOutline.OutlineColor = yellow;
                finishedOutline.enabled = true;

                partOutline = parts.transform.Find(this.activePart.name).GetComponent<Outline>();
                partOutline.OutlineColor = yellow;
                partOutline.enabled = true;

                MainBrowser.RunJavaScript("document.getElementById('" + activePart.name + "').style.backgroundColor = '" + htmlYellow + "';");
                activePart.GetComponent<Renderer>().material.color = yellow;
            }
            activePart.SetActive(true);
            //document.getElementById(activePart.name).style.backgroundColor = "yellow";

            if (edge.Tag[1] == "-1")
            {
                if (reverse)
                {
                    activePart.SetActive(false);
                }
                else
                {
                    activePart.transform.localPosition = new Vector3(0, 0, 0);
                    activePart.transform.localRotation = Quaternion.identity;
                    Debug.Log(assembly.transform.Find(edge.Tag[0] + "").transform.localPosition);
                    activePartOutline.enabled = false;
                }
            }
            else
            {
                loadAndPlayTransition(edge.Tag[0], edge.Tag[1]);
            }

            MainBrowser.RunJavaScript("loadInstructions();");
        }

        void showExtraParts()
        {
            if (!reverse)
            {
                for (int i = 0; i < extraParts.transform.childCount; i++)
                {
                    GameObject extraPart = extraParts.transform.GetChild(i).gameObject;
                    DottetLine line = extraPart.GetComponent<DottetLine>();
                    if (!line.assembled && (line.lineData.assembledParts.Except(assembledParts).Count() == 0))
                    {
                        extraPart.SetActive(true);
                        line.assembled = true;
                    }
                    else
                    {
                        extraPart.SetActive(false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < extraParts.transform.childCount; i++)
                {
                    GameObject extraPart = extraParts.transform.GetChild(i).gameObject;
                    DottetLine line = extraPart.GetComponent<DottetLine>();
                    if (extraPart.activeSelf && (line.lineData.assembledParts.Except(assembledParts).Count() > 0))
                    {
                        Debug.Log("TRUE");
                        line.assembled = false;
                        extraPart.SetActive(false);
                    }
                    else if (line.assembled){
                        bool hasPartThatCanBeDisassembled = false;
                        foreach (string part in line.lineData.assembledParts)
                        {
                            if (removableParts.Contains(part))
                            {
                                hasPartThatCanBeDisassembled = true;
                                break;
                            }
                        }
                        if (hasPartThatCanBeDisassembled)
                        {
                            extraPart.SetActive(true);
                        }
                    }
                }
            }
        }

        string path = "";
        public bool play = false;
        GameObject transitionObject;
        List<Matrix4x4> matrixes;
        void loadAndPlayTransition(string partID, string transitionID)
        {
            if (!(path.Contains("\\" + transitionID + "\\") || path.Contains("/" + transitionID + "/")))
            {
                Debug.Log(path);
                matrixes = new List<Matrix4x4>();
                path = Path.Combine(directory, "steps", transitionID, "transformationMatrices.npz");
                Debug.Log(path);
                NpzDictionary<Array> dict;
                var data = np.Load_Npz(path, out dict);
                foreach (var item in data)
                {
                    matrixes.Add(NDArrayToMatrix4x4(item.Value));
                }
                transitionObject = assembly.transform.Find(partID).gameObject;
                dict.Dispose();
            }
            t = reverse ? 0 : matrixes.Count - 1;
            play = true;
        }

        private Matrix4x4 NDArrayToMatrix4x4(NDArray nDarray)
        {
            double[,] array = (double[,])nDarray.ToMuliDimArray<double>();
            Matrix4x4 matrix = new Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                for (int k = 0; k < 4; k++)
                {
                    matrix[i, k] = (float)array[i, k];
                }
            }
            return matrix;
        }

        public float tickTime;
        public bool repeat = true;
        float LastTick = 0;
        int t = -1;
        void playTransition(GameObject part, bool reverse = false)
        {
            if ((Time.time - LastTick > tickTime) && (t >= 0) && (t < matrixes.Count))
            {
                part.transform.localPosition = matrixes[t].GetPosition();
                part.transform.localRotation = matrixes[t].rotation;
                t += reverse ? 1 : -1;

                //object1.transform.Translate(matrixes[t++].GetPosition());
                //object1.transform.Rotate(matrixes[t++].rotation.eulerAngles);
                LastTick = Time.time;
            }
            if (((t < 0) || t >= matrixes.Count))
            {
                if (repeat)
                {
                    Debug.Log("repeat");
                    t = reverse ? 0 : matrixes.Count - 1;
                    part.transform.localPosition = matrixes[t].GetPosition();
                    part.transform.localRotation = matrixes[t].rotation;
                }
                else
                {
                    if (reverse)
                    {
                        part.SetActive(false);
                    }
                    play = false;
                    part.transform.localPosition = Vector3.zero;
                    part.transform.localRotation = Quaternion.identity;
                    showExtraParts();
                    activePartOutline.enabled = false;
                }
            }
        }

        void stopTransition()
        {

        }



        public void chooseZIP()
        {
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "zip", false);
            if (paths.Length > 0)
            {
                Debug.Log(paths[0]);
                importZIP(paths[0]);
            }
        }

        void removeAllObjects()
        {
            foreach (Transform child in assembly.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in parts.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in finished.transform)
            {
                Destroy(child.gameObject);
            }
        }

        string folderName;
        string basePath;
        SerializableList<DottetLine.LineData> dottetLines;

        public void importZIP(string zipFile)
        {
            removeAllObjects();
            Destroy(gameObject.GetComponent<ObjectImporter>());
            objImporter = gameObject.AddComponent<ObjectImporter>();
            objImporter.ImportingComplete += ObjImporter_ImportingComplete;

            basePath = Path.GetDirectoryName(zipFile);
            folderName = Path.GetFileNameWithoutExtension(zipFile);
            directory = Path.Combine(basePath, folderName);

            //ZipFile.ExtractToDirectory("C:\\Users\\janni\\Documents\\GitHub\\ATM-AGV\\assembly_00013.zip", "C:\\Users\\janni\\Documents\\GitHub\\ATM-AGV\\assembly_00013",true);
            ZipFile.ExtractToDirectory(zipFile, directory, true);
            //import objects

            foreach (string file in Directory.GetFiles(Path.Combine(directory, "objects")))
            {
                objImporter.ImportModelAsync(Path.GetFileNameWithoutExtension(file), file, parts.transform, importOptions);
                childCount++;
            }

            string extraPartsFolder = Path.Combine(directory, "extraParts");
            if (File.Exists(Path.Combine(directory, "extraParts.json")))
            {
                ObjectImporter extraPartsFileImporter = assembly.AddComponent<ObjectImporter>();
                extraPartsFileImporter.ImportingComplete += extraPartsInitialImportComplete;
                using (StreamReader reader = new StreamReader(Path.Combine(directory, "extraParts.json")))
                {
                    string json = reader.ReadToEnd();
                    dottetLines = JsonUtility.FromJson<SerializableList<DottetLine.LineData>>(json);
                    foreach (DottetLine.LineData line in dottetLines.list)
                    {
                        Debug.Log(line.partName + " , " + Path.Combine(extraPartsFolder, line.partName + ".obj"));
                        extraPartsFileImporter.ImportModelAsync(line.partName, Path.Combine(extraPartsFolder, line.partName + ".obj"), extraParts.transform, importOptions);
                    }
                }
            }

            //build graph
            graph = new AdjacencyGraph<string, STaggedEdge<string, string[]>>();
            using (StreamReader r = new StreamReader(Path.Combine(directory, "graph.json")))
            {
                string json = r.ReadToEnd();
                JObject array = (JObject)JsonConvert.DeserializeObject(json);
                Debug.Log(array["nodes"]);
                Debug.Log(array["links"]);
                foreach (dynamic item in array["nodes"]) //add all nodes to the graph
                {
                    graph.AddVertex(item["id"].ToString(Formatting.None));
                }
                foreach (dynamic item in array["links"]) //add all edges to the graph
                {
                    graph.AddEdge(new STaggedEdge<string, string[]>(item["source"].ToString(Formatting.None), item["target"].ToString(Formatting.None), new string[] { item["moveID"].ToObject<string>(), item["edgeID"].ToObject<string>() }));
                }
            }
            Debug.Log(exportDotGraph(graph));
        }

        private void extraPartsInitialImportComplete()
        {
            extraPartsImportComplete();
            selectedExtraParts.Clear();
            for (int i = 0; i < extraParts.transform.childCount; i++)
            {
                Transform part = extraParts.transform.GetChild(i);
                DottetLine dottetLine = part.GetComponent<DottetLine>();
                Debug.Log(part.name + dottetLines.list[0].partName);
                int index = dottetLines.list.FindIndex(line => { Debug.Log(line.partName + " " + part.name); return line.partName == part.name; });
                DottetLine.LineData lineData = dottetLines.list[index];
                dottetLines.list.RemoveAt(index);
                dottetLine.lineData = lineData;
                part.localPosition = lineData.position;
                part.localRotation = lineData.rotation;
                part.localScale = lineData.scale;
                dottetLine.meshRenderer = part.GetComponent<MeshRenderer>();
                dottetLine.lineRenderer = part.GetComponent<LineRenderer>();
                dottetLine.setLine(lineData.axis, lineData.reverse, lineData.start, lineData.end);
                part.GetComponent<Outline>().enabled = false;
                dottetLine.selected = false;
                part.gameObject.SetActive(false);
            }
        }

        //you can visualize the output here: https://dreampuf.github.io/GraphvizOnline
        public string exportDotGraph(AdjacencyGraph<string, STaggedEdge<string, string[]>> graph, bool edgeLabels = true)
        {
            string graphString = "";
            graphString += "digraph G {\n";
            foreach (string s in graph.Vertices)
            {
                graphString += "\"" + s.Replace("\"", "") + "\";\n";
            }
            foreach (STaggedEdge<string, string[]> edge in graph.Edges)
            {
                if (edgeLabels)
                {
                    graphString += "\"" + edge.Source.Replace("\"", "") + "\" -> \"" + edge.Target.Replace("\"", "") + "\" [label=" + edge.Tag + "];\n";
                }
                else
                {
                    graphString += "\"" + edge.Source.Replace("\"", "") + "\" -> \"" + edge.Target.Replace("\"", "") + "\";\n";
                }
            }
            return graphString + "}";
        }

        public string getDotGraphURL(AdjacencyGraph<string, STaggedEdge<string, string[]>> graph, bool edgeLabels = true)
        {
            return "https://dreampuf.github.io/GraphvizOnline/?engine=dot#" + Uri.EscapeDataString(exportDotGraph(graph, edgeLabels));
        }

        public void openGraphVisualization(AdjacencyGraph<string, STaggedEdge<string, string[]>> graph, bool edgeLabels = true)
        {
            System.Diagnostics.Process.Start(getDotGraphURL(graph, edgeLabels));
        }

        public void stepForward()
        {
            if (timeLinePosition + 1 >= timeLine.Count)
            {
                return;
            }
            STaggedEdge<string, string[]> edge;
            if (timeLine[timeLinePosition].Length > timeLine[timeLinePosition + 1].Length)
            {
                graph.TryGetEdge(timeLine[timeLinePosition + 1], timeLine[timeLinePosition], out edge);
                reverse = true;
            }
            else
            {
                graph.TryGetEdge(timeLine[timeLinePosition], timeLine[timeLinePosition + 1], out edge);
                reverse = false;
            }
            currentVertex = timeLine[timeLinePosition + 1];
            playEdgeTransition(edge);
            timeLinePosition++;
        }

        public void log(string message)
        {
            Debug.Log(message);
        }

        public void stepBackward()
        {
            if (timeLinePosition - 1 < 0)
            {
                return;
            }
            STaggedEdge<string, string[]> edge;
            if (timeLine[timeLinePosition].Length > timeLine[timeLinePosition - 1].Length)
            {
                graph.TryGetEdge(timeLine[timeLinePosition - 1], timeLine[timeLinePosition], out edge);
                reverse = true;
            }
            else
            {
                graph.TryGetEdge(timeLine[timeLinePosition], timeLine[timeLinePosition - 1], out edge);
                reverse = false;
            }
            currentVertex = timeLine[timeLinePosition - 1];
            playEdgeTransition(edge);
            timeLinePosition--;
        }

        public void repeatAnimation()
        {
            //transitionObject.SetActive(true);
            //t = reverse ? 0 : matrixes.Count - 1;
            stepBackward();
            stepForward();
            MainBrowser.RunJavaScript("document.getElementById('playPause').innerHTML = &#xf04c;");

            pause = false;
        }

        bool resizedOnce = false;

        public void resizeUI()
        {
            MainBrowser.OnRectTransformDimensionsChange();
        }

        public void playPause()
        {
            if (play)
            {
                pause = !pause;
                if (pause)
                {
                    MainBrowser.RunJavaScript("document.getElementById('playPause').innerHTML = &#xf04b;");
                }
                else
                {
                    MainBrowser.RunJavaScript("document.getElementById('playPause').innerHTML = &#xf04c;");
                }
            }
        }

        private void MainBrowser_OnJSQuery(string message, string prompt, DialogEventType type)
        {

            if (prompt == null)
            {
                Debug.Log("Hallo!!!! " + message);
                this.GetType().GetMethod(message).Invoke(this, null);
            }
            else
            {
                Debug.Log("Hallo!!!! " + message + " " + prompt);
                this.GetType().GetMethod(message).Invoke(this, new object[] { prompt });
            }
        }

        public void copyText(string text)
        {
            Debug.Log("Copied " + text + " to clipboard");
            GUIUtility.systemCopyBuffer = text;
        }

        public void pasteText()
        {
            Debug.Log("Pasted " + GUIUtility.systemCopyBuffer);
            MainBrowser.RunJavaScript("pasteToTextArea('" + GUIUtility.systemCopyBuffer + "');");
        }

        [SerializeField]
        public Texture2D resizeCursor;
        [SerializeField]
        public Texture2D resizeCursorBottom;

        public CursorMode cursorMode = CursorMode.Auto;

        public void changeToResizeCursor()
        {
            Cursor.SetCursor(resizeCursor, new Vector2(11, 2), cursorMode);
        }

        public void changeToResizeCursorBottom()
        {
            Cursor.SetCursor(resizeCursorBottom, new Vector2(2, 11), cursorMode);
        }

        public void changeToNormalCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }

        public bool textAreaHasFocus = false;
        public void textAreaFocus()
        {
            textAreaHasFocus = true;
        }

        public void textAreaBlur()
        {
            textAreaHasFocus = false;
        }

        [System.Serializable]
        public class SerializableList<T>
        {
            public List<T> list = new List<T>();
        }

        public void saveFile(string instructionJSON)
        {
            Debug.Log("saving JSON:" + instructionJSON + " to " + Path.Combine(directory, "instructions.json"));
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(directory, "instructions.json")))
            {
                outputFile.WriteLine(instructionJSON.Replace(@"\",@"\\"));
                outputFile.Flush();
                outputFile.Close();
                outputFile.Dispose();
            }



            SerializableList<DottetLine.LineData> dottetLines = new SerializableList<DottetLine.LineData>();
            for (int i = 0; i < extraParts.transform.childCount; i++)
            {
                Transform part = extraParts.transform.GetChild(i);
                dottetLines.list.Add(part.GetComponent<DottetLine>().getLineData());
            }

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(directory, "extraParts.json")))
            {
                outputFile.WriteLine(JsonUtility.ToJson(dottetLines));
                outputFile.Flush();
                outputFile.Close();
                outputFile.Dispose();
            }

            string fileName = StandaloneFileBrowser.SaveFilePanel("save File", basePath, folderName, "zip");
            if (fileName != "")
            {
                ZipFile.CreateFromDirectory(directory, fileName);
                Debug.Log("File saved to " + fileName);
            }
        }

        ObjectImporter extraPartsImporter;
        string[] extraPartsPaths;
        public void addExtraPart()
        {
            extraPartsImporter = extraParts.AddComponent<ObjectImporter>();
            extraPartsPaths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "obj", true);
            string extraPartsFolderPath = Path.Combine(directory, "extraParts");
            Directory.CreateDirectory(extraPartsFolderPath);
            foreach (string path in extraPartsPaths)
            {
                if (!File.Exists(Path.Combine(extraPartsFolderPath, Path.GetFileName(path))))
                {
                    File.Copy(path, Path.Combine(extraPartsFolderPath, Path.GetFileName(path)), false);
                }
                extraPartsImporter.ImportModelAsync(Path.GetFileNameWithoutExtension(path), path, extraParts.transform, importOptions);
                extraPartsImporter.ImportingComplete += extraPartsImportComplete;
            }
        }

        public List<DottetLine> selectedExtraParts;

        void extraPartsImportComplete()
        {
            for (int i = 0; i < extraParts.transform.childCount; i++)
            {
                Transform part = extraParts.transform.GetChild(i);
                if (part.GetComponent<DottetLine>() != null)
                {
                    continue;
                }
                //part.AddComponent<onMouseClickExtraPart>();
                Renderer renderer = part.GetComponent<Renderer>();
                renderer.material.shader = standardShader;
                part.AddComponent<MeshCollider>();
                LineRenderer lineRenderer = part.AddComponent<LineRenderer>();
                DottetLine dottetLine = part.AddComponent<DottetLine>();
                dottetLine.selected = true;
                selectedExtraParts.Add(dottetLine);
                Outline outline = part.AddComponent<Outline>();
                lineRenderer.material = dottetLineMaterial;
                lineRenderer.useWorldSpace = false;
                lineRenderer.SetPosition(0, part.transform.InverseTransformPoint(renderer.bounds.center));
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.widthMultiplier = 0.3f;
                lineRenderer.enabled = false;
                outline.OutlineColor = Color.green;
                part.gameObject.layer = 3;
                part.transform.position = assemblyBounds.center + assembly.transform.position - new Vector3(assemblyBounds.size.magnitude, 0, 0);
                if (part.transform.position == Vector3.zero)
                {
                    part.transform.position = -Vector3.one * 3;
                }
            }
            Debug.Log("Finished Importing Extra Parts");
        }

        public void setExtraPartLine(string args)
        {
            string[] arguments = args.Split(',');
            string name = arguments[0];
            string axis = arguments[1];
            bool reverse = false;
            if (name != "undefined")
            {
                if (arguments[2] == "true")
                {
                    reverse = true;
                }
                if (!float.TryParse(arguments[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float start))
                {
                    start = 0f;
                }
                if (!float.TryParse(arguments[4], NumberStyles.Any, CultureInfo.InvariantCulture, out float end))
                {
                    end = 0f;
                }
                for (int i = 0; i < extraParts.transform.childCount; i++)
                {
                    Transform part = extraParts.transform.GetChild(i);
                    if (part.name == name)
                    {
                        part.GetComponent<DottetLine>().setLine(axis, reverse, start, end);
                    }
                }
            }
        }

        public void saveExtraParts(string parts)
        {
            List<string> selectedParts = new List<string>();
            Hashset<string> hashset = new Hashset<string>();
            foreach (string part in parts.Split(','))
            {
                selectedParts.Add(part);
            }
            foreach (DottetLine line in selectedExtraParts)
            {
                line.lineData.assembledParts = selectedParts;
                line.assembled = true;
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (play && !pause)
            {
                playTransition(transitionObject, reverse);
            }
            if (UnityEngine.Input.GetKeyDown("r") && !textAreaHasFocus)
            {
                repeatAnimation();
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
            {
                stepBackward();
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
            {
                stepForward();
            }
        }
    }
}
