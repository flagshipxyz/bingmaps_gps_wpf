using GalaSoft.MvvmLight.Command;

using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using BingMaps_GPS_WPF.Model;
using Microsoft.Win32;
using Microsoft.Maps.MapControl.WPF;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BingMaps_GPS_WPF.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : DisposableViewModelBase, INotifyDataErrorInfo
    {
        private readonly IDataService _dataService;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }
                });

            this.Restore();

        }

        //public override void Cleanup()
        //{
        //    // Clean up if needed

        //    base.Cleanup();
        //}

        #region Dispose

        private ICommand _command_Closed = null;

        public ICommand Command_Closed
        {
            get
            {
                if (_command_Closed == null)
                {
                    _command_Closed = new RelayCommand(new Action(Closed));
                }
                return _command_Closed;
            }
        }

        private void Closed()
        {
            this.Save();

            this.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (_watcher != null)
            {
                _watcher.PositionChanged -= _watcher_PositionChanged;
                _watcher.Dispose();
                _watcher = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        private void Restore()
        {
            Properties.Settings.Default.Reload();

            this.LogPanelWidth = new GridLength(Properties.Settings.Default.LogPanelWidth);
            this.SettingPanelWidth = new GridLength(Properties.Settings.Default.SettingPanelWidth);

            this.StatusVisible = Properties.Settings.Default.StatusVisible;

            this.CredentialsProviderKey = Properties.Settings.Default.CredentialsProviderKey;
            this.MovementThreshold = Properties.Settings.Default.MovementThreshold;
            this.MovementThresholdTemp = this.MovementThreshold;

            this.GPSLogFilePath = Properties.Settings.Default.GPSLogFilePath;
        }

        private void Save()
        {
            Properties.Settings.Default.LogPanelWidth = this.LogPanelWidth.Value;
            Properties.Settings.Default.SettingPanelWidth = this.SettingPanelWidth.Value;

            Properties.Settings.Default.StatusVisible = this.StatusVisible;

            Properties.Settings.Default.CredentialsProviderKey = this.CredentialsProviderKey;
            Properties.Settings.Default.MovementThreshold = this.MovementThreshold;

            Properties.Settings.Default.GPSLogFilePath = this.GPSLogFilePath;

            Properties.Settings.Default.Save();

        }

        /// <summary>
        /// The <see cref="Location" /> property's name.
        /// </summary>
        public const string LocationPropertyName = "Location";
        /// <summary>
        /// 大阪駅
        /// </summary>
        private Location _location = new Location(34.7012, 135.496);

        /// <summary>
        /// Mapの位置
        /// ≠現在ピンの位置
        /// 現在ピンのないときや、現在ピンとは異なる位置を示すこともある
        /// </summary>
        public Location Location
        {
            get
            {
                return _location;
            }

            set
            {
                if (_location == value)
                {
                    return;
                }

                _location = value;
                RaisePropertyChanged(LocationPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ZoomLevel" /> property's name.
        /// </summary>
        public const string ZoomLevelPropertyName = "ZoomLevel";

        private double _zoomLevel = 15;

        /// <summary>
        /// Sets and gets the ZoomLevel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double ZoomLevel
        {
            get
            {
                return _zoomLevel;
            }

            set
            {
                if (_zoomLevel == value)
                {
                    return;
                }

                _zoomLevel = value;
                RaisePropertyChanged(ZoomLevelPropertyName);
            }
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

            set
            {
                if (_pins == value)
                {
                    return;
                }
                _pins = value;
                RaisePropertyChanged(PinsPropertyName);
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

        #region Get Position

        private ICommand _command_GetPosition = null;

        public ICommand Command_GetPosition
        {
            get
            {
                if (_command_GetPosition == null)
                {
                    _command_GetPosition = new RelayCommand(new Action(GetPosition));
                }
                return _command_GetPosition;
            }
        }

        /// <summary>
        /// 現在位置を取得する
        /// </summary>
        private void GetPosition()
        {
            System.Device.Location.GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

            watcher.PositionChanged += watcher_PositionChanged;
            watcher.Start(false);
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            System.Device.Location.GeoCoordinateWatcher watcher = sender as System.Device.Location.GeoCoordinateWatcher;

            // 一回の取得で解除
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

        // 下の方法では初回時は取得できないため、イベントでの取得方法に変更
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
            this.Location = pin.Location;
        }


        #endregion


        #region Logging

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

                if (_gpsLogging)
                {
                    this.GPSLoggingButtonText = Consts.c_StopLogging;
                }
                else
                {
                    this.GPSLoggingButtonText = Consts.c_StartLogging;
                }

                SetLogging(_gpsLogging);

                RaisePropertyChanged(GPSLoggingPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="GPSLoggingButtonText" /> property's name.
        /// </summary>
        public const string GPSLoggingButtonTextPropertyName = "GPSLoggingButtonText";

        private string _gpsLoggingButtonText = Consts.c_StartLogging;

        /// <summary>
        /// Sets and gets the GPSLoggingButtonText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GPSLoggingButtonText
        {
            get
            {
                return _gpsLoggingButtonText;
            }

            set
            {
                if (_gpsLoggingButtonText == value)
                {
                    return;
                }

                _gpsLoggingButtonText = value;
                RaisePropertyChanged(GPSLoggingButtonTextPropertyName);
            }
        }

        private System.Device.Location.GeoCoordinateWatcher _watcher = null;

        private bool SetLogging(bool start)
        {
            bool ret = false;

            if (start)
            {
                // 開始

                _watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

                _watcher.MovementThreshold = this.MovementThreshold;
                _watcher.PositionChanged += _watcher_PositionChanged;
                _watcher.Start(false);

                ret = true;
            }
            else
            {
                // 終了
                _watcher.PositionChanged -= _watcher_PositionChanged;
                _watcher.Dispose();
                _watcher = null;
            }

            return ret;
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

        #endregion


        #region Log

        private ICommand _command_OpenLogFile = null;

        public ICommand Command_OpenLogFile
        {
            get
            {
                if (_command_OpenLogFile == null)
                {
                    _command_OpenLogFile = new RelayCommand(new Action(OpenLogFile));
                }
                return _command_OpenLogFile;
            }
        }

        private void OpenLogFile()
        {
            // ファイル選択
            string path = GetOpenFilePath();

            if (!File.Exists(path))
                return;

            // ファイルをリストに展開
            string text = File.ReadAllText(path);

            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            if (lines != null && lines.Length > 0)
            {
                this.LogList = new ObservableCollection<string>(lines);

                this.LogPanelVisible = true;
            }
        }

        string GetOpenFilePath()
        {
            string path = string.Empty;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "GPSログファイルの選択";
            dialog.FileName = "";
            dialog.Filter = "GPSログファイル|*.log";
            //dialog.DefaultExt = "*.log";

            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
            }

            return path;
        }

        /// <summary>
        /// The <see cref="LogList" /> property's name.
        /// </summary>
        public const string LogListPropertyName = "LogList";

        private ObservableCollection<string> _logList = null;

        /// <summary>
        /// Sets and gets the LogList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> LogList
        {
            get
            {
                return _logList;
            }

            set
            {
                if (_logList == value)
                {
                    return;
                }

                _logList = value;
                RaisePropertyChanged(LogListPropertyName);
            }
        }

        private ICommand _command_LogListBox_SelectionChanged = null;

        public ICommand Command_LogListBox_SelectionChanged
        {
            get
            {
                if (_command_LogListBox_SelectionChanged == null)
                {
                    _command_LogListBox_SelectionChanged = new RelayCommand<System.Windows.Controls.SelectionChangedEventArgs>
                        (new Action<System.Windows.Controls.SelectionChangedEventArgs>(LogListBox_SelectionChanged));
                }
                return _command_LogListBox_SelectionChanged;
            }
        }

        private void LogListBox_SelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e)
        {
            IList<object> obj_lines = (IList<object>)e.AddedItems;

            if (obj_lines != null && obj_lines.Count > 0)
            {
                object obj_line = obj_lines[0];

                string line = obj_line as string;

                PointfromString(line);
            }
        }

        private void PointfromString(string line)
        {
            // ログされた位置の復元テスト
            string[] parts = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            string str_time = parts[0];
            string str_lat = parts[1];
            string str_lon = parts[2];
            string str_alt = parts[3];
            string str_accuracy = parts[4];
            string str_altaccuracy = parts[5];

            long long_filetime = 0;
            double lat = 0;
            double lon = 0;
            double alt = 0;
            double accuracy = 0;
            double altaccuracy = 0;

            long.TryParse(str_time, out long_filetime);
            double.TryParse(str_lat, out lat);
            double.TryParse(str_lon, out lon);
            double.TryParse(str_alt, out alt);
            double.TryParse(str_accuracy, out accuracy);
            double.TryParse(str_altaccuracy, out altaccuracy);

            DateTimeOffset time = DateTimeOffset.FromFileTime(long_filetime);

            var location = new Location(lat, lon, alt);

            GeoCoordinate geoCoordinate = new GeoCoordinate(lat, lon, alt);
            GeoPosition<GeoCoordinate> geoPosition = new GeoPosition<GeoCoordinate>(time, geoCoordinate);

            AddPushpin(geoPosition, true);

        }

        #endregion


        #region Show Status

        private ICommand _command_ChangeStatusVisible = null;

        public ICommand Command_ChangeStatusVisible
        {
            get
            {
                if (_command_ChangeStatusVisible == null)
                {
                    _command_ChangeStatusVisible = new RelayCommand(new Action(ChangeStatusVisible));
                }
                return _command_ChangeStatusVisible;
            }
        }

        private void ChangeStatusVisible()
        {
            this.StatusVisible = !this.StatusVisible;
        }

        /// <summary>
        /// The <see cref="StatusVisible" /> property's name.
        /// </summary>
        public const string StatusVisiblePropertyName = "StatusVisible";

        private bool _statusVisible = false;

        /// <summary>
        /// Sets and gets the StatusVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool StatusVisible
        {
            get
            {
                return _statusVisible;
            }

            set
            {
                if (_statusVisible == value)
                {
                    return;
                }

                _statusVisible = value;

                if (_statusVisible)
                {
                    this.StatusVisibility = Visibility.Visible;
                }
                else
                {
                    this.StatusVisibility = Visibility.Collapsed;
                }

                RaisePropertyChanged(StatusVisiblePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="StatusVisibility" /> property's name.
        /// </summary>
        public const string StatusVisibilityPropertyName = "StatusVisibility";

        private Visibility _statusVisibility = Visibility.Collapsed;

        /// <summary>
        /// Sets and gets the StatusVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility StatusVisibility
        {
            get
            {
                return _statusVisibility;
            }

            set
            {
                if (_statusVisibility == value)
                {
                    return;
                }

                _statusVisibility = value;
                RaisePropertyChanged(StatusVisibilityPropertyName);
            }
        }


        #endregion

        #region Clear Pin

        private ICommand _command_ClearPin = null;

        public ICommand Command_ClearPin
        {
            get
            {
                if (_command_ClearPin == null)
                {
                    _command_ClearPin = new RelayCommand(new Action(ClearPin));
                }
                return _command_ClearPin;
            }
        }

        private void ClearPin()
        {
            _pins.Clear();
            this.CurrentPin = null;
        }

        #endregion

        #region Log Panel

        private ICommand _command_ChangeLogPanelVisible = null;

        public ICommand Command_ChangeLogPanelVisible
        {
            get
            {
                if (_command_ChangeLogPanelVisible == null)
                {
                    _command_ChangeLogPanelVisible = new RelayCommand(new Action(ChangeLogPanelVisible));
                }
                return _command_ChangeLogPanelVisible;
            }
        }

        private void ChangeLogPanelVisible()
        {
            this.LogPanelVisible = !this.LogPanelVisible;
        }

        /// <summary>
        /// The <see cref="LogPanelVisible" /> property's name.
        /// </summary>
        public const string LogPanelVisiblePropertyName = "LogPanelVisible";

        private bool _logPanelVisible = false;

        /// <summary>
        /// Sets and gets the LogPanelVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool LogPanelVisible
        {
            get
            {
                return _logPanelVisible;
            }

            set
            {
                if (_logPanelVisible == value)
                {
                    return;
                }

                _logPanelVisible = value;

                RaisePropertyChanged(LogPanelVisiblePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LogPanelWidth" /> property's name.
        /// </summary>
        public const string LogPanelWidthPropertyName = "LogPanelWidth";

        private GridLength _logPanelWidth = new GridLength(200);

        /// <summary>
        /// Sets and gets the LogPanelWidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GridLength LogPanelWidth
        {
            get
            {
                return _logPanelWidth;
            }

            set
            {
                if (_logPanelWidth == value)
                {
                    return;
                }

                _logPanelWidth = value;
                RaisePropertyChanged(LogPanelWidthPropertyName);
            }
        }

        #endregion

        #region Setting Panel

        /// <summary>
        /// The <see cref="SettingPanelVisible" /> property's name.
        /// </summary>
        public const string SettingPanelVisiblePropertyName = "SettingPanelVisible";

        private bool _settingPanelVisible = false;

        /// <summary>
        /// Sets and gets the SettingPanelVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool SettingPanelVisible
        {
            get
            {
                return _settingPanelVisible;
            }

            set
            {
                if (_settingPanelVisible == value)
                {
                    return;
                }

                _settingPanelVisible = value;
                RaisePropertyChanged(SettingPanelVisiblePropertyName);

            }
        }

        /// <summary>
        /// The <see cref="SettingPanelWidth" /> property's name.
        /// </summary>
        public const string SettingPanelWidthPropertyName = "SettingPanelWidth";

        private GridLength _settingPanelWidth = new GridLength(200);

        /// <summary>
        /// Sets and gets the SettingPanelWidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public GridLength SettingPanelWidth
        {
            get
            {
                return _settingPanelWidth;
            }

            set
            {
                if (_settingPanelWidth == value)
                {
                    return;
                }

                _settingPanelWidth = value;
                RaisePropertyChanged(SettingPanelWidthPropertyName);
            }
        }

        #endregion

        #region CredentialsProvider

        /// <summary>
        /// The <see cref="CredentialsProviderKey" /> property's name.
        /// </summary>
        public const string CredentialsProviderKeyPropertyName = "CredentialsProviderKey";

        private string _credentialsProviderKey = string.Empty;

        /// <summary>
        /// Sets and gets the CredentialsProviderKey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string CredentialsProviderKey
        {
            get
            {
                return _credentialsProviderKey;
            }

            set
            {
                if (_credentialsProviderKey == value)
                {
                    return;
                }

                _credentialsProviderKey = value;

                this.CredentialsProvider = new ApplicationIdCredentialsProvider(_credentialsProviderKey);

                RaisePropertyChanged(CredentialsProviderKeyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CredentialsProvider" /> property's name.
        /// </summary>
        public const string CredentialsProviderPropertyName = "CredentialsProvider";

        private ApplicationIdCredentialsProvider _credentialsProvider = null;

        /// <summary>
        /// Sets and gets the CredentialsProvider property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ApplicationIdCredentialsProvider CredentialsProvider
        {
            get
            {
                return _credentialsProvider;
            }

            set
            {
                if (_credentialsProvider == value)
                {
                    return;
                }

                _credentialsProvider = value;
                RaisePropertyChanged(CredentialsProviderPropertyName);
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

        private ICommand _command_SetLogFilePath = null;

        public ICommand Command_SetLogFilePath
        {
            get
            {
                if (_command_SetLogFilePath == null)
                {
                    _command_SetLogFilePath = new RelayCommand(new Action(SetLogFilePath));
                }
                return _command_SetLogFilePath;
            }
        }

        void SetLogFilePath()
        {
            string path = GetLogFilePath();

            if (Path.IsPathRooted(path))
            {
                this.GPSLogFilePath = path;
            }
        }

        private string GetLogFilePath()
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


        #endregion


        #region MovementThreshold

        /// <summary>
        /// The <see cref="MovementThresholdTemp" /> property's name.
        /// </summary>
        public const string MovementThresholdTempPropertyName = "MovementThresholdTemp";

        private object _movementThresholdTemp = 50;

        /// <summary>
        /// GPSログを出力する間隔（距離）
        /// 入力検証用
        /// Sets and gets the MovementThresholdTemp property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [Required(ErrorMessage = "10-1000の数字を入力してください。")]
        [Range(10, 1000,
                ErrorMessage = "{1}-{2}の数字を入力してください。")]
        public object MovementThresholdTemp
        {
            get
            {
                return _movementThresholdTemp;
            }

            set
            {
                if (_movementThresholdTemp == value)
                {
                    return;
                }

                _movementThresholdTemp = value;
                RaisePropertyChanged(MovementThresholdTempPropertyName);

                ValidateProperty(value, MovementThresholdTempPropertyName);


                var error = _errors.ContainsKey(MovementThresholdTempPropertyName);
                if (!error)
                {
                    double d = this.MovementThreshold;
                    if (double.TryParse(_movementThresholdTemp.ToString(), out d))
                    {
                        this.MovementThreshold = d;
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="MovementThreshold" /> property's name.
        /// </summary>
        public const string MovementThresholdPropertyName = "MovementThreshold";

        private double _movementThreshold = 50;

        /// <summary>
        /// GPSログを出力する間隔（距離）
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

                _movementThreshold = value;
                RaisePropertyChanged(MovementThresholdPropertyName);
            }
        }


        #endregion


        #region GridSplitter

        private ICommand _command_HideLogPanel = null;

        public ICommand Command_HideLogPanel
        {
            get
            {
                if (_command_HideLogPanel == null)
                {
                    _command_HideLogPanel = new RelayCommand(new Action(HideLogPanel));
                }
                return _command_HideLogPanel;
            }
        }

        private void HideLogPanel()
        {
            this.LogPanelVisible = false;
        }

        private ICommand _command_HideSettingPanel = null;

        public ICommand Command_HideSettingPanel
        {
            get
            {
                if (_command_HideSettingPanel == null)
                {
                    _command_HideSettingPanel = new RelayCommand(new Action(HideSettingPanel));
                }
                return _command_HideSettingPanel;
            }
        }

        private void HideSettingPanel()
        {
            this.SettingPanelVisible = false;
        }

        #endregion


        #region ValidateProperty

        protected void ValidateProperty(object value, [CallerMemberName]string propertyName = null)
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var validationErrors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            if (!Validator.TryValidateProperty(value, context, validationErrors))
            {
                var errors = validationErrors.Select(error => error.ErrorMessage);
                SetErrors(propertyName, errors);
            }
            else
            {
                ClearErrors(propertyName);
            }
        }

        readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        protected void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            var existserror = _errors.ContainsKey(propertyName);
            var existsnewerror = errors != null && errors.Count() > 0;

            if (!existserror && !existsnewerror)
                return;

            if (existsnewerror)
            {
                _errors[propertyName] = new List<string>(errors);
            }
            else
            {
                _errors.Remove(propertyName);
            }
        }

        protected void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }

        private void OnErrorsChanged(string propertyName)
        {
            var h = this.ErrorsChanged;
            if (h != null)
            {
                h(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyDataErrorInfo

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) ||
                !_errors.ContainsKey(propertyName))
                return null;

            return _errors[propertyName];
        }

        public bool HasErrors
        {
            get { return _errors.Count > 0; }
        }

        #endregion

    }
}