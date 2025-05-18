using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Doozy.Engine.UI;
using MainGameAssets.Scripts.Menus_UI;
using Pool;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Component = UnityEngine.Component;
using Random = UnityEngine.Random;

public class MenuControl : MonoBehaviour
{
    public static MenuControl Instance;
    public bool betaMode;
    public bool testMode;
    public bool DemoMode => demoMode;
    bool demoMode = false;
    public bool stoveKoreanMode;
    public bool useAlternateSprites;
    public bool forceOldVersionInvalidateRun;

    public bool saveDebugData = false;

    public bool weatherFeatureOn = true;
    public bool largeHeroMoveFix = true;
    public bool publishVersionFeatureOn = true;
    public bool newHeroFeatureOn = false;
    public float speedScale = 0.87f;

    [HideInInspector]
    public bool shownSteamPage = false;

    public bool isDemo = true;

    [HideInInspector] public List<string> defeatedEncounter = new List<string>();

    public IndicatorMenu indicatorMenu;
    public VisibleCard visibleCardPrefab;
    public ChoicePanel choicePanelPrefab;
    public CSVLoader csvLoader;
    
    public LoginMenu loginMenu;
    public MainMenu mainMenu;
    public AdventureMenu adventureMenu;
    public HeroMenu heroMenu;
    public BigAdventureMenu bigAdventureMenu;
    public PathMenu pathMenu;
    public BattleMenu battleMenu;
    public InfoMenu infoMenu;
    public EventMenu eventMenu;
    public DataControl dataControl;
    public AreaMenu areaMenu;
    public QuestMenu questMenu;
    public ProgressMenu progressMenu;
    public VictoryMenu victoryMenu;
    public DefeatMenu defeatMenu;
    public AftermathMenu aftermathMenu;
    public SettingsMenu settingsMenu;
    public AchievementsMenu achievementsMenu;

    public ShopMenu shopMenu;

    //  public WeaponsMenu weaponsMenu;
    public ItemMenu itemsMenu;
    public DeckMenu deckMenu;
    public LevelUpMenu levelUpMenu;
    public CardChoiceMenu cardChoiceMenu;
    public ConfirmPopupView confirmPopupView;
    public ImageAndTextPopup imageAndTextPopup;
    public RulebookMenu rulebookMenu;
    public LibraryMenu libraryMenu;

    public CardViewerMenu cardViewerMenu;

    //public AlphaMenu alphaMenu;
    //public BountyMenu bountyMenu;
    public CutsceneMenu cutsceneMenu;

    public LoadingMenu loadingMenu;

    //public CreditsMenu creditsMenu;
    public ChangeLogMenu changeLogMenu;
    public SteamLogic steamLogic;
    public LootMenu lootMenu;
    public DemoView demoView;
    public AgeTipsMenu ageTipsMenu;

    public UpgradeSelectCardView upgradeSelectCardView;

    public GameObject initialBlockerPanel;
    public GameObject inputBlockerPanel;

    public bool tutorialFinished;
    public bool cutSceneFinished;
    public bool firstRun;
    public string currentSeed;
    [System.NonSerialized] public string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789"; //add the characters you want

    public CardTag lootTag;
    public CardTag spellTag;
    public CardTag potionTag;
    public CardTag artifactTag;
    public CardTag treasureTag;
    public CardTag summonInFrontLikeMeleeTag;
    public CardTag evolveBlessingTag;

    public CardTag naughtyTag;
    public CardTag niceTag;

    public Doozy.Engine.Soundy.SoundyData battleMusic;
    public Doozy.Engine.Soundy.SoundyData adventureMusic;
    public Doozy.Engine.Soundy.SoundyController battleMusicController;
    public Doozy.Engine.Soundy.SoundyController adventureMusicController;
    public UnityEngine.Audio.AudioMixer soundMixer;

    public System.Action actionToPerformAfterBlockingPopup;

    public List<Sprite> defaultCardFronts = new List<Sprite>();

    public Doozy.Engine.Soundy.SoundyData cardLiftSound;
    public Doozy.Engine.Soundy.SoundyData buffActionSound;
    public Doozy.Engine.Soundy.SoundyData doorOpenSound;
    public Doozy.Engine.Soundy.SoundyData errorSound;

    float oldAdventureMusicIdleTime;
    float oldBattleMusicIdleTime;

    public List<Text> buildLabelTexts = new List<Text>();
    public string previousVersionText;

    public Font defaultMenuFont;
    public List<Font> safeMenuFonts = new List<Font>();

    public Sprite naughtySprite;
    public Sprite niceSprite;

    [Category] [Header("Error Checks")] 
    public bool checkIMPORTANT;
    public bool checkCardUpgradeInfo;
    public bool checkNameAndDescription;
    public bool checkSpriteExist;
    public bool exhustAllMinionAfterUsage = true;

    /// <summary>跳过登录</summary>
    private bool skipLogin = false;

    public enum Languages
    {
        English,
        Chinese,
        ChineseTraditional,
        // German,
        // Portuguese,
        // Russian,
        // Italian,
        // Spanish,
        // Polish,
        Japanese,
        // Korean,
        // Vietnamese,
        // French
    }

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;

#if UNITY_EDITOR
#else
        testMode = false;
        //futureFeatureOn = false;
        //newHeroSystemOn = false;
        saveDebugData = false;
#endif
    }

    bool LoadDataAndException(Action action, string removeData)
    {
        try
        {
            action();
        }
        catch (System.Exception e)
        {
            //consider remove this?
            //dataControl.ResetData();

            File.WriteAllText(Application.persistentDataPath + removeData, null);
            //MenuControl.Instance.mainMenu.NewHeroNormal();
            Debug.Log(e);
            //ShowNotification(null, "Error", e.ToString(), false, true, true);

            HideSubMenus();
            cardChoiceMenu.ShowNotifcation(new List<Card>(), () => { },
                "<color=white>Oh no sorry! Please screenshot and send to us.\n</color>" + e.ToString());
            cardChoiceMenu.promptText.rectTransform.sizeDelta = new Vector2(2000f, 1000f);
            return false;
        }

        return true;
    }

    private void Start()
    {
        bool succeed = true;

        if (!dataControl.HasSettingData())
        {
            settingsMenu.languageSetting.value = 1;
        }

        succeed &= LoadDataAndException(() => { dataControl.LoadSettingData(); },
            $"/SaveSetting{dataControl.surfix}.txt");
        succeed &= LoadDataAndException(() => { dataControl.LoadGlobalData(); },
            $"/SaveGlobal{dataControl.surfix}.txt");
        succeed &= LoadDataAndException(() => { dataControl.LoadData(); }, $"/SaveData{dataControl.surfix}.txt");
        // try
        // {
        //     dataControl.LoadSettingData();
        //     dataControl.LoadGlobalData();
        //     //todo: don't load data here. load when continue game
        //     dataControl.LoadData();
        // }
        // catch (System.Exception e)
        // {
        //     //consider remove this?
        //     //dataControl.ResetData();
        //     
        //     File.WriteAllText(Application.persistentDataPath + "/SaveData.txt", null);
        //     //MenuControl.Instance.mainMenu.NewHeroNormal();
        //     Debug.Log(e);
        //     //ShowNotification(null, "Error", e.ToString(), false, true, true);
        //
        //     HideSubMenus();
        //     cardChoiceMenu.ShowNotifcation(new List<Card>(), () => { }, "<color=white>Oh no sorry! Please screenshot and send to us.\n</color>" + e.ToString());
        //     cardChoiceMenu.promptText.rectTransform.sizeDelta = new Vector2(2000f, 1000f);
        //     return;
        // }
        if (!succeed)
        {
            return;
        }

        foreach (Text text in buildLabelTexts)
        {
            text.text = "游戏版本: " + GetBuildLabelText() + (betaMode ? " BETA" : "");

            if (demoMode)
            {
                text.text = GetBuildLabelText() + (demoMode ? " DEMO" : "");
            }
        }

        if (stoveKoreanMode && firstRun)
        {
            settingsMenu.languageSetting.value = 10;
        }
        else
        {
            settingsMenu.ChangedLanuage();
        }

        initialBlockerPanel.SetActive(true);
        StopInput();
        // LeanTween.delayedCall(0.01f, () =>
        // {
            if (!skipLogin)
            {
                ShowLoginMenu();
            }
            else
            {
                ShowMainMenu();
            }
            initialBlockerPanel.SetActive(false);

            // if (!cutSceneFinished)
            // {
            //     cutsceneMenu.ShowIntroCutscenes();
            // }
        // });

#if UNITY_STANDALONE
        foreach (ScrollRect scrollRect in GetComponentsInChildren<ScrollRect>(true))
        {
            scrollRect.scrollSensitivity = 35f;
        }
#endif

        heroMenu.LoadDropRates();

        if (betaMode && !testMode)
        {
            ShowNotification(null, "BETA", GetLocalizedString("BETAMessage"), true, false, true);
        }

        //Change run is still valid
        CheckRunStillValidAfterUpdate();


        if (testMode)
        {
            TestCards();
        }

        LogEvent("GameLoaded");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIPopupManager.CurrentVisibleQueuePopup != null)
            {
            }
            else if (deckMenu.gameObject.activeInHierarchy)
            {
                deckMenu.CloseMenu();
            }
            // else if (weaponsMenu.gameObject.activeInHierarchy)
            // {
            //     weaponsMenu.CloseMenu();
            // }
            else if (itemsMenu.gameObject.activeInHierarchy)
            {
                itemsMenu.CloseMenu();
            }
            else if (cardChoiceMenu.gameObject.activeInHierarchy)
            {
            }
            else if (eventMenu.gameObject.activeInHierarchy)
            {
            }

            else if (settingsMenu.gameObject.activeInHierarchy)
            {
                settingsMenu.CloseMenu();
            }
            else if (cardViewerMenu.gameObject.activeInHierarchy)
            {
                cardViewerMenu.CloseMenu();
            }
            else if (rulebookMenu.gameObject.activeInHierarchy)
            {
                rulebookMenu.CloseMenu();
            }
            else if (libraryMenu.gameObject.activeInHierarchy)
            {
                libraryMenu.CloseMenu();
            }
            else if (achievementsMenu.gameObject.activeInHierarchy)
            {
                achievementsMenu.CloseMenu();
            }
            else if (progressMenu.gameObject.activeInHierarchy)
            {
                progressMenu.CloseMenu();
            }
            // else if (creditsMenu.gameObject.activeInHierarchy)
            // {
            //     creditsMenu.CloseMenu();
            // }
            else if (changeLogMenu.gameObject.activeInHierarchy)
            {
                changeLogMenu.CloseMenu();
            }
            // else if (alphaMenu.gameObject.activeInHierarchy)
            // {
            //     alphaMenu.CloseMenu();
            // }
            // else if (bountyMenu.gameObject.activeInHierarchy)
            // {
            //     bountyMenu.CloseMenu();
            // }
            else if (ageTipsMenu.gameObject.activeInHierarchy)
            {
                ageTipsMenu.CloseMenu();
            }
            else if (levelUpMenu.gameObject.activeInHierarchy)
            {
                levelUpMenu.CloseMenu();
            }
            else if (shopMenu.gameObject.activeInHierarchy)
            {
                if (!shopMenu.confirmationPanel.gameObject.activeInHierarchy)
                {
                    shopMenu.CloseMenu();
                }
                else
                {
                    shopMenu.CancelAction();
                }
            }
            else if (heroMenu.gameObject.activeInHierarchy)
            {
                if (!settingsMenu.gameObject.activeInHierarchy)
                {
                    settingsMenu.ShowMenu();
                }
            }
            else if (mainMenu.gameObject.activeInHierarchy)
            {
                if (!settingsMenu.gameObject.activeInHierarchy)
                {
                    settingsMenu.ShowMenu();
                }
            }
            else if (battleMenu.gameObject.activeInHierarchy)
            {
                if (!settingsMenu.gameObject.activeInHierarchy && UIPopupManager.CurrentVisibleQueuePopup == null &&
                    FindObjectOfType<SettingButton>() != null)
                {
                    settingsMenu.ShowMenu();
                }
            }

            infoMenu.HideMenu();
        }

        if (adventureMusicController != null)
        {
            if (adventureMusicController.PlayProgress <= 0f && oldAdventureMusicIdleTime > 0f)
            {
                adventureMusicController = null;
                oldAdventureMusicIdleTime = 0f;
                LeanTween.delayedCall(gameObject, 2f, () => { PlayAdventureMusic(); });
            }
            else
            {
                if (testMode && Input.GetKeyDown(KeyCode.S))
                {
                    adventureMusicController.AudioSource.time = adventureMusicController.AudioSource.clip.length - 3f;
                }

                oldAdventureMusicIdleTime = adventureMusicController.PlayProgress;
            }
        }

        if (battleMusicController != null)
        {
            if (battleMusicController.PlayProgress <= 0f && oldBattleMusicIdleTime > 0f)
            {
                battleMusicController = null;
                oldBattleMusicIdleTime = 0f;
                LeanTween.delayedCall(gameObject, 2f, () => { PlayBattleMusic(); });
            }
            else
            {
                if (testMode && Input.GetKeyDown(KeyCode.S))
                {
                    battleMusicController.AudioSource.time = battleMusicController.AudioSource.clip.length - 3f;
                }

                oldBattleMusicIdleTime = battleMusicController.PlayProgress;
            }
        }
    }


    public void PlayAdventureMusic()
    {
        if (adventureMusicController == null)
        {
            LeanTween.value(gameObject, 1f, 0f, 1.5f).setOnUpdate((float val) =>
            {
                if (battleMusicController != null)
                {
                    battleMusicController.AudioSource.volume = val;
                }
            }).setOnComplete(() =>
            {
                if (battleMusicController != null)
                    battleMusicController.Kill();

                LeanTween.delayedCall(gameObject, 1f,
                    () =>
                    {
                        if (battleMusicController != null)
                            battleMusicController.Kill();
                        adventureMusicController = Doozy.Engine.Soundy.SoundyManager.Play(adventureMusic);
                    });
            });
        }
        else
        {
            adventureMusicController.Play();
            LeanTween.value(gameObject, 0f, 1f, 2f).setOnUpdate((float val) =>
            {
                if (adventureMusicController != null)
                    adventureMusicController.AudioSource.volume = val;
            });

            if (battleMusicController != null)
                battleMusicController.Kill();
        }
    }

    public void PlayBattleMusic()
    {
        if (battleMusicController == null)
        {
            LeanTween.value(gameObject, 1f, 0f, 1f).setOnUpdate((float val) =>
            {
                if (adventureMusicController != null)
                {
                    adventureMusicController.AudioSource.volume = val;
                }
            }).setOnComplete(() =>
            {
                oldBattleMusicIdleTime = 0f;

                if (adventureMusicController != null)
                    adventureMusicController.Pause();

                LeanTween.delayedCall(gameObject, 3f,
                    () =>
                    {
                        if (adventureMusicController != null)
                            adventureMusicController.Pause();
                        battleMusicController = Doozy.Engine.Soundy.SoundyManager.Play(battleMusic);
                    });
            });
        }
    }

    public void HideSubMenus()
    {
        loginMenu.HideMenu();
        mainMenu.HideMenu();
        adventureMenu.HideMenu(true);
        bigAdventureMenu.HideMenu(true);
        heroMenu.HideMenu(true);
        pathMenu.HideMenu(true);
        if (battleMenu.yourTurnIndicator.IsActive())
        {
            battleMenu.yourTurnIndicator.Hide(true);
        }

        if (battleMenu.notEnoughResourcesIndicator.IsActive())
        {
            battleMenu.notEnoughResourcesIndicator.Hide(true);
        }

        battleMenu.HideMenu(true);
        infoMenu.HideMenu(true);
        eventMenu.HideMenu(true);
        areaMenu.HideMenu(true);
        questMenu.HideMenu(true);
        progressMenu.HideMenu(true);
        settingsMenu.HideMenu(true);
        victoryMenu.HideMenu(true);
        lootMenu.HideMenu(true);
        demoView.HideMenu(true);
        upgradeSelectCardView.HideMenu(true);
        defeatMenu.HideMenu(true);
        aftermathMenu.HideMenu(true);
        achievementsMenu.HideMenu(true);
        shopMenu.HideMenu(true);
        deckMenu.HideMenu(true);
        //weaponsMenu.HideMenu(true);
        itemsMenu.HideMenu(true);
        levelUpMenu.HideMenu(true);
        cardChoiceMenu.HideMenu(true);
        confirmPopupView.HideMenu(true);
        imageAndTextPopup.HideMenu(true);
        rulebookMenu.HideMenu(true);
        libraryMenu.HideMenu(true);
        cardViewerMenu.HideMenu(true);
        //alphaMenu.HideMenu(true);
        //bountyMenu.HideMenu(true);
        cutsceneMenu.HideMenu(true);
        //creditsMenu.HideMenu(true);
        changeLogMenu.HideMenu(true);
        loadingMenu.HideMenu(true);
        ageTipsMenu.HideMenu(true);
    }

    public string GetChineseLocalizedString(string stringToLocalize, string defaultString = null)
    {
        if (defaultString == null)
            defaultString = stringToLocalize;

        if (I2.Loc.LocalizationManager.GetTermsList().Contains(stringToLocalize))
        {
            string localString =
                I2.Loc.LocalizationManager.GetTranslation(stringToLocalize, overrideLanguage: "Chinese");

            if (localString == stringToLocalize)

                return defaultString;
            else if (localString == null) return "";
            else

                return localString.Replace("\\n", "\n");
        }
        else
        {
            return defaultString;
        }
    }

    public string GetLocalizedString(string stringToLocalize, string defaultString = null)
    {
        if (defaultString == null)
            defaultString = stringToLocalize;

        if (I2.Loc.LocalizationManager.GetTermsList().Contains(stringToLocalize))
        {
            string localString = I2.Loc.LocalizationManager.GetTranslation(stringToLocalize);

            if (localString == stringToLocalize)

                return defaultString;
            else if (localString == null) return "";
            else

                return localString.Replace("\\n", "\n");
        }
        else
        {
            return defaultString;
        }
    }

    public void SetLanguage(int languageInt)
    {
        Font oldFont = GetSafeFont();
        I2.Loc.LocalizationManager.CurrentLanguage = ((Languages)languageInt).ToString();
        EventPool.Trigger("ChangeLanguage");
        foreach (Text text in GetComponentsInChildren<Text>(true))
        {
            if (text.GetComponentInParent<Dropdown>() == null)
            {
                if (languageInt != 0)
                {
                    if (text.font == defaultMenuFont)
                    {
                        text.font = GetSafeFont();
                    }
                }
                else
                {
                    if (text.font == oldFont)
                    {
                        text.font = defaultMenuFont;
                    }
                }
            }
        }
    }

    public void ReportLeaderboardScores()
    {
    }

    public string GetCardTypeStringForTags(List<CardTag> tags)
    {
        return "";
    }


    public string GenerateSeed()
    {
        currentSeed = null;

        int charAmount = 6;
        for (int i = 0; i < charAmount; i++)
        {
            currentSeed += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];
        }

        return currentSeed;
    }

    public void ShowEnemyDeckPopup()
    {
        deckMenu.ShowEnemyDeck();
    }

    public void ShowMyDeckPopup()
    {
        deckMenu.ShowMyDeck();
    }

    public void ReloadGame()
    {
        Doozy.Engine.Soundy.SoundyManager.StopAllSounds();
        MenuControl.Instance.HideSubMenus();

        if (MenuControl.Instance.newHeroFeatureOn)
        {
            
            int newHero = heroMenu.hasNewUnlockHeroVisualization();
            if (newHero > 0)
            {
                MenuControl.Instance.bigAdventureMenu.ShowMenu();
                MenuControl.Instance.bigAdventureMenu.ShowUnlockView(newHero);
            }
            else
            {
                if (!skipLogin)
                {
                    ShowLoginMenu();
                }
                else
                {
                    Start();
                }
            }

        }
        else
        {
            if (!skipLogin)
            {
                ShowLoginMenu();
            }
            else
            {
                ShowMainMenu();
            }

        }
        //add loading page here?
        //MenuControl.Instance.loadingMenu.ShowLoading();
        //SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        // Application.LoadLevel(Application.loadedLevel);
    }

    public void ShowLoginMenu()
    {
        
        battleMenu.inBattle = false;

        HideSubMenus();
        
        loginMenu.ShowMenu();
    }

    public void ShowMainMenu()
    {
        battleMenu.inBattle = false;

        HideSubMenus();

        mainMenu.ShowMenu();
    }

    public int ApplySeed()
    {
        int integerY = 0;
        for (int ii = 0; ii < currentSeed.Length; ii += 1)
        {
            char character = currentSeed[ii];
            integerY += Mathf.RoundToInt(glyphs.IndexOf(character) * Mathf.Pow(10, ii));
        }

        int seed = integerY + MenuControl.Instance.areaMenu.areasVisited * 100 +
                   (MenuControl.Instance.areaMenu.currentArea != null
                       ? MenuControl.Instance.areaMenu.allAreas.IndexOf(MenuControl.Instance.areaMenu.currentArea)
                       : 0);

        Random.InitState(seed);

        return seed;
    }

    public void ShowNotification(Sprite sprite, string titleText, string bodyText, bool dismissable, bool dismissOther,
        bool useOverlay, bool isRight = true)
    {
        if (dismissOther && UIPopupManager.CurrentVisibleQueuePopup != null)
        {
            UIPopupManager.CurrentVisibleQueuePopup.Hide();
        }

        UIPopup popup =
            UIPopup.GetPopup((isRight ? "Right" : "Left") + "Notification" + (useOverlay ? "" : "NoOverlay"));
        if (popup == null) return;

        popup.Data.Images[0].sprite = sprite;
        popup.Data.SetLabelsTexts(titleText, bodyText);

        if (I2.Loc.LocalizationManager.CurrentLanguage != MenuControl.Languages.English.ToString())
        {
            popup.Data.Labels[0].GetComponent<Text>().font = MenuControl.Instance.GetSafeFont();
        }

        if (I2.Loc.LocalizationManager.CurrentLanguage == MenuControl.Languages.Chinese.ToString() ||
            I2.Loc.LocalizationManager.CurrentLanguage == MenuControl.Languages.ChineseTraditional.ToString() ||
            I2.Loc.LocalizationManager.CurrentLanguage == MenuControl.Languages.Japanese.ToString())
        {
            popup.Data.Labels[1].GetComponent<Text>().resizeTextForBestFit = false;
        }

#if !UNITY_STANDALONE
        popup.Data.Labels[0].transform.parent.parent.localScale = Vector3.one * 1.25f;
        popup.Data.Labels[0].transform.parent.parent.GetComponent<RectTransform>().anchoredPosition +=
 Vector2.up * 100f;
#endif

        popup.Data.Buttons[0].gameObject.SetActive(dismissable);
        UIPopupManager.AddToQueue(popup);
    }

    public void ShowBlockingNotification(Sprite sprite, string titleText, string bodyText,
        System.Action actionToPerform)
    {
        if (UIPopupManager.CurrentVisibleQueuePopup != null)
        {
            UIPopupManager.CurrentVisibleQueuePopup.Hide();
        }

        UIPopup popup = UIPopup.GetPopup("RightNotificationBlocking");
        if (popup == null) return;

        popup.Data.Images[0].GetComponent<Image>().sprite = sprite;

        popup.Data.Images[0].GetComponent<Image>().gameObject.SetActive(sprite != null);

        popup.Data.SetLabelsTexts(titleText, bodyText);

        if (I2.Loc.LocalizationManager.CurrentLanguage != MenuControl.Languages.English.ToString())
        {
            popup.Data.Labels[0].GetComponent<Text>().font = MenuControl.Instance.GetSafeFont();
        }

        if (I2.Loc.LocalizationManager.CurrentLanguage == MenuControl.Languages.Chinese.ToString() ||
            I2.Loc.LocalizationManager.CurrentLanguage == MenuControl.Languages.ChineseTraditional.ToString() ||
            I2.Loc.LocalizationManager.CurrentLanguage == MenuControl.Languages.Japanese.ToString())
        {
            popup.Data.Labels[1].GetComponent<Text>().resizeTextForBestFit = false;
        }

#if !UNITY_STANDALONE
        popup.Data.Labels[0].transform.parent.parent.localScale = Vector3.one * 1.25f;
        popup.Data.Labels[0].transform.parent.parent.GetComponent<RectTransform>().anchoredPosition +=
 Vector2.up * 100f;
#endif

        UIPopupManager.AddToQueue(popup);
        actionToPerformAfterBlockingPopup = actionToPerform;
    }

    public void BlockingPopupDismissed()
    {
        if (UIPopupManager.CurrentVisibleQueuePopup != null)
        {
            UIPopupManager.CurrentVisibleQueuePopup.Hide();
        }

        if (actionToPerformAfterBlockingPopup != null)
            actionToPerformAfterBlockingPopup();
    }


    public void StopInput()
    {
        inputBlockerPanel.SetActive(true);
    }

    public void AllowInput()
    {
        inputBlockerPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public Font GetSafeFont()
    {
        Languages language =
            (Languages)System.Enum.Parse(typeof(Languages), I2.Loc.LocalizationManager.CurrentLanguage);
        return safeMenuFonts[(int)language];
    }

    public void CheckRunStillValidAfterUpdate()
    {
        bool endRun = false;
        if (heroMenu.isAlive == true)
        {
            if (adventureMenu.adventureItems.Count != adventureMenu.adventureItemCompletions.Count)
            {
                endRun = true;
            }

            foreach (Card card in adventureMenu.itemCards)
            {
                if (card == null)
                {
                    endRun = true;
                }
            }

            if (forceOldVersionInvalidateRun && previousVersionText != GetBuildLabelText()) endRun = true;

            if (endRun)
            {
                heroMenu.isAlive = false;
                dataControl.SaveData();

                ShowNotification(null, GetLocalizedString("InvalidRunTitle"), GetLocalizedString("InvalidRunPrompt"),
                    true, true, true);
            }
        }
    }

    void TestCards()
    {
        //

        if (saveDebugData)
        {
            //if data folder not exist, create folder
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
            }
        }

        string text = "";
        foreach (UnlockableStartingCards unlock in MenuControl.Instance.heroMenu.unlockableStartingCardLists)
        {
            text += "\n" + unlock.name + "\n";
            foreach (StartingDeck deck in unlock.startingDecks)
            {
                text += deck.GetName() + " " + deck.GetDescription() + "\n";
                foreach (Card card in deck.startingCards)
                {
                    if (heroMenu.GetCardByID(card.UniqueID) == null)
                    {
                        //it could happen now, as all card removed lots of cards
                        //todo: remove later
                        //Debug.LogError(card.UniqueID);
                    }

                    text += card.GetName() + " " + card.GetDescription() + "\n";
                }
            }
        }

        if (saveDebugData)
        {
            File.WriteAllText("data\\unlockableStartingCardLists.txt", text);
        }

        foreach (HeroClass heroClass in heroMenu.heroClasses)
        {
            foreach (Card card in heroClass.classCards)
            {
                if (heroMenu.GetCardByID(card.UniqueID) == null)
                {
                    //todo:change back
                    //Debug.LogError(card.UniqueID);
                }

                //Check all talents that have an effect are in alleffects
                foreach (Card skill in heroClass.basicTalents)
                {
                    if (skill.GetComponent<SkillApplyEffect>() != null)
                    {
                        if (!heroMenu.allEffects.Contains(skill.GetComponent<SkillApplyEffect>().effectTemplate))
                        {
                            Debug.LogError(skill.gameObject.name + " - " + skill.UniqueID +
                                           " effect used by talent not in all effects");
                        }
                    }
                }

                foreach (Card skill in heroClass.advancedTalents)
                {
                    if (skill.GetComponent<SkillApplyEffect>() != null)
                    {
                        if (!heroMenu.allEffects.Contains(skill.GetComponent<SkillApplyEffect>().effectTemplate))
                        {
                            Debug.LogError(skill.gameObject.name + " - " + skill.UniqueID +
                                           " effect used by talent not in all effects");
                        }
                    }
                }

                foreach (Card skill in heroClass.midpointTalents)
                {
                    if (skill.GetComponent<SkillApplyEffect>() != null)
                    {
                        if (!heroMenu.allEffects.Contains(skill.GetComponent<SkillApplyEffect>().effectTemplate))
                        {
                            Debug.LogError(skill.gameObject.name + " - " + skill.UniqueID +
                                           " effect used by talent not in all effects");
                        }
                    }
                }
            }
        }

        foreach (HeroPath heroPath in heroMenu.heroPaths)
        {
            foreach (Card card in heroPath.startingCards)
            {
                if (heroMenu.GetCardByID(card.UniqueID) == null)
                {
                    //todo:change back
                    //Debug.LogError(card.UniqueID);
                }
            }

            foreach (Card card in heroPath.pathCards)
            {
                if (heroMenu.GetCardByID(card.UniqueID) == null)
                {
                    //todo:change back
                    //Debug.LogError(card.UniqueID);
                }
            }
        }


        foreach (Card card in heroMenu.unlockableCards)
        {
            if (heroMenu.GetCardByID(card.UniqueID) == null)
            {
                //todo:change back
                //Debug.LogError(card.UniqueID);
            }
        }

        foreach (AdventureItem item in adventureMenu.allAdventureItems)
        {
            if (item is AdventureItemEncounter)
            {
                foreach (Card card in ((AdventureItemEncounter)item).rewardCards)
                {
                    if (heroMenu == null || card == null)
                    {
                        continue;
                    }

                    if (heroMenu.GetCardByID(card.UniqueID) == null)
                    {
                        //todo:change back
                        //Debug.LogError(card.UniqueID);
                    }
                }
            }
        }


        foreach (Card card in MenuControl.Instance.heroMenu.allCards)
        {
            if (card.upgradeCards.Count > 0)
            {
                foreach (var upgradeCard in card.upgradeCards)
                {
                    if (card.level == 1)
                    {
                        if (upgradeCard.level != 2)
                        {
                            if (checkCardUpgradeInfo)
                                Debug.LogError(card.UniqueID + " 等级为1的卡牌升级后不是等级2 " + upgradeCard.UniqueID);
                        }
                    }
                    else if (card.level == 2)
                    {
                        if (upgradeCard.level != 3)
                        {
                            if (checkCardUpgradeInfo)
                                Debug.LogError(card.UniqueID + " 等级为2的卡牌升级后不是等级3 " + upgradeCard.UniqueID);
                        }
                    }
                    else
                    {
                        if (checkCardUpgradeInfo)
                            Debug.LogError(card.UniqueID + " 等级为3的不应该还能升级到 " + upgradeCard.UniqueID);
                    }
                }
            }

            foreach (CardTag cardTag in card.cardTags)
            {
                if (cardTag == null)
                    Debug.LogError(card.UniqueID + " Contains Null Tag");
                if (cardTag == naughtyTag)
                    Debug.LogError(card.UniqueID + " Contains Naughty Tag");
                if (cardTag == niceTag)
                    Debug.LogError(card.UniqueID + " Contains Nice Tag");
            }
        }

        //Castables without a tag
        foreach (Card card in heroMenu.allCards)
        {
            if (card is Castable && card.cardTags.Count == 0 && !(card is NewWeapon))
            {
                Debug.LogError(card.UniqueID + " missing tags " + card.GetName());
            }
            else if (card is Castable && card.cardTags.Count == 1 && card.cardTags.Contains(lootTag) &&
                     !(card is NewWeapon))
            {
                Debug.LogError(card.UniqueID + " loot missing tags " + card.GetName());
            }
            else if (card is Castable && card.cardTags.Contains(null))
            {
                Debug.LogError(card.UniqueID + " null tag " + card.GetName());
            }
            else if (card == null)
            {
                Debug.LogError(card.UniqueID + " null card in allCards" + card.GetName());
            }

            foreach (CardTag cardTag in card.cardTags)
            {
                if (cardTag == null)
                {
                    Debug.LogError(card.UniqueID + " null cardTag " + card.gameObject.name);
                }
            }
        }

        foreach (Card card in heroMenu.unlockableCards)
        {
            if (card is Castable && card.cardTags.Count == 0)
            {
                Debug.LogError(card.UniqueID + " missing tags " + card.GetName());
            }
            else if (card is Castable && card.cardTags.Count == 1 && card.cardTags.Contains(lootTag))
            {
                Debug.LogError(card.UniqueID + " loot missing tags " + card.GetName());
            }
            else if (card is Castable && card.cardTags.Contains(null))
            {
                Debug.LogError(card.UniqueID + " null tag " + card.GetName());
            }

            foreach (CardTag cardTag in card.cardTags)
            {
                if (cardTag == null)
                {
                    Debug.LogError(card.UniqueID + " null cardTag " + card.gameObject.name);
                }
            }
        }

//记录所有buff和使用的component
        text = "";
        foreach (Effect effect in MenuControl.Instance.heroMenu.allEffects)
        {
            text += effect.UniqueID + " " + effect.name + " " + effect.GetName() + " " + effect.GetDescription() + "\n";
            if (effect == null)
            {
                Debug.LogError(effect.UniqueID + " null effect in allEffects" + effect.GetName());
            }

//get all effect's components
            foreach (var component in effect.GetComponents<Component>())
            {
                if (!(component is Transform))
                {
                    text += component.GetType() + "\n";
                }
            }

            text += "\n";
        }

        if (saveDebugData)
            File.WriteAllText("data\\Effect.txt", text);
        text = "";
        var missing = "";

        text = "";
        foreach (var chineseNameToTalentMapInfo in MenuControl.Instance.csvLoader.chineseNameToTalentMap)
        {
            bool found = false;
            foreach (var card in MenuControl.Instance.heroMenu.originalAllCards)
            {
                if (card is Skill && card.UniqueID == chineseNameToTalentMapInfo.Key)
                {
                    text += card.UniqueID + "\n";
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.LogError((chineseNameToTalentMapInfo.Key + " " + chineseNameToTalentMapInfo.Value));
            }
        }

        if (saveDebugData)
            File.WriteAllText("data\\talentRemap.txt", text);


        HashSet<string> visitedEncounterEnemy = new HashSet<string>();

        foreach (AdventureItem item1 in adventureMenu.allAdventureItems)
        {
            if (item1 is AdventureItemEncounter)
            {
                bool isEncounterInDemo =
                    MenuControl.Instance.csvLoader.EnemyGroupsChineseName.Contains(item1.GetChineseName());
                text += "\n" + item1.GetName() + " " + item1.GetDescription() + "\n";
                AdventureItemEncounter encounter = ((AdventureItemEncounter)item1);

                if (isEncounterInDemo)
                {
                    //检查special challenge
                    //检查是否设定了ferocityACards
                    bool hasFerocity = false;
                    var intentSystem = encounter.GetHero().GetIntentSystem();
                    foreach (var hand in intentSystem.hands)
                    {
                        if (hand.ferocityACards.Count > 0)
                        {
                            foreach (var card in hand.ferocityACards)
                            {
                                if (card == null)
                                {
                                    Debug.LogError("" + item1.name + " " + item1.UniqueID + " " +
                                                   item1.GetChineseName() + " ferocityACards 有null");
                                }
                            }

                            hasFerocity = true;
                        }
                    }

                    if (!hasFerocity)
                    {
                        Debug.LogError("" + item1.name + " " + item1.UniqueID + " " + item1.GetChineseName() +
                                       " 没有设定ferocityACards");
                    }

                    if (encounter.specialChallengeSkills.Count == 0)
                    {
                        Debug.LogError("" + item1.name + " " + item1.UniqueID + " " + item1.GetChineseName() +
                                       " 没有设定specialChallengeSkills");
                    }
                }


                foreach (Card card in encounter.CardsInPlay)
                {
                    if (card == null)
                    {
                        Debug.LogError(item1.name + " " + item1.UniqueID + " " + item1.GetChineseName() + " CardsInPlay中有null card");
                        continue;
                    }
                    text += "场上卡牌：" + card.GetName() + " " + card.GetDescription() + "\n";

                    //  if (/*isEncounterInDemo &&*/
                    //      !MenuControl.Instance.csvLoader.chineseNameToPlayerCardMap.ContainsKey(card.GetChineseName()))
                    if (!visitedEncounterEnemy.Contains(card.UniqueID))
                    {
                        visitedEncounterEnemy.Add(card.UniqueID);
                        missing += recordCard(card);
                        // missing+=$"{card.UniqueID} {card.name} {card.GetName()} {card.GetDescription()}\n";
                        // foreach (var component in card.GetComponents<Component>())
                        // {
                        //     if (!(component is Transform))
                        //     {
                        //         missing += component.GetType() + "\n";
                        //     }
                        // }
                        // missing += "\n";
                    }

                    if (card is Castable && card.cardTags.Count == 0)
                    {
                        Debug.LogError(card.UniqueID + " missing tags " + card.GetName());
                    }
                    else if (card is Castable && card.cardTags.Count == 1 && card.cardTags.Contains(lootTag))
                    {
                        Debug.LogError(card.UniqueID + " loot missing tags " + card.GetName());
                    }
                    else if (card is Castable && card.cardTags.Contains(null))
                    {
                        Debug.LogError(card.UniqueID + " null tag " + card.GetName());
                    }
                    else if (card.GetName() == card.UniqueID + "CardName" && checkNameAndDescription)
                    {
                        Debug.LogError(card.UniqueID + " missing name " + card.gameObject.name + " encounter: " +
                                       encounter.GetName());
                    }

                    if (card.GetDescription() == card.UniqueID + "CardDescription" && checkNameAndDescription)
                    {
                        Debug.LogError(card.UniqueID + " missing description " + card.gameObject.name + " encounter: " +
                                       encounter.GetName());
                    }

                    foreach (ApplyEffects applyEffectScript in card.GetComponents<ApplyEffects>())
                    {
                        foreach (Effect effect in applyEffectScript.templateEffects)
                        {
                            text += "效果：" + effect.GetName() + " " + effect.GetDescription() + "\n";
                            if (effect.GetName() == effect.UniqueID + "CardName")
                            {
                                Debug.LogError(effect.UniqueID + " missing name " + effect.gameObject.name +
                                               " encounter: " + encounter.GetName());
                            }

                            if (effect.GetDescription() == effect.UniqueID + "CardDescription")
                            {
                                Debug.LogError(effect.UniqueID + " missing description " + effect.gameObject.name +
                                               " encounter: " + encounter.GetName());
                            }
                        }
                    }

                    foreach (CardTag cardTag in card.cardTags)
                    {
                        if (cardTag == null)
                        {
                            Debug.LogError(card.UniqueID + " null cardTag " + card.gameObject.name + " encounter: " +
                                           encounter.GetName());
                        }
                    }
                }

                if (encounter.allOwnedCards == null)
                {
                    Debug.LogError($"encounter {encounter.GetChineseName()} does not have all owned cards");
                }

                foreach (Card card in encounter.allOwnedCards)
                {
                    text += "敌人卡牌：" + card.GetName() + " " + card.GetDescription() + "\n";

                    if (!visitedEncounterEnemy.Contains(card.UniqueID))
                    {
                        visitedEncounterEnemy.Add(card.UniqueID);
                        missing += recordCard((card));
                    }

                    if (card is Castable && card.cardTags.Count == 0)
                    {
                        Debug.LogError(card.UniqueID + " missing tags " + card.GetName());
                    }
                    else if (card is Castable && card.cardTags.Count == 1 && card.cardTags.Contains(lootTag))
                    {
                        Debug.LogError(card.UniqueID + " loot missing tags " + card.GetName());
                    }
                    else if (card is Castable && card.cardTags.Contains(null))
                    {
                        Debug.LogError(card.UniqueID + " null tag " + card.GetName());
                    }
                    else if (card.GetName() == card.UniqueID + "CardName")
                    {
                        if (!card.UniqueID.Contains("test") && checkNameAndDescription)

                            Debug.LogError(card.UniqueID + " missing name " + card.gameObject.name + " encounter: " +
                                           encounter.GetName());
                    }
                    else if (card.GetDescription() == card.UniqueID + "CardDescription")
                    {
                        if (!card.UniqueID.Contains("test") && checkNameAndDescription)

                            Debug.LogError(card.UniqueID + " missing description " + card.gameObject.name +
                                           " encounter: " + encounter.GetName());
                    }

                    if (card == null || encounter.allOwnedCards == null)
                    {
                        Debug.Log("???");
                    }

                    foreach (ApplyEffects applyEffectScript in card.GetComponents<ApplyEffects>())
                    {
                        foreach (Effect effect in applyEffectScript.templateEffects)
                        {
                            text += "效果：" + effect.GetName() + " " + effect.GetDescription() + "\n";
                            if (effect.GetName() == effect.UniqueID + "CardName")
                            {
                                if (checkNameAndDescription)
                                    Debug.LogError(effect.UniqueID + " missing name " + effect.gameObject.name +
                                                   " encounter: " + encounter.GetName());
                            }

                            if (effect.GetDescription() == effect.UniqueID + "CardDescription")
                            {
                                if (checkNameAndDescription)

                                    Debug.LogError(effect.UniqueID + " missing description " + effect.gameObject.name +
                                                   " encounter: " + encounter.GetName());
                            }
                        }
                    }

                    foreach (CardTag cardTag in card.cardTags)
                    {
                        if (cardTag == null)
                        {
                            Debug.LogError(card.UniqueID + " null cardTag " + card.gameObject.name + " encounter: " +
                                           encounter.GetName());
                        }
                    }
                }

                if (encounter.GetHero().GetComponent<IntentSystemHand>() != null)
                {
                    foreach (IntentSystemHand hand in encounter.GetHero().GetComponents<IntentSystemHand>())
                    {
                        foreach (Card card in hand.cards)
                        {
                            text += "手牌卡牌：" + card.GetName() + " " + card.GetDescription() + "\n";

                            if (!visitedEncounterEnemy.Contains(card.UniqueID))
                            {
                                visitedEncounterEnemy.Add(card.UniqueID);
                                missing += recordCard((card));
                            }

                            if (card is Castable && card.cardTags.Count == 0)
                            {
                                Debug.LogError(card.UniqueID + " missing tags " + card.GetName());
                            }
                            else if (card is Castable && card.cardTags.Count == 1 && card.cardTags.Contains(lootTag))
                            {
                                Debug.LogError(card.UniqueID + " loot missing tags " + card.GetName());
                            }
                            else if (card is Castable && card.cardTags.Contains(null))
                            {
                                Debug.LogError(card.UniqueID + " null tag " + card.GetName());
                            }
                            else if (card.GetName() == card.UniqueID + "CardName")
                            {
                                Debug.LogError(card.UniqueID + " missing name " + card.gameObject.name +
                                               " encounter: " + encounter.GetName());
                            }
                            else if (card.GetDescription() == card.UniqueID + "CardDescription")
                            {
                                Debug.LogError(card.UniqueID + " missing description " + card.gameObject.name +
                                               " encounter: " + encounter.GetName());
                            }

                            foreach (ApplyEffects applyEffectScript in card.GetComponents<ApplyEffects>())
                            {
                                foreach (Effect effect in applyEffectScript.templateEffects)
                                {
                                    text += "效果：" + effect.GetName() + " " + effect.GetDescription() + "\n";
                                    if (effect.GetName() == effect.UniqueID + "CardName")
                                    {
                                        if (checkNameAndDescription)
                                            Debug.LogError(effect.UniqueID + " missing name " + effect.gameObject.name +
                                                           " encounter: " + encounter.GetName());
                                    }

                                    if (effect.GetDescription() == effect.UniqueID + "CardDescription")
                                    {
                                        if (checkNameAndDescription)
                                            Debug.LogError(effect.UniqueID + " missing description " +
                                                           effect.gameObject.name + " encounter: " +
                                                           encounter.GetName());
                                    }
                                }
                            }


                            foreach (CardTag cardTag in card.cardTags)
                            {
                                if (cardTag == null)
                                {
                                    Debug.LogError(card.UniqueID + " null cardTag " + card.gameObject.name +
                                                   " encounter: " + encounter.GetName());
                                }
                            }
                        }
                    }
                }
            }
        }

        if (saveDebugData)
        {
            File.WriteAllText("data\\Encounter.txt", text);
            File.WriteAllText("data\\EncounterMissingEnemyCards.txt", missing);
        }

        //text = "";
        //Area Encounters in full list
        // foreach (Area area in areaMenu.allAreas)
        // {
        //     text += "\n" + area.GetName() + " " + area.GetDescription() + "\n";
        //     foreach (AdventureItemEncounter encounter in area.enemies)
        //     {
        //         text += encounter.GetName() + " " + encounter.GetDescription() + "\n";
        //         int count = 0;
        //         foreach (AdventureItem item1 in adventureMenu.allAdventureItems)
        //         {
        //             if (item1 == encounter)
        //             {
        //                 count += 1;
        //             }
        //         }
        //
        //         if (count == 0)
        //         {
        //             Debug.LogError(encounter.UniqueID + " encounter missing in AllAdventureItems " +
        //                            encounter.GetName() + " area " + area.GetName());
        //         }
        //
        //         if (count > 1)
        //         {
        //             Debug.LogError(encounter.UniqueID + " encounter duplicated in AllAdventureItems " +
        //                            encounter.GetName() + " area " + area.GetName());
        //         }
        //     }
        // }

        if (saveDebugData)
            File.WriteAllText("data\\Area.txt", text);
        text = "";
        var upgradeText = "";
        //Missing card names or descriptions
        foreach (Card card in MenuControl.Instance.heroMenu.originalAllCards)
        {
            if (card is null)
            {
                continue;
            }

            //text +=  "\n"+card.UniqueID+" "+card.GetChineseName() + " " + card.GetDescription() + "\n";
            text += "消耗 " + card.initialCost + " 金币 " + card.goldWorth + "\n";
            if (heroMenu.cardDict.ContainsKey((card.UniqueID)) && card.upgradeCards.Count>0)
            {
                foreach (var upgradeCard in card.upgradeCards)
                {
                    
                    text += "升级为 " + upgradeCard.GetChineseName() + "\n";
                    // if (card.level == 1)
                    {
                        //upgradeText += "\n" + card.UniqueID + "," + upgradeCard.UniqueID;
                    }
                }
            }


            if (card.cardTags != null && card.cardTags.Count > 0)
            {
                text += "card tags\n";
                foreach (CardTag cardTag in card.cardTags)
                {
                    if (cardTag != null)
                    {
                        text += cardTag + "\n";
                    }
                }
            }

            if (card is Unit unit)
            {
                text += "血量 " + unit.initialHP + " 攻击 " + unit.initialPower + "\n";
            }

            if (MenuControl.Instance.heroMenu.allCards.Contains(card))
            {
                if (card.GetName() == card.UniqueID + "CardName")
                {
                    Debug.LogError(card.UniqueID + " missing name " + card.gameObject.name);
                }

                if (card.GetDescription() == card.UniqueID + "CardDescription")
                {
                    Debug.LogError(card.UniqueID + " missing description " + card.gameObject.name);
                }
            }

            // foreach (ApplyEffects applyEffectScript in card.GetComponents<ApplyEffects>())
            // {
            //     foreach (Effect effect in applyEffectScript.templateEffects)
            //     {
            //         if (effect == null)
            //         {
            //             Debug.LogError($"effect in {card.name} is null");
            //         }
            //         else
            //         {
            //             text +=  "效果： "+effect.GetName() + " " + effect.GetDescription() + "\n";
            //             if (effect is PowerModifierMultiply)
            //             {
            //                 Debug.Log(card.gameObject.name + " using PowerModifierMultiply");
            //             }
            //         }
            //     }
            // }
            //text +=  "Ability ： "+card + "\n";
            // List<Component> recordComponents = new List<Component>();
            // foreach(Ability script in card.GetComponents<Ability>())
            // {
            //     recordComponents.Add(script);
            // }
            // foreach(Trigger script in card.GetComponents<Trigger>())
            // {
            //     recordComponents.Add(script);
            // }

            // foreach (var script in recordComponents)
            // { if (script is Trigger)
            //     {
            //         text +=  "Trigger ： "+script + "\n";
            //     }
            //     else if (script is Ability)
            //     {
            //         text +=  "Ability ： "+script + "\n";
            //     }
            // }


            //if(!visitedEncounterEnemy.Contains(card.UniqueID))
            {
                //visitedEncounterEnemy.Add(card.UniqueID);
                text += recordCard(card);
            }
        }


        if (saveDebugData)
        {
            File.WriteAllText("data\\Cards.txt", text);
            //File.WriteAllText("data\\UpgradeCards.txt", upgradeText);
        }


        text = "";
        var textName = "";
        foreach (var achievement in Instance.achievementsMenu.achievements)
        {
            text += "\n" + achievement.name + " " + achievement.GetName() + " " + achievement.GetDescription() + "\n";
            textName += "\n" + achievement.GetName();
        }

        if (saveDebugData)
        {
            File.WriteAllText("data\\Achievements.txt", text);
            File.WriteAllText("data\\AchievementsName.txt", textName);
        }

        // foreach (Effect effect in MenuControl.Instance.heroMenu.allEffects)
        // {
        //     if (effect.GetName() == effect.UniqueID + "CardName")
        //     {
        //         Debug.LogError(effect.UniqueID + " missing name " + effect.gameObject.name);
        //     }
        //     if (effect.GetDescription() == effect.UniqueID + "CardDescription")
        //     {
        //         Debug.LogError(effect.UniqueID + " missing description " + effect.gameObject.name);
        //     }
        // }

        text = "";
        var mapText = "";
        foreach (AdventureItem item1 in adventureMenu.allAdventureItems)
        {
            mapText += "\n" + item1.GetName(); //+"," + item1.name;
            text += "\n" + item1.GetType() + " " + item1.name + " " + item1.GetName() + " " + item1.GetDescription() +
                    "\n";
            if (item1.GetName() == item1.UniqueID + "CardName")
            {
                if (checkNameAndDescription)
                    Debug.LogError(item1.UniqueID + " missing name " + item1.gameObject.name);
            }

            if (item1.GetDescription() == item1.UniqueID + "CardDescription")
            {
                if (checkNameAndDescription)
                    Debug.LogError(item1.UniqueID + " missing description " + item1.gameObject.name);
            }

            foreach (EventChoice choice in item1.GetComponentsInChildren<EventChoice>())
            {
                text += "选择: " + choice.GetType() + " " + choice.GetName() + " " + choice.GetDescription() + "\n";
                if (choice.GetName() == choice.UniqueID + "CardName")
                {
                    if (checkNameAndDescription)
                        Debug.LogError(choice.UniqueID + " missing name " + choice.gameObject.name);
                }

                if (choice.GetDescription() == choice.UniqueID + "CardDescription")
                {
                    if (checkNameAndDescription)

                        Debug.LogError(choice.UniqueID + " missing description " + choice.gameObject.name);
                }
            }

            foreach (EventDefinition choice in item1.GetComponentsInChildren<EventDefinition>())
            {
                text += "定义: " + choice.GetType() + " " + choice.GetName() + " " + choice.GetDescription() + "\n";
                if (choice.GetName() == choice.UniqueID + "CardName")
                {
                    if (checkNameAndDescription)
                        Debug.LogError(choice.UniqueID + " missing name " + choice.gameObject.name);
                }

                if (choice.GetDescription() == choice.UniqueID + "CardDescription")
                {
                    if (checkNameAndDescription)

                        Debug.LogError(choice.UniqueID + " missing description " + choice.gameObject.name);
                }
            }
        }


        if (saveDebugData)
        {
            File.WriteAllText("data\\AdventureItem.txt", text);
            File.WriteAllText("data\\AdventureItemMap.txt", mapText);
        }

        //???
        // Card card1 = heroMenu.GetCardByID("Loot01");
        // //Debug.Log(card1.GetPointScore());
        // //foreach (string synergyTag in card1.GetSynergyTags())
        // //{
        // //    Debug.Log(synergyTag);
        // //}
        // //Debug.Log(card1.GetBaseDropScore());
        // //Debug.Log(card1.GetPremiumScore());
        //
        // card1.GetPointScore();
        // card1.GetSynergyTags();
        // card1.GetBaseDropScore();
        // card1.GetPremiumScore();


        //Duplicate IDs check
        foreach (AdventureItem item1 in adventureMenu.allAdventureItems.ToArray())
        {
            if (item1.UniqueID != "EncounterSpiderNest" && item1.UniqueID != "EventPurchase")
            {
                foreach (AdventureItem item2 in adventureMenu.allAdventureItems.ToArray())
                {
                    if (item1.UniqueID == item2.UniqueID && item1 != item2)
                    {
                        Debug.LogError(item1.UniqueID + " duplicate adventure item ID for " + item1.name + " and " +
                                       item2.name);
                    }
                }
            }
        }

        foreach (Card item1 in heroMenu.allCards)
        {
            foreach (Card item2 in heroMenu.allCards)
            {
                if (item1.UniqueID == item2.UniqueID && item1 != item2)
                {
                    if(checkIMPORTANT)
                    Debug.LogError(item1.UniqueID + " 重复的 unique ID for " + item1.name + " and " + item2.name);
                }
            }
        }

        //Encounters with Starting cards positions dont have randomised player placement
        foreach (AdventureItem item1 in adventureMenu.allAdventureItems)
        {
            if (item1 is AdventureItemEncounter)
            {
                AdventureItemEncounter encounter = ((AdventureItemEncounter)item1);
                if (encounter.CardsInPlay.Count > 0 || encounter.PositionsInPlay.Count > 0)
                {
                    if (encounter.CardsInPlay.Count != encounter.PositionsInPlay.Count)
                    {
                        Debug.LogError(item1.gameObject.name + " " + item1.UniqueID +
                                       " 的CardsInPlay和PositionsInPlay的数量不匹配");
                    }

                    if (encounter.randomStartingPositions)
                    {
                        //Debug.LogError(item1.gameObject.name+" "+item1.UniqueID + " 不能同时有随机位置randomStartingPositions和场上仆从CardsInPlay");
                    }
                }

                if (encounter.minObstacleCount > encounter.maxObstacleCount)
                {
                    Debug.LogError(encounter.UniqueID + " minObastableCount 比 maxObastableCount 大");
                }
            }
        }
    }

    string recordCard(Card card)
    {
        var text = "";
        //visitedEncounterEnemy.Add(card.UniqueID);
        text += $"{card.UniqueID} {card.name} {card.GetName()} {card.GetDescription()}\n";
        foreach (var component in card.GetComponents<Component>())
        {
            if (!(component is Transform))
            {
                text += component.GetType() + "\n";


                if (component is ApplyEffects)
                {
                    foreach (var effect in ((ApplyEffects)component).templateEffects)
                    {
                        if (effect == null)
                        {
                            if(checkIMPORTANT)
                            Debug.LogError($"Null effect in ApplyEffects in {card.name} {card.UniqueID}");
                            continue;
                        }

                        text += "\t" + effect.UniqueID + " " + effect.GetName() + " " + effect.GetDescription() + "\n";
                    }
                }
            }
        }

        text += "\n";
        return text;
    }

    public string GetBuildLabelText()
    {
        return Application.version;
    }

    public int CountOfCardsInList(Card card, List<Card> cards)
    {
        int count = 0;
        foreach (Card card1 in cards)
        {
            if (card1.UniqueID == card.UniqueID)
            {
                count += 1;
            }
        }

        return count;
    }

    public void LogEvent(string eventString)
    {
#if !UNITY_EDITOR
        if (!testMode)
            Analytics.CustomEvent(eventString);

        Debug.Log(eventString);

#endif
    }
}