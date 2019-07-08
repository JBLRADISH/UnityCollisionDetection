using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
	public List<AABB> aabbs;

	public void Add(AABB aabb)
	{
		if (aabbs == null)
		{
			aabbs = new List<AABB>();
		}

		if (!aabbs.Contains(aabb))
		{
			aabbs.Add(aabb);
		}
	}
}

public class Grid
{
	Voxel[,,] voxels;
	AABB aabb;
	Vector3 unitWidth;
	Vector3 invUnitWidth;

	public Grid()
	{
		BoxCollider[] boxColliders = GameObject.FindObjectsOfType<BoxCollider>();
		aabb = AABB.Default;
		AABB[] aabbs = new AABB[boxColliders.Length];
		for (int i = 0; i < boxColliders.Length; i++)
		{
			aabbs[i] = boxColliders[i].box.OuterAABB();
			aabb.Union(aabbs[i]);
		}

		//计算单位距离的体素数量
		Vector3 delta = aabb.transformMax - aabb.transformMin;
		int dim = aabb.MaximumExtent();
		float dimVoxel = 3 * Mathf.Pow((float) aabbs.Length, 1.0f / 3);
		float unitVoxel = dimVoxel / delta[dim];
		int[] nVoxel = new int[3];
		for (int i = 0; i < 3; i++)
		{
			nVoxel[i] = (int) (delta[i] * unitVoxel);
			nVoxel[i] = Mathf.Clamp(nVoxel[i], 1, 64);
		}

		voxels = new Voxel[nVoxel[0], nVoxel[1], nVoxel[2]];

		for (int i = 0; i < 3; i++)
		{
			unitWidth[i] = delta[i] / voxels.GetLength(i);
			invUnitWidth[i] = unitWidth[i] <= float.Epsilon ? 0 : 1 / unitWidth[i];
		}

		int[] voxelMin = new int[3];
		int[] voxelMax = new int[3];
		for (int i = 0; i < aabbs.Length; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				voxelMin[j] = Point2Voxel(aabbs[i].transformMin, j);
				voxelMax[j] = Point2Voxel(aabbs[i].transformMax, j);
			}

			for (int x = voxelMin[0]; x <= voxelMax[0]; x++)
			{
				for (int y = voxelMin[1]; y <= voxelMax[1]; y++)
				{
					for (int z = voxelMin[2]; z <= voxelMax[2]; z++)
					{
						Voxel voxel = GetVoxel(x, y, z);
						voxel.Add(aabbs[i]);
					}
				}
			}
		}
	}

	Voxel GetVoxel(int x, int y, int z)
	{
		Voxel voxel = voxels[x, y, z];
		if (voxel == null)
		{
			voxel = new Voxel();
			voxels[x, y, z] = voxel;
		}

		return voxel;
	}

	int Point2Voxel(Vector3 point, int axis)
	{
		int voxel = (int) ((point[axis] - aabb.transformMin[axis]) * invUnitWidth[axis]);
		return Mathf.Clamp(voxel, 0, voxels.GetLength(axis) - 1);
	}

	float Voxel2Point(int voxel, int axis)
	{
		return aabb.transformMin[axis] + unitWidth[axis] * voxel;
	}

	public void DrawGrid()
	{
		for (int i = 0; i < voxels.GetLength(0); i++)
		{
			for (int j = 0; j < voxels.GetLength(1); j++)
			{
				for (int k = 0; k < voxels.GetLength(2); k++)
				{
					Vector3 min = this.aabb.transformMin +
					              new Vector3(i * unitWidth[0], j * unitWidth[1], k * unitWidth[2]);
					Vector3 max = min + unitWidth;
					AABB aabb = new AABB(min, max);
					aabb.DrawAABB();
				}
			}
		}
	}

	public bool RayDetection(Ray ray, RaycastHit hitInfo)
	{
		float t = 0;
		if (!aabb.RayDetection(ray, ref t))
		{
			return false;
		}

		Vector3 p = ray.origin + ray.direction * t;

		int[] step = new int[3];
		int[] end = new int[3];
		int[] vp = new int[3];
		float[] deltaT = new float[3];
		float[] nextT = new float[3];
		for (int i = 0; i < 3; i++)
		{
			vp[i] = Point2Voxel(p, i);
			if (ray.direction[i] >= 0)
			{
				step[i] = 1;
				end[i] = voxels.GetLength(i);
				deltaT[i] = unitWidth[i] / ray.direction[i];
				nextT[i] = t + (Voxel2Point(vp[i] + 1, i) - p[i]) / ray.direction[i];
			}
			else
			{
				step[i] = -1;
				end[i] = -1;
				deltaT[i] = -unitWidth[i] / ray.direction[i];
				nextT[i] = t + (Voxel2Point(vp[i], i) - p[i]) / ray.direction[i];
			}
		}

		HashSet<AABB> hs = new HashSet<AABB>();
		bool hit = false;
		while (true)
		{
			Voxel voxel = voxels[vp[0], vp[1], vp[2]];
			if (voxel != null && voxel.aabbs != null)
			{
				for (int i = 0; i < voxel.aabbs.Count; i++)
				{
					AABB tmp = voxel.aabbs[i];
					if (!hs.Contains(aabb))
					{
						hs.Add(tmp);
						hit |= tmp.transform.GetComponent<BoxCollider>().box
							.RayDetection(ray, hitInfo);
					}
				}
			}

			int bits = ((nextT[0] < nextT[1] ? 1 : 0) << 2) + ((nextT[0] < nextT[2] ? 1 : 0) << 1) +
			           nextT[1] < nextT[2] ? 1 : 0;
			int[] cmpToAxis = {2, 1, 2, 1, 2, 2, 0, 0};
			int stepAxis = cmpToAxis[bits];
			if (ray.distance < nextT[stepAxis])
				break;
			vp[stepAxis] += step[stepAxis];
			if (vp[stepAxis] == end[stepAxis])
				break;
			nextT[stepAxis] += deltaT[stepAxis];
		}

		return hit;
	}
}

