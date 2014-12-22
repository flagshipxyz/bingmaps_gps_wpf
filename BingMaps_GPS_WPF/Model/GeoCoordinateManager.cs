using System.Collections.ObjectModel;
using System.Device.Location;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.ComponentModel.DataAnnotations;

namespace BingMaps_GPS_WPF.Model
{
    /// <summary>
    /// System.Device.Location.GeoCoordinateWatcherに関する操作およびステート管理を行う
    /// </summary>
    public class GeoCoordinateManager : ValidatableModelBase
    {
        private System.Device.Location.GeoCoordinateWatcher _watcher = null;

        public GeoCoordinateManager()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (_watcher != null)
            {
                _watcher.Stop();

                _watcher.Dispose();
                _watcher = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// The <see cref="Pins" /> property's name.
        /// </summary>
        public const string PinsPropertyName = "Pins";

        private ObservableCollection<Pin> _pins = new ObservableCollection<Pin>();

        /// <summary>
        /// Sets and gets the Pins property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pin> Pins
        {
            get
            {
                return _pins;
            }
        }

        /// <summary>
        /// The <see cref="CurrentPin" /> property's name.
        /// </summary>
        public const string CurrentPinPropertyName = "CurrentPin";

        private Pin _currentPin = null;

        /// <summary>
        /// 現在ピン
        /// </summary>
        public Pin CurrentPin
        {
            get
            {
                return _currentPin;
            }

            set
            {
                if (_currentPin == value)
                {
                    return;
                }

                _currentPin = value;
                RaisePropertyChanged(CurrentPinPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="GPSLogFilePath" /> property's name.
        /// </summary>
        public const string GPSLogFilePathPropertyName = "GPSLogFilePath";

        private string _gpsLogFilePath = string.Empty;

        /// <summary>
        /// Sets and gets the GPSLogFilePath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GPSLogFilePath
        {
            get
            {
                return _gpsLogFilePath;
            }

            set
            {
                if (_gpsLogFilePath == value)
                {
                    return;
                }

                _gpsLogFilePath = value;
                RaisePropertyChanged(GPSLogFilePathPropertyName);
            }
        }

        /// <summary>
        /// GPSログの間隔（メートル）
        /// 検証に成功した値（GPSログ時に適用される値）
        /// </summary>
        private double _valid_MovementThreshold = 50;

        public double Valid_MovementThreshold
        {
            get
            {
                return _valid_MovementThreshold;
            }
        }

        /// <summary>
        /// The <see cref="MovementThreshold" /> property's name.
        /// </summary>
        public const string MovementThresholdPropertyName = "MovementThreshold";

        private double _movementThreshold = 50;

        [Required]
        [Range(10, 1000, ErrorMessage = "{1}-{2}の数字を入力してください。")]
        /// <summary>
        /// GPSログの間隔（メートル）
        /// Sets and gets the MovementThreshold property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double MovementThreshold
        {
            get
            {
                return _movementThreshold;
            }

            set
            {
                if (_movementThreshold == value)
                {
                    return;
                }

                ValidateProperty(value, MovementThresholdPropertyName);

                if (!this.Errors.ContainsKey(MovementThresholdPropertyName))
                {
                    _valid_MovementThreshold = value;
                }

                _movementThreshold = value;
                RaisePropertyChanged(MovementThresholdPropertyName);
            }
        }

        /// <summary>
        /// 現在位置を取得する
        /// </summary>
        public void GetPosition()
        {
            System.Device.Location.GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

            watcher.PositionChanged += watcher_PositionChanged;
            watcher.Start(false);
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            System.Device.Location.GeoCoordinateWatcher watcher = sender as System.Device.Location.GeoCoordinateWatcher;

            // 一回の取得で解除
            watcher.Stop();
            watcher.PositionChanged -= watcher_PositionChanged;

            GeoPosition<GeoCoordinate> geoPosition = e.Position;

            GeoCoordinate coordinate = geoPosition.Location;

            if (coordinate.IsUnknown != true)
            {
                AddPushpin(geoPosition, true);
            }
        }

        private Pin ExistsSamePosition(GeoPosition<GeoCoordinate> geoPosition)
        {
            Pin ret = null;

            var location = new Location(geoPosition.Location.Latitude, geoPosition.Location.Longitude, geoPosition.Location.Altitude);

            foreach (var item in _pins)
            {
                if (item.Location == location)
                {
                    ret = item;
                    break;
                }
            }

            return ret;
        }

        // TODO 下の方法では初回時は取得できない？ため、イベントでの取得方法に変更
        //private void AddPushpin()
        //{
        //    System.Device.Location.GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

        //    watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));

        //    GeoCoordinate coordinate = watcher.Position.Location;

        //    if (coordinate.IsUnknown != true)
        //    {
        //        AddPushpin(watcher.Position);
        //    }
        //}

        private void AddPushpin(GeoPosition<GeoCoordinate> geoPosition, bool removeSamePin)
        {
            // 同一ピンの重複削除
            if (removeSamePin)
            {
                Pin samepin = ExistsSamePosition(geoPosition);
                if (samepin != null)
                {
                    _pins.Remove(samepin);
                }
            }

            GeoCoordinate coordinate = geoPosition.Location;

            Pin pin = new Pin(geoPosition);
            _pins.Add(pin);

            this.CurrentPin = pin;

        }

        //// 外部から重複して開始される危険がある
        //public bool SetLogging(bool start)
        //{
        //    bool ret = false;

        //    if (start)
        //    {
        //        // 開始
        //        _watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

        //        _watcher.MovementThreshold = this.MovementThreshold;
        //        _watcher.PositionChanged += _watcher_PositionChanged;

        //        _watcher.Start(false);

        //        ret = true;
        //    }
        //    else
        //    {
        //        // 終了
        //        _watcher.Stop();

        //        _watcher.Dispose();
        //        _watcher = null;
        //    }

        //    return ret;
        //}


        /// <summary>
        /// The <see cref="GPSLogging" /> property's name.
        /// </summary>
        public const string GPSLoggingPropertyName = "GPSLogging";

        private bool _gpsLogging = false;

        /// <summary>
        /// Sets and gets the GPSLogging property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool GPSLogging
        {
            get
            {
                return _gpsLogging;
            }

            set
            {
                if (_gpsLogging == value)
                {
                    return;
                }

                _gpsLogging = value;
                RaisePropertyChanged(GPSLoggingPropertyName);
            }
        }

        public void StartLogging()
        {
            if (_watcher != null)
            {
                throw new Exception("すでに位置情報の記録は開始されています。");
            }

            // 開始
            _watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

            _watcher.MovementThreshold = this.Valid_MovementThreshold;
            _watcher.PositionChanged += _watcher_PositionChanged;

            _watcher.Start(false);

            this.GPSLogging = true;
        }

        public void StopLogging()
        {
            // 終了
            _watcher.Stop();

            _watcher.Dispose();
            _watcher = null;

            this.GPSLogging = false;
        }

        void _watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            GeoPosition<GeoCoordinate> geoPosition = e.Position;

            GeoCoordinate coordinate = geoPosition.Location;

            if (coordinate.IsUnknown != true)
            {
                AddPushpin(geoPosition, false);

                // ログ出力
                CommonApp.LogGPS(this.GPSLogFilePath, geoPosition);
            }
        }

        public void PointfromString(string line)
        {
            // ログされた位置の復元テスト
            GeoPosition<GeoCoordinate> geoPosition = CommonApp.GetGeoPositionfromString(line);

            AddPushpin(geoPosition, true);

        }

        public void ClearPin()
        {
            _pins.Clear();
            this.CurrentPin = null;
        }

    }
}
