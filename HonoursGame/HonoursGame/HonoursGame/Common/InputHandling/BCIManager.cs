using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

/*
 * The first number in the frame is posterior alpha (mean of O1 and O2 @ 8-12 Hz).
    The second number in the frame is frontal theta (mean of F3 and F4 @ 4-7 Hz).

    The band order is:
    1.	Theta (4-7 Hz)
    2.	Alpha1 (8-10 Hz)
    3.	Alpha2 (10-12 Hz)
    4.	Alpha (8-12 Hz)
    5.	Beta1 (15-20 Hz)
    6.	Beta2 (20-25 Hz)
    7.	Beta (15-25 Hz)
    8.	Gamma (25-45 Hz)

    The channel order is:
    1.	AF3
    2.	AF4
    3.	F3
    4.	F4
    5.	F7
    6.	F8
    7.	FC5
    8.	FC6
    9.	T7
    10.	T8
    11.	P7
    12.	P8
    13.	O1
    14.	O2
    15.	GYROX
    16.	GYROY
 * 
 * Custom Alpha: 3,[12&13]
 */

namespace HonoursGame
{
    public class BCIManager
    {
        public enum Mode { Random, KeyboardInput, GamePadInput, Real };

        public static int STATECOUNT = 8;
        public static int SUBSTATECOUNT = 16;

        private Mode mode;
        private float[,] curInput;
        private float alpha, theta;
        private Random rand;
        private int nextUpdate;

        private bool smoothingEnabled;
        private float[,] oldInput;
        private float alphaold, thetaold;

        private InputManager inputManager;
        private BCINetworkMgr bciNetworkMgr;

        private Calibrator calibrator;
        private float customAlpha, customOldAlpha;
        private bool useCustom;

        public BCIManager(Mode mode, InputManager inputManager, string gamePath, string sessionID)
        {
            this.mode = mode;
            this.inputManager = inputManager;
            rand = new Random();

            curInput = new float[STATECOUNT, SUBSTATECOUNT];
            oldInput = new float[STATECOUNT, SUBSTATECOUNT];
            alpha = theta = 0;
            alphaold = thetaold = 0;
            nextUpdate = 1000;
            
            // NOTE: do not enable this... 
            smoothingEnabled = false;

            calibrator = new Calibrator(20);
            customAlpha = customOldAlpha = 0;
            useCustom = true;

            bciNetworkMgr = new BCINetworkMgr(this, gamePath, sessionID);
        }

        public void update(GameTime gameTime)
        {
            // NOTE: this will not actually work; MUST follow same delay system to work
            // backup the old state to be used for smoothing
            for (int i = 0; i < STATECOUNT; i++)
            {
                for (int j = 0; j < SUBSTATECOUNT; j++)
                {
                    oldInput[i,j] = curInput[i, j];
                }
            }
            alphaold = alpha;
            thetaold = theta;
            customOldAlpha = customAlpha;

            #region OtherInput
            if (mode == Mode.Random)
            {
                for (int i = 0; i < STATECOUNT; i++)
                    for(int j = 0; j < SUBSTATECOUNT; j++)
                        curInput[i,j] = randomBCI();
                alpha = randomBCI();
                theta = randomBCI();
            }
            else if (mode == Mode.GamePadInput)
            {
                for (int i = 0; i < STATECOUNT; i++)
                    for (int j = 0; j < SUBSTATECOUNT; j++)
                        curInput[i, j] = inputManager.getTriggerState(true);
                alpha = theta = inputManager.getTriggerState(true);
                if (!inputManager.isGamePadConnected() || inputManager.isBtnPressed(Buttons.Y))
                {
                    mode = Mode.KeyboardInput;
                    Console.WriteLine("Switching to Keyboard Input");
                }
            }
            else if (mode == Mode.KeyboardInput)
            {
                if (inputManager.isBtnPressed(Buttons.Y))
                {
                    mode = Mode.GamePadInput;
                    Console.WriteLine("Switching to Gamepad Input");
                }

                if (inputManager.isKeyPressed(Keys.OemPlus))
                {
                    for (int i = 0; i < STATECOUNT; i++)
                    {
                        for (int j = 0; j < SUBSTATECOUNT; j++)
                        {
                            curInput[i, j] += 0.1f;
                            if (curInput[i, j] > 1) curInput[i, j] = 1;
                        }
                    }

                    alpha += 0.1f;
                    theta += 0.1f;
                    if (alpha > 1) alpha = 1;
                    if (theta > 1) alpha = 1;
                }
                if (inputManager.isKeyPressed(Keys.OemMinus))
                {
                    for (int i = 0; i < STATECOUNT; i++)
                    {
                        for (int j = 0; j < SUBSTATECOUNT; j++)
                        {
                            curInput[i, j] -= 0.1f;
                            if (curInput[i, j] < 0) curInput[i, j] = 0;
                        }
                    }
                    alpha -= 0.1f;
                    theta -= 0.1f;
                    if (alpha < 0) alpha = 0;
                    if (theta < 0) alpha = 0;
                }
                if (inputManager.isKeyPressed(Keys.Home))
                {
                    for (int i = 0; i < STATECOUNT; i++)
                        for (int j = 0; j < SUBSTATECOUNT; j++)
                            curInput[i, j] = 0;
                    alpha = theta = 0;
                }
                if (inputManager.isKeyPressed(Keys.Insert))
                {
                    for (int i = 0; i < STATECOUNT; i++)
                        for (int j = 0; j < SUBSTATECOUNT; j++)
                            curInput[i, j] = -1;
                    alpha = theta = -1;
                }
                if (inputManager.isKeyPressed(Keys.End))
                {
                    for (int i = 0; i < STATECOUNT; i++)
                        for (int j = 0; j < SUBSTATECOUNT; j++)
                            curInput[i, j] = 1;
                    alpha = theta = 1;
                }
            }
            #endregion
            else
            {
                nextUpdate -= gameTime.ElapsedGameTime.Milliseconds;
                if (nextUpdate > 0) return;
                for (int i = 0; i < STATECOUNT; i++)
                    for (int j = 0; j < SUBSTATECOUNT; j++)
                        curInput[i, j] = bciNetworkMgr.getState(i, j);
                alpha = bciNetworkMgr.getAlpha1();
                theta = bciNetworkMgr.getTheta1();

                if (useCustom)
                {
                    customAlpha = (curInput[3, 12] + curInput[3, 13]) / 2;
                    if (calibrator.isCalibrated())
                    {
                        customAlpha = calibrator.applyCalibratedScale(customAlpha);

                        // NOTE: This will cull outlier values
                        if (customAlpha > 3.0f)
                            customAlpha = customOldAlpha;
                    }
                    else if (calibrator.getMode() != Calibrator.CalibrateMode.CalibrateComplete && calibrator.getMode() != Calibrator.CalibrateMode.UnCalibrated)
                    {
                        calibrator.pushValue(customAlpha);
                    }
                }
                nextUpdate += 1000;
            }
        }

        private float randomBCI()
        {
            return (float)(rand.NextDouble() * 2 - 1);
        }

        #region StateRetrieval
        public float getCurInput(int stateID, int subStateID)
        {
            if (smoothingEnabled)
                return oldInput[stateID, subStateID] + (curInput[stateID, subStateID] - oldInput[stateID, subStateID]) * (nextUpdate / 1000.0f);
            else
                return curInput[stateID, subStateID];
        }

        public float getAlphaState()
        {
            if (smoothingEnabled)
                return alphaold + (alpha - alphaold) * (nextUpdate / 1000.0f);
            else
                return alpha;
        }

        public float getThetaState()
        {
            if (smoothingEnabled)
                return thetaold + (theta - thetaold) * (nextUpdate / 1000.0f);
            else
                return theta;
        }

        public float getOldCurInput(int stateID, int subStateID)
        {
            return oldInput[stateID, subStateID];
        }

        public void setOldAlphaState(float oldalpha)
        {
            this.alphaold = oldalpha;
        }

        public float getOldAlphaState()
        {
            return alphaold;
        }

        public float getOldTheta()
        {
            return thetaold;
        }
        #endregion

        #region Custom Value Retrieval (and config)
        public bool getUseCustomAlpha()
        {
            return useCustom;
        }

        public void setUseCustomAlpha(bool useCustom)
        {
            this.useCustom = useCustom;
        }

        public float getCustomAlpha()
        {
            return customAlpha;
        }

        public Calibrator getCalibrator()
        {
            return calibrator;
        }

        public Calibrator.CalibrateMode getCalibratorMode()
        {
            return calibrator.getMode();
        }

        public void beginCalibration(Calibrator.CalibrateMode mode)
        {
            calibrator.beginCalibration(mode);
        }
        #endregion

        public float getScaledNextUpdate()
        {
            return ((1000 - nextUpdate) / 1000.0f);
        }

        public float applySmoothing(float targetValue, float oldValue)
        {
            return targetValue + (targetValue - oldValue) * getScaledNextUpdate();
        }

        public bool isServerConnected()
        {
            return bciNetworkMgr.isConnected();
        }

        public void setMode(Mode mode)
        {
            this.mode = mode;
        }

        public void UnLoadContent()
        {
            bciNetworkMgr.killThreads();
        }
    }
}
