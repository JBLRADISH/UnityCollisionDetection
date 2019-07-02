using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class BVHNode
{
    public AABB aabb;
    public BVHNode leftChild;
    public BVHNode rightChild;

    public BVHNode(AABB aabb)
    {
        this.aabb = aabb;
        leftChild = null;
        rightChild = null;
    }

    public BVHNode()
    {
        
    }
}

//默认构造AABB BVH 
public class BVH
{
    private Transform transform;
    private BVHNode root;

    //transform为null则构建基于场景的粗略测试bvh, 不为null则构建基于对象的精细测试bvh
    public BVH(Transform transform)
    {
        this.transform = transform;
    }

    public void CreateBVH()
    {
        if (transform == null)
        {
            MeshFilter[] meshFilters = GameObject.FindObjectsOfType<MeshFilter>();
            AABB[] aabbs = new AABB[meshFilters.Length];
            for (int i = 0; i < meshFilters.Length; i++)
            {
                AABB aabb = new AABB(meshFilters[i].transform);
                aabb.UpdateAABB(AABBStructureMode.Compact);
                aabbs[i] = aabb;
            }

            root = BuildBVH(aabbs, 0, aabbs.Length);
        }
    }

    BVHNode BuildBVH(AABB[] aabbs, int start, int end)
    {
        if (start == end - 1)
        {
            return new BVHNode(aabbs[start]);
        }
        else
        {
            BVHNode bvhNode = new BVHNode();
            AABB centroidAABB = new AABB(Vector3.positiveInfinity, Vector3.negativeInfinity);
            for (int i = start; i < end; i++)
            {
                centroidAABB.Union(aabbs[i].GetCentroid(), true);
            }

            int dim = centroidAABB.MaximumExtent();
            if (Mathf.Abs(centroidAABB.transformMax[dim] - centroidAABB.transformMin[dim]) <= float.Epsilon)
            {
                int mid = (start + end) / 2;
                bvhNode.leftChild = BuildBVH(aabbs, 0, mid);
                bvhNode.rightChild = BuildBVH(aabbs, mid, end);
                bvhNode.aabb = bvhNode.leftChild.aabb.Union(bvhNode.rightChild.aabb, false);
                return bvhNode;
            }

            float pivot = centroidAABB.GetCentroid()[dim];
            int left = start;
            int right = end - 1;
            do
            {
                while (pivot > aabbs[left].GetCentroid()[dim])
                {
                    left++;
                }

                while (pivot < aabbs[right].GetCentroid()[dim])
                {
                    right--;
                }

                if (left > right)
                {
                    break;
                }

                if (left < right)
                {
                    AABB tmp = aabbs[left];
                    aabbs[left] = aabbs[right];
                    aabbs[right] = tmp;
                }

                left++;
                right--;
            } while (left < right);

            bvhNode.leftChild = BuildBVH(aabbs, 0, left);
            bvhNode.rightChild = BuildBVH(aabbs, left, end);
            bvhNode.aabb = bvhNode.leftChild.aabb.Union(bvhNode.rightChild.aabb, false);
            return bvhNode;
        }
    }

    public void DrawBVH()
    {
        DrawBVH(root);
    }

    void DrawBVH(BVHNode bvhNode)
    {
        if (bvhNode == null)
        {
            return;
        }

        bvhNode.aabb.DrawAABB();
        DrawBVH(bvhNode.leftChild);
        DrawBVH(bvhNode.rightChild);
    }
}
