using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    private IInteractable _previousInteractable = null;

    private void Update()
    {
        var mousePos = Input.mousePosition;
        RaycastHit hit;
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            var objectHit = hit.transform.gameObject;
            var interactable = objectHit.GetComponent<IInteractable>();
            if (interactable != _previousInteractable)
            {
                if (_previousInteractable != null)
                {
                    _previousInteractable.HoverOff();
                }
                if (interactable != null)
                {
                    interactable.HoverOn();
                }
                _previousInteractable = interactable;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                var objectHit = hit.transform.gameObject;
                var interactable = objectHit.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Select();
                }
            }
        }
            
    }
}
