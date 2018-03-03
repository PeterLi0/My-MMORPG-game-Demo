using System;
using System.Collections.Generic;


public partial class RedisCacheManager : Singleton<RedisCacheManager>
{
    private RedisManager _redis = new RedisManager();

    public void Init()
    {
        _redis.Init();
    }
}
