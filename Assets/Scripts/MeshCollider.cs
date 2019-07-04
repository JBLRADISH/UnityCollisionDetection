using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

public class MeshCollider : MonoBehaviour
{

	public BVHSplitMethod bvhSplitMethod = BVHSplitMethod.SplitSAH;

	private BVH bvh;
	private CameraHelper cameraHelper;

	// Use this for initialization
	void Start()
	{
		bvh = new BVH(transform, bvhSplitMethod, 4);
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
				transform.GetComponent<Renderer>().material.color = Color.green;
				Debug.DrawLine(cameraHelper.transform.position, info.point, Color.red, 100);
			}
			else
			{
				transform.GetComponent<Renderer>().material.color = Color.white;
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
