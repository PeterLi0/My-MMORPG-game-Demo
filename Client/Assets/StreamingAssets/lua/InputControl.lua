---
--- Created by shang.
--- DateTime: 2017/12/14 15:27
---

require "Main"
require "Component"

local ch;

function Main()
    LuaHelper =LuaFramework.LuaHelper;
    resMgr = LuaHelper.GetResManager();
    resMgr:LoadPrefab("C1005_AAA",{"C1005"},OnLoadFinish);

    local  go = UnityEngine.GameObject("go");
    LuaComponent.Add(go,Component);
    local cmp = LuaComponent.Get(go,Component);
    print("攻击力：" ,cmp.hit);

    resMgr:LoadPrefab("button", {"Button"},OnButtonLoad );

end


function Update()
    local Input = UnityEngine.Input;
    local vertical = Input.GetAxis("Vertical");
    local horizontal = Input.GetAxis("Horizontal");
    local x= ch.transform.position.x + vertical;
    local y= ch.transform.position.y + horizontal;
    ch.transform.position = Vector3.New(x,0,z);


    MyCustom.CSharpTest.ShowMessage();
    MyCustom.CSharpTest.ShowMessage(55555);

    local test = MyCustom.CSharpTest().New();
    test:Show();
end


function OnLoadFinish(objs)
    ch =UnityEngine.GameObject.Instantiate(objs[0]);
    LuaFramework.Util.Log("实例化完毕");
    UpdateBeat:Add(Update,Self);
end

function OnButtonLoad(objs)
    local btn = UnityEngine.GameObject.Instantiate(objs[0]);
    btn.transform:SetParent(UnityEngine.GameObject.Find("Canvas").transform);
    btn.transform.localPosition = Vector3.zero;
    btn.transform.localScale = Vector3.one;
    UIEvent.AddButtonClick(btn, OnBtnClick);
end

function OnBtnClick()
    print("按钮点击了");
end
