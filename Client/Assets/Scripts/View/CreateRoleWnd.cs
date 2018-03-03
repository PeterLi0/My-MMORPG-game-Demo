using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using proto.character;
using account;
using common;

public class CreateRoleWnd : BaseWnd
{
    public class ButtonClickListener : MonoBehaviour, IPointerClickHandler
    {
        public RoleCfg cfg;

        public void OnPointerClick(PointerEventData eventData)
        {
            WindowManager.instance.Get<CreateRoleWnd>().currentRoleCfg = cfg;
            SelectRole.instance.Select(cfg.ID);
        }
    }

    private InputField _roleName;

    // 当前角色配置
    public RoleCfg currentRoleCfg;

    public void Initialize()
    {
        _roleName = _transform.Find("RoleName").GetComponent<InputField>();

        Button btnReturn = _transform.Find("BtnReturn").GetComponent<Button>();
        btnReturn.onClick.AddListener(OnReturn);

        Button btnEnterGame = _transform.Find("BtnCreateRole").GetComponent<Button>();
        btnEnterGame.onClick.AddListener(OnCreateRole);

        Transform content = _transform.Find("Scroll View/Viewport/Content");
        Button btnClone = _transform.Find("Scroll View/Viewport/BtnRole").GetComponent<Button>();

        Dictionary<int, RoleCfg> roleCfgs = ConfigManager.instance.GetTypeRoleCfgs(RoleType.Player);
        currentRoleCfg = roleCfgs[1001];

        foreach (RoleCfg cfg in roleCfgs.Values)
        {
            if (cfg.RoleType != RoleType.Player) continue;

            Transform btnRole = (GameObject.Instantiate(btnClone.gameObject) as GameObject).transform;
            btnRole.SetParent(content);
            btnRole.localScale = Vector3.one;
            btnRole.localPosition = Vector3.zero;
            btnRole.gameObject.SetActive(true);

            Text ID = btnRole.Find("ID").GetComponent<Text>();
            ID.text = cfg.ID.ToString();

            Text name = btnRole.Find("Name").GetComponent<Text>();
            name.text = cfg.RoleName;

            btnRole.gameObject.AddComponent<ButtonClickListener>().cfg = cfg;
        }
    }


    private void OnReturn()
    {
        ReqOffline req = new ReqOffline();
        Net.instance.Send((int)MsgID.ACC_OFFLINE_CREQ, req);
    }

    private void OnCreateRole()
    {
        ReqAddCharacter req = new ReqAddCharacter();
        CharacterDTO d = new CharacterDTO();
        d.id = 0;
        d.accountid = 0;
        d.name = _roleName.text;
        d.race = 0;
        d.job = 0;
        d.gender = 0;
        d.level = 0;
        d.exp = 0;
        d.diamond = 0;
        d.gold = 0;
        d.pos_x = 0f;
        d.pos_y = 0f;
        d.pos_z = 0f;
        d.cfgid = currentRoleCfg.ID ;

        req.character = d;

        Net.instance.Send((int)MsgID.CHAR_CREATE_CREQ, req);
    }
}