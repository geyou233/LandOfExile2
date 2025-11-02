using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueInBattleTriggerType
{
    战斗开始,
    任一指定id对话完成,
    首次使用指定id卡牌,
    指定id怪物离场,
    指定encounter战斗开始,
    指定id对话完成,
    战斗开始指定事件未获得宝藏,
    战斗开始指定事件获得至少1件宝藏,
    指定id怪物生命低于一半,
    指定id怪物首次触发技能,
    指定id怪物受到伤害
}

public class InBattleDialogueController : Trigger
{
    Queue<DialogueInBattleInfo> dialogueQueue = new Queue<DialogueInBattleInfo>();
    public float showDialogueTime = 3f;
    public float showDialogueShowTime = 2f;
    private float dialogueTime = 0;
    private BattleMenu battleMenu;
    public List<string> preDialogueInfo = new List<string>();

    private void Awake()
    {
        battleMenu = GetComponent<BattleMenu>();
    }

    public void AddPreDialogueInfo(string str)
    {
        preDialogueInfo.Add(str);
    }

    public override void CardPlayed(Card card, Tile tile, List<Tile> tiles)
    {
        foreach (var info in MenuControl.Instance.csvLoader.dialogueInBattleInfoDict.Values)
        {
            if (info.triggerType == DialogueInBattleTriggerType.首次使用指定id卡牌.ToString())
            {
                if (card.UniqueID == info.args[0] && !preDialogueInfo.Contains(card.UniqueID))
                {
                    EnqueueDialogue(info);
                    preDialogueInfo.Add(card.UniqueID);
                    return;
                }
            }
        }
    }

    public override void MinionSummoned(Minion minion)
    {
        foreach (var info in MenuControl.Instance.csvLoader.dialogueInBattleInfoDict.Values)
        {
            if (info.triggerType == DialogueInBattleTriggerType.首次使用指定id卡牌.ToString())
            {
                if (minion.UniqueID == info.args[0]&& !preDialogueInfo.Contains(minion.UniqueID))
                {
                    EnqueueDialogue(info);
                    preDialogueInfo.Add(minion.UniqueID);
                    return;
                }
            }
        }
    }

    public override void UnitDamaged(Card sourceCard, Unit unit, Ability ability, int damageAmount,
        bool triggerUnitDamage = true)
    {
        
        foreach (var info in MenuControl.Instance.csvLoader.dialogueInBattleInfoDict.Values)
        {
            if (info.triggerType == DialogueInBattleTriggerType.指定id怪物生命低于一半.ToString())
            {
                if (unit.UniqueID == info.args[0] && (unit.currentHP/(float)unit.initialHP)<0.5f)
                {
                    EnqueueDialogue(info);
                    return;
                }
            }
        }

        if (damageAmount > 0)
        {
            
            foreach (var info in MenuControl.Instance.csvLoader.dialogueInBattleInfoDict.Values)
            {
                if (info.triggerType == DialogueInBattleTriggerType.指定id怪物受到伤害.ToString())
                {
                    if (unit.UniqueID == info.args[0])
                    {
                        EnqueueDialogue(info);
                        return;
                    }
                }
            }
        }
    }

    public override void GameStarted()
    {
        base.GameStarted();
        foreach (var info in MenuControl.Instance.csvLoader.dialogueInBattleInfoDict.Values)
        {
            if (info.triggerType == DialogueInBattleTriggerType.战斗开始.ToString())
            {
                foreach (var unit in battleMenu.GetAllUnitsOnBoard())
                {
                    if (unit.UniqueID == info.speakerUid)
                    {
                        EnqueueDialogue(info);
                        break;
                    }
                }
            }

            if (info.triggerType == DialogueInBattleTriggerType.战斗开始指定事件未获得宝藏.ToString())
            {
                if (isSpeakerExisted(info) && !preDialogueInfo.Contains(info.args[0]))
                {
                    EnqueueDialogue(info);
                    break;
                }
            }

            if (info.triggerType == DialogueInBattleTriggerType.战斗开始指定事件获得至少1件宝藏.ToString())
            {
                if (isSpeakerExisted(info) && preDialogueInfo.Contains(info.args[0]))
                {
                    EnqueueDialogue(info);
                    break;
                }
            }
            
            
            if (info.triggerType == DialogueInBattleTriggerType.指定encounter战斗开始.ToString())
            {
                if (MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter()?.UniqueID == info.args[0])
                {
                    EnqueueDialogue(info);
                    break;
                }
            }
        }
    }

    public void ShowBattleDialogue(string str)
    {
        EnqueueDialogue(MenuControl.Instance.csvLoader.dialogueInBattleInfoDict[str]);
    }
    void EnqueueDialogue(DialogueInBattleInfo info)
    {
        if (info.heroIds.Contains(MenuControl.Instance.heroMenu.getCurrentClassIndex()) && !preDialogueInfo.Contains(info.dialogueId))
        {
            dialogueQueue. Enqueue(info);
            preDialogueInfo.Add(info.dialogueId);
        }
    }
    public override void MinionDestroyed(Card sourceCard, Ability ability, int damageAmount, Minion minion)
    {
        base.MinionDestroyed(sourceCard, ability, damageAmount, minion);
        
        foreach (var info in MenuControl.Instance.csvLoader.dialogueInBattleInfoDict.Values)
        {
            if (info.triggerType == DialogueInBattleTriggerType.指定id怪物离场.ToString())
            {
                if (minion.UniqueID == info.args[0])
                {
                    EnqueueDialogue(info);
                    return;
                }
            }
        }
    }

    public void BattleFinished()
    {
        dialogueQueue.Clear();
        preDialogueInfo.Clear();
    }

    private void Update()
    {
        if (MenuControl.Instance.publishVersionFeatureOn)
        {
            if (dialogueTime > 0)
            {
                dialogueTime -= Time.deltaTime;
            }
            if (dialogueQueue.Count > 0)
            {
                if(dialogueTime<=0)
                {
                    dialogueTime = showDialogueTime;
                    var dequeueInfo = dialogueQueue.Dequeue();
                    ShowDialogue(dequeueInfo);

                    foreach (var info in MenuControl.Instance.csvLoader.dialogueInBattleInfoDict.Values)
                    {
                        if (info.triggerType == DialogueInBattleTriggerType.任一指定id对话完成.ToString() ||
                            info.triggerType == DialogueInBattleTriggerType.指定id对话完成.ToString())
                        {
                            if (info.args.Contains(dequeueInfo.dialogueId))
                            {
                                EnqueueDialogue(info);
                            }
                        }
                    }
                }
            }
        }
    }

    void ShowDialogue(DialogueInBattleInfo info)
    {
        foreach (var unit in battleMenu.GetAllUnitsOnBoard())
        {
            if (unit.UniqueID == info.speakerUid ||
                (info.speakerUid == "MyHero" && unit == battleMenu.player1.GetHero()))
            {
                unit.player.ShowDialogue(unit, info);
                return;
            }
        }
    }

    bool isSpeakerExisted(DialogueInBattleInfo info)
    {
        foreach (var unit in battleMenu.GetAllUnitsOnBoard())
        {
            if (unit.UniqueID == info.speakerUid ||
                (info.speakerUid == "MyHero" && unit == battleMenu.player1.GetHero()))
            {
                return true;
            }
        }

        return false;
    }
}