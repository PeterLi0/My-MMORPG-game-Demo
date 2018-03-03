--主入口函数。从这里开始lua逻辑

function Main()
	LuaHelper =LuaFramework.LuaHelper;
	resMgr = LuaHelper.GetResManager();
	resMgr:LoadPrefab("C1001",{"C1001"},OnLoadFinish);

	--UpdateBeat:Add(Tick, self);
end

function Tick()
	LuaFramework.Util.Log("每帧执行");
end

function OnLoadFinish(objs)
	UnityEngine.GameObject.Instantiate(objs[0]);
	LuaFramework.Util.Log("实例化完毕");

end


--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

