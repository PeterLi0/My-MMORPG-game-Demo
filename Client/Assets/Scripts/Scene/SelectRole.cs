using System;
using System.Collections.Generic;
using UnityEngine;
using common;

public class SelectRole : Singleton<SelectRole>, IScene
{
    // 当创建时，保存配置ID和游戏对象；当选择角色时，保存角色ID和游戏对象
    private Dictionary<int, GameObject> _roles = new Dictionary<int, GameObject>();

    // 当前角色
    private GameObject _currentRole;

    public void Initialize()
    {
        if(DataCache.instance.chDtos.Count > 0)
        {
            ShowSelectRole();
        }
        else
        {
            ShowCreateRole();
        }
    }

    // 创建角色
    public void ShowCreateRole()
    {
        WindowManager.instance.Open<CreateRoleWnd>().Initialize();
        Dictionary<int, RoleCfg> cfgs = ConfigManager.instance.GetTypeRoleCfgs(RoleType.Player);

        foreach (GameObject go in _roles.Values)
        {
            GameObject.Destroy(go);
        }
        _roles.Clear();

        int id = 1001;
        foreach(RoleCfg cfg in cfgs.Values)
        {
            Transform role = (GameObject.Instantiate(Resources.Load("Units/" + cfg.ModelName)) as GameObject).transform;
            role.name = cfg.ModelName;
            role.localScale = Vector3.one;
            role.position = Vector3.zero;

            if(cfg.ID == id)
            {
                role.gameObject.SetActive(true);
                _currentRole = role.gameObject;
            }
            else
            {
                role.gameObject.SetActive(false);
            }


            _roles.Add(cfg.ID, role.gameObject);
        }
    }

    public void Select(int id)
    {
        foreach(KeyValuePair<int,GameObject> pair in _roles)
        {
            if(pair.Key == id)
            {
                pair.Value.SetActive(true);
                _currentRole = pair.Value;
            }
            else
            {
                pair.Value.SetActive(false);
            }
        }
    }

    public void RotateRole(float x)
    {
        _currentRole.transform.Rotate(0, -x, 0);
    }


    // 选择角色
    public void ShowSelectRole()
    {
        WindowManager.instance.Open<SelectRoleWnd>().Initialize();

        foreach(GameObject go in _roles.Values)
        {
            GameObject.Destroy(go);
        }
        _roles.Clear();

        List<CharacterDTO> dtos = DataCache.instance.chDtos;
        for(int i = 0; i < dtos.Count; i++)
        {
            CharacterDTO dto = dtos[i];
            RoleCfg cfg = ConfigManager.instance.GetRoleCfg(dto.cfgid);
            Transform role = (GameObject.Instantiate(Resources.Load("Units/" + cfg.ModelName)) as GameObject).transform;
            role.name = cfg.ModelName;
            role.localScale = Vector3.one;
            role.position = Vector3.zero;

            if(dto.id == dtos[0].id)
            {
                role.gameObject.SetActive(true);
                _currentRole = role.gameObject;
                DataCache.instance.currentCharacter = dto;
            }
            else
            {
                role.gameObject.SetActive(false);
            }


            _roles.Add(dto.id, role.gameObject);
        }
    }

    public void Finalise()
    {
        WindowManager.instance.Close<CreateRoleWnd>();
        WindowManager.instance.Close<SelectRoleWnd>();
    }
}