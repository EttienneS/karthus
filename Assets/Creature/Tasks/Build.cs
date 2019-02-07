using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Build : TaskBase
{
    
    public Coordinates Coordinates;

    
    public StructureData Structure;

    public Build(StructureData structure, Coordinates coordinates)
    {
        Structure = structure;
        Coordinates = coordinates;

        SubTasks = new Queue<TaskBase>();

        foreach (var itemType in structure.RequiredItemTypes)
        {
            SubTasks.Enqueue(new MoveItemToCell(itemType, Coordinates, true));
            SubTasks.Enqueue(new PlaceHeldItemInStructure(Structure));
        }

        SubTasks.Enqueue(new Wait(3f, "Building"));
        SubTasks.Enqueue(new Move(MapGrid.Instance.GetCellAtCoordinate(coordinates).Neighbors
                                    .First(c => c.TravelCost != 0).Data.Coordinates));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            Structure.DestroyContainedItems();
            Structure.ToggleBluePrintState();

            if (Structure.SpriteName == "Box")
            {
                var pile = Structure.LinkedGameObject.gameObject.AddComponent<Stockpile>();

                if (Structure.Name.Contains("Wood"))
                {
                    pile.Data.ItemType = "Wood";
                }
                else
                {
                    pile.Data.ItemType = "Rock";
                }
            }

            return true;
        }
        return false;
    }

    public override void Update()
    {
        if (Structure == null)
        {
            throw new CancelTaskException();
        }

        Taskmaster.ProcessQueue(SubTasks);
    }
}