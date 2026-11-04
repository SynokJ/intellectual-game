using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Otara
{
    /// <summary>View: the pen, the families and the sheep walking between them. Knows nothing about the rules.</summary>
    public class OtaraView : MonoBehaviour
    {
        [SerializeField] GameObject stage;
        [SerializeField] GameObject pen;
        [SerializeField] GameObject hudRoot;
        [SerializeField] SheepView sheepTemplate;
        [SerializeField] Transform[] familyYurts;
        [SerializeField] Text questionText;
        [SerializeField] Text eventText;
        [SerializeField] Text feedbackText;
        [SerializeField] Text progressText;

        static readonly Color CorrectColor = new Color(0.15f, 0.55f, 0.2f);
        static readonly Color WrongColor = new Color(0.75f, 0.15f, 0.15f);

        // pen slots are filled front/back alternately so the pen never looks lopsided
        static readonly Vector2[] PenSlots =
        {
            new Vector2(0.5f, 0.1f), new Vector2(1.1f, 0.7f), new Vector2(1.75f, 0.1f), new Vector2(2.35f, 0.7f),
            new Vector2(3.0f, 0.1f), new Vector2(3.6f, 0.7f), new Vector2(4.25f, 0.1f),
        };
        const float FrontScale = 0.8f;
        const float BackScale = 0.68f;

        static readonly Vector2 GatePoint = new Vector2(-1.3f, 0.4f);
        static readonly Vector2 OffscreenLeft = new Vector2(-9.8f, 0.4f);
        static readonly float[] TwoFamilies = { 1.5f, 5.0f };
        static readonly float[] ThreeFamilies = { 0f, 3.2f, 6.4f };
        const float FamilyBaseY = 0.35f;
        const float FamilySheepY = -0.1f;
        const float SheepSpacing = 0.95f;

        readonly SheepView[] penSheep = new SheepView[PenSlots.Length];
        readonly List<SheepView> flock = new List<SheepView>();
        Vector2 questionHome;
        Sequence feedbackSequence;

        void Awake()
        {
            sheepTemplate.gameObject.SetActive(false);
            questionHome = questionText.rectTransform.anchoredPosition;
        }

        public void ResetView()
        {
            StopAllCoroutines();
            StopFeedback();

            ClearFlock();
            pen.SetActive(false);
            foreach (var yurt in familyYurts) yurt.gameObject.SetActive(false);

            questionText.rectTransform.anchoredPosition = questionHome;
            questionText.rectTransform.localScale = Vector3.one;
            questionText.text = string.Empty;
            eventText.text = string.Empty;
            feedbackText.text = string.Empty;
            progressText.text = string.Empty;
        }

        public void SetVisible(bool visible)
        {
            stage.SetActive(visible);
            hudRoot.SetActive(visible);
        }

        public void ShowQuestion(string text) => questionText.text = text;

        public void HideQuestion()
        {
            questionText.text = string.Empty;
            feedbackText.text = string.Empty;
        }

        public void ShowEvent(string text) => eventText.text = text;

        public void ShowFeedback(bool correct)
        {
            StopFeedback();
            feedbackText.color = correct ? CorrectColor : WrongColor;
            feedbackText.text = correct ? "Верно!" : "Неверно, попробуй ещё";

            feedbackSequence = DOTween.Sequence().SetLink(gameObject);
            if (!correct)
                feedbackSequence.Append(questionText.rectTransform.DOShakeAnchorPos(0.35f, new Vector2(18f, 0f), 13, 0f));
            feedbackSequence.AppendInterval(0.7f).OnComplete(() =>
            {
                feedbackText.text = string.Empty;
                feedbackSequence = null;
            });
        }

        /// <summary>Clears the previous round and puts the sheep that are already there into place.</summary>
        public void PrepareRound(OtaraRound round, int number, int total)
        {
            ClearFlock();
            eventText.text = string.Empty;
            progressText.text = $"Раунд {number} из {total}";

            if (round.kind == OtaraRoundKind.Count)
            {
                pen.SetActive(true);
                foreach (var yurt in familyYurts) yurt.gameObject.SetActive(false);
                for (int i = 0; i < round.startCount; i++) PlaceInPen(Spawn(), i);
            }
            else
            {
                pen.SetActive(false);
                for (int i = 0; i < familyYurts.Length; i++)
                    familyYurts[i].gameObject.SetActive(i < round.families);
                for (int i = 0; i < round.startCount; i++)
                    Spawn().Place(new Vector2(-7.2f + SheepSpacing * (i % 3), 0.05f + 0.6f * (i / 3)), FrontScale);
            }
        }

        /// <summary>Sheep enter (positive) or leave (negative) the pen through the gate.</summary>
        public IEnumerator PlayMove(int delta)
        {
            var routines = new List<IEnumerator>();
            if (delta > 0)
            {
                ShowEvent($"Зашли в загон: {delta} {SheepWord(delta)}");
                int added = 0;
                for (int slot = 0; slot < penSheep.Length && added < delta; slot++)
                {
                    if (penSheep[slot] != null) continue;
                    var sheep = Spawn();
                    sheep.Place(OffscreenLeft, FrontScale);
                    penSheep[slot] = sheep;
                    routines.Add(Delayed(added * 0.45f, EnterPen(sheep, slot)));
                    added++;
                }
            }
            else if (delta < 0)
            {
                ShowEvent($"Ушли из загона: {-delta} {SheepWord(-delta)}");
                int removed = 0;
                for (int slot = penSheep.Length - 1; slot >= 0 && removed < -delta; slot--)
                {
                    if (penSheep[slot] == null) continue;
                    var sheep = penSheep[slot];
                    penSheep[slot] = null;
                    routines.Add(Delayed(removed * 0.45f, LeavePen(sheep)));
                    removed++;
                }
            }

            yield return RunAll(routines);
        }

        /// <summary>Every sheep walks to its family, one after another, until each family has the same number.</summary>
        public IEnumerator PlayShare(int families)
        {
            ShowEvent("Овцы расходятся по семьям");
            float[] xs = families == 2 ? TwoFamilies : ThreeFamilies;
            for (int i = 0; i < families; i++)
                familyYurts[i].position = new Vector3(xs[i], FamilyBaseY, 0f);

            int perFamily = flock.Count / families;
            var routines = new List<IEnumerator>();
            for (int k = 0; k < flock.Count; k++)
            {
                int family = k % families;
                int index = k / families;
                float x = xs[family] + (index - (perFamily - 1) / 2f) * SheepSpacing;
                routines.Add(Delayed(k * 0.35f, flock[k].WalkTo(new Vector2(x, FamilySheepY))));
            }

            yield return RunAll(routines);
        }

        public IEnumerator PlayCelebrate()
        {
            var routines = new List<IEnumerator>();
            for (int i = 0; i < flock.Count; i++)
                routines.Add(Delayed(i * 0.08f, flock[i].Cheer(2)));
            yield return RunAll(routines);
        }

        IEnumerator EnterPen(SheepView sheep, int slot)
        {
            yield return sheep.WalkTo(new Vector2(GatePoint.x, PenSlots[slot].y));
            yield return sheep.WalkTo(PenSlots[slot]);
            sheep.Place(PenSlots[slot], SlotScale(slot));
        }

        IEnumerator LeavePen(SheepView sheep)
        {
            yield return sheep.WalkTo(GatePoint);
            yield return sheep.WalkTo(OffscreenLeft);
            flock.Remove(sheep);
            Destroy(sheep.gameObject);
        }

        void StopFeedback()
        {
            feedbackSequence?.Kill();
            feedbackSequence = null;
            questionText.rectTransform.anchoredPosition = questionHome;
        }

        SheepView Spawn()
        {
            var sheep = Instantiate(sheepTemplate, sheepTemplate.transform.parent);
            sheep.name = "Sheep";
            sheep.gameObject.SetActive(true);
            flock.Add(sheep);
            return sheep;
        }

        void PlaceInPen(SheepView sheep, int slot)
        {
            penSheep[slot] = sheep;
            sheep.Place(PenSlots[slot], SlotScale(slot));
        }

        void ClearFlock()
        {
            foreach (var sheep in flock)
                if (sheep != null) Destroy(sheep.gameObject);
            flock.Clear();
            Array.Clear(penSheep, 0, penSheep.Length);
        }

        static float SlotScale(int slot) => PenSlots[slot].y > 0.3f ? BackScale : FrontScale;

        IEnumerator Delayed(float delay, IEnumerator routine)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            yield return routine;
        }

        IEnumerator RunAll(List<IEnumerator> routines)
        {
            int pending = routines.Count;
            foreach (var routine in routines)
                StartCoroutine(Track(routine, () => pending--));
            while (pending > 0) yield return null;
        }

        static IEnumerator Track(IEnumerator routine, Action done)
        {
            yield return routine;
            done();
        }

        static string SheepWord(int n)
        {
            int lastTwo = n % 100, last = n % 10;
            if (lastTwo >= 11 && lastTwo <= 14) return "овец";
            if (last == 1) return "овца";
            if (last >= 2 && last <= 4) return "овцы";
            return "овец";
        }
    }
}
