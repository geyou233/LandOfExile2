using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.Soundy;
using Doozy.Engine.UI;
using UnityEngine.Serialization;

public class TriggeredAbility
{
    public Card card;
    public Effect effect;
    public System.Action actionToPerform;
}

public class BattleMenu : BasicMenu
{
    public bool testMode;
    public AIControl defaultAIControl;
    public AIControl aiControl;
    public TutorialControl tutorialControl;
    public GameObject settingButton;
    public BoardMenu boardMenu;
    public ArtifactSlotMenu artifactSlotMenu;

    public SpecialChallengeView specialChallengeView;

    public WeatherIcon weatherIcon;

    public CardHistoryMenu cardHistoryMenu;
    public Zone board;
    public Zone hand;
    public Zone deck;
    public Zone discard;
    public Zone removedFromGame;
    public Zone limbo;
    public Zone artifact;
    public Player player1;
    public Player playerAI;
    public Player currentPlayer;
    public bool Player1Starts;
    public int currentRound;//player+enemy
    public int currentTurn = 0;//player or enemy
    
    
    public Transform visibleBoardCardsHolderFlying;
    public Transform visibleBoardCardsHolderIndicator;

    public ActionAnimation defaultActionAnimation;
    public ActionAnimation defaultSummoningAnimation;
    public ActionAnimation defaultCastingAnimation;

    public VisibleCard selectedVisibleCard;
    public VisibleCard hoveredVisibleCard;

    public System.Action afterProcessingTriggeredAbilities = null;
    List<TriggeredAbility> triggeredAbilities = new List<TriggeredAbility>();
    List<float> triggeredAbilitiesTime = new List<float>();

    public bool player1CanAct;
    public Button endTurnButton;
    public Button endTurnFrameButton;
    public Doozy.Engine.UI.UIView yourTurnIndicator;
    public Doozy.Engine.UI.UIView notEnoughResourcesIndicator;
    public ArrowControl arrowControl;
#if !UNITY_STANDALONE
    public Vector3 originalTouchPoint;
#endif
    public VisibleCard visibleLargeHeroPrefab;
    public bool tutorialMode;
    public VSCutsceneControl vsCutsceneControl;
    public Transform battleBGHolder;
    public bool skipCutscene;
    public bool hoveringEndTurn;
    public GameObject discardPileHighlight;

    public SoundyData endTurnSound;
    public SoundyData drawCardSound;
    public Text encounterNameText;
    public GameObject player1TurnImage;
    public GameObject playerAITurnImage;

    public bool inBattle;
    public bool usingIntentSystem;

    public List<Tile> targetTiles = new List<Tile>();

    public Effect summoningSickEffectTemplate;

    public List<Achievement> achievements = new List<Achievement>();

    public Transform obstacleParent;
    public Transform trapParent;
[HideInInspector]
    public List<Minion> destroyedMinions = new List<Minion>();
    [HideInInspector]
    public List<string> usedCards = new List<string>();
    public void AddMinionDestroyed(Minion minion)
    {
        destroyedMinions.Add(minion);
    }

    public void AddUsedCardUniqueId(string uniqueId)
    {
        usedCards.Add(uniqueId);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            clickCount = 0;
        }
        if (inBattle)
        {
            if (Input.GetMouseButtonUp(1) && targetTiles.Count > 0)
            {
                CancelSelectVisibleCard();
            }
            if (Input.GetMouseButton(0) && player1CanAct)
            {
                if (selectedVisibleCard != null && currentPlayer == player1 && selectedVisibleCard.card.player == player1)
                {
                    selectedVisibleCard.transform.rotation = Quaternion.identity;
                    if (!(selectedVisibleCard is VisibleArtifactSlot))
                        selectedVisibleCard.transform.SetAsLastSibling();
                    Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    if (selectedVisibleCard.card is Castable && (player1.cardsInHand.Contains(selectedVisibleCard.card) || selectedVisibleCard.card.GetZone() == artifact))
                    {
                        if ((selectedVisibleCard.card.GetZone() != artifact && worldPoint.y > player1.visibleHandCardsHolder.transform.position.y + 2f) || (selectedVisibleCard.card.GetZone() == artifact &&(!(selectedVisibleCard.card is Artifact) || ((Artifact)selectedVisibleCard.card).currentCoolDown == 0 )&& Vector2.Distance(worldPoint, selectedVisibleCard.transform.position) > 0.5f))
                        {
                            worldPoint = new Vector3(worldPoint.x, player1.visibleHandCardsHolder.transform.position.y + 2f, worldPoint.z);
                            arrowControl.DrawArrow(selectedVisibleCard);
                        }
                        else
                        {
                            arrowControl.HideArrow();
                        }
                        if (selectedVisibleCard.card.GetZone() == hand)
                        {
                            selectedVisibleCard.transform.position = new Vector2(worldPoint.x, worldPoint.y);
                        }
                    }
                    else if (selectedVisibleCard.card is Minion && player1.cardsInHand.Contains(selectedVisibleCard.card))
                    {
                        selectedVisibleCard.transform.position = new Vector2(worldPoint.x, worldPoint.y);

                    }
                    else if (selectedVisibleCard.card.GetZone() == board)
                    {

                        Unit unit = (Unit)selectedVisibleCard.card;

                        Tile tile = boardMenu.GetTileAtScreenPos(Input.mousePosition);

                        if (tile != null && selectedVisibleCard.card.CanTarget(tile))
                        {
                            if (tile.GetUnit())
                            {
                                selectedVisibleCard.transform.position = unit.GetTile().transform.position;
                                arrowControl.DrawArrow(selectedVisibleCard);
                            }
                            else if (unit.CanMove())

                            {
                                selectedVisibleCard.transform.position = new Vector2(worldPoint.x, worldPoint.y);
                                arrowControl.HideArrow();
                            }


                        }
                        else if (unit.CanMove())

                        {
                            selectedVisibleCard.transform.position = new Vector2(worldPoint.x, worldPoint.y);
                        }
                    }

                    //移动到丢弃会变小
                    // if (selectedVisibleCard.card.GetZone() == hand)
                    // {
                    //
                    //     if (Vector2.Distance(worldPoint, player1.discardPileText.transform.position) < 1f)
                    //     {
                    //         selectedVisibleCard.transform.localScale = Vector3.one * 0.5f;
                    //     }
                    //     else
                    //     {
                    //         selectedVisibleCard.transform.localScale = Vector3.one * 1f;
                    //     }
                    //
                    // }

                    Tile mouseTile = boardMenu.GetTileAtScreenPos(Input.mousePosition);
                    if (mouseTile != null && selectedVisibleCard.card.CanTarget(mouseTile))
                    {
                        if (!LeanTween.isTweening(mouseTile.gameObject))
                        {
                            LeanTween.alphaCanvas(mouseTile.GetComponent<CanvasGroup>(), 0f, 0.2f).setLoopPingPong(1);
                        }
                    }

#if !UNITY_STANDALONE
                    if (Vector3.Distance(originalTouchPoint, Camera.main.ScreenToWorldPoint(Input.mousePosition)) > 0.5f)
                    {
                        if (MenuControl.Instance.infoMenu.gameObject.activeInHierarchy)
                        {
                            MenuControl.Instance.infoMenu.HideMenu();
                        }
                    }
#endif

                }
            }

            if (player1CanAct)
            {
                if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                {
                    AltViewPressed();
                }
                if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt) || Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
                {
                    AltViewReleased();
                }
            }
        }
    }

    public void ResetBattle()
    {
        vsCutsceneControl.gameObject.SetActive(false);
        tutorialMode = MenuControl.Instance.adventureMenu.tutorialEncounterOnly;
        //tutorialButton.SetActive(tutorialMode);
        afterProcessingTriggeredAbilities = null;
        triggeredAbilities.Clear();
        triggeredAbilitiesTime.Clear();
        destroyedMinions.Clear();
        usedCards.Clear();
        player1.ResetBattle();
        playerAI.ResetBattle();
        cardHistoryMenu.ResetBattle();
        

        currentRound = 0;
        SetEndTurnButton(false);
        if(yourTurnIndicator.IsActive())
        yourTurnIndicator.Hide(true);
        if(notEnoughResourcesIndicator.IsActive())
        notEnoughResourcesIndicator.Hide(true);
        discardPileHighlight.SetActive(false);

        arrowControl.HideArrow();
        tutorialControl.HideHandPointer();
        inBattle = false;
        player1CanAct = false;
        selectedVisibleCard = null;
        targetTiles.Clear();
        willEndWhenAnimationFinish = false;
        finishedSetup = false;
        isAnimatingNow = false;

        foreach (Transform child in battleBGHolder)
        {
            Destroy(child.gameObject);
        }

        foreach (Achievement achievement in achievements)
        {
            Destroy(achievement.gameObject);
        }
        achievements.Clear();

    }
[HideInInspector]
    public bool finishedSetup = false;

    private bool isSpecialChallenge = false;
    public void SetupBattle()
    {
        if (tutorialMode)
        {
            MenuControl.Instance.eventMenu.isSpecialChallenge = false;
            MenuControl.Instance.eventMenu.specialChallengeToggle.isOn = false;
        }
        
        ShowMenu();
        
        settingButton.SetActive(false);
        inBattle = true;

        foreach (Achievement achievementTemplate in MenuControl.Instance.achievementsMenu.achievements)
        {
            if (achievementTemplate.GetComponent<Trigger>() != null)
            {
                Achievement achievement = Instantiate(achievementTemplate, transform);
                achievements.Add(achievement);
            }
        }
        
        
        weatherIcon.gameObject.SetActive(MenuControl.Instance.weatherFeatureOn);
        weatherIcon.icon.sprite = MenuControl.Instance.adventureMenu.weatherController.CurrentWeatherIconInAdventure();

        MenuControl.Instance.PlayBattleMusic();
        encounterNameText.text = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().GetName();
        Player1Starts = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().Player1Starts;
        if (MenuControl.Instance.heroMenu.ascensionMode >= 5 && !tutorialMode)
        {
            Player1Starts = false;
        }

        currentPlayer = Player1Starts ? playerAI : player1; //so that nextplayersturn will swap

        aiControl = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().aiControl;
        if (aiControl == null)
            aiControl = defaultAIControl;

        Instantiate(MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().GetBattleMapPrefab(), battleBGHolder);

        boardMenu.totalActiveTiles = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().BoardColumns * 4;
        boardMenu.SetupBoard();

        List<Card> enemyCards = new List<Card>();
        foreach (Card card in MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().allOwnedCards)
        {
            enemyCards.Add(card);
        }

        List<Card> player1Cards = new List<Card>();
        foreach (Card card in MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().GetPlayerCardsOwned())
        {
            player1Cards.Add(card);
        }

        Tile enemyStartTile = boardMenu.tiles[0];
        Tile player1StartTile = boardMenu.tiles[boardMenu.tiles.Count - 1];
        var currentEncounter = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter();
        if (currentEncounter.RandomStartingPositions)
        {
            if (MenuControl.Instance.heroMenu.ascensionMode >= 10) //Corner to corner after Ferocity 10
            {
                enemyStartTile = boardMenu.tiles[0];
                player1StartTile = boardMenu.tiles[boardMenu.totalActiveTiles - 1];
            }
            else
            {
                var initialProbability = new List<int>() { 40, 30, 20, 10 };
                {
                    var tileList = new List<Tile>();
                    var tileProbability = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        if (!currentEncounter.PositionsInPlay.Contains(i) && boardMenu.tiles[i].isMoveable())
                        {
                            tileList.Add((boardMenu.tiles[i]));
                            tileProbability.Add(initialProbability[i]);
                        }

                    }

                    if (tileList.Count == 0)
                    {
                        Debug.LogError(("No valid starting positions for this encounter! " + currentEncounter.name));
                        enemyStartTile = boardMenu.GetAllEmptyTiles().RandomItem();
                    }
                    else
                    {
                        var selectId = RandomUtil.RandomBasedOnProbability(tileProbability);
                        enemyStartTile = tileList[selectId];
                    }

                }
               
                
                // int randomInt = UnityEngine.Random.Range(1, 101);
                // if (randomInt > 90 && boardMenu.tiles[3].isMoveable()) enemyStartTile = boardMenu.tiles[3];
                // else if (randomInt > 70 && boardMenu.tiles[2].isMoveable()) enemyStartTile = boardMenu.tiles[2];
                // else if (randomInt > 40 && boardMenu.tiles[1].isMoveable()) enemyStartTile = boardMenu.tiles[1];
                // else enemyStartTile = boardMenu.tiles[0];
                

                // randomInt = UnityEngine.Random.Range(1, 101);
                // if (randomInt > 90 && boardMenu.tiles[boardMenu.totalActiveTiles - 4].isMoveable()) player1StartTile = boardMenu.tiles[boardMenu.totalActiveTiles - 4];
                // else if (randomInt > 70 && boardMenu.tiles[boardMenu.totalActiveTiles - 3].isMoveable()) player1StartTile = boardMenu.tiles[boardMenu.totalActiveTiles - 3];
                // else if (randomInt > 40 && boardMenu.tiles[boardMenu.totalActiveTiles - 2].isMoveable()) player1StartTile = boardMenu.tiles[boardMenu.totalActiveTiles - 2];
                // else player1StartTile = boardMenu.tiles[boardMenu.totalActiveTiles - 1];
                
                {
                    var tileList = new List<Tile>();
                    var tileProbability = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        var j = boardMenu.totalActiveTiles - i-1;
                        if (!currentEncounter.PositionsInPlay.Contains(j) && boardMenu.tiles[i].isMoveable())
                        {
                            tileList.Add((boardMenu.tiles[j]));
                            tileProbability.Add(initialProbability[i]);
                        }
                    }
                    if (tileList.Count == 0)
                    {
                        Debug.LogError(("No valid starting positions for this encounter! " + currentEncounter.name));
                        enemyStartTile = boardMenu.GetAllEmptyTiles().RandomItem();
                    }
                    else
                    {
                        var selectId = RandomUtil.RandomBasedOnProbability(tileProbability);
                        player1StartTile = tileList[selectId];
                    }
                }

                //test
                //enemyStartTile = boardMenu.tiles[2];
                //player1StartTile = boardMenu.tiles[2];
            }
        }
        else
        {
            player1StartTile = boardMenu.tiles[MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().Player1StartingPos];
            enemyStartTile = boardMenu.tiles[MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().PlayerAIStartingPos];
        }

        artifactSlotMenu.SetupSlots();

        foreach (Card templateCard in enemyCards)
        {

            if (templateCard is Hero) //Put on board
            {
                Hero hero = playerAI.CreateOrReplaceHeroWithTemplate((Hero)templateCard, enemyStartTile);

                
                if (MenuControl.Instance.eventMenu.isSpecialChallenge &&
                    MenuControl.Instance.eventMenu.specialChallengeSkill != null)
                {
                    foreach (var skill in MenuControl.Instance.eventMenu.specialChallengeSkill.skills)
                    {
                        if (skill.abilityToPerform)
                        {
                        
                            skill.abilityToPerform.PerformAbility(hero, null, 0);
                        }
                    }
                }
                
                usingIntentSystem = hero.GetIntentSystem() != null;
                playerAI.discardPileText.transform.parent.gameObject.SetActive(!usingIntentSystem);
                playerAI.deckPileText.transform.parent.gameObject.SetActive(!usingIntentSystem);
                playerAI.manaText.transform.parent.gameObject.SetActive(!usingIntentSystem);

                if (Player1Starts)
                {
                    hero.remainingMoves = hero.GetInitialMoves();
                    hero.remainingActions = hero.GetInitialActions();
                }

            }
            else
            {
                Card card = playerAI.CreateCardInGameFromTemplate(templateCard);
                card.PutIntoZone(deck);

            }

        }

        foreach (Card templateCard in player1Cards)
        {

            if (templateCard is Hero) //Put on board
            {
                Hero hero = player1.CreateOrReplaceHeroWithTemplate((Hero)templateCard, player1StartTile);
                hero.currentHP = ((Hero)templateCard).currentHP;

                if (!Player1Starts || usingIntentSystem)
                {
                    hero.remainingMoves = hero.GetInitialMoves();
                    hero.remainingActions = hero.GetInitialActions();
                }

            }
            else
            {
                Card card = player1.CreateCardInGameFromTemplate(templateCard);
                card.PutIntoZone(deck);
            }

        }

        List<Card> CardsInPlay = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().CardsInPlay;
        List<int> PositionsInPlay = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().PositionsInPlay;
        int playerAIInitialMana = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().initialMana;
        int playerInitialMana = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().GetPlayerInitialMana();
        bool shufflePlayerAIDeck = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().shufflePlayerAIDeck;
        bool shufflePlayerDeck = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().shufflePlayerDeck;
        playerAI.baseDrawsPerTurn = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().drawsPerTurn;
        player1.baseDrawsPerTurn = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().GetPlayerBaseDrawsPerTurn();

        RenderAll();
        player1.visibleBoardCardsHolder.gameObject.SetActive(false);
        MenuControl.Instance.battleMenu.visibleBoardCardsHolderFlying.gameObject.SetActive(false);
        MenuControl.Instance.battleMenu.visibleBoardCardsHolderIndicator.gameObject.SetActive(false);
        
        bool skipping = skipCutscene;

        LeanTween.delayedCall(GetPlaySpeed(), () =>
        {
            
            InitObastacles();
            InitTraps();
            
            RenderAll();

            vsCutsceneControl.ShowVSCutScene();

            LeanTween.delayedCall(skipping ? 0.1f : 2.3f, () =>
            {
                player1.visibleBoardCardsHolder.gameObject.SetActive(true);
                MenuControl.Instance.battleMenu.visibleBoardCardsHolderFlying.gameObject.SetActive(true);
                MenuControl.Instance.battleMenu.visibleBoardCardsHolderIndicator.gameObject.SetActive(true);
            });

            LeanTween.delayedCall(skipping ? 0.2f : 3f, () =>
            {
                InitialShuffle(shufflePlayerDeck, shufflePlayerAIDeck, playerInitialMana, playerAIInitialMana, CardsInPlay, PositionsInPlay);
            });

        });

        //todo 更正确的做法是改正special challenge的读取方法，通过战斗确定，而不是事件
        if (MenuControl.Instance.eventMenu.isSpecialChallenge)
        {
            specialChallengeView.Show();
        }
        else
        {
            specialChallengeView.Hide();
        }
        
        finishedSetup = true;
    }

    void InitialShuffle(bool shufflePlayerDeck, bool shufflePlayerAIDeck, int playerInitialMana, int playerAIInitialMana, List<Card> CardsInPlay, List<int> PositionsInPlay)
    {
        if (shufflePlayerAIDeck)
            playerAI.ShuffleDeck();
        if (shufflePlayerDeck)
            player1.ShuffleDeck();

        player1.initialMana = playerInitialMana;
        player1.currentMana = 0;
        player1.ChangeMana(player1.initialMana);

        playerAI.initialMana = playerAIInitialMana;
        playerAI.currentMana = 0;
        playerAI.ChangeMana(playerAI.initialMana);

        //before initial draw event
        foreach (Trigger trigger in GetComponentsInChildren<Trigger>())
        {
            try
            {
                trigger.BeforeInitialDraw();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

        }


        //AI units on board to start
        for (int ii = 0; ii < CardsInPlay.Count; ii += 1)
        {
            Card templateCard = CardsInPlay[ii];
            int startingPos = PositionsInPlay[ii];

            if (templateCard is Minion)
            {


                if (boardMenu.tiles[startingPos].GetUnit())
                {
                    Debug.LogError($"{MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().name} Trying to put a minion on a tile {startingPos} that already has a unit");
                    var allEmptyIndex =  boardMenu.GetAllEmptyTileIndex();
                    if (allEmptyIndex.Count == 0)
                    {
                        break;
                    }
                    startingPos = allEmptyIndex.RandomItem();
                    //continue;
                }
                
                Minion card = (Minion)playerAI.CreateCardInGameFromTemplate(templateCard);
                card.TargetTile(boardMenu.tiles[startingPos], false);

                if (usingIntentSystem)
                {
                    card.RemoveEffect(null, null, card.GetEffectsWithTemplate(MenuControl.Instance.battleMenu.summoningSickEffectTemplate)[0]);
                }
            }
            else if (templateCard is Hero)
            {
                if (boardMenu.tiles[startingPos].GetUnit())
                {
                    Debug.LogError($"{MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().name} Trying to put a minion on a tile {startingPos} that already has a unit");
                    var allEmptyIndex =  boardMenu.GetAllEmptyTileIndex();
                    if (allEmptyIndex.Count == 0)
                    {
                        break;
                    }
                    startingPos = allEmptyIndex.RandomItem();
                    //continue;
                }
                
                Hero hero = playerAI.CreateOrReplaceHeroWithTemplate((Hero)templateCard, boardMenu.tiles[startingPos], true);
                hero.remainingMoves = hero.GetInitialMoves();
                hero.remainingActions = hero.GetInitialActions();

                var itemEncounter = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter();
                if (itemEncounter && itemEncounter.specialChallengeSkillApplyOnCardsInPlay && MenuControl.Instance.eventMenu.isSpecialChallenge &&
                    MenuControl.Instance.eventMenu.specialChallengeSkill != null)
                {
                    foreach (var skill in MenuControl.Instance.eventMenu.specialChallengeSkill.skills)
                    {
                        skill.abilityToPerform.PerformAbility(hero, null, 0);
                    }
                }
                
            }
        }

        //Put cards in players hand
        foreach (string cardID in MenuControl.Instance.heroMenu.startOfBattleHandCardIDs)
        {
            foreach (Card card in player1.cardsInDeck)
            {
                if (card.cardTemplate.UniqueID == cardID)
                {
                    card.DrawThisCard();
                    break;
                }
            }
        }

        InitialDraw(false);
    }
    
    
    void InitObastacles()
    {

        if (!MenuControl.Instance.publishVersionFeatureOn)
        {
            return;
        }
        
        int minObastacleCount = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().MinObstacleCount;
        int maxObastacleCount = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().MaxObstacleCount;

        if (MenuControl.Instance.settingsMenu.forceGenerateObstacle)
        {
            minObastacleCount = maxObastacleCount = 3;
                        var column = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().BoardColumns;
                        List<int> obstacleTileIndexes = new List<int>() { 1, column + 2, column * 2 + 1 };
            
                            foreach (var id in obstacleTileIndexes)
                            {
                
                                    var randomTile = boardMenu.tiles[id];
                
                                    var obstaclePrefab = MenuControl.Instance.areaMenu.currentArea.obstacle; 
                                    var obstacle = Instantiate(obstaclePrefab, randomTile.transform.position, Quaternion.identity,obstacleParent);
                
                                    boardMenu.AddObstacleToTile(obstacle,randomTile);
                            }
            
                            return;
        }

        if (maxObastacleCount == 0)
        {
            return;
        }
        int obstacleCount = UnityEngine.Random.Range(minObastacleCount, maxObastacleCount+1);
        if (obstacleCount == 0)
        {
            return;
        }

        //generate obstacles one by one, if failed to generate, break and show error
        for (int i = 0; i < obstacleCount; i++)
        {
            
            var possibleObstacleTile = new List<Tile>();
            for (int ii = 0; ii < boardMenu.totalActiveTiles; ii += 1)
            {
                var tile = boardMenu.tiles[ii];
                //todo need double check if block others, especially large hero..
                if (boardMenu.tiles[ii].canPlaceObstacle() && !boardMenu. tiles[ii].isBlocked() &&  
                    !MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().PositionsInPlay.Contains(ii))
                {
                    possibleObstacleTile.Add(tile);
                }
            
            
            }

            if (possibleObstacleTile.Count == 0)
            {
                Debug.LogError($"尝试生成第{i}个障碍物的时候失败了，障碍物可能设置的太多了");
                return;
            }

            var randomTile = possibleObstacleTile.RandomItem();

            var obstaclePrefab = MenuControl.Instance.areaMenu.currentArea.obstacle;
            var obstacle = Instantiate(obstaclePrefab, randomTile.transform.position, Quaternion.identity,obstacleParent);
            
            boardMenu.AddObstacleToTile(obstacle,randomTile);
        }
        
    }
    
    void InitTraps()
    {

        if (!MenuControl.Instance.weatherFeatureOn)
        {
            return;
        }

        var trapCount = MenuControl.Instance.adventureMenu.weatherController.GetBattleTrapCount(MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().BoardColumns);
        if (MenuControl.Instance.settingsMenu.forceGenerateObstacle)
        {
            trapCount  = 2;
        }
        if (trapCount == 0)
        {
            return;
        }

        var trapPrefab = MenuControl.Instance.adventureMenu.weatherController.CurrentWeatherTrap();
        //generate traps one by one, if failed to generate, break and show error
        for (int i = 0; i < trapCount; i++)
        {
            
            var possibleObstacleTile = new List<Tile>();
            for (int ii = 0; ii < boardMenu.totalActiveTiles; ii += 1)
            {
                var tile = boardMenu.tiles[ii];
                //todo need double check if block others, especially large hero..
                if (/*!boardMenu.tiles[ii].isOnBorder() &&*/ !boardMenu. tiles[ii].isBlocked() &&  
                    !MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().PositionsInPlay.Contains(ii))
                {
                    possibleObstacleTile.Add(tile);
                }
            
            
            }

            if (possibleObstacleTile.Count == 0)
            {
                Debug.LogError($"尝试生成第{i}个陷阱的时候失败了，陷阱可能设置的太多了");
                return;
            }

            var randomTile = possibleObstacleTile.RandomItem();

            var obstacle = Instantiate(trapPrefab, randomTile.transform.position, Quaternion.identity,trapParent);
            obstacle.transform.position = randomTile.transform.position;
            boardMenu.AddTrapToTile(obstacle.GetComponent<WeatherTrap>(),randomTile);
        }
        
    }

    public void FullyRemoveTrap(WeatherTrap trap)
    {
        boardMenu.RemoveTrap(trap);
        Destroy(trap.gameObject);
    }
    public bool MoveTrapToRandomTile(WeatherTrap trap)
    {
        var possibleObstacleTile = new List<Tile>();
        for (int ii = 0; ii < boardMenu.totalActiveTiles; ii += 1)
        {
            var tile = boardMenu.tiles[ii];
            //todo need double check if block others, especially large hero..
            if (/*!boardMenu.tiles[ii].isOnBorder() &&*/trap.GetTile()!=tile && boardMenu.tiles[ii].GetObstacle() == null&&boardMenu. tiles[ii].GetTrap() == null &&  
                                                         !MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().PositionsInPlay.Contains(ii))
            {
                possibleObstacleTile.Add(tile);
            }
            
            
        }

        if (possibleObstacleTile.Count == 0)
        {
            //Debug.LogError($"尝试移动陷阱的时候失败了，陷阱可能设置的太多了");
            return false;
        }

        var randomTile = possibleObstacleTile.RandomItem();
        
        boardMenu.MoveTrapToTile(trap,randomTile);
        return true;
    }
    

    void InitialDraw(bool mulligan)
    {

        RenderAll();

        if (mulligan || tutorialMode)
        {
            ContinueGame();
        }
        else
        {
            List<Card> cardsToDraw = new List<Card>();
            int cardsDrawn = player1.GetDrawsPerTurn() - player1.cardsInHand.Count;

            for (int ii = 0; ii < cardsDrawn; ii += 1)
            {
                if (player1.cardsInDeck.Count > ii)
                    cardsToDraw.Add(player1.cardsInDeck[ii]);
            }

            if (cardsToDraw.Count == 0)
            {
                ContinueGame();
            }
            else
            {

                List<Action> actions = new List<Action>();
                actions.Add(() => { ContinueGame(); });
                actions.Add(() =>
                {
                    for (int ii = 0; ii < MenuControl.Instance.cardChoiceMenu.cardsToShow.Count; ii += 1)
                    {
                        if (MenuControl.Instance.cardChoiceMenu.selectedVisibleCardInts.Contains(ii))
                        {
                            Card card = MenuControl.Instance.cardChoiceMenu.cardsToShow[ii];
                            card.PutIntoZone(removedFromGame);
                            card.PutIntoZone(deck,true);
                        }
                    }

                    ContinueGame();
                });

                List<string> buttonLabels = new List<string>();

                buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Keep All"));
                buttonLabels.Add(MenuControl.Instance.GetLocalizedString("Redraw"));

                string titleToShow = Player1Starts ? MenuControl.Instance.GetLocalizedString("FirstTurnPrompt1") : MenuControl.Instance.GetLocalizedString("FirstTurnPrompt2");
                string textToShow =  MenuControl.Instance.GetLocalizedString("MulliganPrompt");
                MenuControl.Instance.cardChoiceMenu.ShowChoice(cardsToDraw, buttonLabels, actions, textToShow, 1, cardsToDraw.Count, true, 2, true, titleToShow,true);
                MenuControl.Instance.cardChoiceMenu.highlightColor = CardChoiceMenu.HighlightColor.Cross;
                MenuControl.Instance.cardChoiceMenu.hideShowButton.gameObject.SetActive(true);
            }
        }
    }

    void ContinueGame()
    {
        
        LeanTween.delayedCall(GetPlaySpeed(), () =>
        {


            //Opponents Draw
            int cardsDrawnPlayerAI = playerAI.GetDrawsPerTurn();

            for (int ii = 0; ii < cardsDrawnPlayerAI; ii += 1)
            {
                playerAI.DrawACard();
            }
            if (usingIntentSystem)
            {
                List<Card> cardsToDraw = playerAI.GetHero().GetIntentSystem().GetCurrentHand().GetHandCards();
                foreach (Card card in cardsToDraw)
                {
                    Card newCard = playerAI.CreateCardInGameFromTemplate(card);
                    newCard.DrawThisCard();
                    if (MenuControl.Instance.heroMenu.seasonsMode)
                    {
                        int randomInt = UnityEngine.Random.Range(0, 3);
                        if (randomInt == 0)
                        {
                            newCard.cardTags.Add(MenuControl.Instance.niceTag);
                        }
                        else if (randomInt == 1)
                        {
                            newCard.cardTags.Add(MenuControl.Instance.naughtyTag);
                        }
                    }
                }
                LeanTween.delayedCall(GetPlaySpeed() + 0.4f, () =>
                {
                    playerAI.RenderIntent2ndHand();
                });
            }

            //Players draw
            int cardsDrawn = player1.GetDrawsPerTurn() - player1.cardsInHand.Count;

            for (int ii = 0; ii < cardsDrawn; ii += 1)
            {
                player1.DrawACard();
            }

            //show setting button, dont' allow player use setting button when battle is preparing
            settingButton .SetActive(true);
            //game started event
            foreach (Trigger trigger in GetComponentsInChildren<Trigger>())
            {
                try
                {
                    trigger.GameStarted();
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }

            }

            NextPlayersTurn();
        });
    }


    void NextPlayersTurn()
    {

        if (currentPlayer == player1)
        {
            currentPlayer = playerAI;
        }
        else
        {
            currentPlayer = player1;
        }

        if ((Player1Starts && currentPlayer == player1) || (!Player1Starts && currentPlayer == playerAI))
        {
            currentRound += 1;
        }

        currentTurn += 1;

        //Begin turn
        //BroadCast
        foreach (Trigger trigger in GetComponentsInChildren<Trigger>())
        {
            try
            {
                trigger.TurnStarted(currentPlayer);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

        }

        if (currentPlayer == player1)
        {
            afterProcessingTriggeredAbilities = IndicatePlayersTurn;
            //MenuControl.Instance.heroMenu.turnsUsed += 1;
        }
        else
        {
            if (aiControl != null)
            {
                afterProcessingTriggeredAbilities = TakeEnemyTurn;
            }
        }

        ProcessTriggeredAbilities();

    }

    void TakeEnemyTurn()
    {
        LeanTween.delayedCall(GetPlaySpeed(), () =>
        {
            if (aiControl.TakeAITurn(playerAI))
            {
                //Nothing done, end turn
                EndTurn();
            }

        });
    }

    void RenderAll(bool noBoardMovement = false)
    {
        player1.RenderCards(noBoardMovement);
        playerAI.RenderCards(noBoardMovement);
        boardMenu.RenderBoard();
        artifactSlotMenu.RenderSlots();
    }

    private int clickCount = 0;
    public override void SelectVisibleCard(VisibleCard vc, bool withClick = true)
    {
        base.SelectVisibleCard(vc, withClick);

        if (GetComponent<Doozy.Engine.UI.UIView>().IsHiding) return;

        if (!arrowControl.gameObject.activeInHierarchy)
        {
            if (!Input.GetMouseButton(0) && !withClick && (player1CanAct || MenuControl.Instance.cardChoiceMenu.gameObject.activeInHierarchy))
            {
                MenuControl.Instance.infoMenu.ShowInfo(vc);
            }
            else
            {
                MenuControl.Instance.infoMenu.HideMenu();
            }
        }

        if (withClick)
        {
            if (clickCount == 1)
            {
                clickCount = clickCount;
            }

            clickCount = 1 - clickCount;
        }

        //!(vc.card is Skill) 是为了特殊挑战中的技能，鼠标移动上去不要显示Highlight
        if (player1CanAct && targetTiles.Count == 0 && !(vc.card is Skill))
        {
            if (withClick)
            {
                selectedVisibleCard = vc;


#if !UNITY_STANDALONE
                originalTouchPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (player1CanAct || MenuControl.Instance.cardChoiceMenu.gameObject.activeInHierarchy)
                {
                    MenuControl.Instance.infoMenu.ShowInfo(vc);
                }
#endif

                // if (selectedVisibleCard.card.GetZone() == hand && selectedVisibleCard.card.player == player1)
                // {
                //     discardPileHighlight.SetActive(true);
                // }

                if (vc.card.player == player1)
                {
                    SoundyManager.Play("Menu", "CardLift");
                }

            }
            hoveredVisibleCard = vc;
            boardMenu.RenderBoard();
            if (selectedVisibleCard == hoveredVisibleCard || selectedVisibleCard == null)
                vc.HighlightBlue();

        }

    }

    public void CancelSelectVisibleCard()
    {
        if (targetTiles.Count > 0)
        {
            targetTiles.Clear();
            SetEndTurnButton(true);
        }

        MenuControl.Instance.infoMenu.HideMenu();
        hoveredVisibleCard = null;
        selectedVisibleCard = null;
        discardPileHighlight.SetActive(false);
        if (inBattle)
            RenderAll();
    }

    public override void DeSelectVisibleCard(VisibleCard vc, bool withClick = true)
    {
        base.DeSelectVisibleCard(vc, withClick);
        MenuControl.Instance.infoMenu.HideMenu();
        hoveredVisibleCard = null;

        if (withClick && targetTiles.Count == 0)
        {
            if (selectedVisibleCard != null && selectedVisibleCard == vc)
            {

                discardPileHighlight.SetActive(false);
                selectedVisibleCard.StopHighlight();
                if (vc.card.player == player1 && player1CanAct)
                {
                    SoundyManager.Play("Menu", "CardLift");
                    //determine which tile is underneath mouse pos
                    Tile tile = boardMenu.GetTileAtScreenPos(Input.mousePosition);

                    if (tile != null && selectedVisibleCard.card.CanTarget(tile))
                    {

                        if ((selectedVisibleCard.card.GetZone() == hand || selectedVisibleCard.card.GetZone() == artifact) && !selectedVisibleCard.card.CanAffordCost())
                        {
                            IndicateNotEnoughMana();
                        }
                        else
                        {
                            SetEndTurnButton(false);
                            if (selectedVisibleCard.card.activatedAbility != null && selectedVisibleCard.card.activatedAbility is MultiTargetAbility)
                            {
                                targetTiles.Add(tile);
                                arrowControl.HideArrow();
                                boardMenu.RenderBoard();
                            }
                            else
                            {
                                AnimationTargetAction(selectedVisibleCard, tile);
                                selectedVisibleCard = null;
                            }
                            return;
                        }
                    }
                    else
                    {
                        // //手动丢弃卡片
                        // if (selectedVisibleCard.card.GetZone() == hand)
                        // {
                        //
                        //     Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        //     if (Vector2.Distance(worldPoint, player1.discardPileText.transform.position) < 1f)
                        //     {
                        //         if (selectedVisibleCard.card.CanBeDiscarded(false))
                        //         {
                        //             //Discard
                        //             SetEndTurnButton(false);
                        //
                        //             AnimateDiscard(selectedVisibleCard);
                        //
                        //             selectedVisibleCard = null;
                        //             arrowControl.HideArrow();
                        //             return;
                        //         }
                        //     }
                        // }
                    }
                }

                selectedVisibleCard = null;
                arrowControl.HideArrow();
                RenderAll();
            }

        }
        else
        {

            vc.StopHighlight();
            boardMenu.RenderBoard();

        }


    }

    public override void ClickVisibleCard(VisibleCard vc)
    {
        base.ClickVisibleCard(vc);

        if (targetTiles.Count > 0 && vc.card is Unit && vc.card.GetZone() == board)
        {
            List<Tile> newTargetTiles = new List<Tile>();
            newTargetTiles.AddRange(targetTiles);
            newTargetTiles.Add(((Unit)vc.card).GetTile());
            if (((Castable)selectedVisibleCard.card).CanTarget(newTargetTiles))
            {


                if (newTargetTiles.Count >= ((MultiTargetAbility)selectedVisibleCard.card.activatedAbility).minTargets && newTargetTiles.Count <= ((MultiTargetAbility)selectedVisibleCard.card.activatedAbility).maxTargets)
                {
                    AnimationTargetAction(selectedVisibleCard, newTargetTiles[0], newTargetTiles);
                    selectedVisibleCard = null;
                    targetTiles.Clear();
                }
            }
        }
        // else if (vc.card .isItem)
        // {
        //     MenuControl.Instance.ShowNotification(null, "", MenuControl.Instance.GetLocalizedString("ArtifactUsagePrompt"), true, false, true);
        // }
    }

    public void ClickOnTile(Tile tile)
    {
        if (targetTiles.Count > 0)
        {
            List<Tile> newTargetTiles = new List<Tile>();
            newTargetTiles.AddRange(targetTiles);
            newTargetTiles.Add(tile);
            if (((Castable)selectedVisibleCard.card).CanTarget(newTargetTiles))
            {

                if (newTargetTiles.Count >= ((MultiTargetAbility)selectedVisibleCard.card.activatedAbility).minTargets && newTargetTiles.Count <= ((MultiTargetAbility)selectedVisibleCard.card.activatedAbility).maxTargets)
                {
                    AnimationTargetAction(selectedVisibleCard, newTargetTiles[0], newTargetTiles);
                    selectedVisibleCard = null;
                    targetTiles.Clear();
                }
            }
        }
    }
    
    private bool willEndWhenAnimationFinish = false;
    private bool isAnimatingNow = false;

    private void setEndTurnButton(bool enabled)
    {
        endTurnButton.GetComponent<UIButton>().OnPointerEnter.Enabled = false;
        endTurnButton.interactable = enabled;
        endTurnFrameButton.interactable = enabled;
    }
    public void SetEndTurnButton(bool enabled)
    {
        player1CanAct = enabled;
        player1TurnImage.SetActive(currentPlayer == player1);
        playerAITurnImage.SetActive(currentPlayer == playerAI);
        if (tutorialMode)
        {
            endTurnButton.interactable = tutorialControl.SetEndTurnButton(enabled);
            endTurnFrameButton.interactable = tutorialControl.SetEndTurnButton(enabled);
            return;
        }

        if (currentPlayer == playerAI)
        {
            
            setEndTurnButton(false);
            
            willEndWhenAnimationFinish = false;
        }
        else
        {
            if (enabled)
            {
                isAnimatingNow = false;
            }
            else
            {
                isAnimatingNow = true;
            }
            if (willEndWhenAnimationFinish)
            {
                setEndTurnButton(false);
                Player1PressedEndTurn();
            }
            else
            {
                setEndTurnButton(true);
            }
        }

        
    }
    
    public void SetEndTurnButton_deprecated(bool enabled)
    {
        player1CanAct = enabled;
        player1TurnImage.SetActive(currentPlayer == player1);
        playerAITurnImage.SetActive(currentPlayer == playerAI);
        if (tutorialMode)
        {
            endTurnButton.interactable = tutorialControl.SetEndTurnButton(enabled);
            endTurnFrameButton.interactable = tutorialControl.SetEndTurnButton(enabled);
            return;
        }
        endTurnButton.interactable = enabled;
        endTurnFrameButton.interactable = enabled;
    }
    public void AnimateDiscard(VisibleCard vc, bool automaticDiscard = false)
    {
        //if (!MenuControl.Instance.heroMenu.alternateManaModeToggle.isOn)
        //{
        //    vc.card.player.ChangeMana(1);
        //}
        vc.disableInteraction = true;
        // do animation
        GameObject vCToDiscard = Instantiate(vc.gameObject, vc.transform.parent) as GameObject;
        vCToDiscard.transform.position = vc.transform.position;
        vCToDiscard.transform.localScale = vc.transform.localScale;
        LeanTween.move(vCToDiscard, vc.card.player.discardPileText.transform.position, GetPlaySpeed() * 1.5f).setEaseOutSine().setDestroyOnComplete(true);
        LeanTween.scale(vCToDiscard, Vector3.one * 0.5f, GetPlaySpeed());
        LeanTween.rotateAround(vCToDiscard, Vector3.forward, 720f, GetPlaySpeed() * 1.5f);

        try
        {
            vc.card.DiscardThisCard(automaticDiscard);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        ProcessTriggeredAbilities();
    }

    public void AnimationTargetAction(VisibleCard vc, Tile tile, List<Tile> tiles = null)
    {
        if (tutorialMode) tutorialControl.HideHandPointer();
        if (!(vc.card is Artifact))
            vc.transform.SetAsLastSibling();
        arrowControl.HideArrow();
        if (vc.card.GetZone() == board && (tile.isMoveable() || tile.GetUnit() == vc.card)) //Move
        {
            try
            {
                vc.card.TargetTile(tile, true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            ProcessTriggeredAbilities();
        }
        else if (vc.card.GetZone() == board && tile.GetUnit().player == vc.card.player) //Friendly assist action
        {

            //Do animation
            LeanTween.rotateAroundLocal(vc.gameObject, Vector3.up, 360f, GetPlaySpeed()).setEaseInOutQuad().setOnComplete(() =>
            {
                try
                {
                    vc.card.TargetTile(tile, true);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                RenderAll(true);
                LeanTween.delayedCall(GetPlaySpeed(), () =>
                {
                    ProcessTriggeredAbilities();
                });
            });

        }
        else if (vc.card.GetZone() == board && tile.GetUnit().player != vc.card.player) //Attack action
        {
            float timing = 0f;
            if (vc.card.actionAnimation != null)
            {
                timing = vc.card.actionAnimation.PerformAnimation(vc, tile);
                SoundyManager.Play(vc.card.actionAnimation.audioToPlay);
            }
            else
            {
                timing = MenuControl.Instance.battleMenu.defaultActionAnimation.PerformAnimation(vc, tile);
                SoundyManager.Play(MenuControl.Instance.battleMenu.defaultActionAnimation.audioToPlay);
            }

            if (vc.card is Unit)
            {
                SoundyManager.Play(((Unit)vc.card).attackingSound);
            }

            LeanTween.delayedCall(timing, () =>
            {
                try
                {
                    vc.card.TargetTile(tile, true);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                RenderAll(true);
                LeanTween.delayedCall(GetPlaySpeed(), () =>
                {
                    ProcessTriggeredAbilities();
                });

            });


        }
        else if (vc.card.GetZone() == hand && tile.isMoveable() && vc.card is Minion) //Summoning
        {
            vc.RenderCardForMenu(vc.card);
            vc.disableInteraction = true;
            float timing = defaultSummoningAnimation.PerformAnimation(vc, tile);
            SoundyManager.Play(defaultSummoningAnimation.audioToPlay);

            LeanTween.delayedCall(timing, () =>
            {
                try
                {
                    vc.card.TargetTile(tile, true);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                ProcessTriggeredAbilities();
            });
        }
        else if (vc.card.GetZone() == hand && vc.card is Castable castable) //Castable
        {
            vc.RenderCardForMenu(vc.card);
            vc.disableInteraction = true;
            float timing = 0f;
            if (vc.card.actionAnimation != null && !(vc.card is NewWeapon))//new weapon is castable, but it would not use action animation for casting
            {
                timing = vc.card.actionAnimation.PerformAnimation(vc, tile);
                SoundyManager.Play(vc.card.actionAnimation.audioToPlay);
            }
            else
            {
                timing = defaultCastingAnimation.PerformAnimation(vc, tile);
                SoundyManager.Play(defaultCastingAnimation.audioToPlay);
            }

            LeanTween.delayedCall(timing, () =>
            {
                try
                {
                    if (tiles != null) ((Castable)vc.card).TargetTiles(tiles, true);
                    else
                        vc.card.TargetTile(tile, true);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

var cardActionTime = vc.card.PerformAnimationTime();
if (float.IsPositiveInfinity(cardActionTime))
{
    
    RenderAll(true);
}
else
{
    
    cardActionTime+=GetPlaySpeed();
    RenderAll(true);
    LeanTween.delayedCall(cardActionTime, () =>
    {
        ProcessTriggeredAbilities();
    });
}
            });
        }
        else if (vc.card.GetZone() == artifact) //Artifact activation
        {
            VisibleCard vc2 = Instantiate(MenuControl.Instance.visibleCardPrefab, transform);
            vc2.RenderCardForMenu(vc.card);
            vc2.transform.position = vc.transform.position;
            vc2.transform.localScale = Vector3.zero;
            vc2.disableInteraction = true;
            float timing = 0f;
            if (vc2.card.actionAnimation != null)
            {
                timing = vc2.card.actionAnimation.PerformAnimation(vc2, tile);
                SoundyManager.Play(vc.card.actionAnimation.audioToPlay);
            }
            else
            {
                timing = defaultCastingAnimation.PerformAnimation(vc2, tile);
                SoundyManager.Play(defaultCastingAnimation.audioToPlay);
            }

            LeanTween.delayedCall(timing, () =>
            {
                Destroy(vc2.gameObject);
                try
                {
                    if (tiles != null) ((Castable)vc.card).TargetTiles(tiles, true);
                    else
                        vc.card.TargetTile(tile, true);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                RenderAll(true);
                LeanTween.delayedCall(GetPlaySpeed(), () =>
                {
                    ProcessTriggeredAbilities();
                });
            });
        }

    }



    void IndicateNotEnoughMana()
    {
        if (!notEnoughResourcesIndicator.IsShowing)
        {
            
            notEnoughResourcesIndicator.GetComponent<BasicMenu>().ShowMenu();
        }
    }

    void IndicatePlayersTurn()
    {
        afterProcessingTriggeredAbilities = null;

        if (MenuControl.Instance.settingsMenu.playSpeedSlider.value == MenuControl.Instance.settingsMenu.playSpeedSlider.maxValue)
        {
            yourTurnIndicator.Show(true);
            LeanTween.delayedCall(0.2f, () =>
            {
                SetEndTurnButton(true);
            });
        }
        else
        {
            yourTurnIndicator.Show();
            LeanTween.delayedCall(0.7f, () =>
            {
                SetEndTurnButton(true);
            });
        }

    }

    public void Player1PressedEndTurn()
    {
        if (currentPlayer == player1 && isAnimatingNow)
        {
            willEndWhenAnimationFinish = true;
            setEndTurnButton(false);
            return;
        }
        targetTiles.Clear();
        endTurnButton.transform.localScale = Vector3.one;
        LeanTween.cancel(endTurnButton.transform.GetChild(0).GetComponent<RectTransform>());
        LeanTween.color(endTurnButton.transform.GetChild(0).GetComponent<RectTransform>(), new Color(1f, 1f, 1f, 0f), 0.7f);
        PointerExitedEndTurn();

        SoundyManager.Play(endTurnSound);

        afterProcessingTriggeredAbilities = EndTurn;

        foreach (Trigger trigger in GetComponentsInChildren<Trigger>())
        {
            try
            {

                trigger.BeforeEndTurn(currentPlayer);

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        ProcessTriggeredAbilities(true);

    }
    
    private void Player1PressedEndTurn_deprecated()
    {
        targetTiles.Clear();
        endTurnButton.transform.localScale = Vector3.one;
        LeanTween.cancel(endTurnButton.transform.GetChild(0).GetComponent<RectTransform>());
        LeanTween.color(endTurnButton.transform.GetChild(0).GetComponent<RectTransform>(), new Color(1f, 1f, 1f, 0f), 0.7f);
        PointerExitedEndTurn();

        SoundyManager.Play(endTurnSound);

        afterProcessingTriggeredAbilities = EndTurn;

        foreach (Trigger trigger in GetComponentsInChildren<Trigger>())
        {
            try
            {

                trigger.BeforeEndTurn(currentPlayer);

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        ProcessTriggeredAbilities(true);

    }

    public void EndTurn()
    {
        List<Card> cardsThatCanBeDiscarded = new List<Card>();
        foreach (Card card in currentPlayer.cardsInHand)
        {
            if (card.CanBeDiscarded(true))
            {
                cardsThatCanBeDiscarded.Add(card);
            }
        }

        if (cardsThatCanBeDiscarded.Count > 0)
        {
            afterProcessingTriggeredAbilities = EndTurn;

            VisibleCard vc = currentPlayer.GetVisibleCardForCard(cardsThatCanBeDiscarded[0]);
            if (vc != null)
            {
                LeanTween.delayedCall(GetPlaySpeed() / 2f, () =>
                  {
                      AnimateDiscard(vc, true);
                  });
            }
            else
            {
                cardsThatCanBeDiscarded[0].DiscardThisCard(true);
                ProcessTriggeredAbilities();
            }

            return;
        }

        foreach (Trigger trigger in GetComponentsInChildren<Trigger>())
        {
            try
            {
                trigger.TurnEnded(currentPlayer);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        afterProcessingTriggeredAbilities = EndTurnDrawAndReset;

        ProcessTriggeredAbilities();

        foreach (Unit unit in MenuControl.Instance.battleMenu.GetAllUnitsOnBoard())
        {
            if (unit.player == currentPlayer)
            {
                
                unit.resetMoveCountThisTurn();
            }
        }

    }

    void EndTurnDrawAndReset()
    {

        //Draw
        int amountToDraw = currentPlayer.GetDrawsPerTurn() - currentPlayer.cardsInHand.Count;

        for (int ii = 0; ii < amountToDraw; ii += 1)
        {
            currentPlayer.DrawACard();
        }

        if (MenuControl.Instance.battleMenu.usingIntentSystem && currentPlayer == playerAI)
        {
            List<Card> cardsToDraw = playerAI.GetHero().GetIntentSystem().GetCurrentHand().GetHandCards();
            foreach (Card card in cardsToDraw)
            {
                Card newCard = playerAI.CreateCardInGameFromTemplate(card);
                newCard.DrawThisCard();
                if (MenuControl.Instance.heroMenu.seasonsMode)
                {
                    int randomInt = UnityEngine.Random.Range(0, 3);
                    if (randomInt == 0)
                    {
                        newCard.cardTags.Add(MenuControl.Instance.niceTag);
                    }
                    else if (randomInt == 1)
                    {
                        newCard.cardTags.Add(MenuControl.Instance.naughtyTag);
                    }
                }
            }
            LeanTween.delayedCall(GetPlaySpeed() + 0.4f, () =>
            {
                playerAI.RenderIntent2ndHand();
            });
        }

        //Restore/reset mana
        if (currentPlayer.currentMana != currentPlayer.initialMana)
        {
            currentPlayer.ChangeMana(currentPlayer.initialMana - currentPlayer.currentMana);
        }

        //Lower Artifact Cooldowns
        foreach (Card card in currentPlayer.GetArtifacts())
        {
            if (card is Artifact artifact)
            {
                if (artifact.currentCoolDown > 0)
                {
                    artifact.currentCoolDown -= 1;
                }
            }
        }

        //Reset board units moves & actions
        foreach (Unit unit in currentPlayer.cardsOnBoard)
        {
            unit.remainingMoves = unit.GetInitialMoves();
            unit.remainingActions = unit.GetInitialActions();
        }

        foreach (Trigger trigger in GetComponentsInChildren<Trigger>())
        {
            try
            {
                trigger.AfterTurnEnded(currentPlayer);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        afterProcessingTriggeredAbilities = NextPlayersTurn;

        ProcessTriggeredAbilities();
    }

    public void ProcessTriggeredAbilities(bool oneAtATime = false)
    {

        SetEndTurnButton(false);
        RenderAll();

        if (triggeredAbilities.Count == 0)
        {

            //End game check
            if (player1.GetHero().GetHP() <= 0)
            {
                CompleteBattle(false);
                return;
            }
            else
            {
                bool oneHeroAlive = false;
                foreach (Card card in playerAI.cardsOnBoard)
                {
                    if (card is Hero && ((Hero)card).GetHP() > 0)
                    {
                        oneHeroAlive = true;
                    }
                }

                if (!oneHeroAlive)
                {
                    CompleteBattle(true);
                    return;
                }
            }

            //Check minions on zero HP
            foreach (Unit unit in GetAllUnitsOnBoard())
            {
                if (unit is Minion && unit.GetHP() <= 0)
                {
                    foreach (Trigger trigger in MenuControl.Instance.battleMenu.GetComponentsInChildren<Trigger>())
                    {
                        try
                        {
                            trigger.MinionDestroyed(null, null, 0, (Minion)unit);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e);
                        }

                    }

                    unit.player.PutCardIntoZone(unit, MenuControl.Instance.battleMenu.discard);

                    unit.InitializeUnit(false);
                    RenderAll();
                }
            }
            if (triggeredAbilities.Count > 0)
            {
                ProcessTriggeredAbilities(oneAtATime);
                return;
            }

            if (afterProcessingTriggeredAbilities == null)
            {

                if (currentPlayer == player1)
                {
                    SetEndTurnButton(true);

                    bool movesAvailable = false;
                    foreach (Unit unit in player1.cardsOnBoard)
                    {
                        if (unit.CanMove() || unit.CanAct())
                        {
                            movesAvailable = true;
                            break;
                        }
                    }

                    if (player1.cardsInHand.Count == 0 && !movesAvailable)
                    {
                        endTurnButton.transform.localScale = Vector3.one;
                        LeanTween.scale(endTurnButton.gameObject, Vector3.one * 1.1f, 0.3f).setLoopPingPong(2).setEaseOutSine().setDelay(0.3f);
                        endTurnButton.transform.GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
                        LeanTween.color(endTurnButton.transform.GetChild(0).GetComponent<RectTransform>(), new Color(1f, 1f, 1f, 1f), 0.3f).setEaseInOutSine().setLoopPingPong(2).setDelay(0.3f);
                        LeanTween.color(endTurnButton.transform.GetChild(0).GetComponent<RectTransform>(), new Color(1f, 1f, 1f, 1f), 0.3f).setEaseInOutSine().setDelay(1.5f);
                    }
                }

            }
            else
            {
                afterProcessingTriggeredAbilities();
            }

        }
        else
        {

            LeanTween.delayedCall(GetPlaySpeed(), () =>
            {
                if (this == null || triggeredAbilities.Count == 0)
                {
                    return;
                }

                TriggeredAbility ta = triggeredAbilities[0];
                var time = triggeredAbilitiesTime[0];
                triggeredAbilities.RemoveAt(0);
                triggeredAbilitiesTime.RemoveAt(0);
                try
                {
                    ta.actionToPerform();
                }
                catch (Exception e)
                {
                    //test only!
                    //ta.actionToPerform();
                    Debug.LogError(e);
                }

                if (!oneAtATime)
                {
                    if (time > 0)
                    {
                        LeanTween.delayedCall(time, ()=>
                        {
                            ProcessTriggeredAbilities();
                        });
                    }
                    else
                    {
                        ProcessTriggeredAbilities();
                    }
                    
                }

            });
        }


    }

    public void AddTriggeredAbility(Card card, Effect effect, System.Action actionToPerform, bool triggerLast = false,float animationTime = 0)
    {
        TriggeredAbility ta = new TriggeredAbility();
        ta.actionToPerform = actionToPerform;
        ta.card = card;
        ta.effect = effect;
        if (!triggerLast)
        {
            triggeredAbilities.Insert(0, ta);
            triggeredAbilitiesTime.Insert(0, animationTime);
        }
        else
        {
            triggeredAbilities.Add(ta);
            triggeredAbilitiesTime.Add(animationTime);
        }
    }

    public void CompleteBattle(bool victory)
    {
        GetComponent<InBattleDialogueController>().BattleFinished();
        //clear hero temp effect
        MenuControl.Instance.heroMenu.hero.tempStartingEffects.Clear();
        MenuControl.Instance.heroMenu.hero.tempStartingEffectCharges.Clear();
            
        if (MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter())
        {
            var encounterId = MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter().UniqueID;
            if (!MenuControl.Instance.defeatedEncounter.Contains(encounterId))
            {
                MenuControl.Instance.defeatedEncounter.Add(encounterId);
            }
        }
        Debug.Log("Battle " + (victory ? "Won" : "Lost"));

        //game ended event
        foreach (Trigger trigger in GetComponentsInChildren<Trigger>())
        {
            try
            {
                trigger.GameEnded(victory);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

        }
        RenderAll();
        inBattle = false;

        LeanTween.delayedCall(0.5f + (GetPlaySpeed() * 5f), () =>
          {
              if (tutorialMode)
              {
                  MenuControl.Instance.tutorialFinished = true;
                  MenuControl.Instance.achievementsMenu.CheckAchievements();
                  MenuControl.Instance.dataControl.SaveData();
              }

              if (tutorialMode || MenuControl.Instance.adventureMenu.GetCurrentAdventureItemEncounter() == null) //Tutorial or Test Encounter
              {
                  HideMenu();
                  MenuControl.Instance.mainMenu.ShowMenu();
                  return;
              }

              if (victory)
              {

                  MenuControl.Instance.victoryMenu.BattleCompleted();
              }
              else
              {

                  ResetBattle();
                  MenuControl.Instance.aftermathMenu.ShowLoseMenu();
                  //MenuControl.Instance.defeatMenu.ShowMenu();

              }


          });
    }

    public float GetPlaySpeed()
    {
        float playSpeed = MenuControl.Instance.settingsMenu.playSpeed;

        //if (testMode) return 0.1f;

        return (1.4f - playSpeed)* MenuControl.Instance.speedScale;
    }

    public void PointerEnterEndTurn()
    {
        if (player1CanAct)
        {
            if (MenuControl.Instance.settingsMenu.playSpeedSlider.value != MenuControl.Instance.settingsMenu.playSpeedSlider.maxValue)
            {
                player1.GhostDrawNextHand();
            }

            hoveringEndTurn = true;
            boardMenu.RenderBoard();
            AltViewPressed();
        }
    }

    public void PointerExitedEndTurn()
    {
        player1.HideGhostHand();
        hoveringEndTurn = false;
        boardMenu.RenderBoard();
        AltViewReleased();
    }


    public List<Unit> GetAllUnitsOnBoard()
    {
        List<Unit> units = new List<Unit>();
        foreach (Card card in player1.cardsOnBoard)
        {
            units.Add((Unit)card);
        }
        foreach (Card card in playerAI.cardsOnBoard)
        {
            units.Add((Unit)card);
        }
        return units;
    }

    public void StopAfterProcessingTriggeredAbilities()
    {
        afterProcessingTriggeredAbilities = null;
    }

    void AltViewPressed()
    {
        foreach (Unit unit in player1.cardsOnBoard)
        {
            VisibleCard vc = unit.player.GetVisibleBoardCardForCard(unit);
            if (vc != null)
            {
                vc.ShowAltview();
            }
        }
    }

    void AltViewReleased()
    {
        foreach (Unit unit in player1.cardsOnBoard)
        {
            VisibleCard vc = unit.player.GetVisibleBoardCardForCard(unit);
            if (vc != null)
            {
                vc.HideAltView();
            }
        }
    }

    public void SelectVisibleArtifactSlot(VisibleArtifactSlot slot)
    {
        Debug.Log("visartslot selected");
    }

    public void DeSelectVisibleArtifactSlot(VisibleArtifactSlot slot)
    {
        Debug.Log("visartslot deselected");
    }
}
