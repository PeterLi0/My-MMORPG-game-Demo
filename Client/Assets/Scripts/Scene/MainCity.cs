//using System;
//using System.Collections.Generic;

//using UnityEngine;
//using proto.character;
//using proto.battlescene;

//public class MainCity : Singleton<MainCity>, IScene
//{
//    public void Initialize()
//    {
//        WindowManager.instance.Open<MainWnd>().Initialize();

//        // 向服务端发送角色上线请求
//        ReqCharacterOnline req = new ReqCharacterOnline();
//        req.characterid = DataCache.instance.currentCharacter.id;
//        NetworkManager.instance.Send((int)MsgID.CHAR_ONLINE_CREQ, req);

//        // 请求世界中的其他角色数据
//        ReqSceneCharacters reqScene = new ReqSceneCharacters();

//        NetworkManager.instance.Send((int)MsgID.SCENE_Char_CREQ, reqScene);
//    }
//    public void Finalise()
//    {
//        WindowManager.instance.Close<MainWnd>();
//        CharacterManager.instance.Clear();
//    }
//}