using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Sinbad;
using Spine.Unity;
using UnityEngine;

[Serializable]
public class MapEventInfo
{
    public int index;
    public int level;
    public int eventId;
    public string eventType;
    public int enemyGroup;
    public string storyType;
    public Vector2 position;
    public int positionGroup;
    public string unlockConditionType;
    public List<int> unlockConditionParam;
    public string text;
}

public class PlayerCardInfo
{
    public string uid;
    public string requireUnlock;
    public int hero;
    public int path;
    public List<string> school;
    // public int level;
    // public int cost;
    // public int attack;
    // public int hp;
    // public float attackTime;

    public string version;
    //  public string description;
}

public class StoryEventInfo
{
    public string questName;
    public string questEventId;
    public string spriteName;
    public List<string> options;
    public string optionsShowLogic;
}

public class ChineseNameToTalentMapInfo
{
    public string chineseName;
    public string spriteName;
    public int star;
}

public class NameToChineseName
{
    public string Key;
    public string Chinese;
}

public class CardUpgradeInfo
{
    public string cardUniqueId;
    public string upgradeCardUniqueId;
}

public class ChineseNameToEventMapInfo : ChineseNameToTalentMapInfo
{
    public string iconName;
}

public class PositionGroupInfo
{
    public int group;

    public Vector2 position;
}

public class EnemyGroupInfo
{
    public int enemyGroup;
    public string UID;
    //public string enemyChineseName;
    public string eventChat;
}


public class LevelUpUnlockCardsInfo
{
    public int level;
    public int experienceToNextLevel;
    public int cardCountToUnlock;
    public int totalExperience;
}

public class DialogueInBattleInfo
{
    public string speakerUid;
   // public string name;
   // public string from;
    public string dialogueId;
    public string triggerType;
    public List<string> args;
    public List<int> heroIds;
 
}

public class CSVLoader : MonoBehaviour
{
    [HideInInspector] public List<MapEventInfo> mapEventInfos;
    [HideInInspector] public Dictionary<int, MapEventInfo> mapEventInfoByEventId = new Dictionary<int, MapEventInfo>();

    [HideInInspector]
    public Dictionary<string, StoryEventInfo> storyEventInfoByEventId = new Dictionary<string, StoryEventInfo>();

    [HideInInspector]
    public Dictionary<int, List<PositionGroupInfo>> positionGroupsByGroupId =
        new Dictionary<int, List<PositionGroupInfo>>();

    [HideInInspector]
    public Dictionary<int, List<EnemyGroupInfo>> EnemyGroupsByGroupId = new Dictionary<int, List<EnemyGroupInfo>>();

    [HideInInspector] public List<string> EnemyGroupsChineseName = new List<string>();
    [HideInInspector] public Dictionary<string, int> EnemyEncounterIdToGroupId = new Dictionary<string, int>();

    [HideInInspector]
    public Dictionary<string, ChineseNameToTalentMapInfo> chineseNameToTalentMap =
        new Dictionary<string, ChineseNameToTalentMapInfo>();

    [HideInInspector]
    public Dictionary<string, ChineseNameToEventMapInfo> chineseNameToEventMap =
        new Dictionary<string, ChineseNameToEventMapInfo>();

    [HideInInspector]
    public Dictionary<string, ChineseNameToTalentMapInfo> chineseNameToBuffMap =
        new Dictionary<string, ChineseNameToTalentMapInfo>();
    //
    // [HideInInspector]
    // public Dictionary<string, ChineseNameToTalentMapInfo> chineseNameToEnemyCardMap =
    //     new Dictionary<string, ChineseNameToTalentMapInfo>();

    [HideInInspector] public Dictionary<string, ChineseNameToTalentMapInfo> chineseNameToAchievementMap =
        new Dictionary<string, ChineseNameToTalentMapInfo>();

    [HideInInspector] public Dictionary<string, ChineseNameToTalentMapInfo> chineseNameToPlayerCardMap =
        new Dictionary<string, ChineseNameToTalentMapInfo>();

    [HideInInspector] public Dictionary<int, HashSet<string>> classTypeToSchool = new Dictionary<int, HashSet<string>>();
    [HideInInspector] public Dictionary<string, ChineseNameToTalentMapInfo> chineseNameToStoryEventMap =
        new Dictionary<string, ChineseNameToTalentMapInfo>();

    [HideInInspector]
    public Dictionary<string, string> cardUniqueIdToDowngradeCardUniqueId = new Dictionary<string, string>();
    [HideInInspector]
    public Dictionary<string, List<string>> cardUniqueIdToUpgradeCardUniqueId = new Dictionary<string,  List<string>>();

    [HideInInspector]
    public Dictionary<string, PlayerCardInfo> uniqueIdToPlayerCardInfo = new Dictionary<string, PlayerCardInfo>();

    
    [HideInInspector] public Dictionary<string, string> nameToChineseName = new Dictionary<string, string>();


    [HideInInspector]
    public Dictionary<int, LevelUpUnlockCardsInfo> levelToUnlockCardsInfoMap =
        new Dictionary<int, LevelUpUnlockCardsInfo>();
    
    [HideInInspector]
    public Dictionary<string, DialogueInBattleInfo> dialogueInBattleInfoDict =
        new Dictionary<string, DialogueInBattleInfo>();

    private Dictionary<string, PlayerCardInfo> playerCardInfoDict = new Dictionary<string, PlayerCardInfo>();
    // Start is called before the first frame update
    void Awake()
    {
        mapEventInfos = CsvUtil.LoadObjects<MapEventInfo>("mapEvent");
        for (int i = 0; i < mapEventInfos.Count; i++)
        {
            mapEventInfos[i].index = i;
            mapEventInfoByEventId[mapEventInfos[i].eventId] = mapEventInfos[i];
        }

        var nameToChienseNameInfos = CsvUtil.LoadObjects<NameToChineseName>("nameToChineseName");
        foreach (var info in nameToChienseNameInfos)
        {
            nameToChineseName[info.Key] = info.Chinese;
        }

        var storyEventInfo = CsvUtil.LoadObjects<StoryEventInfo>("storyEvent");
        foreach (var info in storyEventInfo)
        {
            storyEventInfoByEventId[info.questEventId] = info;
        }

        var positionInfos = CsvUtil.LoadObjects<PositionGroupInfo>("positionGroup");
        foreach (var info in positionInfos)
        {
            if (!positionGroupsByGroupId.ContainsKey(info.group))
            {
                positionGroupsByGroupId[info.group] = new List<PositionGroupInfo>();
            }

            positionGroupsByGroupId[info.group].Add(info);
        }

        // var chineseNameToEncounterNameMapInfos =
        //     CsvUtil.LoadObjects<ChineseNameToEncounterNameMapInfo>("chineseNameToEncounterNameMap");
        // foreach (var info in chineseNameToEncounterNameMapInfos)
        // {
        //     ChineseNameToEncounterNameMap[info.chineseName] = info.encounterName;
        //     EncounterNameToChineseNameMap[info.encounterName] = info.chineseName;
        // }


        var enemyGroupInfos = CsvUtil.LoadObjects<EnemyGroupInfo>("enemyGroup");
        foreach (var info in enemyGroupInfos)
        {
            if (!EnemyGroupsByGroupId.ContainsKey(info.enemyGroup))
            {
                EnemyGroupsByGroupId[info.enemyGroup] = new List<EnemyGroupInfo>();
            }

            EnemyGroupsByGroupId[info.enemyGroup].Add(info);
            EnemyGroupsChineseName.Add(info.UID);

            // if (!ChineseNameToEncounterNameMap.ContainsKey(info.UID))
            // {
            //     Debug.LogError(info.UID + " is not in ChineseNameToEncounterNameMap");
            // }

            if (info.UID!=null && info.UID.Length>0)
            {
                
                //var encounterId = ChineseNameToEncounterNameMap[info.UID];
                EnemyEncounterIdToGroupId[info.UID] = info.enemyGroup;
            }
        }


        var chineseNameToTalentInfos = CsvUtil.LoadObjects<ChineseNameToTalentMapInfo>("chineseNameToTalentMap");
        foreach (var info in chineseNameToTalentInfos)
        {
            chineseNameToTalentMap[info.chineseName] = info;
        }

        var chineseNameToBuffInfos = CsvUtil.LoadObjects<ChineseNameToTalentMapInfo>("chineseNameToBuffMap");
        foreach (var info in chineseNameToBuffInfos)
        {
            chineseNameToBuffMap[info.chineseName] = info;
        }

        var chineseNameToEventInfos = CsvUtil.LoadObjects<ChineseNameToEventMapInfo>("chineseNameToEventMap");
        foreach (var info in chineseNameToEventInfos)
        {
            chineseNameToEventMap[info.chineseName] = info;
        }

        var chineseNameToStoryEventInfos =
            CsvUtil.LoadObjects<ChineseNameToTalentMapInfo>("chineseNameToStoryEventMap");
        foreach (var info in chineseNameToStoryEventInfos)
        {
            chineseNameToStoryEventMap[info.chineseName] = info;
        }


        var chineseNameToPlayerCardInfos =
            CsvUtil.LoadObjects<ChineseNameToTalentMapInfo>("chineseNameToPlayerCardMap");
        foreach (var info in chineseNameToPlayerCardInfos)
        {
            chineseNameToPlayerCardMap[info.chineseName] = info;
        }
        
        

        // var chineseNameToEnemyCardInfos = CsvUtil.LoadObjects<ChineseNameToTalentMapInfo>("chineseNameToEnemyCardMap");
        // foreach (var info in chineseNameToEnemyCardInfos)
        // {
        //     chineseNameToEnemyCardMap[info.chineseName] = info;
        // }

        var chineseNameToAchievementInfos =
            CsvUtil.LoadObjects<ChineseNameToTalentMapInfo>("chineseNameToAchievementMap");
        foreach (var info in chineseNameToAchievementInfos)
        {
            chineseNameToAchievementMap[info.chineseName] = info;
        }


        var levelUpUnlockCards = CsvUtil.LoadObjects<LevelUpUnlockCardsInfo>("levelUpUnlockCards");

        foreach (var info in levelUpUnlockCards)
        {
            levelToUnlockCardsInfoMap[info.level] = info;
        }

        var playerCardInfos = CsvUtil.LoadObjects<PlayerCardInfo>("playerCards");

        foreach (var info in playerCardInfos)
        {
            if (info.uid != null && info.uid.Length != 0)
            {
                playerCardInfoDict[info.uid] = info;
                uniqueIdToPlayerCardInfo[info.uid] = info;
                var hero = info.hero - 1;
                if (!classTypeToSchool.ContainsKey(hero))
                {
                    classTypeToSchool[hero] = new HashSet<string>();
                }

                if (info.school != null && info.school.Count > 0)
                {
                    
                    foreach (var school in info.school)
                    {
                        classTypeToSchool[hero].Add(school);

                    }
                }
            }
        }
        
        var dialogueInBattleInfos = CsvUtil.LoadObjects<DialogueInBattleInfo>("dialogueInBattle");
        foreach (var info in dialogueInBattleInfos)
        {
            dialogueInBattleInfoDict[info.dialogueId] = info;
            bool isDefined = Enum.IsDefined(typeof(DialogueInBattleTriggerType), info.triggerType);
            if (!isDefined)
            {
                Debug.LogError(( $"{info.dialogueId} {info.triggerType} is not defined"));
            }
        }
        
        
        var upgradeCardInfos = CsvUtil.LoadObjects<CardUpgradeInfo>("upgradeCards");
        foreach (var info in upgradeCardInfos)
        {
            if (info.upgradeCardUniqueId == null)
            {
                //Debug.LogError("?");
                continue;
            }

            if (playerCardInfoDict.ContainsKey(info.cardUniqueId) &&
                playerCardInfoDict.ContainsKey(info.upgradeCardUniqueId))
            {
                if (cardUniqueIdToDowngradeCardUniqueId.ContainsKey(info.upgradeCardUniqueId))
                {
                    Debug.LogError($"多个卡牌{info.cardUniqueId} {cardUniqueIdToDowngradeCardUniqueId[info.upgradeCardUniqueId]}升级到同一个卡牌 {info.upgradeCardUniqueId}");
                }
            
                // if (cardUniqueIdToUpgradeCardUniqueId.ContainsKey(info.cardUniqueId))
                // {
                //     Debug.LogError($"一个卡牌{info.cardUniqueId} 升级到多个卡牌 {info.upgradeCardUniqueId} {cardUniqueIdToUpgradeCardUniqueId[ info.cardUniqueId] }");
                // }
                cardUniqueIdToDowngradeCardUniqueId[info.upgradeCardUniqueId] = info.cardUniqueId;
                if (!cardUniqueIdToUpgradeCardUniqueId.ContainsKey(info.cardUniqueId))
                {
                    cardUniqueIdToUpgradeCardUniqueId[ info.cardUniqueId] =new List<string>();
                }
                cardUniqueIdToUpgradeCardUniqueId[ info.cardUniqueId].Add(info.upgradeCardUniqueId);
            }
            
        }
        
    }

    public void Start()
    {
        foreach (var card in MenuControl.Instance.heroMenu.allCards)
        {
            card.level = getIntProperty(card, "level", card.level);
            card.initialCost = getIntProperty(card, "cost", card.initialCost);
            if (card is Unit unit)
            {
                unit.initialHP = getIntProperty(unit, "hp", unit.initialHP);
                unit.initialPower = getIntProperty(unit, "attack", unit.initialPower);
            }

            var cardVersion = getStringProperty(card, "version");
            var isDemo = cardVersion == "DEMO";
            if (isDemo == MenuControl.Instance.heroMenu.isCardAvailable(card))
            {
            }
            else
            {
                // Debug.Log(card.GetChineseName() + " in playerCards version is " + isDemo + " with sprite version is " +
                //           MenuControl.Instance.heroMenu.isCardAvailable(card));
            }

            // var desc = getStringProperty(card, "description");
            // if (desc != card.GetDescription())
            // {
            //     Debug.Log(card.GetChineseName()+" desc in playerCards is "+desc+"\n in card is "+card.GetDescription());
            // }
        }
    }

    int getIntProperty(Card card, string propertyName, int originValue)
    {
        var levelOB = getProperty(card, propertyName);
        int level = (levelOB != null ? (int)(levelOB) : originValue);
        if (levelOB != null)
        {
            Debug.Log("");
        }

        if (originValue != level)
        {
            Debug.Log(card.GetChineseName() + " card level not match " + originValue + " " + level);
        }

        return level;
    }

    int getIntProperty(Card card, string propertyName)
    {
        var levelOB = getProperty(card, propertyName);
        int level = (levelOB != null ? (int)(levelOB) : -1);

        return level;
    }

    string getStringProperty(Card card, string propertyName)
    {
        var levelOB = getProperty(card, propertyName);
        string level = (levelOB != null ? (string)(levelOB) : "");

        return level;
    }

    object getProperty(Card card, string propertyName)
    {
        var chineseName = card.GetChineseName();
        if (uniqueIdToPlayerCardInfo.ContainsKey(chineseName))
        {
            return GetPropertyValue(uniqueIdToPlayerCardInfo[chineseName], propertyName);
        }

        return null;
    }

    public static object GetPropertyValue(object obj, string propertyName)
    {
        PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);

        if (propertyInfo != null)
        {
            return propertyInfo.GetValue(obj, null);
        }
        else
        {
            return null;
        }
    }

    public bool isLocked(string uid)
    {
        if (uniqueIdToPlayerCardInfo.ContainsKey(uid) &&
            uniqueIdToPlayerCardInfo[uid].requireUnlock != null &&
            uniqueIdToPlayerCardInfo[uid].requireUnlock != "")
        {
            return true;
        }

        return false;
    }
    
    public bool isValidInCurrentVersion(string uid)
    {
        if (uniqueIdToPlayerCardInfo.ContainsKey(uid) &&
            uniqueIdToPlayerCardInfo[uid].version != null )
        {

            if (MenuControl.Instance.isDemo)
            {
                if (uniqueIdToPlayerCardInfo[uid].version == "DEMO")
                {
                    return true;
                }
            }
            else
            {
                
                if (uniqueIdToPlayerCardInfo[uid].version == "DEMO"|| uniqueIdToPlayerCardInfo[uid].version == "正式版")
                {
                    return true;
                }
            }
        }

        return false;
    }

    public string downgradeChineseName(string chineseName)
    {
        if (chineseName == null)
        {
            Debug.LogError("");
        }

        if (cardUniqueIdToDowngradeCardUniqueId.ContainsKey(chineseName))
        {
            return cardUniqueIdToDowngradeCardUniqueId[chineseName];
        }

        return chineseName;
    }

    public Sprite GetDynamicSprite<T>(string chineseName, string folderName,
        Dictionary<string, T> dict) where T : ChineseNameToTalentMapInfo
    {
        if (chineseName == null)
        {
            Debug.LogError("");
            return null;
        }
        if (!dict.ContainsKey(chineseName))
        {
            if(MenuControl.Instance.checkSpriteExist)
            Debug.LogError("图片没有配置"+chineseName + " is not in map " + folderName);
            return null;
        }

        var talentInfo = dict[chineseName];
        var sprite = ResourceManager.LoadResouce<Sprite>(folderName + talentInfo.spriteName);
        if (sprite == null)
        {
            if (chineseName != "离开" && chineseName != "离开冒险")
            {
                if(MenuControl.Instance.checkSpriteExist)
                Debug.LogError("图片没有配置"+talentInfo.spriteName + " for " + chineseName + " is not in " + folderName);
            }
        }

        return sprite;
    }

    public Sprite buffSprite(string chineseName)
    {
        return GetDynamicSprite(chineseName, "Art/icon_buff/", chineseNameToBuffMap);
    }

    // public Sprite enemyCardSprite(string chineseName)
    // {
    //     return GetDynamicSprite(chineseName, "Art/icon_card/", chineseNameToEnemyCardMap);
    // }
    public Sprite playerCardSprite(Card card)
    {
        return GetDynamicSprite(card.UniqueID, "Art/icon_card/", chineseNameToPlayerCardMap);
    }
    public Sprite playerCardSprite(string uniqueId)
    {
        return GetDynamicSprite(uniqueId, "Art/icon_card/", chineseNameToPlayerCardMap);
    }
    // public Sprite playerCardSprite(string chineseName)
    // {
    //     return GetDynamicSprite(chineseName, "Art/icon_card/", chineseNameToPlayerCardMap);
    // }

    public Sprite eventSprite(string chineseName)
    {
        return GetDynamicSprite(chineseName, "Art/large_event/", chineseNameToEventMap);
    }

    void loadSpine(string name, Transform spineTransform)
    {
        SkeletonDataAsset[] spineAnimations = Resources.LoadAll<SkeletonDataAsset>("Art/spine/" + name);
        if (spineAnimations.Count() != 1)
        {
            if(MenuControl.Instance.checkSpriteExist)
            Debug.LogError("spine 没有配置 in " + name + " 's count is " + spineAnimations.Count());
            spineTransform.gameObject.SetActive(false);
            return;
        }

        foreach (var skeletonData in spineAnimations)
        {
            // // 创建一个新的游戏对象来承载Spine动画
            // GameObject go = new GameObject(skeletonData.name);
            // go.transform.SetParent(transform);  // Optional: set parent transform to manage the created game objects

            // 添加SkeletonGraphic组件并设置其属性
            SkeletonGraphic skeletonGraphic = spineTransform.GetComponent<SkeletonGraphic>();
            skeletonGraphic.skeletonDataAsset = skeletonData;
            skeletonGraphic.material =
                new Material(Shader.Find("Spine/SkeletonGraphic")); // Replace with your material or shader

            // 初始化Spine动画
            skeletonGraphic.Initialize(true);

            // 添加到列表中以便后续操作（例如设置动画等）
            //skeletonGraphics.Add(skeletonGraphic);

            // 播放默认动画（如果有）
            if (skeletonGraphic.AnimationState != null)
            {
                skeletonGraphic.AnimationState.SetAnimation(0, "idle",
                    true); // Replace "idle" with your animation name if needed
            }
        }
    }

    public void loadSpineForEncounterEventSpine(string chineseName, Transform spineTransform)
    {
        if (chineseNameToEventMap.ContainsKey(chineseName))
        {
            loadSpine(chineseNameToEventMap[chineseName].spriteName, spineTransform);
        }
        else
        {
            Debug.LogError("no chinese name " + chineseName + " in " + chineseNameToEventMap);
        }
    }

    public void loadSpineForStoryEventSpine(string chineseName, Transform spineTransform)
    {
        if (chineseNameToStoryEventMap.ContainsKey(chineseName))
        {
            loadSpine(chineseNameToStoryEventMap[chineseName].spriteName, spineTransform);
        }
        else
        {
            Debug.LogError("no chinese name " + chineseName + " in " + chineseNameToStoryEventMap);
        }
    }

    public Sprite achievementSprite(string chineseName)
    {
        return GetDynamicSprite(chineseName, "Art/icon_card/", chineseNameToAchievementMap);
    }

    public Sprite eventIcon(string chineseName)
    {
        //return GetDynamicSprite(chineseName, "Art/icon_event/", chineseNameToEventMap);
        var folderName = "Art/icon_event/";
        var dict = chineseNameToEventMap;
        if (!dict.ContainsKey(chineseName))
        {
            Debug.LogError(chineseName + " is not in map " + folderName);
            return null;
        }

        var talentInfo = dict[chineseName];
        var sprite = ResourceManager.LoadResouce<Sprite>(folderName + talentInfo.iconName);
        if (sprite == null)
        {
            Debug.LogError(talentInfo.iconName + " for " + chineseName + " is not in " + folderName);
        }

        return sprite;
    }

    public Sprite talentSprite(string chineseName)
    {
        return GetDynamicSprite(chineseName, "Art/icon_talent/", chineseNameToTalentMap);
        if (!MenuControl.Instance.csvLoader.chineseNameToTalentMap.ContainsKey(chineseName))
        {
            Debug.LogError(chineseName + " is not in " + chineseNameToTalentMap);
        }

        var talentInfo = MenuControl.Instance.csvLoader.chineseNameToTalentMap[chineseName];
        var sprite = ResourceManager.LoadResouce<Sprite>("Art/icon_talent/" + talentInfo.spriteName);
        if (sprite == null)
        {
            Debug.LogError(sprite + " for " + chineseName + " is not in Art/icon_talent/");
        }

        return sprite;
    }

    public int talentStarCount(string chineseName)
    {
        if (!chineseNameToTalentMap.ContainsKey(chineseName))
        {
            Debug.LogError((chineseName + " is not in chineseNameToTalentMap" ));
            return 0;
        }
        return chineseNameToTalentMap[chineseName].star;
    }
}