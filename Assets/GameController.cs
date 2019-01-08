using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            foreach (var cell in MapGrid.Instance.Map)
            {
                Destroy(cell.gameObject);
            }

            MapEditor.Instance.CreateMap();
        }
    }
}
