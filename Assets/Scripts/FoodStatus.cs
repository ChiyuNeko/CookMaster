using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class FoodStatus : MonoBehaviour
{
    public int Life = 0;
    public float Doneness = 0; 
    public bool Cutted = false;
    public bool isFloating = false;
    public bool NeedCook = false;
    public TextMeshPro text;
    void Start()
    {

    }


    void Update()
    {

        if (!NeedCook)
        {
            text.text = Life.ToString();
            if (Life <= 0)
            {
                Life = 0;
                Cutted = true;
                text.enabled = false;
            }
        }
        else
        {
            text.text = Life.ToString() + "\nDoneness:" + Doneness + "%";
            if (Life <= 0 && Doneness >= 100)
            {
                Life = 0;
                Doneness = 100;
                Cutted = true;
                text.enabled = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Sword" && isFloating)
        {
            Life--;
            Life = math.clamp(Life, 0, 100);
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Fire" && isFloating)
        {
            Doneness += 50 * Time.deltaTime;
            Doneness = math.clamp(Doneness, 0, 100);

        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground" && isFloating)
        {
            Destroy(gameObject);
        }
        
    }
}
