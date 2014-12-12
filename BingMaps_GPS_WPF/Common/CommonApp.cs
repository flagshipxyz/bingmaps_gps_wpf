using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Microsoft.Maps.MapControl.WPF;
//using Bing.Maps;
using System.Device.Location;

namespace BingMaps_GPS_WPF
{
    public class CommonApp
    {
        /// <summary>
        /// GPSログを出力する
        /// 書式：CSV形式
        /// （時刻、緯度、経度、高度、緯度経度の精度、高度の精度）
        /// </summary>
        /// <param name="path">出力先（指定のないときは実行ディレクトリ下に出力）</param>
        /// <param name="geopos"></param>
        public static void LogGPS(string path, GeoPosition<GeoCoordinate> geopos)
        {
            if (!Path.IsPathRooted(path))
            {
                string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                path = Path.Combine(dir, "gps.log");            
            }

            GeoCoordinate geo = geopos.Location;

            string s = string.Format("{0},{1},{2},{3},{4},{5}", geopos.Timestamp.ToFileTime(),
                geo.Latitude, geo.Longitude, geo.Altitude,
                geo.HorizontalAccuracy, geo.VerticalAccuracy);

            File.AppendAllText(path, s + Environment.NewLine);
        
        }
    }
}
