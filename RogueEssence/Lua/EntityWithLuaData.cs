using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NLua;

namespace RogueEssence.Script
{
    /// <summary>
    /// Interface for entities having a lua data table.
    /// Its meant for the lua script to be able to store state information inside entities.
    /// </summary>
    interface IEntityWithLuaData
    {
        /// <summary>
        /// Lua table containing the data for the object.
        /// Ideally, implement the interface by using a OptionalAttribute field for this one.
        /// </summary>
        LuaTable LuaData { get; set; }
    }
}