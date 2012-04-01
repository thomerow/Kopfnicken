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
      private KinectSensor _sensor = null;      // Only one Kinect sensor is used in this project

      public MainWindow()
      {
         InitializeComponent();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
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

         _sensor.SkeletonStream.Enable(new TransformSmoothParameters()     // Enable skeleton tracking with a bit of smoothing
         {
            Smoothing = 0.5f,
            Correction = 0.5f,
            Prediction = 0.5f,
            JitterRadius = 0.05f,
            MaxDeviationRadius = 0.04f
         });
         _sensor.DepthStream.Enable();
         _sensor.Start();

         _sensor.DepthFrameReady += Sensor_DepthFrameReady;
         _sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;
      }

      void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
      {
         SkeletonFrame frame = e.OpenSkeletonFrame();

         if (frame.SkeletonArrayLength == 0) return;  // No skeleton found

         Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
         frame.CopySkeletonDataTo(skeletons);

         foreach (Skeleton s in skeletons)
         {
            if (s.TrackingState == SkeletonTrackingState.Tracked)    // Only process the first skeleton found
            {
               TrackHeadAndShoulders(s.Joints);
               break;   // Exit loop after processing one skeleton
            }
         }

      }

      private void TrackHeadAndShoulders(JointCollection joints)
      {
         Joint jHead = joints[JointType.Head], jShoulderCenter = joints[JointType.ShoulderCenter];
         if ((jHead.TrackingState != JointTrackingState.Tracked) || (jShoulderCenter.TrackingState != JointTrackingState.Tracked)) return;  // Bail out if head or shouldr center is not tracked correctly

      }

      void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
      {
         // ToDo: implement
      }
   }
}
