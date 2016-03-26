using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
class EditorGraphics : MonoBehaviour
{
    [SerializeField]
    private Button newButton;

    [SerializeField]
    private Button saveButton;

    [SerializeField]
    private Button gameButton;

    [SerializeField]
    private Text messageText;

    [SerializeField]
    private EditorFigure figure;

    private void Start()
    {
        if (!newButton || !saveButton || !gameButton || !figure)
        {
            Debug.LogError("EditorGraphics: fields are not set");
            return;
        }

        saveButton.interactable = false;
        newButton.interactable = false;

        newButton.onClick.AddListener(OnNewClick);
        saveButton.onClick.AddListener(OnSaveClick);
        gameButton.onClick.AddListener(OnGameClick);

        figure.FigureStarted += () =>
        {
            saveButton.interactable = true;
            newButton.interactable = true;
        };
        figure.ValidationFailed += OnValidationFailed;
    }

    private void OnValidationFailed()
    {
        saveButton.interactable = false;
        newButton.interactable = true;
        ShowMessage("Ошибка: Фигура некорректна");
    }

    private void ShowMessage(string message)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;
    }

    private void OnGameClick()
    {
        newButton.interactable = false;
        saveButton.interactable = false;
        gameButton.interactable = false;
    }

    private void OnNewClick()
    {
        messageText.gameObject.SetActive(false);
        saveButton.interactable = false;
        newButton.interactable = false;
    }

    private void OnSaveClick()
    {
        saveButton.interactable = false;
    }

    internal void OnSaveSuccessful()
    {
        ShowMessage("Фигуры сохранены");
        newButton.interactable = true;
    }

    internal void OnSaveSaveFailed()
    {
        ShowMessage("Ошибка: Сохранение не удалось");
        newButton.interactable = true;
    }
}

