using System;
using System.Collections.Generic;
using common;

class MatchCharacterData
{
    public int characterid;
    public Race race;
}


public class Match
{
    private uint _matchid;

    private int _limitNumber = 0;

    private BattleType _battleType;

    private Dictionary<int, MatchCharacterData> _alliances = new Dictionary<int, MatchCharacterData>();

    private Dictionary<int, MatchCharacterData> _hordes = new Dictionary<int, MatchCharacterData>();

    public Match(uint matchid, int limitNumber, BattleType battleType)
    {
        _matchid = matchid;
        _limitNumber = limitNumber;
        _battleType = battleType;
    }

    public bool IsFull(Race race)
    {
        if(race == Race.Alliance)
        {
            return _alliances.Count >= _limitNumber / 2;
        }
        else
        {
            return _hordes.Count >= _limitNumber / 2;
        }
    }
}

