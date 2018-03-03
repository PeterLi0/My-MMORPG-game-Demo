---
--- Created by shang.
--- DateTime: 2017/12/19 12:24
---

CharacterHandler = {};
local this = CharacterHandler;

function CharacterHandler.Register()
    networkMgr:Register(1011, CharacterHandler.OnCreateCharacter);
end

function CharacterHandler.OnCreateCharacter(m)

end