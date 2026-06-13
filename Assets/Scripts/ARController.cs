using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARController : MonoBehaviour
{
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private Camera arCamera;

    [SerializeField] private GameObject[] placeblePrefabs;
    [SerializeField] private GameObject pointerPrefab;

    [SerializeField] private Material[] swappableMaterials;

    [SerializeField] private float rotationStep = 15f;
    [SerializeField] private float scaleStep = 0.1f;
    [SerializeField] private float minScale = 0.05f;
    [SerializeField] private float maxScale = 3f;

    private int _selectedPrefabIndex;
    private PlacedObject _selectedObject;

    private readonly List<ARRaycastHit> _arHits = new();
    private readonly List<PlacedObject> _placedObjects = new();

    public PlacedObject SelectedObject => _selectedObject;
    public int PlacedCount => _placedObjects.Count;
    public Material[] SwappableMaterials => swappableMaterials;

    public event System.Action<PlacedObject> OnObjectSelected;
    public event System.Action OnSelectionCleared;
    public event System.Action OnPlacedObjectsChanged;

    private void Update()
    {
        UpdatePointer();

        if (TryGetPointerPlaced(out Vector2 screenPosition) && !IsPointerOverUI())
        {
            HandleInput(screenPosition);
        }
    }

    //This one is to automaticlly assing the possition
    private void UpdatePointer()
    {
        Vector2 screenPosition = new(Screen.width * 0.5f, Screen.height * 0.5f);
        bool valid = arRaycastManager.Raycast(screenPosition, _arHits, TrackableType.Planes);

        pointerPrefab.SetActive(valid);

        if (valid)
        {
            Pose pose = _arHits[0].pose;
            pointerPrefab.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }

    //This one will be updated with the input inside the project (And subsequent functions)
    private void HandleInput(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            PlacedObject placed = hitInfo.collider.GetComponent<PlacedObject>();
            if (placed)
            {
                SelectObject(placed);
                return;
            }
        }

        if (arRaycastManager.Raycast(screenPosition, _arHits, TrackableType.PlaneWithinPolygon))
        {
            ClearSelection();
            PlaceObject(_arHits[0].pose);
        }
    }

    private void PlaceObject(Pose pose)
    {
        GameObject go = Instantiate(placeblePrefabs[_selectedPrefabIndex], pose.position, pose.rotation);

        PlacedObject placed = go.GetComponent<PlacedObject>();

        if (!placed) placed = go.AddComponent<PlacedObject>();

        _placedObjects.Add(placed);
        OnPlacedObjectsChanged?.Invoke();
    }

    public void ClearSelection()
    {
        if (_selectedObject)
        {
            _selectedObject.SetSelected(false);
            _selectedObject = null;
            OnSelectionCleared?.Invoke();
        }
    }

    private void SelectObject(PlacedObject placed)
    {
        if (!placed) return;
        if (_selectedObject == placed) return;
        ClearSelection();
        _selectedObject = placed;
        _selectedObject.SetSelected(true);
        OnObjectSelected?.Invoke(_selectedObject);
    }

    public void clearAll()
    {
        foreach (PlacedObject placedObject in _placedObjects)
        {
            if (placedObject) Destroy(placedObject.gameObject);
        }
        _placedObjects.Clear();
        _selectedObject = null;
        OnSelectionCleared?.Invoke();
        OnPlacedObjectsChanged?.Invoke();
    }

    private bool TryGetPointerPlaced(out Vector2 position)
    {
        Pointer pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            position = pointer.position.ReadValue();
            return true;
        }

        position = default;
        return false;
    }

    public void RotateSelected(float direction)
    {
        if (!_selectedObject) return;
        _selectedObject.transform.Rotate(Vector3.up, direction * rotationStep);
    }

    public void SetSelectedMaterial(int materialIndex)
    {
        if (_selectedObject && materialIndex >= 0 && materialIndex < swappableMaterials.Length)
        {
            _selectedObject.SetMaterial(swappableMaterials[materialIndex]);
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public void SetSelectedPrefab(int index)
    {
        _selectedPrefabIndex = Mathf.Clamp(index, 0, placeblePrefabs.Length - 1);
    }

    public void DeleteSelectedPrefab()
    {
        if (!_selectedObject) return;

        _placedObjects.Remove(_selectedObject);
        Destroy(_selectedObject.gameObject);
        OnSelectionCleared?.Invoke();
        OnPlacedObjectsChanged?.Invoke();
    }
}