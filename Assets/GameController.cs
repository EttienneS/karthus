using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public bool BuildMode;
    public Cell SelectedCell;
    private static GameController _instance;

    private Cell lastClickedCell;

    public static GameController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("GameController").GetComponent<GameController>();
            }

            return _instance;
        }
    }

    private static void DestroyAndRecreateMap()
    {
        foreach (var cell in MapGrid.Instance.Map)
        {
            Destroy(cell.gameObject);
        }

        foreach (var creature in CreatureController.Instance.Creatures)
        {
            Destroy(creature.gameObject);
        }
        CreatureController.Instance.Creatures.Clear();

        MapEditor.Instance.Generating = false;
    }

    private void Update()
    {
        //if (Input.GetButtonDown("Fire1"))
        //{
        //    if (Time.timeScale == 1.0f)
        //        Time.timeScale = 0.0f;
        //    else
        //        Time.timeScale = 1.0f;

        //    // Adjust fixed delta time according to timescale
        //    // The fixed delta time will now be 0.02 frames per real-time second
        //    Time.fixedDeltaTime = 0.02f * Time.timeScale;
        //}

        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                                     Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

            var hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);
            if (hit)
            {
                var clickedCell = MapGrid.Instance.GetCellAtPoint(hit.point);

                if (lastClickedCell == null || clickedCell != lastClickedCell)
                {
                    SelectedCell?.DisableBorder();

                    SelectedCell = clickedCell;
                    SelectedCell.EnableBorder(Color.red);

                    if (OrderSelectionController.Instance.CellClicked != null)
                    {
                        OrderSelectionController.Instance.CellClicked.Invoke(SelectedCell);
                    }

                    lastClickedCell = clickedCell;
                }
            }
        }
    }
}