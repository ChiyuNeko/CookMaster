using UnityEngine;
using EzySlice;

public class MouseSlice : MonoBehaviour
{
    public Material crossSectionMaterial;
    public LayerMask sliceMask;

    private Vector3 mouseStartWorld;
    private Vector3 mouseEndWorld;
    private bool isDragging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, sliceMask))
            {
                mouseStartWorld = hit.point;
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, sliceMask))
            {
                mouseEndWorld = hit.point;
                SliceObject(mouseStartWorld, mouseEndWorld);
            }

            isDragging = false;
        }
    }

    void SliceObject(Vector3 start, Vector3 end)
    {
        // 計算切割平面
        Vector3 sliceDirection = end - start;
        Vector3 sliceNormal = Vector3.Cross(sliceDirection, Camera.main.transform.forward).normalized;
        Vector3 slicePosition = (start + end) / 2f;

        Collider[] meats = Physics.OverlapSphere(slicePosition, 1.0f, sliceMask);
        foreach (Collider col in meats)
        {
            GameObject obj = col.gameObject;
            SlicedHull hull = obj.Slice(slicePosition, sliceNormal, crossSectionMaterial);

            if (hull != null)
            {
                GameObject upperHull = hull.CreateUpperHull(obj, crossSectionMaterial);
                GameObject lowerHull = hull.CreateLowerHull(obj, crossSectionMaterial);

                SetupSlicedObject(upperHull);
                SetupSlicedObject(lowerHull);

                Destroy(obj);
            }
        }
    }

    void SetupSlicedObject(GameObject go)
    {
        go.transform.position = go.transform.position;
        go.transform.rotation = go.transform.rotation;
        go.AddComponent<MeshCollider>().convex = true;
        go.AddComponent<Rigidbody>();
    }
}
