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
	}

	private void OnDrawGizmos()
	{
		if (obb != null)
		{
			obb.DrawOBB();
		}
	}
}
