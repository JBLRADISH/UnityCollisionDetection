﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ray
{
    public Vector3 origin;
    public Vector3 direction;
    public float distance;

    public Ray(Vector3 origin, Vector3 direction, float distance)
    {
        this.origin = origin;
        this.direction = direction.normalized;
        this.distance = distance;
    }

    public bool Raycast(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Vector3 e0 = p0 - p1;
        Vector3 e1 = p1 - p2;
        Vector3 n = Vector3.Cross(e0, e1);
        float dot = Vector3.Dot(n, direction);
        if (dot >= 0)
        {
            return false;
        }

        float d = Vector3.Dot(n, p0);
        float t = (d - Vector3.Dot(origin, n)) / dot;
        if (t < 0 || t > distance)
        {
            return false;
        }

        Vector3 p = origin + direction * t;
        Vector3 e2 = p2 - p0;
        Vector3 d0 = p0 - p;
        Vector3 d2 = p2 - p;
        float inverseNN = 1 / Vector3.Dot(n, n);
        float b0 = Vector3.Dot(Vector3.Cross(e1, d2), n) * inverseNN;
        if (b0 < 0 || b0 > 1)
        {
            return false;
        }

        float b1 = Vector3.Dot(Vector3.Cross(e2, d0), n) * inverseNN;
        if (b1 < 0 || b1 > 1)
        {
            return false;
        }

        float b2 = 1 - b0 - b1;
        if (b2 < 0 || b2 > 1)
        {
            return false;
        }

        if (t > distance)
        {
            return false;
        }
        else
        {
            distance = t;
            return true;
        }
    }

    public void Transform(Matrix4x4 m)
    {
        origin = m * MathUtil.Vector4(origin, 1);
        direction = m * direction;
        direction.Normalize();
    }

    public void DrawRay()
    {
        Debug.DrawLine(origin, origin + direction * distance, Color.green, 10);
    }
}

public class RaycastHit
{
    public Vector3 point;
    public Transform transform;
}
