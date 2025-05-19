using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Doozy.Engine.UI;
using I2.Loc;
using SuperScrollView;
using UnityEngine.Serialization;

public class LibraryMenu : BasicMenu
{
    public List<Card> cardsToShow = new List<Card>();
    public Transform cardGrid;
    public Transform heroClassHolder;
    public Transform pathHolder;
    public Transform cardTypeHolder;

    public Transform pathParent;
    public Transform classParent;
    [FormerlySerializedAs("cardTypeOptions")] public Dropdown schoolTypeOptions;
    public Text selectedCardType;
    public Color textSelectColor;
    public Color textDeselectColor;
    public bool showLocked = false;
    public LoopGridView mLoopGridView;

    public void discoverAllUnlockedHeros()
    {
        foreach (var heroClass in MenuControl.Instance.heroMenu. heroClasses)
        {
            if (MenuControl.Instance.heroMenu.isHeroUnlocked(
                    MenuControl.Instance.heroMenu.heroClasses.IndexOf(heroClass)))
            {
                
                foreach (var card in heroClass.startCards)
                {
                    MenuControl.Instance.progressMenu.discoverCard(card);
                }
            }
        }
    }

    private void Awake()
    {
        mLoopGridView.InitGridView(10, OnGetItemByRowColumn);
    }

    private void Start()
    {

        discoverAllUnlockedHeros();
        // foreach (var heroClass in MenuControl.Instance.heroMenu.heroPaths)
        // {
        //     foreach (var card in heroClass.startingCards)
        //     {
        //         MenuControl.Instance.progressMenu.discoverCard(card);
        //     }
        // }
    }

    private void filterCardsToShow()
    {
        var newAllCards = new List<Card>();
        var csvLoader = MenuControl.Instance.csvLoader;
        HashSet<string> visitedUniqueName = new HashSet<string>();
        foreach (var card in cardsToShow)
        {
            if (card is Skill)
            {
                continue;
            }

            if (!csvLoader.isValidInCurrentVersion(card.UniqueID))
            {
                continue;
            }

            if (visitedUniqueName.Contains(card.UniqueID))
            {
                continue;
            }

            visitedUniqueName.Add(card.UniqueID);
            var chineseName = card.GetChineseName();
            chineseName = csvLoader.downgradeChineseName(chineseName);
            if(csvLoader.isValidInCurrentVersion(card.UniqueID) || 
               csvLoader.chineseNameToTalentMap.ContainsKey(chineseName))
            {newAllCards.Add(card);}
        }

        cardsToShow =  newAllCards;
    }

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseMenu();
            }
        }
    }

    public void ShowDeck(List<Card> cardsToBeShown)
    {
        cardsToShow.Clear();
        cardsToShow.AddRange(cardsToBeShown);
        ShowMenu();
    }

    public void updateClassAndPathButtons()
    {
        LibraryClassCell[] classButtons = GetComponentsInChildren<LibraryClassCell>();
        foreach (var button in classButtons)
        {
            button.highlightImage.SetActive(false);
        }

        //if (_classIndex >= 0)
        {
            classButtons[_classIndex+1].highlightImage.SetActive(true);
        }
        
        LibraryCardTypeCell[] libraryCardTypeCells = GetComponentsInChildren<LibraryCardTypeCell>();
        foreach (var button in libraryCardTypeCells)
        {
            button.deselect();
        }
        libraryCardTypeCells[_typeIndex].select();
        schoolTypeOptions.gameObject.SetActive(false);
        // if (_classIndex == -1)
        // {
        //     schoolTypeOptions.gameObject.SetActive(false);
        //     schoolTypeOptions.value = 0;
        // }
        // else
        // {
        //     schoolTypeOptions.gameObject.SetActive(true);
        // }
        // //UIButton[] pathButtons = pathParent.GetComponentsInChildren<UIButton>();
        // foreach (var button in classButtons)
        // {
        //     button.GetComponent<Image>().enabled = false;
        // }
        // // foreach (var button in pathButtons)
        // // {
        // //     button.GetComponentInChildren<Text>().color = textDeselectColor;
        // // }
        //
        // // if (_classIndex >= 0)
        // // {
        // //     
        // //     classButtons[_classIndex].GetComponent<Image>().enabled = true;
        // // }
        //
        // if (_classIndex >= 0)
        // {
        //     classButtons[_classIndex].GetComponentInChildren<Text>().color = textSelectColor;
        // }
    }

    private int _classIndex = -1;
    //private int _pathIndex = -1;
    // public void ShowClass(int classIndex)
    // {
    //     if (_classIndex == classIndex)
    //     {
    //         _classIndex = -1;
    //     }
    //     else
    //     {
    //         _classIndex = classIndex;
    //         _pathIndex = -1;
    //     }
    //     //cardTypeOptions.value = 0;
    //     //selectedCardType.text =MenuControl.Instance.GetLocalizedString( cardTypeOptions.options[cardTypeOptions.value].text);
    //     //updateClassAndPathButtons(true,classIndex);
    //     
    //     // cardsToShow.Clear();
    //     // cardsToShow.AddRange(MenuControl.Instance.heroMenu.heroClasses[classIndex].classCards);
    //     //
    //     // foreach (Card card in MenuControl.Instance.heroMenu.heroClasses[classIndex].basicTalents)
    //     // {
    //     //    
    //     //         if (!cardsToShow.Contains(card))
    //     //             cardsToShow.Add(card);
    //     //     
    //     // }
    //     // foreach (Card card in MenuControl.Instance.heroMenu.heroClasses[classIndex].advancedTalents)
    //     // {
    //     //     
    //     //         if (!cardsToShow.Contains(card))
    //     //             cardsToShow.Add(card);
    //     //    
    //     // }
    //     // foreach (Card card in MenuControl.Instance.heroMenu.heroClasses[classIndex].midpointTalents)
    //     // {
    //     //     
    //     //         if (!cardsToShow.Contains(card))
    //     //             cardsToShow.Add(card);
    //     //     
    //     // }
    //     //
    //     // ShowMenu(0,classIndex);
    //     ShowMenu();
    // }

    public void ShowPath(int pathIndex)
    {
        
        if (_classIndex == -1 && pathIndex != -1)
        {
            schoolTypeOptions.options.RemoveRange(1, schoolTypeOptions.options.Count - 1);
            //schoolTypeOptions.options.Add(new Dropdown.OptionData("SchoolAllName"));
            foreach (var school in MenuControl.Instance.csvLoader.classTypeToSchool[_classIndex])
            {
                
                schoolTypeOptions.options.Add(new Dropdown.OptionData(school+"Name"));
            }
        }

        // if (_classIndex == pathIndex)
        // {
        //     _classIndex = -1;
        // }
        // else
        {
            _classIndex = pathIndex;
            
               // _classIndex= -1;
        }
        //cardTypeOptions.value = 0;
        //selectedCardType.text =MenuControl.Instance.GetLocalizedString( cardTypeOptions.options[cardTypeOptions.value].text);
        //updateClassAndPathButtons(false,pathIndex);
        // cardsToShow.Clear();
        //
        // cardsToShow.AddRange(MenuControl.Instance.heroMenu.heroPaths[pathIndex].pathCards);
        //
        // ShowMenu(1,pathIndex);
        ShowMenu();
    }
    // public void SelectCardType()
    // {
    //     if (gameObject.activeInHierarchy)
    //     {
    //         ShowCardType(cardTypeOptions.value);
    //
    //         selectedCardType.text =MenuControl.Instance.GetLocalizedString( cardTypeOptions.options[cardTypeOptions.value].text);
    //     }
    // }

    public int _typeIndex = 0;
    
    public void ShowCardType(int index)
    {
        _typeIndex = index;
        ShowMenu();
    }
    
    
    
    LoopGridViewItem OnGetItemByRowColumn(LoopGridView gridView, int index,int row,int column)
    {
        if (cardsToShowInOrder == null ||index < 0 || index >= cardsToShowInOrder.Count)
        {
            return null;
        }
        //get the data to showing
        Card itemData = cardsToShowInOrder[index];
        if (itemData == null)
        {
            return null;
        }
        /*get a new item. Every item can use a different prefab,
        the parameter of the NewListViewItem is the prefab’name.
        And all the prefabs should be listed in ItemPrefabList in LoopGridView Inspector Setting  */
        LoopGridViewItem item = gridView.NewListViewItem("VisualCardPrefab");
        //get your own component
        VisibleCard itemScript = item.GetComponent<VisibleCard>();
        // IsInitHandlerCalled is false means this item is new created but not fetched from pool.
        // if (item.IsInitHandlerCalled == false)
        // {
        //     item.IsInitHandlerCalled = true;
        //     itemScript.Init();// here to init the item, such as add button click event listener.
        // }
        //update the item’s content for showing, such as image,text.
        renderOneCard(itemScript, itemData);
        return item;
    }
    // void renderOneCard(Card card)
    // {
    //     mLoopGridView
    //     VisibleCard vc = Instantiate(MenuControl.Instance.visibleCardPrefab, cardGrid);
    //                 
    //     renderOneCard(vc, card);
    // }

    void renderOneCard(VisibleCard vc, Card card)
    {
        vc.RenderCardForMenu(card);
        vc.transform.localScale = Vector3.one * 0.65f;
                    
        if (!card.IsDiscovered() /*&& !MenuControl.Instance.testMode*/)
        {
            vc.GetComponent<CanvasGroup>().alpha = 0.5f;
            vc.RenderCardBackOnly(null);
        }
        else
        {
            vc.GetComponent<CanvasGroup>().alpha = 1f;
        }
    }

    private List<Card> cardsToShowInOrder;
    public override void ShowMenu()
    {
        base.ShowMenu();

        updateClassAndPathButtons();
        //selectedCardType.text =MenuControl.Instance.GetLocalizedString( schoolTypeOptions.options[schoolTypeOptions.value].text);
        cardsToShow.Clear();

        var newCardsToShow = new List<Card>();
        if (_classIndex >= 0)
        {
                newCardsToShow.AddRange(MenuControl.Instance.heroMenu.separatedCards["All"][_classIndex+1]);
                newCardsToShow.AddRange(MenuControl.Instance.heroMenu.separatedCards["All"][0]);
                newCardsToShow.AddRange(MenuControl.Instance.heroMenu.
                    separatedCards["Path"][0]);
                newCardsToShow.AddRange(MenuControl.Instance.heroMenu.
                    separatedCards["Path"][_classIndex+1]);
                
        }
        else
        {
            
            newCardsToShow.AddRange(MenuControl.Instance.heroMenu.allCards);
        }
        
        if (_typeIndex == 0) // All
        {
            cardsToShow.AddRange(newCardsToShow);
        }
        else if (_typeIndex == 1) //Minions
        {
            foreach (Card card in newCardsToShow)
            {
                if (card is Minion && !cardsToShow.Contains(card))
                    cardsToShow.Add(card);
            }
        }
        else if (_typeIndex == 2) //Spells
        {
            foreach (Card card in newCardsToShow)
            {
                if (card is Castable && !(card is Artifact) && !cardsToShow.Contains(card) && card.cardTags.Contains(MenuControl.Instance.spellTag))
                    cardsToShow.Add(card);
            }
        }
        else if (_typeIndex == 3) //Items
        {
            foreach (Card card in newCardsToShow)
            {
                if (card.isItem)
                    cardsToShow.Add(card);
            }
        }
        else if (_typeIndex == 4) //Treasures
        {
            // foreach (Card card in MenuControl.Instance.heroMenu.GetAllTreasures())
            // {
            //     if (!cardsToShow.Contains(card))
            //         cardsToShow.Add(card);
            // }
            foreach (Card card in newCardsToShow)
            {
                if (card.IsTreasure())
                    cardsToShow.Add(card);
            }
        }
        // else if (_typeIndex == 5) //Artifacts
        // {
        //     foreach (Card card in MenuControl.Instance.heroMenu.allCards)
        //     {
        //         if (!cardsToShow.Contains(card) && card is Artifact && card.cardTags.Contains(MenuControl.Instance.artifactTag))
        //             cardsToShow.Add(card);
        //     }
        // }
        // else if (index == 6) //Loot
        // {
        //     foreach (Card card in MenuControl.Instance.heroMenu.allCards)
        //     {
        //         if (!cardsToShow.Contains(card) && card.cardTags.Contains(MenuControl.Instance.lootTag))
        //             cardsToShow.Add(card);
        //     }
        // }
        else if (_typeIndex == 5) //Weapons
        {
            foreach (Card card in newCardsToShow)
            {
                if (card is NewWeapon)
                    cardsToShow.Add(card);
            }
        }
        
        //filter class
        if (_classIndex >= 0)
        {
             // var newCardsToShow = new List<Card>();
             // newCardsToShow.AddRange(MenuControl.Instance.heroMenu.separatedCards["All"][_classIndex+1]);
             // newCardsToShow.AddRange(MenuControl.Instance.heroMenu.separatedCards["All"][0]);
            // newCardsToShow.AddRange(MenuControl.Instance.heroMenu.heroClasses[_classIndex].classCards);
            //
            // foreach (Card card in MenuControl.Instance.heroMenu.heroClasses[_classIndex].basicTalents)
            // {
            //    
            //         if (!newCardsToShow.Contains(card))
            //             newCardsToShow.Add(card);
            //     
            // }
            // foreach (Card card in MenuControl.Instance.heroMenu.heroClasses[_classIndex].advancedTalents)
            // {
            //     
            //         if (!newCardsToShow.Contains(card))
            //             newCardsToShow.Add(card);
            //    
            // }
            // foreach (Card card in MenuControl.Instance.heroMenu.heroClasses[_classIndex].midpointTalents)
            // {
            //     
            //         if (!newCardsToShow.Contains(card))
            //             newCardsToShow.Add(card);
            //     
            // }
            // newCardsToShow.AddRange(MenuControl.Instance.heroMenu.heroPaths[_classIndex].pathCards);
            // cardsToShow = cardsToShow.Intersect(newCardsToShow).ToList();
        // }
        //
        // if (_pathIndex >= 0)
        // {
           // cardsToShow = cardsToShow.Intersect(MenuControl.Instance.heroMenu.heroPaths[_pathIndex].pathCards).ToList();
        }
        
        //filter school
        if (_classIndex >= 0 && schoolTypeOptions.value > 0)
        {
            var text = schoolTypeOptions.options[schoolTypeOptions.value].text;
            text = text.Substring(0, text.Length - 4);
            List<Card> tempCardsToShow = new List<Card>();
            foreach (var card in cardsToShow)
            {
                var schools = MenuControl.Instance.csvLoader.uniqueIdToPlayerCardInfo[card.UniqueID].school;
                if (schools!=null && schools.Contains(text))
                {
                    tempCardsToShow.Add(card);
                }
            }

            cardsToShow = tempCardsToShow;
        }
        
        filterCardsToShow();

        // foreach (Transform child in cardGrid)
        // {
        //     Destroy(child.gameObject);
        // }

        // //filter out locked cards
        // var newCardsToShow = new List<Card>();
        // foreach (var card in cardsToShow)
        // {
        //     //if (!MenuControl.Instance.heroMenu.isLocked(card))
        //     {
        //         newCardsToShow.Add(card);
        //     }
        // }

        //cardsToShow = newCardsToShow;
        cardsToShowInOrder = new List<Card>();

        int CardTypeOrdered(Card card)
        {
            if (card is Minion)
            {
                return 100;
            }

            if (card is NewWeapon)
            {
                return 200;
            }

            if (card.isPotion)
            {
                return 500;
            }
            if (card is Artifact)
            {
                return 600;
            }

            if (card.IsTreasure())
            {
                return 400;
            }

            return 300;//spell
        }
        
        cardsToShowInOrder.AddRange(cardsToShow.OrderBy(x =>CardTypeOrdered(x)+  x.GetIDForOrderedList() ));
        Debug.Log($"library has cards {cardsToShowInOrder.Count}");
        int yy = 0;
        int zz = 0;
        HashSet<string> cardIDs = new HashSet<string>();
        
        mLoopGridView.SetListItemCount(cardsToShowInOrder.Count, false);
        mLoopGridView.RefreshAllShownItem();               
        //change to enhanced scroller
        // int xx = 0;
        // for (; xx < cardsToShowInOrder.Count; xx++)
        // {
        //     if (cardGrid.childCount > xx)
        //     {
        //         cardGrid.GetChild(xx).gameObject.SetActive(true);
        //         renderOneCard(cardGrid.GetChild(xx).GetComponent<VisibleCard>(), cardsToShowInOrder[xx]);
        //         //cardGrid.GetChild(xx).GetComponent<VisibleCard>(). RenderCard((cardsToShowInOrder[xx]));
        //     }
        //     else
        //     {
        //         
        //         renderOneCard(cardsToShowInOrder[xx]);
        //     }
        //     
        // }

        // for (; xx < cardGrid.childCount; xx++)
        // {
        //     cardGrid.GetChild(xx).gameObject.SetActive(false);
        // }

        // for (int xx = 0; xx < cardsToShowInOrder.Count; xx++)
        // {
        //     if (yy >= cardsToShowInOrder.Count)
        //     {
        //         break;
        //     }
        //     LeanTween.delayedCall(0.07f * zz, () =>
        //     {
        //         for (int ii = 0; ii < 20; ii++)
        //         {
        //             if (yy >= cardsToShowInOrder.Count)
        //             {
        //                 break;
        //             }
        //             var card = cardsToShowInOrder[yy];
        //             if (cardIDs.Contains(card.UniqueID))
        //             {
        //                 continue;
        //             }
        //
        //             renderOneCard(card);
        //             yy++;
        //             cardIDs.Add(card.UniqueID);
        //             int test = 0;
        //             //ii++;
        //             while (card.upgradeCard != null)
        //             {
        //                 card = card.upgradeCard;
        //                 renderOneCard(card);
        //                 yy++;
        //                 cardIDs.Add(card.UniqueID);
        //                 test++;
        //                 ii++;
        //                 if (test > 10)
        //                 {
        //                     Debug.LogError(("infinite loop"));
        //                     break;
        //                 }
        //             }
        //         }
        //     });
        //
        //     zz++;
        // }
        // for (int xx = 0; xx < Mathf.CeilToInt(cardsToShow.Count / 20f); xx += 1)
        // {
        //     int yy = xx;
        //     LeanTween.delayedCall(0.07f * yy, () =>
        //     {
        //         for (int ii = 0; ii < 20; ii += 1)
        //         {
        //             if (cardsToShowInOrder.Count > ii + (20 * yy))
        //             {
        //                 Card card = cardsToShowInOrder[ii + (20 * yy)];
        //                 VisibleCard vc = Instantiate(MenuControl.Instance.visibleCardPrefab, cardGrid);
        //
        //                 vc.RenderCardForMenu(card);
        //                 vc.transform.localScale = Vector3.one * 0.65f;
        //
        //                 if (!card.IsDiscovered() /*&& !MenuControl.Instance.testMode*/)
        //                 {
        //                     vc.GetComponent<CanvasGroup>().alpha = 0.5f;
        //                     vc.RenderCardBackOnly(null);
        //                 }
        //             }
        //         }
        //     });
        //
        // }

        cardGrid.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        MenuControl.Instance.LogEvent("OpenLibrary");
        
        selectedCardType.GetComponent<Localize>().Term = schoolTypeOptions.options[schoolTypeOptions.value].text;
    }

    public override void SelectVisibleCard(VisibleCard vc, bool withClick = true)
    {
        base.SelectVisibleCard(vc, withClick);
        if (GetComponent<Doozy.Engine.UI.UIView>().IsHiding) return;
        if ((vc.card != null && vc.card.IsDiscovered())/* || MenuControl.Instance.testMode*/)
            MenuControl.Instance.infoMenu.ShowInfo(vc);
    }

    public override void DeSelectVisibleCard(VisibleCard vc, bool withClick = true)
    {
        base.DeSelectVisibleCard(vc, withClick);
        MenuControl.Instance.infoMenu.HideMenu();
    }
}
