using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlaceController : MonoBehaviour
{
    [SerializeField] private ARController _arController;
    [SerializeField] private Button[] _prefabButtons;

    [SerializeField] private GameObject _actionPanel;
    [SerializeField] private GameObject _materialPanel;
    [SerializeField] private Button _deleteButton;
    [SerializeField] private Button _rotateLeftButton;
    [SerializeField] private Button _rotateRightButton;
    [SerializeField] private Button _clearAllButton;
    [SerializeField] private Button[] _materialBottons;

    void Start()
    {
        SetUpPrefabButtons();
        SetUpActionButtons();
        SetUpMaterialButtons();
        
        _actionPanel.SetActive(true);
        
        SusbcribeToEvents();
    }

    private void SusbcribeToEvents()
    {
        _arController.OnObjectSelected += OnObjectSelected;
        _arController.OnSelectionCleared += OnSelectionCleared;
        _arController.OnPlacedObjectsChanged += RefreshStatus;
    }

    //Preset the Prefab buttons
    private void SetUpPrefabButtons()
    {
        for (int i = 0; i < _prefabButtons.Length; i++)
        {
            int index = i;
            _prefabButtons[i].onClick.AddListener(() => { _arController.SetSelectedPrefab(index); });
        }
    }

    //Preset the actions buttons
    private void SetUpActionButtons()
    {
        _deleteButton.onClick.AddListener(() => { _arController.DeleteSelectedPrefab(); });
        _rotateLeftButton.onClick.AddListener(() => _arController.RotateSelected(-1f));
        _rotateRightButton.onClick.AddListener(() => _arController.RotateSelected(1f));
        _clearAllButton.onClick.AddListener(() => { _arController.clearAll(); });
    }

    //Preset the materials
    private void SetUpMaterialButtons()
    {
        for (int i = 0; i < _materialBottons.Length; i++)
        {
            int index = i;
            _materialBottons[i].onClick.AddListener(() => { _arController.SetSelectedMaterial(index); });
        }
    }

    private void OnObjectSelected(PlacedObject selected)
    {
        _actionPanel.SetActive(true);
        _materialPanel.SetActive(true);
        RefreshStatus();
    }

    private void OnSelectionCleared()
    {
        _actionPanel.SetActive(true);
        _materialPanel.SetActive(true);
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        //This will to update UI text or simillar elements
    }

    private void OnDestroy()
    {
        _arController.OnObjectSelected -= OnObjectSelected;
        _arController.OnSelectionCleared -= OnSelectionCleared;
        _arController.OnPlacedObjectsChanged -= RefreshStatus;
    }
}