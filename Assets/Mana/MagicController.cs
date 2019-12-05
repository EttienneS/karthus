using System;
using System.Collections.Generic;
using UnityEngine;

public class MagicController : MonoBehaviour
{
    public List<Structure> Structures = new List<Structure>();
    public Queue<EffectBase> Tasks = new Queue<EffectBase>();
    internal float WorkTick;

    public void AddEffector(Structure structure)
    {
        Structures.Add(structure);
    }

    public void RemoveEffector(Structure stucture)
    {
        Structures.Remove(stucture);
    }

    private void Update()
    {
        if (!Game.Ready)
            return;

        if (Game.TimeManager.Paused)
            return;

        WorkTick += Time.deltaTime;

        if (Tasks.Count == 0)
        {
            foreach (var structure in Structures)
            {
                foreach (var interaction in structure.GetInteraction())
                {
                    Tasks.Enqueue(interaction);
                }
            }
        }

        try
        {
            if (WorkTick >= Game.TimeManager.MagicInterval && Tasks.Count > 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    if (Tasks.Count == 0)
                    {
                        break;
                    }
                    var task = Tasks.Dequeue();

                    if (task != null)
                    {
                        task.Done();
                    }
                    else
                    {
                        break;
                    }
                }
                WorkTick = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Effect glitch: {ex.Message}");
        }
    }
}