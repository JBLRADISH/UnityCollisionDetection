using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBBCollider : MonoBehaviour
{
	public OBBStructureMode mode = OBBStructureMode.AABB;

	private OBB obb;
	private Camera camera;

	// Use this for initialization
	void Start()
	{
		obb = new OBB(transform, mode);
		camera = new Camera(UnityEngine.Camera.main);
	}

	// Update is called once per frame
	void Update()
	{
		obb.UpdateOBB(mode);
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = camera.ScreenPointToRay();
			RaycastHit info;
			if (obb.RayDetection(ray, out info))
			{
				GetComponent<Renderer>().material.color = Color.red;
				Debug.DrawLine(camera.transform.position, info.point, Color.green, 10);
			}
			else
			{
				GetComponent<Renderer>().material.color = Color.white;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (obb != null)
		{
			obb.DrawOBB();
		}
	}
}
