using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using Android.Gms.Vision;
using Android.Graphics;
using Android.Gms.Vision.Texts;
using Android.Util;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using static Android.Gms.Vision.Detector;
using System.Text;

namespace ImageScanner
{
    [Activity(Label = "ImageScanner", Theme = "@style/Theme.AppCompat.Light.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISurfaceHolderCallback, IProcessor
    {

        private SurfaceView cameraView;
        private TextView textView;
        private CameraSource cameraSource;
        private const int RequestCameraPermissionID = 1;

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {

                case RequestCameraPermissionID:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            cameraSource.Start(cameraView.Holder);
                        }
                        break;
                    }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            cameraView = FindViewById<SurfaceView>(Resource.Id.surface_view);
            textView = FindViewById<TextView>(Resource.Id.text_view);

            TextRecognizer textRecognizer = new TextRecognizer.Builder(ApplicationContext).Build();
            if (!textRecognizer.IsOperational)
            {
                Log.Error("Main Activity", "Detector Dependency is not yet available");
            }
            else
            {
                cameraSource = new CameraSource.Builder(ApplicationContext, textRecognizer)
                    .SetFacing(CameraFacing.Back)
                    .SetRequestedPreviewSize(1280, 1024)
                    .SetRequestedFps(2.0f)
                    .SetAutoFocusEnabled(true)
                    .Build();

                cameraView.Holder.AddCallback(this);
                textRecognizer.SetProcessor(this);
            }
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            if(ActivityCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) 
                                != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[]
                {
                    Android.Manifest.Permission.Camera
                    }, RequestCameraPermissionID);
                return;
            }

            cameraSource.Start(cameraView.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            cameraSource.Stop();
        }

        public void ReceiveDetections(Detections detections)
        {
            SparseArray items = detections.DetectedItems;

            if (items.Size() != 0)
            {

                textView.Post(() =>
                {

                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < items.Size(); ++i)
                    {
                        stringBuilder.Append(((TextBlock)items.ValueAt(i)).Value);
                        stringBuilder.Append("\n");
                    }
                    textView.Text = stringBuilder.ToString();
                });
            }
        }

        

        void IProcessor.Release()
        {
            
        }
    }
}