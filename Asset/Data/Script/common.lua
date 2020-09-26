--[[
    common.lua
    A collection of frequently used functions and values!
]]--

----------------------------------------
-- Debugging
----------------------------------------
DEBUG = 
{
  EnableDbgCoro = function() end, --Call this function inside coroutines you want to allow debugging of, at the start. Default is empty
  --FIXME: fix mobdebug and sockets
  IsDevMode = function() return false end, --RogueEssence.DiagManager.Instance.DevMode end,
  GroundAIShowDebugInfo = false,
}

DEBUG.GroundAIShowDebugInfo = false--DEBUG.IsDevMode()

--Disable debugging for non devs
if DEBUG.IsDevMode() then
  ___mobdebug = require('mobdebug')
  ___mobdebug.coro() --Enable coroutine debugging
  ___mobdebug.checkcount = 1 --Increase debugger update frequency
  ___mobdebug.verbose=true --Enable debugger verbose mode
  ___mobdebug.start() --Enable debugging
  DEBUG.EnableDbgCoro = function() require('mobdebug').on() end --Set the content of the function to this in dev mode, so it does something
end


----------------------------------------
-- Lib Definitions
----------------------------------------
--Reserve the "class" symbol for instantiating classes
Class    = require 'lib.middleclass'
--Reserve the "Mediator" symbol for instantiating the message lib class
Mediator = require 'lib.mediator' 
--Reserve the "Serpent" symbol for the serializer
Serpent = require 'lib.serpent'

----------------------------------------------------------
-- Console Writing
----------------------------------------------------------

--Prints to console!
function PrintInfo(text)
  if DiagManager then 
    DiagManager.Instance:LogInfo( '[SE]:' .. text) 
  else
    print('[SE]:' .. text)
  end
end

--Prints to console!
function PrintError(text)
  if DiagManager then 
    DiagManager.Instance:LogInfo( '[SE]:' .. text) 
  else
    print('[SE](ERROR): ' .. text)
  end
end

--Will print the stack and the specified error message
function PrintStack(err)
  PrintInfo(debug.traceback(tostring(err),2)) 
end

function PrintSVAndStrings(mapstr)
  print("DUMPING SCRIPT VARIABLE STATE..")
  print(Serpent.block(SV))
  print(Serpent.block(mapstr))
  print("-------------------------------")
end

----------------------------------------
-- Common namespace
----------------------------------------
COMMON = {}

--Automatically load the appropriate localization for the specified package, or defaults to english!
function COMMON.AutoLoadLocalizedStrings()
  print("AutoLoading Strings!..")
  --Get the package path
  local packagepath = SCRIPT:CurrentScriptDir()
  
  --After we made the path, load the file
  return STRINGS:MakePackageStringTable(packagepath)
end

