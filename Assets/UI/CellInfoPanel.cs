using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CellInfoPanel : MonoBehaviour
{
    private static CellInfoPanel _instance;

    public static CellInfoPanel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("CellInfoPanel").GetComponent<CellInfoPanel>();
            }

            return _instance;
        }
    }

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
            CellName.text = _cell.Data.Coordinates.ToString();
            CellContent.text = string.Empty;

            foreach (var item in _cell.Data.ContainedItems.GroupBy(g => g.ItemType))
            {
                CellContent.text += $"{item.Key}:\t{item.Count()}\n";
            }

            CellContent.text += "\n";

            if(_cell.Data.Structure != null)
            {
                CellContent.text += $"Structure:\t{_cell.Data.Structure}\n";
            }
            CellContent.text += "\n";
        }
    }

    private Cell _cell;

    public void Show(Cell cell)
    {
        _cell = cell;
        CreatureInfoPanel.Instance.Hide();
        Instance.gameObject.SetActive(true);
    }

    public void Hide()
    {
        Instance.gameObject.SetActive(false);
    }
}