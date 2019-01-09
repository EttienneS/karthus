using UnityEngine;

public class GameController : MonoBehaviour
{
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

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
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
    }
}