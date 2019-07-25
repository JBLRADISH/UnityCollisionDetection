using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollider : BoxCollider
{

	public SphereStructureMode mode = SphereStructureMode.RitterIter;

	private Sphere sphere;

	// Use this for initialization
	void Awake()
	{
		sphere = new Sphere(transform, mode);
		box = sphere;
	}

	// Update is called once per frame
	void Update()
	{
		sphere.UpdateSphere(mode);
	}
	
	void OnDrawGizmos()
	{
		if (sphere != null)
		{
			sphere.DrawSphere();
		}
	}
}
