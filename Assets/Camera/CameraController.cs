using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera Camera;
    public float MaxAngle = 90;
    public float MinAngle = 20;
    public float SpeedMax = 2f;
    public float SpeedMin = 0.1f;
    public int ZoomMax = 15;
    public int ZoomMin = 2;
    public float ZoomSpeed = 5;

    internal float Speed;

    private const int _zoomBound = 12;
    private float _journeyLength;
    private Vector3 _panDesitnation;
    private bool _panning;
    private Vector3 _panSource;
    private float _startTime;

    public void Start()
    {
        Camera = GetComponent<Camera>();
    }

    public void Update()
    {
        if (_panning)
        {
            var distCovered = (Time.time - _startTime) * 1000;
            var fracJourney = distCovered / _journeyLength;

            var lerp = Vector3.Lerp(_panSource, _panDesitnation, fracJourney);

            transform.position = new Vector3(lerp.x, -15, lerp.y);

            if (transform.position.x == _panDesitnation.x
                && transform.position.z == _panDesitnation.z)
            {
                _panning = false;
            }
        }
        else
        {
            if (Game.Instance == null || Game.Instance.Typing)
            {
                return;
            }

            var rotation = new Vector3(0, 15, 0);
            if (Input.GetKeyDown(KeyCode.E))
            {
                Camera.transform.eulerAngles += rotation;
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                Camera.transform.eulerAngles -= rotation;
            }

            var horizontal = Input.GetAxis("Horizontal") / Time.timeScale;
            var vertical = Input.GetAxis("Vertical") / Time.timeScale;
            var mouseWheel = Input.GetAxis("Mouse ScrollWheel");
            if (horizontal != 0 || vertical != 0 || mouseWheel != 0)
            {
                var y = Mathf.Clamp(Camera.transform.position.y - (mouseWheel * ZoomSpeed), ZoomMin, ZoomMax);
                Speed = Helpers.ScaleValueInRange(SpeedMin, SpeedMax, ZoomMin, ZoomMax, y);

                var x = Mathf.Clamp(transform.position.x + (horizontal * Speed), Game.Instance.Map.MinX - _zoomBound, Game.Instance.Map.MaxX);
                var z = Mathf.Clamp(transform.position.z + (vertical * Speed), Game.Instance.Map.MinZ - _zoomBound, Game.Instance.Map.MaxZ);

                Camera.transform.position = new Vector3(x, y, z);
            }

            var xrot = Mathf.Lerp(MinAngle, MaxAngle, (transform.position.y - ZoomMin) / (ZoomMax - ZoomMin));
            Camera.transform.eulerAngles = new Vector3(xrot, Camera.transform.eulerAngles.y, 0);
        }
    }
    public void ViewPoint(Vector3 point)
    {
        var x = Mathf.Clamp(point.x, Game.Instance.Map.MinX - _zoomBound, Game.Instance.Map.MaxX);
        var y = Mathf.Clamp(point.y, ZoomMin, ZoomMax);
        var z = Mathf.Clamp(point.z - _zoomBound, Game.Instance.Map.MinZ - _zoomBound, Game.Instance.Map.MaxZ);

        Camera.transform.position = new Vector3(x, y, z);
    }
}