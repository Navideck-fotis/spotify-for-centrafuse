using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotiFire.SpotifyLib;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using centrafuse.Plugins;   

namespace Spotify
{
	//LK, 22-may-2016, General: Add support for full function visuals, including album art
    public partial class Spotify
    {
        private string currentImageId;
        private Image currentImage;
        private void LoadImage(string imageId)
        {
            currentImageId = imageId;
            
            CF_clearPictureImage("AlbumArt");
            if (currentImage != null)
                currentImage.Dispose();

            if (!string.IsNullOrEmpty(imageId))
            {
                ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    try
                    {
                        var image = SpotifySession.GetImageFromId(imageId);
                        image.WaitForLoaded(); //and it's not really loaded yet :(
                        if (imageId.Equals(currentImageId))
                        {
                            for (int i = 0; i < 60; i++)
                            {
                                if (image.Format == sp_imageformat.SP_IMAGE_FORMAT_UNKNOWN)
                                {
                                    Thread.Sleep(1000);
                                }
                                else
                                    break;
                            }

                            var imageObject = image.GetImage();
                            if (imageId.Equals(currentImageId))
                            {
                                this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                                {
                                    if (imageId.Equals(currentImageId))
                                    {
                                        currentImage = imageObject;
                                        //---   CF_setPictureImage("AlbumArt", imageObject);
                                        SetVisImage(imageObject, imageId);
                                        imageObject = ResizeToFitBox(imageObject,CF_pluginGetVisBounds());
                                    }
                                    else
                                    {
                                        imageObject.Dispose();
                                        SetVisImage(null, string.Empty);
                                    }
                                }));
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        WriteError(ex);
                    }
                });
            }
        }

        
        private Image ResizeToFitBox(Image imageObject, Rectangle albumArtBounds)
        {
            int albumArtSize = albumArtBounds.Width;

            Rectangle drawRectangle;
            if (imageObject.Width > imageObject.Height)
            {
                double factor = (double)albumArtSize / imageObject.Width;
                int resizeHeight = (int)Math.Ceiling(imageObject.Height * factor);
                drawRectangle = new Rectangle(0, (albumArtSize - resizeHeight) / 2, albumArtSize, resizeHeight);
            }
            else
            {
                double factor = (double)albumArtSize / imageObject.Height;
                int resizeWidth = (int)Math.Ceiling(imageObject.Width * factor);
                drawRectangle = new Rectangle((albumArtSize - resizeWidth) / 2, 0, resizeWidth, albumArtSize);
            }

            Bitmap resized = new Bitmap(albumArtSize, albumArtSize);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.Clear(Color.Black);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
                g.DrawImage(imageObject, drawRectangle);
                g.Flush();
            }
            imageObject.Dispose();
            return resized;
        }
    }
}
