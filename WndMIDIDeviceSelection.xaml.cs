using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kopfnicken
{
   /// <summary>
   /// Interaction logic for MIDIDeviceSelection.xaml
   /// </summary>
   public partial class WndMIDIDeviceSelection : Window
   {
      public WndMIDIDeviceSelection()
      {
         InitializeComponent();
      }

      public int SelectedIndex { get; protected set; }

      public ListBox ListBox
      {
         get { return _lbDevices; }
      }

      protected void BtnSelect_Click(object sender, RoutedEventArgs e)
      {
         SelectedIndex = _lbDevices.SelectedIndex;
         Close();
      }

      private void LbDevices_MouseDoubleClick(object sender, MouseButtonEventArgs e)
      {
         SelectedIndex = _lbDevices.SelectedIndex;
         Close();
      }
   }
}
