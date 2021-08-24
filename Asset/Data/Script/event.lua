require 'common'

SINGLE_CHAR_SCRIPT = {}

function SINGLE_CHAR_SCRIPT.Test(owner, ownerChar, character, args)
  PrintInfo("Test")
end


BATTLE_SCRIPT = {}

function BATTLE_SCRIPT.Test(owner, ownerChar, context, args)
  PrintInfo("Test")
end

STATUS_SCRIPT = {}

function STATUS_SCRIPT.Test(owner, ownerChar, context, args)
  PrintInfo("Test")
end


MAP_STATUS_SCRIPT = {}

function MAP_STATUS_SCRIPT.Test(owner, ownerChar, character, status, msg, args)
  PrintInfo("Test")
end

REFRESH_SCRIPT = {}

function REFRESH_SCRIPT.Test(owner, ownerChar, character, args)
  PrintInfo("Test")
end

SKILL_CHANGE_SCRIPT = {}

function SKILL_CHANGE_SCRIPT.Test(owner, character, skillIndices, args)
  PrintInfo("Test")
end

ZONE_GEN_SCRIPT = {}

function ZONE_GEN_SCRIPT.Test(zoneContext, context, queue, seed, args)
  PrintInfo("Test")
end


FLOOR_GEN_SCRIPT = {}

function FLOOR_GEN_SCRIPT.Test(map, args)
  PrintInfo("Test")
end




