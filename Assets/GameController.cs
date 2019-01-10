using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Cell SelectedCell;
    private static GameController _instance;

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

    public bool BuildMode;

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

    public void Build()
    {
        var btn = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        if (btn.image.color != Color.red)
        {
            selectedSprite = btn.image.sprite;
            btn.image.color = Color.red;
        }
        else
        {
            selectedSprite = null;
            btn.image.color = Color.white;
        }
    }

    private Sprite selectedSprite;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
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

                if (clickedCell == SelectedCell)
                {
                    foreach (var creature in CreatureController.Instance.Creatures)
                    {
                        creature.SetTarget(clickedCell);
                    }
                    SelectedCell.EnableBorder(Color.magenta);
                }
                else
                {
                    if (SelectedCell != null)
                    {
                        SelectedCell.DisableBorder();
                    }

                    SelectedCell = clickedCell;
                    SelectedCell.EnableBorder(Color.red);

                    if (selectedSprite != null)
                    {
                        SelectedCell.Content.sprite = selectedSprite;
                        SelectedCell.TravelCost = -1;
                    }
                }

            }
        }
    }
}