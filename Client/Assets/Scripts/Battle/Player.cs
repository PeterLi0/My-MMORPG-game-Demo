using proto.battlesync;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Player : Character
{
    private Camera _camera;
    public override void Init(CharacterAttr attr)
    {
        base.Init(attr);

        _camera = Camera.main;
    }
    public override void Update(float dt)
    {
        UpdateMouseInput();
        base.Update(dt);
    }

    /// <summary>
    /// 更新鼠标输入
    /// </summary>
    private void UpdateMouseInput()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
#if UNITY_IPHONE || UNITY_ANDROID
    			if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
#else
            if (!EventSystem.current.IsPointerOverGameObject())
#endif

            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    int layer = hitInfo.collider.gameObject.layer;
                    if (layer == LayerMask.NameToLayer("Map"))
                    {
                        Vector3 pos = new Vector3 { x = hitInfo.point.x, y = hitInfo.point.y, z = hitInfo.point.z };

                        ReqCharacterMove req = new ReqCharacterMove();
                        req.dest = new common.Vector();
                        req.dest.x = pos.x;
                        req.dest.y = pos.y;
                        req.dest.z = pos.z;
                        Net.instance.Send((int)MsgID.ReqCharacterMove, req);
                    }
                }
            }
        }
    }
}
