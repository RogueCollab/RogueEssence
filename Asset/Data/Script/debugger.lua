--[[
    debugger.lua
    
]]--
----------------------------------------
-- Debugging
----------------------------------------
DEBUG = 
{
  EnableDbgCoro = function() end, --Call this function inside coroutines you want to allow debugging of, at the start. Default is empty
  IsDevMode = function() return RogueEssence.DiagManager.Instance.DevMode end,
  IsDevLua = function() return RogueEssence.DiagManager.Instance.DebugLua end,
  GroundAIShowDebugInfo = false,
}

DEBUG.GroundAIShowDebugInfo = false--DEBUG.IsDevMode()

--Disable debugging for non devs
if DEBUG.IsDevLua() then
  ___mobdebug = require('mobdebug')
  ___mobdebug.coro() --Enable coroutine debugging
  ___mobdebug.checkcount = 1 --Increase debugger update frequency
  ___mobdebug.verbose=true --Enable debugger verbose mode
  ___mobdebug.start() --Enable debugging
  DEBUG.EnableDbgCoro = function() require('mobdebug').on() end --Set the content of the function to this in dev mode, so it does something
end
