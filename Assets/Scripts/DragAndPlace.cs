using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DragAndPlace : MonoBehaviour
{
    [SerializeField] private ARRaycastManager _arRaycastManager;
    [SerializeField] private Camera _arCamera;

    [SerializeField] private GameObject _placeableObjectPrefab;

    [SerializeField] private float _objectRadius = 0.5f;
    
    private readonly List<ARRaycastHit> _arRaycastHits = new();
    private Rigidbody dragTarget;
    private Collider dragCollider;
    private bool isDragging;
    private float dragDistance;

    private void Update()
    {
        Pointer pointer = Pointer.current;
        if(pointer == null) return;
        
        Vector2 screenPos = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame && !IsPointerOverUI())
        {
            OnPressDown(screenPos);
        }else if (pointer.press.wasPressedThisFrame && isDragging)
        {
            OnDragging(screenPos);
        }else if (!pointer.press.wasReleasedThisFrame && isDragging)
        {
            OnRelese();
        }
    }

    private void OnPressDown(Vector2 screenPos)
    {
        throw new NotImplementedException();
    }
    
    private void OnDragging(Vector2 screenPos)
    {
        throw new NotImplementedException();
    }
    
    private void OnRelese()
    {
        throw new NotImplementedException();
    }
    
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    
}