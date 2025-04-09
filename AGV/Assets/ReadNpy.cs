using UnityEngine;
using NumSharp;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;


public class ReadNpy : MonoBehaviour
{
    public GameObject object1, object2;
    public float tickTime;
    public string path;
    public bool repeat = true;

    List<Matrix4x4> matrixes = new List<Matrix4x4>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("1");
        //string[] files = Directory.GetFiles("C:\\Users\\janni\\Desktop\\Bachelor\\AssembleThemAll\\save");
        DirectoryInfo info = new DirectoryInfo(path);
        Debug.Log("2");
        var data = np.Load_Npz<Array>(path);
        foreach ( var item in data)
        {
            matrixes.Add(NDArrayToMatrix4x4(item.Value));
        }
       
        //Debug.Log($"{file.FullName}");
        Debug.Log("4");
        print(matrixes);
        Debug.Log("5");
        t = matrixes.Count - 1;
        Debug.Log("6");
    }

    float LastTick = 0;
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

    int t;
    // Update is called once per frame
    void Update()
    {
        if ((Time.time - LastTick > tickTime) && (t >= 0))
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

