﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class AABB : Box
{
    private Vector3 originMin;
    private Vector3 originMax;
    public Vector3 transformMin;
    public Vector3 transformMax;
    private Matrix4x4 matrix;
    private AABBStructureMode mode;
    public int triangle;

    public static AABB Default
    {
        get { return new AABB(Vector3.positiveInfinity, Vector3.negativeInfinity); }
    }

    public AABB(Transform transform)
    {
        this.transform = transform;
        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        originMin = originMax = mesh.vertices[0];
        for (int i = 1; i < mesh.vertices.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (mesh.vertices[i][j] < originMin[j])
                    originMin[j] = mesh.vertices[i][j];
                if (mesh.vertices[i][j] > originMax[j])
                    originMax[j] = mesh.vertices[i][j];
            }
        }

        transformMin = originMin;
        transformMax = originMax;
        matrix = Matrix4x4.identity;
        mode = AABBStructureMode.None;
        UpdateAABB(AABBStructureMode.Compact);
    }

    public AABB(Vector3 min, Vector3 max)
    {
        transformMin = min;
        transformMax = max;
    }

    public void UpdateAABB(AABBStructureMode mode)
    {
        Matrix4x4 m = transform.localToWorldMatrix;
        //只发生平移的话不需要重新计算包围盒，直接加上偏移量即可
        if (MathUtil.IsOnlyContainTranslation(matrix, m) && this.mode == mode)
        {
            Vector3 translation = MathUtil.GetTranslation(matrix, m);
            transformMin += translation;
            transformMax += translation;
        }
        else
        {
            this.mode = mode;
            //加速模式：只对max和min进行变换，不保证计算得到的包围盒是紧凑的
            if (mode == AABBStructureMode.Accelerate)
            {
                Vector3 translate = new Vector3(m.m03, m.m13, m.m23);
                transformMin = translate;
                transformMax = translate;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        float e = m[j, i] * originMin[i];
                        float f = m[j, i] * originMax[i];
                        if (e < f)
                        {
                            transformMin[j] += e;
                            transformMax[j] += f;
                        }
                        else
                        {
                            transformMin[j] += f;
                            transformMax[j] += e;
                        }
                    }
                }
            }
            //紧凑模式：对所有顶点进行变换，计算得到的包围盒是紧凑的
            else if (mode == AABBStructureMode.Compact)
            {
                Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
                List<Vector3> vertices = Util.GetNoRepeatVertices(mesh);
                transformMin = transformMax = m * MathUtil.Vector4(vertices[0], 1);
                for (int i = 1; i < vertices.Count; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Vector3 tmp = m * MathUtil.Vector4(vertices[i], 1);
                        if (tmp[j] < transformMin[j])
                            transformMin[j] = tmp[j];
                        if (tmp[j] > transformMax[j])
                            transformMax[j] = tmp[j];
                    }
                }
            }
        }

        matrix = m;
    }

    public void DrawAABB()
    {
        Vector3 offset0 = Vector3.right * (transformMax - transformMin).x;
        Vector3 offset1 = Vector3.up * (transformMax - transformMin).y;
        Vector3 offset2 = Vector3.forward * (transformMax - transformMin).z;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transformMin, transformMin + offset0);
        Gizmos.DrawLine(transformMin, transformMin + offset1);
        Gizmos.DrawLine(transformMin, transformMin + offset2);
        Gizmos.DrawLine(transformMin + offset0, transformMax - offset2);
        Gizmos.DrawLine(transformMin + offset1, transformMax - offset2);
        Gizmos.DrawLine(transformMin + offset0, transformMax - offset1);
        Gizmos.DrawLine(transformMin + offset2, transformMax - offset1);
        Gizmos.DrawLine(transformMin + offset1, transformMax - offset0);
        Gizmos.DrawLine(transformMin + offset2, transformMax - offset0);
        Gizmos.DrawLine(transformMax, transformMax - offset0);
        Gizmos.DrawLine(transformMax, transformMax - offset1);
        Gizmos.DrawLine(transformMax, transformMax - offset2);
    }

    public override bool RayDetection(Ray ray, RaycastHit hitInfo)
    {
        float[] t = new float[3];
        for (int i = 0; i < 3; i++)
        {
            if (ray.origin[i] < transformMin[i])
            {
                t[i] = (transformMin[i] - ray.origin[i]);
                if (t[i] > ray.distance * ray.direction[i])
                {
                    return false;
                }

                t[i] /= ray.direction[i];
            }
            else if (ray.origin[i] > transformMax[i])
            {
                t[i] = (transformMax[i] - ray.origin[i]);
                if (t[i] < ray.distance * ray.direction[i])
                {
                    return false;
                }

                t[i] /= ray.direction[i];
            }
            else
            {
                t[i] = -1;
            }
        }

        int idx = 0;
        for (int i = 1; i < 3; i++)
        {
            if (t[i] > t[idx])
            {
                idx = i;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (i == idx)
            {
                continue;
            }

            float tmp = ray.origin[i] + ray.direction[i] * t[idx];
            if (tmp < transformMin[i] || tmp > transformMax[i])
            {
                return false;
            }
        }

        ray.distance = t[idx];
        hitInfo.point = ray.origin + ray.direction * t[idx];
        hitInfo.transform = transform;
        return true;
    }

    public bool RayDetection(Ray ray, ref float dist)
    {
        float[] t = new float[3];
        for (int i = 0; i < 3; i++)
        {
            if (ray.origin[i] < transformMin[i])
            {
                t[i] = (transformMin[i] - ray.origin[i]);
                if (t[i] > ray.distance * ray.direction[i])
                {
                    return false;
                }

                t[i] /= ray.direction[i];
            }
            else if (ray.origin[i] > transformMax[i])
            {
                t[i] = (transformMax[i] - ray.origin[i]);
                if (t[i] < ray.distance * ray.direction[i])
                {
                    return false;
                }

                t[i] /= ray.direction[i];
            }
            else
            {
                t[i] = -1;
            }
        }

        int idx = 0;
        for (int i = 1; i < 3; i++)
        {
            if (t[i] > t[idx])
            {
                idx = i;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (i == idx)
            {
                continue;
            }

            float tmp = ray.origin[i] + ray.direction[i] * t[idx];
            if (tmp < transformMin[i] || tmp > transformMax[i])
            {
                return false;
            }
        }

        dist = t[idx];
        return true;
    }

    public bool RayDetection(Ray ray)
    {
        float t = 0;
        return RayDetection(ray, ref t);
    }

    public override bool BoxDetection(Box box)
    {
        if (box is AABB)
        {
            return Util.TestAABBAABB(this, box as AABB);
        }
        else if (box is Sphere)
        {
            return Util.TestAABBSphere(this, box as Sphere);
        }
        else if (box is OBB)
        {
            return Util.TestAABBOBB(this, box as OBB);
        }

        return false;
    }

    //找到AABB上离Point最近的一点
    public Vector3 ClosestPoint(Vector3 point)
    {
        for (int i = 0; i < 3; i++)
        {
            if (point[i] < transformMin[i])
            {
                point[i] = transformMin[i];
            }
            else if (point[i] > transformMax[i])
            {
                point[i] = transformMax[i];
            }
        }

        return point;
    }

    public OBB GetOBB()
    {
        Vector3[] axis = new[] {Vector3.right, Vector3.up, Vector3.forward};
        return new OBB(axis, (transformMax + transformMin) * 0.5f, (transformMax - transformMin) * 0.5f);
    }

    public AABB Union(AABB aabb)
    {
        for (int i = 0; i < 3; i++)
        {
            if (aabb.transformMin[i] < transformMin[i])
            {
                transformMin[i] = aabb.transformMin[i];
            }

            if (aabb.transformMax[i] > transformMax[i])
            {
                transformMax[i] = aabb.transformMax[i];
            }
        }

        return this;
    }

    public AABB Union(Vector3 point)
    {
        for (int i = 0; i < 3; i++)
        {
            if (point[i] < transformMin[i])
            {
                transformMin[i] = point[i];
            }

            if (point[i] > transformMax[i])
            {
                transformMax[i] = point[i];
            }
        }

        return this;
    }

    public Vector3 GetCentroid()
    {
        return (transformMin + transformMax) * 0.5f;
    }

    public int MaximumExtent()
    {
        int idx = 0;
        for (int i = 1; i < 3; i++)
        {
            if (transformMax[i] - transformMin[i] > transformMax[idx] - transformMin[idx])
            {
                idx = i;
            }
        }

        return idx;
    }

    public float GetSurfaceArea()
    {
        Vector3 d = transformMax - transformMin;
        return 2 * (d.x * d.y + d.x * d.z + d.y * d.z);
    }

    public override AABB OuterAABB()
    {
        return Clone<AABB>();
    }

}
