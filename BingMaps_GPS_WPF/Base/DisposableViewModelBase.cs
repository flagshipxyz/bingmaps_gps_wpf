using GalaSoft.MvvmLight;
using System;

namespace BingMaps_GPS_WPF.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DisposableViewModelBase : ViewModelBase, IDisposable
    {
        public DisposableViewModelBase()
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

        ~DisposableViewModelBase()
        {
            Dispose(false);
        }
    }
}