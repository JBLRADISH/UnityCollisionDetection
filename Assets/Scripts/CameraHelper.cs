using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHelper
{

    private Camera camera;

    public Transform transform;

    public CameraHelper(Camera camera)
    {
        this.camera = camera;
        transform = camera.transform;
    }

    public Ray ScreenPointToRay()
    {
        Vector2 uv = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height) -
                     Vector2.one * 0.5f;
        //注意tan的参数是弧度
        float y = camera.nearClipPlane * Mathf.Tan(camera.fieldOfView / 360 * Mathf.PI) * 2;
        float x = camera.aspect * y;
        //注意观察空间和世界空间的Z轴是反向的，所以z值要取反
        Vector4 p = new Vector4(x * uv.x, y * uv.y, -camera.nearClipPlane, 1);
        Vector3 wp = camera.cameraToWorldMatrix * p;
        Ray ray = new Ray(camera.transform.position, (wp - camera.transform.position), 1000);
        return ray;
    }
}
