using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using common;

public class MailWnd : BaseWnd
{

    public class ButtonEventListener : MonoBehaviour, IPointerClickHandler
    {
        public MailDTO dto;
        public void OnPointerClick(PointerEventData eventData)
        {
            WindowManager.instance.Open<MailContentWnd>().Initialize(dto);
        }
    }
    Transform _content;
    public void Initialize(List<MailDTO> mails)
    {
        _content = _transform.Find("Scroll View/Viewport/Content");

        Button mailItemModle = _transform.Find("Scroll View/Button").GetComponent<Button>();

        foreach (MailDTO mail in mails)
        {
            Transform item = (GameObject.Instantiate(mailItemModle.gameObject) as GameObject).transform;
            item.SetParent(_content.transform);
            item.gameObject.SetActive(true);
            item.localPosition = Vector3.zero;
            item.localScale = Vector3.one;
            item.Find("Subject").GetComponent<Text>().text = mail.subject;
            item.Find("ID").GetComponent<Text>().text = mail.id.ToString();

            item.gameObject.AddComponent<ButtonEventListener>().dto = mail;
        }

        Button btnClose = _transform.Find("BtnClose").GetComponent<Button>();
        btnClose.onClick.AddListener(OnBtnClose);
    }

    /// <summary>
    /// 删除邮件
    /// </summary>
    /// <param name="mailid"></param>
    public void Delete(int mailid)
    {
        for(int i = 0; i < _content.childCount; i++)
        {
            Transform child = _content.GetChild(i);
            MailDTO dto = child.GetComponent<ButtonEventListener>().dto;

            if (dto.id == mailid)
                GameObject.Destroy(child.gameObject);
        }
    }

    private void OnBtnClose()
    {
        WindowManager.instance.Close<MailWnd>();
    }
}