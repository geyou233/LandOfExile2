using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EventMenu : BasicMenu
{
    public Button gmRevealAreaBtn;
    public Button gmWinBattleBtn;
    public Image normalEventBK;
    public Image battleEventBK;
    public Transform choicesPanel;
    public EventDefinition encounterEventDefinition;
    public EventDefinition unskippableEncounterEventDefinition;
    public EventDefinition bossEncounterEventDefinition;
    public Text eventTitleText;
    public Text eventDescriptionText;
    //public GameObject squareBorderImage;
    public Image eventImage;
    private float originalChoicesPanelSpacing= float.NegativeInfinity;

    public GameObject exitButton;

    public GameObject storyEventNode;


    public GameObject enemyNode;
    public Image enemyNodeImage;
    public Text enemyTitleText;
    public Text levelLabel;
    public Text expLabel;
    public Transform enemyCards;

    public GameObject settingButton;


    [Header("Special Challenge")] 
    public GameObject specialChallenge;
    //public Image specialChallengeImage;
    public VisibleCard specialChallengeVcCard;
    public Image specialChallengeRewardImage;
    public Text specialChallengeRewardText;
    public Text specialChallengeRewardDesc;
    public Text specialChallengeText;
   public Toggle specialChallengeToggle;
   public Text specialChallengeRewardCountText;
   public Sprite[] specialChallengeRewardSprites;
   public int[] specialChallengeRewardStones;
   public int[] specialChallengeRewardUpgrades;
   public int[] specialChallengeRewardDeletes;

   [HideInInspector]
   public List<string> orderedStorysEventIndex = new List<string>();//事件的选项顺序触发
   public List<string> orderedStorysEventIndexThisRound = new List<string>();//事件的选项顺序触发

   public void Awake()
   {
       specialChallengeToggle.onValueChanged.AddListener(UpdateView);
   }
   // public void ShowSpecialChallengeCard()
   // {
   //
   //     if (GetComponent<Doozy.Engine.UI.UIView>().IsHiding) return;
   //     if (ferocityFirstCard != null)
   //     {
   //         MenuControl.Instance.infoMenu.ShowInfo(ferocityFirstCard,specialChallengeImage.transform);
   //     }else if (specialChallengeSkill != null)
   //     {
   //         MenuControl.Instance.infoMenu.ShowInfo(specialChallengeSkill.skills[0],specialChallengeImage.transform);
   //         
   //     }
   //     else
   //     {
   //         Debug.LogError(("No Special Challenge"));
   //     }
   // }

   public void HideSpecialChallengeCardInfo()
   {
       MenuControl.Instance.infoMenu.HideMenu();
   }
    
    public override void ShowMenu()
    {
        base.ShowMenu();
        
        settingButton.SetActive(MenuControl.Instance.testMode);
        gmWinBattleBtn.interactable = false;
        gmRevealAreaBtn.interactable = true;
    }
    
    public override void HideMenu(bool instantly = false)
    {
        base.HideMenu(instantly);
        settingButton.SetActive(true);
        gmWinBattleBtn.interactable = false;
        gmRevealAreaBtn.interactable = true;
    }

    public void ShowEnemyDeck()
    {
        MenuControl.Instance.ShowEnemyDeckPopup();
    }


    public void UpdateView(bool _ = false)
    {
        gmWinBattleBtn.interactable = true;
        gmRevealAreaBtn.interactable = false;
        isSpecialChallenge = specialChallengeToggle.isOn;
        foreach (Transform child in enemyCards)
        {
            Destroy(child.gameObject);
        }
        var cardsToShow = new List<Card>();
        cardsToShow.AddRange(MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().allOwnedCards);
        Dictionary<Card, int> cardCount = new Dictionary<Card, int>();
        if (MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().GetHero().GetIntentSystem() != null)
        {
            foreach (IntentSystemHand hand in MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().GetHero().GetIntentSystem().hands)
            {
                if (hand.GetHandCards().Count > 0)
                {
                    cardsToShow.AddRange(hand.GetHandCards());
                }
                else
                {
                    cardsToShow.AddRange(hand.cards);
                }
            }
        }

        if (cardsToShow.Count > 0)
        {
            enemyNodeImage.sprite = MenuControl.Instance.csvLoader.playerCardSprite(cardsToShow[0]);
        }
        
        foreach (Card card in cardsToShow)
        {
            if (cardCount.ContainsKey(card))
            {
                cardCount[card]++;
            }
            else
            {
                cardCount[card] = 1;
            }
        }

        var cards = cardCount.Keys.ToList();
        var sortedCards = cards.OrderBy(x => x.CardTypeSortedValue());
        foreach (var key in sortedCards)
        {
            var value = cardCount[key];
            VisibleCard vc = Instantiate(MenuControl.Instance.visibleCardPrefab, enemyCards);
            vc.isEnemy = true;
            vc.RenderCardForMenu(key);
            vc.SetHandCardCount(value);
        }
    }
    
    public void ShowEncounterEvent(bool forceBattle,int index)
    {
        var encounter = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter();
        if (encounter.isBoss)
            ShowEvent(bossEncounterEventDefinition,index);
        else if (encounter.cannotBeSkipped)
            ShowEvent(unskippableEncounterEventDefinition,index);
        else 
            ShowEvent(encounterEventDefinition,index);

        enemyNode.SetActive(true);
        eventTitleText.transform.parent.gameObject.SetActive(false);
        

        levelLabel.text = encounter.level.ToString();
        expLabel.text = encounter.GetAdjustedXP().ToString();

        
        UpdateView();
       

        if (forceBattle)
        {
            exitButton.SetActive(false);
        }
        
        battleEventBK.gameObject.SetActive(true);
        normalEventBK.gameObject.SetActive(false);
    }
    public override void SelectVisibleCard(VisibleCard vc, bool withClick = true)
    {
        base.SelectVisibleCard(vc, withClick);
        if (GetComponent<Doozy.Engine.UI.UIView>().IsHiding) return;
        MenuControl.Instance.infoMenu.ShowInfo(vc);
    }
    public override void DeSelectVisibleCard(VisibleCard vc, bool withClick = true)
    {
        base.DeSelectVisibleCard(vc, withClick);
        MenuControl.Instance.infoMenu.HideMenu();
    }

    public Transform spineTransform;


    [HideInInspector] public bool isSpecialChallenge;
    [HideInInspector] public bool specialChallengeFerocity = false;
    [HideInInspector] public SpecialChallengeSkillSet specialChallengeSkill = null;
    //public AdventureItemEncounter currentEncounter;
    [HideInInspector]
    public Card ferocityFirstCard = null;
    [HideInInspector] public AdventureItemEncounter.EncounterSpeicallChallengeRewardType specialChallengeReward = AdventureItemEncounter.EncounterSpeicallChallengeRewardType.None;
    void LoadSpecialChallenge(AdventureItemEncounter encounter, int index)
    {
        specialChallengeToggle.isOn = false;
        //currentEncounter = encounter;
        if (!MenuControl.Instance.defeatedEncounter.Contains(encounter.UniqueID) && !MenuControl.Instance.settingsMenu.forceShowSpecialChallenge)
        {
            specialChallenge.SetActive(false);
            return;
        }

        AdventureItemInfo info = MenuControl.Instance.adventureMenu.adventureItemInfos[index];
        specialChallengeReward = info.specialChallengeRewardIndex;
        specialChallenge.SetActive(true);
        if (info.specialChallengeIndex == -1)
        {
            info = encounter.GenerateSpecialChallenge();
            encounter.PickRandomSettingValue();
            MenuControl.Instance.adventureMenu.adventureItemInfos[index] = info;
        }

        if (info.specialChallengeIndex == -1)
        {
            Debug.LogError("No Special Challenge");
            specialChallenge.SetActive(false);
            return;

        }
        
        var challengeType = encounter.GetSpecialChallengeType(info);
        
        
        switch (challengeType)
        {
            case AdventureItemEncounter.SpecialChallengeType.Cards:
                var intentSystem = encounter.GetHero().GetIntentSystem();
                bool hasFerocityCard = false;
                foreach (var hand in intentSystem.hands)
                {
                    if (hand.ferocityACards.Count > 0)
                    {
                        ferocityFirstCard = hand.ferocityACards[0];
                        if (hand.ferocityACards[0] != null)
                        {
                            hasFerocityCard = true;
                            specialChallengeVcCard.isEnemy = true;
                            specialChallengeVcCard.RenderCard(hand.ferocityACards[0]);
                            //specialChallengeImage.sprite = MenuControl.Instance.csvLoader.playerCardSprite(hand.ferocityACards[0]);
                        }
                    }
                }

                if (!hasFerocityCard)
                {
                    Debug.LogError($"{encounter.name} {encounter.GetChineseName()} 特殊挑战 没有ferocity card" );
                }
                specialChallengeFerocity = true;
                specialChallengeText.text = MenuControl.Instance.GetLocalizedString("SpecialChallenge_Card");
                break;
            case  AdventureItemEncounter.SpecialChallengeType.Skill:
                var innerIndex = encounter.GetSpecialChallengeInnerIndex(info);
                if (encounter.specialChallengeSkills.Count <= innerIndex)
                {
                    Debug.LogError($"specialChallengeSkills index {encounter.specialChallengeSkills.Count} 小于需要的index  {{innerIndex}}");
                    innerIndex = 0;
                }

                if (encounter.specialChallengeSkills.Count > innerIndex)
                {
                    if (encounter.specialChallengeSkills[innerIndex].skills.Count == 0)
                    {
                        Debug.LogError("no skill defined");
                        specialChallenge.SetActive(false);
                        return;
                    }
                    specialChallengeVcCard.RenderCard(encounter.specialChallengeSkills[innerIndex].skills[0]);
                    //specialChallengeImage.sprite = MenuControl.Instance.csvLoader.talentSprite(encounter.specialChallengeSkills[innerIndex].skills[0].GetChineseName());
                    
                    specialChallengeSkill = encounter.specialChallengeSkills[innerIndex];
                }
                else
                {
                    Debug.LogError("no specialChallengeSkills defined");
                   // specialChallenge.SetActive(false);
                    return;
                }

                specialChallengeText.text = MenuControl.Instance.GetLocalizedString("SpecialChallenge_Skill");
                break;
        }

        var rewardName = $"SpecialChallenge_{info.specialChallengeRewardIndex.ToString()}CardName";
        
        var rewardDesc = $"SpecialChallenge_{info.specialChallengeRewardIndex.ToString()}CardDescription";
        int[] rewardList = new int[3];
        switch (info.specialChallengeRewardIndex)
        {
            case AdventureItemEncounter.EncounterSpeicallChallengeRewardType.Upgrade:
                rewardList = MenuControl.Instance.eventMenu.specialChallengeRewardUpgrades;
                break;
            case AdventureItemEncounter.EncounterSpeicallChallengeRewardType.Delete:
                rewardList = MenuControl.Instance.eventMenu.specialChallengeRewardDeletes;
                break;
            case AdventureItemEncounter.EncounterSpeicallChallengeRewardType.ExtraStone:
                rewardList = MenuControl.Instance.eventMenu.specialChallengeRewardStones;
                break;
        }
        var count = LootMenu.GetChallengeRewardValue(rewardList,
            (AdventureItemEncounter)MenuControl.Instance.adventureMenu.GetCurrentAdventureItem());
        specialChallengeRewardText.text =  MenuControl.Instance.GetLocalizedString(rewardName);
        var spriteIndex = (int)info.specialChallengeRewardIndex;
        if (spriteIndex >= specialChallengeRewardSprites.Length)
        {
            spriteIndex = specialChallengeRewardSprites.Length - 1;
        }
        specialChallengeRewardImage.sprite = specialChallengeRewardSprites[spriteIndex];
        specialChallengeRewardDesc.text =  string.Format(MenuControl.Instance.GetLocalizedString(rewardDesc),count);
        specialChallengeRewardCountText.text = count.ToString();

    }
    public void ShowEvent(EventDefinition definition,int index = 0)
    {
        if (originalChoicesPanelSpacing == float.NegativeInfinity)
        {
            originalChoicesPanelSpacing = choicesPanel.GetComponent<HorizontalLayoutGroup>().spacing;
        }
        if (definition.UniqueID == "EventArtifactChest")
        {
            definition.GetEventChoices()[1]. BeforePerformChoice();
            definition.GetEventChoices()[1].PerformChoice();
            return;
        }
        battleEventBK.gameObject.SetActive(false);
        normalEventBK.gameObject.SetActive(true);
        MenuControl.Instance.eventMenu.isSpecialChallenge = false;
        specialChallengeFerocity = false;
        specialChallengeSkill = null;
        //currentEncounter = null;
        ferocityFirstCard = null;
        specialChallengeReward = AdventureItemEncounter.EncounterSpeicallChallengeRewardType.None;
        var eventType = definition.GetChineseName();
        if (eventType == "吟游诗人" || eventType == "驱灵人" || eventType == "旅行商人" || definition is EventDefinitionEncounter 
            /*eventType == "治疗师"*/)
        {
            if (definition is EventDefinitionEncounter)
            {
                
            }
            else
            {
                MenuControl.Instance.adventureMenu.FinishItem();
            }
            exitButton.SetActive(true);
        }
        else
        {
            exitButton.SetActive(false);
        }

        if (definition.UniqueID == "EventStory")
        {
            storyEventNode.SetActive(true);
        }
        else
        {
            
            storyEventNode.SetActive(false);
        }
        
        if (definition.GetComponent<AdventureItem>() is AdventureItemUpgradeCards)
        {
            MenuControl.Instance.shopMenu.ShowShopUpgrade(MenuControl.Instance.adventureMenu.GetBlacksmith().GetName());
            return;
        }
        if (definition.GetComponent<AdventureItem>() is AdventureItemPurchaseCards)
        {
            MenuControl.Instance.shopMenu.ShowShopPurchase(MenuControl.Instance.adventureMenu.GetItemCards(),MenuControl.Instance.adventureMenu.GetShop(). GetName());
            return;
        }
        if (definition.GetComponent<AdventureItem>() is AdventureItemRemoveCards)
        {
            MenuControl.Instance.shopMenu.ShowShopRemoval(MenuControl.Instance.adventureMenu.GetMonastery().GetName());
            return;
        }
        if (definition.GetComponent<AdventureItem>() is AdventureItemTreasure)
        {
            MenuControl.Instance.shopMenu.ShowShopTreasure(MenuControl.Instance.adventureMenu.GetTreasure().GetName());
            return;
        }
        ShowMenu();
        
        //GetComponentInChildren<HeroInfoPanel>().updateHeroInfo();
        
        enemyNode.SetActive(false);
        spineTransform.gameObject.SetActive(false);
        eventImage.gameObject.SetActive(false);
        if (definition.hasLargeImage)
        {
            if (definition.UniqueID == "EventStory")
            {
                eventImage.sprite = null;
                spineTransform.gameObject.SetActive(true);
                //load spine
                MenuControl.Instance.csvLoader.loadSpineForStoryEventSpine(definition.GetChineseName(), spineTransform);
            }else if (definition.isEncounter)
            {
            
                eventImage.sprite = null;
                spineTransform.gameObject.SetActive(true);
                //load spine
                MenuControl.Instance.csvLoader.loadSpineForEncounterEventSpine(MenuControl.Instance.adventureMenu.GetCurrentAdventureItem()
                    .GetChineseName(), spineTransform);
                
                LoadSpecialChallenge(MenuControl.Instance.adventureMenu.GetCurrentAdventureItem() as AdventureItemEncounter,index);

            }
            else if (definition.GetSprite() == null)
            {
                eventImage.gameObject.SetActive(true);
                eventImage.sprite =
                    MenuControl.Instance.csvLoader.eventSprite(MenuControl.Instance.adventureMenu.GetCurrentAdventureItem()
                        .GetChineseName());// MenuControl.Instance.adventureMenu.GetCurrentAdventureItem().GetSprite();
                //eventImage.sprite = MenuControl.Instance.adventureMenu.GetCurrentAdventureItem().GetSprite();
            }
            else
            {
                eventImage.gameObject.SetActive(true);
                eventImage.sprite = definition.GetSprite();
                // eventImage.sprite = definition.GetSprite();
            }
            
            eventImage.gameObject.SetActive(eventImage.sprite != null);
            eventImage.SetNativeSize();
        }
        else
        {
            eventImage.gameObject.SetActive(false);
        }
        

        eventTitleText.transform.parent.gameObject.SetActive(true);
        eventTitleText.text = definition.GetName();
        enemyTitleText.text = definition.GetName();
        var description = definition.GetDescription();
        if (definition.isEncounter)
        {
            string[] lines = description.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length > 2) 
            {
                description = string.Join("\n", lines.Skip(2));
                // 输出: Line3 和 Line4
                //Console.WriteLine(result);
            }
        }
        eventDescriptionText.text = description;
       // squareBorderImage.SetActive(MenuControl.Instance.adventureMenu.GetCurrentAdventureItem() != null && !MenuControl.Instance.adventureMenu.GetCurrentAdventureItem().alwaysOpen);

        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject);
        }

        if (definition.GetEventChoices().Count > 4)
        {
            Debug.LogError(eventType+" event has more than 4 choices");
        }

        float scale = 1;
        if (definition.GetEventChoices().Count > 3)
        {
            scale = 3f / (float)definition.GetEventChoices().Count;
            choicesPanel.GetComponent<HorizontalLayoutGroup>().spacing = 0;
        }
        else
        {
            
            choicesPanel.GetComponent<HorizontalLayoutGroup>().spacing = originalChoicesPanelSpacing;
        }
        for (int ii = 0; ii < definition.GetEventChoices().Count; ii += 1)
        {
            int xx = ii;
            ChoicePanel panel = Instantiate(MenuControl.Instance.choicePanelPrefab, choicesPanel);
#if !UNITY_STANDALONE
            panel.transform.localScale = Vector3.one*1.5f;
            panel.GetComponent<CanvasGroup>().enabled = false;
#endif

            panel.RenderEventChoice(definition.GetEventChoices()[xx], () => {
                //panel.GetComponent<CanvasGroup>().interactable = false;
                definition.GetEventChoices()[xx].BeforePerformChoice();
                definition.GetEventChoices()[xx].PerformChoice();
            }, "A,B,C,D".Split(',')[xx]);
            panel.transform.localScale = Vector3.one * scale;
        }
    }

}
