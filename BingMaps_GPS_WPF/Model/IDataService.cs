using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMaps_GPS_WPF.Model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
    }
}
