using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour
{
    public float CountDownTime;

    public float LeftTime;
    private Slider _slider;
    
    void Start()
    {
        _slider = GetComponent<Slider>();
        LeftTime = CountDownTime;
        _slider.maxValue = CountDownTime;
    }

    // Update is called once per frame
    void Update()
    {
        
        LeftTime -= Time.deltaTime;
        if (LeftTime < 0)
        {
            LeftTime = 0;
        }
        
        _slider.value = LeftTime;
    }
}
