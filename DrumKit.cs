using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace Kopfnicken
{
   class DrumKit
   {
      public bool IsHiHatOpen { get; private set; }

      public void KickTheKick() 
      {
         Debug.WriteLine(MethodInfo.GetCurrentMethod().Name);
         // ToDo: implement
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
   }
}
