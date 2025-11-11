using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class SpecialChallengeSkillSet
{
    public List<Skill> skills = new List<Skill>();
}

public class AdventureItemEncounter : AdventureItem
{
    private int randMapSeetingValue = -1;

    public void PickRandomSettingValue()
    {
        randMapSeetingValue =extraMapSettings.Count==0?-1: Random.Range(-1, extraMapSettings.Count);
    }
    public int boardColumns = 4;

    [HideInInspector]
    public int BoardColumns
    {
        get
        {
            if (randMapSeetingValue == -1 || randMapSeetingValue >= extraMapSettings.Count)
            {
                return boardColumns;
            }

            return extraMapSettings[randMapSeetingValue].boardColumns;
        }
    }

    public bool player1Starts;
    

    [HideInInspector]
    public bool Player1Starts
    {
        get
        {
            if (randMapSeetingValue == -1 || randMapSeetingValue >= extraMapSettings.Count)
            {
                return player1Starts;
            }

            return extraMapSettings[randMapSeetingValue].player1Starts;
        }
    }

    public bool randomStartingPositions = true;
    
    [HideInInspector]
    public bool RandomStartingPositions
    {
        get
        {
            if (randMapSeetingValue == -1 || randMapSeetingValue >= extraMapSettings.Count)
            {
                return randomStartingPositions;
            }

            return extraMapSettings[randMapSeetingValue].randomStartingPositions;
        }
    }
    public int player1StartingPos = 15;
    [HideInInspector]
    public int Player1StartingPos
    {
        get
        {
            if (randMapSeetingValue == -1 || randMapSeetingValue >= extraMapSettings.Count)
            {
                return player1StartingPos;
            }

            return extraMapSettings[randMapSeetingValue].player1StartingPos;
        }
    }
    public int playerAIStartingPos = 0;
    
    [HideInInspector]
    public int PlayerAIStartingPos
    {
        get
        {
            if (randMapSeetingValue == -1 || randMapSeetingValue >= extraMapSettings.Count)
            {
                return playerAIStartingPos;
            }

            return extraMapSettings[randMapSeetingValue].playerAIStartingPos;
        }
    }

   [HideInInspector] public MapEventInfo eventInfo;

    public List<Card> allOwnedCards = new List<Card>();
    public List<string> signPosts = new List<string>();
    public bool shufflePlayerAIDeck = true;
    public int initialMana;
    public int drawsPerTurn = 5;
    public int level;
    public int xp;
    public int extraIntFlag;
    public bool isBoss;
    public bool isElite=>signPosts.Contains("Elite");
    public bool cannotBeSkipped;
    public List<Card> rewardCards = new List<Card>();
    public bool forceBattle = false;
    public List<Card> cardsInPlay = new List<Card>();
    public List<int> positionsInPlay = new List<int>();
    
    
    [HideInInspector]
    public List<Card> CardsInPlay
    {
        get
        {
            if (randMapSeetingValue == -1 || randMapSeetingValue >= extraMapSettings.Count)
            {
                return cardsInPlay;
            }

            return extraMapSettings[randMapSeetingValue].cardsInPlay;
        }
    }
    [HideInInspector]
    public List<int> PositionsInPlay
    {
        get
        {
            if (randMapSeetingValue == -1 || randMapSeetingValue >= extraMapSettings.Count)
            {
                return positionsInPlay;
            }

            return extraMapSettings[randMapSeetingValue].positionsInPlay;
        }
    }


    public bool overridePlayer;
    public List<Card> playerCards = new List<Card>();
    public Hero playerHero;
    public int playerInitalMana;
    public int playerDrawsPerTurn = 5;
    public bool shufflePlayerDeck = true;

    public GameObject battleMapOverridePrefab;
    public bool randomBattleMap;
    public AIControl aiControl;

    public int minObstacleCount = 0;
    public int maxObstacleCount = 0;
    
    public int MinObstacleCount
    {
        get
        {
            if (randMapSeetingValue == -1 || randMapSeetingValue >= extraMapSettings.Count)
            {
                return minObstacleCount;
            }

            return extraMapSettings[randMapSeetingValue].minObstacleCount;
        }
    }
    public int MaxObstacleCount
    {
        get
        {
            if (randMapSeetingValue == -1 || randMapSeetingValue >= extraMapSettings.Count)
            {
                return maxObstacleCount;
            }

            return extraMapSettings[randMapSeetingValue].maxObstacleCount;
        }
    }
    
    [SerializeField]
    public List<SpecialChallengeSkillSet> specialChallengeSkills = new List<SpecialChallengeSkillSet>();
     public bool specialChallengeSkillApplyOnCardsInPlay;


     public List<EncounterMapSetting> extraMapSettings;
    // [HideInInspector]
    // public int specialChallengeIndex = -1;
    // [HideInInspector]
    // public EncounterSpeicallChallengeRewardType specialChallengeRewardIndex;
    
    

    public enum EncounterSpeicallChallengeRewardType{Upgrade,Delete,ExtraStone,None}
    public enum SpecialChallengeType
    {
        Skill,Cards
    }
    public AdventureItemInfo GenerateSpecialChallenge()
    {
        int res = 0;
        int selectType = Random.Range(0, 2);
        //selectType = 0;
        res = selectType * 100;

        if (selectType == 0)//skill
        {
            int selectId = Random.Range(0, specialChallengeSkills.Count);
            res += selectId;
        }
        else // more card in hands
        {
            
        }

        var specialChallengeIndex = res;
        //random of enum EncounterSpeicallChallengeRewardType 
        
        //pick index based on a list of probablity
        EncounterSpeicallChallengeRewardType specialChallengeRewardIndex = (EncounterSpeicallChallengeRewardType)RandomUtil.RandomBasedOnProbability(MenuControl.Instance.adventureMenu.specialChallengeProbability);

        var info =  new AdventureItemInfo(specialChallengeIndex,specialChallengeRewardIndex);
        return info;
    }

    public SpecialChallengeType GetSpecialChallengeType(AdventureItemInfo info)
    {
        if (info.specialChallengeIndex == -1)
        {
            GenerateSpecialChallenge();
        }
        
        int type =info. specialChallengeIndex / 100;
        return (SpecialChallengeType)type;
    }

    public int GetSpecialChallengeInnerIndex(AdventureItemInfo info)
    {
        return info.specialChallengeIndex % 100;
    }

    public int GetAdjustedXP()
    {
        int finalXP = xp;

        for (int ii = 0; ii < MenuControl.Instance.CountOfCardsInList(MenuControl.Instance.levelUpMenu.extraXPTalent, MenuControl.Instance.levelUpMenu.variableTalentsAcquired); ii += 1)
        {
            finalXP = Mathf.CeilToInt(finalXP * 1.15f);
        }

        if (MenuControl.Instance.heroMenu.ascensionMode >= 3)
        {
            finalXP = Mathf.CeilToInt(finalXP * 0.85f);
        }

        return finalXP;
    }


    public override void PerformItem(int index)
    {
        MenuControl.Instance.eventMenu.ShowEncounterEvent(forceBattle,index);
    }

   
    public override string GetDescription()
    {
        string extraString = "";
        if (signPosts.Count > 0)
        {
            extraString += " - ";
            foreach (string signpost in signPosts)
            {
                extraString += "<color=#F8D69E>"+MenuControl.Instance.GetLocalizedString(signpost) + "</color>, ";
            }
            extraString  = extraString.Substring(0, extraString.Length - 2);
        }

        string returnString = "<color=#F8D69E>Lv.: " + level + " XP: " + GetAdjustedXP() + extraString + "</color>\n\n" + MenuControl.Instance.GetLocalizedString(UniqueID + "CardDescription");

        if (isBoss) return "<color=red>Boss</color> - " + returnString;

        return returnString;
    }

    public Hero GetHero()
    {
        foreach (Card card in allOwnedCards)
        {
            if (card is Hero)
            {
                return (Hero)card;
            }
        }
        return null;
    }

    public override Sprite GetSprite()
    {
        if (base.GetSprite() != null) return base.GetSprite();

        foreach (Card card in allOwnedCards)
        {
            if (card is Hero)
            {
                return card.GetSprite();
            }
        }

        return null;
    }

    public List<Card> GetPlayerCardsOwned()
    {
        List<Card> cards = new List<Card>();
        if (overridePlayer)
        {
            cards.AddRange(playerCards);
        }
        else
        {
            foreach (var card in MenuControl.Instance.heroMenu.cardsOwned)
            {
                if (card.isPotion)
                {
                    
                }
                else
                {
                    cards.Add(card);
                }
            }
            
            //cards.AddRange(MenuControl.Instance.heroMenu.cardsOwned);
        }
        return cards;
    }

    public Hero GetPlayerHero()
    {
        if (overridePlayer)
        {
            return playerHero;
        }
        return MenuControl.Instance.heroMenu.hero;
    }
    public int GetPlayerBaseDrawsPerTurn()
    {
        if (overridePlayer)
        {
            return playerDrawsPerTurn;
        }
        return MenuControl.Instance.heroMenu.drawsPerTurn;
    }
    public int GetPlayerInitialMana()
    {
        if (overridePlayer)
        {
            return playerInitalMana;
        }
        return MenuControl.Instance.heroMenu.initialMana;
    }

    int getMapBGPrefabId()
    {
        switch (BoardColumns)
        {
            case 4:
                return 0;
            case 5:
                    return 1;
            case 6:
                return 2;
            case 8:
                return 3;
            case 10:
                return 4;
            default:
                Debug.LogError("Invalid boardColumns: " + BoardColumns);
                return 0;
        }
    }
    
    public GameObject GetBattleMapPrefab()
    {
        if (battleMapOverridePrefab != null) { return battleMapOverridePrefab; }

        if (randomBattleMap)
        {
            int randomInt = UnityEngine.Random.Range(0, 3) + 2;

            if (MenuControl.Instance.useAlternateSprites)
            {
                if (MenuControl.Instance.areaMenu.allAreas[randomInt].alternateBattleMapBGPrefabs.Count > 0)
                {
                    return MenuControl.Instance.areaMenu.allAreas[randomInt].alternateBattleMapBGPrefabs[getMapBGPrefabId()];
                }
            }

            return MenuControl.Instance.areaMenu.allAreas[randomInt].battleMapBGPrefabs[getMapBGPrefabId()];
        }

        if (MenuControl.Instance.useAlternateSprites)
        {
            if (MenuControl.Instance.areaMenu.currentArea.alternateBattleMapBGPrefabs.Count > 0)
            {
                return MenuControl.Instance.areaMenu.currentArea.alternateBattleMapBGPrefabs[getMapBGPrefabId()];
            }
        }

        return MenuControl.Instance.areaMenu.currentArea.battleMapBGPrefabs[getMapBGPrefabId()];
    }
}
