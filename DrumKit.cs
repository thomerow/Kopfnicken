using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using Midi;
using System.Windows;

namespace Kopfnicken
{
   internal class DrumKit
   {
      protected static Clock _midiClock;
      protected static OutputDevice _devOut;

      enum InstrumentPitch
      {
         KickDrum = Pitch.C1,
         SnareDrum = Pitch.D1,
         ClosedHiHat = Pitch.FSharp1,
         OpenHiHat = Pitch.ASharp1,
      }

      public bool IsHiHatOpen { get; private set; }

      static DrumKit()
      {
         _midiClock = new Clock(120);
         ShowMIDIDevSelDialog();
      }

      public void KickTheKick()
      {
         SendNote((Pitch)InstrumentPitch.KickDrum, 100, 0.5f);
      }

      public void OpenHiHat()
      {
         IsHiHatOpen = true;

         Debug.WriteLine(MethodInfo.GetCurrentMethod().Name);
         // ToDo: implement
      }

      public void CloseHiHat()
      {
         if (!IsHiHatOpen) return;

         Debug.WriteLine(MethodInfo.GetCurrentMethod().Name);
         // ToDo: implement

         IsHiHatOpen = false;
      }

      public void HitTheSnare()
      {
         Debug.WriteLine(MethodInfo.GetCurrentMethod().Name);
         // ToDo: implement
      }

      protected static void UseOutputDevice(Midi.OutputDevice odNew)
      {
         if ((odNew == _devOut) || (odNew == null)) return;
         if (_midiClock.IsRunning) { _midiClock.Stop(); _midiClock.Reset(); }
         if ((_devOut != null) && _devOut.IsOpen) _devOut.Close();
         _devOut = odNew;
         _devOut.Open();
         _midiClock.Start();
      }

      private static void ShowMIDIDevSelDialog()
      {
         int nSelectedMidiDevice = 0;

         WndMIDIDeviceSelection wndDevSel = new WndMIDIDeviceSelection();

         // Find last used output device
         foreach (OutputDevice od in OutputDevice.InstalledDevices)
         {
            int nIdxTmp = wndDevSel._lbDevices.Items.Add(od.Name);
            if (od.Name == Properties.Settings.Default.MIDIOutputDeviceName) nSelectedMidiDevice = nIdxTmp;
         }

         if (wndDevSel.ListBox.Items.Count > 0)
         {
            wndDevSel.ListBox.SelectedIndex = nSelectedMidiDevice;
            wndDevSel.ShowDialog();

            UseOutputDevice(OutputDevice.InstalledDevices[wndDevSel.ListBox.SelectedIndex]);
         }
         else
         {
            MessageBox.Show("No MIDI input devices found.", "MIDI Devices Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
         }
      }

      private void SendNote(Pitch pitch, int velocity, float fDuration)
      {
         if ((_devOut == null) || !_devOut.IsOpen || !_midiClock.IsRunning) return;

         _midiClock.Schedule(new NoteOnOffMessage(_devOut, Channel.Channel1, pitch, velocity, _midiClock.Time, _midiClock, fDuration));
      }

      private void NoteOn(Pitch pitch, int velocity)
      {
         if ((_devOut == null) || !_devOut.IsOpen || !_midiClock.IsRunning) return;

         _midiClock.Schedule(new NoteOnMessage(_devOut, Channel.Channel1, pitch, velocity, _midiClock.Time));
      }

      private void NoteOff(Pitch pitch)
      {
         if ((_devOut == null) || !_devOut.IsOpen || !_midiClock.IsRunning) return;

         _midiClock.Schedule(new NoteOffMessage(_devOut, Channel.Channel1, pitch, 100, _midiClock.Time));
      }

      internal static void ShutDownMIDI()
      {
         if (_midiClock.IsRunning) _midiClock.Stop();
         if ((_devOut != null) && _devOut.IsOpen)
         {
            Properties.Settings.Default.MIDIOutputDeviceName = _devOut.Name;
            _devOut.Close();
         }
         Properties.Settings.Default.Save();
      }
   }
}
