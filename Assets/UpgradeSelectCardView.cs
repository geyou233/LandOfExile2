using System;
using System.Collections;
using System.Collections.Generic;
using SuperScrollView;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UpgradeSelectCardView : BasicMenu
{
    public CardWithDetails originalCard;
    public Transform upgradeCardsParent;
    private CardWithDetails[] upgradeCards;
    
    public TMP_Text costText;
    public Button cancelButton;
    public Button confirmButton;
[HideInInspector]
    public CardWithDetails SelectedCard;
    public string colorString = "<color=#6DB2A9>";

    
    public LoopListView2 mLoopListView;
    
    //for loop list
   public float adjustValue = 5;
   public  float adjustValue2 = 1;
   public float adjustValue3 = 0.3f;
   
    
    public void SelectCard(CardWithDetails c)
    {
        if (!c.GetComponent<Button>().enabled)
        {
            return;
        }
        if (SelectedCard)
        {
            SelectedCard.Deselect();
        }

        if (SelectedCard != c)
        {
            c.Select();
            confirmButton.interactable = MenuControl.Instance.shopMenu.hasEnoughFlareStone();
            SelectedCard = c;
        }
        else
        {
            //点两次取消选择
        confirmButton.interactable = false;
        ClearSelectedCard();
        }
    }

    public void ClearSelectedCard()
    {
         SelectedCard = null;
    }
    
    private void Awake()
    {
        upgradeCards = upgradeCardsParent.GetComponentsInChildren<CardWithDetails>(true);
        cancelButton.onClick.AddListener(() =>
        {
                HideMenu();
        });
        confirmButton.onClick.AddListener(() =>
        {
            if (SelectedCard)
            {
                HideMenu();
                MenuControl.Instance.shopMenu.ConfirmAction();
                extraAction?.Invoke();
            }
        });
        mLoopListView.InitListView(6, OnGetItemByIndex);
    }

    private Action extraAction;
    public void ShowMenuOutOfShop(Card card, Action extraAction)
    {
        MenuControl.Instance.shopMenu.selectedCard = card;
        MenuControl.Instance.shopMenu.shopMode = ShopMode.FreeUpgrade;
        this.extraAction = extraAction;
        costText.text = MenuControl.Instance.GetLocalizedString("Free");
        ShowMenu(card);
    }

    public void ShowMenuFromShop(Card card)
    {
        this.extraAction = null;
        ShowMenu(card);
    }

    private Card card;
    void ShowMenu(Card card)
    {
        base.ShowMenu();
        this.card = card;
        
        GetComponentInChildren<ScrollRect>().horizontalNormalizedPosition = 0f;
        ClearSelectedCard();
        confirmButton.interactable = false;
        string originalBasicInfoStr = "";
        int originalAttack = 0;
        int originalHP = 0;
        if (card is NewWeapon weapon)
        {
            originalAttack = weapon.initialPower;
            originalHP = weapon.initialDuality;
            
            originalBasicInfoStr = $"{MenuControl.Instance.GetLocalizedString("Power")} {originalAttack}    {MenuControl.Instance.GetLocalizedString("Duality")} {originalHP}";
        }else if (card is Unit unit)
        {
            originalAttack = unit.initialPower;
            originalHP = unit.initialHP;
            originalBasicInfoStr = $"{MenuControl.Instance.GetLocalizedString("Power")} {originalAttack}    {MenuControl.Instance.GetLocalizedString("HP")} {originalHP}";
        }
        originalCard.Render(card,this,originalBasicInfoStr);
        int i = 0;

        foreach (var upgradeCard in card.upgradeCards)
        {
            if (i >= upgradeCards.Length)
            {
                Debug.LogError(("upgradeCards.Length < card.upgradeCards.Length"));
                break;
            }
            
            int upgradeAttack = 0;
            int upgradeHP = 0;
            string basicInfoStr = "";
            if (upgradeCard is NewWeapon weapon2)
            {
                upgradeAttack = weapon2.initialPower;
                upgradeHP = weapon2.initialDuality;
                string attackStr = ((upgradeAttack!=originalAttack)?colorString:"") + upgradeAttack.ToString()+ ((upgradeAttack!=originalAttack)?"</Color>":"");
                string hpStr = ((upgradeHP!=originalHP)?colorString:"") + upgradeHP.ToString()+ ((upgradeHP!=originalHP)?"</Color>":"");
                
                basicInfoStr = $"{MenuControl.Instance.GetLocalizedString("Power")} {attackStr}    {MenuControl.Instance.GetLocalizedString("Duality")} {hpStr}";
                
            }else if (upgradeCard is Unit unit2)
            {
                upgradeAttack = unit2.initialPower;
                upgradeHP = unit2.initialHP;
                
                string attackStr = ((upgradeAttack!=originalAttack)?colorString:"") + upgradeAttack.ToString()+ ((upgradeAttack!=originalAttack)?"</Color>":"");
                string hpStr = ((upgradeHP!=originalHP)?colorString:"") + upgradeHP.ToString()+ ((upgradeHP!=originalHP)?"</Color>":"");
                basicInfoStr = $"{MenuControl.Instance.GetLocalizedString("Power")} {attackStr}    {MenuControl.Instance.GetLocalizedString("HP")} {hpStr}";
            }
            
            upgradeCards[i].Render(upgradeCard,this,basicInfoStr);
            upgradeCards[i].gameObject.SetActive(true);
            i++;
        }

        for (; i < upgradeCards.Length; i++)
        {
            upgradeCards[i].gameObject.SetActive(false);
        }
        
        
        mLoopListView.SetListItemCount(card.upgradeCards.Count, false);

        mLoopListView.MovePanelToItemIndex(0, 0);
        mLoopListView.RefreshAllShownItem();

        // var VisibleDetailedCard
        //
        // VisibleCard vc1 = Instantiate(MenuControl.Instance.visibleCardPrefab, confirmationCardHolder);
        // vc1.RenderCardForMenu(vc.card);
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
        SelectCard(vc.GetComponentInParent<CardWithDetails>());
    }
    
    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0)
        {
            return null;
        }

        if (card == null)
        {
            return null;
        }

        LoopListViewItem2 item = listView.NewListViewItem("CardWithDetails");
        CardWithDetails itemScript = item.GetComponent<CardWithDetails>();

        //this block can move out
            string originalBasicInfoStr = "";
            int originalAttack = 0;
            int originalHP = 0;
            if (card is NewWeapon weapon)
            {
                originalAttack = weapon.initialPower;
                originalHP = weapon.initialDuality;

                originalBasicInfoStr =
                    $"{MenuControl.Instance.GetLocalizedString("Power")} {originalAttack}    {MenuControl.Instance.GetLocalizedString("Duality")} {originalHP}";
            }
            else if (card is Unit unit)
            {
                originalAttack = unit.initialPower;
                originalHP = unit.initialHP;
                originalBasicInfoStr =
                    $"{MenuControl.Instance.GetLocalizedString("Power")} {originalAttack}    {MenuControl.Instance.GetLocalizedString("HP")} {originalHP}";
            }

            originalCard.Render(card, this, originalBasicInfoStr);
        //end block


        if (index >= upgradeCards.Length)
        {
            Debug.LogError(("upgradeCards.Length < card.upgradeCards.Length"));
            return item;
        }

        var upgradeCard = card.upgradeCards[index];
        int upgradeAttack = 0;
        int upgradeHP = 0;
        string basicInfoStr = "";
        if (upgradeCard is NewWeapon weapon2)
        {
            upgradeAttack = weapon2.initialPower;
            upgradeHP = weapon2.initialDuality;
            string attackStr = ((upgradeAttack!=originalAttack)?colorString:"") + upgradeAttack.ToString()+ ((upgradeAttack!=originalAttack)?"</Color>":"");
            string hpStr = ((upgradeHP!=originalHP)?colorString:"") + upgradeHP.ToString()+ ((upgradeHP!=originalHP)?"</Color>":"");
                
            basicInfoStr = $"{MenuControl.Instance.GetLocalizedString("Power")} {attackStr}    {MenuControl.Instance.GetLocalizedString("Duality")} {hpStr}";
                
        }else if (upgradeCard is Unit unit2)
        {
            upgradeAttack = unit2.initialPower;
            upgradeHP = unit2.initialHP;
                
            string attackStr = ((upgradeAttack!=originalAttack)?colorString:"") + upgradeAttack.ToString()+ ((upgradeAttack!=originalAttack)?"</Color>":"");
            string hpStr = ((upgradeHP!=originalHP)?colorString:"") + upgradeHP.ToString()+ ((upgradeHP!=originalHP)?"</Color>":"");
            basicInfoStr = $"{MenuControl.Instance.GetLocalizedString("Power")} {attackStr}    {MenuControl.Instance.GetLocalizedString("HP")} {hpStr}";
        }
            
        itemScript.Render(upgradeCard,this,basicInfoStr);

        

        return item;
    }
    
    
    void LateUpdate()
    {
        mLoopListView.UpdateAllShownItemSnapData();
        int count = mLoopListView.ShownItemCount;
        for (int i = 0; i < count; ++i)
        {
            LoopListViewItem2 item = mLoopListView.GetShownItemByIndex(i);
            CardWithDetails itemScript = item.GetComponent<CardWithDetails>();
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
                itemScript.transform.localScale = new Vector3(scale, scale, 1);
            }
        }
    }

    public override void HideMenu(bool instantly = false)
    {
        base.HideMenu(instantly);
        //ClearSelectedCard();
    }
}
