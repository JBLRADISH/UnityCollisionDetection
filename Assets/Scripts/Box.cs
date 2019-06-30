using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Box
{
    public Transform transform;

    public abstract bool RayDetection(Ray ray, out RaycastHit hitInfo);

    public abstract bool BoxDetection(Box box);

    public abstract Box Union(Box box, bool reference);

    public T Clone<T>() where T : Box
    {
        return MemberwiseClone() as T;
    }

}
