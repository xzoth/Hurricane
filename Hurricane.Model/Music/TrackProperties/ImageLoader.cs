﻿ using System.Collections.Generic;
 using System.Threading.Tasks;
 using Hurricane.Utilities;

namespace Hurricane.Model.Music.TrackProperties
{
    static class ImageLoader
    {
        private static readonly List<ImageProvider> Images;
        private static bool _serviceIsRunning;

        static ImageLoader()
        {
            Images = new List<ImageProvider>();
        }

        public static void AddImage(ImageProvider imageProvider)
        {
            Images.Insert(0, imageProvider); //We always add to the top, perhaps we don't need the images at the top any more
            if (_serviceIsRunning)
                return;

            RunService().Forget();
        }

        private static async Task RunService()
        {
            _serviceIsRunning = true;
            while (Images.Count > 0)
            {
                var currentImage = Images[0];
                await currentImage.DownloadImageAsync();
                Images.Remove(currentImage);
            }
            _serviceIsRunning = false;
        }
    }
}