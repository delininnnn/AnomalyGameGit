#if USE_INK
using Ink.Runtime;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace HarmonyDialogueSystem
{
    public class ChoiceSystem : MonoBehaviour
    {
        public static ChoiceSystem Instance { get; private set; }

        List<GameObject> choiceList;

        private GameObject choicePrefab;
        VerticalLayoutGroup choicesContainer;

        // ДОБАВЬТЕ ЭТО СОБЫТИЕ
        public System.Action<int, string> OnChoiceSelected;

        private void Awake()
        {
            Instance = this;
            choiceList = new List<GameObject>();
        }

        public void SetChoiceSystem(GameObject choicePrefab, VerticalLayoutGroup choiceContainer)
        {
            this.choicePrefab = choicePrefab;
            choicesContainer = choiceContainer;
        }

#if USE_INK
        public void DisplayChoices(Story currentStory)
        {
            List<Choice> currentChoices = currentStory.currentChoices;

            int index = 0;
            DisableChoices();

            foreach (Choice choice in currentChoices)
            {
                Debug.Log(currentChoices.Count);
                if (currentChoices.Count > choiceList.Count) 
                    CreateChoice(index, choice.text, currentStory);
                else 
                    EnableChoice(index, choice.text, currentStory);

                if (currentChoices.Count > 1) 
                    StartCoroutine(SelectFirstChoice());

                index++;
            }
        }
#endif

        public void DisplayChoices(TextAsset TXTasset)
        {
            // Ваша реализация для TXT
        }

#if USE_INK
        private void CreateChoice(int index, string _text, Story currentStory)
        {
            GameObject newButton = Instantiate(choicePrefab, choicesContainer.transform);

            // ИЗМЕНЕНО: Добавляем обработчик с передачей текста
            Button button = newButton.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => {
                // Вызываем событие ПЕРЕД выбором варианта
                OnChoiceSelected?.Invoke(index, _text);
                MakeChoice(index, currentStory);
            });

            TextMeshProUGUI choicesText = newButton.GetComponentInChildren<TextMeshProUGUI>() ?? 
                throw new Exception("TextMeshPro is not a child of your Dialogue Choice Prefab. Cannot find Text");
            choicesText.text = _text;
            choiceList.Add(newButton);
        }

        private void EnableChoice(int index, string _text, Story currentStory)
        {
            GameObject choice = choiceList[index];
            Button choiceButton = choice.GetComponentInChildren<Button>();
            choiceButton.onClick.RemoveAllListeners();
            
            // ИЗМЕНЕНО: Добавляем обработчик с передачей текста
            choiceButton.onClick.AddListener(() => {
                // Вызываем событие ПЕРЕД выбором варианта
                OnChoiceSelected?.Invoke(index, _text);
                MakeChoice(index, currentStory);
            });

            TextMeshProUGUI choicesText = choice.GetComponentInChildren<TextMeshProUGUI>() ?? 
                throw new Exception("TextMeshPro is not a child of your Dialogue Choice Prefab. Cannot find Text");
            choicesText.text = _text;
            choice.SetActive(true);
        }

        void DisableChoices()
        {
            foreach (var choice in choiceList) choice.SetActive(false);
        }

        private IEnumerator SelectFirstChoice()
        {
            EventSystem.current.SetSelectedGameObject(null);
            yield return new WaitForEndOfFrame();
            if (choiceList.Count > 0)
                EventSystem.current.SetSelectedGameObject(choiceList[0]);
        }

        public void MakeChoice(int choiceIndex, Story currentStory)
        {
            if (currentStory.currentChoices.Count > choiceIndex) 
                currentStory.ChooseChoiceIndex(choiceIndex);
            DialogueManager.instance.ContinueStory();
        }
#endif
    }
}
