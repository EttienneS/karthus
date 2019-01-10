using System.IO;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera Camera;
    [Range(0.1f, 5)] public float Speed = 2;
    [Range(5, 25)] public int ZoomMax = 15;
    [Range(1, 50)] public int ZoomMin = 2;
    [Range(1, 20)] public int ZoomStep = 5;
    private static CameraController _instance;

    private float _journeyLength;
    private Vector3 _panDesitnation;

    private bool _panning;

    private Vector3 _panSource;

    private float _startTime;

    public static CameraController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("Main Camera").GetComponent<CameraController>();
            }

            return _instance;
        }
    }

    public void Load(string location)
    {
        var path = Path.Combine(location, "camera.data");

        using (var reader = new BinaryReader(File.Open(path, FileMode.Open)))
        {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();

            transform.position = new Vector3(x, y, z);
        }
    }

    public void MoveToViewPoint(Vector3 panDesitnation)
    {
        _startTime = Time.time;
        _panSource = transform.position;
        _panDesitnation = panDesitnation;
        _journeyLength = Vector3.Distance(_panSource, _panDesitnation);

        _panning = true;
    }

    public void Save(string location)
    {
        var path = Path.Combine(location, "camera.data");
        using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            writer.Write(transform.position.x);
            writer.Write(transform.position.y);
            writer.Write(transform.position.z);
        }
    }

    public void Start()
    {
        Camera = GetComponent<Camera>();
    }

    internal void MoveToCell(Cell cell)
    {
        MoveToViewPoint(cell.transform.position);
    }

    private void Update()
    {
        if (_panning)
        {
            var distCovered = (Time.time - _startTime) * 1000;
            var fracJourney = distCovered / _journeyLength;

            var lerp = Vector3.Lerp(_panSource, _panDesitnation, fracJourney);

            transform.position = new Vector3(lerp.x, lerp.y, -10);

            if (transform.position.x == _panDesitnation.x
                && transform.position.y == _panDesitnation.y)
            {
                _panning = false;
            }
        }
        else
        {
#if UNITY_STANDALONE || UNITY_WEBPLAYER
            float horizontal = 0;
            float vertical = 0;
            var z = transform.position.z;

            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            transform.position = new Vector3(transform.position.x + horizontal * Speed,
                transform.position.y + vertical * Speed, z);

            Camera.orthographicSize = Mathf.Clamp(Camera.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * ZoomStep,
                ZoomMin, ZoomMax);

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            var zoomSpeed = 0.4f;
            var scrollSpeed = 0.05f;
            if (Input.touchCount > 0)
            {
                var touchZero = Input.GetTouch(0);

                if (Input.touchCount == 2)
                {
                    var touchOne = Input.GetTouch(1);

                    var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    var prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    var touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    var deltaMagnitudeDiff = (prevTouchDeltaMag - touchDeltaMag) * zoomSpeed;
                }
                else
                {
                    if (touchZero.phase == TouchPhase.Moved)
                    {
                        var touchDeltaPosition = touchZero.deltaPosition;
                        transform.Translate(-touchDeltaPosition.x * scrollSpeed, -touchDeltaPosition.y * scrollSpeed, 0);
                    }
                }
            }

#endif //End of mobile platform dependendent compilation section started above with #elif

            // todo: clamp the camera to stop it from moving off screen
            //var x = transform.position.x;
            //var y = transform.position.y;

            //transform.position = new Vector3(x, y, z);

            // move camera to match with change in FOV
            // RotateAndScale(oldFov);
        }
    }
}