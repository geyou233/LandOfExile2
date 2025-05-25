using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class VisibleCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
{
    public GameObject boardCardObj;
    public Card card;
    public bool isMenuCard;
    public bool disableInteraction;

    public GameObject enemyIntentCardOb;
    public Image enemyIntentCardImage;
    [HideInInspector] public bool isEnemyIntentCard;

    public Image ghostCardBGImage;
    public GameObject ConsumableOb;
    public Image normalBGImage;
    public Image normalBarImage;

    public GameObject dialoguePanel;
    public  Text dialogueText;
    //public Image boardBGImage;

    public Image artifactBK;

    public GameObject boardBGPlayerHero;
    public GameObject boardBGPlayerUnit;
    public GameObject boardBGEnemyBoss;
    public GameObject boardBGEnemyMinion;
    public GameObject boardBGUnderPlayerHero;
    public GameObject boardBGUnderPlayerUnit;
    public GameObject boardBGUnderEnemyBoss;
    public GameObject boardBGUnderEnemyMinion;
    public GameObject inactiveBoardPlayerHero;
    public GameObject inactiveBoardPlayerUnit;
    public GameObject inactiveBoardEnemyBoss;
    public GameObject inactiveBoardEnemyMinion;


    public Image boardMaskImage;
    public Image cardImageHand;
    public Image cardImageBoard;
    public Text powerTextCard;
    public Text powerTextBoard;
    public Text powerTextIntent;
    public Text hPTextCard;
    public Text hPTextBoard;
    public Text hPTextIntent;
    public Text costText;
    public Text costTextIntent;
    public Image inactiveImage;
    public ParticleSystem inactivePS;
    public Image heroIcon;
    public Image weaponIcon;

    public Transform altInfoPanel;
    public Transform effectsPanel;

    public GameObject boardCard;
    public GameObject handCard;
    public GameObject enemyCard;
    public GameObject talentCard;

    public Image talentImageBoard;
    public List<GameObject> talentStars;

    public GameObject niceCard;

    public GameObject effectBorderPrefab;
    public List<Image> positiveEffectImages = new List<Image>();
    public List<Image> negativeEffectImages = new List<Image>();

    public GameObject highlightRedHand;
    public GameObject highlightGreenHand;
    public GameObject highlightBlueHand;
    public GameObject highlightRedBoard;
    public GameObject highlightGreenBoard;
    public GameObject highlightBlueBoard;
    public GameObject highlightCrossBoard;
    public GameObject highlightCheckBoard;
    public GameObject highlightCheckTalent;

    [HideInInspector] public GameObject currentBorderImage;
    [HideInInspector] public GameObject currentUnderBorderImage;
    [HideInInspector] public GameObject currentInactiveImage;
    public Vector3 damagePoint;

    public Transform starsPanel;

    public Sprite boardCardHeroMaskSprite;
    public Sprite boardCardMinionMaskSprite;

    public Sprite powerIconMeleeSprite;
    public Sprite powerIconRangedSprite;

    public GameObject handMultipleCardGO;
    public GameObject talentMultipleCardGO;

    [HideInInspector]
    public bool showCardView = true;
    public Color hpTextLessThanMaxColor;
    private Color hpTextOriginalColor;

    public float copiedItemTransparency = 0.9f;
     float targetTransparency = 1;

     public GameObject discountObj;
     public Text discountText;
     [HideInInspector] public bool isUpgrading;//最后一颗星星一闪一闪

    // public GameObject TauntGo;
    public void SetHandCardCount(int count)
    {
        if (count > 1)
        {
            handMultipleCardGO.SetActive(true);
            handMultipleCardGO.GetComponentInChildren<Text>().text = count.ToString();
        }
        else
        {
            handMultipleCardGO.SetActive(false);
        }
    }
    
    public void SetTalentCardCount(int count)
    {
        if (count > 1)
        {
            talentMultipleCardGO.SetActive(true);
            talentMultipleCardGO.GetComponentInChildren<Text>().text = count.ToString();
        }
        else
        {
            talentMultipleCardGO.SetActive(false);
        }
    }

    public void ShowDialogue(DialogueInBattleInfo info)
    {
        dialoguePanel.SetActive(true);
        dialogueText.GetComponent<Localize>().Term = "battleDialogue_"+info.dialogueId;
        StartCoroutine(HideDialogue());
    }
    
    IEnumerator HideDialogue()
    {
        yield return  new WaitForSeconds(MenuControl.Instance.battleMenu.GetComponent<InBattleDialogueController>().showDialogueShowTime);
        dialoguePanel.SetActive(false);
    }
    
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        //("OnPointerDown");
        if (eventData.button == PointerEventData.InputButton.Middle)
            return;
        else if (eventData.button == PointerEventData.InputButton.Right)
            return;

        if (!disableInteraction)
        {
            GetComponentInParent<BasicMenu>().SelectVisibleCard(this);
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("OnPointerUp");
        if (eventData.button == PointerEventData.InputButton.Middle)
            return;
        else if (eventData.button == PointerEventData.InputButton.Right)
            return;

        if (!disableInteraction)
        {
            GetComponentInParent<BasicMenu>().DeSelectVisibleCard(this);
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("OnPointerEnter");
        if (eventData.button == PointerEventData.InputButton.Middle)
            return;
        else if (eventData.button == PointerEventData.InputButton.Right)
            return;

#if UNITY_STANDALONE
        if (!disableInteraction)
        {
            GetComponentInParent<BasicMenu>().SelectVisibleCard(this, false);
        }
#endif
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("OnPointerExit");
        if (eventData.button == PointerEventData.InputButton.Middle)
            return;
        else if (eventData.button == PointerEventData.InputButton.Right)
            return;

#if UNITY_STANDALONE
        if (!disableInteraction)
        {
            // if (!RectTransformUtility.RectangleContainsScreenPoint(
            //         MenuControl.Instance.infoMenu.masterCardDescription.GetComponent<RectTransform>(),
            //         eventData.position,
            //         eventData.pressEventCamera)) // 如果不在 panel 上则隐藏
            // {
            //     
            //     GetComponentInParent<BasicMenu>().DeSelectVisibleCard(this, false);
            //     //GetComponentInParent<BasicMenu>().SetActive(false);
            // }
            GetComponentInParent<BasicMenu>().DeSelectVisibleCard(this, false);
        }
#endif
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("OnPointerClick");
        if (eventData.button == PointerEventData.InputButton.Middle)
            return;
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // if (card != null && !enemyCard.activeInHierarchy && (!MenuControl.Instance.battleMenu.inBattle ||
            //                                                      !(card.player == MenuControl.Instance.battleMenu
            //                                                            .playerAI &&
            //                                                        card.GetZone() == MenuControl.Instance.battleMenu
            //                                                            .hand) || MenuControl.Instance.battleMenu
            //                                                          .usingIntentSystem))
            
            //
            if (showCardView && (card.showUpgradeView()))
            {
                if (MenuControl.Instance.heroMenu.seasonsMode && niceCard.activeInHierarchy) return;
                MenuControl.Instance.battleMenu.CancelSelectVisibleCard();
                MenuControl.Instance.cardViewerMenu.ShowCard(card);
                return;
            }
        }
        if (!disableInteraction)
        {
            isSelect = true;
           
            GetComponentInParent<BasicMenu>().ClickVisibleCard(this);
#if UNITY_ANDROID || UNITY_IOS
            GetComponentInParent<BasicMenu>().SelectVisibleCard(this, false);
#endif
        }
    }

    public void DisableCard()
    {
        // SetInactive(false);
        disableInteraction = true;
        foreach (var image in handCard.GetComponentsInChildren<Image>())
        {
            image.color = Color.gray;
        }
    }

    public void EnableCard()
    {
        
        disableInteraction = false;
        foreach (var image in handCard.GetComponentsInChildren<Image>())
        {
            image.color = Color.white;
        }
    }
    //a line in inspector
    [Header("soar")]
    public float flyHeight = 10;

    public float flyScaleY = 0.9f;
    private float flyScaleOriginalY;
    public float moveDistance = 2f; // 左右移动的距离
    public float moveDuration = 1f; // 移动一次的持续时间
    public float swingAngle = 10f;  // 摇摆的角度
    public float swingDuration = 0.5f; // 摇摆一次的持续时间
    private Sequence swingSequence;
    public void DisableCardInteraction()
    {
        disableInteraction = true;
    }

    public void Fly()
    {
        transform.parent = MenuControl.Instance.battleMenu.visibleBoardCardsHolderFlying;
        boardCardObj.transform.localPosition = new Vector3(0, flyHeight, 0);
        
        // 创建左右移动的路径点
        Vector3[] path = new Vector3[]
        {
            new Vector3( - moveDistance, flyHeight, 0),
            new Vector3( + moveDistance, flyHeight, 0)
        };

        // 创建左右移动的动画
        boardCardObj.transform.DOLocalPath(path, moveDuration * 2, PathType.Linear)
            .SetEase(Ease.InOutSine) // 设置缓动类型
            .SetLoops(-1, LoopType.Yoyo); // 无限次循环

        swingSequence = DOTween.Sequence();
        swingSequence.Append(boardCardObj.transform.DOLocalRotate(new Vector3(0, 0, swingAngle), swingDuration).SetEase(Ease.InOutSine));
        swingSequence.Append(boardCardObj.transform.DOLocalRotate(Vector3.zero, swingDuration).SetEase(Ease.InOutSine));
        swingSequence.Append(boardCardObj.transform.DOLocalRotate(new Vector3(0, 0, -swingAngle), swingDuration).SetEase(Ease.InOutSine));
        swingSequence.Append(boardCardObj.transform.DOLocalRotate(Vector3.zero, swingDuration).SetEase(Ease.InOutSine));
        swingSequence.SetLoops(-1, LoopType.Restart);
        // // 创建轻微晃动的动画
        // boardCardObj.transform.DOLocalRotate(new Vector3(0, 0, swingAngle), swingDuration)
        //     .SetEase(Ease.InOutSine) // 设置缓动类型
        //     .SetLoops(-1, LoopType.Yoyo); // 无限次摇摆
    }

    public void StopFly()
    {
        transform.parent = MenuControl.Instance.battleMenu.player1.visibleBoardCardsHolder;
        
        
        boardCardObj.transform.DOKill();
        boardCardObj.transform.localPosition = new Vector3(0, 0, 0);
        boardCardObj.transform.localRotation = Quaternion.identity;
        if (swingSequence!=null)
        {
            swingSequence.Kill();
        }
    }

    public bool ignoreBattlefieldEffect = false;
    private bool isFlying = false;
    private bool isShowingDialogue = false;
    public void RenderCard(Card card, bool forceFaceDown = false)
    {
        if (card.GetZone() == MenuControl.Instance.battleMenu.board && !ignoreBattlefieldEffect)
        {
            if (isFlying && !card.isFlying)
            {
                StopFly();
            }else if (card.isFlying && !isFlying)
            {
                Fly();
            }
        }

        if (discountObj)
        {
            
            discountObj .SetActive(false);
        }
        if(!isShowingDialogue)
        isFlying = card.isFlying;

        // if (TauntGo)
        // {
        //     if (card is Unit u && u.player == MenuControl.Instance.battleMenu.playerAI && u.IsTaunt())
        //     {
        //         TauntGo.SetActive(true);
        //         // TauntGo.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        //         // LeanTween.color(TauntGo.GetComponent<RectTransform>(), new Color(1f, 1f, 1f, 0.75f), 1f).setLoopPingPong(-1).setEaseOutSine();
        //     }
        //     else
        //     {
        //     
        //         TauntGo.SetActive(false);
        //     }
        // }
        GetComponent<CanvasGroup>().alpha = targetTransparency;
        this.card = card;
        hpTextOriginalColor = hPTextCard.color;
        if (altInfoPanel != null)
        {
            altInfoPanel.gameObject.SetActive(false);
        }

        if (!ghostCardBGImage)
        {
            return;
        }
        ghostCardBGImage.gameObject.SetActive(false);


        talentCard.SetActive(false);
        handCard.SetActive(false);
        boardCard.SetActive(false);
        enemyCard.SetActive(false);

        if (isEnemyIntentCard)
        {
            enemyIntentCardOb.SetActive(true);
            enemyIntentCardImage.sprite = MenuControl.Instance.csvLoader.playerCardSprite(card);
            costTextIntent.transform.parent.gameObject.SetActive(card.GetCost() > 0);
            costTextIntent.text = card.GetCost().ToString();
        }
        else
        {
            enemyIntentCardOb.SetActive(false);

            cardImageHand.sprite = card.GetSprite();
            cardImageBoard.sprite = card.GetSprite();

            if (card is Skill)
            {
                talentCard.SetActive(true);
                talentImageBoard.sprite = MenuControl.Instance.csvLoader.talentSprite(card.UniqueID);
                foreach (var star in talentStars)
                {
                    star.SetActive(false);
                }

                for (int i = 0; i < MenuControl.Instance.csvLoader.talentStarCount(card.UniqueID); i++)
                {
                    talentStars[i].SetActive(true);
                }
                return;
            }
            // else if(card.player == null || card.player == MenuControl.Instance.battleMenu.playerAI)
            // {
            //     cardImageHand.sprite = MenuControl.Instance.csvLoader.enemyCardSprite(card.GetChineseName());
            //     cardImageBoard.sprite = MenuControl.Instance.csvLoader.enemyCardSprite(card.GetChineseName());
            // }
            else
            {
                //player card

                cardImageHand.sprite = MenuControl.Instance.csvLoader.playerCardSprite(card);
                cardImageBoard.sprite = MenuControl.Instance.csvLoader.playerCardSprite(card);
            }

            if (forceFaceDown || (!MenuControl.Instance.battleMenu.usingIntentSystem && !isMenuCard &&
                                  card.player != null && card.player == MenuControl.Instance.battleMenu.playerAI &&
                                  !(card is Hero)))
            {
                handCard.SetActive(false);
                boardCard.SetActive(false);
                enemyCard.SetActive(true);
            }
            else
            {
                handCard.SetActive(true);
                boardCard.SetActive(false);
                enemyCard.SetActive(false);

                if (card.isCopiedCard)
                {
                    GetComponent<CanvasGroup>().alpha = copiedItemTransparency;
                    targetTransparency = copiedItemTransparency;
                }

                if (card.player != null && card.player != MenuControl.Instance.battleMenu.playerAI)
                {
                    cardImageHand.sprite = MenuControl.Instance.csvLoader.playerCardSprite(card);
                    cardImageBoard.sprite = MenuControl.Instance.csvLoader.playerCardSprite(card);
                }
            }


            normalBGImage.sprite = GetCardFrontSprite();
            normalBarImage.sprite = GetCardFrontBarSprite();
            normalBGImage.gameObject.SetActive(true);
            SetInactive(false, false);
            //heroIcon.transform.gameObject.SetActive(false);

            costText.transform.parent.gameObject.SetActive(card.GetCost() > 0);
            costText.text = card.GetCost().ToString();
            costTextIntent.transform.parent.gameObject.SetActive(card.GetCost() > 0);
            costTextIntent.text = card.GetCost().ToString();
        }

        if (artifactBK)
        {
            if (card.IsTreasure())
            {
                artifactBK.gameObject.SetActive(true);
            }
            else
            {
                artifactBK.gameObject.SetActive(false);
            }
        }

        if (card.GetZone() != MenuControl.Instance.battleMenu.board)
        {
            isMenuCard = true;
        }

        if (card is Unit)
        {
            Unit unit = (Unit)card;
            powerTextCard.transform.parent.gameObject.SetActive(true);
            hPTextCard.transform.parent.gameObject.SetActive(true);
            powerTextCard.text = isMenuCard ? unit.GetInitialPower().ToString() : unit.GetPower().ToString();
            hPTextCard.text = isMenuCard ? unit.GetBaseHP().ToString() : unit.GetHP().ToString();
            
            
            if (isMenuCard &&card is Hero && card.UniqueID != MenuControl.Instance.heroMenu.hero.UniqueID && MenuControl.Instance.eventMenu.isSpecialChallenge)
            {
                hPTextCard.text =((int)( unit.GetBaseHP() * 1.5f)).ToString();
            }

            if (!isMenuCard)
            {
                hPTextCard.color = unit.GetHP() < unit.initialHP ? hpTextLessThanMaxColor : hpTextOriginalColor;
            }

            powerTextIntent.transform.parent.gameObject.SetActive(true);
            hPTextIntent.transform.parent.gameObject.SetActive(true);
            powerTextIntent.text = unit.GetInitialPower().ToString();
            hPTextIntent.text = unit.initialHP.ToString();

            powerTextCard.transform.parent.GetComponent<Image>().sprite = GetPowerIconSprite();
            powerTextIntent.transform.parent.GetComponent<Image>().sprite = GetPowerIconSprite();
        }
        else
        {
            powerTextCard.transform.parent.gameObject.SetActive(false);
            hPTextCard.transform.parent.gameObject.SetActive(false);

            hPTextIntent.transform.parent.gameObject.SetActive(false);
            powerTextIntent.transform.parent.gameObject.SetActive(false);
        }

        if (handCard.activeSelf || card is Skill)
        {
            RenderStars();
        }

        // if (ConsumableOb && !MenuControl.Instance.libraryMenu.isVisible())
        // {
        //     
        //     ConsumableOb .SetActive(this.card.isConsumable);
        // }
        if (ConsumableOb)
        {
            ConsumableOb.SetActive(false);
        }

        StopHighlight();
    }

    public void RenderStars()
    {
        int highestLevel = card.level;
        if (card.FirstUpgradeCard != null)
        {
            highestLevel = card.FirstUpgradeCard.level;
            if (card.FirstUpgradeCard.FirstUpgradeCard != null)
                highestLevel = card.FirstUpgradeCard.FirstUpgradeCard.level;
        }

        highestLevel = Math.Max(1, highestLevel);
        var cardLevel = Math.Max(1, card.level);
        for (int ii = 0; ii < 3; ii += 1)
        {
            starsPanel.GetChild(ii).gameObject.SetActive(ii < highestLevel);
            starsPanel.GetChild(ii).GetChild(0).gameObject.SetActive(ii < cardLevel);
            var color = starsPanel.GetChild(ii).GetChild(0).GetComponentInChildren<Image>().color;
            color.a = 1;
            starsPanel.GetChild(ii).GetChild(0).GetComponentInChildren<Image>().color = color;
        }

        if (isUpgrading)
        {
            var starId = cardLevel - 1;
            starsPanel.GetChild(starId).GetChild(0).gameObject.SetActive(true);
            var color = starsPanel.GetChild(starId).GetChild(0).GetComponentInChildren<Image>().color;
            color.a = 0;
            starsPanel.GetChild(starId).GetChild(0).GetComponentInChildren<Image>().color = color;
            starsPanel.GetChild(starId).GetChild(0).GetComponentInChildren<Image>().DOFade(1, 0.5f)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    bool isSleeping = false;

    public void RenderCardOnBoard(Card card)
    {
        RenderCard(card);
        handCard.SetActive(false);
        boardCard.SetActive(true);
        enemyCard.SetActive(false);

        costText.transform.parent.gameObject.SetActive(false);

        normalBGImage.gameObject.SetActive(false);

        GetCardOnBoardBorderSprite();
        if (!(card is LargeHero))
            boardMaskImage.sprite = card is Hero ? boardCardHeroMaskSprite : boardCardMinionMaskSprite;
        isSleeping = false;
        foreach (var effect in ((Unit)card).currentEffects)
        {
            if (effect.UniqueID == "CommonEffect26")
            {
                isSleeping = true;
            }
        }

        SetInactive(!((Unit)card).CanAct() && !((Unit)card).CanMove(), isSleeping);

        hPTextCard.transform.parent.gameObject.SetActive(false);
        heroIcon.transform.gameObject.SetActive(card is Unit);
        weaponIcon.transform.parent.gameObject.SetActive(card is Unit && ((Unit)card).weapon != null);
        if (card is Unit && ((Unit)card).weapon != null)
        {
            weaponIcon.sprite = MenuControl.Instance.csvLoader.playerCardSprite(((Unit)card).weapon);
            weaponIcon.transform.parent.GetComponentInChildren<Text>().text =
                ((Unit)card).weapon.GetDuality().ToString();
        }

        Unit unit = (Unit)card;

        powerTextBoard.transform.parent.GetComponent<Image>().sprite = GetPowerIconSprite();
        powerTextBoard.text = unit.GetPower().ToString();
        hPTextBoard.text = unit.GetHP().ToString();

        hPTextBoard.color = unit.GetHP() < unit.GetInitialHP() ? hpTextLessThanMaxColor : hpTextOriginalColor;

        int posIndex = 0;
        int negIndex = 0;
        foreach (Image image in positiveEffectImages)
        {
            image.transform.parent.gameObject.SetActive(false);
        }

        foreach (Image image in negativeEffectImages)
        {
            image.transform.parent.gameObject.SetActive(false);
        }

        var currentEffects = new List<Effect>(unit.currentEffects);
        currentEffects.Reverse();
        foreach (Effect effect in currentEffects)
        {
            if (!effect.isVisibleOnCard)
            {
                continue;
            }

            if (effect.isPositive && posIndex < 3)
            {
                Image image = positiveEffectImages[posIndex];

                image.transform.parent.gameObject.SetActive(true);
                //image.sprite = effect.GetSprite();
                var sprite = MenuControl.Instance.csvLoader.buffSprite(effect.GetChineseName());
                image.sprite = sprite;
                for (int ii = 1; ii < image.transform.parent.childCount; ii += 1)
                {
                    Destroy(image.transform.parent.GetChild(ii).gameObject);
                }

                for (int ii = 1; ii < Mathf.Min(3, effect.remainingCharges); ii += 1)
                {
                    GameObject stack = Instantiate(effectBorderPrefab, image.transform.parent) as GameObject;
                    stack.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
                    stack.GetComponent<RectTransform>().anchoredPosition +=
                        (Vector2.right * 4f * ii) + (Vector2.up * 4f * ii);
                }

                posIndex += 1;
            }

            if (!effect.isPositive && negIndex < 3)
            {
                Image image = negativeEffectImages[negIndex];
                image.transform.parent.gameObject.SetActive(true);
                var sprite = MenuControl.Instance.csvLoader.buffSprite(effect.GetChineseName());
                image.sprite = sprite;

                for (int ii = 1; ii < image.transform.parent.childCount; ii += 1)
                {
                    Destroy(image.transform.parent.GetChild(ii).gameObject);
                }

                for (int ii = 1; ii < Mathf.Min(3, effect.remainingCharges); ii += 1)
                {
                    GameObject stack = Instantiate(effectBorderPrefab, image.transform.parent) as GameObject;
                    stack.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
                    stack.GetComponent<RectTransform>().anchoredPosition +=
                        (Vector2.right * 4f * ii) + (Vector2.up * 4f * ii);
                }

                negIndex += 1;
            }
        }
    }

    public void ShowAltview()
    {
        effectsPanel.gameObject.SetActive(false);
        altInfoPanel.gameObject.SetActive(true);
        //Alt view icons
        if (!isSleeping && ((Unit)card).remainingActions > 0)
        {
            altInfoPanel.GetChild(0).GetChild(2).gameObject.SetActive(true);
            altInfoPanel.GetChild(0).GetChild(2).GetComponentInChildren<Text>().text =
                ((Unit)card).remainingActions.ToString();
        }
        else
        {
            altInfoPanel.GetChild(0).GetChild(2).gameObject.SetActive(false);
        }

        if (!isSleeping && ((Unit)card).remainingMoves > 0)
        {
            altInfoPanel.GetChild(1).GetChild(2).gameObject.SetActive(true);
            altInfoPanel.GetChild(1).GetChild(2).GetComponentInChildren<Text>().text =
                ((Unit)card).remainingMoves.ToString();
        }
        else
        {
            altInfoPanel.GetChild(1).GetChild(2).gameObject.SetActive(false);
        }
    }

    public void HideAltView()
    {
        effectsPanel.gameObject.SetActive(true);
        altInfoPanel.gameObject.SetActive(false);
    }

    public void RenderCardBackOnly(Card card)
    {
        this.card = card;
        handCard.SetActive(false);
        boardCard.SetActive(false);
        enemyCard.SetActive(true);
    }

    Sprite GetPowerIconSprite()
    {
        if (card is Unit)
        {
            if (card.activatedAbility is Attack)
            {
                if (((Attack)card.activatedAbility).GetTargetValidator() is TargetLinear)
                {
                    if (((TargetLinear)((Attack)card.activatedAbility).GetTargetValidator()).range > 1)
                    {
                        return powerIconRangedSprite;
                    }
                }
            }
        }

        if (card is Hero)
        {
            if (((Hero)card).weapon != null)
            {
                if (((Hero)card).weapon.activatedAbility.GetTargetValidator() is TargetLinear)
                {
                    if (((TargetLinear)(((Hero)card).weapon.activatedAbility).GetTargetValidator()).range > 1)
                    {
                        return powerIconRangedSprite;
                    }
                }
            }
        }

        return powerIconMeleeSprite;
    }

    public void RenderCardForMenu(Card card, bool forceFaceDown = false)
    {
        isMenuCard = true;
        RenderCard(card, forceFaceDown);
    }

    public void HighlightRed()
    {
        highlightRedHand.SetActive(true);
        highlightGreenHand.SetActive(false);
        highlightBlueHand.SetActive(false);

        highlightRedBoard.SetActive(true);
        highlightGreenBoard.SetActive(false);
        highlightBlueBoard.SetActive(false);
    }

    public void HighlightGreen()
    {
        highlightRedHand.SetActive(false);
        highlightGreenHand.SetActive(true);
        highlightBlueHand.SetActive(false);

        highlightRedBoard.SetActive(false);
        highlightGreenBoard.SetActive(true);
        highlightBlueBoard.SetActive(false);
    }

    public void HighlightCheck()
    {
        highlightRedHand.SetActive(false);
        highlightGreenHand.SetActive(false);
        highlightBlueHand.SetActive(false);

        highlightRedBoard.SetActive(false);
        highlightGreenBoard.SetActive(false);
        highlightBlueBoard.SetActive(false);
        highlightCheckBoard.SetActive(true);
        highlightCheckTalent.SetActive(true);
    }

    public void HighlightCross()
    {
        highlightRedHand.SetActive(false);
        highlightGreenHand.SetActive(false);
        highlightBlueHand.SetActive(false);

        highlightRedBoard.SetActive(false);
        highlightGreenBoard.SetActive(false);
        highlightBlueBoard.SetActive(false);
        highlightCrossBoard.SetActive(true);
    }

    public void HighlightBlue()
    {
        highlightRedHand.SetActive(false);
        highlightGreenHand.SetActive(false);
        highlightBlueHand.SetActive(true);

        highlightRedBoard.SetActive(false);
        highlightGreenBoard.SetActive(false);
        highlightBlueBoard.SetActive(true);
        if (highlightCheckTalent)
        {
            highlightCheckTalent.SetActive(true);
        }
    }

    public void StopHighlight()
    {
        highlightRedHand.SetActive(false);
        highlightGreenHand.SetActive(false);
        highlightBlueHand.SetActive(false);

        highlightRedBoard.SetActive(false);
        highlightGreenBoard.SetActive(false);
        highlightBlueBoard.SetActive(false);
        highlightCrossBoard.SetActive(false);
        highlightCheckBoard.SetActive(false);
        if (highlightCheckTalent != null)
        {
            highlightCheckTalent.SetActive(false);
        }
    }

    public void RenderGhostCard()
    {
        ghostCardBGImage.gameObject.SetActive(true);
        handCard.SetActive(false);
        boardCard.SetActive(false);
        enemyCard.SetActive(false);
        disableInteraction = true;
        isMenuCard = true;
    }

    public void SetInactive(bool active, bool sleeping)
    {
        if (currentInactiveImage)
        {
            currentInactiveImage.gameObject.SetActive(active);
        }

        if (active)
        {
            // currentInactiveImage.SetActive(active);
            // inactiveImage.sprite = currentBorderImage.GetComponent<Image>().sprite;
            // inactiveImage.SetNativeSize();
            // inactiveImage.transform.GetChild(0).GetComponent<Image>().sprite = currentUnderBorderImage.GetComponent<Image>().sprite;
            // inactiveImage.transform.GetChild(0).GetComponent<Image>().SetNativeSize();
        }

        if (sleeping)
        {
            inactivePS.Play();
        }
        else
        {
            inactivePS.Stop();
        }
    }

    public void Hide()
    {
        GetComponent<CanvasGroup>().alpha = 0f;
        disableInteraction = true;
    }

    public void Show()
    {
        if (this && GetComponent<CanvasGroup>())
        {
            GetComponent<CanvasGroup>().alpha =targetTransparency;
            disableInteraction = false;
        }

        isSelect = false;
        isDragging  = false;
    }


    int IndexForLevelOfCard()
    {
        return Mathf.Max(0, card.level - 1);
    }

    public Sprite GetCardFrontBarSprite()
    {
        if (card is NewWeapon)
        {
            return MenuControl.Instance.defaultCardFronts[0 + 1];
        }

        if (card is Artifact)
        {
            return MenuControl.Instance.defaultCardFronts[8 + 1];
        }
        // if (card is Skill)
        // {
        //     return MenuControl.Instance.defaultCardFronts[5+1];
        // }

        if (card is Unit)
        {
            if (card.player == MenuControl.Instance.battleMenu.playerAI)
            {
                return MenuControl.Instance.defaultCardFronts[4 + 1];
            }
            else
            {
                return MenuControl.Instance.defaultCardFronts[2 + 1];
            }
        }

        return MenuControl.Instance.defaultCardFronts[6 + 1];
        return MenuControl.Instance.defaultCardFronts[0 + 1];
    }

    public bool isEnemy = false;
    public Sprite GetCardFrontSprite()
    {
        if (card is NewWeapon)
        {
            return MenuControl.Instance.defaultCardFronts[0];
        }
        // if (card is Skill)
        // {
        //     return MenuControl.Instance.defaultCardFronts[5];
        // }

        if (card is Artifact)
        {
            return MenuControl.Instance.defaultCardFronts[8];
        }

        if (card is Unit)
        { 
            if (card.player == MenuControl.Instance.battleMenu.playerAI || isEnemy)
            {
                return MenuControl.Instance.defaultCardFronts[4];
            }
            else
            {
                return MenuControl.Instance.defaultCardFronts[2];
            }
        }

        return MenuControl.Instance.defaultCardFronts[6];
        return MenuControl.Instance.defaultCardFronts[0];
        // if (card.cardTags.Contains(MenuControl.Instance.naughtyTag))
        // {
        //     return MenuControl.Instance.defaultCardFronts[IndexForLevelOfCard() + 9];
        // }
        //
        // if (card.cardTags.Contains(MenuControl.Instance.niceTag))
        // {
        //     return MenuControl.Instance.defaultCardFronts[IndexForLevelOfCard() + 3];
        // }
        //
        // if (card is Artifact)
        // {
        //     return MenuControl.Instance.defaultCardFronts[IndexForLevelOfCard() + 3];
        // }
        //
        // if (card is Skill)
        // {
        //     return MenuControl.Instance.defaultCardFronts[IndexForLevelOfCard() + 6];
        // }
        //
        // if (MenuControl.Instance.heroMenu.hero != null && card.UniqueID == MenuControl.Instance.heroMenu.hero.UniqueID)
        // {
        //     if (MenuControl.Instance.battleMenu.tutorialMode)
        //     {
        //         return MenuControl.Instance.heroMenu.heroPaths[0].cardFrontSprites[IndexForLevelOfCard()];
        //     }
        //
        //     return MenuControl.Instance.heroMenu.heroPath.cardFrontSprites[IndexForLevelOfCard()];
        // }
        //
        // Sprite returnSprite = MenuControl.Instance.defaultCardFronts[IndexForLevelOfCard()];
        //
        // foreach (HeroPath path in MenuControl.Instance.heroMenu.heroPaths)
        // {
        //     foreach (Card card1 in path.startingCards)
        //     {
        //         if (card.UniqueID == card1.UniqueID)
        //         {
        //             returnSprite = path.cardFrontSprites[IndexForLevelOfCard()];
        //             break;
        //         }
        //     }
        //
        //     foreach (Card card1 in path.pathCards)
        //     {
        //         if (card.UniqueID == card1.UniqueID)
        //         {
        //             returnSprite = path.cardFrontSprites[IndexForLevelOfCard()];
        //             break;
        //         }
        //     }
        // }
        //
        // foreach (Card card1 in MenuControl.Instance.heroMenu.unlockableCards)
        // {
        //     if (card.UniqueID == card1.UniqueID)
        //     {
        //         returnSprite = MenuControl.Instance.heroMenu.heroPath.cardFrontSprites[IndexForLevelOfCard()];
        //         break;
        //     }
        // }
        //
        //
        // if (MenuControl.Instance.areaMenu.currentArea)
        // {
        //     if (MenuControl.Instance.battleMenu.playerAI == card.player ||
        //         (MenuControl.Instance.battleMenu.inBattle && MenuControl.Instance.battleMenu.usingIntentSystem &&
        //          MenuControl.Instance.battleMenu.playerAI.GetHero().GetIntentSystem().GetNextHand()
        //              .GetFollowingHandCards().Contains(card)))
        //     {
        //         returnSprite = MenuControl.Instance.areaMenu.currentArea.cardFrontSprites[IndexForLevelOfCard()];
        //         if (MenuControl.Instance.heroMenu.heroPath.cardFrontSprites[IndexForLevelOfCard()] == returnSprite)
        //         {
        //             int index =
        //                 MenuControl.Instance.heroMenu.heroPaths.IndexOf(MenuControl.Instance.heroMenu.heroPath) + 1;
        //             if (index == MenuControl.Instance.heroMenu.heroPaths.Count) index = 0;
        //             returnSprite = MenuControl.Instance.heroMenu.heroPaths[index]
        //                 .cardFrontSprites[IndexForLevelOfCard()];
        //         }
        //     }
        // }

        //return returnSprite;
    }

    public void GetCardOnBoardBorderSprite()
    {
        boardBGUnderPlayerHero.SetActive(false);
        boardBGPlayerHero.SetActive(false);
        boardBGEnemyMinion.SetActive(false);
        boardBGUnderEnemyMinion.SetActive(false);
        boardBGPlayerUnit.SetActive(false);
        boardBGUnderPlayerUnit.SetActive(false);
        boardBGEnemyBoss.SetActive(false);
        boardBGUnderEnemyBoss.SetActive(false);
        inactiveBoardEnemyBoss.SetActive(false);
        inactiveBoardPlayerUnit.SetActive(false);
        inactiveBoardEnemyMinion.SetActive(false);
        inactiveBoardPlayerHero.SetActive(false);

        if (card is Unit)
        {
            if (card is Minion)
            {
                if (card.player == MenuControl.Instance.battleMenu.playerAI)
                {
                    currentBorderImage = boardBGEnemyMinion;
                    currentUnderBorderImage = boardBGUnderEnemyMinion;
                    currentInactiveImage = inactiveBoardEnemyMinion;
                }
                else
                {
                    currentBorderImage = boardBGPlayerUnit;
                    currentUnderBorderImage = boardBGUnderPlayerUnit;
                    currentInactiveImage = inactiveBoardPlayerUnit;
                }
            }
            else
            {
                if (card.player == MenuControl.Instance.battleMenu.playerAI)
                {
                    currentBorderImage = boardBGEnemyBoss;
                    currentUnderBorderImage = boardBGUnderEnemyBoss;
                    currentInactiveImage = inactiveBoardEnemyBoss;
                }
                else
                {
                    currentBorderImage = boardBGUnderPlayerHero;
                    currentUnderBorderImage = boardBGPlayerHero;
                    currentInactiveImage = inactiveBoardPlayerHero;
                }
            }

            currentBorderImage.SetActive(true);
            currentUnderBorderImage.SetActive(true);
        }
        else
        {
            Debug.LogError("what else can you be? " + card.GetName());
        }
    }

    // public Sprite GetCardOnBoardBorderSprite()
    // {
    //     if (MenuControl.Instance.battleMenu.playerAI == card.player)
    //     {
    //         Sprite returnSprite = null;
    //         if (MenuControl.Instance.areaMenu.currentArea != null)
    //         {
    //             if (card is Hero)
    //                 returnSprite = MenuControl.Instance.areaMenu.currentArea.boardCardHeroSprite;
    //             else
    //                 returnSprite = MenuControl.Instance.areaMenu.currentArea.boardCardMinionSprite;
    //         }
    //
    //         if (returnSprite == null ||
    //             (card is Hero && returnSprite == MenuControl.Instance.heroMenu.heroPath.boardCardHeroSprite) ||
    //             (card is Minion && returnSprite == MenuControl.Instance.heroMenu.heroPath.boardCardMinionSprite))
    //         {
    //             int index = MenuControl.Instance.heroMenu.heroPaths.IndexOf(MenuControl.Instance.heroMenu.heroPath) + 1;
    //             if (index == MenuControl.Instance.heroMenu.heroPaths.Count) index = 0;
    //
    //             if (card is Hero)
    //                 return MenuControl.Instance.heroMenu.heroPaths[index].boardCardHeroSprite;
    //             else
    //                 return MenuControl.Instance.heroMenu.heroPaths[index].boardCardMinionSprite;
    //         }
    //
    //         return returnSprite;
    //     }
    //     else
    //     {
    //         if (MenuControl.Instance.battleMenu.tutorialMode)
    //         {
    //             if (card is Hero)
    //                 return MenuControl.Instance.heroMenu.heroPaths[0].boardCardHeroSprite;
    //             else
    //                 return MenuControl.Instance.heroMenu.heroPaths[0].boardCardMinionSprite;
    //         }
    //     }
    //
    //     if (card is Hero)
    //         return MenuControl.Instance.heroMenu.heroPath.boardCardHeroSprite;
    //     else
    //         return MenuControl.Instance.heroMenu.heroPath.boardCardMinionSprite;
    // }
    public void OnBeginDrag(PointerEventData eventData)
    {
        ///Debug.Log("OnBeginDrag");
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("OnEndDrag");

        isDragging = false;
    }
    
    private bool isDragging = false;
    private bool isSelect = false;

    
#if UNITY_ANDROID || UNITY_IOS
    private void Update()
    {
        var canvas = this.GetComponent<Canvas>();
        if (!canvas || isDragging)
            return;
        
        var canvasRT = canvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, Input.mousePosition, canvas.worldCamera, out var p);
        var rect = normalBGImage.GetComponent<RectTransform>().rect;
        if (Input.GetMouseButtonDown(0) && !rect.Contains(p) && isSelect)
        {
            GetComponentInParent<BasicMenu>().DeSelectVisibleCard(this);
            isSelect = false;
        }
    }
#endif
}