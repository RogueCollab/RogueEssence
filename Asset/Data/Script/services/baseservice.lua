--[[
  Inherit this class by using
      NewClass = Class("NewClass", BaseService)
  To make a new class object meant to run a lua service.
  
  A lua service is a persistent lua script that can receive generic callbacks from the engine during execution.
  This allows it to override or modify usual game behavior as the game runs, or offer additional services to script packages.
]]--

require 'common'


-------------------------------------------------------
-- BaseService 
-------------------------------------------------------
BaseService = Class("BaseService")

---Summary
-- Service constructor
function BaseService:initialize()
end


---Summary
-- Subscribe to all channels this service wants callbacks from
function BaseService:Subscribe(med)
end

---Summary
-- un-subscribe to all channels this service subscribed to
function BaseService:UnSubscribe(med)
end

---Summary
-- The update method is run as a coroutine for each services.
function BaseService:Update(gtime)
--  while(true)
--    coroutine.yield()
--  end
end