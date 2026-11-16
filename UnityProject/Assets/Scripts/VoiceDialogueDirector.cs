using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KavehRising
{
    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 4)] public string text;
        public AudioClip voice;
        [Min(.5f)] public float subtitleSeconds = 3.5f;
    }

    // In-game Persian subtitles and optional recorded voice. No background music is created or played here.
    public sealed class VoiceDialogueDirector : MonoBehaviour
    {
        public DialogueLine[] openingLines;
        Canvas canvas;
        Text speakerLabel, subtitleLabel;
        AudioSource voiceSource;
        Coroutine playback;
        int activeChapter = -1;

        void Awake()
        {
            if (openingLines == null || openingLines.Length == 0)
                openingLines = new[] {
                    new DialogueLine { speaker = "راوی", text = "در روزگار فرمانروایی ضحاک، سایه ستم بر ایران افتاده بود.", subtitleSeconds = 4f },
                    new DialogueLine { speaker = "کاوه", text = "تا جان در بدن دارم، در برابر بیداد خاموش نمی‌مانم!", subtitleSeconds = 4f }
                };
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.spatialBlend = 0f;
            BuildSubtitleCanvas();
        }
        public void PlayChapter(int chapter)
        {
            if (openingLines == null || openingLines.Length == 0 || activeChapter == chapter) return;
            activeChapter = chapter;
            if (playback != null) StopCoroutine(playback);
            playback = StartCoroutine(PlayLines());
        }
        public void StopDialogue()
        {
            if (playback != null) StopCoroutine(playback);
            playback = null;
            if (voiceSource) voiceSource.Stop();
            if (canvas) canvas.enabled = false;
        }
        IEnumerator PlayLines()
        {
            canvas.enabled = true;
            foreach (var line in openingLines)
            {
                if (line == null) continue;
                speakerLabel.text = line.speaker ?? "";
                subtitleLabel.text = line.text ?? "";
                if (line.voice) { voiceSource.clip = line.voice; voiceSource.Play(); }
                float duration = line.voice ? Mathf.Max(line.subtitleSeconds, line.voice.length) : line.subtitleSeconds;
                yield return new WaitForSecondsRealtime(Mathf.Max(.5f, duration));
            }
            canvas.enabled = false;
            playback = null;
        }
        void BuildSubtitleCanvas()
        {
            var root = new GameObject("Persian Dialogue Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 30;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
            var panel = new GameObject("Subtitle Panel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(root.transform, false);
            var rect = panel.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(.12f, .035f); rect.anchorMax = new Vector2(.88f, .22f); rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(.025f, .018f, .012f, .88f);
            speakerLabel = MakeText("Speaker", panel.transform, 28, new Vector2(.04f, .64f), new Vector2(.96f, .96f));
            subtitleLabel = MakeText("Subtitle", panel.transform, 30, new Vector2(.04f, .08f), new Vector2(.96f, .68f));
            speakerLabel.alignment = TextAnchor.MiddleRight; subtitleLabel.alignment = TextAnchor.MiddleRight;
            canvas.enabled = false;
        }
        static Text MakeText(string name, Transform parent, int size, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); text.fontSize = size; text.color = Color.white; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
    }
}
