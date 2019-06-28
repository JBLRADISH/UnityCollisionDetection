using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AABBCollider : MonoBehaviour
{
	public AABBStructureMode mode = AABBStructureMode.Accelerate;

	private AABB aabb;
	private Camera camera;

	// Use this for initialization
	void Start()
	{
		aabb = new AABB(transform);
		camera = new Camera(UnityEngine.Camera.main);
	}

	// Update is called once per frame
	void Update()
	{
		aabb.UpdateAABB(mode);
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = camera.ScreenPointToRay();
			RaycastHit info;
			if (aabb.RayDetection(ray, out info))
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
		if (aabb != null)
		{
			aabb.DrawAABB();
		}
	}
}
