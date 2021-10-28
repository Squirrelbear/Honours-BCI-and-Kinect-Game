using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace HonoursGame
{
    [Serializable]
    public class DataLog
    {
        public enum DataType { Event, Brain, Input, Misc };

        public string sessionID, folderPath;
        public List<DataElement> events;
        public List<DataElement> brainStatus;
        public List<DataElement> inputs;
        public List<DataElement> miscData;

        private StreamWriter fileOut;
        private bool savingFileOut;

        public DataLog()
        {
            //throw new Exception("DO NOT USE THIS");
            this.sessionID = "";
            this.folderPath = "";
            events = new List<DataElement>();
            brainStatus = new List<DataElement>();
            inputs = new List<DataElement>();
            miscData = new List<DataElement>();
            savingFileOut = false;
        }

        public DataLog(string gamePath, string sessionID)
        {
            this.sessionID = sessionID;
            this.folderPath = gamePath + "\\" + sessionID;

            events = new List<DataElement>();
            brainStatus = new List<DataElement>();
            inputs = new List<DataElement>();
            miscData = new List<DataElement>();

            string fileOutName = folderPath + "\\textlog.txt";
            fileOut = new StreamWriter(fileOutName);
            savingFileOut = true;
        }

        public DataLog(string gamePath, string sessionID, string extraFileText)
        {
            this.sessionID = sessionID;
            this.folderPath = gamePath + "\\" + sessionID;

            events = new List<DataElement>();
            brainStatus = new List<DataElement>();
            inputs = new List<DataElement>();
            miscData = new List<DataElement>();

            string fileOutName = folderPath + "\\" + extraFileText + "textlog.txt";
            fileOut = new StreamWriter(fileOutName);
            savingFileOut = true;
            this.sessionID = extraFileText + sessionID;
        }

        public void insert(DataType type, DataElement.DataType subType, string time, string data)
        {
            if (type == DataType.Event)
            {
                events.Add(new DataElement(time, data, subType));
            }
            else if (type == DataType.Brain)
            {
                brainStatus.Add(new DataElement(time, data, subType));
            }
            else if (type == DataType.Input)
            {
                inputs.Add(new DataElement(time, data, subType));
            }
            else
            {
                miscData.Add(new DataElement(time, data, subType));
            }
            
            if(savingFileOut)
                fileOut.WriteLine(time + " " + type.ToString() + " " + subType.ToString() + " " + data);
        }

        public void insert(DataType type, DataElement.DataType subType, string data)
        {
            string time = DateTime.Now.ToString("yyyyMMddHHmmssffff");

            insert(type, subType, time, data);
        }

        public void save()
        {
            // based off of the code at: http://www.jonasjohn.de/snippets/csharp/xmlserializer-example.htm

            // Create a new XmlSerializer instance
            XmlSerializer SerializerObj = new XmlSerializer(typeof(DataLog));

            // Create a new file stream to write the serialized object to a file
            TextWriter WriteFileStream = new StreamWriter(folderPath + "\\" + sessionID + ".log");
            SerializerObj.Serialize(WriteFileStream, this);

            // Cleanup
            WriteFileStream.Close();

            if (savingFileOut)
            {
                savingFileOut = false;
                fileOut.Close();
            }
        }

        public static DataLog load(string sessionID, Game1 appRef)
        {
            // create the serializer
            XmlSerializer SerializerObj = new XmlSerializer(typeof(DataLog));

            // Create a new file stream for reading the XML file
            FileStream ReadFileStream = new FileStream(appRef.getGamePath() + "\\SessionInfo\\" + sessionID + ".log", FileMode.Open, FileAccess.Read, FileShare.Read);
            //Debug.Print("Reading level file: " + appRef.getGamePath() + "\\Saves\\" + levelFile);

            // Load the object saved above by using the Deserialize function
            DataLog LoadedObj = (DataLog)SerializerObj.Deserialize(ReadFileStream);
            
            // Cleanup
            ReadFileStream.Close();

            return LoadedObj;
        }
    }
}
