using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HonoursGame
{
    public class InputManager
    {
        public enum HandInputMode { Hand_Both, Hand_Right, Hand_Left, Hand_None };
        public enum HandDevice { Kinect, Mouse, Gamepad };
        
        private KeyboardState oldKbState;
        private KeyboardState newKbState;
        private GamePadState oldGpState;
        private GamePadState newGpState;
        private MouseState mouseState;

        private BCIManager bciManager;

        // hands
        private Vector2 rHandPosition;
        private Vector2 lHandPosition;
        private HandInputMode handInputMode;
        private HandDevice handDevice;

        // hip
        private Vector2 hipCentrePos;
        private bool useHipCentre;

        // shoulder centre
        private Vector2 shoulderCentrePos;
        private bool useShoulderCentre;

        // This is a debug mode for when there is no kinect so that the mouse is automatically used instead
        private bool noKinect;

        private Rectangle rescaleRect;
        private Rectangle screenArea;

        public InputManager(string gamePath, string sessionID)
        {
            oldKbState = newKbState = Keyboard.GetState();
            oldGpState = newGpState = GamePad.GetState(PlayerIndex.One);
            mouseState = Mouse.GetState();
            bciManager = new BCIManager(BCIManager.Mode.Real, this, gamePath, sessionID);
            rHandPosition = new Vector2(0, 0);
            lHandPosition = new Vector2(0, 0);
            handInputMode = HandInputMode.Hand_Right;
            handDevice = HandDevice.Kinect;
            hipCentrePos = new Vector2(300, 300);
            useHipCentre = false;

            rescaleRect = screenArea = new Rectangle(-1,-1,0,0);
        }

        public void update(GameTime gameTime)
        {
            oldKbState = newKbState;
            newKbState = Keyboard.GetState();
            oldGpState = newGpState;
            newGpState = GamePad.GetState(PlayerIndex.One);

            bciManager.update(gameTime);

            if (handDevice == HandDevice.Mouse)
            {
                mouseState = Mouse.GetState();
                lHandPosition = new Vector2(mouseState.X, mouseState.Y);
                rHandPosition = new Vector2(mouseState.X, mouseState.Y);
                hipCentrePos = new Vector2(mouseState.X, 300);
            }
            else if (handDevice == HandDevice.Gamepad)
            {
                if (!newGpState.IsConnected)
                {
                    handDevice = HandDevice.Mouse;
                }
                lHandPosition.X += newGpState.ThumbSticks.Left.X * 200 * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                lHandPosition.Y -= newGpState.ThumbSticks.Left.Y * 200 * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                rHandPosition.X += newGpState.ThumbSticks.Left.X * 200 * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                rHandPosition.Y -= newGpState.ThumbSticks.Left.Y * 200 * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                hipCentrePos = new Vector2(hipCentrePos.X + newGpState.ThumbSticks.Right.X * 150 * gameTime.ElapsedGameTime.Milliseconds / 1000.0f, 300);
            }
        }

        #region KeyboardFunctions
        public bool isKeyPressed(Keys key)
        {
            if (oldKbState.IsKeyDown(key) && newKbState.IsKeyUp(key)) return true;
            return false;
        }

        public bool isKeyDown(Keys key)
        {
            return newKbState.IsKeyDown(key);
        }
        #endregion

        #region GamePadFunctions
        public bool isBtnPressed(Buttons btn)
        {
            if (oldGpState.IsButtonDown(btn) && newGpState.IsButtonUp(btn)) return true;
            return false;
        }

        public bool isBtnDown(Buttons btn)
        {
            return newGpState.IsButtonDown(btn);
        }

        public float getTriggerState(bool isRight)
        {
            if (isRight) return newGpState.Triggers.Right;
            else return newGpState.Triggers.Left;
        }

        public float getThumbStickStateRightX()
        {
            return newGpState.ThumbSticks.Right.X;
        }

        public float getThumbStickStateRightY()
        {
            return newGpState.ThumbSticks.Right.Y;
        }

        public float getThumbStickStateLeftX()
        {
            return newGpState.ThumbSticks.Left.X;
        }

        public float getThumbStickStateLeftY()
        {
            return newGpState.ThumbSticks.Left.Y;
        }

        public bool isGamePadConnected()
        {
            return GamePad.GetState(PlayerIndex.One).IsConnected;
        }

        #endregion

        #region KinectFunctions
        public void setHandPosition(bool isRight, Vector2 pos)
        {
            if (isRight)
            {
                rHandPosition.X = pos.X;
                rHandPosition.Y = pos.Y;
            }
            else
            {
                lHandPosition.X = pos.X;
                lHandPosition.Y = pos.Y;
            }
        }

        public Vector2 getHandPosition(bool isRight, bool rescale)
        {
            if (isRight)
            {
                return (rescale && handDevice == HandDevice.Kinect) ? rescalePoint(rHandPosition) : rHandPosition;
            }
            else
            {
                return (rescale && handDevice == HandDevice.Kinect) ? rescalePoint(lHandPosition) : lHandPosition;
            }
        }

        public void setHandInputMode(HandInputMode mode)
        {
            handInputMode = mode;
        }

        public HandInputMode getHandInputMode()
        {
            return handInputMode;
        }

        public void setHandDevice(HandDevice handDevice)
        {
            this.handDevice = handDevice;
        }

        public HandDevice getHandDevice()
        {
            return handDevice;
        }

        public void setUseHipCentre(bool useHipCentre)
        {
            this.useHipCentre = useHipCentre;
        }

        public bool getUseHipCentre()
        {
            return useHipCentre;
        }

        public float getHipCentre()
        {
            return hipCentrePos.X;
        }

        public void setHipCentre(Vector2 hipCentre)
        {
            this.hipCentrePos = hipCentre;
        }

        public void setUseShoulderCentre(bool useShoulderCentre)
        {
            this.useShoulderCentre = useShoulderCentre;
        }

        public bool getUseShoulderCentre()
        {
            return useShoulderCentre;
        }

        public Vector2 getShoulderCentre()
        {
            return shoulderCentrePos;
        }

        public void setShoulderCentre(Vector2 shoulderCentre)
        {
            this.shoulderCentrePos = shoulderCentre;
        }

        public void setNoKinect(bool noKinect)
        {
            this.noKinect = noKinect;
            if (noKinect)
            {
                if (newGpState.IsConnected)
                    handDevice = HandDevice.Gamepad;
                else
                    handDevice = HandDevice.Mouse;
            }
            else
            {
                handDevice = HandDevice.Kinect;
            }
        }

        public bool getNoKinect()
        {
            return noKinect;
        }
        #endregion

        #region BCIFunctions
        public bool getIsBCIServerConnected()
        {
            return bciManager.isServerConnected();
        }

        public Calibrator.CalibrateMode getCalibratorMode()
        {
            return bciManager.getCalibratorMode();
        }

        public void beginCalibration(Calibrator.CalibrateMode mode)
        {
            bciManager.beginCalibration(mode);
        }

        public void setUseCustomAlpha(bool useCustom)
        {
            bciManager.setUseCustomAlpha(useCustom);
        }

        public Calibrator getCalibrator()
        {
            return bciManager.getCalibrator();
        }

        public bool getUseCustomAlpha()
        {
            return bciManager.getUseCustomAlpha();
        }

        public float getAlphaState()
        {
            if (bciManager.getUseCustomAlpha() && getIsBCIServerConnected())
            {
                return bciManager.getCustomAlpha();
            }
            else
            {
                return bciManager.getAlphaState();
            }
            //return bciManager.getAlphaState();
            //return bciManager.getCurInput(0,0);
        }

        public float getThetaState()
        {
            return bciManager.getThetaState();
        }

        public float getAlphaStateByID(int stateID, int subStateID)
        {
            return bciManager.getCurInput(stateID, subStateID);
        }

        public float getOldAlphaState()
        {
            return bciManager.getOldAlphaState();
        }

        public void setOldAlphaState(float oldalpha)
        {
            bciManager.setOldAlphaState(oldalpha);
        }

        public float getOldThetaState()
        {
            return bciManager.getOldTheta();
        }

        public float getOldAlphaStateByID(int stateID, int subStateID)
        {
            return bciManager.getOldCurInput(stateID, subStateID);
        }

        public float getScaledNextUpdate()
        {
            return bciManager.getScaledNextUpdate();
        }

        public float applySmoothing(float targetValue, float oldValue)
        {
            return bciManager.applySmoothing(targetValue, oldValue);
        }
        #endregion

        #region HelperFunctions
        public float rescale(float value, float rangeMin, float rangeMax, float targetMin, float targetMax)
        {
            float scale = (rangeMax - rangeMin) / (targetMax - targetMin);
            return targetMin + (value - rangeMin) / scale;
        }

        /*public float rescaleWidth(float value, float rangeMin, float rangeMax, float targetMin, float targetMax)
        {
            return rescale(value, rangeMin, rangeMax, targetMin, targetMax);
            if (result > targetMax)
                result = targetMax;
            if (result < targetMin)
                result = targetMin;

            return result;
        }

        public float rescaleHeight(float value, float rangeMin, float rangeMax, float targetMin, float targetMax)
        {
            return rescale(value, rangeMin, rangeMax, targetMin, targetMax);
        }*/

        public void setRescaleRect(Rectangle rect, Rectangle screenArea)
        {
            this.rescaleRect = rect;
            this.screenArea = screenArea;
        }

        public Vector2 rescalePoint(Vector2 p)
        {
            // WARNING! this function assumes for simplicity that rescaleRect has been configured
            Vector2 result = new Vector2(p.X, p.Y);
            if (rescaleRect.X != -1)
            {
                result.X = rescale(p.X, rescaleRect.Left, rescaleRect.Right, screenArea.Left, screenArea.Right);
                result.Y = rescale(p.Y, rescaleRect.Top, rescaleRect.Bottom, screenArea.Top, screenArea.Bottom);
                //Console.WriteLine("Debug output: Unscaled: (" + p.X + ", " + p.Y + ") Scaled: (" + result.X + ", " + result.Y + ")");
            }
            return result;
        }
        #endregion

        public void UnloadContent()
        {
            bciManager.UnLoadContent();
        }
    }
}
