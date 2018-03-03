//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;

//public class SelectLevelWnd : BaseWnd
//{
//    public class ButtonClickListener : MonoBehaviour, IPointerClickHandler
//    {
//        public LevelCfg cfg;

//        public void OnPointerClick(PointerEventData eventData)
//        {
//            WindowManager.instance.Close<SelectLevelWnd>();

//            MainCity.instance.Finalise();
//            Loading.instance.LoadScene(cfg.Scene);

//            DataCache.instance.currentLevelCfg = cfg;
//        }
//    }

//    public void Initialize()
//    {
//        Button btnReturn = _transform.FindChild("BtnReturn").GetComponent<Button>();
//        btnReturn.onClick.AddListener(OnReturn);

//        Transform content = _transform.FindChild("Scroll View/Viewport/Content");
//        Button btnLevel = _transform.FindChild("Scroll View/Viewport/BtnLevel").GetComponent<Button>();

//        Dictionary<int, LevelCfg> levelCfgs = ConfigManager.instance.levelCfgs;
//        foreach(LevelCfg cfg in levelCfgs.Values)
//        {
//            Transform level = (GameObject.Instantiate(btnLevel.gameObject) as GameObject).transform;
//            level.SetParent(content);
//            level.gameObject.SetActive(true);
//            level.localScale = Vector3.one;
//            level.localPosition = Vector3.zero;

//            // 关卡编号
//            Text no = level.FindChild("No").GetComponent<Text>();
//            no.text = cfg.No.ToString();

//            Text name = level.FindChild("Name").GetComponent<Text>();
//            name.text = cfg.Name;

//            level.gameObject.AddComponent<ButtonClickListener>().cfg = cfg;
//        }
//    }

//    private void OnReturn()
//    {
//        WindowManager.instance.Close<SelectLevelWnd>();
//    }
//}

