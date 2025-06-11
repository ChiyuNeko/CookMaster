using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using System.Threading.Tasks;
using UnityEngine.VFX;
using Unity.VisualScripting;
using System.Linq;
using System.Collections;

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
    public VisualEffect[] slash;
    public bool cut = false;
    public GameObject cam;
    int count = 0;

    public void Update()
    {
        Vector3 cutLineDir = (A.position - B.position).normalized;
        List<GameObject> Food = GameObject.FindGameObjectsWithTag("Food").ToList();
        Debug.Log(Camera.main);
        Vector3 cutPlaneNormal = Vector3.Cross(cutLineDir, cam.transform.forward).normalized;
        slicableObjects = Food;
        if (cut)
        {

            //vfxPos.transform.position = slicableObjects[0].transform.position;
            //NewFood();
            StartCoroutine(NewFood());
            //slash.Play();
            StartCoroutine(GenerateSlash());
            cut = false;
            count = 0;
        }
        if (count >= 20)
        {
            //cutPlaneNormal = Vector3.Cross(cutLineDir, Camera.main.transform.forward).normalized;
            AverageSliceAllObjects(new Vector3(-0.32f, 0.79f, 0.52f), sliceCount);
            count = 0;
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

        BoxCollider col = obj.AddComponent<BoxCollider>();
        //col.convex = true;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.drag = 10f;

        obj.name = "SlicedPart";

        Destroy(obj, 3);
    }

    public void AddSlicableObject(GameObject obj)
    {
        if (!slicableObjects.Contains(obj))
            slicableObjects.Add(obj);
    }
    public void FoodFloat()
    {
        foreach (GameObject i in slicableObjects)
        {
            Rigidbody rb = i.GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * 10000);
            rb.drag = 25;
        }
    }
    public IEnumerator NewFood()
    {
        yield return new WaitForSeconds(5);

        for (int i = 0; i < 5; i++)
        {
            Instantiate(FoodObjects[0], SpawnPos.position + Vector3.forward * i * 2, Quaternion.identity);
        }

    }
    IEnumerator GenerateSlash()
    {
        for (int i = 0; i <= 20; i++)
        {
            float x = Random.Range(0, 180);
            float y = Random.Range(0, 180);
            float z = Random.Range(0, 180);
            int index = Random.Range(0, 2);
            VisualEffect vfx = Instantiate(slash[index], SpawnPos.position, Quaternion.Euler(x, y, z));
            vfx.Play();
            count++;
            yield return new WaitForSeconds(0.05f);
            Destroy(vfx.gameObject, 2);
            
        }
    }
    
}
