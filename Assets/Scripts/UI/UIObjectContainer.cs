using System.Collections.Generic;
using UnityEngine;

public class UIObjectContainer : MonoBehaviour
{
    [SerializeField] RectTransform rootTransform;
    [SerializeField] GameObject objectPrefab;
    public List<GameObject> objects = new();

    void Start()
    {
        if (rootTransform == null)
        {
            rootTransform = GetComponent<RectTransform>();
        }
    }

    public GameObject AddObject()
    {
        GameObject obj = Instantiate(objectPrefab, rootTransform);
        objects.Add(obj);
        return obj;
    }

    public void RemoveObject(GameObject obj)
    {
        objects.Remove(obj);
        Destroy(obj);
    }
}
