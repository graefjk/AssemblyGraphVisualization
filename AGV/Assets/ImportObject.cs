using UnityEngine;
using AsImpL;
using System.IO;
using System.IO.Compression;
using QuikGraph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuikGraph.Graphviz;
using Loonim;
using System;

public class ImportObject : MonoBehaviour
{
    [SerializeField]
    public GameObject obj;
    [SerializeField]
    public string zipFile;
    protected ImportOptions importOptions = new ImportOptions();
    ObjectImporter objImporter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //importOptions.litDiffuse = true;
        objImporter = gameObject.GetComponent<ObjectImporter>();
        importZIP(zipFile);
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
            objImporter.ImportModelAsync(Path.GetFileNameWithoutExtension(file), file, null, importOptions);
        }


        //build graph
        AdjacencyGraph<string, STaggedEdge<string, int>> graph = new AdjacencyGraph<string, STaggedEdge<string, int>>();
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
                graph.AddEdge(new STaggedEdge<string, int>(item["source"].ToString(Formatting.None), item["target"].ToString(Formatting.None), item["moveID"].ToObject<int>()));
            }
        }
        var graphviz = new GraphvizAlgorithm<string, STaggedEdge<string, int>>(graph);
        string dotGraph = graphviz.Generate();
        openGraphVisualization(graph);
    }

    //you can visualize the output here: https://dreampuf.github.io/GraphvizOnline
    public string exportDotGraph(AdjacencyGraph<string, STaggedEdge<string, int>> graph, bool edgeLabels=true)
    {
        string graphString = "";
        graphString += "digraph G {\n";
        foreach (string s in graph.Vertices)
        {
            graphString += "\""+s.Replace("\"","")+"\";\n";
        }
        foreach (STaggedEdge<string, int> edge in graph.Edges)
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

    public string getDotGraphURL(AdjacencyGraph<string, STaggedEdge<string, int>> graph, bool edgeLabels = true)
    {
        return "https://dreampuf.github.io/GraphvizOnline/?engine=dot#" + Uri.EscapeDataString(exportDotGraph(graph, edgeLabels));
    }

    public void openGraphVisualization(AdjacencyGraph<string, STaggedEdge<string, int>> graph, bool edgeLabels = true)
    {
        System.Diagnostics.Process.Start(getDotGraphURL(graph,edgeLabels));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
