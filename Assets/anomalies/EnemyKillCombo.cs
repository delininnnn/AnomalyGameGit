using UnityEngine;
using System.Collections.Generic;

public class EnemyKillCombo : MonoBehaviour
{
    [Header("Настройки комбинации")]
    [Tooltip("Первая клавиша комбинации")]
    public KeyCode firstKey = KeyCode.H;

    [Tooltip("Вторая клавиша комбинации")]
    public KeyCode secondKey = KeyCode.J;

    [Header("Настройки триггера")]
    [Tooltip("Радиус зоны поражения (если нет коллайдера)")]
    public float triggerRadius = 5f;

    [Tooltip("Визуализация радиуса в редакторе")]
    public bool showGizmo = true;

    [Header("Настройки интерфейса")]
    [Tooltip("Текст подсказки (можно назначить UI Text)")]
    public GameObject hintPanel; // Панель с подсказкой

    // Приватные переменные
    private bool playerInZone = false;
    private HashSet<KeyCode> pressedKeys = new HashSet<KeyCode>();
    private GameObject player;

    void Start()
    {
        // Проверяем наличие коллайдера-триггера
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Если коллайдера нет, добавляем сферический коллайдер как триггер
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = triggerRadius;
            sphereCol.isTrigger = true;
            Debug.Log("Автоматически добавлен SphereCollider как триггер");
        }
        

        // Скрываем подсказку при старте
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    void Update()
    {
        // Работаем только если игрок в зоне
        if (!playerInZone) return;

        // Проверяем нажатие первой клавиши
        if (Input.GetKeyDown(firstKey))
        {
            pressedKeys.Add(firstKey);
        }

        // Проверяем нажатие второй клавиши
        if (Input.GetKeyDown(secondKey))
        {
            pressedKeys.Add(secondKey);
        }

        // Проверяем, нажаты ли обе клавиши
        if (pressedKeys.Contains(firstKey) && pressedKeys.Contains(secondKey))
        {
            KillEnemy();
        }

        // Сбрасываем при отпускании клавиш (опционально)
        if (Input.GetKeyUp(firstKey) || Input.GetKeyUp(secondKey))
        {
            pressedKeys.Clear();
        }
    }

    void KillEnemy()
    {
        Debug.Log($"Враг {gameObject.name} уничтожен комбинацией {firstKey} + {secondKey}");

        // Можно добавить эффекты перед уничтожением
        // Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Уничтожаем объект врага
        Destroy(gameObject);

        // Скрываем подсказку если она есть
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в триггер вошел игрок
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            player = other.gameObject;

            // Показываем подсказку
            if (hintPanel != null)
            {
                hintPanel.SetActive(true);
                // Обновляем текст подсказки с текущими клавишами
                UnityEngine.UI.Text hintText = hintPanel.GetComponentInChildren<UnityEngine.UI.Text>();
                if (hintText != null)
                {
                    hintText.text = $"Нажмите {firstKey} и {secondKey} для уничтожения врага";
                }
            }

            Debug.Log("Игрок вошел в зону поражения врага");
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Проверяем, что зону покинул игрок
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            player = null;
            pressedKeys.Clear();

            // Скрываем подсказку
            if (hintPanel != null)
                hintPanel.SetActive(false);

            Debug.Log("Игрок покинул зону поражения врага");
        }
    }

    // Визуализация радиуса в редакторе
    void OnDrawGizmosSelected()
    {
        if (showGizmo)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}
