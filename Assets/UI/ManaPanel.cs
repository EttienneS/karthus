using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ManaPanel : MonoBehaviour
{
    public ManaControl[] ManaControls;

    public Text Total;
    public Text Max;
    public Text Status;

    public ManaPool CurrentPool { get; set; }

    private void Update()
    {
        if (CurrentPool != null)
        {
            Max.text = CurrentPool.Sum(m => m.Value.Attunement).ToString();
            Total.text = CurrentPool.Sum(m => m.Value.Total).ToString();

            Status.text = " ==== ";
        }
    }

    public void SetPool(ManaPool pool)
    {
        CurrentPool = pool;
        foreach (var control in ManaControls)
        {
            control.SetPool(pool);
        }
    }
}