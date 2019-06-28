using System.Collections;
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

    public bool Raycast(Transform transform, RaycastHit hitInfo)
    {
        Vector3 localOrigin = transform.worldToLocalMatrix * MathUtil.Vector4(origin, 1);
        Vector3 localDirection = transform.worldToLocalMatrix * direction;
        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        float minT = float.MaxValue;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p0 = mesh.vertices[mesh.triangles[i]];
            Vector3 p1 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 p2 = mesh.vertices[mesh.triangles[i + 2]];
            Vector3 e0 = p0 - p1;
            Vector3 e1 = p1 - p2;
            Vector3 n = Vector3.Cross(e0, e1);
            float dot = Vector3.Dot(n, localDirection);
            if (dot >= 0)
            { 
                continue;
            }

            float d = Vector3.Dot(n, p0);
            float t = (d - Vector3.Dot(localOrigin, n)) / dot;
            if (t < 0 || t > distance || t >= minT)
            {
                continue;
            }

            Vector3 p = localOrigin + localDirection * t;
            Vector3 e2 = p2 - p0;
            Vector3 d0 = p0 - p;
            Vector3 d2 = p2 - p;
            float inverseNN = 1 / Vector3.Dot(n, n);
            float b0 = Vector3.Dot(Vector3.Cross(e1, d2), n) * inverseNN;
            if (b0 < 0 || b0 > 1)
            {
                continue;
            }

            float b1 = Vector3.Dot(Vector3.Cross(e2, d0), n) * inverseNN;
            if (b1 < 0 || b1 > 1)
            {
                continue;
            }

            float b2 = 1 - b0 - b1;
            if (b2 < 0 || b2 > 1)
            {
                continue;
            }

            minT = t;
        }

        if (minT > distance)
        {
            return false;
        }
        else
        {
            Vector3 p = localOrigin + localDirection * minT;
            hitInfo.point = transform.localToWorldMatrix * MathUtil.Vector4(p, 1);
            return true;
        }
    }
}

public class RaycastHit
{
    public Vector3 point;
}
