using UnityEngine;
using NumSharp;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;


public class Importer1 : MonoBehaviour
{
    public GameObject object1, object2;
    public float tickTime;
    public string path;
    public bool repeat = true;

    List<Matrix4x4> matrixes = new List<Matrix4x4>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //string[] files = Directory.GetFiles("C:\\Users\\janni\\Desktop\\Bachelor\\AssembleThemAll\\save");
        DirectoryInfo info = new DirectoryInfo(path);
        var files = info.GetFileSystemInfos().OrderBy(file => file.CreationTime); // sort by creation time, as this is easier than natural sort and works in this case

        for (int i = 0; i < 100; i++)
        {
            matrixes.Add(NDArrayToMatrix4x4(np.load(path)));
        }
            //Debug.Log($"{file.FullName}");
      
        print(matrixes);
        t = matrixes.Count - 1;
    }

    float LastTick = 0;
    private Matrix4x4 NDArrayToMatrix4x4(NDArray nDarray)
    {
        double[,] array = (double[,])nDarray.ToMuliDimArray<double>();
        Matrix4x4 matrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
        {
            for(int k  = 0; k < 4; k++)
            {
                matrix[i, k] = (float)array[i, k];
            }
        }
        return matrix;
    }

    int t;
    // Update is called once per frame
    void Update()
    {
        if ((Time.time - LastTick > tickTime) && (t>=0))
        {
            object1.transform.localPosition = matrixes[t].GetPosition();
            object1.transform.localRotation = matrixes[t--].rotation;

            //object1.transform.Translate(matrixes[t++].GetPosition());
            //object1.transform.Rotate(matrixes[t++].rotation.eulerAngles);
            LastTick = Time.time;
        }
        if ((t < 0) && repeat)
        {
            t = matrixes.Count - 1;
            object1.transform.localPosition = matrixes[t].GetPosition();
            object1.transform.localRotation = matrixes[t].rotation;
        }
        //Debug.Log(object1.transform.position);
    }
}
