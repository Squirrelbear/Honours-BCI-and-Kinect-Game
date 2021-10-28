using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HonoursGame
{
    [Serializable]
    public class DataElement
    {
        public enum DataType {
            Misc = 0, PuzzleData = 1, SolutionData = 2,
            EventMisc = 10, EventPuzzleStart = 11, EventPuzzleEnd = 12,
            VoiceMisc = 21, VoiceHypothesized = 22, VoiceRecognzed = 23, VoiceRejected = 24,
            InputMisc = 31, HandCoords = 32, HipCoords = 33,
            BCI = 41
        };

        public string gameTime;
        public DataType typeID;
        public string data;

        public DataElement()
        {
            this.gameTime = "0";
            this.data = "";
            this.typeID = DataType.Misc;
        }

        public DataElement(string gameTime, string data)
        {
            this.gameTime = gameTime;
            this.data = data;
            this.typeID = DataType.Misc;
        }

        public DataElement(string gameTime, string data, DataType dataType)
        {
            this.gameTime = gameTime;
            this.data = data;
            this.typeID = dataType;
        }

        public void setTime(string gameTime)
        {
            this.gameTime = gameTime;
        }

        public void setData(string data)
        {
            this.data = data;
        }

        public string getTime()
        {
            return gameTime;
        }

        public string getData()
        {
            return data;
        }
    }
}
