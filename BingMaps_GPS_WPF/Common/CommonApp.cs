using System;
using System.IO;
using Microsoft.Maps.MapControl.WPF;
using System.Device.Location;
using Microsoft.Win32;

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

        public static GeoPosition<GeoCoordinate> GetGeoPositionfromString(string line)
        {
            GeoPosition<GeoCoordinate> geoPosition = null;

            // ログされた位置の復元テスト
            string[] parts = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            string str_time = parts[0];
            string str_lat = parts[1];
            string str_lon = parts[2];
            string str_alt = parts[3];
            string str_accuracy = parts[4];
            string str_altaccuracy = parts[5];

            long filetime = 0;
            double lat = 0;
            double lon = 0;
            double alt = 0;
            double accuracy = 0;
            double altaccuracy = 0;

            long.TryParse(str_time, out filetime);
            double.TryParse(str_lat, out lat);
            double.TryParse(str_lon, out lon);
            double.TryParse(str_alt, out alt);
            double.TryParse(str_accuracy, out accuracy);
            double.TryParse(str_altaccuracy, out altaccuracy);

            DateTimeOffset time = DateTimeOffset.FromFileTime(filetime);

            var location = new Location(lat, lon, alt);

            GeoCoordinate geoCoordinate = new GeoCoordinate(lat, lon, alt);

            geoPosition = new GeoPosition<GeoCoordinate>(time, geoCoordinate);

            return geoPosition;

        }

        public static string GetOpenLogFilePath()
        {
            string path = string.Empty;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "GPSログファイルの選択";
            dialog.Filter = "GPSログファイル|*.log";

            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
            }

            return path;
        }

        public static string GetSaveLogFilePath()
        {
            string path = string.Empty;

            var dialog = new SaveFileDialog();
            dialog.Title = "GPSログファイルの保存先";
            dialog.Filter = "GPSログファイル|*.log";

            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
            }

            return path;
        }

    }
}
