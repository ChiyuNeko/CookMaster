using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDetect : MonoBehaviour
{
    public GameObject Scabbard;
    public MultiSlicer multiSlicer;
    bool CanCut = false;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerStay(Collider other)
    {
        if (CanCut)
        {
            if (other.tag == "Controller")
            {
                multiSlicer.cut = true;
                CanCut = false;
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
            if (other.tag == "Controller")
            {
                CanCut = true;
            }
    }
}
