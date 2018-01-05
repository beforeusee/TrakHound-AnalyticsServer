using System;
using NLog;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace mod_db_mysql
{
    [XmlRoot("MySql")]
    public class Configuration
    {
        
        private static Logger logger=LogManager.GetCurrentClassLogger();

        [XmlElement("Server")]
        public string Server { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        [XmlElement("Password")]
        public string Password { get; set; }

        [XmlElement("Database")]
        public string Database { get; set; }

        public Configuration()
        {
            Server = "localhost";
        }

        public static Configuration Get(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));

                    using (FileStream fileStream = new FileStream(path, FileMode.Open))
                    {
                        using (XmlReader xmlReader = XmlReader.Create(fileStream))
                        {
                            return (Configuration)xmlSerializer.Deserialize(xmlReader);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Configuration.logger.Trace<Exception>(ex);
                }
            }
            return null;
        }

    }
}
