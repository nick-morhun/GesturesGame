#define DEBUG_CTRLS

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class InputManager : MonoBehaviour
{
    Vector3 prevPointerScreenPos;

    bool isPointerDown = false;

    public bool AcceptInput;

    public event UnityAction<Vector3> TouchStarted = delegate { };

    public event UnityAction TouchEnded = delegate { };

    public event UnityAction<Vector3> PointerMoved = delegate { };

    public void Reset()
    {
        isPointerDown = false;
    }

    private void Update()
    {
        if (!AcceptInput)
        {
            return;
        }

        Vector3 pointerWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetButtonDown("Touch"))
        {
            if (isPointerDown)
            {
                return;
            }

            isPointerDown = true;

            Debug.Log("Touch started at screen coordinates " + pointerWorldPosition);
            TouchStarted(pointerWorldPosition);
        }

#if DEBUG_CTRLS
        if (Input.GetButtonDown("Cancel"))
#else
        if (Input.GetButtonUp("Touch"))
#endif
        {
            isPointerDown = false;
            Debug.Log("Touch ended at screen coordinates " + pointerWorldPosition);
            TouchEnded();
        }

#if DEBUG_CTRLS
        if (Input.GetButtonDown("Submit"))
#endif
            if (isPointerDown && (prevPointerScreenPos - Input.mousePosition).magnitude > EventSystem.current.pixelDragThreshold)
            {
                Debug.Log("Touch at screen coordinates " + pointerWorldPosition);
                PointerMoved(pointerWorldPosition);
                prevPointerScreenPos = Input.mousePosition;
            }
    }
}
