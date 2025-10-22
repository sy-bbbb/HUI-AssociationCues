using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSizeChecker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var bounds = GetCombinedBounds(this.gameObject);
        Debug.Log(this.gameObject.name + "," + bounds.size);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Calculate combined bounds of all child renderers
    Bounds GetCombinedBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds();

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
}
