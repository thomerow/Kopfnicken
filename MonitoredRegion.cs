using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kopfnicken
{
   class MonitoredRegion
   {
      private double _fFrom, _fTo;
      private bool _bIsInside;

      public event EventHandler MonitoredPositionLeavesRegion;
      public event EventHandler MonitoredPositionEntersRegion;

      public double From
      {
         get { return _fFrom; }

         set
         {
            _fFrom = value;
            if (_fTo < _fFrom) _fTo = _fFrom;
         }
      }

      public double To
      {
         get { return _fTo; }

         set
         {
            _fTo = value;
            if (_fFrom > _fTo) _fFrom = _fTo;
         }
      }

      public double MonitoredPosition
      {
         set
         {
            if ((value >= _fFrom) && (value <= _fTo))
            {
               if (!_bIsInside)
               {
                  OnEnteringRegion();
                  _bIsInside = true;
               }
            }
            else
            {
               if (_bIsInside)
               {
                  OnLeavingRegion();
                  _bIsInside = false;
               }
            }
         }
      }

      private void OnLeavingRegion()
      {
         if (MonitoredPositionLeavesRegion == null) return;
         MonitoredPositionLeavesRegion(this, EventArgs.Empty);
      }

      private void OnEnteringRegion()
      {
         if (MonitoredPositionEntersRegion == null) return;
         MonitoredPositionEntersRegion(this, EventArgs.Empty);
      }
   }
}
