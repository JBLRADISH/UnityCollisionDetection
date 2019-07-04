using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCollider : MonoBehaviour
{

	public BVHSplitMethod bvhSplitMethod = BVHSplitMethod.SplitSAH;

	private BVH bvh;

	// Use this for initialization
	void Start()
	{
		bvh = new BVH(transform, bvhSplitMethod, 4);
		bvh.CreateBVH();
	}

	// Update is called once per fram
	void Update()
	{

	}

	void OnDrawGizmos()
	{
		if (bvh != null)
		{
			bvh.DrawBVH();
		}
	}
}
