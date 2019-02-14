using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectBox : MonoBehaviour
{
    public RectTransform selectSquareImage;

    Vector3 startPos;
    Vector3 endPos;

    void Start()
    {
        selectSquareImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                                        Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

            var hit = Physics2D.Raycast(rayPos, Vector2.zero, Mathf.Infinity);

            if (hit)
            {
                startPos = hit.point;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectSquareImage.gameObject.SetActive(false);
        }

        if (Input.GetMouseButton(0))
        {
            if (!selectSquareImage.gameObject.activeInHierarchy)
            {
                selectSquareImage.gameObject.SetActive(true);
            }
            endPos = Input.mousePosition;

            Vector3 start = Camera.main.WorldToScreenPoint(startPos);
            start.z = 0f;

            selectSquareImage.position = (start + endPos) / 2;

            float sizeX = Mathf.Abs(start.x - endPos.x);
            float sizeY = Mathf.Abs(start.y - endPos.y);

            selectSquareImage.sizeDelta = new Vector2(sizeX, sizeY);
        }
    }
}
