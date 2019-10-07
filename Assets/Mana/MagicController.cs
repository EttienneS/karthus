using System;
using System.Collections.Generic;
using UnityEngine;

public class MagicController : MonoBehaviour
{
    public List<Structure> Structures = new List<Structure>();
    internal float WorkTick;

    public void AddRune(Structure runeStructure)
    {
        Structures.Add(runeStructure);
    }

    public void FreeRune(Structure stucture)
    {
        Structures.Remove(stucture);
    }

    public Queue<EntityTask> Tasks = new Queue<EntityTask>();

    private void Update()
    {
        if (Game.TimeManager.Paused)
            return;

        WorkTick += Time.deltaTime;

        if (Tasks.Count == 0)
        {
            foreach (var structure in Structures)
            {
                var interaction = structure.GetInteraction();
                if (interaction == null)
                {
                    continue;
                }
                Tasks.Enqueue(interaction);
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
            Debug.LogWarning($"Magic glitch: {ex.Message}");
        }
    }
}