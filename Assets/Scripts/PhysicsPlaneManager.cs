using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PhysicsPlaneManager : MonoBehaviour
{
    [SerializeField] private ARPlaneManager _planeManager;

    private void Update()
    {
        foreach (ARPlane plane in _planeManager.trackables)
        {
            EnsureCollider(plane);
        }
    }

    private void EnsureCollider(ARPlane plane)
    {
        MeshCollider meshCollider = plane.GetComponent<MeshCollider>();
        if(!meshCollider) meshCollider = plane.gameObject.AddComponent<MeshCollider>();
        
        MeshFilter meshFilter = plane.GetComponent<MeshFilter>();
        if (meshFilter && !meshFilter.sharedMesh && meshCollider.sharedMesh != meshFilter.sharedMesh)
            meshCollider.sharedMesh = meshFilter.sharedMesh;
    }
}