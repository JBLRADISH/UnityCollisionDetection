using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AABBCollider : BoxCollider
{
	public AABBStructureMode mode = AABBStructureMode.Compact;

	private AABB aabb;

	// Use this for initialization
	void Awake()
	{
		aabb = new AABB(transform);
		box = aabb;
	}

	// Update is called once per frame
	void Update()
	{
		aabb.UpdateAABB(mode);
	}

	void OnDrawGizmos()
	{
		if (aabb != null)
		{
			aabb.DrawAABB();
		}
	}
}
