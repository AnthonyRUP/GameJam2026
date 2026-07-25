using UnityEngine;
using UnityEngine.UI;

namespace Countdown.UI.Monitor
{
    // Classic ECG-style scrolling trace: flat baseline with a single sharp blip
    // exactly once per heartbeat. Not literal audio-waveform analysis - a stylized
    // pulse synthesized purely from BPM timing, so its rate visibly changes the
    // moment Bpm changes (denser spikes when the heart rate spikes).
    public class ECGWaveform : MonoBehaviour
    {
        [SerializeField] private RawImage display;
        [SerializeField] private int width = 128;
        [SerializeField] private int height = 48;
        [SerializeField] private float columnsPerSecond = 30f;
        [SerializeField] private Color lineColor = new(0.2f, 1f, 0.4f, 1f);
        [SerializeField] private Color backgroundColor = new(0f, 0f, 0f, 1f);

        public float Bpm { get; set; } = 72f;

        private Texture2D _texture;
        private Color32[] _pixels;
        private float[] _columnHeights; // 0..1 normalized
        private float _columnAccumulator;
        private float _beatTimer;

        private void Awake()
        {
            _texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _pixels = new Color32[width * height];
            _columnHeights = new float[width];
            for (int i = 0; i < width; i++)
                _columnHeights[i] = 0.5f;

            if (display != null)
                display.texture = _texture;

            Redraw();
        }

        private void OnEnable()
        {
            _beatTimer = 0f;
            _columnAccumulator = 0f;
        }

        private void Update()
        {
            float beatInterval = 60f / Mathf.Max(Bpm, 1f);
            _beatTimer += Time.deltaTime;
            if (_beatTimer >= beatInterval)
                _beatTimer -= beatInterval;

            _columnAccumulator += Time.deltaTime * columnsPerSecond;
            bool changed = false;
            while (_columnAccumulator >= 1f)
            {
                _columnAccumulator -= 1f;
                AppendColumn(beatInterval);
                changed = true;
            }

            if (changed)
                Redraw();
        }

        private void AppendColumn(float beatInterval)
        {
            float t = beatInterval > 0f ? _beatTimer / beatInterval : 0f;
            float pulse = PulseShape(t);
            float value = Mathf.Clamp01(0.5f + pulse * 0.42f);

            for (int i = 0; i < width - 1; i++)
                _columnHeights[i] = _columnHeights[i + 1];
            _columnHeights[width - 1] = value;
        }

        // A brief smooth blip right after each beat cycle starts, flat otherwise.
        private static float PulseShape(float t)
        {
            const float spikeWidth = 0.12f;
            if (t > spikeWidth)
                return 0f;
            float x = t / spikeWidth;
            return Mathf.Sin(x * Mathf.PI);
        }

        private void Redraw()
        {
            for (int i = 0; i < _pixels.Length; i++)
                _pixels[i] = backgroundColor;

            for (int x = 0; x < width; x++)
            {
                int y0 = Mathf.RoundToInt(_columnHeights[x] * (height - 1));
                int yPrev = x > 0 ? Mathf.RoundToInt(_columnHeights[x - 1] * (height - 1)) : y0;
                int lo = Mathf.Min(y0, yPrev);
                int hi = Mathf.Max(y0, yPrev);
                for (int y = lo; y <= hi; y++)
                    _pixels[y * width + x] = lineColor;
            }

            _texture.SetPixels32(_pixels);
            _texture.Apply();
        }
    }
}
