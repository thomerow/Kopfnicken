using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Diagnostics;


namespace Kopfnicken
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      internal DrumKit _drumKit;
      private bool _bInitializingSensor = true;
      private MonitoredRegion _regKick, _regHihat, _regSnare;

      struct ColorFrameData
      {
         public byte[] Data;           // Data
         public Point Dim;             // Dimensions
         public int BytesPerPixel;     // BytesPerPixel
      }

      private KinectSensor _sensor = null;   // Only one Kinect sensor is used in this project
      private ColorFrameData _imgCur;        // Current color frame grabbed by the Kinect's camera
      private Skeleton _skeleton = null;     // Curently tracked skeleton

      private const double[] HeadPosRangeHiHatOpen = null;     // Range the head has to be in to open the hi hat
      private const double[] HeadPosRangeKickKicked = null;    // Range the head has to be in to kick the kick drum

      public MainWindow()
      {
         InitializeComponent();

         _regKick = new MonitoredRegion() { From = 0.08, To = 0.2 };
         _regHihat = new MonitoredRegion() { From = -0.3, To = 0.08 };
         _regSnare = new MonitoredRegion() { From = 0.6, To = 2 };

         _regKick.MonitoredPositionEntersRegion += new EventHandler(Kick_MonitoredPositionEntersRegion);
         _regHihat.MonitoredPositionEntersRegion += new EventHandler(Hihat_MonitoredPositionEntersRegion);
         _regHihat.MonitoredPositionLeavesRegion += new EventHandler(Hihat_MonitoredPositionLeavesRegion);
         _regSnare.MonitoredPositionEntersRegion += new EventHandler(Snare_MonitoredPositionEntersRegion);
      }

      void Snare_MonitoredPositionEntersRegion(object sender, EventArgs e)
      {
         _drumKit.HitTheSnare();
      }

      void Hihat_MonitoredPositionLeavesRegion(object sender, EventArgs e)
      {
         _drumKit.CloseHiHat();
      }

      void Hihat_MonitoredPositionEntersRegion(object sender, EventArgs e)
      {
         _drumKit.OpenHiHat();
      }

      void Kick_MonitoredPositionEntersRegion(object sender, EventArgs e)
      {
         _drumKit.KickTheKick();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         _drumKit = new DrumKit();

         KinectStart();
      }

      private void KinectStart()
      {
         // Check if a sensor is connected right now
         if (KinectSensor.KinectSensors.Count > 0) _sensor = KinectSensor.KinectSensors[0];
         InitSensor();
         KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
      }

      void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
      {
         switch (e.Status)
         {
            case KinectStatus.Connected:
               if (_sensor == null) _sensor = e.Sensor;
               break;

            case KinectStatus.DeviceNotGenuine:
               if (e.Sensor == _sensor) _sensor = null;
               break;

            case KinectStatus.DeviceNotSupported:
               if (e.Sensor == _sensor) _sensor = null;
               break;

            case KinectStatus.Disconnected:
               if (e.Sensor == _sensor) _sensor = null;
               break;

            case KinectStatus.Error:
               if (e.Sensor == _sensor) _sensor = null;
               break;

            case KinectStatus.Initializing:
               if (e.Sensor == _sensor) _sensor = null;
               break;

            case KinectStatus.NotPowered:
               if (e.Sensor == _sensor) _sensor = null;
               break;

            case KinectStatus.NotReady:
               if (e.Sensor == _sensor) _sensor = null;
               break;
         }

         InitSensor();
      }

      private void InitSensor()
      {
         if (_sensor == null) return;

         _bInitializingSensor = true;

         //_sensor.SkeletonStream.Enable(new TransformSmoothParameters()     // Enable skeleton tracking with a bit of smoothing
         //{
         //   Smoothing = 0.5f,
         //   Correction = 0.5f,
         //   Prediction = 0.5f,
         //   JitterRadius = 0.05f,
         //   MaxDeviationRadius = 0.04f
         //});
         _sensor.SkeletonStream.Enable();

         _sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
         _sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
         _sensor.Start();

         // Attach important event handlers
         _sensor.DepthFrameReady += Sensor_DepthFrameReady;
         _sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;
         _sensor.ColorFrameReady += Sensor_ColorFrameReady;

         _bInitializingSensor = false;
      }

      void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
      {
         ColorImageFrame frame = e.OpenColorImageFrame();
         if (frame == null) return;

         _imgCur.BytesPerPixel = frame.BytesPerPixel;
         _imgCur.Data = new byte[frame.BytesPerPixel * frame.Width * frame.Height];
         _imgCur.Dim.X = frame.Width;
         _imgCur.Dim.Y = frame.Height;
         frame.CopyPixelDataTo(_imgCur.Data);

         frame.Dispose();
      }

      void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
      {
         _skeleton = null;
         SkeletonFrame frame = e.OpenSkeletonFrame();
         if (frame == null) return;
         if (frame.SkeletonArrayLength == 0) return;  // No skeleton found

         Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
         frame.CopySkeletonDataTo(skeletons);

         foreach (Skeleton s in skeletons)
         {
            if (s.TrackingState == SkeletonTrackingState.Tracked)    // Only process the first properly tracked skeleton found (for now)
            {
               _skeleton = s;
               TrackHeadAndShoulders(s.Joints);
               TrackRightHand(s.Joints);
               break;   // Exit loop after processing one skeleton (for now)
            }
         }

         frame.Dispose();
      }

      private void TrackRightHand(JointCollection joints)
      {
         Joint jRightHand = joints[JointType.HandRight];
         Joint jShoulderCenter = joints[JointType.ShoulderCenter];

         ProcessRightHandPosition(jShoulderCenter.Position.Z - jRightHand.Position.Z);
      }

      private void ProcessRightHandPosition(float p)
      {
         _lblPosRightHand.Content = p.ToString("0.000");

         _regSnare.MonitoredPosition = p;
      }

      private void TrackHeadAndShoulders(JointCollection joints)
      {
         Joint jHead = joints[JointType.Head];
         Joint jShoulderCenter = joints[JointType.ShoulderCenter];

         // Bail out if head or shoulder center is not tracked correctly
         if ((jHead.TrackingState != JointTrackingState.Tracked) || (jShoulderCenter.TrackingState != JointTrackingState.Tracked)) return;

         ProcessHeadPosition(jShoulderCenter.Position.Z - jHead.Position.Z);
      }

      private void ProcessHeadPosition(float p)
      {
         _lblPosHead.Content = p.ToString("0.000");

         _regKick.MonitoredPosition =
         _regHihat.MonitoredPosition = p;
      }

      void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
      {
         DepthImageFrame frame = e.OpenDepthImageFrame();
         if (frame == null) return;

         // ColorImagePoint p = frame.MapToColorImagePoint(10, 10, ColorImageFormat.RgbResolution640x480Fps30);
         //p.

         // ToDo: implement

         frame.Dispose();
      }

      private void Window_Closed(object sender, EventArgs e)
      {
         // Shut down kinect
         if (_sensor != null) {
            if (_sensor.Status == KinectStatus.Connected) _sensor.Stop();
         }

         // Shut down midi
         DrumKit.ShutDownMIDI();
      }
   }
}
