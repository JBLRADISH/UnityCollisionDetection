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
    private BVHSplitMethod bvhSplitMethod;
    private BVHNode root;

    //transform为null则构建基于场景的粗略测试bvh, 不为null则构建基于对象的精细测试bvh
    public BVH(Transform transform, BVHSplitMethod bvhSplitMethod)
    {
        this.transform = transform;
        this.bvhSplitMethod = bvhSplitMethod;
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
            int split = 0;
            if (Mathf.Abs(centroidAABB.transformMax[dim] - centroidAABB.transformMin[dim]) <= float.Epsilon)
            {
                split = (start + end) / 2;
            }
            else
            {
                if (bvhSplitMethod == BVHSplitMethod.SplitMiddle)
                {
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
                            Swap(aabbs, left, right);
                        }

                        left++;
                        right--;
                    } while (left < right);

                    split = left;
                }
                else if (bvhSplitMethod == BVHSplitMethod.SplitEqualCounts)
                {
                    split = (start + end) / 2;
                    QuickSelect(aabbs, split, start, end - 1, dim);
                }
            }

            bvhNode.leftChild = BuildBVH(aabbs, start, split);
            bvhNode.rightChild = BuildBVH(aabbs, split, end);
            bvhNode.aabb = bvhNode.leftChild.aabb.Union(bvhNode.rightChild.aabb, false);
            return bvhNode;
        }
    }

    void QuickSelect(AABB[] aabbs, int k, int left, int right, int dim)
    {
        float pivot = Median3(aabbs, left, right, dim);
        int i = left;
        int j = right;
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
                Swap(aabbs, left, right);
            }

            left++;
            right--;
        } while (left < right);

        if (k <= right)
        {
            QuickSelect(aabbs, k, i, right, dim);
        }
        else if (k >= left)
        {
            QuickSelect(aabbs, k, left, j, dim);
        }
    }

    void Swap(AABB[] aabbs, int i, int j)
    {
        AABB tmp = aabbs[i];
        aabbs[i] = aabbs[j];
        aabbs[j] = tmp;
    }

    float Median3(AABB[] aabbs, int left, int right, int dim)
    {
        int mid = (left + right) / 2;
        if (aabbs[left].GetCentroid()[dim] > aabbs[mid].GetCentroid()[dim])
        {
            Swap(aabbs, left, mid);
        }

        if (aabbs[left].GetCentroid()[dim] > aabbs[right].GetCentroid()[dim])
        {
            Swap(aabbs, left, right);
        }

        if (aabbs[mid].GetCentroid()[dim] > aabbs[right].GetCentroid()[dim])
        {
            Swap(aabbs, mid, right);
        }

        return aabbs[mid].GetCentroid()[dim];
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
