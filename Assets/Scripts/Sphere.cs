using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class Sphere : Box
{
    public Vector3 center;
    public float radius;
    private Matrix4x4 matrix;
    private SphereStructureMode mode;

    public Sphere(Transform transform, SphereStructureMode mode)
    {
        this.transform = transform;
        this.mode = mode;
        matrix = transform.localToWorldMatrix;
        StructureSphere(mode, matrix);
    }

    void StructureSphere(SphereStructureMode mode, Matrix4x4 local2world)
    {
        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        List<Vector3> vertices = Util.GetNoRepeatVertices(mesh);
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = local2world * MathUtil.Vector4(vertices[i], 1);
        }

        switch (mode)
        {
            case SphereStructureMode.Ritter:
                RitterSphere(vertices);
                break;
            case SphereStructureMode.RitterIter:
                RitterIterSphere(vertices);
                break;
            case SphereStructureMode.RitterEigen:
                RitterEigenSphere(vertices);
                break;
        }
    }

    void RitterSphere(List<Vector3> vertices)
    {
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        for (int i = 1; i < vertices.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (vertices[i][j] < vertices[(int) min[j]][j])
                {
                    min[j] = i;
                }

                if (vertices[i][j] > vertices[(int) max[j]][j])
                {
                    max[j] = i;
                }
            }
        }

        float[] dist2 = new float[3];
        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = vertices[(int) max[i]] - vertices[(int) min[i]];
            dist2[i] = Vector3.Dot(offset, offset);
        }

        int idx = 0;
        for (int i = 1; i < 3; i++)
        {
            if (dist2[i] > dist2[idx])
            {
                idx = i;
            }
        }

        center = (vertices[(int) max[idx]] + vertices[(int) min[idx]]) * 0.5f;
        radius = Vector3.Distance(vertices[(int) max[idx]], center);

        ApproachSphere(this, vertices);

    }

    void ApproachSphere(Sphere sphere, List<Vector3> vertices, bool random = false)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (random && i < vertices.Count - 1)
            {
                int randomIdx = Random.Range(i + 1, vertices.Count);
                Vector3 tmp = vertices[randomIdx];
                vertices[randomIdx] = vertices[i];
                vertices[i] = tmp;
            }

            Vector3 d = vertices[i] - sphere.center;
            float d2 = Vector3.Dot(d, d);
            if (d2 > sphere.radius * sphere.radius)
            {
                float dist = Mathf.Sqrt(d2);
                float newRadius = (sphere.radius + dist) * 0.5f;
                float k = (newRadius - sphere.radius) / dist;
                sphere.radius = newRadius;
                sphere.center += d * k;
            }
        }
    }

    void RitterIterSphere(List<Vector3> vertices, int iter = 8, float shrink = 0.95f)
    {
        RitterSphere(vertices);
        Sphere sphere = Clone<Sphere>();
        for (int i = 0; i < iter; i++)
        {
            sphere.radius *= shrink;
            ApproachSphere(sphere, vertices, true);
            if (sphere.radius < radius)
            {
                radius = sphere.radius;
                center = sphere.center;
            }
        }
    }

    void RitterEigenSphere(List<Vector3> vertices)
    {
        Matrix4x4 covarianceM = MathUtil.GetCovarianceMatrix(vertices);
        Matrix4x4 eigenM = MathUtil.Jacobi(ref covarianceM);
        MathUtil.EigenSort(ref eigenM, ref covarianceM);
        MathUtil.SchmidtOrthogonalization(ref eigenM);
        //得到具有最大特征值的主轴
        Vector3 axis = eigenM.GetColumn(0);
        int min = 0;
        int max = 0;
        float dmin = Vector3.Dot(axis, vertices[0]);
        float dmax = dmin;
        for (int i = 1; i < vertices.Count; i++)
        {
            float tmp = Vector3.Dot(axis, vertices[i]);
            if (tmp < dmin)
            {
                min = i;
                dmin = tmp;
            }

            if (tmp > dmax)
            {
                max = i;
                dmax = tmp;
            }
        }

        center = (vertices[max] + vertices[min]) * 0.5f;
        radius = Vector3.Distance(vertices[max], center);

        ApproachSphere(this, vertices);
    }

    public void UpdateSphere(SphereStructureMode mode)
    {
        Matrix4x4 m = transform.localToWorldMatrix;
        //发生缩放需要重构包围球
        if (MathUtil.IsContainScale(matrix, m) || this.mode != mode)
        {
            StructureSphere(mode, m);
            this.mode = mode;
        }
        else
        {
            //旋转不需要改变包围球
            //平移只需把球心偏移即可
            Vector3 translation = MathUtil.GetTranslation(matrix, m);
            center += translation;
        }

        matrix = m;
    }

    public void DrawSphere()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, radius);
    }

    public override bool RayDetection(Ray ray, out RaycastHit hitInfo)
    {
        hitInfo = new RaycastHit();

        Vector3 e = center - ray.origin;

        float a = Vector3.Dot(e, ray.direction);
        if (a <= 0)
        {
            return false;
        }

        float f2 = radius * radius - Vector3.Dot(e, e) + a * a;
        if (f2 < 0)
        {
            return false;
        }

        float t = a - Mathf.Sqrt(f2);
        if (t < 0 || t > ray.distance)
        {
            return false;
        }

        hitInfo.point = ray.origin + ray.direction * t;
        hitInfo.transform = transform;
        return true;
    }

    public override bool BoxDetection(Box box)
    {
        if (box is Sphere)
        {
            return Util.TestSphereSphere(this, box as Sphere);
        }
        else if (box is AABB)
        {
            return Util.TestAABBSphere(box as AABB, this);
        }
        else if (box is OBB)
        {
            return Util.TestOBBSphere(box as OBB, this);
        }

        return false;
    }

    public Sphere Union(Sphere sphere)
    {
        Vector3 d = sphere.center - center;
        float dist2 = Vector3.Dot(d, d);
        if ((sphere.radius - radius) * (sphere.radius - radius) >= dist2)
        {
            if (sphere.radius > radius)
            {
                center = sphere.center;
                radius = sphere.radius;
            }
        }
        else
        {
            float dist = Mathf.Sqrt(dist2);
            float oldRadius = radius;
            radius = (dist + oldRadius + sphere.radius) * 0.5f;
            center += (radius - oldRadius) / dist * d;
        }

        return this;
    }

    public override AABB OuterAABB()
    {
        AABB aabb = new AABB(center - Vector3.one * radius, center + Vector3.one * radius);
        aabb.transform = transform;
        return aabb;
    }
}
