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
        figure.ValidationFailed += () => { saveButton.interactable = false; };
    }

    private void OnGameClick()
    {
        newButton.interactable = false;
        saveButton.interactable = false;
        gameButton.interactable = false;
    }

    private void OnNewClick()
    {
        saveButton.interactable = false;
        newButton.interactable = false;
    }

    private void OnSaveClick()
    {
        saveButton.interactable = false;
        newButton.interactable = false;
    }
}

