using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace IntellectualGame.Kerege
{
    /// <summary>View: the kerege lattice behind, the empty word slots and the shuffled letter tiles. Knows no rules.</summary>
    public class KeregeView : MonoBehaviour
    {
        [SerializeField] GameObject stage;
        [SerializeField] GameObject lettersRoot;
        [SerializeField] RectTransform slotContainer;
        [SerializeField] RectTransform tileContainer;
        [SerializeField] LetterTileView tileTemplate;
        [SerializeField] Transform lattice;

        readonly List<LetterTileView> slots = new List<LetterTileView>();
        readonly List<LetterTileView> tiles = new List<LetterTileView>();

        void Awake() => tileTemplate.gameObject.SetActive(false);

        public void ResetView()
        {
            Clear();
            lattice.DOKill();
            lattice.localScale = Vector3.one;
        }

        public void SetVisible(bool visible)
        {
            stage.SetActive(visible);
            lettersRoot.SetActive(visible);
        }

        /// <summary>Builds empty slots for the word and one numbered tile per shuffled letter.</summary>
        public void ShowRound(KeregeRound round)
        {
            Clear();
            for (int i = 0; i < round.word.Length; i++)
            {
                var slot = Spawn(slotContainer);
                slot.Set("_", string.Empty);
                slots.Add(slot);
            }

            for (int i = 0; i < round.tiles.Length; i++)
            {
                var tile = Spawn(tileContainer);
                tile.Set(round.tiles[i].ToString().ToUpper(), (i + 1).ToString());
                tiles.Add(tile);
                tile.Pop();
            }
        }

        public void PlaceLetter(int tileIndex, int slotIndex)
        {
            slots[slotIndex].SetLetter(tiles[tileIndex].Letter);
            slots[slotIndex].Pop();
            tiles[tileIndex].SetUsed();
        }

        public void ShakeTile(int tileIndex)
        {
            if (tileIndex >= 0 && tileIndex < tiles.Count) tiles[tileIndex].Shake();
        }

        public IEnumerator PlayWordDone()
        {
            var sequence = DOTween.Sequence().SetLink(gameObject);
            for (int i = 0; i < slots.Count; i++) sequence.Join(slots[i].Pop().SetDelay(0.08f * i));
            sequence.Join(lattice.DOPunchScale(Vector3.one * 0.08f, 0.5f));
            sequence.AppendInterval(0.4f);
            yield return sequence.WaitForCompletion();
        }

        LetterTileView Spawn(RectTransform container)
        {
            var tile = Instantiate(tileTemplate, container);
            tile.gameObject.SetActive(true);
            return tile;
        }

        void Clear()
        {
            foreach (var tile in slots) if (tile != null) Destroy(tile.gameObject);
            foreach (var tile in tiles) if (tile != null) Destroy(tile.gameObject);
            slots.Clear();
            tiles.Clear();
        }
    }
}
