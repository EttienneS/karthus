using System;
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
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

                if (plane.Raycast(ray, out float entry))
                {
                    dragStartPosition = ray.GetPoint(entry);
                }
            }

            if (Input.GetMouseButton(1))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

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

            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
            Camera.transform.localPosition = Vector3.Lerp(Camera.transform.localPosition, newZoom, Time.deltaTime * movementTime);
        }

        public void ViewPoint(Vector3 point)
        {
            const int zoomBound = 12;
            var x = Mathf.Clamp(point.x, Game.Instance.Map.MinX - zoomBound, Game.Instance.Map.MaxX);
            var y = 10;
            var z = Mathf.Clamp(point.z - zoomBound, Game.Instance.Map.MinZ - zoomBound, Game.Instance.Map.MaxZ);

            transform.position = new Vector3(x, y, z);
        }

        internal void MoveToWorldCenter()
        {
            transform.position = new Vector3((Game.Instance.MapData.ChunkSize * Game.Instance.MapData.Size) / 2,
                                              10,
                                             (Game.Instance.MapData.ChunkSize * Game.Instance.MapData.Size) / 2);

            newPosition = transform.position;
        }
    }
}