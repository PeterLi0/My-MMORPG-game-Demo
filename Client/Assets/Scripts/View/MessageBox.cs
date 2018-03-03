using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : BaseWnd
{
    private Text _text;

    private Button _button;

    public void Initialize(string content)
    {
        _text = _transform.Find("Text").GetComponent<Text>();
        _text.text = content;

        _button = _transform.Find("Button").GetComponent<Button>();
        _button.onClick.AddListener(OnOKButtonClick);
    }

    private void OnOKButtonClick()
    {
        WindowManager.instance.Close<MessageBox>();
    }

    public static void Show(string content)
    {
        WindowManager.instance.Open<MessageBox>().Initialize(content);
    }
}