---
--- Created by shang.
--- DateTime: 2017/12/15 9:11
---

Component ={
    hit=100,
    name="tom",
    speed=100.2
}

function  Component:Awake()
    print("Awake isDone");
end

function  Component:Update(args)
    -- print("Update is Done");
end

function  Component:Start()
    print("Start is Done");
end

function  Component:OnCollisionEnter(collision)
    print(collision.x);
end


function  Component:New(o)
    local o = {};
    setmetatable(o,self);
    self.__index=self;
    return o;

end