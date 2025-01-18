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

public class ImportObject : MonoBehaviour
{
    [SerializeField]
    public string zipFile;
    protected ImportOptions importOptions = new ImportOptions();
    ObjectImporter objImporter;


    AdjacencyGraph<string, STaggedEdge<string, int[]>> graph = new AdjacencyGraph<string, STaggedEdge<string, int[]>>();
    int childCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //importOptions.litDiffuse = true;
        objImporter = gameObject.GetComponent<ObjectImporter>();
        objImporter.ImportingComplete += ObjImporter_ImportingComplete;
        importZIP(zipFile);
    }

    public void mouseClick(string name)
    {
        Debug.Log(name);
        assemblePart(name);
    }

    string currentVertex = "[]";
    IEnumerable<STaggedEdge<string, int[]>> edgeList;

    private void ObjImporter_ImportingComplete()
    {
        float xPosition = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.localScale = new Vector3(-1, 1, 1);
            child.AddComponent<MeshCollider>();
            child.AddComponent<OnMouseClick>();

            Renderer renderer = child.GetComponent<Renderer>();
            renderer.material.color = Color.red;

            Bounds bounds = renderer.bounds;
            xPosition += bounds.extents.x;
            child.position = new Vector3(xPosition - bounds.center.x, bounds.extents.y - bounds.center.y, 0);
            xPosition += bounds.extents.x + 0.1f;
        }

        
        graph.TryGetOutEdges("[]", out edgeList);

        foreach (STaggedEdge<string, int[]> edge in edgeList)
        {
            transform.Find(edge.Tag[0] + "").GetComponent<Renderer>().material.color = Color.green;
            Debug.Log(edge);
        }
    }

    void assemblePart(string partName)
    {
        graph.TryGetOutEdges(currentVertex, out edgeList);
        foreach (STaggedEdge<string, int[]> edge in edgeList)
        {
            if(""+edge.Tag[0] == partName)
            {
                Debug.Log(edge);
                currentVertex = edge.Target;
                playEdgeTransition(edge);
            }            
        }
    }

    private void playEdgeTransition(STaggedEdge<string, int[]> edge)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.Find(i + "").GetComponent<Renderer>().material.color = Color.red;
        }
        Debug.Log( "current Vertex: "+currentVertex );
        graph.TryGetOutEdges(currentVertex, out edgeList);
        foreach (STaggedEdge<string, int[]> edgeItem in edgeList)
        {
            Debug.Log(edgeItem + " " + edgeItem.Tag[0] + " " + edgeItem.Tag[1]);
            transform.Find(edgeItem.Tag[0] + "").GetComponent<Renderer>().material.color = Color.green;
        }

        foreach (string s in edge.Target.Split(','))
        {
            string id = string.Concat(s.Where(Char.IsDigit));
            GameObject part = transform.Find(id).gameObject;
            part.transform.position = new Vector3(-5,15,-25);
            part.GetComponent<Renderer>().material.color = Color.white;
        }
        transform.Find(""+edge.Tag[0]).GetComponent<Renderer>().material.color = Color.yellow;
    }





    public void importZIP(string zipFile)
    {
        string basePath = Path.GetDirectoryName(zipFile);
        string folderName = Path.GetFileNameWithoutExtension(zipFile);
        string directory = basePath + "\\" + folderName;

        //ZipFile.ExtractToDirectory("C:\\Users\\janni\\Documents\\GitHub\\ATM-AGV\\assembly_00013.zip", "C:\\Users\\janni\\Documents\\GitHub\\ATM-AGV\\assembly_00013",true);
        ZipFile.ExtractToDirectory(zipFile, directory, true);
        //import objects

        foreach (string file in Directory.GetFiles(directory + "\\objects"))
        {
            objImporter.ImportModelAsync(Path.GetFileNameWithoutExtension(file), file, transform, importOptions);
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
                graph.AddEdge(new STaggedEdge<string, int[]>(item["source"].ToString(Formatting.None), item["target"].ToString(Formatting.None), new int[]{item["moveID"].ToObject<int>() , item["edgeID"].ToObject<int>()}));
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

    // Update is called once per frame
    void Update()
    {

    }
}
