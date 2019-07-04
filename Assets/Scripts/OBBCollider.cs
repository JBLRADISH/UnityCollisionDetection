using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBBCollider : BoxCollider
{
	public OBBStructureMode mode = OBBStructureMode.AABB;

	private OBB obb;

	// Use this for initialization
	void Awake()
	{
		obb = new OBB(transform, mode);
		box = obb;
	}

	// Update is called once per frame
	void Update()
	{
		obb.UpdateOBB(mode);
	}

	void OnDrawGizmosSelected()
	{
		if (obb != null)
		{
			obb.DrawOBB();
		}
	}
}
