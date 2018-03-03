
//using UnityEngine;
//using UnityEngine.EventSystems;
//using common;
//using proto.battlesync;


//public class Player : Character
//{
//    // 跟随相机
//    private Camera _camera;

//    // 相机偏移
//    private Vector3 _cameraOffset;

//    // 目标拣选标记
//    private Transform _targetTip;

//    private ETCJoystick _joystick;

//    public Player(int globalid, RoleCfg roleCfg, Vector3 position) : base(globalid, roleCfg, position)
//    {
//        _camera = Camera.main;

//        // 计算相机偏移
//        _cameraOffset = _camera.transform.position - _transform.position;

//        _targetTip = (GameObject.Instantiate(Resources.Load("Widgets/TargetTip")) as GameObject).transform;
//        _targetTip.position = new Vector3(10000, 10000, 10000);
//        _targetTip.localScale = Vector3.one;

//        // 获取虚拟摇杆，添加摇杆监听事件
//        _joystick = GameObject.Find("UI/Canvas/Joystick").GetComponent<ETCJoystick>();
//        _joystick.gameObject.SetActive(true);
//        _joystick.onMove.AddListener(OnJoystickMove);
//    }

//    public override void Update(float dt)
//    {
//        base.Update(dt);

//        UpdateMouseInput();
//        UpdateKeyInput();
//        UpdateCamera();
//    }

//    private void OnJoystickMove(Vector2 vec)
//    {
//        Vector3 pos = position + new Vector3(vec.x, 0, vec.y) * 3;
//        Move(pos);
//    }

//    /// <summary>
//    /// 更新鼠标输入
//    /// </summary>
//    private void UpdateMouseInput()
//    {
//        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
//        {
//#if UNITY_IPHONE || UNITY_ANDROID
//			if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
//#else
//            if (!EventSystem.current.IsPointerOverGameObject())
//#endif

//            {
//                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

//                RaycastHit hitInfo;
//                if (Physics.Raycast(ray, out hitInfo))
//                {
//                    int layer = hitInfo.collider.gameObject.layer;
//                    if (layer == LayerMask.NameToLayer("Map"))
//                    {
//                        Vector pos = new Vector { x = hitInfo.point.x, y = hitInfo.point.y, z = hitInfo.point.z };
//                        NetworkManager.instance.Send((int)MsgID.SCENE_CharMove_CREQ, new ReqCharacterMove { dest = pos });
//                    }
//                    else if (layer == LayerMask.NameToLayer("Role"))
//                    {
//                        // 设定锁定目标
//                        IDAgent idAgent = hitInfo.collider.GetComponent<IDAgent>();
//                        _lockedTarget = CharacterManager.instance.GetCharacter(idAgent.globalID);

//                        // 设置拣选标记的位置
//                        _targetTip.parent = hitInfo.collider.transform;
//                        _targetTip.position = hitInfo.collider.transform.position;
//                    }
//                }
//            }
//        }
//    }

//    /// <summary>
//    /// 更新键盘输入
//    /// </summary>
//    private void UpdateKeyInput()
//    {
//        if (Input.GetKeyDown(KeyCode.Q))
//        {
//            Cast(1);
//        }
//        else if (Input.GetKeyDown(KeyCode.W))
//        {
//            Cast(2);
//        }
//        else if (Input.GetKeyDown(KeyCode.E))
//        {
//            Cast(3);
//        }
//        else if (Input.GetKeyDown(KeyCode.R))
//        {
//            Cast(4);
//        }
//    }

//    /// <summary>
//    /// 相机跟随
//    /// </summary>
//    private void UpdateCamera()
//    {
//        _camera.transform.position = _transform.position + _cameraOffset;
//    }

//    /// <summary>
//    /// 释放技能
//    /// </summary>
//    /// <param name="index">技能索引</param>
//    public void Cast(int index)
//    {
//        // 技能组配置
//        SkillGroupCfg cfg = ConfigManager.instance.GetSkillGroupCfg(_cfg.ID, index);

//        // 技能基础配置
//        SkillBasicCfg basicCfg = ConfigManager.instance.GetSkillBasicCfg(cfg.ID);

//        // 技能发射器
//        SkillCaster skillCaster = _skillCasters[cfg.ID];

//        // 战斗界面
//        BattleWnd battleWnd = WindowManager.instance.Get<BattleWnd>();

//        if (basicCfg.NeedTarget && _lockedTarget == null)
//        {
//            battleWnd.ShowTips("需要一个目标");
//            return;
//        }

//        if (basicCfg.NeedTarget && _lockedTarget != null && basicCfg.AffectSide == AffectSide.Enemy && _lockedTarget.side == side)
//        {
//            battleWnd.ShowTips("非法目标");
//            return;
//        }

//        if (basicCfg.NeedTarget && _lockedTarget != null && basicCfg.AffectSide == AffectSide.Friend && _lockedTarget.side != side)
//        {
//            battleWnd.ShowTips("非法目标");
//            return;
//        }

//        if (!skillCaster.cooldown)
//        {
//            battleWnd.ShowTips("这个法术还没有准备好");
//            return;
//        }

//        if (basicCfg.NeedTarget && _lockedTarget != null && Vector3.Distance(_transform.position, _lockedTarget.position) >= basicCfg.MaxRange)
//        {
//            battleWnd.ShowTips("不在攻击范围之内");
//            return;
//        }

//        skillCaster.Cast();

//        // 施法者播放动画
//        Attack(basicCfg);

//        // 播放特效
//        if (basicCfg.CastEffect != null)
//            SkillManager.instance.PlayEffect(basicCfg.CastEffect, position, forward);
//    }

//    public override void Finalise()
//    {
//        base.Finalise();

//        _joystick.onMove.RemoveListener(OnJoystickMove);
//        _joystick.gameObject.SetActive(false);
//    }

//    public override void Die()
//    {
//        base.Die();
//        _joystick.onMove.RemoveListener(OnJoystickMove);
//        _joystick.gameObject.SetActive(false);
//    }
//}