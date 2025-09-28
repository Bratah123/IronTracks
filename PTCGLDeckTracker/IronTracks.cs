using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using HarmonyLib;
using MelonLoader;
using RainierClientSDK;
using UnityEngine.SceneManagement;
using TPCI.Rainier.Match.Cards.Ownership;
using PTCGLDeckTracker.CardCollection;
using TPCI.Rainier.Match.Cards;
using CardDatabase.DataAccess;

namespace PTCGLDeckTracker
{
    public class IronTracks : MelonMod
    {
        // General UI Constants
        private const float DefaultWindowWidth = 250f;
        private const float LineHeight = 20f;
        private const float WindowHeaderHeight = 25f;
        private const float WindowVerticalPadding = 60f;
        private const int FontSize = 15;
        private const int TextPadding = 5;
        private const float HorizontalMargin = 5f;
        private const float TotalHorizontalMargin = HorizontalMargin * 2;

        // Deck Tracker Constants
        private const float DeckTrackerInitialHeight = 100f;
        private const int DeckCounterLines = 3; // 2 counters + 1 blank line

        // Prize Tracker Constants
        private const float PrizeTrackerInitialHeight = 100f;
        private const float PrizeTrackerInitialY = 200f;
        private const int PrizeCounterLines = 2; // 1 counter + 1 blank line

        // 3D Prize Display Constants
        private const float PrizeTitleWidth = 200f;
        private const float PrizeTitleHeight = 30f;
        private const float PrizeTitleInitialY = 10f;
        private const float CardSpawnXRotation = -55f;
        private const float CardSpawnYRotation = 180f;
        private const float CardSpawnScale = 2f;
        private const int PrizeGridColumns = 3;
        private const float PrizeGridStartX = -14f;
        private const float PrizeGridStartY = 2.5f;
        private const float PrizeGridSpacingX = 14f;
        private const float PrizeGridSpacingY = -20f;

        // Control Panel Constants
        private const float ControlPanelWidth = 300f;
        private const float ControlPanelHeight = 200f;

        // Tooltip Constants
        private const float TooltipHoverDelay = 0.5f; // Half a second

        bool enableDeckTracker = false;
        bool enablePrizeCards = false;
        static Player player = new Player();
        const String GAME_SCENE_NAME = "Match_Landscape";
        const float HighlightDuration = 2.0f;

        private TrackedCard _hoveredCard;
        private CardBasic _tooltipCardObject;
        private string _currentlyDisplayedTooltipCardId;

        private float _hoverStartTime;
        private TrackedCard _pendingTooltipCard;

        private Rect _deckTrackerWindowRect = new Rect(Screen.width - DefaultWindowWidth, 0, DefaultWindowWidth, DeckTrackerInitialHeight);
        private Rect _prizeCardsWindowRect = new Rect(0, PrizeTrackerInitialY, DefaultWindowWidth, PrizeTrackerInitialHeight);
        private bool _prizeCardsSpawned = false;
        private readonly List<CardBasic> _spawnedPrizeCards = new List<CardBasic>();
        private GameObject _prizeCardBackground;
        private bool _showPrizeCardTitle = false;
        private Rect _prizeCardTitleRect = new Rect(Screen.width / 2 - (PrizeTitleWidth / 2), PrizeTitleInitialY, PrizeTitleWidth, PrizeTitleHeight);
        private bool _showControlPanel = false;
        private Rect _controlPanelRect = new Rect(Screen.width / 2 - (ControlPanelWidth / 2), Screen.height / 2 - (ControlPanelHeight / 2), ControlPanelWidth, ControlPanelHeight);

        public override void OnUpdate()
        {
            HandleCardTooltip();

            if (Input.GetKeyDown(KeyCode.F1))
            {
                _showControlPanel = !_showControlPanel;
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                enableDeckTracker = !enableDeckTracker;
                LoggerInstance.Msg("Toggled Deck Tracker: " + enableDeckTracker.ToString());
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                enablePrizeCards = !enablePrizeCards;
                LoggerInstance.Msg("Toggled Prize Tracker: " + enablePrizeCards.ToString());
            }
            else if (Input.GetKeyDown(KeyCode.C) && SceneManager.GetActiveScene().name == GAME_SCENE_NAME)
            {
                if (_prizeCardsSpawned)
                {
                    DespawnPrizeCardsInWorld();
                }
                else
                {
                    SpawnPrizeCardsInWorld();
                }
            }
        }

        public override void OnGUI()
        {
            // Reset hovered card at the beginning of the GUI frame
            _hoveredCard = null;

            if (enableDeckTracker)
            {
                _deckTrackerWindowRect = GUI.Window(0, _deckTrackerWindowRect, DrawDeckTrackerWindow, "Deck " + "(" + player.deck.GetDeckOwner() + ")");
            }

            if (enablePrizeCards)
            {
                _prizeCardsWindowRect = GUI.Window(1, _prizeCardsWindowRect, DrawPrizeCardsWindow, "Prize Cards");
            }

            if (_showPrizeCardTitle)
            {
                GUI.Window(2, _prizeCardTitleRect, DrawPrizeTitleWindow, "Prize Cards");
            }

            if (_showControlPanel)
            {
                _controlPanelRect = GUI.Window(3, _controlPanelRect, DrawControlPanelWindow, "Mod Controls");
            }
        }

        void DrawDeckTrackerWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            var cards = player.deck.GetCardsForRender();
            var boxHeight = (cards.Count + DeckCounterLines) * LineHeight;
            if (boxHeight == 0)
            {
                boxHeight = DeckTrackerInitialHeight;
            }
            _deckTrackerWindowRect.height = boxHeight + WindowVerticalPadding;

            var totalAssumedCards = player.deck.GetAssumedTotalQuantityOfCards();
            var totalActualCards = player.deck.GetTotalQuantityOfCards();
            var isUncertain = totalAssumedCards != totalActualCards;

            var yOffset = WindowHeaderHeight;
            foreach (var card in cards)
            {
                var deckGUIStyle = new GUIStyle
                {
                    fontSize = FontSize,
                    padding = new RectOffset(TextPadding, TextPadding, TextPadding, TextPadding)
                };

                if (card.highlightState != HighlightState.None)
                {
                    if (Time.time >= card.highlightEndTime)
                    {
                        card.highlightState = HighlightState.None;
                        deckGUIStyle.normal.textColor = Color.white;
                    }
                    else
                    {
                        Color highlightColor = GetHighlightColor(card.highlightState);
                        float elapsedTime = Time.time - (card.highlightEndTime - HighlightDuration);
                        float t = elapsedTime / HighlightDuration;
                        deckGUIStyle.normal.textColor = Color.Lerp(highlightColor, Color.white, t);
                    }
                }
                else
                {
                    deckGUIStyle.normal.textColor = Color.white;
                }

                string cardText = card.card.quantity + " " + card.card.englishName;
                if (isUncertain)
                {
                    cardText += " (?)";
                }
                var cardLabelRect = new Rect(HorizontalMargin, yOffset, _deckTrackerWindowRect.width - TotalHorizontalMargin, LineHeight);
                GUI.Label(cardLabelRect, cardText, deckGUIStyle);

                // Check for hover
                if (cardLabelRect.Contains(Event.current.mousePosition))
                {
                    _hoveredCard = card;
                }

                yOffset += LineHeight;
            }

            var counterGUIStyle = new GUIStyle
            {
                fontSize = FontSize,
                padding = new RectOffset(TextPadding, TextPadding, TextPadding, TextPadding),
                normal = { textColor = Color.white }
            };

            yOffset += LineHeight;
            GUI.Label(new Rect(HorizontalMargin, yOffset, _deckTrackerWindowRect.width - TotalHorizontalMargin, LineHeight), "Total Cards in Deck: " + totalActualCards, counterGUIStyle);
            yOffset += LineHeight;
            GUI.Label(new Rect(HorizontalMargin, yOffset, _deckTrackerWindowRect.width - TotalHorizontalMargin, LineHeight), "Total ASSUMED Cards in Deck: " + totalAssumedCards, counterGUIStyle);
        }

        void DrawPrizeTitleWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        void DrawPrizeCardsWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            var cards = player.GetPrizeCards().GetCardsForRender();
            var boxHeight = (cards.Count + PrizeCounterLines) * LineHeight;
            if (boxHeight == 0)
            {
                boxHeight = PrizeTrackerInitialHeight;
            }
            _prizeCardsWindowRect.height = boxHeight + WindowVerticalPadding;

            var yOffset = WindowHeaderHeight;
            foreach (var card in cards)
            {
                var deckGUIStyle = new GUIStyle
                {
                    fontSize = FontSize,
                    padding = new RectOffset(TextPadding, TextPadding, TextPadding, TextPadding)
                };

                if (card.highlightState != HighlightState.None)
                {
                    if (Time.time >= card.highlightEndTime)
                    {
                        card.highlightState = HighlightState.None;
                        deckGUIStyle.normal.textColor = Color.white;
                    }
                    else
                    {
                        Color highlightColor = GetHighlightColor(card.highlightState);
                        float elapsedTime = Time.time - (card.highlightEndTime - HighlightDuration);
                        float t = elapsedTime / HighlightDuration;
                        deckGUIStyle.normal.textColor = Color.Lerp(highlightColor, Color.white, t);
                    }
                }
                else
                {
                    deckGUIStyle.normal.textColor = Color.white;
                }

                var cardLabelRect = new Rect(HorizontalMargin, yOffset, _prizeCardsWindowRect.width - TotalHorizontalMargin, LineHeight);
                GUI.Label(cardLabelRect, card.card.quantity + " " + card.card.englishName, deckGUIStyle);

                // Check for hover
                if (cardLabelRect.Contains(Event.current.mousePosition))
                {
                    _hoveredCard = card;
                }

                yOffset += LineHeight;
            }

            var counterGUIStyle = new GUIStyle
            {
                fontSize = FontSize,
                padding = new RectOffset(TextPadding, TextPadding, TextPadding, TextPadding),
                normal = { textColor = Color.white }
            };

            yOffset += LineHeight;
            GUI.Label(new Rect(HorizontalMargin, yOffset, _prizeCardsWindowRect.width - TotalHorizontalMargin, LineHeight), "Total Prize Cards: " + player.GetPrizeCards().GetPrizeCount(), counterGUIStyle);
        }

        private Color GetHighlightColor(HighlightState state)
        {
            switch (state)
            {
                case HighlightState.Added:
                    return Color.green;
                case HighlightState.Removed:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        private void HandleCardTooltip()
        {
            // User is hovering over a card
            if (_hoveredCard != null)
            {
                // If this is a new card they're hovering over, reset the timer and set the new pending card
                if (_pendingTooltipCard != _hoveredCard)
                {
                    _pendingTooltipCard = _hoveredCard;
                    _hoverStartTime = Time.time;
                    DespawnTooltipCard(); // Despawn immediately if we move to a new card
                }
                // If enough time has passed and no tooltip is currently shown for this card, spawn it
                else if (Time.time - _hoverStartTime >= TooltipHoverDelay && _currentlyDisplayedTooltipCardId == null)
                {
                    SpawnTooltipCard(_pendingTooltipCard);
                }
            }
            // User is not hovering over any card
            else
            {
                _pendingTooltipCard = null;
                _hoverStartTime = 0f;
                DespawnTooltipCard();
            }
        }

        private void SpawnTooltipCard(TrackedCard card)
        {
            _tooltipCardObject = ManagerSingleton<RainierManager>.instance.cardSpawner.SpawnCardBasic();
            if (_tooltipCardObject == null) return;

            // Use a fixed central position
            _tooltipCardObject.transform.position = Vector3.zero;

            // Use the EXACT same rotation and scale as the 3D prize grid cards
            _tooltipCardObject.transform.rotation = Quaternion.Euler(CardSpawnXRotation, CardSpawnYRotation, 0f);
            _tooltipCardObject.transform.localScale = new Vector3(CardSpawnScale, CardSpawnScale, CardSpawnScale);

            _tooltipCardObject.Init(card.card.cardID);
            _currentlyDisplayedTooltipCardId = card.card.cardID;
        }

        private void DespawnTooltipCard()
        {
            if (_tooltipCardObject != null)
            {
                UnityEngine.Object.Destroy(_tooltipCardObject.gameObject);
                _tooltipCardObject = null;
            }
            _currentlyDisplayedTooltipCardId = null;
        }

        void DrawControlPanelWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginVertical();

            enableDeckTracker = GUILayout.Toggle(enableDeckTracker, "Show Deck Tracker");
            enablePrizeCards = GUILayout.Toggle(enablePrizeCards, "Show Prize Cards");

            if (GUILayout.Button("Show/Hide 3D Prize Cards"))
            {
                if (SceneManager.GetActiveScene().name == GAME_SCENE_NAME)
                {
                    if (_prizeCardsSpawned)
                    {
                        DespawnPrizeCardsInWorld();
                    }
                    else
                    {
                        SpawnPrizeCardsInWorld();
                    }
                }
            }

            if (GUILayout.Button("Reset Window Positions"))
            {
                _deckTrackerWindowRect = new Rect(Screen.width - DefaultWindowWidth, 0, DefaultWindowWidth, DeckTrackerInitialHeight);
                _prizeCardsWindowRect = new Rect(0, PrizeTrackerInitialY, DefaultWindowWidth, PrizeTrackerInitialHeight);
            }

            GUILayout.EndVertical();
        }

        private void SpawnPrizeCardsInWorld()
        {
            var prizeCards = player.GetPrizeCards().GetCardsForRender();
            if (prizeCards.Count == 0) return;

            _prizeCardsSpawned = true;
            _showPrizeCardTitle = true;

            int column = 0;
            int row = 0;

            foreach (var prizeCard in prizeCards)
            {
                for (int i = 0; i < prizeCard.card.quantity; i++)
                {
                    CardBasic cardBasic = ManagerSingleton<RainierManager>.instance.cardSpawner.SpawnCardBasic();
                    Vector3 position = new Vector3(PrizeGridStartX + (column * PrizeGridSpacingX), 14f, PrizeGridStartY + (row * PrizeGridSpacingY));
                    cardBasic.transform.position = position;
                    cardBasic.transform.rotation = Quaternion.Euler(CardSpawnXRotation, CardSpawnYRotation, 0f);
                    cardBasic.transform.localScale = new Vector3(CardSpawnScale, CardSpawnScale, CardSpawnScale);
                    cardBasic.Init(prizeCard.card.cardID);
                    _spawnedPrizeCards.Add(cardBasic);

                    column++;
                    if (column >= PrizeGridColumns)
                    {
                        column = 0;
                        row++;
                    }
                }
            }
        }

        private void DespawnPrizeCardsInWorld()
        {
            _prizeCardsSpawned = false;
            _showPrizeCardTitle = false;
            foreach (var card in _spawnedPrizeCards)
            {
                UnityEngine.Object.Destroy(card.gameObject);
            }
            _spawnedPrizeCards.Clear();

            if (_prizeCardBackground != null)
            {
                UnityEngine.Object.Destroy(_prizeCardBackground);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MatchManager), "SendMatchStartTelemetry")]
        class Patch
        {
            static void Prefix(MatchManager __instance, NetworkMatchController.MatchDetails game)
            {
                var assumedLocalPlayer = game.players[0];
                var playerName = assumedLocalPlayer.playerName;

                if (playerName != NetworkMatchController.GetPlayerName(PlayerID.LOCAL))
                {
                    // Swap to the other player if this isn't our own player name.
                    assumedLocalPlayer = game.players[1];
                    Melon<IronTracks>.Logger.Msg("Player mismatch detected");
                }

                var playerOneName = assumedLocalPlayer.playerName;

                player.username = playerOneName;

                // For other developers to note, it seems that pokemon exposes the 2nd player's information too :/
                // This could prove to be problematic for many reasons I won't get into.
                var playerTwoName = game.players[1].playerName;

                player.deck.SetDeckOwner(playerOneName);

                Melon<IronTracks>.Logger.Msg(playerOneName + " vs. " + playerTwoName);

                player.deck.PopulateDeck(assumedLocalPlayer.deckInfo.cards);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(PlayerCardOwner), "ProcessCardGainedResult")]
        class ProcessCardGainedPatch
        {
            static void Postfix(PlayerCardOwner __instance, OwnerData data, bool gainedFromDrop)
            {
                if (!__instance)
                {
                    return;
                }
                // Make sure that we are targetting the local player (ourself)
                if (__instance.playerID != PlayerID.LOCAL)
                {
                    return;
                }
                player.OnGainCardIntoCollection(data.card, __instance);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(PlayerCardOwner), "ProcessCardRemovalResult")]
        class ProcessCardRemovalResultPatch
        {
            static void Postfix(PlayerCardOwner __instance, OwnerData data, bool droppingCard)
            {
                if (!__instance)
                {
                    return;
                }
                // Make sure that we are targetting the local player (ourself)
                if (__instance.playerID != PlayerID.LOCAL)
                {
                    return;
                }
                player.OnRemovedCardFromCollection(data.card, __instance);
            }
        }
    }

}