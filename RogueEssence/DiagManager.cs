using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Diagnostics;
using RogueEssence.Dev;

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

        public const string CONTENT_PATH = ASSET_PATH + "Content/";
        public const string LOG_PATH = "LOG/";


        public const string REG_PATH = "HKEY_CURRENT_USER\\Software\\RogueEssence";
#if !DEBUG && !PROFILING
        public const string ASSET_PATH = "";
        public const string DEV_PATH = "DevContent/";
        public const string TEMP_PATH = "temp/";
#else
        public const string ASSET_PATH = "../../../../Asset/";
        public const string DEV_PATH = "../../../../RawAsset/";
        public const string TEMP_PATH = "../../../../temp/";
#endif


        object lockObj = new object();

        public delegate void LogAdded(string message);

        private LogAdded errorAddedEvent;

        public bool RecordingInput { get { return (ActiveDebugReplay == null && inputWriter != null); } }
        private BinaryWriter inputWriter;
        public List<FrameInput> ActiveDebugReplay;
        public int DebugReplayIndex;

        public bool DevMode;
        public IRootEditor DevEditor;

        public bool GamePadActive;
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
            Settings.InitStatic();

            CurSettings = LoadSettings();


        }

        public void SetErrorListener(LogAdded errorAdded)
        {
            errorAddedEvent = errorAdded;
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


        public void LogError(Exception exception, bool signal = true)
        {

            lock (lockObj)
            {
                if (errorAddedEvent != null && signal)
                    errorAddedEvent(exception.Message);

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


                Debug.Write(errorMsg);

                try
                {

                    string filePath = LOG_PATH + String.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".txt";

                    using (StreamWriter writer = new StreamWriter(filePath, true))
                        writer.Write(errorMsg);
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.ToString());
                }
            }
        }

        public void LogInfo(string diagInfo)
        {
            lock (lockObj)
            {
                string fullMsg = String.Format("[{0}] {1}", String.Format("{0:yyyy/MM/dd HH:mm:ss.FFF}", DateTime.Now), diagInfo);
                if (DevMode)
                    Debug.WriteLine(fullMsg);

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
                    Debug.Write(ex.ToString());
                }
            }
        }

        public Settings LoadSettings()
        {
            //try to load from file

            try
            {
                Settings settings = new Settings();

                string path = "Config.xml";
                if (File.Exists(path))
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    settings.BGMBalance = Int32.Parse(xmldoc.SelectSingleNode("Config/BGM").InnerText);
                    settings.SEBalance = Int32.Parse(xmldoc.SelectSingleNode("Config/SE").InnerText);

                    settings.BattleFlow = (Settings.BattleSpeed)Enum.Parse(typeof(Settings.BattleSpeed), xmldoc.SelectSingleNode("Config/BattleFlow").InnerText);

                    settings.Window = Int32.Parse(xmldoc.SelectSingleNode("Config/Window").InnerText);
                    settings.Border = Int32.Parse(xmldoc.SelectSingleNode("Config/Border").InnerText);
                    settings.Language = xmldoc.SelectSingleNode("Config/Language").InnerText;

                }

                path = "Keyboard.xml";
                if (File.Exists(path))
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    int index = 0;
                    XmlNode keys = xmldoc.SelectSingleNode("Config/DirKeys");
                    foreach (XmlNode key in keys.SelectNodes("DirKey"))
                    {
                        settings.DirKeys[index] = (Keys)Enum.Parse(typeof(Keys), key.InnerText);
                        index++;
                    }

                    index = 0;
                    keys = xmldoc.SelectSingleNode("Config/ActionKeys");
                    foreach (XmlNode key in keys.SelectNodes("ActionKey"))
                    {
                        while (!Settings.UsedByKeyboard((FrameInput.InputType)index) && index < settings.ActionKeys.Length)
                            index++;
                        settings.ActionKeys[index] = (Keys)Enum.Parse(typeof(Keys), key.InnerText);
                        index++;
                    }
                }

                path = "Gamepad.xml";
                if (File.Exists(path))
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    int index = 0;
                    XmlNode keys = xmldoc.SelectSingleNode("Config/ActionButtons");
                    foreach (XmlNode key in keys.SelectNodes("ActionButton"))
                    {
                        while (!Settings.UsedByGamepad((FrameInput.InputType)index) && index < settings.ActionButtons.Length)
                            index++;
                        settings.ActionButtons[index] = (Buttons)Enum.Parse(typeof(Buttons), key.InnerText);
                        index++;
                    }
                }

                path = "Contacts.xml";
                if (File.Exists(path))
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
                return settings;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            return new Settings();
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

                xmldoc.Save("Config.xml");
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

                xmldoc.Save("Keyboard.xml");
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

                xmldoc.Save("Gamepad.xml");
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


                xmldoc.Save("Contacts.xml");
            }
        }


        public string GetControlString(FrameInput.InputType inputType)
        {
            if (GamePadActive)
                return "(" + CurSettings.ActionButtons[(int)inputType].ToLocal() + ")";
            return "[" + CurSettings.ActionKeys[(int)inputType].ToLocal() + "]";
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
