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

	}

	void OnDrawGizmosSelected()
	{
		if (grid != null)
		{
			grid.DrawGrid();
		}
	}
}
