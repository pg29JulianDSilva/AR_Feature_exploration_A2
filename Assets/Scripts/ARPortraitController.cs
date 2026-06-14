using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPortraitController : MonoBehaviour
{
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private Camera arCamera;

    [SerializeField] private GameObject portraitPrefab;
    [SerializeField] private GameObject radiousTelegraph;
    
    [SerializeField] private float rotationStep = 15f;
    [SerializeField] private float scaleStep = 0.1f;
    
    [SerializeField] private Color AprovedColor = new Color(0,1,0,1);
    [SerializeField] private Color DisapprovedColor = new Color(1,0,0,1);
    
    private PlacedObject selectedObject;
    
    private float Angle = 0;
    
    private readonly List<ARRaycastHit> arHits = new();
    private readonly List<PlacedObject> placedObjects = new();
    
    [field: SerializeField] public UnityEvent<PlacedObject> OnObjectSelected;
    [field: SerializeField] public UnityEvent OnSelectionCleared;
    [field: SerializeField] public UnityEvent OnPlacedObjectsChanged;

    private Vector3 currentPointerPosition;
    private Quaternion currentPointerRotation;
    
    private bool doesPortraitFits = false;
    
    private void Update()
    {
        UpdatePointer();

        if (TryGetPointerPlaced(out Vector2 screenPosition) && !IsPointerOverUI())
        {
            HandleInput(screenPosition);
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void HandleInput(Vector2 screenPosition)
    {
        if(doesPortraitFits) return;
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

        if (arRaycastManager.Raycast(screenPosition, arHits, TrackableType.PlaneWithinPolygon))
        {
            ClearSelection();
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        GameObject go = Instantiate(portraitPrefab, currentPointerPosition, currentPointerRotation);
        
        PlacedObject placed = go.GetComponent<PlacedObject>();

        if (!placed) placed = go.AddComponent<PlacedObject>();

        placedObjects.Add(placed);
        OnPlacedObjectsChanged?.Invoke();
    }

    private void ClearSelection()
    {
        if (selectedObject)
        {
            selectedObject.SetSelected(false);
            selectedObject = null;
            OnSelectionCleared?.Invoke();
        }
    }
    
    public void RotatePreview(float direction)
    {
        if (!portraitPrefab || !radiousTelegraph) return;
        Angle += direction;
    }


    public void RotateSelected(float direction)
    {
        if (!selectedObject) return;
        selectedObject.transform.Rotate(Vector3.up, direction * rotationStep);
    }

    private void SelectObject(PlacedObject placed)
    {
        if (!placed) return;
        if (selectedObject == placed) return;
        ClearSelection();
        selectedObject = placed;
        selectedObject.SetSelected(true);
        OnObjectSelected?.Invoke(selectedObject);
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

    private void UpdatePointer()
    {
        Vector2 screenPosition = new(Screen.width * 0.5f, Screen.height * 0.5f);
        bool valid = arRaycastManager.Raycast(screenPosition, arHits, TrackableType.Planes);

        portraitPrefab.SetActive(valid);
        radiousTelegraph.SetActive(valid);

        if (valid)
        {
            Pose pose = arHits[0].pose;
            
            portraitPrefab.transform.SetPositionAndRotation(pose.position, pose.rotation);
            radiousTelegraph.transform.SetPositionAndRotation(pose.position, pose.rotation);
            portraitPrefab.transform.Rotate(Vector3.up, Angle * rotationStep);
            radiousTelegraph.transform.Rotate(Vector3.up, Angle * rotationStep);
            currentPointerPosition = portraitPrefab.transform.position;
            currentPointerRotation = portraitPrefab.transform.rotation;

            var plane = arPlaneManager.GetPlane(arHits[0].trackableId);

            foreach (var boundary in plane.boundary)
            {
                Vector3 coordinatesToLocal = new Vector3(boundary.x, 0f, boundary.y);
                
                Vector3 localToWorld = plane.transform.TransformPoint(coordinatesToLocal);
                
                float currentDistance = Vector3.Distance(localToWorld, pose.position);
                doesPortraitFits = currentDistance < radiousTelegraph.transform.localScale.x / 2;
                if(doesPortraitFits)
                {
                    radiousTelegraph.GetComponent<MeshRenderer>().material.color = DisapprovedColor;
                }
                else
                {
                    radiousTelegraph.GetComponent<MeshRenderer>().material.color = AprovedColor;
                }
            }
        }
    }
    
    public void DeleteSelectedPrefab()
    {
        if (!selectedObject) return;

        placedObjects.Remove(selectedObject);
        Destroy(selectedObject.gameObject);
        OnSelectionCleared?.Invoke();
        OnPlacedObjectsChanged?.Invoke();
    }
}