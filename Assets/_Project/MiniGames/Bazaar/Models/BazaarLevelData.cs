using System;
using System.Collections.Generic;
using System.Linq;
using IntellectualGame.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IntellectualGame.Bazaar
{
    public enum BazaarRoundKind { Feed, Pay, Change }

    public enum ProductShape { Disc, Block }

    [Serializable]
    public struct BazaarProduct
    {
        [Tooltip("Singular, e.g. 'лепёшка' (1 лепёшка).")] public string one;
        [Tooltip("2-4, e.g. 'лепёшки'.")] public string few;
        [Tooltip("5 and more, e.g. 'лепёшек'.")] public string many;
        public Color color;
        public ProductShape shape;

        public string Form(int count) => Plural.Pick(count, one, few, many);
    }

    /// <summary>
    /// One shopping problem. Feed: a guests, each needs b products - how many to buy.
    /// Pay: a products, b coins each - how many coins to pay. Change: a coins, a product costs b - how much change.
    /// </summary>
    [Serializable]
    public struct BazaarRound
    {
        public BazaarRoundKind kind;
        public int productIndex;
        public int a;
        public int b;

        public int Answer => kind == BazaarRoundKind.Change ? a - b : a * b;
    }

    /// <summary>
    /// Model (config): the market goods and the rounds of the game. Either a fixed list, or new random rounds generated
    /// at the start of every run: feeding guests, paying for goods and counting change in a mixed order.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Bazaar/Level", fileName = "BazaarLevel")]
    public class BazaarLevelData : ScriptableObject
    {
        [Tooltip("On: generate new random rounds every run. Off: use the fixed list below.")]
        [SerializeField] bool randomize = true;
        [Tooltip("Random: rounds 'guests come, how many products to buy'.")]
        [SerializeField, Min(0)] int feedRounds = 2;
        [Tooltip("Random: rounds 'how many coins to pay'.")]
        [SerializeField, Min(0)] int payRounds = 2;
        [Tooltip("Random: rounds 'how much change'.")]
        [SerializeField, Min(0)] int changeRounds = 2;
        [Tooltip("Random: the biggest answer, also the biggest amount of coins.")]
        [SerializeField, Range(6, 12)] int maxAnswer = 9;
        [SerializeField] List<BazaarProduct> products = new List<BazaarProduct>();
        [SerializeField] List<BazaarRound> rounds = new List<BazaarRound>();

        public IReadOnlyList<BazaarProduct> Products => products;
        public IReadOnlyList<BazaarRound> Rounds => rounds;

        /// <summary>The biggest answer the level can produce; the answer pad gets at least this many buttons.</summary>
        public int MaxAnswer => randomize ? maxAnswer : (rounds.Count == 0 ? 0 : rounds.Max(round => round.Answer));

        /// <summary>The rounds of one run: random ones, or a copy of the fixed list.</summary>
        public List<BazaarRound> CreateRun()
        {
            if (!randomize) return new List<BazaarRound>(rounds);

            var run = new List<BazaarRound>();
            for (int i = 0; i < feedRounds; i++) run.Add(RandomFeed());
            for (int i = 0; i < payRounds; i++) run.Add(RandomPay());
            for (int i = 0; i < changeRounds; i++) run.Add(RandomChange());
            Shuffle(run);
            return run;
        }

        public string Question(BazaarRound round)
        {
            var product = products[round.productIndex];
            switch (round.kind)
            {
                case BazaarRoundKind.Feed:
                    return $"К тебе пришли {round.a} {Plural.Pick(round.a, "гость", "гостя", "гостей")}. " +
                           $"Каждому нужно по {round.b} {product.Form(round.b)}.\nСколько всего нужно купить?";
                case BazaarRoundKind.Pay:
                    return $"Одна штука стоит {round.b} {Coin(round.b)}.\n" +
                           $"Сколько монет заплатить за {round.a} {product.Form(round.a)}?";
                default:
                    return $"У тебя {round.a} {Coin(round.a)}. {Capitalize(product.one)} стоит {round.b} {Coin(round.b)}.\n" +
                           "Сколько монет сдачи?";
            }
        }

        BazaarRound RandomFeed()
        {
            int guests = Random.Range(2, maxAnswer / 2 + 1);
            return new BazaarRound { kind = BazaarRoundKind.Feed, productIndex = RandomProduct(), a = guests, b = Random.Range(2, maxAnswer / guests + 1) };
        }

        BazaarRound RandomPay()
        {
            int count = Random.Range(2, maxAnswer / 2 + 1);
            return new BazaarRound { kind = BazaarRoundKind.Pay, productIndex = RandomProduct(), a = count, b = Random.Range(2, maxAnswer / count + 1) };
        }

        BazaarRound RandomChange()
        {
            int money = Random.Range(3, maxAnswer + 1);
            return new BazaarRound { kind = BazaarRoundKind.Change, productIndex = RandomProduct(), a = money, b = Random.Range(1, money) };
        }

        int RandomProduct() => Random.Range(0, products.Count);

        static void Shuffle(List<BazaarRound> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        static string Coin(int count) => Plural.Pick(count, "монета", "монеты", "монет");

        static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpper(text[0]) + text.Substring(1);

        void OnValidate()
        {
            for (int i = 0; i < rounds.Count; i++)
            {
                var round = rounds[i];
                if (round.productIndex < 0 || round.productIndex >= products.Count)
                    Debug.LogWarning($"{name}: round {i + 1} uses a product that does not exist.", this);
                if (round.Answer < 1)
                    Debug.LogWarning($"{name}: round {i + 1} has answer {round.Answer}, but the buttons start at 1.", this);
            }
        }
    }
}
