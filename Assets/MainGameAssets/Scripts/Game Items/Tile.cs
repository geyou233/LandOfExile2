using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    public BoardMenu boardMenu;
    public Image tileImage;
    public GameObject indicator;
    public List<Effect> currentEffects = new List<Effect>();
    public List<Image> effectImages = new List<Image>();
    public List<Effect> GetEffectsWithTemplate(Effect template)
    {
        List<Effect> effects = new List<Effect>();
        if (template == null)
        {
            return effects;
        }
        foreach (Effect effect in currentEffects)
        {
            if (effect.originalTemplate.UniqueID == template.UniqueID)
            {
                effects.Add(effect);
            }
        }

        return effects;
    }

    public virtual void RemoveAllEffects()
    {
        for (int i = currentEffects.Count - 1; i >= 0; i--)
        {
            var effect = currentEffects[i];
            RemoveEffect(null,null,effect);
        }
    }
    public virtual void RemoveEffect(Card sourceCard, Ability ability, Effect effect)
    {
        if (currentEffects.Contains(effect))
        {
            // //Trigger Unit Removed Effect event
            // foreach (Trigger trigger in MenuControl.Instance.battleMenu.GetComponentsInChildren<Trigger>())
            // {
            //     try
            //     {
            //         trigger.UnitRemovedEffect(this, ability, effect);
            //     }
            //     catch (System.Exception e)
            //     {
            //         Debug.LogError(e);
            //     }
            // }

            currentEffects.Remove(effect);

            Destroy(effect.gameObject);
            effect.enabled = false;
        }
    }
    public virtual Effect ApplyEffect(Card sourceCard, Ability ability, Effect effectTemplate, int charges)
    {
        var tempEffect = Instantiate(effectTemplate, transform);
        tempEffect?.DoActionBeforeApply(sourceCard,this,charges);
        Destroy(tempEffect.gameObject);
        
        
        //if there is unit on tile, apply effect on unit instead of tile
        if (GetUnit() != null)
        {
            var unitEffectTemplate = effectTemplate.GetComponent<ApplyEffects>().templateEffects[0];
            return GetUnit().ApplyEffect(sourceCard, ability, unitEffectTemplate, charges);
        }
        
        Effect effect = null;
        if (effectTemplate.chargesStack && GetEffectsWithTemplate(effectTemplate).Count > 0)
        {
            effect = GetEffectsWithTemplate(effectTemplate)[0];
            effect.remainingCharges += charges;
            
            currentEffects.Remove(effect);
            currentEffects.Add(effect);
            
        }
        else
        {
            effect = Instantiate(effectTemplate, transform);

            
            currentEffects.Add(effect);
            effect.originalTemplate = effectTemplate;
            effect.remainingCharges = charges;
        }

        effect.lastApplyTurnCount = MenuControl.Instance.battleMenu.currentTurn;

        //Trigger Unit Applied Effect event
        foreach (Trigger trigger in MenuControl.Instance.battleMenu.GetComponentsInChildren<Trigger>())
        {
            try
            {
                trigger.TileAppliedEffect(this, ability, effect, charges);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }

        //Trigger Effect Charges Changed event
        // foreach (Trigger trigger in MenuControl.Instance.battleMenu.GetComponentsInChildren<Trigger>())
        // {
        //     try
        //     {
        //         trigger.EffectChargesChanged(ability, effect, 0);
        //     }
        //     catch (System.Exception e)
        //     {
        //         Debug.LogError(e);
        //     }
        // }


        return effect;
    }

    private void Awake()
    {
        //tileImage = GetComponent<Image>();
        boardMenu = MenuControl.Instance.battleMenu.boardMenu;
    }

    public Unit GetUnit()
    {
        return boardMenu.GetUnitOnTile(this);
    }

    public bool CanPlaceTrap()
    {
        return GetObstacle() == null;
    }
    public Obstacle GetObstacle()
    {
        return boardMenu.GetObstacleOnTile(this);
    }
    public WeatherTrap GetTrap()
    {
        return boardMenu.GetTrapOnTile(this);
    }

    public bool isBlocked()
    {
        //为了demon这个怪物不记得为什么加入了GetUnit().GetHP()>0的检查，然后之前检查的是currentHP，出了很多问题，因为GetHP()还要算上modifier
        return (GetUnit() != null && GetUnit().GetHP()>0)|| GetObstacle() != null;
    }

    public bool isMoveable()
    {
        return !isBlocked();
    }

    public bool isMoveableIgnoreFriends()
    {
        bool res = true;
        if (GetObstacle() != null)
        {
            res = false;
        }

        // if ((GetUnit() != null && GetUnit().GetHP() > 0))
        // {
        //     if (GetUnit().player != unit.player)
        //     {
        //         
        //         res = false;
        //     }
        // }
        //

        return res;
    }

    public bool isOnBorder()
    {
        return GetRow() == 0 || GetRow() == 3 || GetCol() == 0 || GetCol() == boardMenu.totalActiveTiles/4 -1;
    }

    public bool canPlaceObstacle()
    {
        return !isOnBorder() || GetRow() == 1||GetRow()==2;//中间两行可以放obstacle
    }

    public int GetRow()
    {
        return Mathf.CeilToInt((boardMenu.tiles.IndexOf(this) + 1) / (boardMenu.totalActiveTiles / 4f)) -1;
    }

    public int GetCol()
    {
        int returnInt = ((boardMenu.tiles.IndexOf(this) + 1) % (boardMenu.totalActiveTiles/4));
        if (returnInt == 0) return (boardMenu.totalActiveTiles / 4) - 1;
        return returnInt - 1;
    }

    public List<Tile> GetAdjacentTilesLinear(int range = 1)
    {
        List<Tile> tilesNearby = new List<Tile>();

        Tile tile = this;
        for (int ii = 0; ii < range; ii += 1)
        {
            tile = tile.GetTileUp();
            if (tile == null)
            {
                break;
            }
            tilesNearby.Add(tile);

        }
        tile = this;
        for (int ii = 0; ii < range; ii += 1)
        {
            tile = tile.GetTileDown();
            if (tile == null)
            {
                break;
            }
            tilesNearby.Add(tile);

        }
        tile = this;
        for (int ii = 0; ii < range; ii += 1)
        {
            tile = tile.GetTileLeft();
            if (tile == null)
            {
                break;
            }
            tilesNearby.Add(tile);

        }
        tile = this;
        for (int ii = 0; ii < range; ii += 1)
        {
            tile = tile.GetTileRight();
            if (tile == null)
            {
                break;
            }
            tilesNearby.Add(tile);

        }

        return tilesNearby;
    }

    public Tile GetTileUp(int spaces = 1)
    {
        int returnInt = boardMenu.tiles.IndexOf(this);
        for (int ii = 0; ii < spaces; ii += 1)
        {
            returnInt -= (boardMenu.totalActiveTiles / 4);
        }

        if (returnInt < 0) return null;

        return boardMenu.tiles[returnInt];
    }

    public Tile GetTileDown(int spaces = 1)
    {
        int returnInt = boardMenu.tiles.IndexOf(this);
        for (int ii = 0; ii < spaces; ii += 1)
        {
            returnInt += (boardMenu.totalActiveTiles / 4);
        }

        if (returnInt >= boardMenu.totalActiveTiles) return null;

        return boardMenu.tiles[returnInt];
    }

    public Tile GetTileLeft(int spaces = 1)
    {
        int col = GetCol();

        for (int ii = 0; ii < spaces; ii += 1)
        {
            col -= 1;
        }

        if (col < 0) return null;

        return boardMenu.tiles[(GetRow() * (boardMenu.totalActiveTiles/4)) + col];
    }

    public Tile GetTileRight(int spaces = 1)
    {
        int col = GetCol();

        for (int ii = 0; ii < spaces; ii += 1)
        {
            col += 1;
        }

        if (col >= boardMenu.totalActiveTiles/4) return null;

        return boardMenu.tiles[(GetRow() * (boardMenu.totalActiveTiles/4)) + col];
    }

    public Color redColor = new Color(0f / 255f, 185f / 255f, 85f / 255f,0.75f); // new Color(190f / 255, 87f / 255, 85f / 255,0.75f);
    public Color greenColor = new Color(0f / 255f, 185f / 255f, 85f / 255f,0.75f);
    public Color whiteColor = new Color(1f, 1f, 1f, 0.75f);
    
    GameObject tauntIndicator = null;
    public void RenderTile()
    {
        tileImage.color = Color.clear;
        GameObject newIndicator = null;
        GetComponent<CanvasGroup>().alpha = 1f;
        HideTauntIndicator();
        if (MenuControl.Instance.battleMenu.hoveringEndTurn)
        {

            if (GetUnit() != null && GetUnit().player == MenuControl.Instance.battleMenu.player1)
            {
                if (GetUnit().CanMove() || GetUnit().CanAct())
                {
                    tileImage.color = redColor;
                }
            }
        }
        else if (MenuControl.Instance.battleMenu.selectedVisibleCard != null)
        {
            bool canTarget = false;
            if (MenuControl.Instance.battleMenu.targetTiles.Count > 0)
            {
                List<Tile> newTargets = new List<Tile>();
                newTargets.AddRange(MenuControl.Instance.battleMenu.targetTiles);
                newTargets.Add(this);
                if (((Castable)MenuControl.Instance.battleMenu.selectedVisibleCard.card).CanTarget(newTargets))
                {
                    canTarget = true;
                }
            }
            else if (MenuControl.Instance.battleMenu.selectedVisibleCard.card.CanTarget(this))
            {
                canTarget = true;

            }

            if (canTarget)
            {
                if (GetUnit() != null && GetUnit() != MenuControl.Instance.battleMenu.selectedVisibleCard.card)
                {
                    tileImage.color = redColor;
                    if (MenuControl.Instance.battleMenu.selectedVisibleCard.card.player != GetUnit().player)
                    {
                        newIndicator = boardMenu.attackIndicator;
                    }
                    else
                    {

                        newIndicator = boardMenu.buffIndicator;
                    }
                }
                else
                {
                    tileImage.color = greenColor;
                    // if (MenuControl.Instance.battleMenu.selectedVisibleCard.card.player == MenuControl.Instance.battleMenu.playerAI)
                    // {
                    //     tileImage.color = ;
                    // }
                    newIndicator = boardMenu.moveIndicator;
                }

                if (MenuControl.Instance.battleMenu.selectedVisibleCard.card.cardTargetType == CardTargetType.BuffOnly)
                {
                    newIndicator = boardMenu.buffIndicator;
                }
                //tileImage.color = new Color(tileImage.color.r, tileImage.color.g, tileImage.color.b, 0.7f);
            }
            
            
            //taunted
            var unit = MenuControl.Instance.battleMenu.selectedVisibleCard.card as Unit;
            if (unit && GetUnit() != null && GetUnit() != unit && unit.GetTile() != null &&
                unit.IsAdjacentToUnit(GetUnit()) && GetUnit().IsTaunt() && unit.player!= GetUnit().player)
            {
                ShowTauntIndicator();
            }

        }
        else if (MenuControl.Instance.battleMenu.hoveredVisibleCard != null)
        {
            
            

            if (MenuControl.Instance.battleMenu.hoveredVisibleCard.card.CanTarget(this))
            {
                if (GetUnit() != null && GetUnit() != MenuControl.Instance.battleMenu.hoveredVisibleCard.card)
                {
                    tileImage.color = Color.red;
                    if (MenuControl.Instance.battleMenu.hoveredVisibleCard.card.player != GetUnit().player)
                    {
                        newIndicator = boardMenu.attackIndicator;
                    }
                    else
                    {

                        newIndicator = boardMenu.buffIndicator;
                    }
                }
                else
                {
                    tileImage.color = greenColor;
                    // if (MenuControl.Instance.battleMenu.hoveredVisibleCard.card.player == MenuControl.Instance.battleMenu.playerAI)
                    // {
                    //     tileImage.color = new Color(0f / 255f, 185f / 255f, 0f / 255f);
                    // }
                    newIndicator = boardMenu.moveIndicator;
                }
                
                if (MenuControl.Instance.battleMenu.hoveredVisibleCard.card.cardTargetType == CardTargetType.BuffOnly)
                {
                    newIndicator = boardMenu.buffIndicator;
                }
               // tileImage.color = new Color(tileImage.color.r, tileImage.color.g, tileImage.color.b, 0.7f);
            }
           
            //taunted
            var unit = MenuControl.Instance.battleMenu.hoveredVisibleCard.card as Unit;
            if (unit && GetUnit() != null && GetUnit() != unit && unit.GetTile() != null &&
                unit.IsAdjacentToUnit(GetUnit()) && GetUnit().IsTaunt() && unit.player!= GetUnit().player)
            {
                ShowTauntIndicator();
            }
            
        }

        if (newIndicator == null && indicator != null)
        {
            Destroy(indicator);
            indicator = null;
        }
        else if (newIndicator != null)
        {
            if (indicator == null || newIndicator.GetComponentInChildren<Image>().sprite != indicator.GetComponentInChildren<Image>().sprite)
            {
                RenderTileIndicator(newIndicator);
            }
        }

        RenderTileEffect();
    }

    void ShowTauntIndicator()
    {
        if (!tauntIndicator)
        {
            tauntIndicator = Instantiate(boardMenu.tauntIndicator, MenuControl.Instance.battleMenu.visibleBoardCardsHolderIndicator) as GameObject;
            tauntIndicator.transform.position = transform.position;
            tauntIndicator.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            LeanTween.color(tauntIndicator.GetComponent<RectTransform>(), whiteColor, 1f).setLoopPingPong(-1).setEaseOutSine();
        }
        tauntIndicator.SetActive(true);
    }

    void HideTauntIndicator()
    {
        if (tauntIndicator)
        {
            tauntIndicator.SetActive(false);
        }
    }

    void scaleEffectSprite(Transform effect)
    {
        
        effect.transform.localScale = Vector3.zero;
        effect.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
    }

    private List<string> renderedEffect = new List<string>();
    
    void RenderTileEffect()
    {
        int i = 0;
        List<string> newRenderedEffect = new List<string>();
        for (; i < currentEffects.Count; i++)
        {
            if (i >= effectImages.Count)
            {
                break;
            }
            
            
            
            effectImages[i].gameObject.SetActive(true);
            effectImages[i].sprite = MenuControl.Instance.csvLoader.buffSprite(currentEffects[i].GetChineseName());
            newRenderedEffect.Add(currentEffects[i].UniqueID);

            if (!renderedEffect.Contains(currentEffects[i].UniqueID))
            {
                scaleEffectSprite(effectImages[i].transform);
            }
            
            i++;
        }

        if (i < effectImages.Count)
        {
            if (GetTrap() != null)
            {
                effectImages[i].gameObject.SetActive(true);
                effectImages[i].sprite = MenuControl.Instance.csvLoader.buffSprite(GetTrap().GetChineseName());
                
                newRenderedEffect.Add(GetTrap().UniqueID);
                
                
                if (!renderedEffect.Contains(GetTrap().UniqueID))
                {
                    scaleEffectSprite(effectImages[i].transform);
                }
                i++;
            }
        }
        
        for (; i < effectImages.Count; i++)
        {
            
            effectImages[i].gameObject.SetActive(false);
        }

        renderedEffect = newRenderedEffect;
    }

    void RenderTileIndicator(GameObject prefab)
    {
        if (indicator != null)
        {
            Destroy(indicator);
        }

        indicator = Instantiate(prefab, MenuControl.Instance.battleMenu.visibleBoardCardsHolderIndicator) as GameObject;
        indicator.transform.position = transform.position;
        indicator.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        LeanTween.color(indicator.GetComponent<RectTransform>(), whiteColor, 1f).setLoopPingPong(-1).setEaseOutSine();
    }

    public virtual List<Tile> GetDiagonalTiles()
    {
        List<Tile> diagonalTiles = new List<Tile>();
        if (GetTileRight() != null && GetTileRight().GetTileUp() != null)
        {
            if (!diagonalTiles.Contains(GetTileRight().GetTileUp()))
            {
                diagonalTiles.Add(GetTileRight().GetTileUp());
            }
        }
        if (GetTileRight() != null && GetTileRight().GetTileDown() != null)
        {
            if (!diagonalTiles.Contains(GetTileRight().GetTileDown()))
            {
                diagonalTiles.Add(GetTileRight().GetTileDown());
            }
        }
        if (GetTileLeft() != null && GetTileLeft().GetTileUp() != null)
        {
            if (!diagonalTiles.Contains(GetTileLeft().GetTileUp()))
            {
                diagonalTiles.Add(GetTileLeft().GetTileUp());
            }
        }
        if (GetTileLeft() != null && GetTileLeft().GetTileDown() != null)
        {
            if (!diagonalTiles.Contains(GetTileLeft().GetTileDown()))
            {
                diagonalTiles.Add(GetTileLeft().GetTileDown());
            }
        }
        if (GetTileUp() != null && GetTileUp().GetTileRight() != null)
        {
            if (!diagonalTiles.Contains(GetTileUp().GetTileRight()))
            {
                diagonalTiles.Add(GetTileUp().GetTileRight());
            }
        }
        if (GetTileUp() != null && GetTileUp().GetTileLeft() != null)
        {
            if (!diagonalTiles.Contains(GetTileUp().GetTileLeft()))
            {
                diagonalTiles.Add(GetTileUp().GetTileLeft());
            }
        }
        if (GetTileDown() != null && GetTileDown().GetTileRight() != null)
        {
            if (!diagonalTiles.Contains(GetTileDown().GetTileRight()))
            {
                diagonalTiles.Add(GetTileDown().GetTileRight());
            }
        }
        if (GetTileDown() != null && GetTileDown().GetTileLeft() != null)
        {
            if (!diagonalTiles.Contains(GetTileDown().GetTileLeft()))
            {
                diagonalTiles.Add(GetTileDown().GetTileLeft());
            }
        }

        return diagonalTiles;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
            return;
        else if (eventData.button == PointerEventData.InputButton.Right)
            return;

        MenuControl.Instance.battleMenu.ClickOnTile(this);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!GetUnit())
        {
            MenuControl.Instance.infoMenu.ShowInfo(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MenuControl.Instance.infoMenu.HideMenu(this);
    }
}
