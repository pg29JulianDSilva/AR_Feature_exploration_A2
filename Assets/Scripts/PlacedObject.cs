using System;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    [SerializeField] private Color _highlighColor = new(0.2f, 0.2f, 0.2f);
    [SerializeField] private float _highlightStrength = 0.5f;

    private Renderer[] _renderers;
    private Color[] _originalColors;
    private bool _isSelected;

    private void Awake()
    {
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalColors[i] = _renderers[i].material.color;
        }
    }

    //To mark it as selected
    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (!_renderers[i].enabled || _renderers[i] == null) continue;
            _renderers[i].material.color = _isSelected
                ? Color.Lerp(_originalColors[i], _highlighColor, _highlightStrength)
                : _originalColors[i];
        }
    }

    //To set a selected material
    public void SetMaterial(Material material)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].material = material;
            _originalColors[i] = material.color;
        }

        if (_isSelected)
        {
            SetSelected(true);
        }
    }
}