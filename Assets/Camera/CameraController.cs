using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera Camera;
    internal float Speed;

    public float SpeedMin = 0.1f;
    public float SpeedMax = 2f;
    public int ZoomMax = 15;
    public int ZoomMin = 2;
    public float ZoomSpeed = 5;

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
        MoveToViewPoint(cell.Vector);
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

            float horizontal = Input.GetAxis("Horizontal") / Time.timeScale;
            float vertical = Input.GetAxis("Vertical") / Time.timeScale;
            float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
            if (horizontal != 0 || vertical != 0 || mouseWheel != 0)
            {
                var y = Mathf.Clamp(Camera.transform.position.y - (mouseWheel * ZoomSpeed * Time.deltaTime), ZoomMin, ZoomMax);
                Speed = Helpers.ScaleValueInRange(SpeedMin, SpeedMax, ZoomMin, ZoomMax, y);

                var x = Mathf.Clamp(transform.position.x + (horizontal * Speed * Time.deltaTime), Game.Instance.Map.MinX - 12, Game.Instance.Map.MaxX);
                var z = Mathf.Clamp(transform.position.z + (vertical * Speed * Time.deltaTime), Game.Instance.Map.MinZ - 12, Game.Instance.Map.MaxZ);


                Camera.transform.position = new Vector3(x, y, z);
            }
        }
    }
}