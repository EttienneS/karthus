using Camera;
using UnityEngine;

public class CameraData
{
    public float X;
    public float Y;
    public float Z;

    public float RotationX;
    public float RotationY;
    public float RotationZ;

    public float ZoomX;
    public float ZoomY;
    public float ZoomZ;

    public CameraData()
    {
    }

    public CameraData(CameraController cameraRig)
    {
        X = cameraRig.transform.position.x;
        Y = cameraRig.transform.position.y;
        Z = cameraRig.transform.position.z;

        RotationX = cameraRig.transform.eulerAngles.x;
        RotationY = cameraRig.transform.eulerAngles.y;
        RotationZ = cameraRig.transform.eulerAngles.z;

        ZoomX = cameraRig.Camera.transform.position.x;
        ZoomY = cameraRig.Camera.transform.position.y;
        ZoomZ = cameraRig.Camera.transform.position.z;
    }

    public void Load(CameraController c)
    {
        c.transform.position = new Vector3(X, Y, Z);
        c.transform.eulerAngles = new Vector3(X, Y, Z);
        c.Camera.transform.position = new Vector3(ZoomX, ZoomY, ZoomZ);
    }
}