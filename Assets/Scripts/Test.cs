using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
	private CameraHelper cameraHelper;

	void Start()
	{
		cameraHelper = new CameraHelper(Camera.main);
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			TestRayDetection();
		}
		else if (Input.GetKeyDown(KeyCode.A))
		{
			TestBoxDetection();
		}
	}

	void TestRayDetection()
	{
		Ray ray = cameraHelper.ScreenPointToRay();
		ray.DrawRay();
		RaycastHit info;
		BoxCollider[] boxColliders = GameObject.FindObjectsOfType<BoxCollider>();
		for (int i = 0; i < boxColliders.Length; i++)
		{
			if (boxColliders[i].box.RayDetection(ray, out info))
			{
				boxColliders[i].transform.GetComponent<Renderer>().material.color = Color.green;
			}
			else
			{
				boxColliders[i].transform.GetComponent<Renderer>().material.color = Color.white;
			}
		}
	}

	void TestBoxDetection()
	{
		BoxCollider[] boxColliders = GameObject.FindObjectsOfType<BoxCollider>();
		for (int i = 0; i < boxColliders.Length - 1; i++)
		{
			for (int j = i + 1; j < boxColliders.Length; j++)
			{
				if (boxColliders[i].box.BoxDetection(boxColliders[j].box))
				{
					Debug.DrawLine(boxColliders[i].transform.position, boxColliders[j].transform.position, Color.green,
						10);
				}
			}
		}
	}
}
