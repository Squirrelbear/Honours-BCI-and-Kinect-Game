using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HonoursGame
{
    public class Calibrator
    {
        public enum CalibrateMode { CalibrateMin = 0, CalibrateMax = 1, UnCalibrated, CalibrateComplete };

        private float[,] alphaHistory;
        private float min, max;
        private bool minFound, maxFound;

        private CalibrateMode mode;
        private int historyIndex;
        private int frameSize;

        public Calibrator(int frameSize)
        {
            this.frameSize = frameSize;
            alphaHistory = new float[2,frameSize];
            historyIndex = 0;
            minFound = maxFound = false;
            mode = CalibrateMode.UnCalibrated;
        }

        public void beginCalibration(CalibrateMode mode)
        {
            this.mode = mode;
            historyIndex = 0;

            for (int i = 0; i < frameSize; i++)
                alphaHistory[(int)mode, i] = 0;
        }

        public void pushValue(float value)
        {
            if (mode == CalibrateMode.CalibrateComplete || mode == CalibrateMode.UnCalibrated) return;

            alphaHistory[(int)mode, historyIndex++] = value;

            if (historyIndex == frameSize)
            {
                // calibration complete
                if (mode == CalibrateMode.CalibrateMin)
                {
                    min = solveMin();
                    minFound = true;
                }
                else
                {
                    max = solveMax();
                    maxFound = true;
                }
                mode = CalibrateMode.CalibrateComplete;
            }
        }

        #region Retrieval Methods
        public CalibrateMode getMode()
        {
            return mode;
        }

        public float getMin()
        {
            return min;
        }

        public float getMax()
        {
            return max;
        }

        public bool foundMin()
        {
            return minFound;
        }

        public bool foundMax()
        {
            return maxFound;
        }

        public bool isCalibrated()
        {
            return foundMin() && foundMax();
        }
        #endregion

        public float applyCalibratedScale(float value)
        {
            return (value - min) / (max - min); 
        }

        private float solveMin()
        {
            sortArray(mode);
            return getQuartile(0.25f, mode);
        }

        private float solveMax()
        {
            sortArray(mode);
            return getQuartile(0.75f, mode);
        }

        public void clearCalibration()
        {
            minFound = false;
            maxFound = false;
        }

        private float getQuartile(float quartile, CalibrateMode mode)
        {
            double result;

            // Get roughly the index
            double index = quartile * (frameSize + 1);

            // Get the remainder of that index value if exists
            double remainder = index % 1;

            // Get the integer value of that index
            index = Math.Floor(index) - 1;

            if (remainder.Equals(0))
            {
                // we have an integer value, no interpolation needed
                result = alphaHistory[(int)mode, (int)index];
            }
            else
            {
                // we need to interpolate
                double value = alphaHistory[(int)mode, (int)index];
                double interpolationValue = (alphaHistory[(int)mode, (int)index + 1] - alphaHistory[(int)mode, (int)index]) * remainder;

                result = value + interpolationValue;
            }

            return (float)result;
        }

        private void sortArray(CalibrateMode mode)
        {
            int min = 0;
            for (int i = 0; i < frameSize-1; i++)
            {
                min = i;
                for (int j = 0; j < frameSize; j++)
                {
                    if (alphaHistory[(int)mode, j] < alphaHistory[(int)mode, i])
                    {
                        min = j;
                    }
                }

                float temp = alphaHistory[(int)mode, i];
                alphaHistory[(int)mode, i] = alphaHistory[(int)mode, min];
                alphaHistory[(int)mode, min] = temp;
            }
        }
    }
}
