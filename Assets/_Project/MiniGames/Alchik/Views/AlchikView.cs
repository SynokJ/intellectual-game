using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace IntellectualGame.Alchik
{
    /// <summary>View: the thrower, the angle arrow, the target ring and the flying knucklebone. Knows no rules.</summary>
    public class AlchikView : MonoBehaviour
    {
        [SerializeField] GameObject stage;
        [SerializeField] Transform startPoint;
        [SerializeField] Transform alchik;
        [SerializeField] Transform angleArrow;
        [SerializeField] Transform target;
        [SerializeField] float spinDegrees = 540f;

        Sequence sequence;

        public void ResetView()
        {
            sequence?.Kill();
            sequence = null;
            alchik.DOKill();
            target.DOKill();
            alchik.position = startPoint.position;
            alchik.rotation = Quaternion.identity;
            target.localScale = Vector3.one;
        }

        public void SetVisible(bool visible) => stage.SetActive(visible);

        /// <summary>Puts the target at its distance, sizes it to the hit radius and turns the arrow to the throw angle.</summary>
        public void ShowRound(AlchikRound round, float hitRadius)
        {
            ResetView();
            target.position = startPoint.position + Vector3.right * round.distance;
            target.localScale = new Vector3(hitRadius * 2f, hitRadius * 2f, 1f);
            angleArrow.rotation = Quaternion.Euler(0f, 0f, round.angle);
        }

        /// <summary>The knucklebone flies along the parabola and stays where it lands.</summary>
        public IEnumerator PlayThrow(float landingDistance, float apex, float flightTime)
        {
            Vector3 start = startPoint.position;
            alchik.position = start;
            sequence?.Kill();
            sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.Append(DOVirtual.Float(0f, 1f, flightTime, t =>
                alchik.position = start + new Vector3(landingDistance * t, apex * 4f * t * (1f - t), 0f)).SetEase(Ease.Linear));
            sequence.Join(alchik.DORotate(new Vector3(0f, 0f, -spinDegrees), flightTime, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            sequence.AppendInterval(0.3f);
            yield return sequence.WaitForCompletion();
        }

        public IEnumerator PlayHit()
        {
            target.DOKill();
            Vector3 size = target.localScale;
            yield return target.DOPunchScale(size * 0.5f, 0.6f, 5).SetLink(target.gameObject).WaitForCompletion();
            target.localScale = size;
        }

        public IEnumerator PlayFinale()
        {
            alchik.position = startPoint.position;
            yield return alchik.DOJump(target.position, 2f, 3, 1.2f).SetLink(gameObject).WaitForCompletion();
            yield return alchik.DOPunchScale(Vector3.one * 0.5f, 0.5f).SetLink(gameObject).WaitForCompletion();
        }
    }
}
