using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.UI
{
    public delegate void OnOptionSelectedDelegate(FilterViewOption option);

    public class FilterViewController : MonoBehaviour
    {
        public ImageButton CategoryButtonPrefab;
        public GameObject CategoryContainer;
        public TMP_InputField InputField;
        public ImageButton OptionButtonPrefab;
        public GameObject OptionsContainer;
        public TMP_Text TitleText;
        private List<string> _activeCategories = new List<string>();

        private IEnumerable<FilterViewOption> _allOptions;

        private OnOptionSelectedDelegate _onOptionSelected;

        public void CategoryClicked(ImageButton clickedButton, string category)
        {
            if (_activeCategories.Contains(category))
            {
                _activeCategories.Remove(category);
                clickedButton.SetBaseColor(ColorConstants.WhiteAccent);
            }
            else
            {
                _activeCategories.Add(category);
                clickedButton.SetBaseColor(ColorConstants.BlueAccent);
            }

            PopulateOptions(GetFilteredOptions());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Load(string title, IEnumerable<FilterViewOption> filterViewOptions, OnOptionSelectedDelegate onOptionSelected)
        {
            TitleText.text = title;
            _allOptions = filterViewOptions;
            _onOptionSelected = onOptionSelected;

            PopulateCategories(_allOptions);
            PopulateOptions(_allOptions);

            Show();
        }

        public void OptionClicked(FilterViewOption option)
        {
            Debug.Log($"Clicked {option.Name}");
            _onOptionSelected.Invoke(option);
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void TextChanged()
        {
            PopulateOptions(GetFilteredOptions());
        }

        private static IEnumerable<string> GetCategories(IEnumerable<FilterViewOption> filterViewOptions)
        {
            return filterViewOptions.SelectMany(f => f.Categories).Distinct();
        }

        private void CleanChildObjects(GameObject gameobject)
        {
            foreach (Transform child in gameobject.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private IEnumerable<FilterViewOption> FilterByCategory(IEnumerable<FilterViewOption> options)
        {
            var requiredMatchCount = _activeCategories.Count;
            return options.Where(a => a.Categories.Intersect(_activeCategories).Count() == requiredMatchCount);
        }

        private IEnumerable<FilterViewOption> FilterByText(IEnumerable<FilterViewOption> options, string inputText)
        {
            return options.Where(a => a.Name.IndexOf(inputText, StringComparison.OrdinalIgnoreCase) >= 0
                                      || a.Categories.Any(c => c.IndexOf(inputText, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private IEnumerable<FilterViewOption> GetFilteredOptions()
        {
            var inputText = InputField.text;
            var filteredOptions = _allOptions;
            if (!string.IsNullOrEmpty(inputText))
            {
                filteredOptions = FilterByText(_allOptions, inputText);
            }
            return FilterByCategory(filteredOptions);
        }

        private void PopulateCategories(IEnumerable<FilterViewOption> filterViewOptions)
        {
            CleanChildObjects(CategoryContainer);

            foreach (var category in GetCategories(filterViewOptions))
            {
                var categoryButton = Instantiate(CategoryButtonPrefab, CategoryContainer.transform);
                categoryButton.SetText(category);
                categoryButton.SetOnClick(() => CategoryClicked(categoryButton, category));
            }
        }

        private void PopulateOptions(IEnumerable<FilterViewOption> filterViewOptions)
        {
            CleanChildObjects(OptionsContainer);

            foreach (var option in filterViewOptions)
            {
                var optionButton = Instantiate(OptionButtonPrefab, OptionsContainer.transform);
                optionButton.SetText(option.Name);
                optionButton.SetImage(option.Sprite);
                optionButton.SetOnClick(() => OptionClicked(option));
            }
        }
    }
}