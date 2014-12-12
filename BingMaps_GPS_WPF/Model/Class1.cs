using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingMaps_GPS_WPF.Model
{
    public class Class1 : GalaSoft.MvvmLight.ObservableObject
    {
        public Class1()
        {
            this.MyProperty = _count.ToString();
        }

        /// <summary>
        /// The <see cref="MyProperty" /> property's name.
        /// </summary>
        public const string MyPropertyPropertyName = "MyProperty";

        private string _myProperty = string.Empty;

        /// <summary>
        /// Sets and gets the MyProperty property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string MyProperty
        {
            get
            {
                return _myProperty;
            }

            set
            {
                if (_myProperty == value)
                {
                    return;
                }

                _myProperty = value;
                RaisePropertyChanged(MyPropertyPropertyName);
            }
        }

        private int _count = 0;
        public void Test()
        {
            _count++;

            this.MyProperty = _count.ToString();
        }
    }
}
