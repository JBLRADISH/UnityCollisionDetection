using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

public class MeshCollider : BoxCollider
{

	public BVHSplitMethod bvhSplitMethod = BVHSplitMethod.SplitSAH;

	private BVH bvh;

	// Use this for initialization
	void Awake()
	{
		bvh = new BVH(transform, bvhSplitMethod, 4);
		bvh.CreateBVH();
		box = bvh;
	}

	void OnDrawGizmosSelected()
	{
		if (bvh != null)
		{
			bvh.DrawBVH();
		}
	}
}
