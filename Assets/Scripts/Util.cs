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

    public static bool TestAABBAABB(AABB aabb1, AABB aabb2)
    {
        for (int i = 0; i < 3; i++)
        {
            if (aabb1.transformMin[i] > aabb2.transformMax[i] || aabb1.transformMax[i] < aabb2.transformMin[i])
                return false;
        }

        return true;
    }

    public static bool TestSphereSphere(Sphere sphere1, Sphere sphere2)
    {
        Vector3 d = sphere2.center - sphere1.center;
        float rSum = sphere1.radius + sphere2.radius;
        return Vector3.Dot(d, d) <= rSum * rSum;
    }


    public static bool TestAABBSphere(AABB aabb, Sphere sphere)
    {
        Vector3 p = aabb.ClosestPoint(sphere.center);
        Vector3 d = sphere.center - p;
        return Vector3.Dot(d, d) <= sphere.radius * sphere.radius;
    }

}
