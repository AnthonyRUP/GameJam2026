using System;
using Countdown.World;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Countdown.UI.Tutorial
{
    // Boot-time walkthrough shown once before the first patient loads: one page per
    // station (its live sprite + the same blurb ScientistHelpPanel shows for H),
    // then a final controls page. Left/Right browses, clamped rather than wrapped
    // (matching the Book's page-flip convention); pressing Right on the last page
    // finishes the tutorial and starts the game. Not routed through
    // GameManager.OpenPanel/ClosePanel since its lifecycle - ending in "start the
    // game" - is one-shot and different from a station panel's open/close cycle.
    public class TutorialPanel : MonoBehaviour
    {
        [Serializable]
        private struct StationPage
        {
            public string gameObjectName; // station's name in the scene - pulls its live sprite + HelpDescription
            public string title;
        }

        [SerializeField] private GameObject root;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private TextMeshProUGUI pageIndicatorLabel;

        [SerializeField]
        private StationPage[] stationPages =
        {
            new() { gameObjectName = "Tank", title = "Patient Tank" },
            new() { gameObjectName = "Monitor", title = "Vitals Monitor" },
            new() { gameObjectName = "Injector", title = "Injector" },
            new() { gameObjectName = "BloodResearchStation", title = "Blood Research Station" },
            new() { gameObjectName = "ShelfLeft", title = "Shape Shelf" },
            new() { gameObjectName = "ShelfRight", title = "Dye Shelf" },
            new() { gameObjectName = "Mixer", title = "Mixer" },
            new() { gameObjectName = "Book", title = "Codex" },
        };

        private const string ControlsTitle = "Controls";
        private const string ControlsDescription = "WASD - Move\nE - Interact\nH - Help";

        private string[] _descriptions;
        private Sprite[] _icons;
        private string[] _titles;
        private int _pageIndex;
        private Action _onFinished;

        public void Show(Action onFinished)
        {
            _onFinished = onFinished;
            BuildPages();
            _pageIndex = 0;
            if (root != null)
                root.SetActive(true);
            Refresh();
        }

        private void BuildPages()
        {
            int count = stationPages.Length + 1;
            _descriptions = new string[count];
            _icons = new Sprite[count];
            _titles = new string[count];

            for (int i = 0; i < stationPages.Length; i++)
            {
                var page = stationPages[i];
                var go = GameObject.Find(page.gameObjectName);
                _titles[i] = page.title;
                _descriptions[i] = go != null ? go.GetComponent<Interactable>()?.HelpDescription : null;
                _icons[i] = go != null ? go.GetComponent<SpriteRenderer>()?.sprite : null;
            }

            _titles[stationPages.Length] = ControlsTitle;
            _descriptions[stationPages.Length] = ControlsDescription;
            _icons[stationPages.Length] = null;
        }

        private void Update()
        {
            if (root == null || !root.activeSelf || Keyboard.current == null)
                return;

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame && _pageIndex > 0)
            {
                _pageIndex--;
                Refresh();
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                if (_pageIndex < _titles.Length - 1)
                {
                    _pageIndex++;
                    Refresh();
                }
                else
                {
                    Finish();
                }
            }
        }

        private void Refresh()
        {
            if (titleLabel != null)
                titleLabel.text = _titles[_pageIndex];
            if (descriptionLabel != null)
                descriptionLabel.text = _descriptions[_pageIndex];
            if (iconImage != null)
            {
                var sprite = _icons[_pageIndex];
                iconImage.sprite = sprite;
                iconImage.enabled = sprite != null;
            }
            if (pageIndicatorLabel != null)
            {
                bool isLastPage = _pageIndex == _titles.Length - 1;
                string hint = isLastPage ? "-> Begin" : "<- -> Browse";
                pageIndicatorLabel.text = $"{_pageIndex + 1} / {_titles.Length}     {hint}";
            }
        }

        private void Finish()
        {
            if (root != null)
                root.SetActive(false);
            _onFinished?.Invoke();
            _onFinished = null;
        }
    }
}
