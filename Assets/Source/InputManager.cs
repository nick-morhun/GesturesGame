using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class InputManager : MonoBehaviour
{
    public bool AcceptInput;

    public event UnityAction<Vector3> TouchStarted = delegate { };

    public event UnityAction<Vector3> TouchEnded = delegate { };

    private void Update()
    {
        if (!AcceptInput)
        {
            return;
        }

        if (Input.GetButtonDown("Touch"))
        {
            Debug.Log("Touch at screen coordinates " + Input.mousePosition);
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TouchStarted(pos);
        }

        if (Input.GetButtonUp("Touch"))
        {
            Debug.Log("Touch ended at screen coordinates " + Input.mousePosition);
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TouchEnded(pos);
        }
    }
}
