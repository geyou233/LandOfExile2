using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using Spine.Unity;
using UnityEngine.Serialization;


public class HeroMenu : BasicMenu
{
    
    
    public Hero hero;
    public int initialMana;
    public int drawsPerTurn;

    [HideInInspector]public bool hasInitHero;
    public List<Card> cardsOwned = new List<Card>();

    public List<Card> cardsInDeck()
    {
        var cards = new List<Card>(cardsOwned);
        cards.RemoveAll(x => x is Hero);
        cards.RemoveAll(x => x.isPotion);
        return cards;
    }

    public Transform classSelectionIndicatorParent;
    SelectIndicator[] classSelectionIndicators;

    public GameObject[] lockHiddenParts;
    public List<Card> weaponsOwned = new List<Card>();
    public List<Card> artifactsOwned = new List<Card>();

    public List<Card> artifactsEquipped = new List<Card>();

    public List<TargetValidator> allMovementTypes = new List<TargetValidator>();
    public List<Effect> allEffects = new List<Effect>();

    //移动到resource后load出来cards和effects
    public bool useResource = true;
    
    public List<Card> allCards = new List<Card>();
    public Dictionary<string,Card> cardDict = new Dictionary<string, Card>();
    List<Card> testAllCards = new List<Card>();
    List<Effect> testAllEffects = new List<Effect>();
    
    public Dictionary<string, List<List<Card>>> separatedCards = new Dictionary<string, List<List<Card>>>();

    
    public List<Card> originalAllCards = new List<Card>();
    public List<Card> enemyCards = new List<Card>();

    public List<bool> unlockedClasses = new List<bool>();
    
    public List<bool> finishedClasses = new List<bool>();
    public List<bool> finishedUnlockVisualizationClasses = new List<bool>();
    
    public List<HeroClass> heroClasses = new List<HeroClass>();
    public List<int> heroLevels = new List<int>();
    public List<int> heroExperiences = new List<int>();
    public List<HeroPath> heroPaths = new List<HeroPath>();

    public List<UnlockableStartingCards> unlockableStartingCardLists = new List<UnlockableStartingCards>();
    public List<Card> unlockableCards = new List<Card>();
    public List<int> unlockableCardCosts = new List<int>();
    public List<Achievement> unlockableCardAchievements = new List<Achievement>();

    public List<int> levelsXP = new List<int>();

    //public float shiftDistance = 4f;
    public Text seedText;

    public Doozy.Engine.UI.UIView classTooltipView;
    public Doozy.Engine.UI.UIView pathTooltipView;
    public Doozy.Engine.UI.UIView ascensionTooltipView;

    public Image pathImage;
    public Text pathNameText;
    public Doozy.Engine.UI.UIView pathDescriptionUIView;
    public Text pathTitleText;
    public Text pathDescriptionText;

    public Transform classCardsPanel;

    public List<Card> startingCardsUnlocked = new List<Card>();
    public List<string> startingCardsUnlockedNames = new List<string>();
    public List<Card> artifactsUnlocked = new List<Card>();
    public StartingCardContainer startingCardContainerPrefab;

    //public Text accumulatedGoldText;
    // public Text gameModeText;
    // public Image gameModeLabelImage;
    // public List<Sprite> gameModeButtonLabels = new List<Sprite>();
    // public Doozy.Engine.UI.UIView gameModePanelView;

    public Doozy.Engine.UI.UIView editPanel;

    public Button editDeckButton;
    public Button createHeroButton;
    public Button draftHeroButton;
    public Button unlockClassButton;
    public Text deckNameText;
    public GameObject rhsPanel;
    public Text ascensionText;
    public int ascensionMode;
    public Button nextAscensionButton;
    public Button prevAscensionButton;

    //Variables
    public HeroClass heroClass;
    public HeroPath heroPath;

    public int flareStones;
    public int flareStoneShards;
    public int flareStoneWhenSell = 3;
    //public int accumulatedGold;
    public int currentXP;
    public int currentLevel;
    public bool isAlive;
    public int damageDealtThisRun;
    public int turnsUsedThisRun;
    //public int goldConvertedThisRun;

    public bool seasonsMode;
    public bool foundSanta;
    public bool foundKrampus;
    public bool easyMode;
    public bool reaperMode;
    public int reaperProgress;
    public List<int> reaperProgressHeroPathInts = new List<int>();

    public List<string> achievementStringsCompletedThisRun = new List<string>();

    public GameObject iAPButton;

    //public Doozy.Engine.UI.UIView heroNamePanelView;
    //public InputField heroNameInputField;

    public string HeroName => MenuControl.Instance.GetLocalizedString(heroClass.UniqueID + "HeroName")/*heroNameText.text*/;
    public Doozy.Engine.UI.UIView PathMenu;

    public List<string[]> dropRateLines = new List<string[]>();
    public int synergisticDropsSkipped;
    public int dropsSinceLastTreasureDropped;
    public int extraLootCardLastOffered;

    public VisibleCard selectedVisibleCard;
    public VisibleCard ghostVisibleCard;
    public VisibleCard draggedVisibleCard;

    public Card healingPotionTemplate;
    public DateTime startDate;

    public Transform decklistVCs;
    public Image largeHeroImage;
    
    public SkeletonGraphic largeHeroAnimation;
    
    public Image heroIcon;

    public Text heroClassNameText;
    public Text heroDescText;
    
    // left bar, hero info
    
    public Text heroNameText;
    
    public Color attributeOriginalColor;
    public Color attributeModifiedColor;
    
    public Text attackAttributeText;
    public Text hpAttributeText;
    public GameObject hpAttributeTooltip;
    
    
    
    public Text hpIncreaseAttributeText;
    public List<HeroTalentCell> startTalents = new List<HeroTalentCell>();

    public int cardsDiscoveredThisRun;
    [HideInInspector] public int encountersNormalWonThisRun;
    [HideInInspector] public int encountersEliteWonThisRun;
    [HideInInspector] public int encountersBossWonThisRun;
    public int startingDeckInt;

    public bool draftMode;
    public List<Card> draftCards = new List<Card>();
    public bool pickingSpells;
    public List<int> ascensionUnlocks = new List<int>();
    public int maxAscensionLevels = 5;
    public List<string> startOfBattleHandCardIDs = new List<string>();
    public int artifactSlots;
    public bool skippedLastLootDrops;
    public List<Card> originalStartingCards = new List<Card>();
    public Effect wakingNightmareEffectTemplate;
    public Effect easyModeEffectTemplate;

    public Effect seasonsModeEffectTemplate;

    public int seasonsLootCountDown;

    public int levelToIncreaseHp = 2;
    
    public int GetHeroClassIndex()
    {
         return heroClasses.IndexOf(heroClass);
    }
    public int getHeroExperience(int index)
    {
        while (index >= heroExperiences.Count)
        {
            heroExperiences.Add(0);
        }

        return heroExperiences[index];
    }
    public void setHeroExperience(int index,int value)
    {
        while (index >= heroExperiences.Count)
        {
            heroExperiences.Add(0);
        }

        heroExperiences[index] = value;
    } 
    public int getHeroLevel(int index)
    {
        while (index >= heroLevels.Count)
        {
            heroLevels.Add(1);
        }

        return heroLevels[index];
    }
    public void setHeroLevel(int index,int value)
    {
        while (index >= heroLevels.Count)
        {
            heroLevels.Add(0);
        }

        heroLevels[index] = value;
    }
    private void Start()
    {
        classSelectionIndicators = classSelectionIndicatorParent.GetComponentsInChildren<SelectIndicator>();
        SelectPath(heroPath);
        SelectClass(heroClass);
        //set unlockable cards
        unlockableCards.Clear();
        foreach (var card in allCards)
        {
            if (MenuControl.Instance.csvLoader.isLocked(card.UniqueID))
            {
                unlockableCards.Add(card);
            }
        }

        foreach (var heroClass in heroClasses)
        {
            heroClass.Init();
        }

        foreach (var heroPath in heroPaths)
        {
            heroPath.Init();
        }
    }

    public void consumeFlareStone(int amount)
    {
        flareStones -= amount;
        GameObject.FindObjectOfType<HeroInfoPanel>()?.showAddStone(-amount);
    }
    
    public void addFlareStone(int amount)
    {
        flareStones += amount;
        if (flareStones < 0)
        {
            var diff = -flareStones;
            flareStones = 0;
            amount += diff;
            
        }

        if (amount != 0)
        {
            GameObject.FindObjectOfType<HeroInfoPanel>()?.showAddStone(amount);
        }
    }

    public bool isLocked(Card card)
    {
        if (MenuControl.Instance.csvLoader.isLocked(card.UniqueID))
        {
            if(startingCardsUnlockedNames.Contains(card.UniqueID))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public int getCurrentClassIndex()
    {
        return heroClasses.IndexOf(heroClass);
    }

    public int getCurrentClassIndexReal()
    {
        
        return heroClasses.IndexOf(heroClass)+1;
    }
    public void Update()
    {
        if (gameObject.activeInHierarchy && ghostVisibleCard != null && Input.GetMouseButton(0))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            ghostVisibleCard.transform.position = new Vector2(worldPoint.x, worldPoint.y);
        }
    }

    public void StartNewHero(bool newSeed, bool draftMode, bool reaperMode, bool easyMode, bool seasonsMode)
    {
        if (newSeed)
        {
            seedText.text = MenuControl.Instance.GetLocalizedString("Seed") + ": " +
                            MenuControl.Instance.GenerateSeed();
        }
        else
        {
            seedText.text = MenuControl.Instance.GetLocalizedString("Seed") + ": " + MenuControl.Instance.currentSeed;
        }

        this.draftMode = draftMode;
        this.reaperMode = reaperMode;
        this.easyMode = easyMode;
        this.seasonsMode = seasonsMode;
        ShowMenu();

        // if (draftMode)
        // {
        //     createHeroButton.gameObject.SetActive(false);
        //     draftHeroButton.gameObject.SetActive(true);
        //     rhsPanel.SetActive(false);
        // }
        // else
        // {
        //     createHeroButton.gameObject.SetActive(true);
        //     draftHeroButton.gameObject.SetActive(false);
        //     rhsPanel.SetActive(true);
        // }

        MenuControl.Instance.dataControl.SaveData();
    }

    public void ShowModifiedHpTooltip()
    {
        hpAttributeTooltip.SetActive(true);
        hpAttributeTooltip.GetComponentInChildren<Text>().text = string.Format(
            MenuControl.Instance.GetLocalizedString("HPAttributeTooltip"), levelToIncreaseHp,
            heroClass.InitialHpAfterLevel() - heroClass.initialHP, heroClass.level);
    }

    public void HideModifiedHpTooltip()
    {
        hpAttributeTooltip.SetActive(false);
    }

    IEnumerator test()
    {
        
        MenuControl.Instance.victoryMenu.ShowMenu();
        yield return new WaitForSeconds(2);
        MenuControl.Instance.victoryMenu.HideMenu();
    }
    public override void ShowMenu()
    {
        base.ShowMenu();
        HideModifiedHpTooltip();
        //StartCoroutine((test()));
        
        //reset all kinds of data
        isAlive = false;
        achievementStringsCompletedThisRun.Clear();
        foreach (Card card in cardsOwned)
        {
            if (!(card is Hero))
            {
                Destroy(card.gameObject);
            }
        }

        cardsOwned.Clear();
        weaponsOwned.Clear();
        startOfBattleHandCardIDs.Clear();

        MenuControl.Instance.levelUpMenu.variableTalentsAcquired.Clear();

        MenuControl.Instance.adventureMenu.randomAdventureItems.Clear();

        currentLevel = 1;
        currentXP = 0;
        flareStones = 3;
        flareStoneShards = 0;
        cardsDiscoveredThisRun = 0;
        encountersNormalWonThisRun = 0;
        encountersEliteWonThisRun = 0;
        encountersBossWonThisRun = 0;
        damageDealtThisRun = 0;
        skippedLastLootDrops = false;

        //artifactSlots = 2;
        MenuControl.Instance.heroMenu.artifactsOwned.Clear();
        MenuControl.Instance.heroMenu.artifactsEquipped.Clear();

        synergisticDropsSkipped = 0;
        dropsSinceLastTreasureDropped = 0;
        extraLootCardLastOffered = 0;

        turnsUsedThisRun = 0;
        //goldConvertedThisRun = 0;

        foundSanta = false;
        foundKrampus = false;
        seasonsLootCountDown = 0;

        MenuControl.Instance.battleMenu.ResetBattle();

        MenuControl.Instance.areaMenu.currentArea = null;
        MenuControl.Instance.areaMenu.currentAreaComplete = true;
        MenuControl.Instance.areaMenu.areasVisited = 0;

        MenuControl.Instance.shopMenu.purchases = 0;
        MenuControl.Instance.shopMenu.removals = 0;
        MenuControl.Instance.shopMenu.upgrades = 0;
        MenuControl.Instance.shopMenu.purchaseRefreshCount = 0;
        MenuControl.Instance.shopMenu.currentRemoveCardCost = MenuControl.Instance.shopMenu.removeCardStartCost;
        MenuControl.Instance.eventMenu.orderedStorysEventIndexThisRound.Clear();


        //pathDescriptionUIView.Hide(true);
        //gameModePanelView.Hide(true);
        //heroNamePanelView.Hide(true);
        
        if (heroClass == null)
            heroClass = heroClasses[0];

        if (MenuControl.Instance.DemoMode)
        {
            var index = GetHeroClassIndex();
            var isUnlocked = isHeroUnlocked(index);
            if (!isUnlocked)
            {
                heroClass = heroClasses[0];
            }
        }
        // if (heroPath == null)
        //     heroPath = heroPaths[0];
        heroPath = heroPaths[GetHeroClassIndex()];
        // if (MenuControl.Instance.demoMode)
        // {
        //     heroPath = heroPaths[1];
        // }

        if (reaperMode)
        {
            heroPath = heroPaths[MenuControl.Instance.heroMenu.reaperProgressHeroPathInts[reaperProgress]];
        }


        //accumulatedGoldText.text = accumulatedGold.ToString();

        if (classSelectionIndicators != null)
        {
            SelectPath(heroPath);
            SelectClass(heroClass);
        }

        //SetGameMode((int)gameMode);
//        editPanel.Hide(true);

//        classTooltipView.Hide(true);
////        pathTooltipView.Hide(true);
  //      ascensionTooltipView.Hide(true);
    }

    // public UnlockableStartingCards GetUnlockbleCardList()
    // {
    //     // foreach (UnlockableStartingCards cardsList in unlockableStartingCardLists)
    //     // {
    //     //     if (cardsList.heroPath == heroPath && cardsList.heroClass == heroClass)
    //     //     {
    //     //         return cardsList;
    //     //     }
    //     // }
    //     foreach (UnlockableStartingCards cardsList in heroClass.startCards)
    //     {
    //         if (cardsList.heroPath == heroPath && cardsList.heroClass == heroClass)
    //         {
    //             return cardsList;
    //         }
    //     }
    //
    //     return null;
    // }

    public List<Card> GetUnlockedCards()
    {
        List<Card> cards = new List<Card>();

        foreach (Card card in CurrentHeroUnlockableCards())
        {
            if (isCardAvailable(card))
            {
                
                if (startingCardsUnlockedNames.Contains(card.UniqueID))
                {
                    cards.Add(card);
                }
            }
            
        }

        return cards;
    }

    public List<Card> GetUnlockedCardsExceptTreasureAndArtifact()
    {
        List<Card> cards = new List<Card>();

        foreach (Card card in CurrentHeroUnlockableCards())
        {
            if (isCardAvailable(card))
            {
                if (card.IsTreasure() || card is Artifact)
                {
                    continue;
                }

                if (!card.IsCurrentClassCard())
                {
                    continue;
                }
                if (startingCardsUnlockedNames.Contains(card.UniqueID))
                {
                    cards.Add(card);
                }
            }
            
        }

        return cards;
    }

    public List<Card> GetUnlockedSpellCards(bool exceptTreasure = false)
    {
        List<Card> cards = new List<Card>();

        foreach (Card card in GetUnlockedCards())
        {
            if (card.cardTags.Contains(MenuControl.Instance.spellTag))
            {
                if (exceptTreasure && card.IsTreasure())
                {
                    continue;
                }
                cards.Add(card);
            }
        }

        // if (draftMode)
        // {
        //     //Add other class cards
        //     foreach (HeroClass anotherClass in heroClasses)
        //     {
        //         if (heroClass != anotherClass)
        //         {
        //             cards.AddRange(anotherClass.classCards);
        //         }
        //     }
        // }

        return cards;
    }
    public List<Card> GetUnlockedMinionCardsWithoutTreasure()
    {
        List<Card> cards = new List<Card>();

        foreach (Card card in GetUnlockedCards())
        {
            if (card is Minion && isCardAvailable(card) && !card.IsTreasure())
                cards.Add(card);
        }

        // if (draftMode)
        // {
        //     //Add other path cards
        //     foreach (HeroPath anotherPath in heroPaths)
        //     {
        //         if (heroClass != anotherPath)
        //         {
        //             cards.AddRange(anotherPath.pathCards);
        //         }
        //     }
        // }

        return cards;
    }
    public List<Card> GetUnlockedMinionCards()
    {
        List<Card> cards = new List<Card>();

        foreach (Card card in GetUnlockedCards())
        {
            if (card is Minion && isCardAvailable(card))
                cards.Add(card);
        }

        // if (draftMode)
        // {
        //     //Add other path cards
        //     foreach (HeroPath anotherPath in heroPaths)
        //     {
        //         if (heroClass != anotherPath)
        //         {
        //             cards.AddRange(anotherPath.pathCards);
        //         }
        //     }
        // }

        return cards;
    }

    public override void CloseMenu()
    {
        base.CloseMenu();
        MenuControl.Instance.infoMenu.HideMenu();
        //heroNamePanelView.Hide(true);
       // gameModePanelView.Hide(true);
    }

    public void HideInfo()
    {
        MenuControl.Instance.infoMenu.HideMenu();

        if (pathDescriptionUIView.IsActive()) pathDescriptionUIView.Hide();
        if (classTooltipView.IsActive()) classTooltipView.Hide();
        if (pathTooltipView.IsActive()) pathTooltipView.Hide();
        if (ascensionTooltipView.IsActive()) ascensionTooltipView.Hide();
    }

    public void PressClassInfo()
    {
        MenuControl.Instance.infoMenu.ShowInfo(hero, heroIcon.transform.position,false);
        classTooltipView.Show();
    }

    public void PressClassSkills()
    {
        MenuControl.Instance.levelUpMenu.ShowCharacterSheet();
    }

    public void PressPathInfo()
    {
        if (!pathDescriptionUIView.IsVisible) pathDescriptionUIView.Show();
        pathTooltipView.Show();
    }

    public void PressAscensionInfo()
    {
        ascensionTooltipView.GetComponentInChildren<Text>().text = "";
        for (int ii = 1; ii <= maxAscensionLevels; ii += 1)
        {
            string stringToShow = (ascensionTooltipView.GetComponentInChildren<Text>().text == "" ? "" : "\n") +
                                  ii.ToString() + ". " +
                                  MenuControl.Instance.GetLocalizedString("AscensionModeDescription" + ii);
            if (ii <= ascensionMode)
            {
                stringToShow = "<color=white>" + stringToShow + "</color>";
            }

            ascensionTooltipView.GetComponentInChildren<Text>().text += stringToShow;
        }

        ascensionTooltipView.GetComponentInChildren<Text>().text =
            MenuControl.Instance.GetLocalizedString("AscensionModeDescription0") + "\n" +
            ascensionTooltipView.GetComponentInChildren<Text>().text;

        if (!ascensionTooltipView.IsVisible) ascensionTooltipView.Show();
    }

    public void SelectClass(HeroClass heroClass)
    {
        if (this.heroClass != heroClass) startingDeckInt = 0;

        int classIndex = heroClasses.IndexOf(heroClass);

        foreach (var indicator in classSelectionIndicators)
        {
            indicator.selectedIndicator.gameObject.SetActive(false);
        }
        classSelectionIndicators[classIndex].selectedIndicator.SetActive(true);

        var unlocked = isHeroUnlocked(classIndex);
       
            foreach (var part in lockHiddenParts)
            {
                part.SetActive(unlocked);
            }
        

        //update hero class info
        this.heroClass = heroClass;

        heroIcon.sprite = heroClass.iconSprite;

        if (unlocked)
        {
            largeHeroAnimation.gameObject.SetActive((true));
            largeHeroImage.gameObject.SetActive(false);
                largeHeroAnimation.skeletonDataAsset = heroClass.spineAsset;

            // Reinitialize the skeleton with the new data asset
            largeHeroAnimation.Initialize(true);

            largeHeroAnimation.AnimationState.SetAnimation(0, "idle", true);
        }
        else
        {
            
            largeHeroAnimation.gameObject.SetActive((false));
            largeHeroImage.gameObject.SetActive(true);
            largeHeroImage.sprite = heroClass.largeHeroSelectSprite;
        
            largeHeroImage.color = unlocked? Color.white : new Color(0.1f,0.1f,0.1f,1);
        }

        heroClassNameText.text = heroClass.GetName();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(heroClassNameText.GetComponent<RectTransform>());
        
        // classTooltipView.GetComponentInChildren<Text>().text =
        //     MenuControl.Instance.GetLocalizedString(heroClass.UniqueID + "Tooltip");

        var descText = unlocked
            ? MenuControl.Instance.GetLocalizedString(heroClass.UniqueID + "Tooltip")
            : MenuControl.Instance.GetLocalizedString("Hero"+classIndex + "_Unlock");
        
        heroDescText.text =  descText;

        if (hero != null)
        {
            Destroy(hero.gameObject);
            hero = null;
        }

        InitalizeHero();

        hero.initialHP = heroClass.InitialHpAfterLevel();
        hero.currentHP = hero.GetInitialHP();
        initialMana = heroClass.initialMana;
        drawsPerTurn = heroClass.initialCardsDrawnPerTurn;


        originalStartingCards[0] = hero;
        
        
        
        // update hero detail info ( left panel)
        heroNameText.text = MenuControl.Instance.GetLocalizedString(heroClass.UniqueID + "HeroName");
        
        
        attackAttributeText.text = hero.initialPower.ToString();


        if (heroClass.level > levelToIncreaseHp)
        {
            hpAttributeText.text = heroClass.InitialHpAfterLevel().ToString();
            hpAttributeText.color = attributeModifiedColor;
        }
        else
        {
            hpAttributeText.text = heroClass.initialHP.ToString();
            hpAttributeText.color = attributeOriginalColor;
        }
        
        hpIncreaseAttributeText.text = heroClass.hpGainPerLevel.ToString();

        if (heroClass.startTalents.Count > startTalents.Count)
        {
            Debug.LogError("初始天赋太多了");
        }
        for (int i = 0; i <Math.Min(heroClass.startTalents.Count, startTalents.Count) ; i++)
        {
            var talent = heroClass.startTalents[i];
            var talentCell = startTalents[i];
            talentCell.init(talent);
            talentCell.gameObject.SetActive(true);
        }

        for (int i = heroClass.startTalents.Count; i < startTalents.Count; i++)
        {
            var talentCell = startTalents[i];
            talentCell.gameObject.SetActive(false);
        }
        

        LoadStartingCardsInDeck();

        RenderDeck();

        //unlockClassButton.gameObject.SetActive(!unlockedClasses[classIndex]);
        //unlockClassButton.GetComponentInChildren<Text>().text = heroClass.unlockGold.ToString();

        RenderCreateButton();

        //editDeckButton.interactable = unlockedClasses[classIndex];

        RenderAscension(true);
    }

    void RenderCreateButton()
    {
        // if (MenuControl.Instance.demoMode)
        // {
        //     createHeroButton.interactable = heroClasses.IndexOf(heroClass) == 0 && heroPaths.IndexOf(heroPath) == 1;
        //     return;
        // }

        int classIndex = heroClasses.IndexOf(heroClass);
        createHeroButton.interactable =
            isHeroUnlocked(classIndex); //unlockedClasses[heroClasses.IndexOf(heroClass)];
    }

    void SelectPath(HeroPath heroPath)
    {
        if (this.heroPath != heroPath) startingDeckInt = 0;

        this.heroPath = heroPath;

        //pathImage.sprite = heroPath.GetSprite();
        //pathNameText.text = heroPath.GetName();
        //pathTitleText.text = heroPath.GetName();
        //pathDescriptionText.text = heroPath.GetDescription();
        // pathTooltipView.GetComponentInChildren<Text>().text =
        //     MenuControl.Instance.GetLocalizedString(heroPath.UniqueID + "Tooltip");

        LoadStartingCardsInDeck();
        RenderCreateButton();
        RenderDeck();
    }

    public void UnlockClassPressed()
    {
        //if (accumulatedGold >= heroClass.unlockGold)
        {
            //accumulatedGold -= heroClass.unlockGold;
            unlockedClasses[heroClasses.IndexOf(heroClass)] = true;
            //MenuControl.Instance.dataControl.SaveData();
           // accumulatedGoldText.text = accumulatedGold.ToString();

            SelectClass(heroClass);
        }
        // else
        // {
        //     MenuControl.Instance.ShowNotification(null, MenuControl.Instance.GetLocalizedString("NotEnoughGoldPrompt"),
        //         MenuControl.Instance.GetLocalizedString("NotEnoughGoldText"), true, true, true);
        // }
    }

    void LoadStartingCardsInDeck()
    {
        originalStartingCards.Clear();
        originalStartingCards.Add(hero);
        foreach (Card card in heroClass.startCards)
        {
            // if (card is Deprecated Weapon)
            // {
            //     hero.weapon = (Deprecated Weapon) card;
            // }
            // else
            {
                originalStartingCards.Add(card);
            }
        }
        
        //combine path menu
        MenuControl.Instance.pathMenu.Setup();
        // foreach (Card card in MenuControl.Instance.pathMenu. GetUnlockbleCardList().startingDecks[startingDeckInt].startingCards)
        // {
        //     originalStartingCards.Add(card);
        // }
    }

    public void InitalizeHero()
    {
        hero = Instantiate(heroClass.heroPrefab, transform);
    }

    public void CreateHero()
    {
        CreateHeroAndClose();
        //heroNamePanelView.Show();
        //heroNameInputField.text = heroClass.defaultNamesForPaths[heroPaths.IndexOf(heroPath)];
    }

    // public void CancelCreateHero()
    // {
    //     heroNamePanelView.Hide();
    // }
    public bool isCardAvailable(Card card)
    {
        if (card == null)
        {
            return false;
        }
        var csvLoader = MenuControl.Instance.csvLoader;
        //chineseName = MenuControl.Instance.csvLoader.downgradeChineseName(chineseName);
        // if (csvLoader.chineseNameToPlayerCardMap.ContainsKey(chineseName) ||
        //     csvLoader.chineseNameToTalentMap.ContainsKey(chineseName))
        // {
        //         return true;
        // }
        if (csvLoader.isValidInCurrentVersion(card.UniqueID))
        {
            return true;
        }

        return false;
    }

    public bool isCardUnlockedAndAvailable(Card card)
    {
        return isCardAvailable(card) && !isLocked(card);
    }

    void addCardResource(string pathName)
    {
        var gameObjects = Resources.LoadAll<GameObject>("Cards/"+pathName);
        //convert from array of gameobjects to array of card
        testAllCards.AddRange(gameObjects.Select(go => go.GetComponent<Card>()));
    }
    void addSkillResource(string pathName)
    {
        var gameObjects = Resources.LoadAll<GameObject>("Skills/"+pathName);
        //convert from array of gameobjects to array of card
        testAllCards.AddRange(gameObjects.Select(go => go.GetComponent<Card>()));
    }
    void addEffectResource(string pathName)
    {
        var gameObjects = Resources.LoadAll<GameObject>("Effects/"+pathName);
        //convert from array of gameobjects to array of card
        testAllEffects.AddRange(gameObjects.Select(go => go.GetComponent<Effect>()));
    }

    void addEffectsInEncounter()
    {
        
        var gameObjects = Resources.LoadAll<GameObject>("Adventure Items/Encounters");
        if (gameObjects.Length > 0)
        {
            foreach (var gameObject in gameObjects)
            {
                
                var eff = gameObject.GetComponent<Effect>();
                if (eff != null)
                {
                    testAllEffects.Add(eff);
                }
            }
        }
    }
    public void Awake()
    {
        var newAllCards = new List<Card>();


        allCards.Distinct();
        allCards.RemoveAll(card => card == null);
        allEffects.Distinct();
        allEffects.RemoveAll(card => card == null);
        if (useResource)
        {
            testAllCards = new List<Card>();
            addCardResource("Artifacts");
            addCardResource("Red - Helm");
            addCardResource("Black - Karim");
            addCardResource("Green - Tribes");
            addCardResource("Common");
            addCardResource("Loot");
            addCardResource("Rogue");
            addCardResource("Starting Unlocks");
            addCardResource("Treasure");
            addCardResource("Warrior");
            addCardResource("Mage");
            addCardResource("Mage/MagicNew");
            addCardResource("DeprecatedWeapon");
            addCardResource("Green - Tribes/Evolve Cards");
            addSkillResource("Mage");
            addSkillResource("Rogue");
            addSkillResource("Warrior");
            
            

            testAllEffects = new List<Effect>();
            addEffectResource("Black - Karim");
            addEffectResource("Common");
            addEffectResource("Green - Tribes");
            addEffectResource("Loot");
            addEffectResource("Red - Helm");
            addEffectResource("Mage");
            addEffectResource("MagicNew");
            addEffectResource("Rogue");
            addEffectResource("Trap");
            addEffectResource("Warrior");

            addEffectsInEncounter();

            testAllCards.Distinct();
            testAllCards.RemoveAll(card => card == null);
            testAllEffects.Distinct();
            testAllEffects.RemoveAll(card => card == null);
            allCards = testAllCards;
            allEffects = testAllEffects;
        }
//allcards remove duplicate elements





        
        foreach (var card in allCards)
        {
            if (card == null)
            {
                Debug.LogError("card is null");
            }
            
            //update card upgrade info
        }

        allCards.RemoveAll(card => card == null);
        foreach (var card in allCards)
        {
            if(isCardAvailable(card))
            {newAllCards.Add(card);}
        }
        
        originalAllCards = new List<Card>(allCards);
        allCards = newAllCards;

        foreach (var card in allCards)
        {
            cardDict[card.UniqueID] = card;
        }
        
        
        // separate them
        separatedCards["Artifact"] = new List<List<Card>>(){new List<Card>(), new List<Card>(),new List<Card>(), new List<Card>()};
        separatedCards["Treasure"] = new List<List<Card>>(){new List<Card>(), new List<Card>(),new List<Card>(), new List<Card>()};
        separatedCards["Class"] = new List<List<Card>>(){new List<Card>(), new List<Card>(),new List<Card>(), new List<Card>()};
        separatedCards["Path"] = new List<List<Card>>(){new List<Card>(), new List<Card>(),new List<Card>(), new List<Card>()};
        separatedCards["Loot"] = new List<List<Card>>(){new List<Card>(), new List<Card>(),new List<Card>(), new List<Card>()};
        separatedCards["All"] = new List<List<Card>>(){new List<Card>(), new List<Card>(),new List<Card>(), new List<Card>()};
        separatedCards["Potion"] = new List<List<Card>>(){new List<Card>(), new List<Card>(),new List<Card>(), new List<Card>()};
        
        separatedCards["Unlockable"] = new List<List<Card>>(){new List<Card>(), new List<Card>(),new List<Card>(), new List<Card>()};
        foreach (var card in allCards)
        {
            var info = MenuControl.Instance.csvLoader.uniqueIdToPlayerCardInfo[card.UniqueID];
            if (info.hero != -1)
            {
                bool isNormal = true;
                separatedCards["All"]       [info.hero].Add(card);
                if (MenuControl.Instance.csvLoader.isLocked(info.uid))
                {
                    separatedCards["Unlockable"]       [info.hero].Add(card);
                }
                if (card.cardTags.Contains(MenuControl.Instance.lootTag))
                {
                    isNormal = false;
                    separatedCards["Loot"][info.hero].Add(card);        
                }
                if (card.cardTags.Contains(MenuControl.Instance.potionTag))
                {
                    isNormal = false;
                    separatedCards["Potion"][info.hero].Add(card);        
                }
                if(card.cardTags.Contains(MenuControl.Instance.treasureTag))
                {
                    isNormal = false;
                    separatedCards["Treasure"][info.hero].Add(card);        
                }

                if (card is Artifact)
                {
                    isNormal = false;
                    separatedCards["Artifact"][info.hero].Add(card);
                }

                if (isNormal)
                {
                    separatedCards["Class"]       [info.hero].Add(card);
                }
                
            }
            else
            {
                if (info.path < 0 || info.path >= 4)
                {
                    Debug.LogError($"{info.uid} path{info.path} is not valid");
                    continue;
                }
                separatedCards["Path"][info.path].Add(card);
            }
        }
    }

    public List<Card> ClassCards()
    {
        var res = new List<Card>();
        res.AddRange((separatedCards["Class"][0]));
        res.AddRange((separatedCards["Class"][getCurrentClassIndexReal()]));
        return res;
    }
    
    public List<Card> PathCards()
    {
        var res = new List<Card>();
        res.AddRange((separatedCards["Path"][0]));
        res.AddRange((separatedCards["Path"][getCurrentClassIndexReal()]));
        return res;
    }
    
    public List<Card> CurrentHeroUnlockableCards()
    {
        var res = new List<Card>();
        res.AddRange((separatedCards["Unlockable"][0]));
        res.AddRange((separatedCards["Unlockable"][getCurrentClassIndexReal()]));
        return res;
    } 
    
    public List<Card> CurrentHeroAllCards()
    {
        var res = new List<Card>();
        res.AddRange((separatedCards["All"][0]));
        res.AddRange((separatedCards["All"][getCurrentClassIndexReal()]));
        return res;
    }

    public List<Card> CurrentHeroAndPathAllCards()
    {
        var res = new List<Card>();
        
        res.AddRange((separatedCards["All"][0]));
        res.AddRange((separatedCards["All"][getCurrentClassIndexReal()]));
        
        res.AddRange(separatedCards["Path"][getCurrentClassIndexReal()]);
        return res;
    }
    
    public List<Card> CurrentHeroPotionCards()
    {
        return CurrentHeroSpecialCards("Potion");
    }  
    
    public List<Card> CurrentHeroTreasureCards()
    {
        return CurrentHeroSpecialCards("Treasure");
    }  
    public List<Card> CurrentHeroArtifactCards()
    {
        return CurrentHeroSpecialCards("Artifact");
    }  
    
    
    public List<Card> CurrentHeroSpecialCards(string n)
    {
        var res = new List<Card>();
        res.AddRange((separatedCards[n][0]));
        res.AddRange((separatedCards[n][getCurrentClassIndexReal()]));
        return res;
    }  
    
    public List<Card> CurrentHeroLootCards()
    {
        var res = new List<Card>();
        res.AddRange((separatedCards["Loot"][0]));
        res.AddRange((separatedCards["Loot"][getCurrentClassIndexReal()]));
        return res;
    }  

    public void CreateHeroAndClose()
    {
        hasInitHero = false;
        isAlive = true;
        
        foreach (var startTalent in MenuControl.Instance.heroMenu.heroClass.startTalents)
        {
            //Card card = MenuControl.Instance.heroMenu.GetCardByID(startTalent);
            if (startTalent is Skill skill)
            {
                MenuControl.Instance.levelUpMenu.variableTalentsAcquired.Add(skill);

                if (skill.abilityToPerform != null)
                {
                    // start ability is added
                    skill.abilityToPerform.PerformAbility(null, null, 0);
                }
            }
            else
            {
                Debug.LogError(startTalent.GetName()+"不是英雄天赋");
            }
        }

        foreach (Card card in cardsOwned)
        {
            if (!(card is Hero))
            {
                Destroy(card.gameObject);
            }
        }


        cardsOwned.Clear();
        foreach (Card card in originalStartingCards)
        {
            if (!MenuControl.Instance.progressMenu.cardsDiscovered.Contains(card.UniqueID) && !(card is Hero))
            {
                MenuControl.Instance.progressMenu.cardsDiscovered.Add(card.UniqueID);
                cardsDiscoveredThisRun += 1;
            }

            if (card is Hero)
            {
                cardsOwned.Add(card);
            }
            else
            {
                AddCardToDeck(card);
               // CreateCardToOwn(card);
            }
        }

        // weaponsOwned.Clear();
        // if (hero.weapon != null)
        // {
        //     weaponsOwned.Add(hero.weapon);
        // }

        startDate = DateTime.Now;

        

        // Card ankhCard = GetCardByID("ArtifactEquip14");
        // if (ankhCard != null && ascensionMode == 0)
        // {
        //     MenuControl.Instance.heroMenu.AddCardToDeck(ankhCard);
        // }
        // if (ascensionMode >= 6)
        // {
        //     flareStones = 0;
        // }

        // if (ascensionMode >= 13)
        // {
        //     hero.currentHP -= 3;
        //     hero.initialHP -= 3;
        // }
        //
        // if (ascensionMode >= 15)
        // {
        //     hero.startingEffects.Add(wakingNightmareEffectTemplate);
        //     hero.startingEffectCharges.Add(0);
        // }
        //
        // if (easyMode)
        // {
        //     hero.startingEffects.Add(easyModeEffectTemplate);
        //     hero.startingEffectCharges.Add(0);
        // }
        //
        // if (seasonsMode)
        // {
        //     hero.startingEffects.Add(seasonsModeEffectTemplate);
        //     hero.startingEffectCharges.Add(0);
        // }

        //MenuControl.Instance.dataControl.SaveData();

        CloseMenu();

        //combine path menu
        
        MenuControl.Instance.pathMenu.selectedPath = true;
        MenuControl.Instance.dataControl.SaveData();
        
        MenuControl.Instance.adventureMenu.ContinueAdventure();
        
        //MenuControl.Instance.pathMenu.ShowMenu();
        //MenuControl.Instance.adventureMenu.ContinueAdventure();

        MenuControl.Instance.LogEvent("NewHero_" + GetLevelClassPathString());
        MenuControl.Instance.LogEvent("StartNewGame");
        
        //PathMenu.Show();
    }

    public string GetLevelClassPathString()
    {
        return currentLevel.ToString() + "_" + heroClass.UniqueID;
    }

    public void LevelUp()
    {
        currentLevel += 1;
        if (currentLevel >= 2)
        {
            MenuControl.Instance.heroMenu.hero.initialHP += heroClass.GetHPGainPerLevel();
            MenuControl.Instance.heroMenu.hero.currentHP += heroClass.GetHPGainPerLevel();
        }

        MenuControl.Instance.dataControl.SaveData();
    }


    public Card GetCardByID(string uniqueID)
    {
        foreach (Card card in originalAllCards)
        {
            if (card.UniqueID == uniqueID)
                return card;
        }

        foreach (Card card  in enemyCards)
        {
            if (card.UniqueID == uniqueID)
                return card;
        }

        return null;
    }
    
    
    public Card GetCardByCardUniqueId(int cardUniqueId)
    {
        foreach (var card in cardsOwned)
        {
            if (card.CardUniqueId == cardUniqueId)
            {
                return card;
            }
        }
        Debug.LogError($"no card with card unique Id {cardUniqueId}");
        return null;
    }

    public Effect GetEffectByID(string uniqueID)
    {
        foreach (Effect effect in allEffects)
        {
            if (effect.UniqueID == uniqueID)
                return effect;
        }

        return null;
    }
    
    

    public int currentXPForLevel()
    {
        if (currentLevel <= 1)
        {
            return currentXP;
        }

        return currentXP - levelsXP[currentLevel - 2];
    }
    
    public int currentXPForLevel(int offset= 0)
    {
        if (currentLevel+offset <= 1)
        {
            return currentXP;
        }

       
        return currentXP - levelsXP[currentLevel +offset - 2];

    }

    public int xPForNextLevel()
    {
        if (currentLevel <= 1)
        {
            return levelsXP[0];
        }

        return levelsXP[currentLevel - 1] - levelsXP[currentLevel - 2];
    }
    
    public int xPForNextLevel(int offset=0)
    {
        if (currentLevel+offset <= 1)
        {
            return levelsXP[0];
        }

        return levelsXP[currentLevel +offset - 1] - levelsXP[currentLevel +offset- 2];
    }

    public bool CanLevelUp()
    {
        //return true;
        return (currentXP >= levelsXP[currentLevel - 1]);
    }

    public void AddXPToUpgrade()
    {
        currentXP = levelsXP[currentLevel - 1];
    }
    
    public void AddXPToUpgrade3()
    {
        currentXP = levelsXP[currentLevel +1] ;
    }

    public List<Card> FilterCardsOfLevel(List<Card> cardsToFilter, int level)
    {
        int cardLevel = Mathf.Clamp(level, 1, 3);

        List<Card> cardsToReturn = new List<Card>();
        foreach (Card card in cardsToFilter)
        {
            if (card.level == cardLevel && isCardUnlockedAndAvailable(card))
            {
                cardsToReturn.Add(card);
            }
        }

        return cardsToReturn;
    }

    public List<Card> FilterCardsWithTag(List<Card> cardsToFilter, CardTag tag)
    {
        List<Card> cardsToReturn = new List<Card>();
        foreach (Card card in cardsToFilter)
        {
            if (card.cardTags.Contains(tag) && isCardUnlockedAndAvailable(card))
            {
                cardsToReturn.Add(card);
            }
        }

        return cardsToReturn;
    }


    public void AddXP(int xpToAdd)
    {
        float multiplier = 1f;

        //foreach (EffectXPModifier mod in MenuControl.Instance.battleMenu.player.GetEffectsOfType<EffectXPModifier>())
        //{
        //    multiplier *= mod.fractionMultiplier;
        //}
        int amount = Mathf.FloorToInt(xpToAdd * multiplier);
        currentXP += amount;
        MenuControl.Instance.progressMenu.xPEarned += amount;
    }

    public Card CreateCardToOwn(Card cardTemplate)
    {
        Card card = Instantiate(cardTemplate, transform);
        card.player = MenuControl.Instance.battleMenu.player1;
        cardsOwned.Add(card);
        card.CardUniqueId = card.GetInstanceID();
        return card;
    }


    public void RemoveCardToOwn(Card card)
    {
        cardsOwned.Remove(card);
        Destroy(card.gameObject);
    }

    public void RemoveCardWithSameTemplate(Card removeCard)
    {
        foreach (var card in cardsOwned)
        {
            //var template = card.cardTemplate;
            if (card == removeCard.cardTemplate)
            {
                RemoveCardFromDeck(card);
                return;
            }
        }
        Debug.LogError(("card not found"));
    }

    public Card InsertCardToOwn(Card cardTemplate, int index)
    {
        Card card = Instantiate(cardTemplate, transform);
        cardsOwned.Insert(index, card);
        return card;
    }

    public Card AddCardToDeck(Card originalCardTemplate)
    {
        Card cardTemplate = originalCardTemplate;
        if (!(cardTemplate is Hero))
            cardTemplate = MenuControl.Instance.heroMenu.GetCardByID(cardTemplate.UniqueID);

        Card cardToReturn = cardTemplate;
        if (cardTemplate is Artifact)
        {
            MenuControl.Instance.heroMenu.artifactsOwned.Add(cardTemplate);
            if (MenuControl.Instance.heroMenu.artifactSlots > MenuControl.Instance.heroMenu.artifactsEquipped.Count)
            {
                MenuControl.Instance.heroMenu.artifactsEquipped.Add(cardTemplate);
            }
        }
        else
        {
            cardToReturn = CreateCardToOwn(cardTemplate);
            cardToReturn.PutIntoZone(MenuControl.Instance.battleMenu.deck);

            if (cardTemplate.isPotion && MenuControl.Instance.heroMenu.artifactSlots > MenuControl.Instance.heroMenu.artifactsEquipped.Count)
            {
                MenuControl.Instance.heroMenu.artifactsEquipped.Add(cardToReturn);
            }
        }

        if (!MenuControl.Instance.progressMenu.cardsDiscovered.Contains(cardTemplate.UniqueID) && !(cardTemplate is Hero))
        {
            MenuControl.Instance.progressMenu.cardsDiscovered.Add(cardTemplate.UniqueID);
            cardsDiscoveredThisRun += 1;
        }

        if (originalCardTemplate.cardTags.Contains(MenuControl.Instance.naughtyTag))
        {
            cardToReturn.cardTags.Add(MenuControl.Instance.naughtyTag);
        }
        else if (originalCardTemplate.cardTags.Contains(MenuControl.Instance.niceTag))
        {
            cardToReturn.cardTags.Add(MenuControl.Instance.niceTag);
        }

        MenuControl.Instance.LogEvent("AddCardToDeck_" +
                                      cardTemplate
                                          .UniqueID); // + MenuControl.Instance.heroMenu.GetLevelClassPathString());

        
        if (!MenuControl.Instance.battleMenu.inBattle)
        {
            MenuControl.Instance.dataControl.SaveData();
        }

        cardToReturn.cardTemplate = cardTemplate;
        return cardToReturn;
    }

    public void RemoveCardFromDeck(Card card)
    {
        if (cardsOwned.Contains(card))
        {
            RemoveCardToOwn(card);
        }

        // if (weaponsOwned.Contains(card))
        // {
        //     weaponsOwned.Remove(card);
        //     if (hero.weapon == card)
        //     {
        //         if (weaponsOwned.Count > 0)
        //         {
        //             hero.weapon = (Deprecated Weapon) weaponsOwned[0];
        //         }
        //         else
        //         {
        //             hero.weapon = null;
        //         }
        //     }
        // }

        if (artifactsOwned.Contains(card))
        {
            int numberOfCopiesOwned = MenuControl.Instance.CountOfCardsInList(card, artifactsOwned);
            int numberOfCopiesEquipped = MenuControl.Instance.CountOfCardsInList(card, artifactsEquipped);
            artifactsOwned.Remove(card);
            if (numberOfCopiesEquipped == numberOfCopiesOwned)
            {
                artifactsEquipped.Remove(card);
            }
        }

        if (startOfBattleHandCardIDs.Contains(card.UniqueID))
        {
            startOfBattleHandCardIDs.Remove(card.UniqueID);
        }

        if (artifactsEquipped.Contains(card))
        {
            
            if (card.isPotion)
            {
                MenuControl.Instance.heroMenu.artifactsEquipped.Remove(card);
                
            }
        }

        MenuControl.Instance.LogEvent("AddCardToDeck_" +
                                      card.UniqueID); // + MenuControl.Instance.heroMenu.GetLevelClassPathString());
    }

    public void ClassLeftPressed()
    {
        //MenuControl.Instance.indicatorMenu.ShowIndicator(MenuControl.Instance.GetLocalizedString("DemoSelectLockedHero"));
        int nextIndex = heroClasses.IndexOf(heroClass) - 1;
        if (nextIndex <= -1) nextIndex = heroClasses.Count - 1;
        SelectClass(heroClasses[nextIndex]);
    }

    public void ClassRightPressed()
    {
        //MenuControl.Instance.indicatorMenu.ShowIndicator(MenuControl.Instance.GetLocalizedString("DemoSelectLockedHero"));
        int nextIndex = heroClasses.IndexOf(heroClass) + 1;
        if (nextIndex >= heroClasses.Count) nextIndex = 0;
        SelectClass(heroClasses[nextIndex]);
    }

    public void PathLeftPressed()
    {
        int nextIndex = heroPaths.IndexOf(heroPath) - 1;
        if (nextIndex <= -1) nextIndex = heroPaths.Count - 1;

        if (reaperMode)
        {
            List<int> heroPathInts = new List<int>();
            heroPathInts.Add(0);
            heroPathInts.Add(2);
            heroPathInts.Add(1);

            while (heroPathInts.IndexOf(nextIndex) > reaperProgress)
            {
                nextIndex -= 1;
                if (nextIndex <= -1) nextIndex = heroPaths.Count - 1;
            }
        }

        SelectPath(heroPaths[nextIndex]);
    }

    public void PathRightPressed()
    {
        int nextIndex = heroPaths.IndexOf(heroPath) + 1;
        if (nextIndex >= heroPaths.Count) nextIndex = 0;

        if (reaperMode)
        {
            while (reaperProgressHeroPathInts.IndexOf(nextIndex) > reaperProgress)
            {
                nextIndex += 1;
                if (nextIndex >= heroPaths.Count) nextIndex = 0;
            }
        }

        SelectPath(heroPaths[nextIndex]);
    }

    // public void DeckLeftPressed()
    // {
    //     startingDeckInt -= 1;
    //     if (startingDeckInt < 0) startingDeckInt = GetUnlockbleCardList().startingDecks.Count - 1;
    //     SelectPath(heroPath);
    // }
    //
    // public void DeckRightPressed()
    // {
    //     startingDeckInt += 1;
    //     if (startingDeckInt == GetUnlockbleCardList().startingDecks.Count) startingDeckInt = 0;
    //     SelectPath(heroPath);
    // }

    public int StartingCardCost(Card card)
    {
        if (card is Artifact)
        {
            return ((Artifact) card).unlockCost;
        }

        int index = unlockableCards.IndexOf(card);
        if (index >= 0)
            return unlockableCardCosts[index];

        return 0;
    }

    public Achievement StartingCardAchievement(Card card)
    {
        int index = unlockableCards.IndexOf(card);
        if (index >= 0)
            return unlockableCardAchievements[index];

        return null;
    }

    public Card UpgradeToCardInDeck(Card selectedCard, Card upgradeCard)
    {
        if (cardsOwned.Contains(selectedCard))
        {
            int index = cardsOwned.IndexOf(selectedCard);

            bool hasNaughty = false;
            bool hasNice = false;
            if (selectedCard.cardTags.Contains(MenuControl.Instance.naughtyTag)) hasNaughty = true;
            if (selectedCard.cardTags.Contains(MenuControl.Instance.niceTag)) hasNice = true;

            RemoveCardToOwn(selectedCard);
            Card newCard = InsertCardToOwn(upgradeCard, index);

            if (hasNaughty)
            {
                newCard.cardTags.Add(MenuControl.Instance.naughtyTag);
            }

            if (hasNice)
            {
                newCard.cardTags.Add(MenuControl.Instance.niceTag);
            }

            if (!MenuControl.Instance.progressMenu.cardsDiscovered.Contains(upgradeCard.UniqueID) &&
                !(upgradeCard is Hero))
            {
                MenuControl.Instance.progressMenu.cardsDiscovered.Add(upgradeCard.UniqueID);
                cardsDiscoveredThisRun += 1;
            }

            if (startOfBattleHandCardIDs.Contains(selectedCard.UniqueID))
            {
                int index1 = startOfBattleHandCardIDs.IndexOf(selectedCard.UniqueID);
                startOfBattleHandCardIDs.RemoveAt(index1);
                startOfBattleHandCardIDs.Insert(index1, upgradeCard.UniqueID);
            }

            MenuControl.Instance.LogEvent("UpgradedCard" +
                                          upgradeCard
                                              .UniqueID); // + MenuControl.Instance.heroMenu.GetLevelClassPathString());

            return newCard;
        }

        return null;
    }
    
    public Card UpgradeToRandomCardInDeck(Card selectedCard)
    {
        return UpgradeToCardInDeck(selectedCard, selectedCard.RandomUpgradeCard);
    }

    public void PurchaseStartingCard(Card card)
    {
        // if (MenuControl.Instance.heroMenu.accumulatedGold >= MenuControl.Instance.heroMenu.StartingCardCost(card))
        // {
        //     MenuControl.Instance.heroMenu.accumulatedGold -= MenuControl.Instance.heroMenu.StartingCardCost(card);
        //     if (card is Artifact)
        //     {
        //         MenuControl.Instance.heroMenu.artifactsUnlocked.Add(card);
        //     }
        //     else
        //     {
        //         MenuControl.Instance.heroMenu.startingCardsUnlocked.Add(card);
        //     }
        //
        //     MenuControl.Instance.dataControl.SaveData();
        //
        //     SelectClass(heroClass);
        //     SelectPath(heroPath);
        //
        //     accumulatedGoldText.text = accumulatedGold.ToString();
        // }
    }

    public override void SelectVisibleCard(VisibleCard vc, bool withClick = true)
    {
        base.SelectVisibleCard(vc, withClick);
        selectedVisibleCard = vc;
        if (!withClick)
        {
            MenuControl.Instance.infoMenu.ShowInfo(vc);
        }
    }

    public override void DeSelectVisibleCard(VisibleCard vc, bool withClick = true)
    {
        base.DeSelectVisibleCard(vc, withClick);
        MenuControl.Instance.infoMenu.HideMenu();
    }


    // public void SetGameMode(int gameMode)
    // {
    //     //this.gameMode = (GameMode)gameMode;
    //     //gameModeText.text = MenuControl.Instance.GetLocalizedString(((GameMode)gameMode).ToString() + "Name", ((GameMode)gameMode).ToString());
    //     gameModeLabelImage.sprite = gameModeButtonLabels[(int) gameMode];
    //     gameModePanelView.Hide();
    // }

    // public int GetCurrentGold()
    // {
    //     int goldAmount = flareStones * 5;
    //     foreach (Card card in cardsOwned)
    //     {
    //         if (card.cardTags.Contains(MenuControl.Instance.treasureTag))
    //         {
    //             goldAmount += card.goldWorth;
    //         }
    //     }
    //
    //     foreach (Card card in artifactsOwned)
    //     {
    //         goldAmount += card.goldWorth;
    //     }
    //
    //     return goldAmount;
    // }

    // public Card GetLowerLevelCard(Card card)
    // {
    //     foreach (Card previousCard in allCards)
    //     {
    //         if (previousCard.upgradeCard&& previousCard.upgradeCard.UniqueID == card.UniqueID)
    //         {
    //             return previousCard;
    //         }
    //     }
    //
    //     return null;
    // }

    // public Card GetLowestLevelCard(Card card)
    // {
    //     while (GetLowerLevelCard(card)  is Card c)
    //     {
    //         card = c;
    //     }
    //
    //     return card;
    // }

    public List<Card> GetTreasureCardsOwned(bool ignoreArtifacts = false)
    {
        List<Card> cards = new List<Card>();
        foreach (Card card in MenuControl.Instance.heroMenu.cardsOwned)
        {
            if (card.cardTags.Contains(MenuControl.Instance.treasureTag))
            {
                cards.Add(card);
            }
        }

        foreach (Card card in MenuControl.Instance.heroMenu.weaponsOwned)
        {
            if (card.cardTags.Contains(MenuControl.Instance.treasureTag))
            {
                cards.Add(card);
            }
        }

        if (!ignoreArtifacts)
        {
            foreach (Card card in MenuControl.Instance.heroMenu.artifactsOwned)
            {
                cards.Add(card);
            }
        }

        return cards;
    }

    public void LoadDropRates()
    {
        TextAsset file = Resources.Load<TextAsset>("Spellsword_ DungeonTop - Items - Drop Rates");
        string fileData = file.text;
        dropRateLines.Clear();
        foreach (string lineItem in fileData.Split("\n"[0]))
        {
            string[] dropRateLineData = (lineItem.Trim()).Split("\t"[0]);
            dropRateLines.Add(dropRateLineData);
        }
    }

    public void ToggleEditMode()
    {
        if (editPanel.gameObject.activeInHierarchy)
        {
            editPanel.Hide(false);
        }
        else
        {
            editPanel.Show();
        }
    }

    public void RenderDeck()
    {
        foreach (Transform child in classCardsPanel)
        {
            Destroy(child.gameObject);
        }

        // foreach (Card card in unlockableCards)
        // {
        //     StartingCardContainer scc = Instantiate(startingCardContainerPrefab, classCardsPanel);
        //     scc.RenderCard(card);
        // }
        //
        // foreach (Card card in allCards)
        // {
        //     if (card is Artifact && ((Artifact) card).unlockCost > 0)
        //     {
        //         StartingCardContainer scc = Instantiate(startingCardContainerPrefab, classCardsPanel);
        //         scc.RenderCard(card);
        //     }
        // }
        
        
        List<Card> uniqueCards = new List<Card>();
        foreach (Card card in originalStartingCards)
        {
            bool unique = true;
            foreach (Card cc in uniqueCards)
            {
                if (card.UniqueID == cc.UniqueID)
                {
                    unique = false;
                }
            }
            if (unique)
            {
                uniqueCards.Add(card);
            }
        }

        int ii = 0;
        for (; ii < uniqueCards.Count; ii += 1)
        {
            // skip rendering the hero card
            if (ii > 0)
            {
                var card = uniqueCards[ii];
                if (ii - 1 >= decklistVCs.GetComponentsInChildren<VisibleCard>().Length)
                {
                    break;
                }
                VisibleCard vc = decklistVCs.GetComponentsInChildren<VisibleCard>()[ii - 1];
                
                
                int count = MenuControl.Instance.CountOfCardsInList(card, originalStartingCards);
                
                vc.SetHandCardCount(count);
                vc.RenderCardForMenu(card);
                vc.Show();
            }
        }

        for (; ii <= decklistVCs.GetComponentsInChildren<VisibleCard>().Length; ii++)
        {
            VisibleCard vc = decklistVCs.GetComponentsInChildren<VisibleCard>()[ii - 1];
            vc.Hide();
        }
        //deckNameText.text = GetUnlockbleCardList().startingDecks[startingDeckInt].GetName();
    }

    public List<Card> GetAllTreasures() //Not artifacts
    {
        List<Card> treasures = new List<Card>();
        foreach (Card card in allCards)
        {
            if (card.cardTags.Contains(MenuControl.Instance.treasureTag))
            {
                if (isCardAvailable(card))
                {

                    treasures.Add(card);
                }
            }
        }

        return treasures;
    }
    public List<Card> GetAllUnlockedTreasures() //Not artifacts
    {
        List<Card> treasures = new List<Card>();
        foreach (Card card in CurrentHeroTreasureCards())
        {
            if (card.cardTags.Contains(MenuControl.Instance.treasureTag) && isCardUnlockedAndAvailable(card))
            {/*MenuControl.Instance.heroMenu.GetUnlockedCards().Contains(card)*/
                treasures.Add(card);
            }
        }

        return treasures;
    }

    public List<Card> GetAllLoot()
    {
        List<Card> drops = new List<Card>();
        drops.AddRange(separatedCards["Loot"][0]);
        drops.AddRange(separatedCards["Loot"][getCurrentClassIndexReal()]);
        // foreach (Card card in allCards)
        // {
        //     if (card.cardTags.Contains(MenuControl.Instance.lootTag) && isCardUnlockedAndAvailable(card))
        //     {
        //         drops.Add(card);
        //     }
        // }

        //Include class weapons
        foreach (Card card in MenuControl.Instance.heroMenu.ClassCards())
        {
            if (card is NewWeapon && isCardUnlockedAndAvailable(card) && !card .IsTreasure())
            {
                drops.Add(card);
            }
        }

        foreach (Card card in MenuControl.Instance.heroMenu.GetUnlockedCards())
        {
            if (card is NewWeapon && isCardUnlockedAndAvailable(card) && !card .IsTreasure())
            {
                drops.Add(card);
            }
        }

        return drops;
    }


    public List<Card> GetArtifacts(bool noDuplicates = false)
    {
        List<Card> cards = new List<Card>();
        foreach (Card card in CurrentHeroArtifactCards())
        {
            if (card is Artifact) //.cardTags.Contains(MenuControl.Instance.artifactTag))
            {
                if(isCardUnlockedAndAvailable(card))
                if (!card.achievementRestricted || GetUnlockedAchievementCards().Contains(card))
                {
                    if (!noDuplicates || !MenuControl.Instance.heroMenu.artifactsOwned.Contains(card))
                    {
                        {
                            if (!cards.Contains(card) &&  isCardUnlockedAndAvailable(card))
                            {
                                cards.Add(card);
                            }
                        }
                    }
                }
            }
        }

        return cards;
    }

    public List<Card> GetLockedLevel1Cards()
    {
        var res = GetLockedCards();
        List<Card> newRes = new List<Card>();
        foreach (var card in res)
        {
            if (card.level <= 1)
            {
                newRes.Add(card);
            }
        }

        return newRes;
    }
    public List<Card> GetLockedCards()
    {
        List<Card> cards = new List<Card>();
        foreach (Card card in CurrentHeroAndPathAllCards())
        {
            // if (card is Artifact) //.cardTags.Contains(MenuControl.Instance.artifactTag))
            // {
            //     // if (!card.achievementRestricted || GetUnlockedAchievementCards().Contains(card))
            //     // {
            //     //     if (!noDuplicates || !MenuControl.Instance.heroMenu.artifactsOwned.Contains(card))
            //     //     {
            //     if (!artifactsUnlocked.Contains(card) && ((Artifact) card).unlockCost != 0 &&  isCardAvailable(card))
            //     {
            //         //if (!cards.Contains(card))
            //         {
            //             cards.Add(card);
            //         }
            //     }
            //     if(MenuControl.Instance.heroMenu.unlockableArtifacts.con)
            //     //     }
            //     // }
            // }
            // else
            {
                if(MenuControl.Instance.heroMenu.unlockableCards.Contains(card) && !MenuControl.Instance.heroMenu.startingCardsUnlockedNames.Contains( card.UniqueID))
                {
                    cards.Add(card);
                }
            }
        }
        
        

        return cards;
    }

    public List<Card> GetHealingPotionsInDeck()
    {
        List<Card> cards = new List<Card>();
        foreach (Card card in cardsOwned)
        {
            if (card.isPotion)
            {
                cards.Add(card);
            }
        }

        return cards;
    }

    public List<Card> GetItemsInDeck()
    {
        List<Card> cards = new List<Card>();
        foreach (Card card in cardsOwned)
        {
            if (card.isPotion)
            {
                cards.Add(card);
            }
        }
        cards.AddRange(MenuControl.Instance.heroMenu.artifactsOwned);

        return cards;
    }

    public void StartDraft()
    {
        // pickingSpells = false;
        // MenuControl.Instance.ApplySeed();
        // originalStartingCards.RemoveRange(1, originalStartingCards.Count - 1);
        //
        // draftCards.Clear();
        // draftCards.AddRange(FilterCardsOfLevel(heroPath.pathCards, 1));
        // draftCards.AddRange(FilterCardsOfLevel(GetUnlockedMinionCards(), 1));
        // draftCards.Shuffle();
        // draftCards.RemoveRange(15, draftCards.Count - 15);
        //
        // PickCards();
    }

    void PickCards()
    {
        if (originalStartingCards.Count == 6 && !pickingSpells)
        {
            pickingSpells = true;
            // draftCards.Clear();
            // draftCards.AddRange(FilterCardsOfLevel(heroClass.classCards, 1));
            // draftCards.AddRange(FilterCardsOfLevel(GetUnlockedSpellCards(), 1));
            // // foreach (Card card in draftCards.ToArray())
            // // {
            // //     if (card is Deprecated Weapon)
            // //     {
            // //         draftCards.Remove(card);
            // //     }
            // // }
            //
            // draftCards.Shuffle();
            // draftCards.RemoveRange(15, draftCards.Count - 15);
        }

        List<Card> cardsToShow = new List<Card>();
        cardsToShow.AddRange(draftCards.GetRange(0, 5));
        draftCards.RemoveRange(0, 5);

        //Have player pick 5
        List<Action> actions = new List<Action>();

        List<string> buttonLabels = new List<string>();

        string textToShow = "";
        int mandatoryCardsToPick = 0;
        int maxPickableCards = 5;

        if (originalStartingCards.Count == 1 && draftCards.Count == 10)
        {
            textToShow = MenuControl.Instance.GetLocalizedString("DraftModePrompt");
            textToShow += "\n\n" + MenuControl.Instance.GetLocalizedString("DraftModePrompt2");
        }
        else
        {
            if (draftCards.Count == 0)
            {
                if (originalStartingCards.Count < 6)
                {
                    mandatoryCardsToPick = (6 - originalStartingCards.Count);
                }
                else if (originalStartingCards.Count < 11)
                {
                    mandatoryCardsToPick = (11 - originalStartingCards.Count);
                }

                textToShow = MenuControl.Instance.GetLocalizedString("DraftModePrompt3")
                    .Replace("XX", mandatoryCardsToPick.ToString());
            }
            else if (draftCards.Count == 5)
            {
                if (originalStartingCards.Count > 1 && originalStartingCards.Count < 6)
                {
                    maxPickableCards = (6 - originalStartingCards.Count);
                }
                else if (originalStartingCards.Count > 6 && originalStartingCards.Count < 11)
                {
                    maxPickableCards = (11 - originalStartingCards.Count);
                }

                if (maxPickableCards == 5)
                {
                    textToShow = MenuControl.Instance.GetLocalizedString("DraftModePrompt2");
                }
                else
                {
                    textToShow = MenuControl.Instance.GetLocalizedString("DraftModePrompt4")
                        .Replace("XX", maxPickableCards.ToString());
                }
            }
            else if (draftCards.Count == 10)
            {
                textToShow = MenuControl.Instance.GetLocalizedString("DraftModePrompt2");
            }
        }

        if (mandatoryCardsToPick > 0)
        {
            actions.Add(() =>
            {
                foreach (int seletedInt in MenuControl.Instance.cardChoiceMenu.selectedVisibleCardInts)
                {
                    originalStartingCards.Add(cardsToShow[seletedInt]);
                }

                if (originalStartingCards.Count == 11)
                {
                    CreateHero();
                }
                else
                {
                    PickCards();
                }
            });
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Confirm"));

            MenuControl.Instance.cardChoiceMenu.ShowChoice(cardsToShow, buttonLabels, actions, textToShow,
                mandatoryCardsToPick, mandatoryCardsToPick, false, -1, false);
        }
        else
        {
            actions.Add(() => { PickCards(); });
            actions.Add(() =>
            {
                foreach (int seletedInt in MenuControl.Instance.cardChoiceMenu.selectedVisibleCardInts)
                {
                    originalStartingCards.Add(cardsToShow[seletedInt]);
                }

                if (originalStartingCards.Count == 11)
                {
                    CreateHero();
                }
                else
                {
                    PickCards();
                }
            });
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Skip"));
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Confirm"));
            MenuControl.Instance.cardChoiceMenu.ShowChoice(cardsToShow, buttonLabels, actions, textToShow, 0,
                maxPickableCards, true, -1, false);
        }
    }

    int HighestAscensionAvailable()
    {
        // if (MenuControl.Instance.testMode)
        // {
        //     return maxAscensionLevels;
        // }

        if (ascensionUnlocks.Count == 0)
        {
            Debug.LogError("ascensionUnlocks.Count is 0");
        }
        int returnValue = Mathf.Min(ascensionUnlocks[heroClasses.IndexOf(heroClass)], maxAscensionLevels);

        return returnValue;
    }

    public void NextAscension()
    {
        ascensionMode = Mathf.Min(HighestAscensionAvailable(), ascensionMode + 1);
        RenderAscension(false);
        PressAscensionInfo();
    }

    public void PrevAscension()
    {
        ascensionMode = Mathf.Max(0, ascensionMode - 1);
        RenderAscension(false);
        PressAscensionInfo();
    }

    void RenderAscension(bool setAscensionToClass)
    {
        // if (setAscensionToClass)
        // {
        //     ascensionMode = HighestAscensionAvailable();
        // }
        //
        // ascensionText.text = ascensionMode.ToString();
        // nextAscensionButton.interactable = ascensionMode < HighestAscensionAvailable();
        // prevAscensionButton.interactable = ascensionMode > 0;
    }

    public List<Card> GetUnlockedAchievementCards()
    {
        List<Card> cards = new List<Card>();

        foreach (Achievement achievement in MenuControl.Instance.achievementsMenu.achievements)
        {
            if (achievement.IsCompleted() && achievement.cardReward != null)
            {
                cards.Add(achievement.cardReward);
            }
        }

        return cards;
    }

    public bool DeckContainsCardTemplate(Card card)
    {
        foreach (Card card1 in cardsOwned)
        {
            if (card1.UniqueID == card.UniqueID)
            {
                return true;
            }
        }

        return false;
    }

    public Card GetDeckCardByID(string uniqueID)
    {
        foreach (Card card1 in cardsOwned)
        {
            if (card1.UniqueID == uniqueID)
            {
                return card1;
            }
        }

        return null;
    }

    public void finishClass()
    {
        finishClass(0);
        finishClass(1);
    }
    public void finishClass(int i)
    {
        if (MenuControl.Instance.publishVersionFeatureOn)
        {
            finishedClasses[i] = true;
        }
        MenuControl.Instance.libraryMenu.discoverAllUnlockedHeros();
    }

    public void finishShowUnlockClassVisual(int i)
    {
        finishedUnlockVisualizationClasses[i] = true;
    }

    public bool isHeroFinished(int index)
    {
        return finishedClasses[index];
    }
    public bool isHeroUnlockedVisual(int i)
    {
        return finishedUnlockVisualizationClasses[i];
    }
    public bool isHeroUnlocked(int i)
    {
        if (i == 0)
        {
            return true;
        }
        if (i == 1)
        {
            return finishedClasses[0];
        }
        if (i == 2)
        {
            return finishedClasses[1] && false;
        }
        else
        {
            Debug.LogError( "hero not exist "+i);
            return false;
        }
    }

    public int hasNewUnlockHeroVisualization()
    {
        if (isHeroUnlocked(1) && !finishedUnlockVisualizationClasses[1])
        {
            return 1;
        }
        if (isHeroUnlocked(2) && !finishedUnlockVisualizationClasses[2])
        {
            return 2;
        }

        return -1;
    }

    public string heroLockString(int i)
    {
        if (i == 1)
        {
            return "Hero1_Unlock";
        }
        if (i == 2)
        {
            if (!finishedClasses[1])
            {
                return "Hero2_Unlock";
            }
            else
            {
                return "Hero2_UnlockPurchase";
            }
        }

            Debug.LogError( "hero lock string not exist "+i);
        return "";
    }
}