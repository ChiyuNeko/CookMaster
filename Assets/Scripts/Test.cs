using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

public class Test : MonoBehaviour
{
    public int x = 0;
    async Task Start()
    {
        Debug.Log("Start");
        await m_func();
        Debug.Log(x);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("A");
    }
    async Task<int> m_func()
    {
        await Task.Run(() =>
        {
            for (int i = 0; i < 100000; i++)
            {
                x++;
                Debug.Log(x);
            }
        });
        return x;
        
        
    }
    
}
