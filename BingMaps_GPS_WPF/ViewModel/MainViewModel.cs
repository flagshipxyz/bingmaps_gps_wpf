using GalaSoft.MvvmLight.Command;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using BingMaps_GPS_WPF.Model;
using Microsoft.Maps.MapControl.WPF;

using System.ComponentModel.DataAnnotations;

namespace BingMaps_GPS_WPF.ViewModel
{
    // TODO BingMapsに関する操作と状態の管理機能を別クラスに分ける
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ValidatableViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _geoCoordinateManager = new GeoCoordinateManager();
            
            _geoCoordinateManager.PropertyChanged += _geoCoordinateManager_PropertyChanged;
            _geoCoordinateManager.ErrorsChanged += _geoCoordinateManager_ErrorsChanged;
            
            this.Restore();

        }

        /// <summary>
        /// GeoCoordinateManagerでのプロパティ値変更を同期
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _geoCoordinateManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string name = e.PropertyName;

            RaisePropertyChanged(name);

            if (name == GeoCoordinateManager.CurrentPinPropertyName)
            {
                if (_geoCoordinateManager.CurrentPin != null)
                {
                    this.Location = _geoCoordinateManager.CurrentPin.Location;
                }
            }
            else if (name == GeoCoordinateManager.GPSLoggingPropertyName)
            {
                RaisePropertyChanged(GPSLoggingButtonTextPropertyName);            
            }
        }

        /// <summary>
        /// GeoCoordinateManagerでの入力検証結果をマージしてViewに公開
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _geoCoordinateManager_ErrorsChanged(object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            string name = e.PropertyName;

            if (_geoCoordinateManager.Errors.ContainsKey(name))
            {
                List<string> errors = _geoCoordinateManager.Errors[name];

                SetErrors(name, errors);
            }
            else
            {
                ClearErrors(name);
            }

            //System.Collections.Generic.IEnumerable<string> errors = _geoCoordinateManager.GetErrors(name) as System.Collections.Generic.IEnumerable<string>;

            //if (errors != null && errors.Count() > 0)
            //{
            //    SetErrors(name, errors);
            //}
            //else
            //{
            //    ClearErrors(name);
            //}
        }

        public override void Cleanup()
        {
            // Clean up if needed
            this.Save();

            this.Dispose();

            base.Cleanup();
        }

        #region Dispose

        //private ICommand _command_Closed = null;

        //public ICommand Command_Closed
        //{
        //    get
        //    {
        //        if (_command_Closed == null)
        //        {
        //            _command_Closed = new RelayCommand(new Action(Closed));
        //        }
        //        return _command_Closed;
        //    }
        //}

        //private void Closed()
        //{
        //    this.Save();

        //    this.Dispose();
        //}

        private GeoCoordinateManager _geoCoordinateManager = null;

        protected override void Dispose(bool disposing)
        {
            _geoCoordinateManager.Dispose();
            _geoCoordinateManager = null;

            base.Dispose(disposing);
        }

        #endregion

        private void Restore()
        {
            Properties.Settings.Default.Reload();

            // viewmodel
            this.LogPanelWidth = new GridLength(Properties.Settings.Default.LogPanelWidth);
            this.SettingPanelWidth = new GridLength(Properties.Settings.Default.SettingPanelWidth);
            this.StatusVisible = Properties.Settings.Default.StatusVisible;
            this.CredentialsProviderKey = Properties.Settings.Default.CredentialsProviderKey;
            this.Location = _iniLocation = Properties.Settings.Default.IniLocation;
            this.ZoomLevel = Properties.Settings.Default.ZoomLevel;

            // model            
            this.MovementThreshold = Properties.Settings.Default.MovementThreshold;
            this.GPSLogFilePath = Properties.Settings.Default.GPSLogFilePath;
        }

        private void Save()
        {
            // viewmodel
            Properties.Settings.Default.LogPanelWidth = this.LogPanelWidth.Value;
            Properties.Settings.Default.SettingPanelWidth = this.SettingPanelWidth.Value;
            Properties.Settings.Default.StatusVisible = this.StatusVisible;
            Properties.Settings.Default.CredentialsProviderKey = this.CredentialsProviderKey;
            Properties.Settings.Default.IniLocation = _iniLocation;
            Properties.Settings.Default.ZoomLevel = this.ZoomLevel;

            // model
            Properties.Settings.Default.MovementThreshold = this.Valid_MovementThreshold;
            //Properties.Settings.Default.MovementThreshold = this.MovementThreshold;
            Properties.Settings.Default.GPSLogFilePath = this.GPSLogFilePath;

            Properties.Settings.Default.Save();

        }

        /// <summary>
        /// Sets and gets the GPSLogFilePath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GPSLogFilePath
        {
            get
            {
                return _geoCoordinateManager.GPSLogFilePath;
            }

            set
            {
                if (_geoCoordinateManager.GPSLogFilePath == value)
                {
                    return;
                }

                _geoCoordinateManager.GPSLogFilePath = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// GPSログの間隔
        /// 検証に成功した値（GPSログ時に適用される値）
        /// </summary>
        public double Valid_MovementThreshold
        {
            get
            {
                return _geoCoordinateManager.Valid_MovementThreshold;
            }
        }

        /// <summary>
        /// GPSログの間隔
        /// Sets and gets the MovementThreshold property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double MovementThreshold
        {
            get
            {
                return _geoCoordinateManager.MovementThreshold;
            }

            set
            {
                if (_geoCoordinateManager.MovementThreshold == value)
                {
                    return;
                }

                _geoCoordinateManager.MovementThreshold = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Pin> Pins
        {
            get
            {
                return _geoCoordinateManager.Pins;
            }
        }

        public Pin CurrentPin
        {
            get
            {
                return _geoCoordinateManager.CurrentPin;
            }
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
                if (value == null)
                {
                    return;
                }

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

        private void GetPosition()
        {
            _geoCoordinateManager.GetPosition();
        }


        #endregion


        #region GPSLogging

        private ICommand _command_SetGPSLogging = null;

        public ICommand Command_SetGPSLogging
        {
            get
            {
                if (_command_SetGPSLogging == null)
                {
                    _command_SetGPSLogging = new RelayCommand(new Action(SetGPSLogging));
                }
                return _command_SetGPSLogging;
            }
        }

        private void SetGPSLogging()
        {
            if (!_geoCoordinateManager.GPSLogging)
            {
                _geoCoordinateManager.StartLogging();
            }
            else
            {
                _geoCoordinateManager.StopLogging();            
            }
        }

        public bool GPSLogging
        {
            get
            {
                return _geoCoordinateManager.GPSLogging;
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
                if (this.GPSLogging)
                {
                    _gpsLoggingButtonText = Consts.c_StopLogging;
                }
                else
                {
                    _gpsLoggingButtonText = Consts.c_StartLogging;
                }

                return _gpsLoggingButtonText;
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
            string path = CommonApp.GetOpenLogFilePath();
            
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

                _geoCoordinateManager.PointfromString(line);
            }
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
            _geoCoordinateManager.ClearPin();
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

        private ICommand _command_ChangeSettingPanelVisible = null;

        public ICommand Command_ChangeSettingPanelVisible
        {
            get
            {
                if (_command_ChangeSettingPanelVisible == null)
                {
                    _command_ChangeSettingPanelVisible = new RelayCommand(new Action(ChangeSettingPanelVisible));
                }
                return _command_ChangeSettingPanelVisible;
            }
        }

        private void ChangeSettingPanelVisible()
        {
            this.SettingPanelVisible = !this.SettingPanelVisible;
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
            string path = CommonApp.GetSaveLogFilePath();

            if (Path.IsPathRooted(path))
            {
                this.GPSLogFilePath = path;
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


        #region InitialLocation

        private Location _iniLocation = null;
        private Location _temp_IniLocation = null;

        private ICommand _command_SetInitialLocation = null;

        public ICommand Command_SetInitialLocation
        {
            get
            {
                if (_command_SetInitialLocation == null)
                {
                    _command_SetInitialLocation = new RelayCommand(new Action(SetInitialLocation));
                }
                return _command_SetInitialLocation;
            }
        }

        /// <summary>
        /// 初期開始位置の取得
        /// </summary>
        private void SetInitialLocation()
        {
            _iniLocation = _temp_IniLocation;
        }

        //Set InitialLocation
        private ICommand _command_SetTempInitialLocation = null;

        public ICommand Command_SetTempInitialLocation
        {
            get
            {
                if (_command_SetTempInitialLocation == null)
                {
                    _command_SetTempInitialLocation = 
                        new RelayCommand<System.Windows.Input.MouseButtonEventArgs>
                            (new Action<System.Windows.Input.MouseButtonEventArgs>(SetTempInitialLocation));
                }
                return _command_SetTempInitialLocation;
            }
        }

        /// <summary>
        /// 初期開始位置の仮取得
        /// </summary>
        /// <param name="e"></param>
        private void SetTempInitialLocation(System.Windows.Input.MouseButtonEventArgs e)
        {
            Map map = (Map)e.Source;
            //Map map = (Map)sender;

            Point pt = e.GetPosition(map);

            Location loc = null;
            if (map.TryViewportPointToLocation(pt, out loc))
            {
                _temp_IniLocation = loc;
            }
        }


        #endregion




    }
}