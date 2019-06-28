using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class Util
{
    //去掉unity mesh里面重复的顶点
    public static List<Vector3> GetNoRepeatVertices(Mesh mesh)
    {
        HashSet<Vector3> set = new HashSet<Vector3>();

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            if (!set.Contains(mesh.vertices[i]))
            {
                set.Add(mesh.vertices[i]);
            }
        }

        return set.ToList();
    }
    
}
