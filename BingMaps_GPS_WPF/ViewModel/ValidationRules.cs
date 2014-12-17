using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BingMaps_GPS_WPF.ViewModel
{
    // TODO 保留
    // Required
    public class RequiredDoubleValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            bool ret = false;

            double val;
            if (!string.IsNullOrEmpty((string)value))
            {
                if (double.TryParse(value.ToString(), out val))
                {
                    ret = true;
                }
            }

            if (!ret)
            {
                //return new ValidationResult(false, "");
                return new ValidationResult(false, "数字を入力してください。");
            }

            return new ValidationResult(true, null);
        }
    }

    // nullable
    public class ValidationRules : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            double val;
            if (!string.IsNullOrEmpty((string)value))
            {
                if (!double.TryParse(value.ToString(), out val))
                {
                    return new ValidationResult(false, "数字を入力してください。");
                }
            }
            return new ValidationResult(true, null);
        }
    }
}
