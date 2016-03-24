using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class CometPointer : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem cometTail;

    // Use this for initialization
    private void Start()
    {
        if (!cometTail)
        {
            Debug.LogError("Fields were not set");
            return;
        }

        cometTail.gameObject.SetActive(false);
    }

    public void OnInputPointerMoved(Vector3 pointerPos)
    {
        if (!cometTail.gameObject.activeSelf)
        {
            cometTail.gameObject.SetActive(true);
        }

        cometTail.transform.position = new Vector3(pointerPos.x, pointerPos.y, 0);
    }

    public void Hide()
    {
        cometTail.gameObject.SetActive(false);
    }

    public void SetAngle(float angle)
    {
        cometTail.transform.localRotation = Quaternion.Euler(angle, -90, 0);
    }
}
