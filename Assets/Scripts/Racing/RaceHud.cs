using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BoardRacing
{
    public sealed class RaceHud : MonoBehaviour
    {
        private Text topRedText;
        private Text topBlueText;
        private Text topPositionText;
        private Text bottomRedText;
        private Text bottomBlueText;
        private Text bottomPositionText;
        private Text winnerTopText;
        private Text winnerBottomText;
        private GameObject restartButton;

        public static RaceHud Create(Transform parent)
        {
            EnsureEventSystem();

            var hudObject = new GameObject("Race HUD", typeof(RectTransform));
            hudObject.transform.SetParent(parent, false);
            var hud = hudObject.AddComponent<RaceHud>();
            hud.Build();
            return hud;
        }

        public void UpdateRace(IReadOnlyList<RaceProgressTracker> racers, bool raceFinished, RaceProgressTracker winner)
        {
            var red = racers.FirstOrDefault(racer => racer.RacerName == "RED");
            var blue = racers.FirstOrDefault(racer => racer.RacerName == "BLUE");

            SetText(topRedText, FormatRacer(red));
            SetText(bottomRedText, FormatRacer(red));
            SetText(topBlueText, FormatRacer(blue));
            SetText(bottomBlueText, FormatRacer(blue));

            var leader = racers.OrderBy(racer => racer.CurrentPosition).FirstOrDefault();
            var position = leader == null ? "" : $"LEADER: {leader.RacerName}";
            SetText(topPositionText, position);
            SetText(bottomPositionText, position);

            var winnerText = raceFinished && winner != null
                ? $"{winner.RacerName} WINS\nPress OPTIONS or X to restart"
                : "";
            SetText(winnerTopText, winnerText);
            SetText(winnerBottomText, winnerText);
        }

        public void SetRestartVisible(bool visible)
        {
            restartButton.SetActive(visible);
        }

        private void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            var topHud = CreatePanel("Top HUD", Vector2.up, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -72f), 0f);
            topRedText = CreateHudText(topHud, "Top Red", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(260f, 0f), TextAnchor.MiddleLeft, Color.red);
            topPositionText = CreateHudText(topHud, "Top Position", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            topBlueText = CreateHudText(topHud, "Top Blue", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-260f, 0f), TextAnchor.MiddleRight, new Color(0.25f, 0.55f, 1f));

            var bottomHud = CreatePanel("Bottom HUD", Vector2.zero, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 72f), 180f);
            bottomRedText = CreateHudText(bottomHud, "Bottom Red", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(260f, 0f), TextAnchor.MiddleLeft, Color.red);
            bottomPositionText = CreateHudText(bottomHud, "Bottom Position", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            bottomBlueText = CreateHudText(bottomHud, "Bottom Blue", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-260f, 0f), TextAnchor.MiddleRight, new Color(0.25f, 0.55f, 1f));

            winnerTopText = CreateWinnerText("Winner Top", new Vector2(0.5f, 0.5f), new Vector2(0f, 128f), 0f);
            winnerBottomText = CreateWinnerText("Winner Bottom", new Vector2(0.5f, 0.5f), new Vector2(0f, -128f), 180f);
            restartButton = CreateRestartButton();
            restartButton.SetActive(false);
        }

        private RectTransform CreatePanel(string name, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, float rotation)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(1840f, 118f);
            rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
            return rect;
        }

        private Text CreateHudText(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, TextAnchor alignment, Color color)
        {
            var text = CreateText(name, parent);
            text.rectTransform.anchorMin = anchorMin;
            text.rectTransform.anchorMax = anchorMax;
            text.rectTransform.pivot = anchorMin;
            text.rectTransform.anchoredPosition = anchoredPosition;
            text.rectTransform.sizeDelta = new Vector2(520f, 110f);
            text.alignment = alignment;
            text.fontSize = 42;
            text.color = color;
            return text;
        }

        private Text CreateWinnerText(string name, Vector2 anchor, Vector2 anchoredPosition, float rotation)
        {
            var text = CreateText(name, transform as RectTransform);
            text.rectTransform.anchorMin = anchor;
            text.rectTransform.anchorMax = anchor;
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchoredPosition = anchoredPosition;
            text.rectTransform.sizeDelta = new Vector2(1050f, 190f);
            text.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 56;
            text.color = Color.white;
            return text;
        }

        private GameObject CreateRestartButton()
        {
            var buttonObject = new GameObject("Restart Button", typeof(RectTransform));
            buttonObject.transform.SetParent(transform, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -6f);
            rect.sizeDelta = new Vector2(430f, 92f);

            var image = buttonObject.AddComponent<Image>();
            image.sprite = RacingVisuals.WhiteSprite;
            image.color = new Color(0f, 0f, 0f, 0.72f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => Object.FindAnyObjectByType<RaceGameController>()?.ResetRace());

            var label = CreateText("Restart Label", rect);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 42;
            label.color = Color.white;
            label.text = "RESTART";

            return buttonObject;
        }

        private Text CreateText(string name, RectTransform parent)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<Text>();
            text.font = RacingVisuals.DefaultFont;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }

        private static string FormatRacer(RaceProgressTracker racer)
        {
            if (racer == null)
            {
                return "";
            }

            return $"{racer.RacerName}\nLap {Mathf.Min(racer.CompletedLaps + 1, 3)}/3  {FormatOrdinal(racer.CurrentPosition)}";
        }

        private static string FormatOrdinal(int position)
        {
            return position == 1 ? "1st" : "2nd";
        }

        private static void SetText(Text text, string value)
        {
            if (text != null && text.text != value)
            {
                text.text = value;
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }
    }
}
