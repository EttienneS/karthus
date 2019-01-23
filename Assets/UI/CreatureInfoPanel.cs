using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CreatureInfoPanel : MonoBehaviour
{
    private static CreatureInfoPanel _instance;

    public static CreatureInfoPanel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("CreatureInfoPanel").GetComponent<CreatureInfoPanel>();
            }

            return _instance;
        }
    }

    public Text CreatureName;

    private bool _firstRun = true;
    public void Start()
    {
        if (_firstRun)
        {
            var children = GetComponentsInChildren<Text>().ToList();
            CreatureName = children.First(t => t.name == "CreatureName");


            _firstRun = true;
        }
    }

    public void Show()
    {
        Instance.gameObject.SetActive(true);

        var creature = GameController.Instance.SelectedCreature;
        CreatureName.text = creature.name;
    }

    public void Hide()
    {
        Instance.gameObject.SetActive(false);
    }

}
