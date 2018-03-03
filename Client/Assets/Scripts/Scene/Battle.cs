//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using Object = UnityEngine.Object;
//using System.Linq;
//using UnityEngine.EventSystems;
//using common;
//using Vector3 = UnityEngine.Vector3;

//public class Battle : Singleton<Battle>
//{
//    // 导航网格信息，管理导航网格的读取和实时更新
//    private LunaNavmeshQuery _navmeshQuery;

//    /// <summary>
//    /// 对象池管理器，管理关卡中的角色和特效
//    /// </summary>
//    private PoolManager _poolManager;

//    /// <summary>
//    /// 玩家
//    /// </summary>
//    private Character _player;

//    /// <summary>
//    /// 主相机，跟随相机
//    /// </summary>
//    private Camera _camera;

//    /// <summary>
//    /// 相机偏移
//    /// </summary>
//    private Vector3 _cameraOffset;

//    /// <summary>
//    /// 选中目标时，角色脚下的标记
//    /// </summary>
//    private Transform _targetTip;


//    // 当前的关卡ID
//    public int LevelID;

//    /// <summary>
//    /// 关卡中的所有角色
//    /// </summary>
//    private Dictionary<int, Character> _roles = new Dictionary<int, Character>();

//    /// <summary>
//    /// 特效对象的容器
//    /// </summary>
//    private List<GameObject> _effects = new List<GameObject>();

//    /// <summary>
//    /// 子弹容器
//    /// </summary>
//    private List<SpellBullet> _bullets = new List<SpellBullet>();

//    public LunaNavmeshQuery navmesh
//    {
//        get
//        {
//            return _navmeshQuery;
//        }
//    }

//    private bool _battleStart = false;

//    public void Initialize(string mapName)
//    {
//        // 读取导航数据，并初始化导航网格信息
//        _navmeshQuery = new LunaNavmeshQuery();
//        _navmeshQuery.Initialize("Navmesh/" + mapName);

//        Object targetTipObj = Resources.Load("Widgets/TargetTip");
//        _targetTip = (GameObject.Instantiate(targetTipObj) as GameObject).transform;
//        _targetTip.position = new Vector3(10000, 10000, 10000);
//        _targetTip.localScale = Vector3.one;

//        // 创建对象池管理器
//        _poolManager = new PoolManager();

//        _battleStart = true;
//    }

//    public void BindMainHero(Character role)
//    {
//        _player = role;

//        // 获取主相机
//        GameObject blueSideCamera = GameObject.Find("Camera_Blue");
//        GameObject redSideCamera = GameObject.Find("Camera_Red");
//        blueSideCamera.SetActive(_player.side == Race.Alliance);
//        redSideCamera.SetActive(_player.side == Race.Horde);

//        _camera = _player.side == Race.Alliance ? blueSideCamera.GetComponent<Camera>() : redSideCamera.GetComponent<Camera>();

//        // 获取相机偏移
//        _cameraOffset = _camera.transform.position - _player.position;
//    }

//    public void AddRole(int globalID, Character role)
//    {
//        if (!_roles.ContainsKey(role.GlobalID))
//        {
//            _roles.Add(role.GlobalID, role);
//        }
//    }

//    public void Update(float dt)
//    {

//        if (!_battleStart) return;


//        // 更新导航网格

//        _navmeshQuery.Update(dt);

//        foreach (Character role in _roles.Values.ToArray())
//        {
//            if (!role.alive)
//            {
//                _roles.Remove(role.GlobalID);
//            }
//            else
//            {
//                role.Update(dt);
//            }
//        }

//        // 如果特效的生存周期已经过去，就将特效对象回收到池中
//        for (int i = 0; i < _effects.Count; i++)
//        {
//            GameObject go = _effects[i];
//            ParticleSystem ps = go.GetComponent<ParticleSystem>();
//            if (!ps.IsAlive())
//            {
//                _effects.Remove(go);
//                PoolManager.instance.Unspawn(go);
//            }
//        }

//        // 更新子弹，如果子弹命中，就从子弹容器中移除
//        for (int i = 0; i < _bullets.Count; i++)
//        {
//            SpellBullet bullet = _bullets[i];
//            bullet.Update(dt);

//            if (bullet.hited)
//            {
//                _bullets.Remove(bullet);
//            }
//        }
//    }

//    /// <summary>
//    /// 更新相机跟随
//    /// </summary>
//    private void UpdateCamera()
//    {
//        if (_camera != null)
//        {
//            _camera.transform.position = _player.position + _cameraOffset;
//        }
//    }


//    /// <summary>
//    /// 玩家攻击
//    /// </summary>
//    /// <param name="index"></param>
//    private void PlayerAttack(int index)
//    {
//        if (_player.lockedTarget == null)
//            return;

//        //_player.Chase(_player.target, index);
//    }

//    /// <summary>
//    /// 通过全局唯一ID获取角色对象
//    /// </summary>
//    /// <param name="id"></param>
//    /// <returns></returns>
//    public Character GetRole(uint id)
//    {
//        if (_roles.ContainsKey((int)id))
//            return _roles[(int)id];
//        return null;
//    }

//    /// <summary>
//    /// 将特效对象添加到容器中
//    /// </summary>
//    /// <param name="go"></param>
//    public void AddEffect(GameObject go)
//    {
//        _effects.Add(go);
//    }

//    public void AddBullet(SpellBullet bullet)
//    {
//        _bullets.Add(bullet);
//    }

//    /// <summary>
//    ///  获取某个阵营的所有活着的单位
//    /// </summary>
//    /// <param name="side"></param>
//    /// <returns></returns>
//    public Dictionary<int, Character> GetAliveRoles(Race side)
//    {
//        Dictionary<int, Character> aliveRoles = new Dictionary<int, Character>();
//        foreach (Character role in _roles.Values)
//        {
//            if (role.alive && role.side == side)
//                aliveRoles.Add(role.GlobalID, role);
//        }

//        return aliveRoles;
//    }

//    /// <summary>
//    /// 获取某一阵营的某种类型的单位
//    /// </summary>
//    /// <param name="side">阵营</param>
//    /// <param name="type">角色类型</param>
//    /// <returns></returns>
//    public Dictionary<int, Character> GetSideTypeRoles(Race side, RoleType type)
//    {
//        Dictionary<int, Character> roles = new Dictionary<int, Character>();
//        foreach (Character role in _roles.Values)
//        {
//            if (role.alive && role.side == side && role._currAttr.RoleType == type)
//            {
//                roles.Add(role.GlobalID, role);
//            }
//        }

//        return roles;
//    }

//    public void Clear()
//    {
//        PoolManager.instance.Clear();
//        _poolManager = null;

//        _roles.Clear();

//        LevelID = 0;
//        _bullets.Clear();
//        _effects.Clear();
//    }


//    public bool AllMonsterDie()
//    {
//        bool end = true;
//        foreach (Character role in _roles.Values)
//        {
//            if (role._currAttr.RoleType == RoleType.Monster && role.alive)
//            {
//                end = false;
//            }
//        }
//        return end;
//    }


//    public void Initialize()
//    {
//        WindowManager.instance.Close<LoadingWnd>();
//        BattleWnd battleWnd = WindowManager.instance.Open<BattleWnd>().Initialize();

//        int levelID = 0;// DataCache.instance.currentLevelCfg.ID;
//        Dictionary<int, BornPointCfg> bpcs = ConfigManager.instance.GetBornPoints(levelID);


//        foreach (BornPointCfg bpc in bpcs.Values)
//        {
//            int roleid = bpc.RoleID == 0 ? DataCache.instance.currentCharacter.cfgid : bpc.RoleID;

//            // 通过角色ID获取角色配置
//            RoleCfg roleCfg = ConfigManager.instance.GetRoleCfg(roleid);

//            // 创建角色
//            Character character = null;// = CharacterManager.instance.Create(roleCfg, MathTools.GetPosition(bpc.Position));

//            character.side = roleCfg.RoleType == RoleType.Player ? Race.Alliance : Race.Horde;

//            // 战斗界面监听战斗中的事件
//            character.hpChanged = battleWnd.CreateBloodText;
//            character.characterDie = battleWnd.OnCharacterDie;

//            // 角色创建
//            battleWnd.OnCharacterCreate(character);
//        }
//    }

//    public void Finalise()
//    {
//        CharacterManager.instance.Clear();
//        SkillManager.instance.Clear();
//        PoolManager.instance.Clear();
//        TimerManager.instance.Clear();
//        WindowManager.instance.Close<BattleWnd>();
//    }
//}