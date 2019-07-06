using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
	List<AABB> aabbs;

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
	Vector3 min;
	Vector3 unitWidth;

	public Grid()
	{
		BoxCollider[] boxColliders = GameObject.FindObjectsOfType<BoxCollider>();
		AABB aabb = AABB.Default;
		AABB[] aabbs = new AABB[boxColliders.Length];
		for (int i = 0; i < boxColliders.Length; i++)
		{
			aabbs[i] = boxColliders[i].box.OuterAABB();
			aabb.Union(aabbs[i]);
		}

		min = aabb.transformMin;

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

		float[] invUnitWidth = new float[3];
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
				voxelMin[j] = Point2Voxel(aabb, aabbs[i].transformMin, j, invUnitWidth);
				voxelMax[j] = Point2Voxel(aabb, aabbs[i].transformMax, j, invUnitWidth);
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

	int Point2Voxel(AABB aabb, Vector3 point, int axis, float[] invUnitWidth)
	{
		int voxel = (int) ((point[axis] - aabb.transformMin[axis]) * invUnitWidth[axis]);
		return Mathf.Clamp(voxel, 0, voxels.GetLength(axis) - 1);
	}

	public void DrawGrid()
	{
		for (int i = 0; i < voxels.GetLength(0); i++)
		{
			for (int j = 0; j < voxels.GetLength(1); j++)
			{
				for (int k = 0; k < voxels.GetLength(2); k++)
				{
					Vector3 min = this.min + new Vector3(i * unitWidth[0], j * unitWidth[1], k * unitWidth[2]);
					Vector3 max = min + unitWidth;
					AABB aabb = new AABB(min, max);
					aabb.DrawAABB();
				}
			}
		}
	}
}
