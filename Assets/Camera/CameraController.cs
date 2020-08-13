using UnityEngine;

namespace Camera
{
    public class CameraController : MonoBehaviour
    {
        private Transform _followTransform;

        public UnityEngine.Camera Camera;

        public float normalSpeed;
        public float fastSpeed;
        public float movementSpeed;
        public float movementTime;
        public float rotationAmount;
        public Vector3 zoomAmount;

        public float minZoom;
        public float maxZoom;

        public Vector3 newPosition;
        public Quaternion newRotation;
        public Vector3 newZoom;

        public Vector3 dragStartPosition;
        public Vector3 dragCurrentPosition;

        public Vector3 rotateStartPosition;
        public Vector3 rotateCurrentPosition;

        public void Start()
        {
            newPosition = transform.position;
            newRotation = transform.rotation;
            newZoom = Camera.transform.localPosition;
        }

        public void Update()
        {
            if (_followTransform != null)
            {
                transform.position = _followTransform.position;
            }
            else
            {
                HandleMouseInput();
                HandleMovementInput();
            }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StopFollowing();
            }
        }

        public void FollowTransform(Transform transform)
        {
            if (_followTransform == transform)
            {
                StopFollowing();
            }
            else
            {
                _followTransform = transform;
            }
        }

        public void StopFollowing()
        {
            _followTransform = null;
        }

        private void HandleMouseInput()
        {
            // https://www.youtube.com/watch?v=rnqF6S7PfFA&t=212s (from 12:00)
            if (Input.mouseScrollDelta.y != 0)
            {
                newZoom += Input.mouseScrollDelta.y * zoomAmount;
            }

            if (Input.GetMouseButtonDown(1))
            {
                var plane = new Plane(Vector3.up, Vector3.zero);
                var ray = Camera.ScreenPointToRay(Input.mousePosition);

                if (plane.Raycast(ray, out float entry))
                {
                    dragStartPosition = ray.GetPoint(entry);
                }
            }

            if (Input.GetMouseButton(1))
            {
                var plane = new Plane(Vector3.up, Vector3.zero);
                var ray = Camera.ScreenPointToRay(Input.mousePosition);

                if (plane.Raycast(ray, out float entry))
                {
                    dragCurrentPosition = ray.GetPoint(entry);
                    newPosition = transform.position + dragStartPosition - dragCurrentPosition;
                }
            }

            if (Input.GetMouseButtonDown(2))
            {
                rotateStartPosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(2))
            {
                rotateCurrentPosition = Input.mousePosition;

                var diff = rotateStartPosition - rotateCurrentPosition;
                rotateStartPosition = rotateCurrentPosition;

                newRotation *= Quaternion.Euler(Vector3.up * (-diff.x / 5));
            }
        }

        private void HandleMovementInput()
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                movementSpeed = fastSpeed;
            }
            else
            {
                movementSpeed = normalSpeed;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                newPosition += (transform.forward * movementSpeed);
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                newPosition += (transform.forward * -movementSpeed);
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                newPosition += (transform.right * movementSpeed);
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                newPosition += (transform.right * -movementSpeed);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
            }
            if (Input.GetKey(KeyCode.E))
            {
                newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
            }

            if (Input.GetKey(KeyCode.R))
            {
                newZoom += zoomAmount;
            }
            if (Input.GetKey(KeyCode.F))
            {
                newZoom -= zoomAmount;
            }

            transform.position = ClampPosition(Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime));
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
            Camera.transform.localPosition = ClampZoom(Vector3.Lerp(Camera.transform.localPosition, newZoom, Time.deltaTime * movementTime));
        }

        private Vector3 ClampPosition(Vector3 position)
        {
            return new Vector3(Mathf.Clamp(position.x, Map.Instance.MinX - 5f, Map.Instance.MaxX),
                               position.y,
                               Mathf.Clamp(position.z, Map.Instance.MinZ - 5f, Map.Instance.MaxZ));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0, 0, 0.75f);
            Gizmos.DrawCube(transform.position, new Vector3(0.25f, 0.25f, 0.25f));
        }

        private Vector3 ClampZoom(Vector3 zoom)
        {
            return new Vector3(zoom.x,
                               Mathf.Clamp(zoom.y, minZoom, maxZoom),
                               Mathf.Clamp(zoom.z, -maxZoom, -minZoom));
        }

        public void ViewPoint(Vector3 point)
        {
            const int zoomBound = 12;
            var x = Mathf.Clamp(point.x, Map.Instance.MinX - zoomBound, Map.Instance.MaxX);
            var y = 1;
            var z = Mathf.Clamp(point.z - zoomBound, Map.Instance.MinZ - zoomBound, Map.Instance.MaxZ);

            transform.position = new Vector3(x, y, z);
        }

        internal void MoveToWorldCenter()
        {
            transform.position = new Vector3((Game.Instance.MapData.ChunkSize * Game.Instance.MapData.Size) / 2,
                                              1,
                                             (Game.Instance.MapData.ChunkSize * Game.Instance.MapData.Size) / 2);

            newPosition = transform.position;
        }
    }
}