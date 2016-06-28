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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        IList<FileSystemInfo> files;
        string recordFolder = "/sdcard/ravenrecord/";
        string recordFile;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Main);
                        
            SetDisplay();
            UpdateDisplay("Ready");
            CreateDirectory(recordFolder);

            BindUI();
            RefreshFiles();

        }

        protected void BindUI()
        {
            start.Click += delegate {
                StartRecord();
            };

            stop.Click += delegate {
                StopRecord();
                RefreshFiles();
            };

            listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {
                string fileSelected = listView.GetItemAtPosition(e.Position).ToString();
                PlayFile(fileSelected);
                Toast.MakeText(this, fileSelected, Android.Widget.ToastLength.Short).Show();
            };
            
        }

        protected void SetDisplay()
        {
            start = FindViewById<Button>(Resource.Id.start);
            stop = FindViewById<Button>(Resource.Id.stop);
            counter = FindViewById<TextView>(Resource.Id.counter);
            listView = FindViewById<ListView>(Resource.Id.listView);
            listView.FastScrollEnabled = true;

        }

        protected void RefreshFiles()
        {
            files = GetFiles(recordFolder);
            listView.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, GetFileNames(files));
        }

        protected void StartRecord()
        {
            string fileName = DateTime.Now.ToString("MM-dd-yyyy-HHmmss");            
            recordFile = fileName + ".mp3";

            stop.Enabled = !stop.Enabled;
            start.Enabled = !start.Enabled;

            aTimer = new Timer();
            aTimer.Interval = 1000;
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Enabled = true;
            aTimer.Start();

            recorder.SetAudioSource(AudioSource.Mic);
            recorder.SetOutputFormat(OutputFormat.Mpeg4);
            recorder.SetAudioEncoder(AudioEncoder.AmrNb);
            recorder.SetOutputFile(recordFolder + "/" + recordFile);
            recorder.Prepare();
            recorder.Start();

        }

        protected void StopRecord()
        {
            stop.Enabled = !stop.Enabled;
            start.Enabled = !start.Enabled;

            aTimer.Stop();
            UpdateDisplay("Recording Stopped.");

            recorder.Stop();
            recorder.Reset();
        }

        protected void PlayFile(string fileName)
        {

            //Have to get actual file from file system

            string[] audioFile = fileName.Split('|');
            string theFile = audioFile[0].Trim();
            string fileLocation = recordFolder + theFile;
            
            if (player.IsPlaying)
            {
                player.Stop();
                player.Reset();
            }
            
            player.SetDataSource(fileLocation);
            player.Prepare();
            player.Start();

            UpdateDisplay("Playing " + fileName);
        }

        protected void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            intCounter++;
            UpdateDisplay("Recording: " + intCounter.ToString() + " seconds");  

        }

        protected void UpdateDisplay(string msg)
        {
            RunOnUiThread(() => counter.Text = msg);
        }

        protected override void OnResume ()
        {
            base.OnResume ();
            
            recorder = new MediaRecorder ();
            player = new MediaPlayer ();
         
            player.Completion += (sender, e) => {
                player.Reset ();
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

        protected string[] GetFileNames(IList<FileSystemInfo> files)
        {
            string[] fileNames = new string[files.Count];
        
            for (int i = 0; i <= fileNames.Length-1; i++)
            {               
                fileNames[i] = files[i].Name + "    |    " + GetTrackDuration(files[i].FullName);
            }

            return fileNames;
        }

        public string GetTrackDuration(string filePath)
        {
            MediaMetadataRetriever metaRetriever = new MediaMetadataRetriever();
            metaRetriever.SetDataSource(filePath);           
            string duration = metaRetriever.ExtractMetadata(MetadataKey.Duration);
            metaRetriever.Release();

            TimeSpan t = TimeSpan.FromMilliseconds(Convert.ToDouble(duration));
            //duration = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);

            //only need minutes seconds for now
            duration = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

            return duration;
        }

        protected IList<FileSystemInfo> GetFiles(string directoryName)
        {
            IList<FileSystemInfo> visibleThings = new List<FileSystemInfo>();
            var dir = new DirectoryInfo(directoryName);

            foreach (var item in dir.GetFileSystemInfos().Where(item => item.Exists))
            {
                visibleThings.Add(item);
            }

            return visibleThings;
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


