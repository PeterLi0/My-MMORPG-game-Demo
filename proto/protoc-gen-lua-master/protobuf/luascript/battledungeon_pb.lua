-- Generated By protoc-gen-lua Do not Edit
local protobuf = require "protobuf"
local common_pb = require("common_pb")
module('battledungeon_pb')


local REQMATCHDUNGEON = protobuf.Descriptor();
local REQMATCHDUNGEON_MAPID_FIELD = protobuf.FieldDescriptor();
local RSPMATCHDUNGEON = protobuf.Descriptor();
local RSPMATCHDUNGEON_MSGTIPS_FIELD = protobuf.FieldDescriptor();
local REQCANCELMATCHDUNGEON = protobuf.Descriptor();
local RSPCANCELMATCHDUNGEON = protobuf.Descriptor();
local RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD = protobuf.FieldDescriptor();
local NOTIFYMATCHCOMPLETE = protobuf.Descriptor();
local NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD = protobuf.FieldDescriptor();
local REQSTARTDUNGEON = protobuf.Descriptor();
local RSPSTARTDUNGEON = protobuf.Descriptor();
local RSPSTARTDUNGEON_MSGTIPS_FIELD = protobuf.FieldDescriptor();
local REPENTERDUNGEON = protobuf.Descriptor();
local NOTIFYPLAYERENTERDUNGEON = protobuf.Descriptor();
local NOTIFYPLAYERENTERDUNGEON_CHS_FIELD = protobuf.FieldDescriptor();
local REQEXITDUNGEON = protobuf.Descriptor();
local RSPEXITDUNGEON = protobuf.Descriptor();
local RSPEXITDUNGEON_MSGTIPS_FIELD = protobuf.FieldDescriptor();
local NOTIFYEXITDUNGEON = protobuf.Descriptor();
local NOTIFYEXITDUNGEON_CHARACTERID_FIELD = protobuf.FieldDescriptor();

REQMATCHDUNGEON_MAPID_FIELD.name = "mapid"
REQMATCHDUNGEON_MAPID_FIELD.full_name = ".ReqMatchDungeon.mapid"
REQMATCHDUNGEON_MAPID_FIELD.number = 1
REQMATCHDUNGEON_MAPID_FIELD.index = 0
REQMATCHDUNGEON_MAPID_FIELD.label = 2
REQMATCHDUNGEON_MAPID_FIELD.has_default_value = false
REQMATCHDUNGEON_MAPID_FIELD.default_value = 0
REQMATCHDUNGEON_MAPID_FIELD.type = 13
REQMATCHDUNGEON_MAPID_FIELD.cpp_type = 3

REQMATCHDUNGEON.name = "ReqMatchDungeon"
REQMATCHDUNGEON.full_name = ".ReqMatchDungeon"
REQMATCHDUNGEON.nested_types = {}
REQMATCHDUNGEON.enum_types = {}
REQMATCHDUNGEON.fields = {REQMATCHDUNGEON_MAPID_FIELD}
REQMATCHDUNGEON.is_extendable = false
REQMATCHDUNGEON.extensions = {}
RSPMATCHDUNGEON_MSGTIPS_FIELD.name = "msgtips"
RSPMATCHDUNGEON_MSGTIPS_FIELD.full_name = ".RspMatchDungeon.msgtips"
RSPMATCHDUNGEON_MSGTIPS_FIELD.number = 1
RSPMATCHDUNGEON_MSGTIPS_FIELD.index = 0
RSPMATCHDUNGEON_MSGTIPS_FIELD.label = 2
RSPMATCHDUNGEON_MSGTIPS_FIELD.has_default_value = false
RSPMATCHDUNGEON_MSGTIPS_FIELD.default_value = nil
RSPMATCHDUNGEON_MSGTIPS_FIELD.enum_type = COMMON_PB_MSGTIPS
RSPMATCHDUNGEON_MSGTIPS_FIELD.type = 14
RSPMATCHDUNGEON_MSGTIPS_FIELD.cpp_type = 8

RSPMATCHDUNGEON.name = "RspMatchDungeon"
RSPMATCHDUNGEON.full_name = ".RspMatchDungeon"
RSPMATCHDUNGEON.nested_types = {}
RSPMATCHDUNGEON.enum_types = {}
RSPMATCHDUNGEON.fields = {RSPMATCHDUNGEON_MSGTIPS_FIELD}
RSPMATCHDUNGEON.is_extendable = false
RSPMATCHDUNGEON.extensions = {}
REQCANCELMATCHDUNGEON.name = "ReqCancelMatchDungeon"
REQCANCELMATCHDUNGEON.full_name = ".ReqCancelMatchDungeon"
REQCANCELMATCHDUNGEON.nested_types = {}
REQCANCELMATCHDUNGEON.enum_types = {}
REQCANCELMATCHDUNGEON.fields = {}
REQCANCELMATCHDUNGEON.is_extendable = false
REQCANCELMATCHDUNGEON.extensions = {}
RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD.name = "msgtips"
RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD.full_name = ".RspCancelMatchDungeon.msgtips"
RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD.number = 1
RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD.index = 0
RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD.label = 2
RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD.has_default_value = false
RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD.default_value = nil
RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD.enum_type = COMMON_PB_MSGTIPS
RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD.type = 14
RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD.cpp_type = 8

RSPCANCELMATCHDUNGEON.name = "RspCancelMatchDungeon"
RSPCANCELMATCHDUNGEON.full_name = ".RspCancelMatchDungeon"
RSPCANCELMATCHDUNGEON.nested_types = {}
RSPCANCELMATCHDUNGEON.enum_types = {}
RSPCANCELMATCHDUNGEON.fields = {RSPCANCELMATCHDUNGEON_MSGTIPS_FIELD}
RSPCANCELMATCHDUNGEON.is_extendable = false
RSPCANCELMATCHDUNGEON.extensions = {}
NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD.name = "msgtips"
NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD.full_name = ".NotifyMatchComplete.msgtips"
NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD.number = 1
NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD.index = 0
NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD.label = 2
NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD.has_default_value = false
NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD.default_value = nil
NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD.enum_type = COMMON_PB_MSGTIPS
NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD.type = 14
NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD.cpp_type = 8

NOTIFYMATCHCOMPLETE.name = "NotifyMatchComplete"
NOTIFYMATCHCOMPLETE.full_name = ".NotifyMatchComplete"
NOTIFYMATCHCOMPLETE.nested_types = {}
NOTIFYMATCHCOMPLETE.enum_types = {}
NOTIFYMATCHCOMPLETE.fields = {NOTIFYMATCHCOMPLETE_MSGTIPS_FIELD}
NOTIFYMATCHCOMPLETE.is_extendable = false
NOTIFYMATCHCOMPLETE.extensions = {}
REQSTARTDUNGEON.name = "ReqStartDungeon"
REQSTARTDUNGEON.full_name = ".ReqStartDungeon"
REQSTARTDUNGEON.nested_types = {}
REQSTARTDUNGEON.enum_types = {}
REQSTARTDUNGEON.fields = {}
REQSTARTDUNGEON.is_extendable = false
REQSTARTDUNGEON.extensions = {}
RSPSTARTDUNGEON_MSGTIPS_FIELD.name = "msgtips"
RSPSTARTDUNGEON_MSGTIPS_FIELD.full_name = ".RspStartDungeon.msgtips"
RSPSTARTDUNGEON_MSGTIPS_FIELD.number = 1
RSPSTARTDUNGEON_MSGTIPS_FIELD.index = 0
RSPSTARTDUNGEON_MSGTIPS_FIELD.label = 2
RSPSTARTDUNGEON_MSGTIPS_FIELD.has_default_value = false
RSPSTARTDUNGEON_MSGTIPS_FIELD.default_value = nil
RSPSTARTDUNGEON_MSGTIPS_FIELD.enum_type = COMMON_PB_MSGTIPS
RSPSTARTDUNGEON_MSGTIPS_FIELD.type = 14
RSPSTARTDUNGEON_MSGTIPS_FIELD.cpp_type = 8

RSPSTARTDUNGEON.name = "RspStartDungeon"
RSPSTARTDUNGEON.full_name = ".RspStartDungeon"
RSPSTARTDUNGEON.nested_types = {}
RSPSTARTDUNGEON.enum_types = {}
RSPSTARTDUNGEON.fields = {RSPSTARTDUNGEON_MSGTIPS_FIELD}
RSPSTARTDUNGEON.is_extendable = false
RSPSTARTDUNGEON.extensions = {}
REPENTERDUNGEON.name = "RepEnterDungeon"
REPENTERDUNGEON.full_name = ".RepEnterDungeon"
REPENTERDUNGEON.nested_types = {}
REPENTERDUNGEON.enum_types = {}
REPENTERDUNGEON.fields = {}
REPENTERDUNGEON.is_extendable = false
REPENTERDUNGEON.extensions = {}
NOTIFYPLAYERENTERDUNGEON_CHS_FIELD.name = "chs"
NOTIFYPLAYERENTERDUNGEON_CHS_FIELD.full_name = ".NotifyPlayerEnterDungeon.chs"
NOTIFYPLAYERENTERDUNGEON_CHS_FIELD.number = 1
NOTIFYPLAYERENTERDUNGEON_CHS_FIELD.index = 0
NOTIFYPLAYERENTERDUNGEON_CHS_FIELD.label = 3
NOTIFYPLAYERENTERDUNGEON_CHS_FIELD.has_default_value = false
NOTIFYPLAYERENTERDUNGEON_CHS_FIELD.default_value = {}
NOTIFYPLAYERENTERDUNGEON_CHS_FIELD.message_type = COMMON_PB_CHARACTERDTO
NOTIFYPLAYERENTERDUNGEON_CHS_FIELD.type = 11
NOTIFYPLAYERENTERDUNGEON_CHS_FIELD.cpp_type = 10

NOTIFYPLAYERENTERDUNGEON.name = "NotifyPlayerEnterDungeon"
NOTIFYPLAYERENTERDUNGEON.full_name = ".NotifyPlayerEnterDungeon"
NOTIFYPLAYERENTERDUNGEON.nested_types = {}
NOTIFYPLAYERENTERDUNGEON.enum_types = {}
NOTIFYPLAYERENTERDUNGEON.fields = {NOTIFYPLAYERENTERDUNGEON_CHS_FIELD}
NOTIFYPLAYERENTERDUNGEON.is_extendable = false
NOTIFYPLAYERENTERDUNGEON.extensions = {}
REQEXITDUNGEON.name = "ReqExitDungeon"
REQEXITDUNGEON.full_name = ".ReqExitDungeon"
REQEXITDUNGEON.nested_types = {}
REQEXITDUNGEON.enum_types = {}
REQEXITDUNGEON.fields = {}
REQEXITDUNGEON.is_extendable = false
REQEXITDUNGEON.extensions = {}
RSPEXITDUNGEON_MSGTIPS_FIELD.name = "msgtips"
RSPEXITDUNGEON_MSGTIPS_FIELD.full_name = ".RspExitDungeon.msgtips"
RSPEXITDUNGEON_MSGTIPS_FIELD.number = 1
RSPEXITDUNGEON_MSGTIPS_FIELD.index = 0
RSPEXITDUNGEON_MSGTIPS_FIELD.label = 2
RSPEXITDUNGEON_MSGTIPS_FIELD.has_default_value = false
RSPEXITDUNGEON_MSGTIPS_FIELD.default_value = nil
RSPEXITDUNGEON_MSGTIPS_FIELD.enum_type = COMMON_PB_MSGTIPS
RSPEXITDUNGEON_MSGTIPS_FIELD.type = 14
RSPEXITDUNGEON_MSGTIPS_FIELD.cpp_type = 8

RSPEXITDUNGEON.name = "RspExitDungeon"
RSPEXITDUNGEON.full_name = ".RspExitDungeon"
RSPEXITDUNGEON.nested_types = {}
RSPEXITDUNGEON.enum_types = {}
RSPEXITDUNGEON.fields = {RSPEXITDUNGEON_MSGTIPS_FIELD}
RSPEXITDUNGEON.is_extendable = false
RSPEXITDUNGEON.extensions = {}
NOTIFYEXITDUNGEON_CHARACTERID_FIELD.name = "characterid"
NOTIFYEXITDUNGEON_CHARACTERID_FIELD.full_name = ".NotifyExitDungeon.characterid"
NOTIFYEXITDUNGEON_CHARACTERID_FIELD.number = 1
NOTIFYEXITDUNGEON_CHARACTERID_FIELD.index = 0
NOTIFYEXITDUNGEON_CHARACTERID_FIELD.label = 2
NOTIFYEXITDUNGEON_CHARACTERID_FIELD.has_default_value = false
NOTIFYEXITDUNGEON_CHARACTERID_FIELD.default_value = 0
NOTIFYEXITDUNGEON_CHARACTERID_FIELD.type = 13
NOTIFYEXITDUNGEON_CHARACTERID_FIELD.cpp_type = 3

NOTIFYEXITDUNGEON.name = "NotifyExitDungeon"
NOTIFYEXITDUNGEON.full_name = ".NotifyExitDungeon"
NOTIFYEXITDUNGEON.nested_types = {}
NOTIFYEXITDUNGEON.enum_types = {}
NOTIFYEXITDUNGEON.fields = {NOTIFYEXITDUNGEON_CHARACTERID_FIELD}
NOTIFYEXITDUNGEON.is_extendable = false
NOTIFYEXITDUNGEON.extensions = {}

NotifyExitDungeon = protobuf.Message(NOTIFYEXITDUNGEON)
NotifyMatchComplete = protobuf.Message(NOTIFYMATCHCOMPLETE)
NotifyPlayerEnterDungeon = protobuf.Message(NOTIFYPLAYERENTERDUNGEON)
RepEnterDungeon = protobuf.Message(REPENTERDUNGEON)
ReqCancelMatchDungeon = protobuf.Message(REQCANCELMATCHDUNGEON)
ReqExitDungeon = protobuf.Message(REQEXITDUNGEON)
ReqMatchDungeon = protobuf.Message(REQMATCHDUNGEON)
ReqStartDungeon = protobuf.Message(REQSTARTDUNGEON)
RspCancelMatchDungeon = protobuf.Message(RSPCANCELMATCHDUNGEON)
RspExitDungeon = protobuf.Message(RSPEXITDUNGEON)
RspMatchDungeon = protobuf.Message(RSPMATCHDUNGEON)
RspStartDungeon = protobuf.Message(RSPSTARTDUNGEON)

