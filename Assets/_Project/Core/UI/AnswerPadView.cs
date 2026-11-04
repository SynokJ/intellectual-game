using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Core
{
    /// <summary>View: a pad of numbered answer buttons created at runtime. Reports presses, knows nothing about the rules.</summary>
    public class AnswerPadView : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] RectTransform container;
        [SerializeField] Button buttonTemplate;

        readonly List<Button> buttons = new List<Button>();

        public event Action<int> AnswerPressed;

        void Awake() => buttonTemplate.gameObject.SetActive(false);

        public void Show(int count)
        {
            Clear();
            for (int number = 1; number <= count; number++)
            {
                var button = Instantiate(buttonTemplate, container);
                button.name = "Answer Button " + number;
                button.GetComponentInChildren<Text>().text = number.ToString();
                button.gameObject.SetActive(true);

                int value = number;
                button.onClick.AddListener(() =>
                {
                    button.transform.DOKill(true);
                    button.transform.DOPunchScale(Vector3.one * 0.12f, 0.2f).SetLink(button.gameObject);
                    AnswerPressed?.Invoke(value);
                });
                buttons.Add(button);
            }
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
            Clear();
        }

        void Clear()
        {
            foreach (var button in buttons)
                if (button != null) Destroy(button.gameObject);
            buttons.Clear();
        }
    }
}
