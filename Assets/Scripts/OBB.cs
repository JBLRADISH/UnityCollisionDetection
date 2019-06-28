using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBB
{

	private Transform transform;
	private Vector3 center;
	private Vector3[] axis;
	private Vector3 radius;
	private Matrix4x4 matrix;

	public OBB(Transform transform)
	{
		this.transform = transform;
		matrix = transform.localToWorldMatrix;
		StructureOBB(matrix);
	}

	void StructureOBB(Matrix4x4 local2world)
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

	public void UpdateOBB()
	{
		Matrix4x4 m = transform.localToWorldMatrix;
		if (MathUtil.IsOnlyContainTranslation(matrix, m))
		{
			Vector3 translation = MathUtil.GetTranslation(matrix, m);
			center += translation;
		}
		else
		{
			StructureOBB(m);
		}

		matrix = m;
	}

	public void DrawOBB()
	{
		Gizmos.color = Color.yellow;
		Vector3 min = center;
		Vector3 max = center;
		for (int i = 0; i < 3; i++)
		{
			min -= axis[i] * radius[i];
			max += axis[i] * radius[i];
		}

		Vector3 offset0 = axis[0] * (radius[0] * 2);
		Vector3 offset1 = axis[1] * (radius[1] * 2);
		Vector3 offset2 = axis[2] * (radius[2] * 2);
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
}
