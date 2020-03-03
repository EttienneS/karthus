using UnityEngine;

public class CameraData
{
    public float X;
    public float Y;
    public float Z;

    public float Zoom;

    public CameraData()
    {
    }

    public CameraData(Camera c)
    {
        X = c.transform.position.x;
        Y = c.transform.position.y;
        Z = c.transform.position.z;

        Zoom = c.orthographicSize;
    }

    public void Load(Camera c)
    {
        c.transform.position = new Vector3(X, Y, Z);
        c.orthographicSize = Zoom;
    }
}