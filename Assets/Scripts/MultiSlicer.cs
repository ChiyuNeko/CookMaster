using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using System.Threading.Tasks;
using UnityEngine.VFX;

public class MultiSlicer : MonoBehaviour
{
    public Material cutMaterial;
    public List<GameObject> FoodObjects = new List<GameObject>();
    public Transform SpawnPos;
    public List<GameObject> slicableObjects = new List<GameObject>();
    public int sliceCount = 8; // 切成幾塊

    private Vector3 startPos, endPos;
    public Transform A, B;
    public GameObject vfxPos;
    public VisualEffect slash;
    public bool cut = false;

    async void Update()
    {
        Vector3 cutLineDir = (A.position - B.position).normalized;
        Vector3 cutPlaneNormal = Vector3.Cross(cutLineDir, Camera.main.transform.forward).normalized;
        if (cut)
        {
            Debug.Log("start Cut");
            vfxPos.transform.position = slicableObjects[0].transform.position;
            AverageSliceAllObjects(cutPlaneNormal, sliceCount);
            NewFood();
            slash.Play();
            cut = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            startPos = GetMouseHitPoint();
        }

        if (Input.GetMouseButtonUp(0))
        {
            endPos = GetMouseHitPoint();

            if (startPos == Vector3.zero || endPos == Vector3.zero)
            {
                Debug.Log("切割失敗：起點或終點未擊中可切割物件");
                return;
            }

            // Vector3 cutLineDir = (A.position - B.position).normalized;
            //Vector3 cutPlaneNormal = Vector3.Cross(cutLineDir, Camera.main.transform.forward).normalized;
            Vector3 planeCenter = (startPos + endPos) * 0.5f;

            //AverageSliceAllObjects(cutPlaneNormal, sliceCount);
            Debug.Log("cutPlaneNormal:" + cutPlaneNormal);
        }
    }

    Vector3 GetMouseHitPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
    async Task Cut(Vector3 planeNormal, int parts)
    {
        await Task.Run(() =>
        {
            AverageSliceAllObjects(planeNormal, parts);
        });
    }

    void AverageSliceAllObjects(Vector3 planeNormal, int parts)
    {
        List<GameObject> currentObjects = new List<GameObject>(slicableObjects);
        slicableObjects.Clear();

        foreach (GameObject obj in currentObjects)
        {
            if (obj == null) continue;

            Queue<GameObject> queue = new Queue<GameObject>();
            queue.Enqueue(obj);
            
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend == null)
            {
                //slicableObjects.Add(obj);
                continue;
            }

            Bounds bounds = rend.bounds;
            float lengthAlongNormal = Vector3.Dot(bounds.size, planeNormal);

            List<float> sliceOffsets = new List<float>();
            for (int i = 1; i < parts; i++)
            {
                float offset = -lengthAlongNormal / 2f + (lengthAlongNormal * i) / parts;
                sliceOffsets.Add(offset);
            }

            for (int i = 0; i < sliceOffsets.Count; i++)
            {
                int count = queue.Count;
                for (int j = 0; j < count; j++)
                {
                    GameObject target = queue.Dequeue();
                    Vector3 slicePos = target.transform.position + planeNormal * sliceOffsets[i];

                    SlicedHull sliced = target.Slice(slicePos, planeNormal, cutMaterial);
                    if (sliced != null)
                    {
                        GameObject upper = sliced.CreateUpperHull(target, cutMaterial);
                        GameObject lower = sliced.CreateLowerHull(target, cutMaterial);

                        float spreadAmount = 0.05f * (i + 1);
                        SetupSlicedObject(upper, planeNormal * spreadAmount);
                        SetupSlicedObject(lower, -planeNormal * spreadAmount);

                        queue.Enqueue(upper);
                        queue.Enqueue(lower);

                        Destroy(target);
                    }
                    else
                    {
                        //slicableObjects.Add(target);
                    }
                }
            }
            Debug.Log(planeNormal);
            
            // while (queue.Count > 0)
            // {
            //      slicableObjects.Add(queue.Dequeue());
            // }
        }
    }

    void SetupSlicedObject(GameObject obj, Vector3 offset)
    {
        if (obj == null) return;

        obj.transform.position += offset;

        MeshCollider col = obj.AddComponent<MeshCollider>();
        col.convex = true;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.mass = 1f;

        obj.name = "SlicedPart";

        Destroy(obj, 3);
    }

    public void AddSlicableObject(GameObject obj)
    {
        if (!slicableObjects.Contains(obj))
            slicableObjects.Add(obj);
    }
    public void NewFood()
    {
        GameObject food = Instantiate(FoodObjects[0], SpawnPos.position, Quaternion.identity);
        slicableObjects.Add(food);
    }
    
}
