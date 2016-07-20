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

using Java.Lang;
using Dropbox.CoreApi.Android;
using Dropbox.CoreApi.Android.Session;

namespace RecordAudio
{
    public static class DropboxCredentials
    {
        // To get your credentials, create your own Drobox App.
        // Visit the following link: https://www.dropbox.com/developers/apps
        public static string AppKey = "jzr2z55njfftkrm";
        public static string AppSecret = "isbk49be5e9wbhm";
        public static string FolderPath = "/RavenRecord/";
        public static string PreferencesName = "Prefs";
    }

    [Activity (Label = "Raven Record", MainLauncher = true, Icon = "@drawable/ribbonMicPurple")]
    public class MainActivity : Activity
    {
        public static readonly string FilePathKey = "FilePath";
        MediaRecorder recorder;        
        Button start;
        Button stop;
        Button delete;
        Button btnLink;
        TextView counter;
        ListView listView;
        Timer aTimer;
        Int16 intCounter= 0;
        IList<FileSystemInfo> files;
        string recordFolder = "/sdcard/ravenrecord/";
        string recordFile;

        public static DropboxApi DBApi { get; set; }
        
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Main);

            // Creates a new session
            DBApi = new DropboxApi(CreateSession());

            btnLink = FindViewById<Button>(Resource.Id.btnLink);
            // If you have not specified the keys correctly, disable the link button
            btnLink.Enabled = VerifyDropboxKeys();

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
            };

            delete.Click += delegate {
                DeleteAllFiles(recordFolder);
                RefreshFiles();
            };

            // Initializes the process of linking to Dropbox or the process to revoke the permission
            btnLink.Click += delegate {
                try
                {
                    if (!DBApi.Session.IsLinked)
                        (DBApi.Session as AndroidAuthSession).StartOAuth2Authentication(this);
                    else
                        Logout();
                }
                catch (ActivityNotFoundException ex)
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                }
            };

            listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {
                string fileSelected = listView.GetItemAtPosition(e.Position).ToString();
                var intent = new Intent(this, typeof(PlayViewActivity));
                
                intent.PutExtra(MainActivity.FilePathKey, recordFolder + fileSelected);
                StartActivity(intent);
                Toast.MakeText(this, fileSelected, Android.Widget.ToastLength.Short).Show();
            };

            // From DropBox Image Example
            /* 
            lstImagesName.ItemClick += (sender, e) => {
                var intent = new Intent(this, typeof(ImageViewActivity));
                intent.PutExtra(MainActivity.ImagePathKey, adapter.ImagesPath[e.Position]);
                intent.PutExtra(MainActivity.ImageNameKey, adapter.ImagesName[e.Position]);
                StartActivity(intent);
            };

            */

        }

        protected void SetDisplay()
        {
            start = FindViewById<Button>(Resource.Id.start);
            stop = FindViewById<Button>(Resource.Id.stop);
            delete = FindViewById<Button>(Resource.Id.delete);
            counter = FindViewById<TextView>(Resource.Id.counter);
            listView = FindViewById<ListView>(Resource.Id.listView);
            listView.FastScrollEnabled = true;

        }

        protected void RefreshFiles()
        {
            files = GetFiles(recordFolder);
            listView.Adapter = new ArrayAdapter<System.String>(this, Android.Resource.Layout.SimpleListItem1, GetFileNames(files));
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

            recorder = new MediaRecorder();
            recorder.SetAudioSource(AudioSource.Mic);
            recorder.SetOutputFormat(OutputFormat.Mpeg4);
            recorder.SetAudioEncoder(AudioEncoder.AmrNb);
            recorder.SetOutputFile(recordFolder + "/" + recordFile);

            recorder.SetMaxFileSize(0);
            recorder.SetMaxDuration(0);

            recorder.Prepare();
            recorder.Start();

        }

        protected void StopRecord()
        {
            stop.Enabled = !stop.Enabled;
            start.Enabled = !start.Enabled;

            aTimer.Stop();
            intCounter = 0;
            UpdateDisplay("Recording Stopped.");

            recorder.Stop();
            recorder.Reset();

            RefreshFiles();
        }

        

        protected void OnTimedEvent(System.Object source, ElapsedEventArgs e)
        {
            intCounter++;            
            UpdateDisplay("Recording: " + intCounter.ToString() + " seconds");  

        }

        protected void UpdateDisplay(string msg)
        {
            RunOnUiThread(() => counter.Text = msg);
        }

        public void OnError(MediaRecorder mr, [Android.Runtime.GeneratedEnum] MediaRecorderError what, Int32 extra)
        {

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

            foreach (var item in dir.GetFileSystemInfos().Where(item => item.Exists).OrderByDescending(item => item.CreationTime))
            {
                //item.Delete();
                visibleThings.Add(item);
            }

            return visibleThings;
        }

        protected void DeleteAllFiles(string directoryName)
        {
            IList<FileSystemInfo> visibleThings = new List<FileSystemInfo>();
            var dir = new DirectoryInfo(directoryName);

            foreach (var item in dir.GetFileSystemInfos().Where(item => item.Exists).OrderByDescending(item => item.CreationTime))
            {
                item.Delete();                
            }
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

        //DropBox stuff

        protected async override void OnStart()
        {
            base.OnStart();

            // Verifies if you have linked the app with Dropbox before and retrieves the images on Dropbox
            if (DBApi.Session.IsLinked)
            {
                btnLink.Text = "Unlink account";

                // Must be called in another thread, because it makes a network call
                await Task.Factory.StartNew(() => GetFilesFromDropbox());
            }
        }

        void GetFilesFromDropbox()
        {
        }

        protected async override void OnResume()
        {
            base.OnResume();

            // After you allowed to link the app with Dropbox,
            // you need to finish the Authentication process
            var session = DBApi.Session as AndroidAuthSession;
            if (!session.AuthenticationSuccessful())
                return;

            try
            {
                // Call this method to finish the authentcation process
                // Will bind the user's access token to the session.
                session.FinishAuthentication();
                // Save the Dropbox authetication token
                StoreAuth(session);
                btnLink.Text = "Unlink account";

                // Get all the images that live in the specified Dropbox folder
                // Must be called in another thread, because it makes a network call
                await Task.Factory.StartNew(() => GetFilesFromDropbox());
            }
            catch (IllegalStateException ex)
            {
                Toast.MakeText(this, ex.LocalizedMessage, ToastLength.Short).Show();
            }
        }

        // Creates a new Dropbox Session or retrives the existing session
        AndroidAuthSession CreateSession()
        {
            var session = new AndroidAuthSession(new AppKeyPair(DropboxCredentials.AppKey, DropboxCredentials.AppSecret));
            GetAuth(session);
            return session;
        }

        // Retrieves the Dropbox Auth Token saved on preferences
        void GetAuth(AndroidAuthSession session)
        {         
            var prefs = GetSharedPreferences(DropboxCredentials.PreferencesName, 0);
            var key = prefs.GetString(DropboxCredentials.AppKey, null);
            var secret = prefs.GetString(DropboxCredentials.AppSecret, null);

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
                return;

            session.OAuth2AccessToken = secret;
        }

        // Unlink the app from Dropbox
        void Logout()
        {
            DBApi.Session.Unlink();

            RemoveAuth();

            //lstImagesName.Adapter = null;
            btnLink.Text = "Link to Dropbox!";
        }

        // Saves the Dropbox Auth token to proferences
        void StoreAuth(AndroidAuthSession session)
        {
            var oauth2AccessToken = session.OAuth2AccessToken;

            if (!string.IsNullOrWhiteSpace(oauth2AccessToken))
            {
                var edit = GetSharedPreferences(DropboxCredentials.PreferencesName, 0).Edit();
                edit.PutString(DropboxCredentials.AppKey, "oauth2:");
                edit.PutString(DropboxCredentials.AppSecret, oauth2AccessToken);
                edit.Commit();
            }
        }

        // Removes the Dropbox Auth token from preferences
        void RemoveAuth()
        {
            var edit = GetSharedPreferences(DropboxCredentials.PreferencesName, 0).Edit();
            edit.Clear();
            edit.Commit();
        }

        // Verifies that the Dropbox Keys are set correctly
        bool VerifyDropboxKeys()
        {
            // Verifies the keys in the code
            if (DropboxCredentials.AppKey.Equals("YOUR_APP_KEY") || DropboxCredentials.AppSecret.Equals("YOUR_APP_SECRET"))
            {
                Toast.MakeText(this, "Please, set your Dropbox Keys in DropboxCredentials class.", ToastLength.Short).Show();
                return false;
            }

            // Verifies the App key in the AndroidManifest file
            var testIntent = new Intent(Intent.ActionView);
            var manifestKey = "db-" + DropboxCredentials.AppKey;
            var uri = manifestKey + "://" + AuthActivity.AuthVersion + "/test";
            testIntent.SetData(Android.Net.Uri.Parse(uri));

            if (PackageManager.QueryIntentActivities(testIntent, 0).Count == 0)
            {
                Toast.MakeText(this, "The App key in the manifest file doesn't match with your App key in code." +
                " Please verify.", ToastLength.Short).Show();
                return false;
            }

            return true;
        }
    }
}


