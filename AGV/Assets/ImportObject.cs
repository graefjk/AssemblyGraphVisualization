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
using QuikGraph.Collections;
using System.Linq;
using NumSharp;
using System.Xml.Linq;
using NumSharp.Utilities;
using UnityEngine.InputSystem.HID;
using PowerUI;

namespace AGV
{
    public class ImportObject : MonoBehaviour
    {
        [SerializeField]
        public string zipFile;
        protected ImportOptions importOptions = new ImportOptions();
        public List<string> timeLine = new List<string>();
        public int timeLinePosition = 0;

        ObjectImporter objImporter;
        GameObject assembly;
        GameObject parts;
        GameObject finished;
        public GameObject activePart;
        [SerializeField]
        public bool reverse;
        public bool pause = false;
        Dom.Element playPauseElement;

        [SerializeField] public float partTableLenght;


        AdjacencyGraph<string, STaggedEdge<string, int[]>> graph = new AdjacencyGraph<string, STaggedEdge<string, int[]>>();
        int childCount = 0;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            assembly = transform.Find("Assembly").gameObject;
            parts = transform.Find("Parts").gameObject;
            finished = transform.Find("Finished").gameObject;
            standardShader = Shader.Find("Standard");
            addToTimeLine("[]");
            timeLinePosition = 0;
            //importOptions.litDiffuse = true;
            importOptions.zUp = false;
            importOptions.convertToDoubleSided = true;
            objImporter = gameObject.GetComponent<ObjectImporter>();
            objImporter.ImportingComplete += ObjImporter_ImportingComplete;
            importZIP(zipFile);

            HtmlDocument document = UI.document;
            Dom.Element stepBackElement = document.getElementById("stepBack");
            stepBackElement.onclick += stepBackward;
            Dom.Element pauseElement = document.getElementById("repeat");
            pauseElement.onclick += repeatAnimation;
            Dom.Element stepForwardElement = document.getElementById("stepForward");
            stepForwardElement.onclick += stepForward;
            playPauseElement = document.getElementById("playPause");
            playPauseElement.onclick += playPause;
        }

        public void mouseClick(string name)
        {
            Debug.Log(name);
            playPauseElement.innerHTML = "&#xf04c;";
            pause = false;
            if (removableParts.Contains(name))
            {
                disassemblePart(name);
            }
            else
            {
                assemblePart(name);
            }
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
        void placePartsOnTable(float length)
        {
            float xPosition = 0;
            int yPosition = 0;

            for (int i = 0; i < parts.transform.childCount; i++)
            {
                Transform part = parts.transform.GetChild(i);
                Bounds bounds = part.GetComponent<Renderer>().bounds;
                Debug.Log(part.name + " " + xPosition + " " + partTableLenght + " " + bounds + " " + part.transform.position + " " + (part.transform.position.z - bounds.center.z + bounds.extents.z));
                if (xPosition + bounds.extents.x > partTableLenght)
                {
                    //Debug.Log(part.name);
                    xPosition = 0;
                    yPosition++;
                }
                else if (xPosition != 0)
                {
                    xPosition += bounds.extents.x;
                }
                part.transform.localPosition = new Vector3(xPosition - bounds.center.x + part.transform.position.x, 1 * (bounds.extents.z - bounds.center.z + part.transform.position.z) + (3 * maxY * yPosition), part.transform.position.y - bounds.center.y + bounds.extents.y);

                //part.position = new Vector3(0,0,0);
                xPosition += bounds.extents.x + 0.1f;
            }
        }

        private void disassemblePart(String name)
        {
            string sourceVertex = currentVertex.Replace('"' + name + '"', "").Replace(",,", ",").Replace("[,", "[").Replace(",]", "]");
            graph.TryGetEdge(sourceVertex, currentVertex, out outEdge);
            Debug.Log("source: " + sourceVertex);
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

        IEnumerable<STaggedEdge<string, int[]>> edgeList;
        Shader standardShader;
        public Bounds assemblyBounds;

        public Bounds getAssemblyBounds()
        {
            return assemblyBounds;
        }

        private void ObjImporter_ImportingComplete()
        {
            assemblyBounds = assembly.GetComponent<Renderer>().bounds;
            float xPosition = 0;
            for (int i = 0; i < parts.transform.childCount; i++)
            {
                Transform child = parts.transform.GetChild(i);
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
                renderer.material.color = Color.red;

                if (maxY < renderer.bounds.extents.y)
                {
                    maxY = renderer.bounds.extents.y;
                }
            }

            Debug.Log(assemblyBounds);


            graph.TryGetOutEdges("[]", out edgeList);
            addableParts = new List<string>();
            foreach (STaggedEdge<string, int[]> edge in edgeList)
            {
                parts.transform.Find(edge.Tag[0] + "").GetComponent<Renderer>().material.color = Color.green;
                addableParts.Add(edge.Tag[0] + "");
                Debug.Log(edge);
            }

            placePartsOnTable(partTableLenght);
        }

        void assemblePart(string partName)
        {
            graph.TryGetOutEdges(currentVertex, out edgeList);
            foreach (STaggedEdge<string, int[]> edge in edgeList)
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
        STaggedEdge<string, int[]> outEdge = new STaggedEdge<string, int[]>();

        private void playEdgeTransition(STaggedEdge<string, int[]> edge)
        {
            playPauseElement.innerHTML = "&#xf04c;";
            pause = false;
            for (int i = 0; i < parts.transform.childCount; i++)
            {
                parts.transform.Find(i + "").GetComponent<Renderer>().material.color = Color.red;
            }
            Debug.Log("current Vertex: " + currentVertex + " " + currentVertex.Split(','));
            string[] currentParts = currentVertex.ReplaceMultiple(removeChars, ' ').Replace(" ", "").Split(',');
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
                if (graph.TryGetEdge(inVertex, currentVertex, out outEdge))
                {
                    renderer.material.color = Color.blue;
                    removableParts.Add(s);
                }
            }
            graph.TryGetOutEdges(currentVertex, out edgeList);
            addableParts = new List<string>();
            foreach (STaggedEdge<string, int[]> edgeItem in edgeList)
            {
                Debug.Log(edgeItem + " " + edgeItem.Tag[0] + " " + edgeItem.Tag[1]);
                parts.transform.Find(edgeItem.Tag[0] + "").GetComponent<Renderer>().material.color = Color.green;
                addableParts.Add(edgeItem.Tag[0] + "");
            }
            foreach (string s in edge.Source.Split(','))
            {
                if (s == "[]")
                {
                    continue;
                }
                string id = string.Concat(s.Where(Char.IsDigit));
                GameObject part = assembly.transform.Find(id).gameObject;
                part.transform.localPosition = new Vector3(0, 0, 0);
                part.GetComponent<Renderer>().material.color = Color.white;
                Outline outline = part.GetComponent<Outline>();
                outline.OutlineColor = Color.cyan;
                outline.enabled = false;
            }
            GameObject activePart = assembly.transform.Find(edge.Tag[0] + "").gameObject;
            Outline activePartOutline;
            Outline finishedOutline;
            Outline partOutline;
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

            activePartOutline = this.activePart.GetComponent<Outline>();
            activePartOutline.OutlineColor = Color.yellow;
            activePartOutline.enabled = true;

            finishedOutline = finished.transform.Find(this.activePart.name).GetComponent<Outline>();
            finishedOutline.OutlineColor = Color.yellow;
            finishedOutline.enabled = true;

            partOutline = parts.transform.Find(this.activePart.name).GetComponent<Outline>();
            partOutline.OutlineColor = Color.yellow;
            partOutline.enabled = true;

            activePart.SetActive(true);
            activePart.GetComponent<Renderer>().material.color = Color.yellow;
            if (edge.Tag[1] == -1)
            {
                if (reverse)
                {
                    activePart.SetActive(false);
                }
                else
                {
                    activePart.transform.localPosition = new Vector3(0, 0, 0);
                    Debug.Log(assembly.transform.Find(edge.Tag[0] + "").transform.localPosition);
                }
            }
            else
            {
                loadAndPlayTransition(edge.Tag[0], edge.Tag[1]);
            }
        }

        string path = "";
        public bool play = false;
        GameObject transitionObject;
        List<Matrix4x4> matrixes;
        void loadAndPlayTransition(int partID, int transitionID)
        {
            if (!path.Contains("\\" + transitionID + "\\"))
            {
                Debug.Log(path);
                matrixes = new List<Matrix4x4>();
                path = directory + "\\steps\\" + transitionID + "\\transformationMatrices.npz";
                NpzDictionary<Array> dict;
                var data = np.Load_Npz(path, out dict);
                foreach (var item in data)
                {
                    matrixes.Add(NDArrayToMatrix4x4(item.Value));
                }
                transitionObject = assembly.transform.Find(partID + "").gameObject;
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
                //Debug.Log(t);
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
                }

            }
        }

        void stopTransition()
        {

        }

        string directory;

        public void importZIP(string zipFile)
        {
            string basePath = Path.GetDirectoryName(zipFile);
            string folderName = Path.GetFileNameWithoutExtension(zipFile);
            directory = basePath + "\\" + folderName;

            //ZipFile.ExtractToDirectory("C:\\Users\\janni\\Documents\\GitHub\\ATM-AGV\\assembly_00013.zip", "C:\\Users\\janni\\Documents\\GitHub\\ATM-AGV\\assembly_00013",true);
            ZipFile.ExtractToDirectory(zipFile, directory, true);
            //import objects

            foreach (string file in Directory.GetFiles(directory + "\\objects"))
            {
                objImporter.ImportModelAsync(Path.GetFileNameWithoutExtension(file), file, parts.transform, importOptions);
                childCount++;
            }



            //build graph

            using (StreamReader r = new StreamReader(directory + "\\graph.json"))
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
                    graph.AddEdge(new STaggedEdge<string, int[]>(item["source"].ToString(Formatting.None), item["target"].ToString(Formatting.None), new int[] { item["moveID"].ToObject<int>(), item["edgeID"].ToObject<int>() }));
                }
            }
            Debug.Log(exportDotGraph(graph));
        }

        //you can visualize the output here: https://dreampuf.github.io/GraphvizOnline
        public string exportDotGraph(AdjacencyGraph<string, STaggedEdge<string, int[]>> graph, bool edgeLabels = true)
        {
            string graphString = "";
            graphString += "digraph G {\n";
            foreach (string s in graph.Vertices)
            {
                graphString += "\"" + s.Replace("\"", "") + "\";\n";
            }
            foreach (STaggedEdge<string, int[]> edge in graph.Edges)
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

        public string getDotGraphURL(AdjacencyGraph<string, STaggedEdge<string, int[]>> graph, bool edgeLabels = true)
        {
            return "https://dreampuf.github.io/GraphvizOnline/?engine=dot#" + Uri.EscapeDataString(exportDotGraph(graph, edgeLabels));
        }

        public void openGraphVisualization(AdjacencyGraph<string, STaggedEdge<string, int[]>> graph, bool edgeLabels = true)
        {
            System.Diagnostics.Process.Start(getDotGraphURL(graph, edgeLabels));
        }

        public void stepForward(MouseEvent mouseEvent = null)
        {
            if (timeLinePosition + 1 >= timeLine.Count)
            {
                return;
            }
            STaggedEdge<string, int[]> edge;
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

        public void log(MouseEvent mouseEvent)
        {
            Debug.Log("a message from JavaScript!");
        }

        public void stepBackward(MouseEvent mouseEvent = null)
        {
            if (timeLinePosition - 1 < 0)
            {
                return;
            }
            STaggedEdge<string, int[]> edge;
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

        public void repeatAnimation(MouseEvent mouseEvent = null)
        {
            transitionObject.SetActive(true);
            t = reverse ? 0 : matrixes.Count - 1;
            playPauseElement.innerHTML = "&#xf04c;";
            pause = false;
        }

        private void playPause(MouseEvent mouseEvent = null)
        {
            if (play)
            {
                pause = !pause;
                playPauseElement.innerHTML = pause ? "&#xf04b;": "&#xf04c;";
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (play && !pause)
            {
                playTransition(transitionObject, reverse);
            }
            if (UnityEngine.Input.GetKeyDown("r"))
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
