using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CellInfoPanel : MonoBehaviour
{
    public Text CellName;
    public Text CellContent;

    private bool _firstRun = true;

    public void Start()
    {
        if (_firstRun)
        {
            var children = GetComponentsInChildren<Text>().ToList();
            CellName = children.First(t => t.name == "CellName");
            CellContent = children.First(t => t.name == "CellContent");

            _firstRun = true;
        }
    }

    public void Update()
    {
        if (_cell != null)
        {
            CellName.text = _cell.Coordinates.ToString();
            CellContent.text = string.Empty;

            CellContent.text += "\n";

            if (_cell.Structure != null)
            {
                CellContent.text += $"Structure:\t{_cell.Structure}\n";
            }
            CellContent.text += "\n";
        }
    }

    private CellData _cell;

    public void Show(CellData cell)
    {
        _cell = cell;
        Game.CreatureInfoPanel.Hide();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}