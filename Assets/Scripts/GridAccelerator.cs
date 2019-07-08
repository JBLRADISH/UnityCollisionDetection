using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAccelerator : MonoBehaviour
{

	private Grid grid;
	private CameraHelper cameraHelper;

	// Use this for initialization
	void Start()
	{
		grid = new Grid();
		cameraHelper = new CameraHelper(Camera.main);
	}

	// Update is called once per fram
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = cameraHelper.ScreenPointToRay();
			ray.DrawRay();
			RaycastHit info = new RaycastHit();
			if (grid.RayDetection(ray, info))
			{
				info.transform.GetComponent<Renderer>().material.color = Color.green;
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		if (grid != null)
		{
			grid.DrawGrid();
		}
	}
}
