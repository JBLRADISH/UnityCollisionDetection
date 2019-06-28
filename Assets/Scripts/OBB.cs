using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBB
{

	private Transform transform;
	private Vector3 center;
	private Vector3[] axis;
	private Vector3 radius;
	private Vector3 transformCenter;
	private Vector3 transformRadius;
	private Matrix4x4 matrix;
	private OBBStructureMode mode;

	public OBB(Transform transform, OBBStructureMode mode)
	{
		this.transform = transform;
		this.mode = mode;
		switch (mode)
		{
			case OBBStructureMode.Eigen:
				matrix = transform.localToWorldMatrix;
				break;
			case OBBStructureMode.AABB:
				matrix = Matrix4x4.identity;
				break;
		}

		StructureOBB(mode, matrix);
	}

	void StructureOBB(OBBStructureMode mode, Matrix4x4 local2world)
	{
		switch (mode)
		{
			case OBBStructureMode.Eigen:
				EigenOBB(local2world);
				break;
			case OBBStructureMode.AABB:
				AABBOBB();
				break;
		}

		transformCenter = center;
		transformRadius = radius;
	}

	void EigenOBB(Matrix4x4 local2world)
	{
		Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
		List<Vector3> vertices = Util.GetNoRepeatVertices(mesh);
		for (int i = 0; i < vertices.Count; i++)
		{
			vertices[i] = local2world * MathUtil.Vector4(vertices[i], 1);
		}

		Matrix4x4 covarianceM = MathUtil.GetCovarianceMatrix(vertices);
		Matrix4x4 eigenM = MathUtil.Jacobi(ref covarianceM);
		MathUtil.SchmidtOrthogonalization(ref eigenM);

		axis = new Vector3[3];
		for (int i = 0; i < 3; i++)
		{
			axis[i] = eigenM.GetColumn(i);
		}

		Vector3 min = Vector3.positiveInfinity;
		Vector3 max = Vector3.negativeInfinity;
		for (int i = 1; i < vertices.Count; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				float tmp = Vector3.Dot(axis[j], vertices[i]);
				if (tmp < min[j])
				{
					min[j] = tmp;
				}

				if (tmp > max[j])
				{
					max[j] = tmp;
				}
			}
		}

		radius = (max - min) * 0.5f;

		center = Vector3.zero;
		for (int i = 0; i < 3; i++)
		{
			center += (radius + min)[i] * axis[i];
		}
	}

	void AABBOBB()
	{
		Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
		Vector3 originMin = mesh.vertices[0];
		Vector3 originMax = originMin;
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

		axis = new Vector3[3];
		for (int i = 0; i < 3; i++)
		{
			Vector3 tmp = Vector3.zero;
			tmp[i] = 1;
			axis[i] = tmp;
		}

		center = (originMax + originMin) * 0.5f;

		radius = (originMax - originMin) * 0.5f;

	}

	public void UpdateOBB(OBBStructureMode mode)
	{
		Matrix4x4 m = transform.localToWorldMatrix;
		if (MathUtil.IsOnlyContainTranslation(matrix, m) && this.mode == mode)
		{
			Vector3 translation = MathUtil.GetTranslation(matrix, m);
			transformCenter += translation;
		}
		else
		{
			if (mode == OBBStructureMode.Eigen)
			{
				StructureOBB(mode, m);
			}
			else if (mode == OBBStructureMode.AABB)
			{
				if (this.mode != mode)
				{
					StructureOBB(mode, m);
				}

				for (int i = 0; i < 3; i++)
				{
					Vector3 tmp = Vector3.zero;
					tmp[i] = 1;
					axis[i] = m * tmp;
				}

				transformCenter = m * MathUtil.Vector4(center, 1);

				Vector3 scale;
				if (MathUtil.IsContainScale(m, out scale))
				{
					for (int i = 0; i < 3; i++)
					{
						axis[i] /= scale[i];
						transformRadius[i] = radius[i] * scale[i];
					}
				}
				else
				{
					transformRadius = radius;
				}
			}

			this.mode = mode;
		}

		matrix = m;
	}

	public void DrawOBB()
	{
		Gizmos.color = Color.yellow;
		Vector3 min = transformCenter;
		Vector3 max = transformCenter;
		for (int i = 0; i < 3; i++)
		{
			min -= axis[i] * transformRadius[i];
			max += axis[i] * transformRadius[i];
		}

		Vector3 offset0 = axis[0] * (transformRadius[0] * 2);
		Vector3 offset1 = axis[1] * (transformRadius[1] * 2);
		Vector3 offset2 = axis[2] * (transformRadius[2] * 2);
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(min, min + offset0);
		Gizmos.DrawLine(min, min + offset1);
		Gizmos.DrawLine(min, min + offset2);
		Gizmos.DrawLine(min + offset0, max - offset2);
		Gizmos.DrawLine(min + offset1, max - offset2);
		Gizmos.DrawLine(min + offset0, max - offset1);
		Gizmos.DrawLine(min + offset2, max - offset1);
		Gizmos.DrawLine(min + offset1, max - offset0);
		Gizmos.DrawLine(min + offset2, max - offset0);
		Gizmos.DrawLine(max, max - offset0);
		Gizmos.DrawLine(max, max - offset1);
		Gizmos.DrawLine(max, max - offset2);
	}

	public bool RayDetection(Ray ray, out RaycastHit hitInfo)
	{
		hitInfo = new RaycastHit();

		AABB aabb = new AABB(transform, -transformRadius, transformRadius);

		Matrix4x4 t = Matrix4x4.identity;
		t.SetColumn(3, MathUtil.Vector4(-transformCenter, 1));
		Matrix4x4 r = Matrix4x4.identity;
		Matrix4x4 s = Matrix4x4.identity;
		for (int i = 0; i < 3; i++)
		{
			r.SetRow(i, axis[i]);
			s[i, i] = radius[i] / transformRadius[i];
		}

		Matrix4x4 m = s * r * t;

		Ray aabbRay = ray.Clone();
		aabbRay.origin = m * MathUtil.Vector4(ray.origin, 1);
		aabbRay.direction = m * ray.direction;
		aabbRay.direction.Normalize();

		bool res = aabb.AABBRayDetection(aabbRay);
		if (res)
		{
			return ray.Raycast(transform, hitInfo);
		}

		return res;
	}
}
