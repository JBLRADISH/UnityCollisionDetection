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

    public static bool TestOBBSphere(OBB obb, Sphere sphere)
    {
        sphere = sphere.Clone<Sphere>();
        sphere.center = obb.RTMatrix * MathUtil.Vector4(sphere.center, 1);
        return TestAABBSphere(obb.GetAABB(), sphere);
    }

    public static bool TestOBBOBB(OBB obb1, OBB obb2)
    {
        //This algorithm comes from Separating Axis Theorem for Oriented Bounding Boxes

        //设置B到A的旋转矩阵
        Matrix4x4 r = Matrix4x4.identity;
        Matrix4x4 absR = Matrix4x4.identity;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                r[i, j] = Vector3.Dot(obb1.axis[i], obb2.axis[j]);
                absR[i, j] = Mathf.Abs(r[i, j]) + float.Epsilon;
            }
        }

        //t为A到B的位移向量
        Vector3 t = obb2.transformCenter - obb1.transformCenter;
        t = new Vector3(Vector3.Dot(obb1.axis[0], t), Vector3.Dot(obb1.axis[1], t), Vector3.Dot(obb1.axis[2], t));

        float ra, rb;

        //测试分离轴 obb1.axis[0], obb1.axis[1], obb1.axis[2]
        for (int i = 0; i < 3; i++)
        {
            ra = obb1.transformRadius[i];
            rb = obb2.transformRadius[0] * absR[i, 0] + obb2.transformRadius[1] * absR[i, 1] +
                 obb2.transformRadius[2] * absR[i, 2];
            if (Mathf.Abs(t[i]) > ra + rb)
            {
                return false;
            }
        }

        //测试分离轴 obb2.axis[0], obb2.axis[1], obb2.axis[2]
        for (int i = 0; i < 3; i++)
        {
            ra = obb1.transformRadius[0] * absR[0, i] + obb1.transformRadius[1] * absR[1, i] +
                 obb1.transformRadius[2] * absR[2, i];
            rb = obb2.transformRadius[i];
            if (Mathf.Abs(t[0] * r[0, i] + t[1] * r[1, i] + t[2] * r[2, i]) > ra + rb)
            {
                return false;
            }
        }

        //测试分离轴 obb1.axis[0] X obb2.axis[0]
        ra = obb1.transformRadius[1] * absR[2, 0] + obb1.transformRadius[2] * absR[1, 0];
        rb = obb2.transformRadius[1] * absR[0, 2] + obb2.transformRadius[2] * absR[0, 1];
        if (Mathf.Abs(t[2] * absR[1, 0] - t[1] * absR[2, 0]) > ra + rb)
        {
            return false;
        }

        //测试分离轴 obb1.axis[0] X obb2.axis[1]
        ra = obb1.transformRadius[1] * absR[2, 1] + obb1.transformRadius[2] * absR[1, 1];
        rb = obb2.transformRadius[0] * absR[0, 2] + obb2.transformRadius[2] * absR[0, 0];
        if (Mathf.Abs(t[2] * absR[1, 1] - t[1] * absR[2, 1]) > ra + rb)
        {
            return false;
        }

        //测试分离轴 obb1.axis[0] X obb2.axis[2]
        ra = obb1.transformRadius[1] * absR[2, 2] + obb1.transformRadius[2] * absR[1, 2];
        rb = obb2.transformRadius[0] * absR[0, 1] + obb2.transformRadius[1] * absR[0, 0];
        if (Mathf.Abs(t[2] * absR[1, 2] - t[1] * absR[2, 2]) > ra + rb)
        {
            return false;
        }

        //测试分离轴 obb1.axis[1] X obb2.axis[0]
        ra = obb1.transformRadius[0] * absR[2, 0] + obb1.transformRadius[2] * absR[0, 0];
        rb = obb2.transformRadius[1] * absR[1, 2] + obb2.transformRadius[2] * absR[1, 1];
        if (Mathf.Abs(t[0] * absR[2, 0] - t[2] * absR[0, 0]) > ra + rb)
        {
            return false;
        }

        //测试分离轴 obb1.axis[1] X obb2.axis[1]
        ra = obb1.transformRadius[0] * absR[2, 1] + obb1.transformRadius[2] * absR[0, 1];
        rb = obb2.transformRadius[0] * absR[1, 2] + obb2.transformRadius[2] * absR[1, 0];
        if (Mathf.Abs(t[0] * absR[2, 1] - t[2] * absR[0, 1]) > ra + rb)
        {
            return false;
        }

        //测试分离轴 obb1.axis[1] X obb2.axis[2]
        ra = obb1.transformRadius[0] * absR[2, 2] + obb1.transformRadius[2] * absR[0, 2];
        rb = obb2.transformRadius[0] * absR[1, 1] + obb2.transformRadius[1] * absR[1, 0];
        if (Mathf.Abs(t[0] * absR[2, 2] - t[2] * absR[0, 2]) > ra + rb)
        {
            return false;
        }

        //测试分离轴 obb1.axis[2] X obb2.axis[0]
        ra = obb1.transformRadius[0] * absR[1, 0] + obb1.transformRadius[1] * absR[0, 0];
        rb = obb2.transformRadius[1] * absR[2, 2] + obb2.transformRadius[2] * absR[2, 1];
        if (Mathf.Abs(t[1] * absR[0, 0] - t[0] * absR[1, 0]) > ra + rb)
        {
            return false;
        }

        //测试分离轴 obb1.axis[2] X obb2.axis[1]
        ra = obb1.transformRadius[0] * absR[1, 1] + obb1.transformRadius[1] * absR[0, 1];
        rb = obb2.transformRadius[0] * absR[2, 2] + obb2.transformRadius[2] * absR[2, 0];
        if (Mathf.Abs(t[1] * absR[0, 1] - t[0] * absR[1, 1]) > ra + rb)
        {
            return false;
        }

        //测试分离轴 obb1.axis[2] X obb2.axis[2]
        ra = obb1.transformRadius[0] * absR[1, 2] + obb1.transformRadius[1] * absR[0, 2];
        rb = obb2.transformRadius[0] * absR[2, 1] + obb2.transformRadius[1] * absR[2, 0];
        if (Mathf.Abs(t[1] * absR[0, 2] - t[0] * absR[1, 2]) > ra + rb)
        {
            return false;
        }

        return true;
    }

}
