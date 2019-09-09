using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera Camera;
    public float Speed = 1;

    public float SpeedMax = 2f;
    public float SpeedMin = 0.1f;
    public int ZoomMax = 15;
    public int ZoomMin = 2;
    public float ZoomStep = 5;

    public float RenderHeight = 2.5f;
    public float RenderWidth = 4f;
    private float _journeyLength;
    private Vector3 _panDesitnation;

    private bool _panning;

    private Vector3 _panSource;

    private float _startTime;

    public void MoveToViewPoint(Vector3 panDesitnation)
    {
        _startTime = Time.time;
        _panSource = transform.position;
        _panDesitnation = panDesitnation;
        _journeyLength = Vector3.Distance(_panSource, _panDesitnation);

        _panning = true;
    }

    public void Start()
    {
        Camera = GetComponent<Camera>();
    }

    internal void MoveToCell(Cell cell)
    {
        MoveToViewPoint(cell.ToMapVector());
    }

    private void Update()
    {
        bool moved = false;
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
            moved = true;
        }
        else
        {
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

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

            //End of mobile platform dependendent compilation section started above with #elif
            // todo: clamp the camera to stop it from moving off screen
            //var x = transform.position.x;
            //var y = transform.position.y;

            //transform.position = new Vector3(x, y, z);

            // move camera to match with change in FOV
            // RotateAndScale(oldFov);

#else
            float horizontal = Input.GetAxis("Horizontal") / Time.timeScale;
            float vertical = Input.GetAxis("Vertical") / Time.timeScale;
            float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
            if (horizontal != 0 || vertical != 0 || mouseWheel != 0)
            {
                var step = Mathf.Clamp((int)Game.TimeManager.TimeStep, 1, 8);
                var x = Mathf.Clamp(transform.position.x + (horizontal * Speed * step), 0, Game.MapGrid.Width);
                var y = Mathf.Clamp(transform.position.y + (vertical * Speed * step), 0, Game.MapGrid.Height);
                transform.position = new Vector3(x, y, transform.position.z);

                Camera.orthographicSize = Mathf.Clamp(Camera.orthographicSize - (mouseWheel * ZoomStep),
                    ZoomMin, ZoomMax);

                Speed = Helpers.ScaleValueInRange(SpeedMin, SpeedMax, ZoomMin, ZoomMax, Camera.orthographicSize);
                ZoomStep = Mathf.Max(2f, Camera.orthographicSize / 2f);
                moved = true;
            }
#endif
        }

        if (moved)
        {
            var width = Mathf.CeilToInt(Camera.orthographicSize * RenderWidth);
            var height = Mathf.CeilToInt(Camera.orthographicSize * RenderHeight);
            Game.MapGrid.Refresh(new RectInt(
                                 Mathf.CeilToInt(transform.position.x - (width / 2)),
                                 Mathf.CeilToInt(transform.position.y - (height / 2)),
                                 width,
                                 height));
        }
    }
}