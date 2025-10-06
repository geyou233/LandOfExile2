using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class VictoryMenu : BasicMenu
{
    public Text xPText;
    public Slider xPSlider;

    public Text level;

    public int xpGained;

    public List<Card> treasureCards = new List<Card>();

    public Spine.Unity.SkeletonGraphic skeletonGraphic;
    public Doozy.Engine.Soundy.SoundyData impactSFX;

    public LootMenu lootMenu;
    public AdventureItemEncounter currentEncounter;
    public Button lootButton;
    public GameObject levelUpIndicator;

    public void BattleCompleted()
    {
        currentEncounter = (AdventureItemEncounter) MenuControl.Instance.adventureMenu.GetCurrentAdventureItem();
        
        MenuControl.Instance.achievementsMenu.CheckAchievements(currentEncounter);
       // if (GetCurrentAdventureItem() is AdventureItemEncounter)
        {
            MenuControl.Instance.shopMenu.AddShopItem();
        }
        if (currentEncounter.isBoss)
        {
            
            MenuControl.Instance.heroMenu.encountersBossWonThisRun += 1;
        }
        else if (currentEncounter.eventInfo!=null && currentEncounter.eventInfo.eventType == "精英")
        {
            MenuControl.Instance.heroMenu.encountersEliteWonThisRun += 1;
            
        }
        else
        {
            MenuControl.Instance.heroMenu.encountersNormalWonThisRun += 1;
        }
        if (currentEncounter.isBoss && currentEncounter.eventInfo.level == 3)
        {
            MenuControl.Instance.adventureMenu.EndRun(false);
            return;
        }

        ShowVictory();
        lootMenu.HideMenu();

        // MenuControl.Instance.LogEvent("Victory_" + currentEncounter.UniqueID + MenuControl.Instance.heroMenu.GetLevelClassPathString());
        MenuControl.Instance.LogEvent("Victory_" + currentEncounter.UniqueID);
    }

    void ShowVictory()
    {
        ShowMenu();
        MenuControl.Instance.PlayAdventureMusic();

        levelUpIndicator.SetActive(false);

        //Animate 
        //skeletonGraphic.AnimationState.SetAnimation(0, "victory", false);
        //skeletonGraphic.AnimationState.AddAnimation(0, "victory_idle", true, 1.2f);

        LeanTween.delayedCall(0.2f, () => { Doozy.Engine.Soundy.SoundyManager.Play(impactSFX); });

        lootButton.interactable = false;
        LeanTween.delayedCall(1.3f, () => { lootButton.interactable = true; });


        if (MenuControl.Instance.adventureMenu.GetCurrentAdventureItem())
        {
        }

        MenuControl.Instance.heroMenu.turnsUsedThisRun += MenuControl.Instance.battleMenu.currentRound;

        //Sync player hitpoints
        if (MenuControl.Instance.battleMenu.player1.GetHero() != null)
            MenuControl.Instance.heroMenu.hero.currentHP = Mathf.Min(MenuControl.Instance.heroMenu.hero.GetInitialHP(),
                MenuControl.Instance.battleMenu.player1.GetHero().currentHP);

        //show XP gain
        MenuControl.Instance.progressMenu.encountersWon += 1;

        xpGained = currentEncounter.GetAdjustedXP();
        if (MenuControl.Instance.settingsMenu.alwaysUpgradeAfterBattle)
        {
            
            MenuControl.Instance.heroMenu.AddXPToUpgrade3();
        }
        else
        {
            MenuControl.Instance.heroMenu.AddXP(currentEncounter.GetAdjustedXP());
        }
        //Heal if boss encounter and dont have floor 4 key
        if (currentEncounter.isBoss &&
            !MenuControl.Instance.heroMenu.DeckContainsCardTemplate(
                MenuControl.Instance.levelUpMenu.strangeKeyForFloor4))
        {
            int amountToHeal = Mathf.RoundToInt(MenuControl.Instance.heroMenu.hero.initialHP * 0.75f);
            if (MenuControl.Instance.heroMenu.ascensionMode >= 8)
            {
                amountToHeal = Mathf.RoundToInt(MenuControl.Instance.heroMenu.hero.initialHP * 0.35f);
            }

            MenuControl.Instance.heroMenu.hero.currentHP = Mathf.Min(MenuControl.Instance.heroMenu.hero.initialHP,
                MenuControl.Instance.heroMenu.hero.currentHP + amountToHeal);
        }

        // MenuControl.Instance.heroMenu.flareStones += MenuControl.Instance.CountOfCardsInList(
        //     MenuControl.Instance.levelUpMenu.extraFlareStoneEOBTalent,
        //     MenuControl.Instance.levelUpMenu.variableTalentsAcquired);

        Card rustySword = MenuControl.Instance.heroMenu.GetCardByID("ArtifactEquip28");
        if (rustySword != null)
        {
            if (MenuControl.Instance.heroMenu.artifactsEquipped.Contains(rustySword))
            {
                MenuControl.Instance.heroMenu.AddXP(Mathf.CeilToInt(xpGained * 0.35f));
            }
        }


        MenuControl.Instance.adventureMenu.RemoveItem(); //Save data


        level.text = MenuControl.Instance.heroMenu.currentLevel.ToString();

        if (MenuControl.Instance.heroMenu.currentLevel != MenuControl.Instance.heroMenu.levelsXP.Count)
        {
            
            var xpFillOneTime = 0.5f;
             var experienceSequence =  DOTween.Sequence();
             experienceSequence.AppendInterval(0.8f);
             var originLevel = MenuControl.Instance.heroMenu.currentLevel;
            var offset = 0;
            float xpOld = Mathf.Max(0f,
                ((float) MenuControl.Instance.heroMenu.currentXPForLevel(offset) - xpGained) /
                (float) MenuControl.Instance.heroMenu.xPForNextLevel(offset));
            
            xPSlider.value = xpOld;
            xpGained = MenuControl.Instance.heroMenu.currentXPForLevel(offset);
            
            int newLevel = MenuControl.Instance.heroMenu.currentLevel;
            while (true)
            {
                if (xpGained >= MenuControl.Instance.heroMenu.xPForNextLevel(offset))
                {
                    xpGained -= MenuControl.Instance.heroMenu.xPForNextLevel(offset);
                    offset++;
                    if (MenuControl.Instance.heroMenu.currentLevel + offset ==
                        MenuControl.Instance.heroMenu.levelsXP.Count)
                    {
                        break;
                    }


                }
                else
                {
                    break;
                }
            }
            
            
            newLevel = originLevel + offset;
            
            bool noNewCards = newLevel == originLevel;
            var originExpRatio = xpOld;
            var tempLevel = originLevel;
            var tempOffset = 0;
       if (noNewCards)
       {
           experienceSequence.Append(DOTween.To(() => xPSlider.value, x => xPSlider.value = x, xpGained/(float)MenuControl.Instance.heroMenu.xPForNextLevel(offset), xpFillOneTime));
       }
       else
       {
           level.text = originLevel.ToString();
           experienceSequence.Append(DOTween.To(() => xPSlider.value, x => xPSlider.value = x, 1, xpFillOneTime*(1-originExpRatio)));
           experienceSequence.AppendCallback(() =>
           {
               level.text = (tempLevel + 1).ToString();
               tempLevel++;
               tempOffset++;
               LeanTween.delayedCall(0f, () => { levelUpIndicator.SetActive(true); });
           });
           for (int i = originLevel+1; i < newLevel; i++)
           {
               experienceSequence.AppendCallback(() =>
               {
                   xPSlider.value = 0;
               });
               experienceSequence.Append(DOTween.To(() => xPSlider.value, x => xPSlider.value = x, 1, xpFillOneTime));
               experienceSequence.AppendCallback(() =>
               {
                   level.text = (tempLevel + 1).ToString();
                   tempLevel++;

                   tempOffset++;
                   // levelUpIndicator.GetComponent<Localize>().Term = "levelupDescription";
                        //"+" + MenuControl.Instance.heroMenu.heroClass.GetHPGainPerLevel().ToString() + " HP!";
                    //LeanTween.delayedCall(0f, () => { levelUpIndicator.SetActive(true); });
               });
           }
    
           experienceSequence.AppendCallback(() =>
           {
               xPSlider.value = 0;
           });
           experienceSequence.Append(DOTween.To(() => xPSlider.value, x => xPSlider.value = x, xpGained/(float)MenuControl.Instance.heroMenu.xPForNextLevel(offset), xpFillOneTime));
       }

       experienceSequence.OnUpdate(() =>
       {
           var currentMaxExperience = MenuControl.Instance.heroMenu.xPForNextLevel(tempOffset);
           xPText.text = ((int)(xPSlider.value * currentMaxExperience)).ToString()+ " / " + currentMaxExperience;
           //levelText.text
       });
       experienceSequence.OnComplete(() =>
       {
           xPText.text = ((int)( xpGained)).ToString()+ " / " + MenuControl.Instance.heroMenu.xPForNextLevel(offset);
           level.text = newLevel.ToString();
       });
       experienceSequence.Play();
            
            
            
            
            
            
            
            
       //      var xpFillOneTime = 0.5f;
       //      var experienceSequence =  DOTween.Sequence();
       //      var originLevel = MenuControl.Instance.heroMenu.currentLevel;
       //     offset = 0;
       //     var tempOffset = 0;
       //     float xpOld = Mathf.Max(0f,
       //         ((float) MenuControl.Instance.heroMenu.currentXPForLevel(Offset) - xpGained) /
       //         (float) MenuControl.Instance.heroMenu.xPForNextLevel(Offset));
       //     
       //     xPSlider.value = xpOld;
       //     while (true)
       //     {
       //         
       //      
       //         float xpNew = Mathf.Min(1f,
       //             (float) MenuControl.Instance.heroMenu.currentXPForLevel(Offset) /
       //             (float) MenuControl.Instance.heroMenu.xPForNextLevel(Offset));
       //
       //      
       //         experienceSequence.Append(DOTween.To(() => xPSlider.value, x => xPSlider.value = x, xpNew, xpFillOneTime*(xpNew-xpOld)));
       //         
       //         
       //         experienceSequence.AppendCallback(() =>
       //         {
       //         
       //             level.text = (MenuControl.Instance.heroMenu.currentLevel+tempOffset).ToString();
       //         });
       //         
       //         
       //         
       //         experienceSequence.AppendCallback(() =>
       //         {
       //             xPSlider.value = xpOld;
       //         });
       //         // experienceSequence.Append(DOTween.To(() => xPSlider.value, x => xPSlider.value = x, 1, xpFillOneTime));
       //         // experienceSequence.AppendCallback(() =>
       //         // {
       //         //     levelText.text = (tempLevel + 1).ToString();
       //         //     tempLevel++;
       //         // });
       //         
       //         if (MenuControl.Instance.heroMenu.currentXPForLevel(Offset) <
       //             (float)MenuControl.Instance.heroMenu.xPForNextLevel(Offset))
       //         {
       //             break;
       //         }
       //         offset++;
       //         experienceSequence.AppendCallback(() =>
       //         {
       //
       //             tempOffset++;
       //         });
       //         if(MenuControl.Instance.heroMenu.currentLevel+Offset == MenuControl.Instance.heroMenu.levelsXP.Count)
       //         {
       //             break;
       //         }
       //
       //     }
       //
       //     // experienceSequence.AppendCallback(() =>
       //     // {
       //     //     xPSlider.value = 0;
       //     // });
       //     //experienceSequence.Append(DOTween.To(() => xPSlider.value, x => xPSlider.value = x, hero.experienceRatio, xpFillOneTime));
       //
       //
       // experienceSequence.OnUpdate(() =>
       // {
       //     xPText.text = ((int)(xPSlider.value * MenuControl.Instance.heroMenu.xPForNextLevel(tempOffset))).ToString()+ " / " + MenuControl.Instance.heroMenu.xPForNextLevel(tempOffset);
       //     //levelText.text
       // });
       // experienceSequence.OnComplete(() =>
       // {
       //     xPText.text = ((int)( MenuControl.Instance.heroMenu.currentXPForLevel(Offset) )).ToString()+ " / " + MenuControl.Instance.heroMenu.xPForNextLevel(Offset);
       //     level.text = (MenuControl.Instance.heroMenu.currentLevel+Offset).ToString();
       //     
       // });
       // experienceSequence.Play();
       //      
       //      
       //      
            
            
            
            
            
            // float xpOld = Mathf.Max(0f,
            //     ((float) MenuControl.Instance.heroMenu.currentXPForLevel() - xpGained) /
            //     (float) MenuControl.Instance.heroMenu.xPForNextLevel());
            // float xpNew = Mathf.Min(1f,
            //     (float) MenuControl.Instance.heroMenu.currentXPForLevel() /
            //     (float) MenuControl.Instance.heroMenu.xPForNextLevel());
            // xPSlider.value = xpOld;
            // LeanTween.value(gameObject, (val) => { xPSlider.value = val; }, xpOld, xpNew, 2f).setEaseInOutSine();
            //
            // int xpOldInt = MenuControl.Instance.heroMenu.currentXPForLevel() - xpGained;
            // int xpNewInt = MenuControl.Instance.heroMenu.currentXPForLevel();
            // LeanTween.value(gameObject,
            //     (val) =>
            //     {
            //         xPText.text = Mathf.RoundToInt(val).ToString() + " / " +
            //                       MenuControl.Instance.heroMenu.xPForNextLevel();
            //     }, xpOldInt, xpNewInt, 2f).setEaseInOutSine();
            //
            // if (xpNew >= 1f)
            // {
            //    // levelUpIndicator.GetComponent<Localize>().Term = "levelupDescription";
            //         //"+" + MenuControl.Instance.heroMenu.heroClass.GetHPGainPerLevel().ToString() + " HP!";
            //     LeanTween.delayedCall(2f, () => { levelUpIndicator.SetActive(true); });
            // }
        }
        else
        {
            xPSlider.value = 0f;
            xPText.text = "已满级"; // "Max Lv.";
        }

        // if (MenuControl.Instance.demoMode && currentEncounter.isBoss)
        // {
        //     MenuControl.Instance.ShowBlockingNotification(null,
        //         MenuControl.Instance.GetLocalizedString("DEMOMessageTitle"),
        //         MenuControl.Instance.GetLocalizedString("DEMOMessage"), () =>
        //         {
        //             MenuControl.Instance.heroMenu.isAlive = false;
        //             MenuControl.Instance.dataControl.SaveData();
        //             MenuControl.Instance.ReloadGame();
        //         });
        //     return;
        // }
    }

    public void ContinuePressed()
    {
        lootButton.interactable = false;
        try
        {
            lootMenu.EncounterLoot(currentEncounter);
            CloseMenu();
        }
        catch (Exception e)
        {
            lootMenu.PressedButton(false);
        }
    }

    public void FinishVictory()
    {
        MenuControl.Instance.battleMenu.ResetBattle();
        MenuControl.Instance.infoMenu.HideMenu();
        MenuControl.Instance.battleMenu.HideMenu();


        MenuControl.Instance.adventureMenu.ContinueAdventure();
        CloseMenu();
    }
}