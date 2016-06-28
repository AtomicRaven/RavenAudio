using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Media;
using System.IO;
using System.Timers;
using RavenRecord;
using Android.Provider;

namespace RecordAudio
{
    [Activity (Label = "Raven Record", MainLauncher = true, Icon = "@drawable/ribbonMicPurple")]
    public class Activity1 : Activity
    {
        MediaRecorder recorder;
        MediaPlayer player;
        Button start;
        Button stop;
        TextView counter;
        ListView listView;
        Timer aTimer;
        Int16 intCounter= 0;
        string[] items;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Main);

            items = new string[] { "Vegetables", "Fruits", "Flower Buds", "Legumes", "Bulbs", "Tubers" };

            string recordFolder = "/sdcard/ravenrecord/";

            start = FindViewById<Button> (Resource.Id.start);
            stop = FindViewById<Button> (Resource.Id.stop);
            counter = FindViewById<TextView>(Resource.Id.counter);

            listView = FindViewById<ListView>(Resource.Id.listView);           
            listView.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, items);

            counter.Text = "Ready";

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            CreateDirectory(recordFolder);

            string path = recordFolder + "/" + unixTimestamp.ToString() + ".mp3";
         
            start.Click += delegate {
                stop.Enabled = !stop.Enabled;
                start.Enabled = !start.Enabled;
                
                aTimer = new Timer();
                aTimer.Interval = 1000;
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Enabled = true;
                aTimer.Start();

                recorder.SetAudioSource (AudioSource.Mic);      
                recorder.SetOutputFormat (OutputFormat.Mpeg4);            
                recorder.SetAudioEncoder (AudioEncoder.AmrNb);           
                recorder.SetOutputFile (path);
                recorder.Prepare ();               
                recorder.Start ();

            };
         
            stop.Click += delegate {
                stop.Enabled = !stop.Enabled;

                aTimer.Stop();
                RunOnUiThread(() => counter.Text = "Recording Stopped. Playing Back...");

                recorder.Stop ();
                recorder.Reset ();               

                player.SetDataSource (path);
                player.Prepare ();
                player.Start ();
            };       
        }

        protected void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            intCounter++;
            RunOnUiThread(() => counter.Text = "Recording: " + intCounter.ToString() + " seconds");  

        }

        protected override void OnResume ()
        {
            base.OnResume ();
            
            recorder = new MediaRecorder ();
            player = new MediaPlayer ();
         
            player.Completion += (sender, e) => {
                player.Reset ();
                start.Enabled = !start.Enabled;
            };
        }

        protected override void OnPause ()
        {
            base.OnPause ();
            
            player.Release ();
            recorder.Release ();
         
            player.Dispose ();
            recorder.Dispose ();
            player = null;
            recorder = null;
        }

        void CreateDirectory(string directoryName)
        {
            bool directoryExists = Directory.Exists(directoryName);

            if (!directoryExists)
            {
                var directoryUri = Directory.CreateDirectory(directoryName);


                if (directoryUri != null)
                {
                    Toast.MakeText(this, string.Format("Created a directory [{0}]",
                        directoryName), ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(this, string.Format("Failed to created a directory [{0}] : ",
                        directoryName), ToastLength.Short).Show();
                }
            } else
            {
                Toast.MakeText(this, string.Format("Directory already Exists! [{0}] : ",
                        directoryName), ToastLength.Short).Show();
            }
        }
    }
}


