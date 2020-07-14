using UnityEngine;

namespace Camera
{
    public class CameraController : MonoBehaviour
    {
        public UnityEngine.Camera Camera;
        public float MaxAngle = 90;
        public float MinAngle = 20;
        public float SpeedMax = 2f;
        public float SpeedMin = 0.1f;
        public int ZoomMax = 15;
        public int ZoomMin = 2;
        public float ZoomSpeed = 5;

        private const int _zoomBound = 12;

        public void Start()
        {
            Camera = GetComponent<UnityEngine.Camera>();
        }

        public void Update()
        {
            if (Game.Instance == null || Game.Instance.Typing)
            {
                return;
            }

            //var rotation = new Vector3(0, 15, 0);
            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    Camera.transform.eulerAngles += rotation;
            //}
            //else if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    Camera.transform.eulerAngles -= rotation;
            //}

            (float xAxisInput, float zAxisInput, float yAxisInput) = GetCameraInput();

            if (xAxisInput != 0 || zAxisInput != 0 || yAxisInput != 0)
            {
                var newY = Mathf.Clamp(Camera.transform.position.y - (yAxisInput * ZoomSpeed), ZoomMin, ZoomMax);

                var yScaledSpeed = MathHelper.ScaleValueInRange(SpeedMin, SpeedMax, ZoomMin, ZoomMax, newY);

                var newX = Mathf.Clamp(transform.position.x + (xAxisInput * yScaledSpeed), Game.Instance.Map.MinX - _zoomBound, Game.Instance.Map.MaxX);
                var newZ = Mathf.Clamp(transform.position.z + (zAxisInput * yScaledSpeed), Game.Instance.Map.MinZ - _zoomBound, Game.Instance.Map.MaxZ);

                Camera.transform.position = new Vector3(newX, newY, newZ);
            }

            var scaledCameraTiltAngleX = Mathf.Lerp(MinAngle, MaxAngle, (transform.position.y - ZoomMin) / (ZoomMax - ZoomMin));
            Camera.transform.eulerAngles = new Vector3(scaledCameraTiltAngleX, Camera.transform.eulerAngles.y, 0);
        }

        private (float horizontal, float vertical, float zoom) GetCameraInput()
        {
            var horizontal = Input.GetAxis("Horizontal") / Time.timeScale;
            var vertical = Input.GetAxis("Vertical") / Time.timeScale;
            var zoom = Input.GetAxis("Mouse ScrollWheel");

            return (horizontal, vertical, zoom);
        }

        public void ViewPoint(Vector3 point)
        {
            var x = Mathf.Clamp(point.x, Game.Instance.Map.MinX - _zoomBound, Game.Instance.Map.MaxX);
            var y = Mathf.Clamp(point.y, ZoomMin, ZoomMax);
            var z = Mathf.Clamp(point.z - _zoomBound, Game.Instance.Map.MinZ - _zoomBound, Game.Instance.Map.MaxZ);

            Camera.transform.position = new Vector3(x, y, z);
        }

        internal void MoveToWorldCenter()
        {
            transform.position = new Vector3((Game.Instance.MapData.ChunkSize * Game.Instance.MapData.Size) / 2,
                                              20,
                                             (Game.Instance.MapData.ChunkSize * Game.Instance.MapData.Size) / 2);
        }
    }
}