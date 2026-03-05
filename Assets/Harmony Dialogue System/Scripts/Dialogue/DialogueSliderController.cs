using UnityEngine;
using UnityEngine.UI;
using HarmonyDialogueSystem;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueSliderController : MonoBehaviour
{
    [Header("Slider Settings")]
    [Tooltip("The UI Slider to control")]
    [SerializeField] private Slider targetSlider;

    [Tooltip("Minimum slider value")]
    [SerializeField] private float minValue = 0f;

    [Tooltip("Maximum slider value")]
    [SerializeField] private float maxValue = 100f;

    [Tooltip("Starting slider value")]
    [SerializeField] private float startValue = 50f;

    [Header("INK Tag Effects")]
    [Tooltip("Configure how INK tags affect the slider")]
    [SerializeField] private List<TagEffect> tagEffects = new List<TagEffect>();

    [Header("Fallback Settings")]
    [Tooltip("Default change if no tag match is found")]
    [SerializeField] private float defaultChange = 0f;

    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    [SerializeField] private bool debugMode = true;

    // Track current slider value
    private float currentValue;
    private ChoiceSystem choiceSystem;

    // Словарь для быстрого поиска эффектов по тегам
    private Dictionary<string, float> tagEffectLookup;

    [System.Serializable]
    public class TagEffect
    {
        [Tooltip("The INK tag that triggers this effect")]
        public string tagName;

        [Tooltip("How much to change the slider value (positive = increase, negative = decrease)")]
        public float valueChange;
    }

    private void Start()
    {
        InitializeSlider();
        FindChoiceSystem();
        BuildTagLookup();
        SubscribeToChoiceEvents();
    }

    private void InitializeSlider()
    {
        if (targetSlider != null)
        {
            targetSlider.minValue = minValue;
            targetSlider.maxValue = maxValue;
            targetSlider.value = startValue;
            currentValue = startValue;
            if (debugMode) Debug.Log($"Slider initialized with value: {currentValue}");
        }
        else
        {
            Debug.LogError("Target Slider is not assigned!");
        }
    }

    private void FindChoiceSystem()
    {
        choiceSystem = FindObjectOfType<ChoiceSystem>();
        if (choiceSystem == null && debugMode)
        {
            Debug.LogWarning("ChoiceSystem not found! Will try to find it later.");
        }
    }

    private void BuildTagLookup()
    {
        tagEffectLookup = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var effect in tagEffects)
        {
            if (!string.IsNullOrEmpty(effect.tagName))
            {
                tagEffectLookup[effect.tagName] = effect.valueChange;
            }
        }

        if (debugMode) Debug.Log($"Built tag lookup with {tagEffectLookup.Count} entries");
    }

    private void SubscribeToChoiceEvents()
    {
        if (choiceSystem != null)
        {
            choiceSystem.OnChoiceSelected += OnChoiceMade;
            if (debugMode) Debug.Log("Subscribed to ChoiceSystem events");
        }
        else
        {
            // Пробуем найти позже
            Invoke(nameof(SubscribeToChoiceEvents), 0.5f);
        }
    }

    private void OnDestroy()
    {
        if (choiceSystem != null)
        {
            choiceSystem.OnChoiceSelected -= OnChoiceMade;
        }
    }

    // Основной метод, вызываемый при выборе варианта
    private void OnChoiceMade(int choiceIndex, string choiceText)
    {
        if (targetSlider == null) return;

        if (debugMode)
        {
            Debug.Log($"=== CHOICE MADE ===");
            Debug.Log($"Index: {choiceIndex}");
            Debug.Log($"Text: {choiceText}");
        }

        float changeAmount = GetChangeFromTags(choiceText);
        ChangeSliderValue(changeAmount);

        if (debugMode) Debug.Log($"==================");
    }

    // Извлекаем теги из текста варианта и определяем изменение
    private float GetChangeFromTags(string choiceText)
    {
        // Ищем теги в формате #tag или #tag:value
        string pattern = @"#(\w+)(?::([+-]?\d*\.?\d+))?";
        MatchCollection matches = Regex.Matches(choiceText, pattern);

        float totalChange = 0f;
        bool foundTag = false;

        foreach (Match match in matches)
        {
            string tagName = match.Groups[1].Value;
            string valueStr = match.Groups[2].Value;

            if (debugMode) Debug.Log($"Found tag: #{tagName} with value: {valueStr}");

            // Если в теге указано конкретное значение (например #slider:15)
            if (!string.IsNullOrEmpty(valueStr) && float.TryParse(valueStr, out float directValue))
            {
                totalChange += directValue;
                foundTag = true;
                if (debugMode) Debug.Log($"Direct value from tag: {directValue}");
            }
            // Иначе ищем значение в настройках tagEffects
            else if (tagEffectLookup.TryGetValue(tagName, out float configValue))
            {
                totalChange += configValue;
                foundTag = true;
                if (debugMode) Debug.Log($"Value from config for #{tagName}: {configValue}");
            }
        }

        if (!foundTag)
        {
            if (debugMode) Debug.Log($"No matching tags found. Using default: {defaultChange}");
            return defaultChange;
        }

        return totalChange;
    }

    private void ChangeSliderValue(float change)
    {
        currentValue += change;
        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);

        if (targetSlider != null)
        {
            targetSlider.value = currentValue;
        }

        if (debugMode) Debug.Log($"Slider changed by {change}. New value: {currentValue}");
    }

    // Публичные методы для управления слайдером

    public void SetSliderValue(float newValue)
    {
        currentValue = Mathf.Clamp(newValue, minValue, maxValue);
        if (targetSlider != null)
        {
            targetSlider.value = currentValue;
        }
        if (debugMode) Debug.Log($"Slider manually set to: {currentValue}");
    }

    public void AddToSlider(float amount)
    {
        ChangeSliderValue(amount);
    }

    public float GetCurrentSliderValue()
    {
        return currentValue;
    }

    public void ResetSlider()
    {
        currentValue = startValue;
        if (targetSlider != null)
        {
            targetSlider.value = currentValue;
        }
        if (debugMode) Debug.Log($"Slider reset to start value: {startValue}");
    }

    // Метод для добавления тегов через код
    public void AddTagEffect(string tagName, float valueChange)
    {
        // Проверяем, не существует ли уже такой тег
        foreach (var effect in tagEffects)
        {
            if (effect.tagName.Equals(tagName, System.StringComparison.OrdinalIgnoreCase))
            {
                effect.valueChange = valueChange;
                if (debugMode) Debug.Log($"Updated tag effect for '{tagName}' to {valueChange}");
                BuildTagLookup();
                return;
            }
        }

        // Создаём новый эффект
        TagEffect newEffect = new TagEffect
        {
            tagName = tagName,
            valueChange = valueChange
        };
        tagEffects.Add(newEffect);
        BuildTagLookup();

        if (debugMode) Debug.Log($"Added new tag effect for '{tagName}': {valueChange}");
    }
}