﻿using System.Linq;

public partial class OrderSelectionController //.Zone
{
    internal const string DefaultZoneText = "Zones";

    internal OrderButton ZonesButton;

    public void AddZoneClicked()
    {
        Game.Controller.SetMouseSprite(Game.ZoneController.ZoneSprite,
                                       (cell) => CanAddZone(cell));
    }

    public bool CanAddZone(Cell cell)
    {
        return true;
    }

    public void ZoneTypeClicked()
    {
        if (Game.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();
            var button = CreateOrderButton("Add Zone", () => AddZoneClicked(), Game.ZoneController.ZoneSprite);

        }
    }
}