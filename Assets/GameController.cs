using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController _instance;

    public Creature Player;

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

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Player = CreatureController.Instance.SpawnCreature(MapGrid.Instance.GetRandomPathableCell());

        CameraController.Instance.MoveToViewPoint(Player.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            DestroyMap();

            MapEditor.Instance.CreateMap();
            SpawnPlayer();
        }
    }

    private void DestroyMap()
    {
        foreach (var cell in MapGrid.Instance.Map)
        {
            Destroy(cell.gameObject);
        }
        Destroy(Player.gameObject);
    }
}
