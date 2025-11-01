using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;
using SuperScrollView;
using TMPro;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum ShopMode
{
    Upgrade,
    Purchase,
    Removal,
    Treasure,
    FreeUpgrade,
    FreeRemoval,
    PurchaseReaper
}

public class ShopMenu : BasicMenu
{
    public ShopMode shopMode;
    public Text titleText;
    public Text promptText;
    public Text noTreasureTipsText;
    public TMP_Text costText;

    public Color offColor;

    public Color onColor;
    
    
    // public Sprite onImage;
    // public Sprite offImage;

    public GameObject deleteView;
    public GameObject upgradeView;
    public GameObject purchaseView;
    public GameObject currentView;

    public Button leftArrow;
    public Button rightArrow;

    //public Image shopKeeperImage;
    // public List<Sprite> shopKeeperImages = new List<Sprite>();
    //public Image shopIconImage;

    public int upgrades;
    public int purchases;
    public int removals;

    public GameObject sellTreasureCostOb;
    public Text sellTreasureCount;

    public UIView confirmationPanel;
    public Transform confirmationCardHolder;
    public Text confirmationText;

    public UpgradeSelectCardView upgradeSelectCardView;

    public Transform grid;

    [HideInInspector] public List<Card> cards = new List<Card>();
    [HideInInspector] public List<Card> treasureCards = new List<Card>();

    public Card selectedCard;
    public Button confirmButton;

    public RefreshButton refreshButton;

    public System.Action afterFreeUpgradeAction;

    public Card flareStoneTemplate;

    public Card upgradedSeasonsCard;

    ShopButton ActionText;
    ShopButton SellText;

    public float cellSize = 250;

    public int freePurchaseCount = 1;
    public int freeRemoveCount = 1;
    public int freeUpgradeCount = 1;


    public int purchaseCardCostLevel3 = 4;
    public int purchaseCardCostLevel2 = 3;
    public int purchaseCardCostLevel1 = 2;
    public int purchaseCardCostLevel0 = 2;
    public int upgradeCardCostLevel3 = 4;
    public int upgradeCardCostLevel2 = 3;
    public int removeCardStartCost = 1;
    public int removeCardMaxCost = 6;
    public int removeCardCostIncrease = 1;
    [HideInInspector] public int currentRemoveCardCost;
    
    
    public List<int> purchaseRefreshCost = new List<int>() { 1, 2, 3 };
    public int purchaseRefreshCount = 0;

    [Header("Discount")] public int minDiscountCount = 1;
    public int maxDiscountCount = 3;
    public int discount = 50;
    public int potionDiscount = 100;
    

    public LoopListView2 mLoopListView;

    public GameObject arrows;

    private void Awake()
    {
        mLoopListView.InitListView(6, OnGetItemByIndex);
    }

    private void Start()
    {
        
        rightArrow.onClick.AddListener(() => PressArrow(true));
        leftArrow.onClick.AddListener(() => PressArrow(false));
    }

    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0)
        {
            return null;
        }

        LoopListViewItem2 item = listView.NewListViewItem("VisualCardPrefab");
        VisibleCard itemScript = item.GetComponent<VisibleCard>();

        var toShowCards = cards;
        var extraData =  MenuControl.Instance.adventureMenu.GetItemExtraData();
        if (isSellingTreasure)
        {
            toShowCards = treasureCards;
        }

        if (index >= toShowCards.Count)
        {
            return item;
        }

        Card itemData = toShowCards[index];
        itemScript.RenderCard(itemData);

        bool isDisabled = false;
        if (!isSellingTreasure)
        {
            if (shopMode == ShopMode.FreeUpgrade || shopMode == ShopMode.Upgrade)
            {
                if (itemData.RandomUpgradeCard == null)
                {
                    itemScript.DisableCard();
                    isDisabled = true;
                }
            }

            if (shopMode == ShopMode.Purchase)
            {
                if (index >= extraData.Count)
                {
                    Debug.LogError(("extraData.Count != cards.Count"));
                }else
                if (extraData[index] == 1) 
                {
                    itemScript.discountObj.SetActive(true);
                    if (itemData.isPotion)
                    {
                        itemScript.discountText.text = $"-{potionDiscount}%";
                    }
                    else
                    {
                        
                        itemScript.discountText.text = $"-{discount}%";
                    }
                }
            }
        }

        if (!isDisabled)
        {
            itemScript.EnableCard();
        }

        return item;
    }

    //for loop list
    float adjustValue = 5;
    float adjustValue2 = 1;
    float adjustValue3 = 0.3f;

    void LateUpdate()
    {
        mLoopListView.UpdateAllShownItemSnapData();
        int count = mLoopListView.ShownItemCount;
        for (int i = 0; i < count; ++i)
        {
            LoopListViewItem2 item = mLoopListView.GetShownItemByIndex(i);
            VisibleCard itemScript = item.GetComponent<VisibleCard>();
            if (itemScript != null)
            {
                //-5.55
                //-5.79
                // float scale = 1 - Mathf.Abs(item.DistanceWithViewPortSnapCenter) / adjustValue;
                float scale = Mathf.Abs(item.transform.position.x + adjustValue3) - adjustValue;
                scale = 1 - scale;
                scale *= adjustValue2;
                scale = Mathf.Clamp(scale, 0f, 1);
                scale *= scale;
                itemScript.GetComponent<CanvasGroup>().alpha = scale;
                scale *= 0.65f;
                itemScript.transform.localScale = new Vector3(scale, scale, 1);
            }
        }
        leftArrow.gameObject.SetActive( canClickArrow(false));
        rightArrow.gameObject.SetActive(canClickArrow(true));
    }


    public void ShowShopFreeRemoval(string nameOfShop, System.Action action)
    {
        afterFreeUpgradeAction = action;
        titleText.text = nameOfShop;
        cards = new List<Card>();
        shopMode = ShopMode.FreeRemoval;
        //refreshButton.gameObject.SetActive(false);
        ShowMenu();
    }

    public void ShowShopFreeUpgrade(string nameOfShop, System.Action action)
    {
        afterFreeUpgradeAction = action;
        titleText.text = nameOfShop;
        cards = new List<Card>();
        shopMode = ShopMode.FreeUpgrade;
        //refreshButton.gameObject.SetActive(false);
        ShowMenu();
    }

    public bool shownFlareInfoInShop;
    void ShowFlareInfo()
    {
        if (!shownFlareInfoInShop)
        {
            MenuControl.Instance.confirmPopupView.ShowGetFlareInfoPopup();
            shownFlareInfoInShop = true;
            
            MenuControl.Instance.dataControl.SaveData();
        }
    }

    public void ShowShopUpgrade(string nameOfShop)
    {
        ShowFlareInfo();
        titleText.text = nameOfShop;
        cards = new List<Card>();
        shopMode = ShopMode.Upgrade;
        //refreshButton.gameObject.SetActive(false);
        ShowMenu();
        afterShowMenu();
    }

    public void RefreshPurchaseItems()
    {
        if (MenuControl.Instance.heroMenu.flareStones >= refreshFlareStoneCost())
        {
            MenuControl.Instance.heroMenu.consumeFlareStone((refreshFlareStoneCost()));
        }
        else
        {
            return;
        }
        
        purchaseRefreshCount++;
        
        //重抽
        List<Card> newCards = ((AdventureItemPurchaseCards)MenuControl.Instance.adventureMenu.GetCurrentAdventureItem()).MakeCardList();
        //检查是否和之前一样多
        var previousCards = MenuControl.Instance.adventureMenu.GetItemCards();
        // if (newCards.Count < previousCards.Count)
        // {
        //     Debug.LogError("重抽数量错误");
        // }

        MenuControl.Instance.adventureMenu.ReplaceItemCards(newCards);
        cards = MenuControl.Instance.adventureMenu.GetItemCards();

        GenerateDiscounts();
        
        ShowMenu();
        
    }

    public void AddShopItem()
    {
        var indexOfShop = MenuControl.Instance.adventureMenu.adventureItems.FindIndex((x=>x is AdventureItemPurchaseCards));
        var isShopOpen = MenuControl.Instance.adventureMenu.adventureItemChecked[indexOfShop];
        if (!isShopOpen)
        {
            return;
        }
        var list = MenuControl.Instance.heroMenu.heroClass.GetComponent<ClassSpecialization>().CalculatePurchaseDrops();
        list.Shuffle();
        var previousCards = MenuControl.Instance.adventureMenu.GetItemCards(indexOfShop);
        var previousDiscounts = MenuControl.Instance.adventureMenu.GetItemExtraData(indexOfShop);
        
        var addOne = list.RandomItem();
        foreach (var card in list)
        {
            bool isExist = false;
            foreach (var preCard in previousCards)
            {
                
                if (preCard.UniqueID == card.UniqueID)
                {
                    isExist = true;
                    break;
                }
            }

            if (!isExist)
            {
                addOne = card;
                break;
            }
        }
        previousCards.Insert(0,addOne);
        previousDiscounts.Insert(0,Random.Range(0,3)==0?1:0);
        MenuControl.Instance.adventureMenu.ReplaceItemCards(previousCards,indexOfShop);
        MenuControl.Instance.adventureMenu.ReplaceItemExtraData(previousDiscounts,indexOfShop);
    }

    
    
    public void GenerateDiscounts()
    {
        int randomCount = Random.Range(minDiscountCount, maxDiscountCount+1);
        List<int> discountValues = new List<int>();
        int i = 0;
        cards = MenuControl.Instance.adventureMenu.GetItemCards();
        for (; i < randomCount; i++)
        {
            discountValues.Add(1);
        }

        for (; i < cards.Count; i++)
        {
            discountValues.Add(0);
        }
        discountValues.Shuffle();
        MenuControl.Instance.adventureMenu.ReplaceItemExtraData(discountValues);
    }
    
    public void ShowShopPurchase(List<Card> cardsToBuy, string nameOfShop)
    {
        ShowFlareInfo();
        titleText.text = nameOfShop;
        cards = cardsToBuy;
        //cards.Add(flareStoneTemplate);
        shopMode = ShopMode.Purchase;
        ShowMenu();
        afterShowMenu();
    }

    private Card moveToCard;

    public void ShowShopPurchaseReaper(List<Card> cardsToBuy, string nameOfShop)
    {
        ShowFlareInfo();
        titleText.text = nameOfShop;
        cards = cardsToBuy;
        cards.Add(flareStoneTemplate);
        shopMode = ShopMode.PurchaseReaper;
        //refreshButton.gameObject.SetActive(false);
        ShowMenu();
    }

    public void ShowShopRemoval(string nameOfShop)
    {
        ShowFlareInfo();
        titleText.text = nameOfShop;
        cards = new List<Card>();
        shopMode = ShopMode.Removal;
        //refreshButton.gameObject.SetActive(false);
        ShowMenu();
        afterShowMenu();
    }

    public void ShowShopTreasure(string nameOfShop)
    {
        titleText.text = nameOfShop;
        cards = MenuControl.Instance.adventureMenu.GetItemCards();
        shopMode = ShopMode.Treasure;
        ShowMenu();
        afterShowMenu();
    }

    public void ShowShopTreasure(List<Card> cardsToCollect, string nameOfShop)
    {
        titleText.text = nameOfShop;
        cards = cardsToCollect;
        shopMode = ShopMode.Treasure;
        ShowMenu();
    }

    private Sequence sequence;

    void afterShowMenu()
    {
        if (promptText.text != "")
        {
            var textTrans = promptText.transform.parent.gameObject.transform;
            sequence.Kill();
            // textTrans.DOKill();
            textTrans.localScale = Vector3.zero;
            promptText.transform.parent.gameObject.SetActive(promptText.text != "");
            sequence = DOTween.Sequence().Append(textTrans.DOScale(Vector3.one, 0.3f)).AppendInterval(3f)
                .Append(textTrans.DOScale(Vector3.zero, 0.3f)).AppendInterval(3f).SetLoops(-1, LoopType.Restart);
        }
    }

    void setButtonState(ShopButton shopButton, bool isOn)
    {
        if (isOn)
        {
            shopButton.textHide.color = onColor;
            shopButton.textShow.color = onColor;
            shopButton.textShow.gameObject.SetActive(true);
            shopButton.textHide.gameObject.SetActive(false);
            shopButton.buttonShow.gameObject.SetActive(true);
            shopButton.buttonHide.gameObject.SetActive(false);
        }
        else
        {
            shopButton.textHide.color = offColor;
            shopButton.textShow.color = offColor;
            shopButton.textShow.gameObject.SetActive(false);
            shopButton.textHide.gameObject.SetActive(true);
            shopButton.buttonShow.gameObject.SetActive(false);
            shopButton.buttonHide.gameObject.SetActive(true);
        }
    }

    public override void ShowMenu()
    {
        isSellingTreasure = false;

        noTreasureTipsText.gameObject.SetActive(false);
        if (shopMode == ShopMode.Treasure)
        {
            refreshButton.UpdateView();
            // ActionText.text = MenuControl.Instance.GetLocalizedString("Cancel");
            //Show multi select
            List<string> buttonLabels = new List<string>();
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Cancel"));
            buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Confirm"));

            List<System.Action> actions = new List<System.Action>();
            actions.Add(() => { });
            actions.Add(() =>
            {
                List<VisibleCard> vcsToAnimate = new List<VisibleCard>();
                foreach (int integer in MenuControl.Instance.cardChoiceMenu.selectedVisibleCardInts)
                {
                    MenuControl.Instance.adventureMenu.RemoveItemCard(
                        MenuControl.Instance.cardChoiceMenu.cardsToShow[integer]);
                    MenuControl.Instance.heroMenu.AddCardToDeck(
                        MenuControl.Instance.cardChoiceMenu.cardsToShow[integer]);
                    vcsToAnimate.Add(MenuControl.Instance.cardChoiceMenu.visibleCardsShown[integer]);
                }

                MenuControl.Instance.adventureMenu.AnimateVCsToDeck(vcsToAnimate);

                // treasure宝藏即使没领完也算完成事件
                // if (MenuControl.Instance.cardChoiceMenu.selectedVisibleCardInts.Count ==
                //     MenuControl.Instance.cardChoiceMenu.cardsToShow.Count)
                {
                    MenuControl.Instance.adventureMenu.RemoveItem();
                }

                MenuControl.Instance.adventureMenu.RenderScreen(true);
            });

            //低级宝箱
            MenuControl.Instance.cardChoiceMenu.ShowChoice(cards, buttonLabels, actions,
                MenuControl.Instance.GetLocalizedString("ShopTreasurePrompt",
                    "There be some loot! Select any items you wish to keep."), 1, cards.Count, true, 0, true);
            return;
        }

        //GetComponentInChildren<HeroInfoPanel>().updateHeroInfo();
        base.ShowMenu();
        //shopIconImage.sprite = MenuControl.Instance.adventureMenu.GetCurrentAdventureItem().GetSprite();
        grid.transform.parent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        selectedCard = null;
        upgradeSelectCardView.ClearSelectedCard();
        if (currentView)
        {
            purchaseView.SetActive(false);
            deleteView.SetActive(false);
            upgradeView.SetActive(false);
        }

        if (shopMode == ShopMode.Purchase)
        {
            //refreshButton.gameObject.SetActive(true);
            refreshButton.UpdateView(refreshFlareStoneCost());
            
            //shopKeeperImage.sprite = shopKeeperImages[0];
            currentView = purchaseView;
            currentView.SetActive(true);
            ActionText = currentView.transform.Find("Button - shopAction").GetComponentInChildren<ShopButton>();
            SellText = currentView.transform.Find("Button - sellTreasure").GetComponentInChildren<ShopButton>();
            ActionText.SetText(MenuControl.Instance.GetLocalizedString("ChoicePurchaseCardName"));

            promptText.text = NextCost(shopMode) > 0
                ? MenuControl.Instance.GetLocalizedString("EventPurchaseCard2Description")
                : MenuControl.Instance.GetLocalizedString("ShopPurchasePrompt");

            confirmationText.text = MenuControl.Instance.GetLocalizedString("ShopPurchaseConfirmationPrompt",
                "Do you want to purchase this card?");
        }
        else if (shopMode == ShopMode.PurchaseReaper)
        {
            Debug.LogError("no shop mode ShopMode.PurchaseReaper");
            // ActionText.text = MenuControl.Instance.GetLocalizedString("ChoicePurchaseCardName");
            // shopKeeperImage.sprite = shopKeeperImages[4];
            //
            // promptText.text = MenuControl.Instance.GetLocalizedString("ReaperModeStorePrompt");
            //
            // confirmationText.text = MenuControl.Instance.GetLocalizedString("ShopPurchaseConfirmationPrompt",
            //     "Do you want to purchase this card?");
        }
        else if (shopMode == ShopMode.Removal)
        {
            currentView = deleteView;
            currentView.SetActive(true);
            ActionText = currentView.transform.Find("Button - shopAction").GetComponentInChildren<ShopButton>();
            SellText = currentView.transform.Find("Button - sellTreasure").GetComponentInChildren<ShopButton>();
            ActionText.SetText(MenuControl.Instance.GetLocalizedString("ChoiceRemovalCardName"));
            cards.Clear();
            foreach (Card card in MenuControl.Instance.heroMenu.cardsOwned)
            {
                if (!(card is Hero) && !(card.cardTags.Contains(MenuControl.Instance.treasureTag)) && !card.isPotion)
                    cards.Add(card);
            }

            cards = cards;
            cards = Card.SortCardsWithId(cards);
            promptText.text = NextCost(shopMode) > 0
                ? MenuControl.Instance.GetLocalizedString("EventRemovalCard2Description")
                : MenuControl.Instance.GetLocalizedString("ShopRemovalPrompt");
            confirmationText.text = MenuControl.Instance.GetLocalizedString("ShopRemovalConfirmationPrompt",
                "Do you want to remove this card?");
        }
        else if (shopMode == ShopMode.FreeRemoval)
        {
            currentView = deleteView;
            currentView.SetActive(true);
            ActionText = currentView.transform.Find("Button - shopAction").GetComponentInChildren<ShopButton>();
            SellText = currentView.transform.Find("Button - sellTreasure").GetComponentInChildren<ShopButton>();
            ActionText.SetText(MenuControl.Instance.GetLocalizedString("ChoiceRemovalCardName"));
            cards.Clear();
            foreach (Card card in MenuControl.Instance.heroMenu.cardsOwned)
            {
                if (!(card is Hero) && !(card.cardTags.Contains(MenuControl.Instance.treasureTag)) && !card.isPotion)
                    cards.Add(card);
            }
            cards = cards;

            cards = cards.OrderBy(x => MenuControl.Instance.GetCardTypeStringForTags(x.cardTags) + x.GetName())
                .ToList();
            promptText.text = MenuControl.Instance.GetLocalizedString("ShopRemovalPrompt");
            confirmationText.text = MenuControl.Instance.GetLocalizedString("ShopRemovalConfirmationPrompt",
                "Do you want to remove this card?");
        }
        else if (shopMode == ShopMode.Upgrade)
        {
            currentView = upgradeView;
            currentView.SetActive(true);
            ActionText = currentView.transform.Find("Button - shopAction").GetComponentInChildren<ShopButton>();
            SellText = currentView.transform.Find("Button - sellTreasure").GetComponentInChildren<ShopButton>();
            ActionText.SetText(MenuControl.Instance.GetLocalizedString("ChoiceUpgradeCardName"));
            cards.Clear();
            List<Card> cardList = new List<Card>();
            cardList.AddRange(MenuControl.Instance.heroMenu.cardsOwned);
            cardList =Card.SortCards(cardList);
            foreach (Card card in cardList)
            {
                if (card.RandomUpgradeCard != null)
                {
                    cards.Add(card);
                }
            }


            //this part is to show cards that are at max level, we still want to show them, but in grey
            foreach (Card card in cardList)
            {
                if (card.RandomUpgradeCard == null)
                {
                    if (card is Hero || card.cardTags.Contains(MenuControl.Instance.treasureTag) || card.isItem)
                    {
                        continue;
                    }

                    cards.Add(card);
                }
            }

            promptText.text = NextCost(shopMode) > 0
                ? MenuControl.Instance.GetLocalizedString("EventUpgradeCard2Description")
                : MenuControl.Instance.GetLocalizedString("ShopUpgradePrompt");


            confirmationText.text = MenuControl.Instance.GetLocalizedString("ShopUpgradeConfirmationPrompt",
                "Do you want to upgrade this card?");
        }
        else if (shopMode == ShopMode.FreeUpgrade)
        {
            currentView = upgradeView;
            currentView.SetActive(true);
            ActionText = currentView.transform.Find("Button - shopAction").GetComponentInChildren<ShopButton>();
            SellText = currentView.transform.Find("Button - sellTreasure").GetComponentInChildren<ShopButton>();
            ActionText.SetText(MenuControl.Instance.GetLocalizedString("ChoiceUpgradeCardName"));
            cards.Clear();
            List<Card> cardList = new List<Card>();
            cardList.AddRange(MenuControl.Instance.heroMenu.cardsOwned);
            cardList = cardList.OrderBy(x => MenuControl.Instance.GetCardTypeStringForTags(x.cardTags) + x.GetName())
                .ToList();

            foreach (Card card in cardList)
            {
                if (card.RandomUpgradeCard != null)
                {
                    cards.Add(card);
                }
            }

            //if uncomment, check potion
            // foreach (Card card in cardList)
            // {
            //     if (card.upgradeCard == null)
            //     {
            //         if (card is Hero || card.cardTags.Contains(MenuControl.Instance.treasureTag))
            //         {
            //             continue;
            //         }
            //
            //         cards.Add(card);
            //     }
            // }

            promptText.text = MenuControl.Instance.GetLocalizedString("ShopFreeUpgradePrompt");
            confirmationText.text = MenuControl.Instance.GetLocalizedString("ShopUpgradeConfirmationPrompt",
                "Do you want to upgrade this card?");
        }
        else if (shopMode == ShopMode.Treasure)
        {
            //shopKeeperImage.sprite = shopKeeperImages[3];
            promptText.text =
                MenuControl.Instance.GetLocalizedString("ShopTreasurePrompt",
                    "There be some loot. Open for the taking!");
            confirmationText.text =
                MenuControl.Instance.GetLocalizedString("ShopTreasureConfirmationPrompt",
                    "Do you want to take this card?");
        }


        setButtonState(SellText, false);
        setButtonState(ActionText, true);

        sellTreasureCount = SellText.transform.Find("CurrentTreasure").GetComponentInChildren<Text>();
        sellTreasureCount.text = MenuControl.Instance.heroMenu.GetTreasureCardsOwned().Count.ToString();
        sellTreasureCount.transform.parent.gameObject.SetActive(MenuControl.Instance.heroMenu.GetTreasureCardsOwned()
            .Count > 0);


        confirmationPanel.Hide(true);

        //TODO sort cards by type/tag then alphabetically

        // foreach (Transform child in grid)
        // {
        //     Destroy(child.gameObject);
        // }
 
  
        foreach (var card in cards)
        {
            MenuControl.Instance.progressMenu.cardsDiscovered.Add(card.UniqueID);
        }

        mLoopListView.SetListItemCount(cards.Count, false);
        mLoopListView.RefreshAllShownItem();
        
        //arrows.SetActive(cards.Count>6);

        int moveToIndex = 0;
        if (moveToCard != null)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (moveToCard == cards[i])
                {
                    moveToIndex = i;
                }
            }

            if (moveToIndex >= cards.Count)
            {
                moveToIndex = 0;
            }

            moveToCard = null;
        }
        
        mLoopListView.MovePanelToItemIndex(moveToIndex, 0);
        mLoopListView.RefreshAllShownItem();
        // foreach (Card card in cards)
        // {
        //     VisibleCard vc = Instantiate(MenuControl.Instance.visibleCardPrefab, grid);
        //     vc.RenderCardForMenu(card);
        //     if (shopMode == ShopMode.FreeUpgrade || shopMode == ShopMode.Upgrade)
        //     {
        //         if (card.upgradeCard == null)
        //         {
        //             vc.DisableCard();
        //         }
        //     }
        //
        //     vc.transform.localScale = Vector3.one * 0.65f;
        //     vc.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize, cellSize);
        // }

        // shopKeeperImage.sprite =
        //     MenuControl.Instance.csvLoader.eventSprite(MenuControl.Instance.adventureMenu.GetCurrentAdventureItem()
        //         .GetChineseName());


        titleText.text = MenuControl.Instance.adventureMenu.GetCurrentAdventureItem().GetName();
        // grid.GetComponent<RectTransform>().sizeDelta = new Vector2(grid.GetComponent<RectTransform>().sizeDelta.x,
        //     Mathf.CeilToInt(cards.Count / 3f) * (grid.GetComponent<GridLayoutGroup>().cellSize.y +
        //                                          grid.GetComponent<GridLayoutGroup>().spacing.y));

        MenuControl.Instance.adventureMenu.RenderUI();
    }

    private bool isSellingTreasure = false;

    public void PressArrow(bool isRight)
    {
        var index = mLoopListView.GetFirstShownItemIndexAndOffset().mItemIndex;
        var offset = mLoopListView.GetFirstShownItemIndexAndOffset().mItemOffset;
        var indexResult = index + (isRight ? 1 : -1) * 6;
        if (indexResult < 0)
        {
            mLoopListView.MovePanelToItemIndex(0, 0);
            
        }
       else  if (indexResult >= cards.Count)
        {
            
            mLoopListView.MovePanelToItemIndex(cards.Count-1, offset);
        }
        else
        {
            
            mLoopListView.MovePanelToItemIndex(indexResult, offset);
        }
    }

    public bool canClickArrow(bool isRight)
    {
        var index = mLoopListView.GetFirstShownItemIndexAndOffset().mItemIndex;
        var offset = mLoopListView.GetFirstShownItemIndexAndOffset().mItemOffset;
        //var indexResult = index + (isRight ? 1 : -1) * 6;
        if (index <= 0 && !isRight && offset>-100)
        {
            return false;
        }

        if (isRight && index+6 >= cards.Count - 1)
        {
            return false;
        }
        return true;
    }

    public void ShowSellTreasure()
    {
        isSellingTreasure = true;

        refreshButton.UpdateView();
        sellTreasureCount.text = MenuControl.Instance.heroMenu.GetTreasureCardsOwned().Count.ToString();

        confirmationPanel.Hide(true);
        setButtonState(SellText, true);
        setButtonState(ActionText, false);

        //cards.Clear();
        treasureCards = MenuControl.Instance.heroMenu.GetTreasureCardsOwned();

        if (treasureCards.Count <= 0)
        {
            MenuControl.Instance.confirmPopupView.ShowGetFlareInfoPopup();
            shownFlareInfoInShop = true;
        }
        noTreasureTipsText.gameObject.SetActive(treasureCards.Count <= 0);
        

        treasureCards = treasureCards
            .OrderBy(x => MenuControl.Instance.GetCardTypeStringForTags(x.cardTags) + x.GetName())
            .ToList();
        //promptText.text = "";

        // foreach (Transform child in grid)
        // {
        //     Destroy(child.gameObject);
        // }

        mLoopListView.SetListItemCount(treasureCards.Count, false);

        mLoopListView.MovePanelToItemIndex(0, 0);
        mLoopListView.RefreshAllShownItem();

        // foreach (Card card in treasureCards)
        // {
        //     VisibleCard vc = Instantiate(MenuControl.Instance.visibleCardPrefab, grid);
        //     vc.RenderCardForMenu(card);
        //     // if (shopMode == ShopMode.FreeUpgrade || shopMode == ShopMode.Upgrade)
        //     // {
        //     //     if (card.upgradeCard == null)
        //     //     {
        //     //         vc.DisableCard();
        //     //     }
        //     // }
        //
        //     vc.transform.localScale = Vector3.one * 0.65f;
        //     vc.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize, cellSize);
        // }

        confirmationText.text = MenuControl.Instance.GetLocalizedString("ShopRemovalConfirmationPrompt",
            "Do you want to remove this card?");


        MenuControl.Instance.adventureMenu.RenderUI();
    }


    public int NextCost(ShopMode mode)
    {
        int finalCost = 0;

        if (shopMode == ShopMode.FreeUpgrade)
        {
            return 0;
        }
        else if (shopMode == ShopMode.FreeRemoval)
        {
            return 0;
        }
        else if (mode == ShopMode.Purchase || mode == ShopMode.PurchaseReaper)
        {
            int reduction = MenuControl.Instance.CountOfCardsInList(
                MenuControl.Instance.levelUpMenu.extraFreeBarterTalent,
                MenuControl.Instance.levelUpMenu.variableTalentsAcquired);
            var value =  purchases - reduction - freePurchaseCount >= 0 ? 1 : 0;
            return value;
        }
        else if (mode == ShopMode.Removal)
        {
            int reduction = MenuControl.Instance.CountOfCardsInList(
                MenuControl.Instance.levelUpMenu.extraFreeBarterTalent,
                MenuControl.Instance.levelUpMenu.variableTalentsAcquired);
            return removals - reduction - freeRemoveCount >= 0 ? 1 : 0;
        }
        else if (mode == ShopMode.Upgrade)
        {
            int reduction = MenuControl.Instance.CountOfCardsInList(
                MenuControl.Instance.levelUpMenu.extraFreeBarterTalent,
                MenuControl.Instance.levelUpMenu.variableTalentsAcquired);
            return upgrades - reduction - freeUpgradeCount >= 0 ? 1 : 0;
        }

        return finalCost;
    }

    public override void SelectVisibleCard(VisibleCard vc, bool withClick = true)
    {
        if (GetComponent<Doozy.Engine.UI.UIView>().IsHiding) return;
        MenuControl.Instance.infoMenu.ShowInfo(vc);
    }

    public override void DeSelectVisibleCard(VisibleCard card, bool withClick = true)
    {
        MenuControl.Instance.infoMenu.HideMenu();
    }

    public override void ClickVisibleCard(VisibleCard vc)
    {
        if (shopMode != ShopMode.Upgrade && shopMode != ShopMode.FreeUpgrade && !confirmationPanel.IsVisible &&
            !upgradeSelectCardView.isVisible())
        {
            //MenuControl.Instance.infoMenu.ShowInfo(vc);
        }

        if (!confirmationPanel.IsVisible && !upgradeSelectCardView.isVisible())
        {
            selectedCard = vc.card;
            if ((shopMode == ShopMode.Upgrade || shopMode == ShopMode.FreeUpgrade) &&
                selectedCard.upgradeCards.Count > 1 && !isSellingTreasure)
            {
                //显示新的升级页面
                upgradeSelectCardView.ShowMenuFromShop(selectedCard);
            }
            else
            {
                upgradeSelectCardView.ClearSelectedCard();
                foreach (Transform child in confirmationCardHolder)
                {
                    Destroy(child.gameObject);
                }

                VisibleCard vc1 = Instantiate(MenuControl.Instance.visibleCardPrefab, confirmationCardHolder);
                vc1.RenderCardForMenu(vc.card);
                if (!isSellingTreasure)
                {
                    if (shopMode == ShopMode.Upgrade || shopMode == ShopMode.FreeUpgrade)
                    {
                        if (MenuControl.Instance.heroMenu.seasonsMode)
                        {
                            if (upgradedSeasonsCard != null)
                            {
                                Destroy(upgradedSeasonsCard.gameObject);
                            }

                            upgradedSeasonsCard = Instantiate(vc.card.RandomUpgradeCard, transform);
                            if (vc.card.cardTags.Contains(MenuControl.Instance.niceTag))
                            {
                                upgradedSeasonsCard.cardTags.Add(MenuControl.Instance.niceTag);
                            }
                            else if (vc.card.cardTags.Contains(MenuControl.Instance.naughtyTag))
                            {
                                upgradedSeasonsCard.cardTags.Add(MenuControl.Instance.naughtyTag);
                            }

                            VisibleCard vc2 = Instantiate(MenuControl.Instance.visibleCardPrefab,
                                confirmationCardHolder);
                            vc2.RenderCardForMenu(upgradedSeasonsCard);
                        }
                        else
                        {
                            VisibleCard vc2 = Instantiate(MenuControl.Instance.visibleCardPrefab,
                                confirmationCardHolder);
                            vc2.RenderCardForMenu(vc.card.RandomUpgradeCard);
                        }
                    }
                }
                else
                {
                    var go = Instantiate(sellTreasureCostOb, confirmationCardHolder);
                    go.GetComponentInChildren<Text>().text =
                        MenuControl.Instance.heroMenu.flareStoneWhenSell.ToString();
                }

                confirmationPanel.Show();

                confirmButton.interactable = true;
                
            }

            Doozy.Engine.Soundy.SoundyManager.Play(MenuControl.Instance.cardLiftSound);
        }
        if (isSellingTreasure)
        {
            costText.text = "";
            confirmationText.text = MenuControl.Instance.GetLocalizedString("ConfirmSellTreasure");
            ;
        }
        else
        {
            if (NextCost(shopMode) > 0 && MenuControl.Instance.heroMenu.flareStones < flarestoneCost())
            {
                confirmationText.text =
                    MenuControl.Instance.GetLocalizedString("You lack Flarestones to pay with.");
                confirmButton.interactable = false;
            }

            var costTextValue = NextCost(shopMode) == 0
                ? MenuControl.Instance.GetLocalizedString("Free")
                : string.Format(MenuControl.Instance.GetLocalizedString("Pay Flarestone"), flarestoneCost());
            costText.text = costTextValue;
            upgradeSelectCardView.costText.text = costTextValue;
        }
    }

    public int refreshFlareStoneCost()
    {
        return (purchaseRefreshCost[
            (purchaseRefreshCount >= purchaseRefreshCost.Count)
                ? purchaseRefreshCost.Count - 1
                : purchaseRefreshCount]);
    }
    public bool hasEnoughFlareStone()
    {
        return NextCost(shopMode) <=0 || MenuControl.Instance.heroMenu.flareStones >= flarestoneCost();
    }

    public void CancelAction()
    {
        MenuControl.Instance.infoMenu.HideMenu();
        selectedCard = null;
        confirmationPanel.Hide();
    }

    int flarestoneCost()
    {
        int res = 0;

        if (shopMode == ShopMode.Upgrade)
        {
            if (selectedCard.level >= 2)
            {
                res = upgradeCardCostLevel3;
            }
            else
            {
                res = upgradeCardCostLevel2;
            }
        }
        else if (shopMode == ShopMode.Purchase)
        {
            switch (selectedCard.level)
            {
                case 3:
                    res = purchaseCardCostLevel3;
                    break;
                case 2:
                    res = purchaseCardCostLevel2;
                    break;
                case 1:
                    res = purchaseCardCostLevel1;
                    break;
                case 0:
                    res = purchaseCardCostLevel0;
                    break;
            }
            
            if (selectedCard)
            {
                
                var itemCards = MenuControl.Instance.adventureMenu.GetItemCards();
                var extras = MenuControl.Instance.adventureMenu.GetItemExtraData();
                var index = itemCards.IndexOf(selectedCard);
                if (index != -1)
                {
                    if (extras[index] == 1)
                    {
                        if (selectedCard.isPotion)
                        {
                            res = 0;
                        }
                        else
                        {
                            res /= 2;
                        }
                    }
                }
                else
                {
                    Debug.LogError("Card not found in item cards");
                }

            }
        }
        else if (shopMode == ShopMode.Removal)
        {
            res = currentRemoveCardCost;
        }
        else
        {
            Debug.LogError("ShopMode is not valid");
        }

        //移除了新的strong强壮的减费能力
        // if (shopMode == ShopMode.Upgrade)
        // {
        //     if (MenuControl.Instance.levelUpMenu.variableTalentsAcquired.Contains(MenuControl.Instance.levelUpMenu
        //             .lessUpgradeCostTalent))
        //     {
        //         res--;
        //     }
        // }

        return res;
    }

    public void ConfirmAction()
    {
        confirmButton.interactable = false;
        MenuControl.Instance.infoMenu.HideMenu();
        if (shopMode != ShopMode.FreeUpgrade && NextCost(shopMode) > 0 && !isSellingTreasure)
        {
            MenuControl.Instance.heroMenu.consumeFlareStone(flarestoneCost());
            AfterPayment();
            //MenuControl.Instance.cardChoiceMenu.ShowPayTreasure(() => { AfterPayment(); });
        }
        else
        {
            AfterPayment();
        }
    }

    public void AfterPayment()
    {
        MenuControl.Instance.adventureMenu.updateHeroInfo();
        Doozy.Engine.Soundy.SoundyManager.Play(MenuControl.Instance.buffActionSound);

        if (isSellingTreasure)
        {
            MenuControl.Instance.heroMenu.RemoveCardFromDeck(selectedCard);
            MenuControl.Instance.heroMenu.addFlareStone(MenuControl.Instance.heroMenu.flareStoneWhenSell);

            MenuControl.Instance.dataControl.SaveData();

            MenuControl.Instance.adventureMenu.RenderScreen(true);
            if (MenuControl.Instance.heroMenu.GetTreasureCardsOwned().Count == 0)
            {
                ShowMenu();
            }
            else
            {
                ShowSellTreasure();
            }

            return;
        }
        else
        {
            if (shopMode == ShopMode.Purchase || shopMode == ShopMode.PurchaseReaper)
            {
                if (selectedCard != flareStoneTemplate)
                {
                    cards.Remove(selectedCard);
                    MenuControl.Instance.adventureMenu.RemoveItemCard(selectedCard);
                    MenuControl.Instance.heroMenu.AddCardToDeck(selectedCard);
                }
                else
                {
                    MenuControl.Instance.heroMenu.addFlareStone(3);
                }

                if (NextCost(shopMode) == 0)
                {
                    purchases += 1;
                }

                List<VisibleCard> vcsToAnimate = new List<VisibleCard>();
                vcsToAnimate.Add(confirmationCardHolder.GetChild(confirmationCardHolder.childCount - 1)
                    .GetComponent<VisibleCard>());
                MenuControl.Instance.adventureMenu.AnimateVCsToDeck(vcsToAnimate);
            }
            else if (shopMode == ShopMode.Removal)
            {
                currentRemoveCardCost += removeCardCostIncrease;
                currentRemoveCardCost = Math.Min(currentRemoveCardCost, removeCardMaxCost);
                MenuControl.Instance.heroMenu.RemoveCardFromDeck(selectedCard);

                if (NextCost(shopMode) == 0)
                {
                    removals += 1;
                }
            }
            else if (shopMode == ShopMode.FreeRemoval)
            {
                MenuControl.Instance.heroMenu.RemoveCardFromDeck(selectedCard);

                if (afterFreeUpgradeAction != null)
                {
                    afterFreeUpgradeAction();
                }

                MenuControl.Instance.dataControl.SaveData();
                CloseMenu();
                return;
            }
            else if (shopMode == ShopMode.Upgrade)
            {
                List<VisibleCard> vcsToAnimate = new List<VisibleCard>();
                if (upgradeSelectCardView.SelectedCard)
                {
                    var upgradedCard = MenuControl.Instance.heroMenu.UpgradeToCardInDeck(selectedCard,
                        upgradeSelectCardView.SelectedCard.vc.card);
                    
                    vcsToAnimate.Add(upgradeSelectCardView.SelectedCard.vc);
                    moveToCard = upgradedCard;
                }
                else
                {
                   var upgradedCard =  MenuControl.Instance.heroMenu.UpgradeToRandomCardInDeck(selectedCard);
                    vcsToAnimate.Add(confirmationCardHolder.GetChild(confirmationCardHolder.childCount - 1)
                        .GetComponent<VisibleCard>());
                    moveToCard = upgradedCard;
                }

                if (NextCost(shopMode) == 0)
                {
                    upgrades += 1;
                }

                MenuControl.Instance.adventureMenu.AnimateVCsToDeck(vcsToAnimate);
                upgradeSelectCardView.ClearSelectedCard();

            }
            else if (shopMode == ShopMode.FreeUpgrade)
            {
                if (upgradeSelectCardView.SelectedCard)
                {
                    MenuControl.Instance.heroMenu.UpgradeToCardInDeck(selectedCard,
                        upgradeSelectCardView.SelectedCard.vc.card);
                }
                else
                {
                    MenuControl.Instance.heroMenu.UpgradeToRandomCardInDeck(selectedCard);
                }

                List<VisibleCard> vcsToAnimate = new List<VisibleCard>();
                if (upgradeSelectCardView.SelectedCard)
                {
                    MenuControl.Instance.heroMenu.UpgradeToCardInDeck(selectedCard,
                        upgradeSelectCardView.SelectedCard.vc.card);
                    
                    vcsToAnimate.Add(upgradeSelectCardView.SelectedCard.vc);
                }
                else
                {
                    vcsToAnimate.Add(confirmationCardHolder.GetChild(confirmationCardHolder.childCount - 1)
                        .GetComponent<VisibleCard>());
                }

                MenuControl.Instance.adventureMenu.AnimateVCsToDeck(vcsToAnimate);
                upgradeSelectCardView.ClearSelectedCard();
                if (afterFreeUpgradeAction != null)
                {
                    afterFreeUpgradeAction();
                }

                MenuControl.Instance.dataControl.SaveData();
                CloseMenu();
                return;
            }
            else if (shopMode == ShopMode.Treasure)
            {
                cards.Remove(selectedCard);
                MenuControl.Instance.adventureMenu.RemoveItemCard(selectedCard);
                MenuControl.Instance.heroMenu.AddCardToDeck(selectedCard);

                if (cards.Count == 0)
                {
                    MenuControl.Instance.adventureMenu.RemoveItem();
                    CloseMenu();
                    return;
                }
            }
        }

        MenuControl.Instance.dataControl.SaveData();

        MenuControl.Instance.adventureMenu.RenderScreen(true);
        ShowMenu();
        MenuControl.Instance.adventureMenu.updateHeroInfo();
    }
}