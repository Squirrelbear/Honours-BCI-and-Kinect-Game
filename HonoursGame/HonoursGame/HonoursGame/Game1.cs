using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

// Kinect and Audio imports
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Speech.Synthesis;
using System.IO;

// For getting application path
using System.Reflection;

/*
 * Source for XNA related code used in this startup demo
 * http://digitalerr0r.wordpress.com/2011/06/20/kinect-fundamentals-2-basic-programming/
 * http://forums.create.msdn.com/forums/p/85214/513410.aspx
 * http://digitalerr0r.wordpress.com/2011/12/13/kinect-fundamentals-4-implementing-skeletal-tracking/
 * 
 * Audio related source:
 * http://www.ximplosionx.com/2011/06/22/intro-to-the-kinect-sdkadding-speech-recognition/
 * */

namespace HonoursGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Instance variables
        //enum GameMode { MainMenu, TilePuzzle };

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private InputManager inputManager;
        private DataLog log;
        private Screenshot screenshot;

        private Texture2D kinectRGBVideo;
        private Texture2D backDepthTexture;
        private KinectSensor kinectSensor;
        private String connectedStatus;

        private KinectAudioSource kinectSource;
        private SpeechRecognitionEngine speechEngine;
        private Stream stream;
        private SpeechSynthesizer speechSynth;

        private SpriteFont font;
        private SpriteFont largeFont;

        //Vector2 handPosition = new Vector2();
        ///bool useRightHand = true;

        private const int WNDWIDTH = 1024;
        private const int WNDHEIGHT = 768;

        //GameMode gameMode;
        private WndHandle curWnd;

        private int timer;
        private int frames;
        private string fps;
        private string gamePath;

        // Required variables for convert depth frame
        // color divisors for tinting depth pixels
        private static readonly int[] IntensityShiftByPlayerR = { 1, 2, 0, 2, 0, 0, 2, 0 };
        private static readonly int[] IntensityShiftByPlayerG = { 1, 2, 2, 0, 2, 0, 0, 1 };
        private static readonly int[] IntensityShiftByPlayerB = { 1, 0, 2, 2, 0, 2, 0, 2 };

        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        private List<Texture2D> tileInfoTextures;
        private List<Texture2D> streetInfoTextures;
        private List<Texture2D> riverInfoTextures;

        private int reattemptCount;
        private string sessionid;

        // TODO: enable this before release version
        private bool logdata = true;
        private bool enableDebugInput = true;
        #endregion

        #region Initialization/Loading/Unloading
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = WNDHEIGHT;
            graphics.PreferredBackBufferWidth = WNDWIDTH;
            // TODO: enable this before release version
            graphics.ToggleFullScreen();
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // setup a string for the game path
            gamePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Game1)).CodeBase);
            gamePath = gamePath.Substring(6);

            sessionid = Guid.NewGuid().ToString();

            string newFolder = gamePath + "\\loggeddata\\" + sessionid;
            if (!System.IO.Directory.Exists(newFolder))
                System.IO.Directory.CreateDirectory(newFolder);

            inputManager = new InputManager(gamePath + "\\loggeddata", sessionid);
            Rectangle screenRect = new Rectangle(0,0,1024,768);
            float scaleFactor = 0.6f;
            Rectangle rescaleRect = new Rectangle((int)((screenRect.Width * (1-scaleFactor)) / 2), (int)((screenRect.Height * (1-scaleFactor)) / 2), 
                                                    (int)(screenRect.Width * scaleFactor), (int)(screenRect.Height * scaleFactor));
            inputManager.setRescaleRect(rescaleRect, screenRect);
            KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
            DiscoverKinectSensor();
            speechSynth = new SpeechSynthesizer();
            speechSynth.SetOutputToDefaultAudioDevice();
            speechSynth.Volume = 100;
            //PromptBuilder builder = new PromptBuilder(new System.Globalization.CultureInfo("en-US"));
            //builder.AppendText("Hello human.");
            //speechSynth.Speak("Hello Human");
            //speechSynth.SpeakAsync("testing 123");
            //speechSynth.SelectVoice("LH Michael");

            screenshot = new Screenshot(gamePath + "\\loggeddata", sessionid);
            log = new DataLog(gamePath + "\\loggeddata", sessionid);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //gameMode = GameMode.TilePuzzle;
            //curWnd = new TilePuzzle(WNDWIDTH, WNDHEIGHT, this);
            timer = 1000;
            frames = 0;
            fps = "0";

            font = this.Content.Load<SpriteFont>("smallFont");
            largeFont = this.Content.Load<SpriteFont>("hugeFont");

            tileInfoTextures = new List<Texture2D>();
            tileInfoTextures.Add(this.Content.Load<Texture2D>("InfoWnd\\tileinfo1"));
            tileInfoTextures.Add(this.Content.Load<Texture2D>("InfoWnd\\tileinfo2"));

            streetInfoTextures = new List<Texture2D>();
            streetInfoTextures.Add(this.Content.Load<Texture2D>("InfoWnd\\streetinfo1"));
            streetInfoTextures.Add(this.Content.Load<Texture2D>("InfoWnd\\streetinfo2"));

            riverInfoTextures = new List<Texture2D>();
            riverInfoTextures.Add(this.Content.Load<Texture2D>("InfoWnd\\riverinfo1"));
            riverInfoTextures.Add(this.Content.Load<Texture2D>("InfoWnd\\riverinfo2"));

            backDepthTexture = this.Content.Load<Texture2D>("MainMenu\\kinectbackdrop");

            setWnd(WndHandle.WndType.MainMenu);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // terminate server threads
            inputManager.UnloadContent();

            // safely unload the speech and kinect systems
            if(speechEngine != null)
                speechEngine.RecognizeAsyncStop();
            if (kinectSensor != null)
            {
                kinectSensor.Stop();
                kinectSensor.Dispose();
            }
            if (speechSynth != null)
                speechSynth.SpeakAsyncCancelAll();
        }
        #endregion

        #region Updating/Drawing
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            inputManager.update(gameTime);

            timer -= gameTime.ElapsedGameTime.Milliseconds;
            
            if (timer <= 0)
            {
                fps = "FPS: " + frames + " : Alpha State: " + inputManager.getAlphaState() + " SessionID: " + sessionid;
                timer = 1000;
                frames = 0;
            }

            // Allows the game to exit
            if (inputManager.isBtnPressed(Buttons.Back) || inputManager.isKeyPressed(Keys.Escape))
            {
                saveLog();
                this.Exit();
            }

            // Change full screen
            if (inputManager.isKeyPressed(Keys.F))
                graphics.ToggleFullScreen();

            // save any data (only use in case of serious unforseen error)
            /*if (inputManager.isKeyPressed(Keys.F10))
            {
                saveLog();
            }*/

            curWnd.update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //spriteBatch.Begin();//(SpriteSortMode.Deferred, BlendState.NonPremultiplied);//(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            /*if(kinectRGBVideo != null)
                spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, WNDWIDTH, WNDHEIGHT), Color.White);
            */

            /*if (kinectRGBVideo != null && curWnd.getWndType() == WndHandle.WndType.MainMenu)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            }
            else
            {*/
                spriteBatch.Begin();
            //}

            curWnd.draw(spriteBatch);

            spriteBatch.DrawString(font, connectedStatus, new Vector2(2, 20), Color.White);
            if (curWnd.getWndType() != WndHandle.WndType.InfoWnd)
            {
                spriteBatch.DrawString(font, fps, new Vector2(2, 10), Color.White);
                spriteBatch.DrawString(largeFont, "" + inputManager.getAlphaState(), new Vector2(2, 40), Color.Red);
            }
            spriteBatch.End();
            
            base.Draw(gameTime);

            frames++;

            //if (frames == 0)
            //    screenshot.TakeScreenshot(GraphicsDevice);
        }

        public void drawKinectDepthImage()
        {
            if (kinectRGBVideo != null)
            {
                spriteBatch.Draw(backDepthTexture, new Rectangle((int)((WNDWIDTH - 256) * 3.0f / 4), (WNDHEIGHT - 256) / 2, 256, 256), Color.Black);
                spriteBatch.Draw(kinectRGBVideo, new Rectangle((int)((WNDWIDTH - 256) * 3.0f / 4), (WNDHEIGHT - 256) / 2, 256, 256), Color.White);
            }
        }
        #endregion

        #region Helper Methods
        public InputManager getInputManager()
        {
            return inputManager;
        }

        public string getGamePath()
        {
            return gamePath;
        }

        public void setWnd(WndHandle.WndType type)
        {
            if (curWnd == null || curWnd.getWndType() != type)
            {
                reattemptCount = 3;
            }
            else
            {
                reattemptCount--;
            }

            switch (type)
            {
                case WndHandle.WndType.MainMenu:
                    curWnd = new MainMenu(WNDWIDTH, WNDHEIGHT, this);
                    break;
                case WndHandle.WndType.RiverPuzzle:
                    curWnd = new RiverPuzzle(WNDWIDTH, WNDHEIGHT, this);
                    break;
                case WndHandle.WndType.StreetPuzzle:
                    curWnd = new StreetPuzzle(reattemptCount, WNDWIDTH, WNDHEIGHT, this);
                    break;
                case WndHandle.WndType.TilePuzzle:
                    curWnd = new TilePuzzle(WNDWIDTH, WNDHEIGHT, this);
                    break;
                case WndHandle.WndType.Calibration:
                    curWnd = new CalibrationWnd(WNDWIDTH, WNDHEIGHT, this);
                    break;
                case WndHandle.WndType.Quit:
                    saveLog();
                    this.Exit();
                    break;
            }
        }

        public void setInfoWnd(WndHandle.WndType nextWnd)
        {
            switch (nextWnd)
            {
                case WndHandle.WndType.RiverPuzzle:
                    curWnd = new InfoWnd(nextWnd, riverInfoTextures, WNDWIDTH, WNDHEIGHT, this);
                    break;
                case WndHandle.WndType.StreetPuzzle:
                    curWnd = new InfoWnd(nextWnd, streetInfoTextures, WNDWIDTH, WNDHEIGHT, this);
                    break;
                case WndHandle.WndType.TilePuzzle:
                    curWnd = new InfoWnd(nextWnd, tileInfoTextures, WNDWIDTH, WNDHEIGHT, this);
                    break;
            }

            // ERROR THIS SHOULD NEVER BE REACHED
            //curWnd = new InfoWnd(nextWnd, null, WNDWIDTH, WNDHEIGHT, this);
        }

        public void insertLog(DataLog.DataType type, DataElement.DataType subType, string data)
        {
            if(logdata)
                log.insert(type, subType, data);
        }

        public void saveLog()
        {
            log.save();
        }

        public void loadLog(string sessionid)
        {
            log = DataLog.load(sessionid, this);
        }

        public bool getEnableDebugInput()
        {
            return enableDebugInput;
        }
        /* ** DEPRECATED **
        public void enableAdditiveBlend(bool yes)
        {
            if (yes)
            {
                GraphicsDevice.BlendState = BlendState.Additive;
            }
            else
            {
                GraphicsDevice.BlendState = BlendState.Opaque;
            }
        }
         * */
        #endregion

        #region KinectHelperMethods
        private void DiscoverKinectSensor()
        {
            foreach (KinectSensor sensor in KinectSensor.KinectSensors)
            {
                if (sensor.Status == KinectStatus.Connected)
                {
                    // Found one, set our sensor to this
                    kinectSensor = sensor;
                    break;
                }
            }

            if (this.kinectSensor == null)
            {
                connectedStatus = "Found none Kinect Sensors connected to USB";
                inputManager.setNoKinect(true);
                return;
            }

            // You can use the kinectSensor.Status to check for status
            // and give the user some kind of feedback
            switch (kinectSensor.Status)
            {
                case KinectStatus.Connected:
                    {
                        connectedStatus = "Status: Connected";
                        break;
                    }
                case KinectStatus.Disconnected:
                    {
                        connectedStatus = "Status: Disconnected";
                        break;
                    }
                case KinectStatus.NotPowered:
                    {
                        connectedStatus = "Status: Connect the power";
                        break;
                    }
                default:
                    {
                        connectedStatus = "Status: Error";
                        break;
                    }
            }

            // Init the found and connected device
            if (kinectSensor.Status == KinectStatus.Connected)
            {
                InitializeKinect();
            }
        }

        private bool InitializeKinect()
        {
            // Color stream
            //kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            //kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);

            // Depth stream
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
    	    kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(kinectSensor_DepthFrameReady);

            // Skeleton Stream
            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            });
            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);

            try
            {
                kinectSensor.Start();
            }
            catch
            {
                connectedStatus = "Unable to start the Kinect Sensor";
                return false;
            }

            // Audio source code
            // Obtain the KinectAudioSource to do audio capture
            kinectSource = kinectSensor.AudioSource;
            kinectSource.EchoCancellationMode = EchoCancellationMode.None; // No AEC for this sample
            kinectSource.AutomaticGainControlEnabled = false; // Important to turn this off for speech recognition

            RecognizerInfo ri = GetKinectRecognizer();

            if (ri == null)
            {
                Console.WriteLine("Could not find Kinect speech recognizer. Please refer to the sample requirements.");
                Exit();
            }

            Console.WriteLine("Using: {0}", ri.Name);

            // get the complete list of strings that can be used at any time in the game
            List<String> speechList = new List<String>();
            var choices = new Choices();
            foreach (string s in TilePuzzle.speechStrings)
            {
                if (!speechList.Contains(s))
                {
                    choices.Add(s);
                    speechList.Add(s);
                }
                //Console.WriteLine("String: " + s + " added to choices");
            }
            foreach (string s in MainMenu.speechStrings)
            {
                if (!speechList.Contains(s))
                {
                    choices.Add(s);
                    speechList.Add(s);
                }
                //Console.WriteLine("String: " + s + " added to choices");
            }
            foreach (string s in InfoWnd.speechStrings)
            {
                if (!speechList.Contains(s))
                {
                    choices.Add(s);
                    speechList.Add(s);
                }
                //Console.WriteLine("String: " + s + " added to choices");
            }
            foreach (string s in RiverPuzzle.speechStrings)
            {
                if (!speechList.Contains(s))
                {
                    choices.Add(s);
                    speechList.Add(s);
                }
                //Console.WriteLine("String: " + s + " added to choices");
            }
            foreach (string s in StreetPuzzle.speechStrings)
            {
                if (!speechList.Contains(s))
                {
                    choices.Add(s);
                    speechList.Add(s);
                }
                //Console.WriteLine("String: " + s + " added to choices");
            }

            /*choices.Add("select");
            choices.Add("hello");
            choices.Add("change");*/

            GrammarBuilder gb = new GrammarBuilder();
            gb.Culture = ri.Culture;
            gb.Append(choices);

            var g = new Grammar(gb);

            speechEngine = new SpeechRecognitionEngine(ri.Id);
            speechEngine.LoadGrammar(g);
            speechEngine.SpeechHypothesized += sre_SpeechHypothesized;
            speechEngine.SpeechRecognized += sre_SpeechRecognized;
            speechEngine.SpeechRecognitionRejected += sre_SpeechRecognitionRejected;

            stream = kinectSource.Start();
            speechEngine.SetInputToAudioStream(stream,
                new SpeechAudioFormatInfo(
                    EncodingFormat.Pcm, 16000, 16, 1,
                    32000, 2, null));

            speechEngine.RecognizeAsync(RecognizeMode.Multiple);

            return true;
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
        // that displays different players in different colors
        private byte[] ConvertDepthFrame(short[] depthFrame, DepthImageStream depthStream, int depthFrame32Length)
        {
            int tooNearDepth = depthStream.TooNearDepth;
            int tooFarDepth = depthStream.TooFarDepth;
            int unknownDepth = depthStream.UnknownDepth;
            byte[] depthFrame32 = new byte[depthFrame32Length];

            for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < depthFrame32.Length; i16++, i32 += 4)
            {
                int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
                int realDepth = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(~(realDepth >> 4));

                if (player == 0 && realDepth == 0)
                {
                    // white 
                    depthFrame32[i32 + RedIndex] = 255;
                    depthFrame32[i32 + GreenIndex] = 255;
                    depthFrame32[i32 + BlueIndex] = 255;
                }
                else if (player == 0 && realDepth == tooFarDepth)
                {
                    // dark purple
                    depthFrame32[i32 + RedIndex] = 66;
                    depthFrame32[i32 + GreenIndex] = 0;
                    depthFrame32[i32 + BlueIndex] = 66;
                }
                else if (player == 0 && realDepth == unknownDepth)
                {
                    // dark brown
                    depthFrame32[i32 + RedIndex] = 66;
                    depthFrame32[i32 + GreenIndex] = 66;
                    depthFrame32[i32 + BlueIndex] = 33;
                }
                else
                {
                    // tint the intensity by dividing by per-player values
                    depthFrame32[i32 + RedIndex] = (byte)(intensity >> IntensityShiftByPlayerR[player]);
                    depthFrame32[i32 + GreenIndex] = (byte)(intensity >> IntensityShiftByPlayerG[player]);
                    depthFrame32[i32 + BlueIndex] = (byte)(intensity >> IntensityShiftByPlayerB[player]);
                }
            }

            return depthFrame32;
        }

        #endregion

        #region KinectEventHandlers
        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (this.kinectSensor == e.Sensor)
            {
                if (e.Status == KinectStatus.Disconnected ||
                    e.Status == KinectStatus.NotPowered)
                {
                    this.kinectSensor = null;
                    this.DiscoverKinectSensor();
                }
            }
        }

        void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

                    skeletonFrame.CopySkeletonDataTo(skeletonData);
                    Skeleton playerSkeleton = (from s in skeletonData where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                    if (playerSkeleton != null)
                    {
                        //Joint rightHand = playerSkeleton.Joints[JointType.HandRight];
                        //Joint leftHand = playerSkeleton.Joints[JointType.HandRight];
                        if (inputManager.getHandInputMode() == InputManager.HandInputMode.Hand_Right ||
                            inputManager.getHandInputMode() == InputManager.HandInputMode.Hand_Both)
                        {
                            Joint useHand = playerSkeleton.Joints[JointType.HandRight];
                            Vector2 handPosition = new Vector2((((0.5f * useHand.Position.X) + 0.5f) * (WNDWIDTH)), (((-0.5f * useHand.Position.Y) + 0.5f) * (WNDHEIGHT)));
                            inputManager.setHandPosition(true, handPosition);
                        }

                        if (inputManager.getHandInputMode() == InputManager.HandInputMode.Hand_Left ||
                            inputManager.getHandInputMode() == InputManager.HandInputMode.Hand_Both)
                        {
                            Joint useHand = playerSkeleton.Joints[JointType.HandLeft];
                            Vector2 handPosition = new Vector2((((0.5f * useHand.Position.X) + 0.5f) * (WNDWIDTH)), (((-0.5f * useHand.Position.Y) + 0.5f) * (WNDHEIGHT)));
                            inputManager.setHandPosition(false, handPosition);
                        }

                        if (inputManager.getUseHipCentre())
                        {
                            Joint hipJoint = playerSkeleton.Joints[JointType.HipCenter];
                            Vector2 hipPos = new Vector2((((0.5f * hipJoint.Position.X) + 0.5f) * (WNDWIDTH)), WNDHEIGHT / 2);
                            inputManager.setHipCentre(hipPos);
                        }

                        if (inputManager.getUseShoulderCentre())
                        {
                            Joint shoulderJoint = playerSkeleton.Joints[JointType.ShoulderCenter];
                            Vector2 shoulderPos = new Vector2((((0.5f * shoulderJoint.Position.X) + 0.5f) * (WNDWIDTH)), (((-0.5f * shoulderJoint.Position.Y) + 0.5f) * (WNDHEIGHT)));
                            inputManager.setShoulderCentre(shoulderPos);
                        }
                        //inputManager.setHandPosition(false, hipPos);
                        //inputManager.setHandPosition(true, hipPos);
                        //Joint useHand;
                        //if (useRightHand)
                        //{
                        //    useHand = playerSkeleton.Joints[JointType.HandRight];
                        //}
                        //else
                        //{
                        //    useHand = playerSkeleton.Joints[JointType.HandLeft];
                        //}
                        //handPosition = new Vector2((((0.5f * useHand.Position.X) + 0.5f) * (WNDWIDTH)), (((-0.5f * useHand.Position.Y) + 0.5f) * (WNDHEIGHT)));
                    }
                }
            }
        }

        void kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame != null)
                {

                    byte[] pixelsFromFrame = new byte[colorImageFrame.PixelDataLength];

                    colorImageFrame.CopyPixelDataTo(pixelsFromFrame);

                    Color[] color = new Color[colorImageFrame.Height * colorImageFrame.Width];
                    kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, colorImageFrame.Width, colorImageFrame.Height);

                    // Go through each pixel and set the bytes correctly.
                    // Remember, each pixel got a Rad, Green and Blue channel.
                    int index = 0;
                    for (int y = 0; y < colorImageFrame.Height; y++)
                    {
                        for (int x = 0; x < colorImageFrame.Width; x++, index += 4)
                        {
                            color[y * colorImageFrame.Width + x] = new Color(pixelsFromFrame[index + 2], pixelsFromFrame[index + 1], pixelsFromFrame[index + 0]);
                        }
                    }

                    // Set pixeldata from the ColorImageFrame to a Texture2D
                    kinectRGBVideo.SetData(color);
                }
            }
        }

        // http://digitalerr0r.wordpress.com/2011/06/21/kinect-fundamentals-3-getting-data-from-the-depth-sensor/

        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame != null)
                {
                    short[] pixelsFromFrame = new short[depthImageFrame.PixelDataLength];

                    depthImageFrame.CopyPixelDataTo(pixelsFromFrame);
                    byte[] convertedPixels = ConvertDepthFrame(pixelsFromFrame, ((KinectSensor)sender).DepthStream, 640 * 480 * 4);

                    Color[] color = new Color[depthImageFrame.Height * depthImageFrame.Width];
                    kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, depthImageFrame.Width, depthImageFrame.Height);

                    // Set convertedPixels from the DepthImageFrame to a the datasource for our Texture2D
                    kinectRGBVideo.SetData<byte>(convertedPixels);
                }
            }
        }

        #endregion

        #region SpeechEventHandlers
        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= 0.7)
            {
                Console.WriteLine("\nSpeech Recognized: \t{0}\tConfidence:\t{1}", e.Result.Text, e.Result.Confidence);
                curWnd.handleSpeechRecognised(e.Result.Text);
                /*if (curWnd.getWndType() == WndHandle.WndType.TilePuzzle)
                {
                    ((TilePuzzle)curWnd).handleSpeechRecognised(e.Result.Text);
                }
                else if (curWnd.getWndType() == WndHandle.WndType.MainMenu)
                {
                    ((MainMenu)curWnd).handleSpeechRecognised(e.Result.Text);
                }
                else if (curWnd.getWndType() == WndHandle.WndType.InfoWnd)
                {
                    ((InfoWnd)curWnd).handleSpeechRecognised(e.Result.Text);
                }
                else if (curWnd.getWndType() == WndHandle.WndType.StreetPuzzle)
                {
                    ((StreetPuzzle)curWnd).handleSpeechRecognised(e.Result.Text);
                }
                else if (curWnd.getWndType() == WndHandle.WndType.RiverPuzzle)
                {
                    ((RiverPuzzle)curWnd).handleSpeechRecognised(e.Result.Text);
                }*/
               /* if (e.Result.Text == "hello")
                {
                    Console.WriteLine("Greetings to you too!");
                }
                else if (e.Result.Text == "select")
                {
                    // do select if possible during next update
                    //simon.enableSelect();
                }
                else if (e.Result.Text == "change")
                {
                    useRightHand = !useRightHand;
                }*/
            }
            else
            {
                Console.WriteLine("\nSpeech Recognized but confidence was too low: \t{0}", e.Result.Confidence);
            }
        }

        void sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Console.Write("\rSpeech Hypothesized: \t{0}", e.Result.Text);
        }

        void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("\nSpeech Rejected");
            curWnd.handleSpeechNotRecognised();
        }

        public void speakMessage(string message)
        {
            speechSynth.SpeakAsync(message);
        }
        #endregion
    }
}
