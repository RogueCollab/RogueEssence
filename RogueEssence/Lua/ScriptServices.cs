using System;
using System.Collections.Generic;
using System.Linq;
using RogueEssence.Dungeon;
using Microsoft.Xna.Framework;
using NLua;
using System.IO;
using System.Xml.Linq;

namespace RogueEssence.Script
{


    /// <summary>
    /// Component handling managing script services and various script related tasks
    /// </summary>
    class ScriptServices : ILuaEngineComponent
    {
        private struct ServiceEntry
        {
            public string      name;
            public LuaTable    lobj;
            public TimeSpan    updateinterval; //Rate to update this service at
            public Dictionary<string, LuaFunction> callbacks;
        }

        #region Constants
        public static readonly string SInterfaceInstanceName = "SCRIPT";
        public static readonly int ScriptSvcUpdateInt = 20;
        public static readonly string ScriptSvcDir = "services";
        #endregion

        #region Variables
        private LuaEngine m_state; //reference on the main lua state!
        private Dictionary<string, ServiceEntry>    m_services;  //An internal copie of all services instances
        private LuaFunction                         m_fncallsub;
        private LuaFunction                         m_fncallunsub;
        #endregion

        public LuaEngine State { get { return m_state; } set { m_state = value; } }

        public ScriptServices(LuaEngine state )
        {
            m_services      = new Dictionary<string, ServiceEntry>();
            m_state         = state;
        }

        /// <summary>
        /// Returns the script package path for the currently loaded level
        /// </summary>
        /// <returns></returns>
        public string CurrentScriptDir()
        {
            //We can get the path 3 ways.
            if (ZoneManager.Instance.CurrentGround != null)
                return Path.GetDirectoryName(LuaEngine.MakeGroundMapScriptPath(false, ZoneManager.Instance.CurrentGround.AssetName, "/init.lua"));
            else if (ZoneManager.Instance.CurrentMap != null)
                return Path.GetDirectoryName(LuaEngine.MakeDungeonMapScriptPath(false, ZoneManager.Instance.CurrentMap.AssetName, "/init.lua"));
            else
                throw new Exception("ScriptServices.CurrentScriptDir(): No map lua package currently loaded! And no map currently loaded either! Cannot assemble the current package path!");
        }

        /// <summary>
        /// Send a message to all the services listening for it.
        /// </summary>
        /// <param name="msgname">Name of the message</param>
        /// <param name="arguments">Value passed along the message</param>
        public void Publish( string msgname, params object[] arguments )
        {
            //DiagManager.Instance.LogInfo("[SE]: Dispatching " + msgname + " event!!");

            foreach(var svc in m_services)
            {
                if(svc.Value.callbacks.ContainsKey(msgname))
                    svc.Value.callbacks[msgname].Call(svc.Value.lobj, arguments);
            }
        }
        public IEnumerator<YieldInstruction> PublishCoroutine(string msgname, params object[] arguments)
        {
            //DiagManager.Instance.LogInfo("[SE]: Dispatching " + msgname + " event!!");

            foreach (var svc in m_services)
            {
                if (svc.Value.callbacks.ContainsKey(msgname))
                {
                    LuaFunction func = svc.Value.callbacks[msgname];
                    LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(func, svc.Value.lobj, arguments);

                    yield return CoroutineManager.Instance.StartCoroutine(ScriptEvent.ApplyFunc(msgname, func_iter));
                }
            }
        }

        /// <summary>
        /// Installs some common lua functions.
        /// </summary>
        public override void SetupLuaFunctions(LuaEngine state)
        {
            m_fncallsub = State.RunString("return function(med, svc) xpcall(svc.Subscribe, PrintStack, svc, med) end").First() as LuaFunction;
            m_fncallunsub = State.RunString("return function(med, svc) xpcall(svc.UnSubscribe, svc, med) end").First() as LuaFunction;
        }

        /// <summary>
        /// Add a service to the list of managed services
        /// </summary>
        /// <param name="name">Handle for the given service instance.</param>
        /// <param name="instance"></param>
        public void AddService(string name, LuaTable instance)
        {
            ServiceEntry svc = new ServiceEntry();
            svc.name = name;
            svc.lobj = instance;
            svc.updateinterval = new TimeSpan(0, 0, 0, 0, ScriptSvcUpdateInt);
            svc.callbacks = new Dictionary<string, LuaFunction>();
            m_services.Add(name, svc);

            //Tell the service to subscribe its callbacks
            m_fncallsub.Call(this, svc.lobj);
            DiagManager.Instance.LogInfo("[SE]:Registered service " + name + "!");
        }

        /// <summary>
        /// Removes the given service from the service list
        /// </summary>
        /// <param name="name"></param>
        public void RemoveService(string name)
        {
            if (m_services.ContainsKey(name))
            {
                ServiceEntry svc = m_services[name];
                m_fncallunsub.Call(this, svc.lobj);
                m_services.Remove(name);
            }
        }

        /// <summary>
        /// Get a service's lua instance by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LuaTable GetService(string name)
        {
            if (m_services.ContainsKey(name))
                return m_services[name].lobj;
            else
                return null;
        }


        /// <summary>
        /// Returns the path to the services directory
        /// </summary>
        /// <returns></returns>
        public string ServiceDirectoryPath()
        {
            return String.Format("{0}//{1}", LuaEngine.SCRIPT_PATH, ScriptSvcDir);
        }

        /// <summary>
        /// Used to subscribe a lua function to be called on a pre-defined service callback for the given service.
        /// Essentially, use this to register lua callbacks for a lua service.
        /// </summary>
        public void Subscribe(string svc, string eventname, LuaFunction fn)
        {
            foreach( var serv in m_services )
            {
                if (serv.Key == svc)
                    serv.Value.callbacks.Add(eventname, fn);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void UnSubscribe(string svc, string eventname)
        {
            foreach (var serv in m_services)
            {
                if (serv.Key == svc && serv.Value.callbacks.ContainsKey(eventname))
                    serv.Value.callbacks.Remove(eventname);
            }
        }

        /// <summary>
        /// Sends the Update message to all services listening for it!
        /// </summary>
        /// <param name="gtime">Current game engine time.</param>
        public void UpdateServices(GameTime gtime)
        {
            //TODO: Need to come up with something to hopefully reduce script induced latency for stuff being processed often. Coroutines will probably be handy here.
        }
    }
}
