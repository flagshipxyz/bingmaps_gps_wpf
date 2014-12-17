using GalaSoft.MvvmLight;
using System;

namespace BingMaps_GPS_WPF.Model
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DisposableModelBase : GalaSoft.MvvmLight.ObservableObject, IDisposable
    {
        public DisposableModelBase()
        {
        }

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _disposed = true;
            }
        }

        ~DisposableModelBase()
        {
            Dispose(false);
        }
    }
}