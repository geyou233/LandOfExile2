using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class CardDescriptionPanel : MonoBehaviour
{
    public int offset = 700;
    public float scale = 0.9f;

    public int sidePanelOneColumnCount = 6;
    private float moveOffeset = 700;
    public Text titleText;

    //public Text cardTypeText;
    public Transform cardTypeParent;

    public GameObject cardTypePrefab;

    public GameObject upgradeHelpButton;

    //public Image cardImage;
    public VisibleCard visibleCard;
    public Text statsText;
    public Text statsTextStatic;
    public Text descriptionText;
    public GameObject separation;
    public Text costText;
    public GameObject keywordHeader;
    public List<Text> keywordTexts = new List<Text>();
    public Transform[] leftSidePanels;
    public Transform leftSidePanelHolder;
    public Transform leftSidePanelHolder2;
    public Transform rightSidePanelHolder;
    public Transform rightSidePanelHolder2;
    public GameObject descriptionSidePanelPrefab;
    public RectTransform vertPanel;
    public RectTransform vertPanelBG; 
    public int keywordCount;
    public List<string> keywords = new List<string>();

    public Text cooldownLabel;

    public int sidePanelCount;
    
    private float leftMost = 0;
    private float rightMost = 0;

    private void Update()
    {
        vertPanelBG.sizeDelta = vertPanel.sizeDelta + Vector2.up * 80f;
        vertPanelBG.anchoredPosition = vertPanel.anchoredPosition + Vector2.down * 25f;
    }

    string wrapColor(int value1 ,int value2, string text)
    {
        if (value1>value2)
        {
            text = "<color=green>" + text + "</color>";
        }
        else if(value1<value2)
        {
            
            text = "<color=#D1C8AA>" + text + "</color>";
        }

        return text;
    }

    public static string DescriptionText(Card card,Transform sidePanelHolder,CardDescriptionPanel panel,bool clearOriginalColor = true)
    {
        string textToShow = card.GetDescription();

        if (clearOriginalColor)
        {
            textToShow = textToShow.Replace("<color=upgrade>", "").Replace("</color>", "");
        }

        if (panel)
        {
            panel.leftMost =
                RectTransformUtility.WorldToScreenPoint(Camera.main, panel.vertPanel.GetComponent<RectTransform>().position)
                    .x - panel.vertPanel.rect.width/2;
            panel.rightMost =
                RectTransformUtility.WorldToScreenPoint(Camera.main, panel.vertPanel.GetComponent<RectTransform>().position)
                    .x + panel.vertPanel.rect.width/2;
        }
        
        //Tack on Movement for units
        if (card is Unit)
        {
            Unit unit = (Unit)card;
            if (unit.movementType != null && unit.movementType != MenuControl.Instance.heroMenu.allMovementTypes[0] &&
                unit.movementType != MenuControl.Instance.heroMenu.allMovementTypes[1] && !unit.isFlying)
            {
                //只要不是基础移动方式（包括大型英雄基础移动方式）就不显示
                //如果在飞行中也不显示移动方式
                if (!textToShow.Contains(unit.movementType.GetName()))
                    textToShow += (textToShow == "" ? "" : " ") + unit.movementType.GetName();
            }
        }

        //Referring to other cards
        while (textToShow.Contains("{{") && textToShow.Contains("}}"))
        {
            string uniqueID = textToShow.Substring(textToShow.IndexOf("{{") + 2,
                textToShow.IndexOf("}}") - (textToShow.IndexOf("{{") + 2));
            CollectibleItem referredCard = MenuControl.Instance.heroMenu.GetCardByID(uniqueID);
            if (referredCard == null) referredCard = MenuControl.Instance.heroMenu.GetEffectByID(uniqueID);
            if (referredCard != null)
            {
                string newTextToShow = "<b>" + referredCard.GetName() + "</b>";
                if (referredCard is Unit)
                {
                    newTextToShow += " " + ((Unit)referredCard).GetInitialPower() + "/" +
                                     ((Unit)referredCard).GetInitialHP();
                }

                newTextToShow += " - ";
                string newDescription = referredCard.GetDescription();
                if (newDescription.Contains("{{") && newDescription.Contains("}}"))
                {
                    string uniqueID2 = newDescription.Substring(newDescription.IndexOf("{{") + 2,
                        newDescription.IndexOf("}}") - (newDescription.IndexOf("{{") + 2));
                    CollectibleItem referredCard2 = MenuControl.Instance.heroMenu.GetCardByID(uniqueID2);
                    if (referredCard2 == null) referredCard2 = MenuControl.Instance.heroMenu.GetEffectByID(uniqueID2);
                    if (referredCard2 != null)
                    {
                        newDescription = newDescription.Replace("{{" + uniqueID2 + "}}",
                            "<color=#86979c>" + referredCard2.GetName() + "</color>");
                    }
                }

                newTextToShow += newDescription;

                textToShow = textToShow.Replace("{{" + uniqueID + "}}",
                    "<color=%specialColor%>" + referredCard.GetName() + "</color>");

                bool makePanel = true;
                if (card is Unit)
                {
                    if (referredCard is Effect)
                    {
                        foreach (Effect effect in ((Unit)card).currentEffects)
                        {
                            if (effect.originalTemplate == (Effect)referredCard)
                            {
                                makePanel = false;
                            }
                        }
                    }
                }

                if (makePanel && sidePanelHolder)
                {
                    Sprite sprite = null;
                    if (referredCard is Card referredCardCard)
                    {
                        sprite = MenuControl.Instance.csvLoader.playerCardSprite(referredCardCard);
                    }
                    else
                    {
                        sprite = MenuControl.Instance.csvLoader.buffSprite(referredCard.GetChineseName());
                    }

                    if (sprite == null)
                    {
                        Debug.LogError("side panel has no sprite for " + referredCard.GetChineseName());
                    }

                    panel?.CreateSidePanel(sidePanelHolder,sprite /*card referredCard.GetSprite()*/, newTextToShow, card,
                        referredCard is Card);
                }
            }
            else
            {
                textToShow = textToShow.Replace("{{" + uniqueID + "}}", "<color=%specialColor%>" + uniqueID + "</color>");
            }
        }
        //
        // if (card.cardTags.Contains(MenuControl.Instance.naughtyTag))
        // {
        //     string newTextToShow = "<b>" + MenuControl.Instance.GetLocalizedString("CommonEffect45CardName") +
        //                            "</b> - " + MenuControl.Instance.GetLocalizedString("CommonEffect45CardDescription");
        //     CreateSidePanel(sidePanelHolder, MenuControl.Instance.naughtySprite, newTextToShow);
        // }
        //
        // if (card.cardTags.Contains(MenuControl.Instance.niceTag))
        // {
        //     string newTextToShow = "<b>" + MenuControl.Instance.GetLocalizedString("CommonEffect46CardName") +
        //                            "</b> - " + MenuControl.Instance.GetLocalizedString("CommonEffect46CardDescription");
        //     CreateSidePanel(sidePanelHolder, MenuControl.Instance.niceSprite, newTextToShow);
        // }

        if (clearOriginalColor)
        {
            string[] stringArray = "0,1,2,3,4,5,6,7,8,9".Split(',');
            foreach (string numberString in stringArray)
            {
                textToShow = textToShow.Replace(numberString, "<color=%specialColor%>" + numberString + "</color>");
            }
            
        }
        
        textToShow = textToShow.Replace("%specialColor%", "#86979c");
        return textToShow;
    }

    [HideInInspector]
    public bool showCardView = true;
    public bool isEnemy = false;
    public void RenderPanel(Card card, bool rightSidePanel)
    {
        Stopwatch stopwatch = new Stopwatch();
        isOnRight = MenuControl.Instance.battleMenu.inBattle;// card.GetZone() == MenuControl.Instance.battleMenu.board || card.GetZone()==MenuControl.Instance.battleMenu.hand
           // ||(MenuControl.Instance.battleMenu.inBattle && card.GetZone() == null);//intent
        // if (I2.Loc.LocalizationManager.CurrentLanguage != MenuControl.Languages.English.ToString())
        // {
        //     descriptionText.font = MenuControl.Instance.GetSafeFont();
        //     statsText.font = MenuControl.Instance.GetSafeFont();
        //     titleText.font = MenuControl.Instance.GetSafeFont();
        //     //cardTypeText.font = MenuControl.Instance.GetSafeFont();
        // }
        stopwatch.Start();
        sidePanelCount = 0;
        keywordCount = 0;
        keywords.Clear();
        Transform sidePanelHolder;
        if (isOnRight)
        {
            //场上的牌
            sidePanelHolder = leftSidePanels[0];

        }
        else
        {
            sidePanelHolder = rightSidePanel ? rightSidePanelHolder : leftSidePanelHolder;
        }


        foreach (Transform child in leftSidePanels)
        {
            foreach (Transform c in child)
            {
                
                Destroy(c.gameObject);
            }
        }
        foreach (Transform child in leftSidePanelHolder)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in rightSidePanelHolder)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in leftSidePanelHolder2)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in rightSidePanelHolder2)
        {
            Destroy(child.gameObject);
        }

        titleText.text = card.GetName();

        var types = card.GetCardTypes();
        foreach (Transform child in cardTypeParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var type in types)
        {
            var typeGO = Instantiate(cardTypePrefab, cardTypeParent);
            typeGO.GetComponentInChildren<Text>().text = type;
        }
        //cardTypeText.text = card.GetCardTypeText();
        //costText.text = card.GetCost().ToString();
        //costText.transform.parent.gameObject.SetActive(card.GetCost() > 0);

        //cardImage.sprite = card.GetSprite();
        visibleCard.isEnemy = isEnemy;
        visibleCard.ignoreBattlefieldEffect = true;
        visibleCard.RenderCard(card);
        visibleCard.DisableCardInteraction();

        var isStatActive = card is Unit || card is NewWeapon;
        statsText.gameObject.SetActive(isStatActive);
        statsTextStatic.gameObject.SetActive(isStatActive);

        if (card is Unit)
        {
            if (card == MenuControl.Instance.heroMenu.hero && card.GetZone() != MenuControl.Instance.battleMenu.board)
            {
                statsText.text = MenuControl.Instance.GetLocalizedString("Power") + ":  ";
                statsTextStatic.text = ((Unit)card).GetInitialPower().ToString();
                statsText.text += "\n" + MenuControl.Instance.GetLocalizedString("HP") + ":  ";
                statsTextStatic.text += "\n" + ((Unit)card).GetHP().ToString() + " / " + ((Unit)card).GetInitialHP().ToString();
            }
            else if (card.GetZone() == MenuControl.Instance.battleMenu.board)
            {
                var condition = ((Unit)card).GetPower() > ((Unit)card).GetInitialPower();
                statsText.text = wrapColor(((Unit)card).GetPower(),((Unit)card).GetInitialPower(), MenuControl.Instance.GetLocalizedString("Power") + ":  ");
                statsTextStatic.text = wrapColor(((Unit)card).GetPower(),((Unit)card).GetInitialPower(), ((Unit)card).GetPower() + " / " + ((Unit)card).GetInitialPower().ToString());
                // ... similar changes for hpString, actionsString, movesString

                condition = ((Unit)card).GetHP() > ((Unit)card).GetInitialHP();
                statsText.text += wrapColor(((Unit)card).GetHP(),((Unit)card).GetInitialHP(),"\n" + MenuControl.Instance.GetLocalizedString("HP") + ":  ");
                statsTextStatic.text += wrapColor(((Unit)card).GetHP(),((Unit)card).GetInitialHP(),"\n" +((Unit)card).GetHP().ToString() + " / " + ((Unit)card).GetInitialHP().ToString());

                condition = ((Unit)card).remainingActions > ((Unit)card).GetInitialActions();
                statsText.text += wrapColor(((Unit)card).remainingActions,((Unit)card).GetInitialActions(),"\n" + MenuControl.Instance.GetLocalizedString("Actions") + ":  ");
                statsTextStatic.text += wrapColor(((Unit)card).remainingActions,((Unit)card).GetInitialActions(),"\n" +((Unit)card).remainingActions.ToString() + " / " +
                                                            ((Unit)card).GetInitialActions().ToString());
                condition = ((Unit)card).remainingMoves > ((Unit)card).GetInitialMoves();
                statsText.text += wrapColor( ((Unit)card).remainingMoves, ((Unit)card).GetInitialMoves(),"\n" + MenuControl.Instance.GetLocalizedString("Moves") + ":  ");
                statsTextStatic.text += wrapColor(((Unit)card).remainingMoves, ((Unit)card).GetInitialMoves(),"\n" +((Unit)card).remainingMoves.ToString() + " / " +
                                                            ((Unit)card).GetInitialMoves().ToString());


            }
            else
            {
                statsText.text = MenuControl.Instance.GetLocalizedString("Power") + ":  ";
                statsTextStatic.text = ((Unit)card).GetInitialPower().ToString();
                statsText.text += "\n" + MenuControl.Instance.GetLocalizedString("Hit Points") + ":  ";
                statsTextStatic.text += "\n" +  ((Unit)card).GetInitialHP().ToString();
            }
        }

        if (card is NewWeapon)
        {
            statsText.text = MenuControl.Instance.GetLocalizedString("Power") + ":";
            statsTextStatic.text = ((NewWeapon)card).initialPower.ToString();
            statsText.text += "\n" + MenuControl.Instance.GetLocalizedString("Duality") + ":";
            statsTextStatic.text += "\n" +  ((NewWeapon)card).initialDuality.ToString();
        }
//tested here

        if (card is Artifact artifact && artifact.initialCoolDown!=0)
        {
            cooldownLabel.transform.parent.gameObject.SetActive(true);
            cooldownLabel.text = MenuControl.Instance.GetLocalizedString("CooldownKeywordName") + ": " +
                                 artifact.initialCoolDown.ToString();
        }
        else
        {
            cooldownLabel.transform.parent.gameObject.SetActive(false);
        }
        var textToShow = DescriptionText(card,sidePanelHolder,this);
        textToShow = BuildKeywords(textToShow);
        descriptionText.text = textToShow;
        if (I2.Loc.LocalizationManager.CurrentLanguage != MenuControl.Languages.English.ToString())
        {
            descriptionText.text = descriptionText.text.Replace("+", "+ ");
        }
#if !UNITY_STANDALONE
        descriptionText.fontSize = 38;
#endif
        stopwatch.Stop();
        
        long elapsedTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"RenderPanel 方法初始化执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();
        if (card is NewWeapon)
        {
            string textToShow3 = MenuControl.Instance.GetLocalizedString("OneWeaponPrompt");

            CreateSidePanel(sidePanelHolder,  MenuControl.Instance.csvLoader.playerCardSprite(card),
                textToShow3,  card,true,false);
        }else if (card is Castable && card.cardTags.Contains(MenuControl.Instance.spellTag))
        {
            string textToShow3 = MenuControl.Instance.GetLocalizedString("SpellPrompt");

            CreateSidePanel(sidePanelHolder, MenuControl.Instance.csvLoader.playerCardSprite(card),
                textToShow3, card, true,false);
        }else if (card is LargeHero)
        {
            string textToShow3 = MenuControl.Instance.GetLocalizedString("LargeBossPrompt");

            CreateSidePanel(sidePanelHolder, MenuControl.Instance.csvLoader.playerCardSprite(card),
                textToShow3,  card,true,false);
        }

        if (card is Minion && MenuControl.Instance.exhustAllMinionAfterUsage)
        {
            string textToShow3 = MenuControl.Instance.GetLocalizedString("MinionPrompt");

            CreateSidePanel(sidePanelHolder,  MenuControl.Instance.csvLoader.playerCardSprite(card),
                textToShow3,  card,true,false);
        }

        if (card is Unit)
        {
            Unit hero = (Unit)card;
            if (hero.weapon != null)
            {
                string textToShow2 = "<b><color=#86979c>" + hero.weapon.GetName() + "</color></b> - ";
                textToShow2 += MenuControl.Instance.GetLocalizedString("Power") + ":" +
                               hero.weapon.GetPower().ToString();
                textToShow2 += MenuControl.Instance.GetLocalizedString("Duality") + ":" +
                               hero.weapon.GetDuality().ToString();

                string newDescription = hero.weapon.GetDescription();
                
                newDescription = ClearUpgradeColor(newDescription);
                if (newDescription.Contains("{{") && newDescription.Contains("}}"))
                {
                    string uniqueID2 = newDescription.Substring(newDescription.IndexOf("{{") + 2,
                        newDescription.IndexOf("}}") - (newDescription.IndexOf("{{") + 2));
                    CollectibleItem referredCard2 = MenuControl.Instance.heroMenu.GetCardByID(uniqueID2);
                    if (referredCard2 == null) referredCard2 = MenuControl.Instance.heroMenu.GetEffectByID(uniqueID2);
                    if (referredCard2 != null)
                    {
                        newDescription = newDescription.Replace("{{" + uniqueID2 + "}}", referredCard2.GetName());
                    }
                }

                textToShow2 += newDescription;

                textToShow2 = BuildKeywords(textToShow2);

                CreateSidePanel(sidePanelHolder, 
                    MenuControl.Instance.csvLoader.playerCardSprite(hero.weapon), textToShow2,  null,true);
            }
        }


        elapsedTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"RenderPanel 方法hero执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();
        if (card is Unit)
        {
            List<Effect> effectsToShow = ((Unit)card).currentEffects;
            if (card.cardTemplate == null && card is Hero)
            {
                effectsToShow = ((Hero)card).startingEffects;
            }

            elapsedTime = stopwatch.ElapsedMilliseconds;
            Debug.Log($"RenderPanel 方法 unit 准备 执行时间为: {elapsedTime} 毫秒");
            stopwatch.Restart();
            int ii = 0;
            foreach (Effect effect in effectsToShow)
            {
                var stopwatch2 = new Stopwatch();
                stopwatch2.Start();
                // if (effect.UniqueID == "WarriorEffect100")
                // {
                //     continue;
                // }
                if (!effect.isVisibleOnCard)
                {
                    continue;
                }

                string chargesText = "";
                if (effect.remainingCharges > 0)
                {
                    chargesText = " (" + effect.remainingCharges.ToString() + ")";
                }

                if (card.cardTemplate == null && card is Hero)
                {
                    if (((Hero)card).startingEffectCharges[ii] > 0)
                        chargesText = " (" + ((Hero)card).startingEffectCharges[ii].ToString() + ")";
                }

                elapsedTime = stopwatch2.ElapsedMilliseconds;
                //Debug.Log($"RenderPanel unit 1 执行时间为: {elapsedTime} 毫秒");
                stopwatch2.Restart();
                string effectText = "<b>" + MenuControl.Instance.GetLocalizedString(effect.UniqueID + "CardName") +
                                    chargesText + " - " + MenuControl.Instance.GetLocalizedString("Effect") + ":</b> ";

                string newDescription = MenuControl.Instance.GetLocalizedString(effect.UniqueID + "CardDescription");
                
                elapsedTime = stopwatch2.ElapsedMilliseconds;
                //Debug.Log($"RenderPanel unit 2 执行时间为: {elapsedTime} 毫秒");
                stopwatch2.Restart();
                if (newDescription.Contains("{{") && newDescription.Contains("}}"))
                {
                    string uniqueID2 = newDescription.Substring(newDescription.IndexOf("{{") + 2,
                        newDescription.IndexOf("}}") - (newDescription.IndexOf("{{") + 2));
                    CollectibleItem referredCard2 = MenuControl.Instance.heroMenu.GetCardByID(uniqueID2);
                    if (referredCard2 == null) referredCard2 = MenuControl.Instance.heroMenu.GetEffectByID(uniqueID2);
                    if (referredCard2 != null)
                    {
                        newDescription = newDescription.Replace("{{" + uniqueID2 + "}}",
                            "<color=#86979c>" + referredCard2.GetName() + "</color>");
                    }
                }

                elapsedTime = stopwatch2.ElapsedMilliseconds;
                //Debug.Log($"RenderPanel unit 3 执行时间为: {elapsedTime} 毫秒");
                stopwatch2.Restart();
                effectText += newDescription;

                effectText = BuildKeywords(effectText);

                elapsedTime = stopwatch2.ElapsedMilliseconds;
                Debug.Log($"RenderPanel unit 4 执行时间为: {elapsedTime} 毫秒");
                stopwatch2.Restart();
                CreateSidePanel( sidePanelHolder,MenuControl.Instance.csvLoader.buffSprite(effect.GetChineseName()),
                    effectText,null);
                
                elapsedTime = stopwatch2.ElapsedMilliseconds;
                //Debug.Log($"RenderPanel unit 5 执行时间为: {elapsedTime} 毫秒");
                stopwatch2.Restart();
                ii += 1;
            }
        }



        elapsedTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"RenderPanel 方法unit执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();
        if (descriptionText.text.Length == 0 && keywords.Count == 0)
        {
            separation.SetActive(false);
        }
        else
        {
            separation.SetActive(true);
        }


        upgradeHelpButton.SetActive(showCardView && card.showUpgradeView());

        
        //逻辑变化，把说明始终放在最右


        if (isOnRight)
        {
            //场上的牌
            
            // var width =RectTransformUtility.PixelAdjustRect(vertPanel, GameObject.Find("Canvas - MasterCanvas").GetComponent<Canvas>()).width;
            // var offset = vertPanel.rect.width * vertPanel.localScale.x /2;
            // // var maxRightValue = Screen.width - offset;
            // //
            // // 获取屏幕的宽度
            // float screenWidth = Screen.width;
            transform.parent = MenuControl.Instance.infoMenu.rightPanelAnchor;
            transform.localPosition = new Vector3(0, 0, 0);
           // GetComponent<RectTransform>().anchoredPosition = MenuControl.Instance.infoMenu.rightPanelAnchor.position;
            // 计算目标位置
            // Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth - (width/2), 0, Camera.main.nearClipPlane));
            //
            // // 将世界坐标转换为 RectTransform 的锚点坐标
            // Vector2 anchoredPosition = vertPanel.anchoredPosition;
            // transform.position = new Vector3(worldPosition.x, anchoredPosition.y, transform.position.z);
        }
        else
        {
            int offset = 100;
            var minLeftValue = offset;
            if (leftMost<minLeftValue)
            {
                var adjust = minLeftValue-leftMost;
                if (rightMost + adjust>Screen.width)
                {
                    adjust = Math.Min(adjust, Screen.width - rightMost);
                }
                GetComponent<RectTransform>().anchoredPosition += Vector2.right * adjust;
            }
            
            var maxRightValue = Screen.width - offset;
            
            if (rightMost>maxRightValue)
            {
                var adjust = rightMost - maxRightValue;
            
                if (leftMost - adjust<0)
                {
                    adjust = Math.Min(adjust, leftMost - 0);
                }
                GetComponent<RectTransform>().anchoredPosition += Vector2.left * adjust;
            }
        }

        
        
        
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one * scale, 0.2f).setEaseOutQuad();


        elapsedTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"RenderPanel 方法 快结束了 执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();


        Canvas.ForceUpdateCanvases();


        GetComponent<CanvasGroup>().alpha = 0f;
        StartCoroutine(updateMenu());
        
        elapsedTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"RenderPanel 方法 结束了 执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();
        
    }

    string parseDescriptionWithCardUniqueId(string textToShow)
    {
        //为了反噬这个keyword加的，keyword中有{{}} Magic006d
        if (textToShow.Contains("{{") && textToShow.Contains("}}"))
        {
            string uniqueID = textToShow.Substring(textToShow.IndexOf("{{") + 2,
                textToShow.IndexOf("}}") - (textToShow.IndexOf("{{") + 2));
            CollectibleItem referredCard = MenuControl.Instance.heroMenu.GetCardByID(uniqueID);
            if (referredCard == null) referredCard = MenuControl.Instance.heroMenu.GetEffectByID(uniqueID);
            if (referredCard != null)
            {
                string newTextToShow = "<b>" + referredCard.GetName() + "</b>";
                if (referredCard is Unit)
                {
                    newTextToShow += " " + ((Unit)referredCard).GetInitialPower() + "/" +
                                     ((Unit)referredCard).GetInitialHP();
                }

                newTextToShow += " - ";
                string newDescription = referredCard.GetDescription();
                if (newDescription.Contains("{{") && newDescription.Contains("}}"))
                {
                    string uniqueID2 = newDescription.Substring(newDescription.IndexOf("{{") + 2,
                        newDescription.IndexOf("}}") - (newDescription.IndexOf("{{") + 2));
                    CollectibleItem referredCard2 = MenuControl.Instance.heroMenu.GetCardByID(uniqueID2);
                    if (referredCard2 == null) referredCard2 = MenuControl.Instance.heroMenu.GetEffectByID(uniqueID2);
                    if (referredCard2 != null)
                    {
                        newDescription = newDescription.Replace("{{" + uniqueID2 + "}}",
                            "<color=#86979c>" + referredCard2.GetName() + "</color>");
                    }
                }

                newTextToShow += newDescription;

                textToShow = textToShow.Replace("{{" + uniqueID + "}}",
                    "<color=%specialColor%>" + referredCard.GetName() + "</color>");

                bool makePanel = true;
                // if (card is Unit)
                // {
                //     if (referredCard is Effect)
                //     {
                //         foreach (Effect effect in ((Unit)card).currentEffects)
                //         {
                //             if (effect.originalTemplate == (Effect)referredCard)
                //             {
                //                 makePanel = false;
                //             }
                //         }
                //     }
                // }

                if (makePanel && sidePanelHolder)
                {
                    Sprite sprite = null;
                    if (referredCard is Card referredCardCard)
                    {
                        sprite = MenuControl.Instance.csvLoader.playerCardSprite(referredCardCard);
                    }
                    else
                    {
                        sprite = MenuControl.Instance.csvLoader.buffSprite(referredCard.GetChineseName());
                    }

                    if (sprite == null)
                    {
                        Debug.LogError("side panel has no sprite for " + referredCard.GetChineseName());
                    }

                    CreateSidePanel(sidePanelHolder,sprite /*card referredCard.GetSprite()*/, newTextToShow,(referredCard as Card ),
                        referredCard is Card);
                }
            }
            else
            {
                textToShow = textToShow.Replace("{{" + uniqueID + "}}", "<color=%specialColor%>" + uniqueID + "</color>");
            }
            
            textToShow = textToShow.Replace("%specialColor%", "#86979c");
        }

        return textToShow;
    }
    
    IEnumerator updateMenu()
    {
        yield return new WaitForSeconds(0.01f);
        cardTypeParent.GetComponent<LayoutGroup>().enabled = false;
        yield return new WaitForSeconds(0.01f);
        cardTypeParent.GetComponent<LayoutGroup>().enabled = true;
        
        LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 1f, 0.2f).setEaseOutQuad();
    }

    // private float leftMost = 0;
    // private float rightMost = 0;
    public float sidePanelWidth = 220;
    private int currentColumn = 0;
    private bool isOnRight = false;
    Transform sidePanelHolder=> leftSidePanels[currentColumn];

    string ClearUpgradeColor(string text)
    {
        return text.Replace("<color=upgrade>", "").Replace("</color>", "");
    }
    private void CreateSidePanel(Transform sidePanelHolderOld, Sprite sprite, string textToShow,Card card, bool isCard = false,bool showCost = true)
    {
        Transform finalSidePanelHolder = sidePanelHolderOld;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        if (isOnRight)
        {
            sidePanelCount += 1;
            if (sidePanelCount >= sidePanelOneColumnCount * (currentColumn+1))
            {
                currentColumn++;
                //sidePanelHolder = leftSidePanels[currentColumn];
            }

            if (currentColumn >= leftSidePanels.Length)
            {
                //依然超了
                Debug.LogError($"too many side panels {sidePanelCount}");
                return;
            }
            
            finalSidePanelHolder = sidePanelHolder;
        }
        else
        {
            if (sidePanelCount >= sidePanelOneColumnCount)
            {
                if (finalSidePanelHolder == rightSidePanelHolder)
                {
                    finalSidePanelHolder = rightSidePanelHolder2;
                
                    // if (sidePanelCount == sidePanelOneColumnCount && transform.position.x >= -moveOffeset)
                    // {
                    //     GetComponent<RectTransform>().anchoredPosition += Vector2.left * moveOffeset;
                    // }
                }
                if (finalSidePanelHolder == leftSidePanelHolder)
                {
                    finalSidePanelHolder = leftSidePanelHolder2;
                    // if (sidePanelCount == sidePanelOneColumnCount && transform.position.x <= moveOffeset)
                    // {
                    //     GetComponent<RectTransform>().anchoredPosition += Vector2.right * moveOffeset;
                    // }
                }
            }

        if (finalSidePanelHolder == leftSidePanelHolder || finalSidePanelHolder == leftSidePanelHolder2)
        {
            
            
                leftMost = Math.Min(leftMost, RectTransformUtility
                    .WorldToScreenPoint(Camera.main, sidePanelHolder.GetComponent<RectTransform>().position).x - sidePanelWidth);
            }
            else
            {
            
                rightMost = Math.Max(rightMost, RectTransformUtility
                    .WorldToScreenPoint(Camera.main, sidePanelHolder.GetComponent<RectTransform>().position).x+sidePanelWidth); 
        }
        
        }
        
        // if (sidePanelHolder == leftSidePanelHolder || sidePanelHolder == leftSidePanelHolder2)
        // {
        //     
        //     leftMost = Math.Min(leftMost, RectTransformUtility
        //         .WorldToScreenPoint(Camera.main, sidePanelHolder.GetComponent<RectTransform>().position).x - sidePanelWidth);
        // }
        // else
        // {
        //     
        //     rightMost = Math.Max(rightMost, RectTransformUtility
        //         .WorldToScreenPoint(Camera.main, sidePanelHolder.GetComponent<RectTransform>().position).x+sidePanelWidth);
        // }
        
        //add rightSidePanelHolder check
        
        // if (leftMost<0)
        // {
        //     var adjust = -leftMost;
        //         if (rightMost + adjust>Screen.width)
        //         {
        //             adjust = Math.Min(adjust, Screen.width - rightMost);
        //         }
        //         GetComponent<RectTransform>().anchoredPosition += Vector2.right * adjust;
        //     }
        //
        // if (rightMost>Screen.width)
        // {
        //     var adjust = rightMost - Screen.width;
        //     
        //     if (leftMost - adjust<0)
        //     {
        //         adjust = Math.Min(adjust, leftMost - 0);
        //     }
        //     GetComponent<RectTransform>().anchoredPosition += Vector2.left * adjust;
        // }

        GameObject sidePanel = Instantiate(descriptionSidePanelPrefab, finalSidePanelHolder);

       var sidePanelScript =  sidePanel.GetComponent<DescriptionSidePanel>();

       if (isCard)
       {
           sidePanelScript.cardWithBorder.SetActive(true);
           sidePanelScript.contentImage.sprite = sprite;
           
           sidePanelScript.effectImage.gameObject.SetActive(false);
       }
       else
       {
           
           sidePanelScript.cardWithBorder.SetActive(false);
           sidePanelScript.effectImage.gameObject.SetActive(true);
           sidePanelScript.effectImage.sprite = sprite;
       }

       if (card != null && card.initialCost > 0 && showCost)
       {
           sidePanelScript.costOb.SetActive(true);
           sidePanelScript.costOb.GetComponentInChildren<Text>().text = card.GetCost().ToString();
       }
       else
       {
           sidePanelScript.costOb.SetActive(false);
           
       }

       sidePanelScript.text.text = textToShow;


#if !UNITY_STANDALONE
        sidePanel.GetComponentInChildren<Text>().fontSize = 32;
#endif
        
        var elapsedTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"RenderPanel 方法create side panel执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();
    }

    //这个才是真正加入底部
    void AddKeyword( string keyword, string keyWordName, string keyWordDescription)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        if (!keywords.Contains(keyword))
        {
            keywords.Add(keyword);
            keywordCount += 1;
            //超过的keycount先无视掉
            if (keywordCount > keywordTexts.Count)
            {
                keywordCount -= 1;
                return;
            }

            keyWordDescription = parseDescriptionWithCardUniqueId(keyWordDescription);
            keywordTexts[keywordCount - 1].text = "<b><color=#d1c8aa>" + keyWordName + ":</color></b> " + keyWordDescription;
        }

#if !UNITY_STANDALONE
        keywordTexts[keywordCount - 1].fontSize = 32;
#endif

        keywordTexts[keywordCount - 1].transform.parent.gameObject.SetActive(true);
        int keyword1TextId = keywordCount - 1;
        
        var keywordsData = MenuControl.Instance.infoMenu.GetKeywordData(keyWordDescription);
        
        keyWordDescription = keywordsData.afterKeywords;
        var tempKeywords = keywordsData.keywords;

        List<string> newKeywords = new List<string>();
        foreach (var tempKeyword in tempKeywords)
        {
            if (!keywords.Contains(tempKeyword))
            { 
                if (tempKeyword == "Range" && keywords.Contains("Arcing"))
                {
                    continue;
                }
                newKeywords.Add((tempKeyword));
            }
        }
        
        
        //remove keywords that are overlap 中文一个射程一个箭术射程
        if (newKeywords.Contains("Arcing") && newKeywords.Contains("Range"))
        {
            newKeywords.Remove("Range");
        }

        foreach (var k in newKeywords)
        {
            
            keywordCount += 1;
            if (keywordCount > keywordTexts.Count)
            {
                keywordCount -= 1;
                return;
            }
            string keyWordName2 = MenuControl.Instance.GetLocalizedString(k + "KeywordName");
             string keyWordDescription2 = MenuControl.Instance.GetLocalizedString(k + "KeywordDescription");
            keywordTexts[keywordCount - 1].text =
                "<b><color=#d1c8aa>" + keyWordName2 + ":</color></b> " + keyWordDescription2;

#if !UNITY_STANDALONE
                    keywordTexts[keywordCount - 1].fontSize = 32;
#endif

            keywordTexts[keywordCount - 1].transform.parent.gameObject.SetActive(true);
        }
        
        //Keyword references another keyword check
//         foreach (string keyword2 in MenuControl.Instance.infoMenu.keywordStrings.ToArray())
//         {
//             string keyWordName2 = MenuControl.Instance.GetLocalizedString(keyword2 + "KeywordName");
//             string keyWordDescription2 = MenuControl.Instance.GetLocalizedString(keyword2 + "KeywordDescription");
//
//             if (keyWordDescription.Contains(keyWordName2))
//             {
//                 if (keywords.Contains(keyword2))
//                 {
//                     continue;
//                 }
//
//                 //为了水盾术MagicS23a 加的
//                 if (keywords.Contains("Arcing") && keyword2 =="Range")
//                 {
//                     continue;
//                 }
//
//                 {
//                     keyWordDescription2 = parseDescriptionWithCardUniqueId(keyWordDescription2);
//                     keywordTexts[keyword1TextId].text =
//                         keywordTexts[keyword1TextId].text.Replace(keyWordName2, "<color=#86979c>" + keyWordName2 + "</color>");
//                     keywords.Add(keyword2);
//                     keywordCount += 1;
//                     //超过的keycount先无视掉
//                     if (keywordCount > keywordTexts.Count)
//                     {
//                         keywordCount -= 1;
//                         return;
//                     }
//                     keywordTexts[keywordCount - 1].text =
//                         "<b><color=#d1c8aa>" + keyWordName2 + ":</color></b> " + keyWordDescription2;
//
// #if !UNITY_STANDALONE
//                     keywordTexts[keywordCount - 1].fontSize = 32;
// #endif
//
//                     keywordTexts[keywordCount - 1].transform.parent.gameObject.SetActive(true);
//                 }
//             }
//         }

        var elapsedTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"RenderPanel 方法add keyword执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();

        //return textToShow.Replace(keywordFinalString, /*"<color=white>" + */keywordFinalString/* + "</color>"*/);
    }

    string BuildKeywords(string textToShow)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        // highlight keywords and build new text boxes
        if (visibleCard.card.GetCardTypes().Contains(MenuControl.Instance.GetLocalizedString("Treasure")))
        {
            //res.Add(MenuControl.Instance.GetLocalizedString("Treasure"));
            var keyword = "Treasure";
            string keyWordName = MenuControl.Instance.GetLocalizedString(keyword + "KeywordName", keyword);
            string keyWordDescription = MenuControl.Instance.GetLocalizedString(keyword + "KeywordDescription");
            AddKeyword(keyword, keyWordName, keyWordDescription);

        } 
        
        var elapsedTime = stopwatch.ElapsedMilliseconds;
     //   Debug.Log($"RenderPanel BuildKeywords 1 执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();

        List<string> newKeywords = new List<string>();

        // if (MenuControl.Instance.infoMenu.keywordsText.ContainsKey(textToShow))
        // {
        // }
        // else
        // {
        //     
        // // foreach (string keyword in MenuControl.Instance.infoMenu.keywordStrings)
        // // {
        // //     string keyWordName = MenuControl.Instance.GetLocalizedString(keyword + "KeywordName", keyword);
        // //     //string keyWordDescription = MenuControl.Instance.GetLocalizedString(keyword + "KeywordDescription");
        // //
        // //     if (keywords.Contains(keyword))
        // //     {
        // //         //Duplicate
        // //         
        // //     }
        // //     else
        //     {
        //        MenuControl.Instance.infoMenu.AddKeywordsText(textToShow);
        //     }
        // //}
        // }
        //
        
        // if (textToShow.Contains(keyWordName + "ed") && keywordCount < keywordTexts.Count)
        //             {
        //                 textToShow = textToShow.Replace(keyWordName + "ed", "<color=#86979c>" + keyWordName + "ed"+ "</color>");
        //                 newKeywords.Add(keyword);
        //                 //textToShow = AddKeyword(keyWordName + "ed", keyword, keyWordName, keyWordDescription, textToShow);
        //             }
        //             else if (textToShow.Contains(keyWordName + "d") && keywordCount < keywordTexts.Count)
        //             {
        //                 textToShow =  textToShow.Replace(keyWordName + "d", "<color=#86979c>" + keyWordName+ "d" + "</color>");
        //                 newKeywords.Add(keyword);
        //                 //textToShow = AddKeyword(keyWordName + "d", keyword, keyWordName, keyWordDescription, textToShow);
        //             }
        //             else if (textToShow.Contains(keyWordName) && keywordCount < keywordTexts.Count)
        //             {
        //                 var stop = new Stopwatch();
        //                 stop.Start();
        //                //  textToShow = Regex.Replace(textToShow, keyWordName, $"<color=#86979c>{keyWordName}</color>", RegexOptions.None);
        //                 textToShow = textToShow.Replace(keyWordName, "<color=#86979c>" + keyWordName + "</color>");
        //                 
        //                 elapsedTime = stopwatch.ElapsedMilliseconds;
        //                 Debug.Log($"RenderPanel replace 执行时间为: {elapsedTime} 毫秒");
        //                 stop.Restart();
        //                 
        //                 
        //                 newKeywords.Add(keyword);
        //                 //textToShow = AddKeyword(keyWordName, keyword, keyWordName, keyWordDescription, textToShow);
        //             }
        //             else if (textToShow.Contains(keyword) && keywordCount < keywordTexts.Count) //If translation failed
        //             {
        //                 textToShow = textToShow.Replace(keyword, "<color=#86979c>" + keyword + "</color>");
        //                 newKeywords.Add(keyword);
        //                 //textToShow = AddKeyword(keyword, keyword, keyWordName, keyWordDescription, textToShow);
        //             }
        //         }
        var keywordsData =  MenuControl.Instance.infoMenu.GetKeywordData(textToShow);
        
        textToShow = keywordsData.afterKeywords;
        var tempKeywords = keywordsData.keywords;

        foreach (var keyword in tempKeywords)
        {
            if (!keywords.Contains(keyword))
            {
                if (keyword == "Range" && keywords.Contains("Arcing"))
                {
                    continue;
                }
                newKeywords.Add((keyword));
            }
        }
        
        
        //remove keywords that are overlap 中文一个射程一个箭术射程
        if (newKeywords.Contains("Arcing") && newKeywords.Contains("Range"))
        {
            newKeywords.Remove("Range");
        }
        
        
        elapsedTime = stopwatch.ElapsedMilliseconds;
   //     Debug.Log($"RenderPanel BuildKeywords 2 执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();
        
        //newKeywords按照textToShow中出现的顺序排序
        newKeywords = newKeywords.OrderBy(x => textToShow.IndexOf(MenuControl.Instance.GetLocalizedString(x + "KeywordName", x))).ToList();
        
        elapsedTime = stopwatch.ElapsedMilliseconds;
    //    Debug.Log($"RenderPanel BuildKeywords 3 执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();
        foreach (var keyword in newKeywords)
        {
            var keywordName  =MenuControl.Instance.GetLocalizedString(keyword + "KeywordName", keyword);
            string keyWordDescription = MenuControl.Instance.GetLocalizedString(keyword + "KeywordDescription");
            AddKeyword(keyword, keywordName, keyWordDescription);
        }

        elapsedTime = stopwatch.ElapsedMilliseconds;
        Debug.Log($"RenderPanel BuildKeywords 4 执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();

        keywordHeader.SetActive(keywordCount > 0);
        for (int ii = keywordCount; ii < keywordTexts.Count; ii += 1)
        {
            keywordTexts[ii].transform.parent.gameObject.SetActive(false);
        }

        elapsedTime = stopwatch.ElapsedMilliseconds;
  //      Debug.Log($"RenderPanel BuildKeywords 5 执行时间为: {elapsedTime} 毫秒");
        stopwatch.Restart();
        return textToShow;
    }
}