using System;
using System.IO;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HonoursGame
{
    public class Screenshot
    {
        private string screenshotPath;
        //private Game1 appRef;

        public Screenshot(string screenshotPath, string sessionid)
        {
            this.screenshotPath = screenshotPath + "\\" + sessionid;
            if (!System.IO.Directory.Exists(this.screenshotPath))
                System.IO.Directory.CreateDirectory(this.screenshotPath);

        }

        public void TakeScreenshot(GraphicsDevice device)
        {
            int w = device.PresentationParameters.BackBufferWidth;
            int h = device.PresentationParameters.BackBufferHeight;

            //pull the picture from the buffer
            int[] backBuffer = new int[w * h];
            device.GetBackBufferData<int>(backBuffer);

            //copy into a texture
            Texture2D screenshotTexture = new Texture2D(device, w, h, false, device.PresentationParameters.BackBufferFormat);
            screenshotTexture.SetData(backBuffer);

            Thread newThread = new Thread(delegate()
            {
                ScreenshotThread(screenshotTexture);
            });
            newThread.Start();
        }

        private void ScreenshotThread(Texture2D screenshotTexture)
        {
            //try
            //{
            if(!screenshotTexture.IsDisposed)
            {
                string timestampName = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                int i = 1;
                string screenshotPostfix = "";
                while (File.Exists(String.Format("{0}\\{1}{2}.jpg", screenshotPath, timestampName, screenshotPostfix)))
                {
                    screenshotPostfix = " (" + i + ")";
                    i++;
                }

                string filename = String.Format("{0}\\{1}{2}.jpg", screenshotPath, timestampName, screenshotPostfix);

                using (Stream stream = File.OpenWrite(filename))
                {
                    screenshotTexture.SaveAsJpeg(stream, screenshotTexture.Width, screenshotTexture.Height);
                }
        
                screenshotTexture.Dispose();
            }
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("SCREENSHOT ERROR!!!");
            //}
        }
    }
}
