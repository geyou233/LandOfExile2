using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : BasicMenu
{
    public GameObject testPanel;

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider playSpeedSlider;
    public float playSpeed;
    public Dropdown languageSetting;
    public Button exitToMainMenuButton;
    public GameObject surrenderButton;
    public GameObject skipTutorialButton;
    public Card treasureCardToAdd;
    public Text seedText;
    public Dropdown resolutionDropdown;
    Resolution[] resolutions;
    public Toggle fullscreenToggle;

    public Text testNotification;

    public bool unitTakeDamage = true;

    public bool infinitMoveAndActions = false;
    public bool forceGenerateObstacle = false;
    public bool forceShowSpecialChallenge = false;
    public bool alwaysUpgradeAfterBattle = false;
    private void Update()
    {
        // if (MenuControl.Instance.battleMenu.inBattle)
        // {
        //     //if (MenuControl.Instance.battleMenu.player1CanAct || MenuControl.Instance.battleMenu.tutorialMode)
        //         exitToMainMenuButton.interactable = true;
        // }
    }

    private void Awake()
    {
        MenuControl.Instance.soundMixer.SetFloat("SoundVolume", PlayerPrefs.GetFloat("SoundVolume", -5f) * 4f);
        MenuControl.Instance.soundMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", -3.5f) * 4f);

        playSpeed = PlayerPrefs.GetFloat("playSpeed", 1f);
        playSpeedSlider.value = playSpeed * 1.25f;
#if UNITY_STANDALONE
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Screen_FullScreen", 1) == 1;
        //resolutions = Screen.resolutions;
        resolutions = new List<Resolution>()
        {
            newResolution(1920, 1080),
            newResolution(1600, 900),
            newResolution(1280, 720),
        }.ToArray();
        
        int currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        resolutionDropdown.value = currentResolutionIndex;
        SetNewScreenResolution();
#endif
    }

    void Start()
    {
        // MenuControl.Instance.soundMixer.SetFloat("SoundVolume", PlayerPrefs.GetFloat("SoundVolume", -5f) * 4f);
        // MenuControl.Instance.soundMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", -3.5f) * 4f);
        //
        // playSpeed = PlayerPrefs.GetFloat("playSpeed", 1f);
        // playSpeedSlider.value = playSpeed;
        
        
        
// #if UNITY_STANDALONE
//         fullscreenToggle.isOn = PlayerPrefs.GetInt("Screen_FullScreen", 1) == 1;
//         //resolutions = Screen.resolutions;
//         resolutions = new List<Resolution>()
//         {
//             newResolution(1920, 1080),
//             newResolution(1600, 900),
//             newResolution(1280, 720),
//         }.ToArray();
//         
//         int currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
//         resolutionDropdown.value = currentResolutionIndex;
//         SetNewScreenResolution();
// #endif
    }

    Resolution newResolution(int width, int height)
    {
        var result = new Resolution();
        result.width = width;
        result.height = height;
        return result;
    }
    public override void ShowMenu()
    {
#if UNITY_STANDALONE
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Screen_FullScreen", 1) == 1;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            // if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            // {
            //     currentResolutionIndex = i;
            // }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
#endif
#if !UNITY_STANDALONE
    fullscreenToggle.gameObject.SetActive(false);
    resolutionDropdown.gameObject.SetActive(false);
#endif

        seedText.text = "Seed: " + MenuControl.Instance.currentSeed;
        masterVolumeSlider.value = PlayerPrefs.GetFloat("SoundVolume", -5f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", -3.5f);
        base.ShowMenu();
        testPanel.SetActive(MenuControl.Instance.testMode);
        var isInTutorialBattle =
            MenuControl.Instance.battleMenu.tutorialMode && MenuControl.Instance.battleMenu.inBattle;
        skipTutorialButton.SetActive(isInTutorialBattle);
        exitToMainMenuButton.interactable = true;
        surrenderButton.SetActive(MenuControl.Instance.heroMenu.isAlive && !isInTutorialBattle);
        // exitToMainMenuButton.gameObject.SetActive(
        //     (!isInTutorialBattle && (!MenuControl.Instance.battleMenu.inBattle &&
        //                              !MenuControl.Instance.mainMenu.gameObject.activeInHierarchy)) ||
        //     MenuControl.Instance.testMode);
        exitToMainMenuButton.gameObject.SetActive(true);
    }

    // IEnumerator test()
    // {
    //     yield return new WaitForSeconds(0.1f);
    //     
    //     SetNewScreenResolution();
    // }

    public void ChangedVolume()
    {
        if (gameObject.activeInHierarchy)
        {
            // MenuControl.Instance.soundMixer.SetFloat("SoundVolume", masterVolumeSlider.value * 4f);
            MenuControl.Instance.soundMixer.SetFloat("SoundVolume", masterVolumeSlider.value);
            PlayerPrefs.SetFloat("SoundVolume", masterVolumeSlider.value);

            // MenuControl.Instance.soundMixer.SetFloat("MusicVolume", musicVolumeSlider.value * 4f);
            MenuControl.Instance.soundMixer.SetFloat("MusicVolume", musicVolumeSlider.value);
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        }
    }


    public void ChangedPlaySpeed()
    {
        playSpeed = playSpeedSlider.value * 1.25f;
        PlayerPrefs.SetFloat("playSpeed", playSpeed);
    }

    public void ChangedLanuage()
    {
        if (gameObject.activeInHierarchy)
        {
            Debug.Log("lanuage selected: " + languageSetting.value.ToString());
            //MenuControl.Instance.dataControl.SaveData();
            MenuControl.Instance.dataControl.SaveSettingData();
        }

        MenuControl.Instance.SetLanguage(languageSetting.value);
    }


    public void LogEntry(string stringToLog)
    {
        if (MenuControl.Instance.testMode)
        {
            //logText.text = stringToLog + "\n" + logText.text;

            //int maxLength = 5000;

            //if (logText.text.ToString().Length > maxLength)
            //{
            //    logText.text = logText.text.Substring(0, maxLength);
            //}

            Debug.Log(stringToLog);
        }
    }

    public void ClearMemory()
    {
        MenuControl.Instance.steamLogic.ResetAchievements();
        MenuControl.Instance.dataControl.ResetData();
        MenuControl.Instance.ReloadGame();
    }

    public void Revive()
    {
        if (MenuControl.Instance.heroMenu.hero != null)
        {
            MenuControl.Instance.heroMenu.isAlive = true;
            MenuControl.Instance.heroMenu.hero.currentHP = MenuControl.Instance.heroMenu.hero.GetInitialHP();
            MenuControl.Instance.dataControl.SaveData();
            MenuControl.Instance.ShowMainMenu();
        }
    }

    public void WinBattle()
    {
        if (MenuControl.Instance.battleMenu.inBattle)
        {
            MenuControl.Instance.battleMenu.CompleteBattle(true);
            CloseMenu();
        }
        else
        {
            if (MenuControl.Instance.adventureMenu.gameObject.activeInHierarchy)
            {
                int adventureItemIndex = MenuControl.Instance.adventureMenu
                    .mapTiles[MenuControl.Instance.adventureMenu.currentMapTileIndex].adventureItemIndex;
                if (adventureItemIndex >= 0)
                {
                    MenuControl.Instance.adventureMenu.selectedIndex = adventureItemIndex;
                    MenuControl.Instance.HideSubMenus();
                    CloseMenu();
                    MenuControl.Instance.battleMenu.CompleteBattle(true);
                }
            }
        }
    }

    public void LoseBattle()
    {
        if (MenuControl.Instance.battleMenu.inBattle)
        {
            List<string> buttonLabels = new List<string>();
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Confirm"));
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Cancel"));
        
            List<System.Action> actions = new List<System.Action>();
            actions.Add(() => { MenuControl.Instance.battleMenu.CompleteBattle(false); });
            actions.Add(() => { });
        
            MenuControl.Instance.confirmPopupView.ShowConfirmPopup(buttonLabels,actions,MenuControl.Instance.GetLocalizedString("Information"),MenuControl.Instance.GetLocalizedString("SurrenderPrompt"));
            CloseMenu();
        }
        else
        {
            List<string> buttonLabels = new List<string>();
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Confirm"));
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Cancel"));

            List<System.Action> actions = new List<System.Action>();
            actions.Add(() =>
            {
                MenuControl.Instance.aftermathMenu.ShowLoseMenu();
                //MenuControl.Instance.defeatMenu.ShowMenu();
            });
            actions.Add(() => { });
            MenuControl.Instance.confirmPopupView.ShowConfirmPopup(buttonLabels,actions,MenuControl.Instance.GetLocalizedString("Information"),MenuControl.Instance.GetLocalizedString("SurrenderPrompt"));
            // MenuControl.Instance.cardChoiceMenu.ShowChoice(new List<Card>(), buttonLabels, actions,
            //     MenuControl.Instance.GetLocalizedString("SurrenderPrompt"), 0, 0, false, false, false);
            CloseMenu();
        }
    }

    public void CompleteGame()
    {
        MenuControl.Instance.areaMenu.areasVisited = 4;
        MenuControl.Instance.areaMenu.currentAreaComplete = true;
        MenuControl.Instance.adventureMenu.ContinueAdventure();
    }

    public void DiscordButtonPressed()
    {
        Application.OpenURL(MenuControl.Instance.GetLocalizedString("EAButton1URL", "https://discord.gg/TZf9U5d"));
    }

    public void ToggleUnitGetDamage()
    {
        unitTakeDamage = !unitTakeDamage;
    }

    public void ToggleInfinitMoveAndActions()
    {
        infinitMoveAndActions = !infinitMoveAndActions;
    }
    
    public void ToggleForceGenerateObstacle()
    {
        forceGenerateObstacle = !forceGenerateObstacle;
    }

    public void ToggleForceShowSpecialChallenge()
    {
        forceShowSpecialChallenge = !forceShowSpecialChallenge;
    }
    public void ToggleAlwaysUpgradeAfterBattle()
    {
        alwaysUpgradeAfterBattle = !alwaysUpgradeAfterBattle;
    }

    public void RedditButtonPressed()
    {
        Application.OpenURL(
            MenuControl.Instance.GetLocalizedString("EAButton2URL", "https://www.reddit.com/r/SpellswordCards/"));
    }

    public void HowToPlayButtonPressed()
    {
        Application.OpenURL(MenuControl.Instance.GetLocalizedString("HowToPlayURL",
            "https://docs.qq.com/doc/DSVhNVGJ1TFpFUWVv?tdsourcetag=s_qq_aiomsg"));
    }

    public void UnlockGame()
    {
        //MenuControl.Instance.iAPMenu.DidPurchaseGame();
    }

    public void AddKilledEnemy()
    {
        MenuControl.Instance.heroMenu.encountersNormalWonThisRun++;
        MenuControl.Instance.heroMenu.encountersEliteWonThisRun++;
        MenuControl.Instance.heroMenu.encountersBossWonThisRun++;
        MenuControl.Instance.dataControl.SaveData();
    }

    public void RevealArea()
    {
        foreach (EventTile tile in MenuControl.Instance.adventureMenu.mapTiles)
        {
            tile.isRevealed = true;
        }

        MenuControl.Instance.dataControl.SaveData();
        MenuControl.Instance.adventureMenu.RenderScreen();
        MenuControl.Instance.adventureMenu.RenderMapTiles();
        HideMenu();
    }

    public void NextArea()
    {
        for (int ii = 0; ii < MenuControl.Instance.adventureMenu.adventureItems.Count; ii += 1)

        {
            AdventureItem item = MenuControl.Instance.adventureMenu.adventureItems[ii];
            MenuControl.Instance.adventureMenu.selectedIndex = ii;
            if (item is AdventureItemEncounter)
            {
                if (((AdventureItemEncounter)item).isBoss)
                {
                    MenuControl.Instance.victoryMenu.BattleCompleted();
                    return;
                }

                MenuControl.Instance.victoryMenu.BattleCompleted();
            }
            else
            {
                MenuControl.Instance.adventureMenu.SkipCurrentItem();
            }
        }
    }

    public void AddGold()
    {
        MenuControl.Instance.heroMenu.addFlareStone(10);
        // MenuControl.Instance.heroMenu.flareStones += 3;
        //MenuControl.Instance.heroMenu.accumulatedGold += 100;
        MenuControl.Instance.dataControl.SaveData();
    }


    public void LevelUp()
    {
        MenuControl.Instance.heroMenu.AddXP(MenuControl.Instance.heroMenu.xPForNextLevel());
        MenuControl.Instance.dataControl.SaveData();
        HideMenu();
        MenuControl.Instance.adventureMenu.ContinueAdventure();
    }

    public void ExitToMainMenu()
    {
        MenuControl.Instance.dataControl.SaveData();
        //if (MenuControl.Instance.testMode)
        {
            if (Doozy.Engine.UI.UIPopupManager.CurrentVisibleQueuePopup != null)
            {
                Doozy.Engine.UI.UIPopupManager.CurrentVisibleQueuePopup.Hide();
            }

            if (MenuControl.Instance.battleMenu.inBattle)
            {
                MenuControl.Instance.battleMenu.GetComponent<InBattleDialogueController>().BattleFinished();
            }
            MenuControl.Instance.ShowMainMenu();
        }
        // else
        // {
        //     MenuControl.Instance.ReloadGame();
        // }
    }

    public void AddEveryCardToDeck()
    {
        foreach (Card card in MenuControl.Instance.heroMenu.allCards)
        {
            if ( /*!(card is Deprecated Weapon) && */
                !(card is Hero) && !MenuControl.Instance.heroMenu.DeckContainsCardTemplate(card))
                MenuControl.Instance.heroMenu.CreateCardToOwn(card);
        }
    }

    public void GenerateArea()
    {
        
        MenuControl.Instance.adventureMenu.randomAdventureItems.Clear();
        MenuControl.Instance.adventureMenu.GenerateItemsForNewArea(false);

        MenuControl.Instance.dataControl.SaveData();
        MenuControl.Instance.adventureMenu.RenderScreen();
    }

    public void GenerateTestArea()
    {
        // MenuControl.Instance.adventureMenu.randomAdventureItems.Clear();
        // MenuControl.Instance.adventureMenu.GenerateItemsForNewArea(true);
        //
        // foreach (MapTile tile in MenuControl.Instance.adventureMenu.mapTiles)
        // {
        //     tile.revealed = true;
        // }
        // MenuControl.Instance.dataControl.SaveData();
        // MenuControl.Instance.adventureMenu.RenderScreen();
        // HideMenu();
    }

    public void AddTreasure()
    {
        MenuControl.Instance.heroMenu.AddCardToDeck(treasureCardToAdd);
        MenuControl.Instance.dataControl.SaveData();
    }

    public void CompleteCurrentEvent()
    {
        MenuControl.Instance.adventureMenu.RemoveItem();
        CloseMenu();
    }

    public void GenerateAllEvents()
    {
        MenuControl.Instance.adventureMenu.shouldShowAllEvents =
            !MenuControl.Instance.adventureMenu.shouldShowAllEvents;
        CloseMenu();
    }

    public void CompleteAllAchievements()
    {
        foreach (Achievement achievement in MenuControl.Instance.achievementsMenu.achievements)
        {
            if (!achievement.IsCompleted())
                achievement.Complete();
        }

        MenuControl.Instance.dataControl.SaveData();
    }

    public void AddCardToHand()
    {
        if (MenuControl.Instance.battleMenu.inBattle)
        {
            InputField[] inputs = GetComponentsInChildren<InputField>();
            string ID = "";
            foreach (InputField input in inputs)
            {
                if (input.transform.parent.gameObject.name.Contains("Hand"))
                {
                    ID = input.text;
                }
            }

            Player player = MenuControl.Instance.battleMenu.player1;
            Card template = MenuControl.Instance.heroMenu.GetCardByID(ID);
            if (template != null)
            {
                Card card = player.CreateCardInGameFromTemplate(template);
                player.PutCardIntoZone(card, MenuControl.Instance.battleMenu.hand);
                player.RenderCards();

                string bufferText = "" + card.UniqueID + " Added";
                if (testNotification.text.Contains(bufferText))
                {
                    testNotification.text += ".";
                }
                else
                {
                    testNotification.text = bufferText;
                }
            }
        }
    }

    public void FinishHero()
    {
        MenuControl.Instance.heroMenu.finishClass();
    }

    public void RemoveCardFromDeck()
    {
        InputField[] inputs = GetComponentsInChildren<InputField>();
        string ID = "";
        foreach (InputField input in inputs)
        {
            if (input.transform.parent.gameObject.name.Contains("From Deck"))
            {
                ID = input.text;
            }
        }

        Player player = MenuControl.Instance.battleMenu.player1;
        Card template = MenuControl.Instance.heroMenu.GetCardByID(ID);
        if (template != null)
        {
            var selectedCard = MenuControl.Instance.heroMenu.GetDeckCardByID(template.UniqueID);
            if (selectedCard)
            {
                
                MenuControl.Instance.heroMenu.RemoveCardFromDeck(selectedCard);
                // Card card = player.CreateCardInGameFromTemplate(template);
                //   MenuControl.Instance.heroMenu.AddCardToDeck(card);

                string bufferText = "" + selectedCard.UniqueID + " Removed";
                if (testNotification.text.Contains(bufferText))
                {
                    testNotification.text += ".";
                }
                else
                {
                    testNotification.text = bufferText;
                }
            }
            else
            {
                
                string bufferText = "" + selectedCard.UniqueID + " Not Exist";
                testNotification.text = bufferText;
            }
        }

        if (MenuControl.Instance.adventureMenu.isActiveAndEnabled)
        {
            MenuControl.Instance.adventureMenu.RenderDeckList();
        }
    }
    public void AddCardToDeck()
    {
        InputField[] inputs = GetComponentsInChildren<InputField>();
        string ID = "";
        foreach (InputField input in inputs)
        {
            if (input.transform.parent.gameObject.name.Contains("To Deck"))
            {
                ID = input.text;
            }
        }

        Player player = MenuControl.Instance.battleMenu.player1;
        Card template = MenuControl.Instance.heroMenu.GetCardByID(ID);
        if (template != null)
        {
            Card card = player.CreateCardInGameFromTemplate(template);
            MenuControl.Instance.heroMenu.AddCardToDeck(card);

            string bufferText = "" + card.UniqueID + " Added";
            if (testNotification.text.Contains(bufferText))
            {
                testNotification.text += ".";
            }
            else
            {
                testNotification.text = bufferText;
            }
        }

        if (MenuControl.Instance.adventureMenu.isActiveAndEnabled)
        {
            MenuControl.Instance.adventureMenu.RenderDeckList();
        }
    }

    public void RemoveAllCardsFromDeck()
    {
        for (int i = MenuControl.Instance.heroMenu.cardsInDeck().Count - 1; i >= 0; i--)
        {
            MenuControl.Instance.heroMenu.RemoveCardFromDeck(MenuControl.Instance.heroMenu.cardsInDeck()[i]);
        }
    }


    public void AddEffectToHero()
    {
        if (MenuControl.Instance.battleMenu.inBattle)
        {
            InputField[] inputs = GetComponentsInChildren<InputField>();
            string ID = "";
            foreach (InputField input in inputs)
            {
                if (input.transform.parent.gameObject.name.Contains("Effect"))
                {
                    ID = input.text;
                }
            }

            {
                Player player = MenuControl.Instance.battleMenu.player1;
                Hero hero = player.GetHero();
                Effect effect = MenuControl.Instance.heroMenu.GetEffectByID(ID);
                if (effect != null)
                {
                    hero.currentEffects.Add(effect);

                    string bufferText = "" + effect.UniqueID + " Added";
                    if (testNotification.text.Contains(bufferText))
                    {
                        testNotification.text += ".";
                    }
                    else
                    {
                        testNotification.text = bufferText;
                    }
                }
            }

            {
                Player player = MenuControl.Instance.battleMenu.playerAI;
                Hero hero = player.GetHero();
                Effect effect = MenuControl.Instance.heroMenu.GetEffectByID(ID);
                if (effect != null)
                {
                    hero.currentEffects.Add(effect);

                    string bufferText = "" + effect.UniqueID + " Added";
                    if (testNotification.text.Contains(bufferText))
                    {
                        testNotification.text += ".";
                    }
                    else
                    {
                        testNotification.text = bufferText;
                    }
                }
            }
            
        }
    }

    public void DoDamageToEnemyHero()
    {
        if (MenuControl.Instance.battleMenu.inBattle)
        {
            InputField[] inputs = GetComponentsInChildren<InputField>();
            int ID = 0;
            foreach (InputField input in inputs)
            {
                if (int.TryParse(input.text, out ID))
                {
                    // foreach (Unit otherUnit in MenuControl.Instance.battleMenu.GetAllUnitsOnBoard())
                    {
                        //if (otherUnit == MenuControl.Instance.battleMenu.playerAI.GetHero())
                        {
                            var otherUnit = MenuControl.Instance.battleMenu.playerAI.GetHero();
                            otherUnit.SufferDamage(null, null, ID);

                            string bufferText = "" + ID + " Damaged";
                            if (testNotification.text.Contains(bufferText))
                            {
                                testNotification.text += ".";
                            }
                            else
                            {
                                testNotification.text = bufferText;
                            }
                        }
                    }

                }
            }
        }
    }
    public void DoDamageToAll()
    {
        if (MenuControl.Instance.battleMenu.inBattle)
        {
            InputField[] inputs = GetComponentsInChildren<InputField>();
            int ID = 0;
            foreach (InputField input in inputs)
            {
                if (int.TryParse(input.text, out ID))
                {
                    foreach (Unit otherUnit in MenuControl.Instance.battleMenu.GetAllUnitsOnBoard())
                    {
                        otherUnit.SufferDamage(null, null, ID);

                        string bufferText = "" + ID + " Damaged";
                        if (testNotification.text.Contains(bufferText))
                        {
                            testNotification.text += ".";
                        }
                        else
                        {
                            testNotification.text = bufferText;
                        }
                    }

                    return;
                }
            }
        }
    }

    public void AddRandomEffectToAll()
    {
        Player player = MenuControl.Instance.battleMenu.player1;
        Hero hero = player.GetHero();
        if (MenuControl.Instance.battleMenu.inBattle)
        {
            var effect = MenuControl.Instance.heroMenu.allEffects.RandomItem();
            {
                foreach (Unit otherUnit in MenuControl.Instance.battleMenu.GetAllUnitsOnBoard())
                {
                    //if (hero.GetEffectsWithTemplate(effect) != null)
                    {
                        otherUnit.ApplyEffect(null, null, effect, 1);
                        //otherUnit.currentEffects.Add(effect);

                        string bufferText = "" + effect.UniqueID + " Added";
                        if (testNotification.text.Contains(bufferText))
                        {
                            testNotification.text += ".";
                        }
                        else
                        {
                            testNotification.text = bufferText;
                        }

                    }
                }
            }
        }
    }

    public void AddEffectToAll()
    {
        if (MenuControl.Instance.battleMenu.inBattle)
        {
            InputField[] inputs = GetComponentsInChildren<InputField>();
            string ID = "";
            foreach (InputField input in inputs)
            {
                if (input.transform.parent.gameObject.name.Contains("Effect"))
                {
                    ID = input.text;
                }
            }

            Effect effect = MenuControl.Instance.heroMenu.GetEffectByID(ID);
            if (effect != null)
            {
                foreach (Unit otherUnit in MenuControl.Instance.battleMenu.GetAllUnitsOnBoard())
                {

                    // if (!(otherUnit is Hero))
                    // {
                    //     continue;
                    // }
                    otherUnit
                        .ApplyEffect(null, null, effect, 1);
                    //otherUnit.currentEffects.Add(effect);

                    string bufferText = "" + effect.UniqueID + " Added";
                    if (testNotification.text.Contains(bufferText))
                    {
                        testNotification.text += ".";
                    }
                    else
                    {
                        testNotification.text = bufferText;
                    }
                }
            }
        }
    }

    public void AddTalentToHero()
    {
        Hero hero = MenuControl.Instance.heroMenu.hero;
        if (hero != null)
        {
            InputField[] inputs = GetComponentsInChildren<InputField>();
            string ID = "";
            foreach (InputField input in inputs)
            {
                if (input.transform.parent.gameObject.name.Contains("Talent"))
                {
                    ID = input.text;
                }
            }


            Card card = MenuControl.Instance.heroMenu.GetCardByID(ID);
            if (card is Skill skill)
            {
                MenuControl.Instance.levelUpMenu.variableTalentsAcquired.Add(skill);

                if (skill.abilityToPerform != null)
                {
                    skill.abilityToPerform.PerformAbility(null, null, 0);
                }

                string bufferText = "" + skill.UniqueID + " Added";
                if (testNotification.text.Contains(bufferText))
                {
                    testNotification.text += ".";
                }
                else
                {
                    testNotification.text = bufferText;
                }

                MenuControl.Instance.dataControl.SaveData();
            }
        }
    }

    public void ReplaceRandomEvents()
    {
        Hero hero = MenuControl.Instance.heroMenu.hero;
        if (hero != null)
        {
            InputField[] inputs = GetComponentsInChildren<InputField>();
            string ID = "";
            foreach (InputField input in inputs)
            {
                if (input.transform.parent.gameObject.name.Contains("Random"))
                {
                    ID = input.text;
                }
            }


            foreach (AdventureItem item in MenuControl.Instance.adventureMenu.allAdventureItems)
            {
                if (item.UniqueID == ID)
                {
                    for (int ii = 0; ii < MenuControl.Instance.adventureMenu.adventureItems.Count; ii++)
                    {
                        if (MenuControl.Instance.adventureMenu.adventureItems[ii] is AdventureItemRandomEvent || MenuControl.Instance.adventureMenu.adventureItems[ii] is AdventureItemEncounter)
                        {
                            MenuControl.Instance.adventureMenu.adventureItems[ii] = item;
                            if (item is AdventureItemEncounter encounter)
                            {
                                encounter.PickRandomSettingValue();
                                MenuControl.Instance.adventureMenu.adventureItemInfos[ii] =
                                    encounter.GenerateSpecialChallenge();
                            }

                            string bufferText = "" + ID + " Replaced all Random Events";
                            if (testNotification.text.Contains(bufferText))
                            {
                                testNotification.text += ".";
                            }
                            else
                            {
                                testNotification.text = bufferText;
                            }
                        }
                    }
                }
            }
            
            foreach (AdventureItem item in MenuControl.Instance.adventureMenu.originalAllAdventureItems)
            {
                if (item.UniqueID == ID)
                {
                    for (int ii = 0; ii < MenuControl.Instance.adventureMenu.adventureItems.Count; ii++)
                    {
                        //if (MenuControl.Instance.adventureMenu.adventureItems[ii] is AdventureItemRandomEvent)
                        {
                            MenuControl.Instance.adventureMenu.adventureItems[ii] = item;
                            if (item is AdventureItemEncounter encounter)
                            {
                                encounter.PickRandomSettingValue();
                                MenuControl.Instance.adventureMenu.adventureItemInfos[ii] =
                                    encounter.GenerateSpecialChallenge();
                            }

                            string bufferText = "" + ID + " Replaced all Random Events";
                            if (testNotification.text.Contains(bufferText))
                            {
                                testNotification.text += ".";
                            }
                            else
                            {
                                testNotification.text = bufferText;
                            }
                        }
                    }
                }
            }
        }
    }

    public void SetNewScreenResolution()
    {
        if (gameObject.activeInHierarchy)
        {
            SetResolution(resolutionDropdown.value);
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        Screen.SetResolution(resolution.width, resolution.height, PlayerPrefs.GetInt("Screen_FullScreen", 1) == 1);
    }

    void UpdateScreen()
    {
        
        if (resolutions == null)
        {
            return;
        }
        var resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, PlayerPrefs.GetInt("Screen_FullScreen", 1) == 1);
    }

    public void ToggleFullscreen()
    {
        // Toggle fullscreen
        if (gameObject.activeInHierarchy)
        {
            PlayerPrefs.SetInt("Screen_FullScreen", fullscreenToggle.isOn?1:0);
            //Screen.fullScreen = !Screen.fullScreen;
            UpdateScreen();
        }
    }
}