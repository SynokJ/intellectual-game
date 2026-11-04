using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace IntellectualGame.Bazaar
{
    /// <summary>
    /// View of the market: the stall with the merchant, the guests at the dastarkhan, goods, coins and the final beshbarmak.
    /// Plays what the controller asks for and knows nothing about the rules.
    /// </summary>
    public class BazaarStageView : MonoBehaviour
    {
        const int GuestOrder = 10;
        const int ItemOrder = 20;
        const int BubbleOrder = 30;

        [SerializeField] GameObject stage;
        [SerializeField] ItemView itemTemplate;
        [SerializeField] GuestView guestTemplate;
        [SerializeField] GuestView merchant;
        [SerializeField] Transform bubbleTemplate;
        [SerializeField] Transform plate;

        [Header("Sprites")]
        [SerializeField] Sprite discSprite;
        [SerializeField] Sprite blockSprite;

        [Header("Anchors")]
        [SerializeField] Transform goodsAnchor;
        [SerializeField] Transform priceAnchor;
        [SerializeField] Transform walletAnchor;
        [SerializeField] Transform guestAnchor;
        [SerializeField] Transform jarAnchor;

        [Header("Layout")]
        [SerializeField] float goodsSize = 1.1f;
        [SerializeField] float coinSize = 0.45f;
        [SerializeField] float miniSize = 0.3f;
        [SerializeField] float goodsSpacing = 1.4f;
        [SerializeField] float guestSpacing = 1.5f;
        [Tooltip("A row of guests / goods never gets wider than this; the spacing shrinks instead.")]
        [SerializeField] float guestRowWidth = 7.2f;
        [SerializeField] float goodsRowWidth = 5.6f;
        [SerializeField] float coinSpacing = 0.45f;
        [SerializeField] float miniSpacing = 0.36f;
        [SerializeField] int walletColumns = 5;
        [SerializeField] Vector2 bubbleOffset = new Vector2(0f, 2.5f);
        [SerializeField] Vector2 servingOffset = new Vector2(0f, 0.15f);
        [SerializeField] float servingSpacing = 0.45f;

        [Header("Colors")]
        [SerializeField] Color coinColor = new Color(0.95f, 0.77f, 0.2f);
        [SerializeField] Color hatColor = Color.white;
        [SerializeField] Color[] robeColors =
        {
            new Color(0.25f, 0.5f, 0.75f), new Color(0.8f, 0.3f, 0.3f), new Color(0.3f, 0.65f, 0.4f), new Color(0.6f, 0.4f, 0.7f),
        };

        readonly List<GameObject> spawned = new List<GameObject>();
        readonly List<GuestView> guests = new List<GuestView>();
        readonly List<ItemView> priceCoins = new List<ItemView>();
        readonly List<ItemView> walletCoins = new List<ItemView>();
        Sequence sequence;

        void Awake()
        {
            itemTemplate.gameObject.SetActive(false);
            guestTemplate.gameObject.SetActive(false);
            bubbleTemplate.gameObject.SetActive(false);
        }

        public void ResetView()
        {
            sequence?.Kill();
            sequence = null;

            ClearRound();
            plate.DOKill();
            plate.gameObject.SetActive(false);
            merchant.SetOrder(GuestOrder - 5);
        }

        public void SetVisible(bool visible) => stage.SetActive(visible);

        /// <summary>Puts the situation of the round on the stage: guests with their wishes, goods with prices, a wallet.</summary>
        public IEnumerator PlayIntro(BazaarRound round, BazaarProduct product)
        {
            ClearRound();
            sequence = DOTween.Sequence().SetLink(gameObject);

            switch (round.kind)
            {
                case BazaarRoundKind.Feed: BuildFeed(round, product); break;
                case BazaarRoundKind.Pay: BuildPay(round, product); break;
                default: BuildChange(round, product); break;
            }

            yield return sequence.WaitForCompletion();
        }

        /// <summary>The round is solved: goods go to the guests or coins go to the merchant.</summary>
        public IEnumerator PlayCorrect(BazaarRound round, BazaarProduct product)
        {
            sequence?.Kill();
            sequence = DOTween.Sequence().SetLink(gameObject);

            switch (round.kind)
            {
                case BazaarRoundKind.Feed: ServeGuests(round, product); break;
                case BazaarRoundKind.Pay: PayMerchant(priceCoins, priceCoins.Count); break;
                default: PayMerchant(walletCoins, round.b); break;
            }

            yield return sequence.WaitForCompletion();
        }

        /// <summary>The whole run is solved: a dish of beshbarmak is served and everybody cheers.</summary>
        public IEnumerator PlayFinale()
        {
            sequence?.Kill();
            ClearRound();
            sequence = DOTween.Sequence().SetLink(gameObject);

            plate.gameObject.SetActive(true);
            plate.localScale = Vector3.zero;
            sequence.Append(plate.DOScale(1f, 0.6f).SetEase(Ease.OutBack));

            const int guestCount = 4;
            for (int i = 0; i < guestCount; i++)
            {
                var guest = SpawnGuest(i, RowPoint(guestAnchor.position, i, guestCount, guestSpacing * 1.3f));
                sequence.Join(guest.PopIn(0.1f * i));
            }

            sequence.AppendInterval(0.2f);
            foreach (var guest in guests) sequence.Join(guest.Cheer(0f));
            sequence.Join(merchant.Cheer(0f));
            sequence.AppendInterval(0.8f);

            yield return sequence.WaitForCompletion();
        }

        // ---------- rounds ----------

        void BuildFeed(BazaarRound round, BazaarProduct product)
        {
            sequence.Join(SpawnProduct(product, goodsAnchor.position, goodsSize).PopIn(0f));

            float spacing = Mathf.Min(guestSpacing, guestRowWidth / round.a);
            for (int i = 0; i < round.a; i++)
            {
                Vector3 spot = RowPoint(guestAnchor.position, i, round.a, spacing);
                var guest = SpawnGuest(i, spot);
                sequence.Join(guest.PopIn(0.25f * i));

                // a thought bubble shows how many products this guest wants
                var bubble = Spawn(bubbleTemplate.gameObject).transform;
                bubble.position = spot + (Vector3)bubbleOffset;
                bubble.GetComponent<SpriteRenderer>().sortingOrder = BubbleOrder;
                for (int k = 0; k < round.b; k++)
                {
                    var mini = SpawnProduct(product, GridPoint(bubble.position, k, round.b, 2, miniSpacing), miniSize);
                    mini.SetOrder(BubbleOrder + 1);
                    mini.transform.SetParent(bubble, true);
                }
                Vector3 bubbleSize = bubble.localScale;
                bubble.localScale = Vector3.zero;
                sequence.Join(bubble.DOScale(bubbleSize, 0.3f).SetEase(Ease.OutBack).SetDelay(0.25f * i + 0.2f).SetLink(bubble.gameObject));
            }
        }

        void BuildPay(BazaarRound round, BazaarProduct product)
        {
            Vector3 priceOffset = priceAnchor.position - goodsAnchor.position;
            float spacing = Mathf.Min(goodsSpacing, goodsRowWidth / round.a);
            for (int i = 0; i < round.a; i++)
            {
                Vector3 spot = RowPoint(goodsAnchor.position, i, round.a, spacing);
                var item = SpawnProduct(product, spot, goodsSize);
                sequence.Join(item.PopIn(0.2f * i));

                // the price: a small pile of coins above every product
                for (int k = 0; k < round.b; k++)
                {
                    var coin = SpawnCoin(GridPoint(spot + priceOffset, k, round.b, 2, coinSpacing));
                    priceCoins.Add(coin);
                    sequence.Join(coin.PopIn(0.2f * i + 0.1f * k + 0.2f));
                }
            }
        }

        void BuildChange(BazaarRound round, BazaarProduct product)
        {
            sequence.Join(SpawnProduct(product, goodsAnchor.position, goodsSize).PopIn(0f));

            for (int k = 0; k < round.b; k++)
            {
                var coin = SpawnCoin(GridPoint(priceAnchor.position, k, round.b, 4, coinSpacing));
                priceCoins.Add(coin);
                sequence.Join(coin.PopIn(0.1f * k + 0.2f));
            }

            for (int k = 0; k < round.a; k++)
            {
                var coin = SpawnCoin(GridPoint(walletAnchor.position, k, round.a, walletColumns, coinSpacing * 1.3f));
                walletCoins.Add(coin);
                sequence.Join(coin.PopIn(0.1f * k + 0.3f));
            }
        }

        void ServeGuests(BazaarRound round, BazaarProduct product)
        {
            float delay = 0f;
            for (int g = 0; g < guests.Count; g++)
            {
                Vector3 spot = guests[g].transform.position + (Vector3)servingOffset;
                for (int k = 0; k < round.b; k++)
                {
                    var item = SpawnProduct(product, goodsAnchor.position, goodsSize * 0.6f);
                    Vector3 target = RowPoint(spot, k, round.b, servingSpacing);
                    sequence.Join(item.JumpTo(target, delay));
                    delay += 0.1f;
                }
            }

            foreach (var guest in guests) sequence.Insert(delay + 0.5f, guest.Cheer(0f));
            sequence.AppendInterval(0.4f);
        }

        void PayMerchant(List<ItemView> coins, int count)
        {
            float delay = 0f;
            var paid = new List<ItemView>();
            for (int i = coins.Count - 1; i >= 0 && paid.Count < count; i--) paid.Add(coins[i]);

            foreach (var coin in paid)
            {
                sequence.Join(coin.JumpTo(jarAnchor.position, delay));
                sequence.Insert(delay + 0.5f, coin.Shrink(0f));
                delay += 0.08f;
            }

            // coins that stay in the wallet are the change: they pulse
            foreach (var coin in coins)
                if (!paid.Contains(coin)) sequence.Insert(delay + 0.5f, coin.Pulse(0f));

            sequence.Insert(delay + 0.5f, merchant.Cheer(0f));
            sequence.AppendInterval(0.5f);
        }

        // ---------- spawning ----------

        ItemView SpawnProduct(BazaarProduct product, Vector3 position, float size)
        {
            var item = Spawn(itemTemplate.gameObject).GetComponent<ItemView>();
            item.transform.position = position;
            item.Style(product.shape == ProductShape.Disc ? discSprite : blockSprite, product.color, size);
            item.SetOrder(ItemOrder);
            return item;
        }

        ItemView SpawnCoin(Vector3 position)
        {
            var coin = Spawn(itemTemplate.gameObject).GetComponent<ItemView>();
            coin.transform.position = position;
            coin.Style(discSprite, coinColor, coinSize);
            coin.SetOrder(ItemOrder + 1);
            return coin;
        }

        GuestView SpawnGuest(int index, Vector3 position)
        {
            var guest = Spawn(guestTemplate.gameObject).GetComponent<GuestView>();
            guest.transform.position = position;
            guest.Dress(robeColors[index % robeColors.Length], hatColor);
            guest.SetOrder(GuestOrder);
            guests.Add(guest);
            return guest;
        }

        GameObject Spawn(GameObject template)
        {
            var clone = Instantiate(template, template.transform.parent);
            clone.name = template.name.Replace(" Template", string.Empty);
            clone.SetActive(true);
            spawned.Add(clone);
            return clone;
        }

        void ClearRound()
        {
            foreach (var clone in spawned)
                if (clone != null) Destroy(clone);
            spawned.Clear();
            guests.Clear();
            priceCoins.Clear();
            walletCoins.Clear();
        }

        // ---------- layout helpers ----------

        static Vector3 RowPoint(Vector3 center, int index, int count, float spacing) =>
            center + Vector3.right * ((index - (count - 1) / 2f) * spacing);

        static Vector3 GridPoint(Vector3 center, int index, int count, int columns, float spacing)
        {
            int rows = Mathf.CeilToInt(count / (float)columns);
            int row = index / columns;
            int inRow = Mathf.Min(columns, count - row * columns);
            float x = (index % columns - (inRow - 1) / 2f) * spacing;
            float y = ((rows - 1) / 2f - row) * spacing;
            return center + new Vector3(x, y, 0f);
        }
    }
}
