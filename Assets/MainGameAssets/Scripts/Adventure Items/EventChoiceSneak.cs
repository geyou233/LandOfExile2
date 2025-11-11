using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this choice and event is deprectated

public class EventChoiceSneak : EventChoice
{

    public EventDefinition successEventDefinition;
    public EventDefinition failureEventDefinition;

    public override string GetName()
    {

        string chanceString = Mathf.RoundToInt(MenuControl.Instance.areaMenu.GetSkipChance() * 100).ToString() + "% ";


        return "<color=#D1C8AA>" + chanceString + "</color>" + base.GetName();
    }

    public override void PerformChoice()
    {

        // if (Random.Range(0f, 1f) <= MenuControl.Instance.areaMenu.GetSkipChance())
        // {
        //
        //     //MenuControl.Instance.ShowBlockingNotification(MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().GetSprite(), MenuControl.Instance.GetLocalizedString("SneakSuccessTitle"), MenuControl.Instance.GetLocalizedString("SneakSuccessPrompt"), () =>
        //     //{
        //         MenuControl.Instance.adventureMenu.mapTiles[MenuControl.Instance.adventureMenu.currentMapTileIndex].skipped = true;
        //         MenuControl.Instance.adventureMenu.RenderScreen(true);
        //         MenuControl.Instance.dataControl.SaveData();
        //     //    MenuControl.Instance.eventMenu.CloseMenu();
        //     //});
        //
        //     MenuControl.Instance.eventMenu.ShowEvent(successEventDefinition);
        // }
        // else
        // {
        //     //MenuControl.Instance.ShowBlockingNotification(MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().GetSprite(), MenuControl.Instance.GetLocalizedString("SneakFailedTitle"), MenuControl.Instance.GetLocalizedString("SneakFailedPrompt"), () =>
        //     //{
        //     //    MenuControl.Instance.eventMenu.CloseMenu();
        //     //    MenuControl.Instance.adventureMenu.StartEncounter();
        //     //});
        //     MenuControl.Instance.eventMenu.ShowEvent(failureEventDefinition);
        //
        // }
        //
        // MenuControl.Instance.areaMenu.skipsTaken += 1;
        // MenuControl.Instance.dataControl.SaveData();

    }

}