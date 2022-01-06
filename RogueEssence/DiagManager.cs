using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using System.Text;
using RogueEssence.Dev;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;

namespace RogueEssence
{
    public class DiagManager
    {
        private static DiagManager instance;
        public static void InitInstance()
        {
            instance = new DiagManager();
        }
        public static DiagManager Instance { get { return instance; } }

        public static string CONTROLS_PATH { get => PathMod.ASSET_PATH + "Controls/"; }
        public static string LOG_PATH { get => PathMod.ExePath + "LOG/"; }
        public const string REG_PATH = "HKEY_CURRENT_USER\\Software\\RogueEssence";


        object lockObj = new object();

        public delegate void LogAdded(string message);
        public delegate string ErrorTrace();

        private bool inError;
        private LogAdded errorAddedEvent;
        private ErrorTrace errorTraceEvent;

        public SerializationBinder UpgradeBinder { get; set; }
        public bool RecordingInput { get { return (ActiveDebugReplay == null && inputWriter != null); } }
        private BinaryWriter inputWriter;
        public List<FrameInput> ActiveDebugReplay;
        public int DebugReplayIndex;

        public bool DevMode;
        public IRootEditor DevEditor;

        public bool GamePadActive { get; private set; }

        private string gamepadMap;
        public Dictionary<string, Dictionary<Buttons, string>> ButtonToLabel { get; private set; }

        public Settings CurSettings;

        private string loadMessage;
        public string LoadMsg
        {
            get
            {
                lock (lockObj)
                    return loadMessage;
            }
            set
            {
                lock (lockObj)
                    loadMessage = value;
            }
        }


        public DiagManager()
        {
            if (!Directory.Exists(LOG_PATH))
                Directory.CreateDirectory(LOG_PATH);
            if (!Directory.Exists(PathMod.MODS_PATH))
                Directory.CreateDirectory(PathMod.MODS_PATH);
            Settings.InitStatic();
            CurSettings = new Settings();
            gamepadMap = "";
            ButtonToLabel = new Dictionary<string, Dictionary<Buttons, string>>();
            FNALoggerEXT.LogInfo = LogInfo;
            FNALoggerEXT.LogWarn = LogInfo;
            FNALoggerEXT.LogError = LogInfo;
        }

        public void SetErrorListener(LogAdded errorAdded, ErrorTrace errorTrace)
        {
            errorAddedEvent = errorAdded;
            errorTraceEvent = errorTrace;
        }

        public void Unload()
        {
            EndInput();
        }


        public void BeginInput()
        {
            try
            {
                inputWriter = new BinaryWriter(new FileStream(LOG_PATH + String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now), FileMode.Create, FileAccess.Write, FileShare.None));

                inputWriter.Write(RogueElements.MathUtils.Rand.FirstSeed);

                inputWriter.Flush();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public void LogInput(FrameInput input)
        {
            if (inputWriter != null)
            {
                try
                {
                    inputWriter.Write((byte)((int)input.Direction));
                    for (int ii = 0; ii < (int)FrameInput.InputType.Ctrl; ii++)
                        inputWriter.Write(input[(FrameInput.InputType)ii]);
                    //for (int ii = 0; ii < FrameInput.TOTAL_CHARS; ii++)
                    //    inputWriter.Write(input.CharInput[ii]);

                    inputWriter.Flush();
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
        }


        public void EndInput()
        {
            try
            {
                if (inputWriter != null)
                {
                    inputWriter.Close();
                    inputWriter = null;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public void LoadInputs(string path)
        {
            try
            {
                using (FileStream stream = File.OpenRead(LOG_PATH + path))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        //seed
                        RogueElements.MathUtils.ReSeedRand(reader.ReadUInt64());

                        //all inputs
                        ActiveDebugReplay = new List<FrameInput>();

                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            FrameInput input = FrameInput.Load(reader);
                            ActiveDebugReplay.Add(input);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                ActiveDebugReplay = null;
            }
        }


        public void LogError(Exception exception)
        {
            LogError(exception, true);
        }

        /// <summary>
        /// Logs an error to console and output log.  Puts out the entire stack trace including inner exceptions.
        /// </summary>
        /// <param name="exception">THe exception to log.</param>
        /// <param name="signal">Triggers On-Error code if true.  Logs silently if not.</param>
        public void LogError(Exception exception, bool signal)
        {
            lock (lockObj)
            {
                if (inError)
                    throw new InvalidOperationException("Attempted to log an error when logging an error.", exception);
                inError = true;
                try
                {
                    StringBuilder errorMsg = new StringBuilder();
                    errorMsg.Append(String.Format("[{0}] {1}", String.Format("{0:yyyy/MM/dd HH:mm:ss.FFF}", DateTime.Now), exception.Message));
                    errorMsg.Append("\n");
                    Exception innerException = exception;
                    int depth = 0;
                    while (innerException != null)
                    {
                        errorMsg.Append("Exception Depth: " + depth);
                        errorMsg.Append("\n");

                        errorMsg.Append(innerException.ToString());
                        errorMsg.Append("\n\n");

                        innerException = innerException.InnerException;
                        depth++;
                    }

                    if (errorTraceEvent != null)
                        errorMsg.Append(errorTraceEvent());

                    Console.WriteLine(errorMsg);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(errorMsg);
#endif

                    if (errorAddedEvent != null && signal)
                        errorAddedEvent(exception.Message);


                    string filePath = LOG_PATH + String.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".txt";

                    using (StreamWriter writer = new StreamWriter(filePath, true))
                        writer.Write(errorMsg);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
                inError = false;
            }
        }

        public void LogInfo(string diagInfo)
        {
            lock (lockObj)
            {
                string fullMsg = String.Format("[{0}] {1}", String.Format("{0:yyyy/MM/dd HH:mm:ss.FFF}", DateTime.Now), diagInfo);
                if (DevMode)
                {
                    Console.WriteLine(fullMsg);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(fullMsg);
#endif
                }

                try
                {
                    string filePath = LOG_PATH + String.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".txt";

                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine(fullMsg);
                        writer.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }
        }

        public void SetupGamepad()
        {
            ButtonToLabel = new Dictionary<string, Dictionary<Buttons, string>>();
            //try to load from file
            string[] filePaths = Directory.GetFiles(CONTROLS_PATH, "*.xml");
            foreach (string filePath in filePaths)
            {
                Dictionary<Buttons, string> mapping = new Dictionary<Buttons, string>();

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(filePath);

                foreach (XmlNode button in xmldoc.SelectNodes("root/Button"))
                {
                    Buttons btn = Enum.Parse<Buttons>(button.Attributes["name"].Value);
                    mapping[btn] = button.InnerText;
                }

                string fileName = Path.GetFileNameWithoutExtension(filePath);
                ButtonToLabel[fileName] = mapping;
            }
        }

        public Settings LoadSettings()
        {
            //try to load from file

            Settings settings = new Settings();

            string path = PathMod.FromExe("Config.xml");
            if (File.Exists(path))
            {
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    settings.BGMBalance = Int32.Parse(xmldoc.SelectSingleNode("Config/BGM").InnerText);
                    settings.SEBalance = Int32.Parse(xmldoc.SelectSingleNode("Config/SE").InnerText);

                    settings.BattleFlow = Enum.Parse<Settings.BattleSpeed>(xmldoc.SelectSingleNode("Config/BattleFlow").InnerText);

                    settings.Window = Int32.Parse(xmldoc.SelectSingleNode("Config/Window").InnerText);
                    settings.Border = Int32.Parse(xmldoc.SelectSingleNode("Config/Border").InnerText);
                    settings.Language = xmldoc.SelectSingleNode("Config/Language").InnerText;

                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }

            path = PathMod.FromExe("Keyboard.xml");
            if (File.Exists(path))
            {
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    int index = 0;
                    XmlNode keys = xmldoc.SelectSingleNode("Config/DirKeys");
                    foreach (XmlNode key in keys.SelectNodes("DirKey"))
                    {
                        settings.DirKeys[index] = Enum.Parse<Keys>(key.InnerText);
                        index++;
                    }

                    index = 0;
                    keys = xmldoc.SelectSingleNode("Config/ActionKeys");
                    foreach (XmlNode key in keys.SelectNodes("ActionKey"))
                    {
                        while (!Settings.UsedByKeyboard((FrameInput.InputType)index) && index < settings.ActionKeys.Length)
                            index++;
                        settings.ActionKeys[index] = Enum.Parse<Keys>(key.InnerText);
                        index++;
                    }

                    settings.Enter = Boolean.Parse(xmldoc.SelectSingleNode("Config/Enter").InnerText);
                    settings.NumPad = Boolean.Parse(xmldoc.SelectSingleNode("Config/NumPad").InnerText);
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }

            path = PathMod.FromExe("Gamepad.xml");
            if (File.Exists(path))
            {
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    int index = 0;
                    XmlNode keys = xmldoc.SelectSingleNode("Config/ActionButtons");
                    foreach (XmlNode key in keys.SelectNodes("ActionButton"))
                    {
                        while (!Settings.UsedByGamepad((FrameInput.InputType)index) && index < settings.ActionButtons.Length)
                            index++;
                        settings.ActionButtons[index] = Enum.Parse<Buttons>(key.InnerText);
                        index++;
                    }
                    settings.InactiveInput = Boolean.Parse(xmldoc.SelectSingleNode("Config/InactiveInput").InnerText);

                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }

            path = PathMod.FromExe("Contacts.xml");
            if (File.Exists(path))
            {
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    XmlNode contacts = xmldoc.SelectSingleNode("Config/Servers");
                    foreach (XmlNode contact in contacts.SelectNodes("Server"))
                    {
                        ServerInfo info = new ServerInfo();
                        info.ServerName = contact.SelectSingleNode("Name").InnerText;
                        info.IP = contact.SelectSingleNode("IP").InnerText;
                        info.Port = Int32.Parse(contact.SelectSingleNode("Port").InnerText);
                        settings.ServerList.Add(info);
                    }

                    contacts = xmldoc.SelectSingleNode("Config/Contacts");
                    foreach (XmlNode contact in contacts.SelectNodes("Contact"))
                    {
                        ContactInfo info = new ContactInfo();
                        info.UUID = contact.SelectSingleNode("UUID").InnerText;
                        info.LastContact = contact.SelectSingleNode("LastSeen").InnerText;
                        loadContactNode(contact, info);
                        settings.ContactList.Add(info);
                    }

                    contacts = xmldoc.SelectSingleNode("Config/Peers");
                    foreach (XmlNode contact in contacts.SelectNodes("Peer"))
                    {
                        PeerInfo info = new PeerInfo();
                        info.UUID = contact.SelectSingleNode("UUID").InnerText;
                        info.LastContact = contact.SelectSingleNode("LastSeen").InnerText;
                        loadContactNode(contact, info);
                        info.IP = contact.SelectSingleNode("IP").InnerText;
                        info.Port = Int32.Parse(contact.SelectSingleNode("Port").InnerText);
                        settings.PeerList.Add(info);
                    }

                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
            return settings;
        }

        public void SaveSettings(Settings settings)
        {
            {
                XmlDocument xmldoc = new XmlDocument();

                XmlNode docNode = xmldoc.CreateElement("Config");
                xmldoc.AppendChild(docNode);

                appendConfigNode(xmldoc, docNode, "BGM", settings.BGMBalance.ToString());
                appendConfigNode(xmldoc, docNode, "SE", settings.SEBalance.ToString());

                appendConfigNode(xmldoc, docNode, "BattleFlow", settings.BattleFlow.ToString());

                appendConfigNode(xmldoc, docNode, "Window", settings.Window.ToString());
                appendConfigNode(xmldoc, docNode, "Border", settings.Border.ToString());
                appendConfigNode(xmldoc, docNode, "Language", settings.Language.ToString());

                xmldoc.Save(PathMod.FromExe("Config.xml"));
            }

            {
                XmlDocument xmldoc = new XmlDocument();

                XmlNode docNode = xmldoc.CreateElement("Config");
                xmldoc.AppendChild(docNode);

                XmlNode dirKeys = xmldoc.CreateElement("DirKeys");
                foreach (Keys key in settings.DirKeys)
                    appendConfigNode(xmldoc, dirKeys, "DirKey", key.ToString());
                docNode.AppendChild(dirKeys);

                XmlNode actionKeys = xmldoc.CreateElement("ActionKeys");
                for (int ii = 0; ii < settings.ActionKeys.Length; ii++)
                {
                    if (!Settings.UsedByKeyboard((FrameInput.InputType)ii))
                        continue;
                    Keys key = settings.ActionKeys[ii];
                    appendConfigNode(xmldoc, actionKeys, "ActionKey", key.ToString());
                }
                docNode.AppendChild(actionKeys);
                appendConfigNode(xmldoc, docNode, "Enter", settings.Enter.ToString());
                appendConfigNode(xmldoc, docNode, "NumPad", settings.NumPad.ToString());

                xmldoc.Save(PathMod.FromExe("Keyboard.xml"));
            }

            {
                XmlDocument xmldoc = new XmlDocument();

                XmlNode docNode = xmldoc.CreateElement("Config");
                xmldoc.AppendChild(docNode);

                XmlNode actionButtons = xmldoc.CreateElement("ActionButtons");
                for (int ii = 0; ii < settings.ActionButtons.Length; ii++)
                {
                    if (!Settings.UsedByGamepad((FrameInput.InputType)ii))
                        continue;
                    Buttons button = settings.ActionButtons[ii];
                    appendConfigNode(xmldoc, actionButtons, "ActionButton", button.ToString());
                }
                docNode.AppendChild(actionButtons);
                appendConfigNode(xmldoc, docNode, "InactiveInput", settings.InactiveInput.ToString());

                xmldoc.Save(PathMod.FromExe("Gamepad.xml"));
            }

            {
                XmlDocument xmldoc = new XmlDocument();

                XmlNode docNode = xmldoc.CreateElement("Config");
                xmldoc.AppendChild(docNode);

                XmlNode servers = xmldoc.CreateElement("Servers");
                foreach (ServerInfo contact in settings.ServerList)
                {
                    XmlNode node = xmldoc.CreateElement("Server");
                    appendConfigNode(xmldoc, node, "Name", contact.ServerName);
                    appendConfigNode(xmldoc, node, "IP", contact.IP);
                    appendConfigNode(xmldoc, node, "Port", contact.Port.ToString());
                    servers.AppendChild(node);
                }
                docNode.AppendChild(servers);

                XmlNode contacts = xmldoc.CreateElement("Contacts");
                foreach (ContactInfo contact in settings.ContactList)
                {
                    XmlNode node = xmldoc.CreateElement("Contact");
                    appendConfigNode(xmldoc, node, "UUID", contact.UUID);
                    appendConfigNode(xmldoc, node, "LastSeen", contact.LastContact);
                    appendContactNode(xmldoc, node, contact);
                    contacts.AppendChild(node);
                }
                docNode.AppendChild(contacts);

                XmlNode peers = xmldoc.CreateElement("Peers");
                foreach (PeerInfo contact in settings.PeerList)
                {
                    XmlNode node = xmldoc.CreateElement("Peer");
                    appendConfigNode(xmldoc, node, "UUID", contact.UUID);
                    appendConfigNode(xmldoc, node, "LastSeen", contact.LastContact);
                    appendContactNode(xmldoc, node, contact);
                    appendConfigNode(xmldoc, node, "IP", contact.IP);
                    appendConfigNode(xmldoc, node, "Port", contact.Port.ToString());
                    peers.AppendChild(node);
                }
                docNode.AppendChild(peers);


                xmldoc.Save(PathMod.FromExe("Contacts.xml"));
            }
        }

        public void UpdateGamePadActive(bool active)
        {
            if (!GamePadActive && active)
            {
                string guid = GamePad.GetGUIDEXT(PlayerIndex.One);

                //if (guid.Equals("4c05c405") || guid.Equals("4c05cc09"))//PS4
                //else if (guid.Equals("4c05e60c"))//PS5
                //else if (guid.Equals("7e050920") || guid.Equals("7e053003"))//Nintendo
                //else if (guid.Equals("5e04ff02"))//Xbox
                if (ButtonToLabel.ContainsKey(guid))
                    gamepadMap = guid;
                else
                    gamepadMap = "default";

            }
            else if (GamePadActive && !active)
                gamepadMap = "default";
            GamePadActive = active;
        }

        public string GetControlString(FrameInput.InputType inputType)
        {
            if (GamePadActive)
                return GetButtonString(CurSettings.ActionButtons[(int)inputType]);
            return GetKeyboardString(CurSettings.ActionKeys[(int)inputType]);
        }

        public string GetButtonString(Buttons button)
        {
            Dictionary<Buttons, string> dict;
            if (ButtonToLabel.TryGetValue(gamepadMap, out dict))
            {
                string mappedString;
                if (dict.TryGetValue(button, out mappedString))
                    return Regex.Unescape(mappedString);
            }

            return "(" + button.ToLocal() + ")";
        }

        public string GetKeyboardString(Keys key)
        {
            return "[" + key.ToLocal() + "]";
        }


        private static void loadContactNode(XmlNode contact, ContactInfo info)
        {
            string hex = contact.SelectSingleNode("Data").InnerText;
            
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            info.DeserializeData(bytes);
        }

        private static void appendContactNode(XmlDocument xmldoc, XmlNode node, ContactInfo contact)
        {
            byte[] ba = contact.SerializeData();
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            hex.ToString();
            appendConfigNode(xmldoc, node, "Data", hex.ToString());
        }

        private static void appendConfigNode(XmlDocument doc, XmlNode parentNode, string name, string text)
        {
            XmlNode node = doc.CreateElement(name);
            node.InnerText = text;
            parentNode.AppendChild(node);
        }

    }
}
