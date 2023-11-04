using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using HarmonyLib;
using MelonLoader;

namespace PTCGLDeckTracker
{
    public class IronTracks : MelonMod
    {
        bool enableDeckTracker = false;
        static Dictionary<string, int> deck = new Dictionary<string, int>();
        static Dictionary<string, int> deckWithIds = new Dictionary<string, int>();
        static List<string> deckRenderOrder = new List<string>();

        public static int GetTotalQuantityOfCards()
        {
            int total = 0;
            foreach (KeyValuePair<string, int> kvp in deck)
            {
                total += kvp.Value;
            }
            return total;
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                enableDeckTracker = !enableDeckTracker;
                LoggerInstance.Msg("Toggled deck tracker: " + enableDeckTracker.ToString());
            }
        }

        public override void OnGUI()
        {
            if (!enableDeckTracker)
            {
                return;
            }
            var width = 250;
            var location = new Rect(Screen.width - width, 0, width, Screen.height);
            var textLocation = new Rect(Screen.width - width + 5, 25, width, Screen.height);
            GUI.Box(location, "Deck");
            string deckString = "";
            if (deck.Count != 0)
            {
                foreach (var card in deckRenderOrder)
                {
                    deckString += deck[card] + " " + card + "\n";
                }
                deckString += "\nTotal Cards in Deck: " + IronTracks.GetTotalQuantityOfCards();
            }
            GUI.Label(textLocation, deckString);
        }

        [HarmonyLib.HarmonyPatch(typeof(MatchManager), "SendMatchStartTelemetry")]
        class Patch
        {
            static void Prefix(MatchManager __instance, NetworkMatchController.MatchDetails game)
            {
                var playerOneName = game.players[0].playerName;
                var playerTwoName = game.players[1].playerName;
                Melon<IronTracks>.Logger.Msg(playerOneName + " vs. " + playerTwoName);

                IronTracks.deck.Clear();
                IronTracks.deckWithIds.Clear();

                var pokemons = new List<string>();
                var trainers = new List<string>();
                var energies = new List<string>();

                foreach (var pair in game.players[0].deckInfo.cards)
                {
                    var quantity = pair.Value;
                    var cardID = pair.Key;
                    CardDatabase.DataAccess.CardDataRow cdr = ManagerSingleton<CardDatabaseManager>.instance.TryGetCardFromDatabase(cardID);
                    IronTracks.deck[cdr.EnglishCardName] = quantity;
                    IronTracks.deckWithIds[cardID] = quantity;

                    if (cdr.IsPokemonCard())
                    {
                        pokemons.Add(cdr.EnglishCardName);
                    }
                    else if (cdr.IsTrainerCard())
                    {
                        trainers.Add(cdr.EnglishCardName);
                    }
                    else
                    {
                        energies.Add(cdr.EnglishCardName);
                    }
                }

                IronTracks.deckRenderOrder.Clear();

                foreach (var item in pokemons)
                {
                    IronTracks.deckRenderOrder.Add(item);
                }
                foreach (var item in trainers)
                {
                    IronTracks.deckRenderOrder.Add(item);
                }
                foreach (var item in energies)
                {
                    IronTracks.deckRenderOrder.Add(item);
                }
            }
        }
    }

}
