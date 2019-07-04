using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVHAccelerator : MonoBehaviour
{

	public BVHSplitMethod bvhSplitMethod = BVHSplitMethod.SplitSAH;

	private BVH bvh;
	private CameraHelper cameraHelper;

	// Use this for initialization
	void Start()
	{
		bvh = new BVH(null, bvhSplitMethod, 1);
		bvh.CreateBVH();
		cameraHelper = new CameraHelper(Camera.main);
	}

	// Update is called once per fram
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = cameraHelper.ScreenPointToRay();
			RaycastHit info;
			if (bvh.RayDetection(ray, out info))
			{
				info.transform.GetComponent<Renderer>().material.color = Color.green;
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		if (bvh != null)
		{
			bvh.DrawBVH();
		}
	}
}
