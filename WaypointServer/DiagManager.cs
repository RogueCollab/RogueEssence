using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Diagnostics;

namespace WaypointServer
{
    public class DiagManager
    {
        private static DiagManager instance;
        public static void InitInstance()
        {
            instance = new DiagManager();
        }
        public static DiagManager Instance { get { return instance; } }

        public const string LOG_PATH = "Log/";

        public string ServerName;
        public int Port;
        public int Errors;

        public DiagManager()
        {
            if (!Directory.Exists(LOG_PATH))
                Directory.CreateDirectory(LOG_PATH);

            LoadSettings();
            SaveSettings();
        }


        public void LogError(Exception exception)
        {
            Errors++;

            StringBuilder errorMsg = new StringBuilder();
            errorMsg.Append(String.Format("[{0}] {1}", String.Format("{0:yyyy/MM/dd HH:mm:ss.fff}", DateTime.Now), exception.Message));
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

        public void LogInfo(string diagInfo)
        {
            string fullMsg = String.Format("[{0}] {1}", String.Format("{0:yyyy/MM/dd HH:mm:ss.fff}", DateTime.Now), diagInfo);

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

        public void LoadSettings()
        {
            string path = "Config.xml";

            ServerName = "Default Server";
            Port = 1705;

            //try to load from file
            if (File.Exists(path))
            {
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    ServerName = xmldoc.SelectSingleNode("Config/ServerName").InnerText;
                    Port = Int32.Parse(xmldoc.SelectSingleNode("Config/Port").InnerText);

                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
        }

        public void SaveSettings()
        {
            string path = "Config.xml";
            XmlDocument xmldoc = new XmlDocument();

            XmlNode docNode = xmldoc.CreateElement("Config");
            xmldoc.AppendChild(docNode);

            appendConfigNode(xmldoc, docNode, "ServerName", ServerName);
            appendConfigNode(xmldoc, docNode, "Port", Port.ToString());

            xmldoc.Save(path);
        }

        private static void appendConfigNode(XmlDocument doc, XmlNode parentNode, string name, string text)
        {
            XmlNode node = doc.CreateElement(name);
            node.InnerText = text;
            parentNode.AppendChild(node);
        }

    }
}
