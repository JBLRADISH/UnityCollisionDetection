using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollider : MonoBehaviour
{

	public SphereStructureMode mode = SphereStructureMode.RitterIter;

	private Sphere sphere;
	private Camera camera;

	// Use this for initialization
	void Start()
	{
		sphere = new Sphere(transform, mode);
		camera = new Camera(UnityEngine.Camera.main);
	}

	// Update is called once per frame
	void Update()
	{
		sphere.UpdateSphere(mode);
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = camera.ScreenPointToRay();
			RaycastHit info;
			if (sphere.RayDetection(ray, out info))
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
		if (sphere != null)
		{
			sphere.DrawSphere();
		}
	}
}
