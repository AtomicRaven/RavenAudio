using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RecordAudio;
using Android.Media;

namespace RavenRecord
{
    [Activity(Label = "PlayViewActivity")]
    public class PlayViewActivity : Activity
    {
        MediaPlayer player;
        string filePath;
        //string imageName;
        ImageView imgDropbox;
        TextView displayText;

        Button pause;
        Button stop;

        bool isPlaying = false ;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.PlayView);

            //imgDropbox = FindViewById<ImageView>(Resource.Id.imgDropbox);
            displayText = FindViewById<TextView>(Resource.Id.displayText);
            pause = FindViewById<Button>(Resource.Id.pause);
            stop = FindViewById<Button>(Resource.Id.stop);

            // Retrieves the name and the path of the image that will be downloaded
            filePath = Intent.Extras.GetString(MainActivity.FilePathKey);
            PlayFile(filePath);
            //imageName = Intent.Extras.GetString(MainActivity.ImageNameKey);

            pause.Click += delegate {
                Pause();
            };

            stop.Click += delegate {
                Stop();
            };
        }

        protected void PlayFile(string fileName)
        {

            //Have to get actual file from file system

            string[] audioFile = fileName.Split('|');
            string theFile = audioFile[0].Trim();
            string fileLocation = theFile;

            player = new MediaPlayer();

            if (player.IsPlaying)
            {
                Stop();
            }

            player.SetDataSource(fileLocation);
            player.Prepare();
            player.Start();

            UpdateDisplay("Playing " + fileName);
        }

        protected void Pause()
        {
            if (player.IsPlaying)
            {
                player.Pause();
                pause.Text = "Play";
            }

        }

        protected void Stop()
        {            
            player.Stop();
            player.Reset();          
        }


        protected void UpdateDisplay(string msg)

        {
            RunOnUiThread(() => displayText.Text = msg);
        }

        /*
        protected async override void OnStart()
        {
            base.OnStart();

            // Start the download of the image
            // Must be called in another thread, because it makes a network call
            await Task.Factory.StartNew(() => GetImageFromDropbox());
        }

        // Gets the specified image from the Dropbox folder that you specified in the DropboxCredentials class
        // This method must be invoked on a background thread because it makes a network call
        void GetImageFromDropbox()
        {
            try
            {
                // Image to be shown
                Drawable thumbnail;
                var cachePath = CacheDir.AbsolutePath + "/" + imageName;

                // Verifies if the image has not been downloaded before
                if (!File.Exists(cachePath))
                    using (var output = File.OpenWrite(cachePath))
                    {
                        // Get the info of the image to be downloaded
                        var metadata = MainActivity.DBApi.Metadata(imagePath, 0, null, false, null);
                        if (metadata == null)
                            throw new Exception("The file doesn't exist or the name of the file is incorrect");

                        // Gets the image from Dropbox and saves it to the cache folder
                        MainActivity.DBApi.GetThumbnail(metadata.Path, output, DropboxApi.ThumbSize.BestFit1024x768, DropboxApi.ThumbFormat.Jpeg, null);
                    }

                // Get the image from the cache
                thumbnail = Drawable.CreateFromPath(cachePath);
                // Show the image
                RunOnUiThread(() => imgDropbox.SetImageDrawable(thumbnail));
            }
            catch (System.Exception ex)
            {
                RunOnUiThread(() => Toast.MakeText(this, "There was a problem when trying to get the images: " + ex.Message, ToastLength.Long).Show());
            }
        } */
    }
    
}