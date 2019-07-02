using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVHAccelerator : MonoBehaviour
{

	private BVH bvh;

	// Use this for initialization
	void Start()
	{
		bvh = new BVH(null);
		bvh.CreateBVH();
	}

	// Update is called once per fram

	void OnDrawGizmos()
	{
		if (bvh != null)
		{
			bvh.DrawBVH();
		}
	}
}
