using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MainGameAssets.Scripts.Menus_UI;
using Pool;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : BasicMenu
{
    public GameObject bGImage;
    public RectTransform doorTransform;
    float doorAnimateTime = 0.3f;
    public GameObject continueButton;
    
    [SerializeField] private AgeTipsMenu ageTipsMenu;
    // public GameObject newHeroButton;
    // public Button draftButton;
    // public Button reaperButton;
    //
    // public Button seasonsButton;
    public GameObject playButton;
    public GameObject tutorialButton;
    public GameObject testBattleButton;
    public CanvasGroup uIButtons;
    
    public GameObject firstLayerButtons;//this is the first layer of buttons now
    public GameObject difficultyChoiceButtons;//this is the first layer of buttons now, is children of firstLayerButtons
    public GameObject startContinueButtons;

    public GameObject[] chineseOnlyObjects;

    public GameObject[] demoShowButtons;
    public GameObject[] demoHideButtons;

    void UpdateLanguageRelatedStuff()
    {

        // if (LocalizationManager.CurrentLanguage == MenuControl.Languages.Chinese.ToString())
        // {
        //     foreach (GameObject obj in chineseOnlyObjects)
        //     {
        //         obj.SetActive(true);
        //     }
        //
        // }
        // else
        // {
        //     foreach (GameObject obj in chineseOnlyObjects)
        //     {
        //         obj.SetActive(false);
        //     }
        // }
    }

    public override void ShowMenu()
    {
        base.ShowMenu();
        UpdateLanguageRelatedStuff();
        EventPool.OptIn("ChangeLanguage", UpdateLanguageRelatedStuff);

        foreach (var button in demoShowButtons)
        {
            button.SetActive(MenuControl.Instance.DemoMode);
        }
        foreach (var button in demoHideButtons)
        {
            button.SetActive(!MenuControl.Instance.DemoMode);
        }
        
        doorTransform.localPosition = Vector3.zero;
        bGImage.transform.localScale = Vector3.one * 1.8f;
        MenuControl.Instance.AllowInput();
        firstLayerButtons.SetActive(true);
        startContinueButtons.SetActive(false);
        // draftButton.interactable = !MenuControl.Instance.demoMode && MenuControl.Instance.tutorialFinished;
        // reaperButton.interactable = !MenuControl.Instance.demoMode && MenuControl.Instance.tutorialFinished;
        // seasonsButton.interactable = !MenuControl.Instance.demoMode && MenuControl.Instance.tutorialFinished;
        continueButton.SetActive(MenuControl.Instance.heroMenu.isAlive);
        //newHeroButton.SetActive(MenuControl.Instance.tutorialFinished || MenuControl.Instance.testMode);
        testBattleButton.SetActive(MenuControl.Instance.testMode);

        playButton.SetActive(!MenuControl.Instance.tutorialFinished && !MenuControl.Instance.testMode);
        tutorialButton.SetActive(MenuControl.Instance.tutorialFinished || MenuControl.Instance.testMode);
        difficultyChoiceButtons.SetActive(MenuControl.Instance.tutorialFinished || MenuControl.Instance.testMode);

        if (MenuControl.Instance.skipLogin)
        {
            if (MenuControl.Instance.battleMusicController != null || MenuControl.Instance.adventureMusicController == null)
                MenuControl.Instance.PlayAdventureMusic();
        }

        uIButtons.alpha = 1;
        uIButtons.gameObject.SetActive(true);

        MenuControl.Instance.settingsMenu.SetNewScreenResolution();
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.P))
    //     {
    //         
    //         MenuControl.Instance.loadingMenu. ShowMenu();
    //     }
    // }

    public override void HideMenu(bool instantly = false)
    {
        base.HideMenu(instantly);
        MenuControl.Instance.AllowInput();
    }

    public void ChooseNewHeroDifficulty()
    {
        
        // difficultyChoiceButtons.SetActive(true);
        // uIButtons.gameObject.SetActive(false);
    }

    public void CancelDifficultyChoice()
    {
         firstLayerButtons.SetActive(true);
         startContinueButtons.SetActive(false);
        //difficultyChoiceButtons.SetActive(false);
       // uIButtons.gameObject.SetActive(true);
    }

    enum GameMode
    {
        normal
    }

    private GameMode gameMode;
    public void SelectNormal()
    {
        gameMode = GameMode.normal;
        MenuControl.Instance.adventureMenu.tutorialEncounterOnly = false;
        firstLayerButtons.SetActive(false);
        startContinueButtons.SetActive(true);
    }


    public void NewHero()
    {
        switch (gameMode)
        {
            case GameMode.normal:
                NewHeroNormal();
                break;
        }
        
    }

    void clearMapData()
    {
        foreach (EventTile mapTile in MenuControl.Instance.adventureMenu.mapTiles)
        {
            Destroy(mapTile.gameObject);
        }
        MenuControl.Instance.adventureMenu.eventIdSelectOptionId.Clear();
        MenuControl.Instance.adventureMenu.adventureItemCompletions.Clear();
        MenuControl.Instance.adventureMenu.adventureItemChecked.Clear();
        MenuControl.Instance.adventureMenu.adventureItems.Clear();
        MenuControl.Instance.adventureMenu.adventureItemInfos.Clear();
        MenuControl.Instance.adventureMenu.mapTiles.Clear();
        MenuControl.Instance.adventureMenu.randomAdventureItems.Clear();
    }

    public void NewHeroEasy()
    {
        difficultyChoiceButtons.SetActive(false);

        NewHero(true, false, false, true, false);
    }

    public void NewHeroNormal()
    {
        //difficultyChoiceButtons.SetActive(false);

        NewHero(true, false, false, false, false);
    }

    public void NewHeroDraft()
    {
        difficultyChoiceButtons.SetActive(false);

        NewHero(true, true, false, false, false);
    }

    public void NewHeroReaper()
    {
        difficultyChoiceButtons.SetActive(false);

        NewHero(true, false, true, false, false);
    }

    public void NewHeroSeasons()
    {
        //difficultyChoiceButtons.SetActive(false);

        NewHero(true, false, false, false, true);
    }

    void NewHero(bool newSeed, bool draftMode, bool reaperMode, bool easyMode, bool seasonsMode)
    {

        System.Action action = () =>
        {
            
            // firstLayerButtons.SetActive(true);
            // startContinueButtons.SetActive(false);
            clearMapData();
            AnimateDoor();
            LeanTween.delayedCall(doorAnimateTime, () =>
            {
                if (MenuControl.Instance.newHeroFeatureOn)
                {
                    MenuControl.Instance.bigAdventureMenu.ShowMenu();
                }
                else
                {
                    
                    //HideMenu();
                    MenuControl.Instance.heroMenu.StartNewHero(newSeed, draftMode, reaperMode, easyMode, seasonsMode);
                }
            });
        };

        if (MenuControl.Instance.heroMenu.isAlive)
        {
            List<string> buttonLabels = new List<string>();
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Cancel"));
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Confirm"));

            List<System.Action> actions = new List<System.Action>();
            actions.Add(() => { CancelDifficultyChoice(); });
            actions.Add(action);

            MenuControl.Instance.cardChoiceMenu.ShowChoice(new List<Card>(), buttonLabels, actions, MenuControl.Instance.GetLocalizedString("ReplaceHeroPrompt"), 0, 0, false, -1, false);
        }
        else
        {
            action();
        }
    }

    public void ContinueHero()
    {
        AnimateDoor();
        LeanTween.delayedCall(doorAnimateTime, () =>
        {
            if (MenuControl.Instance.heroMenu.isAlive)
            {
                //HideMenu();
                MenuControl.Instance.adventureMenu.ContinueAdventure();
            }
        });
    }

    public void StartTestEncounter()
    {
        AnimateDoor();
        LeanTween.delayedCall(doorAnimateTime, () =>
        {
            MenuControl.Instance.adventureMenu.testEncounterOnly = true;
            MenuControl.Instance.battleMenu.skipCutscene = true;
            MenuControl.Instance.adventureMenu.tutorialEncounterOnly = false;
            MenuControl.Instance.adventureMenu.StartEncounter();
            MenuControl.Instance.adventureMenu.testEncounterOnly = false;
            MenuControl.Instance.battleMenu.skipCutscene = false;
        });
    }

    public void StartTutorialEncounter()
    {
        AnimateDoor();
        LeanTween.delayedCall(doorAnimateTime, () =>
        {
            MenuControl.Instance.adventureMenu.tutorialEncounterOnly = true;
            MenuControl.Instance.adventureMenu.StartEncounter();
        });
    }

    void AnimateDoor()
    {
        Doozy.Engine.Soundy.SoundyManager.Play(MenuControl.Instance.doorOpenSound);
        MenuControl.Instance.StopInput();
        HideMenu();
        //LeanTween.move(doorTransform, Vector2.up * 600f, doorAnimateTime).setEaseInOutQuad();

        //LeanTween.scale(bGImage, Vector3.one * 3.6f, doorAnimateTime).setEaseInOutSine();

        LeanTween.alphaCanvas(uIButtons, 0f, 0.3f);
    }
    
    
    #region Button Events

    public void OnAgeButtonClick()
    {
        ageTipsMenu.ShowMenu();
    }

    #endregion
}
