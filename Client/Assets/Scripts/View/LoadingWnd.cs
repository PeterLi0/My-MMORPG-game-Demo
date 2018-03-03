using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadingWnd : BaseWnd
{
    // 已经过去的时间
    private float _ealpsedTime = 0f;

    private const float _totalTime = 0.5f;

    private Slider _slider;
    public void Initialize()
    {
        _slider = _transform.Find("Slider").GetComponent<Slider>();

        _ealpsedTime = 0;
    }

    public override void Update(float dt)
    {
        _slider.value = _ealpsedTime / _totalTime;

        _ealpsedTime += dt;
    }
}