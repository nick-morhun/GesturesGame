//#define DEBUG_CTRLS

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class InputManager : MonoBehaviour
{
    Vector3 prevPointerScreenPos = -Vector3.one;

    bool isPointerDown;

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

            //Debug.Log("Touch started at world coordinates " + pointerWorldPosition);
            TouchStarted(pointerWorldPosition);
            return;
        }

#if DEBUG_CTRLS
        if (Input.GetButtonDown("Cancel"))
#else
        if (Input.GetButtonUp("Touch"))
#endif
        {
            isPointerDown = false;
            //Debug.Log("Touch ended at world coordinates " + pointerWorldPosition);
            TouchEnded();
            return;
        }

#if DEBUG_CTRLS
        if (Input.GetButtonDown("Submit"))
#endif
        if (isPointerDown && (prevPointerScreenPos - Input.mousePosition).magnitude > EventSystem.current.pixelDragThreshold)
        {
            if (prevPointerScreenPos != -Vector3.one)
            {
                PointerMoved(pointerWorldPosition);
                //Debug.Log("Touch at world coordinates " + pointerWorldPosition + " accept " + AcceptInput + " isPointerDown = " + isPointerDown);
            }

            prevPointerScreenPos = Input.mousePosition;
        }
    }
}
