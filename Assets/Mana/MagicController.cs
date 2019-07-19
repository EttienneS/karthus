using System.Collections.Generic;
using UnityEngine;

public class MagicController : MonoBehaviour
{
    internal float WorkTick;
    internal float MagicRate = 100;

    public void AddRune(Structure source)
    {
        source.Spell.Originator = source.GetGameId();

        Runes.Add(source);
        Work.Enqueue(source.Spell);
    }

    public void FreeRune(Structure stucture)
    {
        Runes.Remove(stucture);
    }

    public List<Structure> Runes = new List<Structure>();
    public Queue<SpellBase> Work = new Queue<SpellBase>();

    private void Update()
    {
        if (Game.TimeManager.Paused)
            return;

        WorkTick += Time.deltaTime;

        if (WorkTick >= Game.TimeManager.MagicInterval)
        {
            var spells = new List<SpellBase>();
            for (int i = 0; i < MagicRate; i++)
            {
                var spell = Work.Dequeue();
                spell.Done();
                spells.Add(spell);
            }

            foreach (var spell in spells)
            {
                if (IdService.GetStructureFromId(spell.Originator) != null)
                {
                    Work.Enqueue(spell);
                }
            }
        }
    }
}