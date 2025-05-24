using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroTalentCell : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image image;
    public Text nameDisplay;
    public Text descDisplay;
    public Card talent;
    public void init(string n, string desc, Sprite ima)
    {
        //image.sprite = ima;
        nameDisplay.text = n;
        descDisplay.text = desc;
        
        image.sprite = ima;
        //update sprite dynamically
        // var talentMap = MenuControl.Instance.csvLoader.chineseNameToTalentMap;
        // if (!talentMap.ContainsKey(n))
        // {
        //     Debug.LogError(n+" is not in chineseNameToTalentMap");
        // }
        // else
        // {
        //     var talentInfo = MenuControl.Instance.csvLoader.chineseNameToTalentMap[n];
        //     var sprite = ResourceManager.LoadResouce<Sprite>("Art/icon_talent/" + talentInfo.spriteName);
        //     if (sprite == null)
        //     {
        //         
        //         Debug.LogError(sprite +" for "+n+" is not in Art/icon_talent/");
        //     }
        //     image.sprite = sprite;
        // }
    }

    public void init(Card talent)
    {
        this.talent = talent;
        init(talent.GetName(),CardDescriptionPanel.DescriptionText(talent,null,null), MenuControl.Instance.csvLoader.talentSprite(talent.UniqueID));
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        MenuControl.Instance.infoMenu.ShowInfo(talent, transform);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
            return;
        else if (eventData.button == PointerEventData.InputButton.Right)
            return;

        //if (!disableInteraction)
        {
            // MenuControl.Instance.infoMenu.ShowInfo(talent, transform);
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
            return;
        else if (eventData.button == PointerEventData.InputButton.Right)
            return;

        //if (!disableInteraction)
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
            MenuControl.Instance.infoMenu.HideMenu();
            //GetComponentInParent<BasicMenu>().DeSelectVisibleCard(this, false);
        }
    }
}
