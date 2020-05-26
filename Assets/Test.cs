using Assets.Helpers;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject ObjectToSpawn;
    public int Amount;

    // Start is called before the first frame update
    private void Start()
    {
        using (Instrumenter.Start())
        {
            for (int i = 0; i < Amount; i++)
            {
                var obj = Instantiate(ObjectToSpawn, transform);
                obj.transform.localPosition = new Vector3(Random.Range(1f, 100f), Random.Range(1f, 100f), Random.Range(1f, 100f));
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}