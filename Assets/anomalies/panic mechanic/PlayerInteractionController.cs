using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInteractionController : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject interactionPanel; // Панель, которая появляется (слайдер + текст)
    public Slider progressSlider;       // Слайдер (шкала)
    public Text progressText;           // Текст для отображения процентов (опционально)

    [Header("Progress Settings")]
    public float fillSpeed = 10f;       // Скорость заполнения (процентов в секунду)
    public int clickReduction = 10;     // На сколько уменьшается шкала при клике (N)
    public int maxProgress = 100;       // Максимум шкалы (100)

    [Header("One-Time Fill Settings")]
    public bool oneTimeOnly = true;     // Если true - шкала заполняется только 1 раз за нахождение в зоне

    [Header("Target System (Другая шкала)")]
    public Slider targetSlider;          // Ссылка на ДРУГУЮ шкалу (например, HP или Очки)
    public int damagePerFull = 10;       // Сколько отнимать от другой шкалы (T)

    [Header("Mouse")]
    public KeyCode actionKey = KeyCode.Mouse0; // Левая кнопка мыши (ЛКМ)

    // Приватные переменные
    private float currentProgress = 0f;
    private bool isInZone = false;
    private bool hasFilledOnce = false;      // Флаг: была ли шкала уже полностью заполнена в текущей зоне
    private Coroutine fillCoroutine;
    private GameObject currentZoneObject;     // Запоминаем объект, в зоне которого находимся

    void Start()
    {
        // Убедимся, что панель изначально выключена
        if (interactionPanel != null)
            interactionPanel.SetActive(false);

        // Настройка слайдера
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = maxProgress;
            progressSlider.value = 0;
        }

        UpdateProgressText();
    }

    void Update()
    {
        // Обработка нажатия мыши, только если игрок внутри зоны И шкала ещё не была заполнена (если oneTimeOnly активен)
        if (isInZone && Input.GetKeyDown(actionKey))
        {
            // Если режим oneTimeOnly включен и шкала уже заполнялась - игнорируем клики
            if (oneTimeOnly && hasFilledOnce)
                return;

            ReduceProgress();
        }
    }

    // Вызывается, когда игрок ВОШЕЛ в триггер
    public void EnterZone(GameObject zoneObject)
    {
        // Если уже в другой зоне (на всякий случай), выходим из старой
        if (isInZone)
            ExitZone();

        currentZoneObject = zoneObject;
        isInZone = true;

        // Сбрасываем флаги для новой зоны
        hasFilledOnce = false;
        currentProgress = 0f;

        // Обновляем UI
        if (progressSlider != null) progressSlider.value = 0;
        UpdateProgressText();

        // Показываем панель
        if (interactionPanel != null)
            interactionPanel.SetActive(true);

        // Запускаем корутину заполнения
        if (fillCoroutine != null) StopCoroutine(fillCoroutine);
        fillCoroutine = StartCoroutine(FillProgressOverTime());
    }

    // Вызывается, когда игрок ВЫШЕЛ из триггера ИЛИ объект уничтожен
    public void ExitZone()
    {
        if (!isInZone) return;

        isInZone = false;
        currentZoneObject = null;

        // Останавливаем заполнение
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }

        // Скрываем шкалу
        if (interactionPanel != null)
            interactionPanel.SetActive(false);

        // Сбрасываем прогресс
        currentProgress = 0f;

        // Флаг сбрасывать не обязательно, но для чистоты
        hasFilledOnce = false;
    }

    // Корутина плавного увеличения шкалы
    private IEnumerator FillProgressOverTime()
    {
        while (isInZone)
        {
            // Если режим oneTimeOnly включен и шкала уже заполнялась - останавливаем заполнение
            if (oneTimeOnly && hasFilledOnce)
            {
                yield return null;
                continue;
            }

            if (currentProgress < maxProgress)
            {
                currentProgress += fillSpeed * Time.deltaTime;
                if (currentProgress > maxProgress) currentProgress = maxProgress;

                // Обновляем UI
                if (progressSlider != null) progressSlider.value = currentProgress;
                UpdateProgressText();

                // Проверяем, достигнуто ли 100
                if (Mathf.Approximately(currentProgress, maxProgress) || currentProgress >= maxProgress)
                {
                    OnProgressFull();
                }
            }
            yield return null;
        }
    }

    // Метод, вызываемый при нажатии ЛКМ
    private void ReduceProgress()
    {
        if (!isInZone) return;

        // Если уже заполнились - не уменьшаем
        if (oneTimeOnly && hasFilledOnce) return;

        // Уменьшаем значение (не ниже 0)
        currentProgress -= clickReduction;
        if (currentProgress < 0) currentProgress = 0;

        // Обновляем UI
        if (progressSlider != null) progressSlider.value = currentProgress;
        UpdateProgressText();

        Debug.Log($"Click! Progress reduced to {currentProgress}");
    }

    // Когда шкала заполнилась на 100
    private void OnProgressFull()
    {
        if (!isInZone) return;

        // Если уже заполнялись и oneTimeOnly включен - игнорируем повторное срабатывание
        if (oneTimeOnly && hasFilledOnce) return;

        Debug.Log("Scale filled to 100%! Dealing damage to target.");

        // Отнимаем очки от ДРУГОЙ шкалы
        if (targetSlider != null)
        {
            targetSlider.value -= damagePerFull;

            if (targetSlider.value < targetSlider.minValue)
                targetSlider.value = targetSlider.minValue;
        }

        // Помечаем, что заполнение произошло
        hasFilledOnce = true;

        if (oneTimeOnly)
        {
            // Режим "только один раз": скрываем шкалу и останавливаем всё
            if (interactionPanel != null)
                interactionPanel.SetActive(false);

            if (fillCoroutine != null)
            {
                StopCoroutine(fillCoroutine);
                fillCoroutine = null;
            }

            // Опционально: можно показать сообщение, что зона больше не активна
            Debug.Log("Zone completed! No more filling in this zone.");
        }
        else
        {
            // Старый режим: сбрасываем шкалу и заполняем заново
            currentProgress = 0f;
            if (progressSlider != null) progressSlider.value = 0;
            UpdateProgressText();
        }
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = Mathf.FloorToInt(currentProgress).ToString() + " / " + maxProgress;
        }
    }

    // Этот метод можно вызвать извне, если объект уничтожается (OnDestroy)
    public void ForceExitFromObject(GameObject destroyedObject)
    {
        if (currentZoneObject == destroyedObject)
        {
            ExitZone();
        }
    }

    // Публичный метод для проверки, активна ли зона (опционально)
    public bool IsZoneActive()
    {
        return isInZone && (!oneTimeOnly || !hasFilledOnce);
    }
}