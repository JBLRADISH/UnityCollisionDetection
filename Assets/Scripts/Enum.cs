using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AABBStructureMode
{
    None,
    Accelerate,
    Compact
}

public enum SphereStructureMode
{
    Ritter,
    RitterIter,
    RitterEigen
}

public enum OBBStructureMode
{
    Eigen,
    AABB
}

public enum BVHSplitMethod
{
    SplitMiddle,
    SplitEqualCounts,
    SplitSAH
}
