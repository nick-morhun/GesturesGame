using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class InputManager : MonoBehaviour
{
    public bool AcceptInput;

    public event UnityAction<Vector3> Touch = delegate { };

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
            Touch(pos);
        }
    }
}
