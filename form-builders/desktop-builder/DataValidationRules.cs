using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace desktop_builder
{
    public class BaseValidationRule : ValidationRule
    {
        public ValidationResult ValidateControl(TextBox control)
        {
            var result = ValidateThis(control);
            return showValidationResult(result, control);
        }

        public ValidationResult ValidateControl(TextBox minimum, TextBox maximum)
        {
            var result = ValidateThis(minimum, maximum);
            showValidationResult(result, minimum);
            showValidationResult(result, maximum);
            return result;
        }

        public ValidationResult showValidationResult(ValidationResult result, TextBox control)
        {
            var expression = control.GetBindingExpression(TextBox.TextProperty);
            if (result.IsValid)
            {
                Validation.ClearInvalid(expression);
            }
            else
            {
                var error = new ValidationError(this, expression, result.ErrorContent, null);
                Validation.MarkInvalid(expression, error);
            }
            return result;
        }

        protected virtual ValidationResult ValidateThis(TextBox control)
        {
            return null;
        }

        protected virtual ValidationResult ValidateThis(TextBox minimum, TextBox maximum)
        {
            return null;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return new ValidationResult(false, "Override this");
        }

        internal long getValueDate(TextBox control)
        {
            var text = string.IsNullOrWhiteSpace(control.Text) ? "" : control.Text;

            if (text == Constants.TODAY)
                return Constants.ATTODAYDATECONSTANT;

            if (string.IsNullOrWhiteSpace(text))
                return Constants.NULL_NUM;

            var parts = new string[3];
            if (Regex.IsMatch(text, @"^\d{8}$"))
            {
                parts[0] = text.Substring(0, 4);
                parts[1] = text.Substring(4, 2);
                parts[2] = text.Substring(6, 2);
            }
            else if (Regex.IsMatch(text, @"^\d{4}-\d{2}-\d{2}$"))
            {
                parts = Regex.Split(text, "-");
                if (parts.Length != 3)
                    return Constants.ERROR_NUM;

                text = Regex.Replace(text, "-", "");
            }
            else
            {
                return Constants.ERROR_NUM;
            }

            var validates = false;
            try
            {
                var date = new DateTime(parts[0].toInt(),
                    parts[1].toInt(), parts[2].toInt());
                validates = true;
            }
            catch
            {
                validates = false;
            }

            if (!validates)
                return Constants.ERROR_NUM;

            var value = long.Parse(text);

            if (value < Constants.MINDATE || value > Constants.MAXDATE)
            {
                return Constants.ERROR_NUM;
            }
            return value;
        }

        protected long getValueLong(TextBox control)
        {
            var value = string.IsNullOrWhiteSpace(control.Text) ? "" : control.Text;
            long toReturn = Constants.NULL_NUM;
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (!long.TryParse(value, out toReturn))
                {
                    toReturn = Constants.ERROR_NUM;
                }
            }
            return toReturn;
        }
    }

    public class MultiLineValidationRules : BaseValidationRule
    {
        protected override ValidationResult ValidateThis(TextBox control)
        {
            var result = new ValidationResult(true, null);
            var value = getValueMultiline(control);
            if (value.Count == 0)
            {
                result = new ValidationResult(false, "No choices specified");
            }
            return result;
        }

        public List<string> getValueMultiline(TextBox control)
        {
            var toReturn = new List<string>();
            var lineCount = control.LineCount;
            for (var i = 0; i < lineCount; i++)
            {
                if (control.GetLineLength(i) > 0)
                {
                    var txt = control.GetLineText(i)
                        .Replace("\r\n", string.Empty)
                        .Replace("\r", string.Empty)
                        .Replace("\n", string.Empty);
                    toReturn.Add(txt);
                }
            }
            return toReturn;
        }
    }

    public class IntegerValidationRules : BaseValidationRule
    {
        protected override ValidationResult ValidateThis(TextBox control)
        {
            var result = new ValidationResult(true, null);
            var value = getValueLong(control);
            if (value == Constants.ERROR_NUM || value > int.MaxValue || value < int.MinValue)
            {
                result = new ValidationResult(false, "Not a valid number");
            }
            return result;
        }

        public int getValue(TextBox control)
        {
            return (int)getValueLong(control);
        }
    }

    public class TextRequiredValidationRules : BaseValidationRule
    {
        protected override ValidationResult ValidateThis(TextBox control)
        {
            var result = new ValidationResult(true, null);
            var value = getValue(control);
            if (string.IsNullOrWhiteSpace(value))
            {
                result = new ValidationResult(false, "Value required");
            }
            return result;
        }

        public string getValue(TextBox control)
        {
            return string.IsNullOrWhiteSpace(control.Text) ? "" : control.Text;
        }
    }

    public class GreaterThanZeroValidationRules : BaseValidationRule
    {
        protected override ValidationResult ValidateThis(TextBox control)
        {
            var result = new ValidationResult(true, null);
            var value = getValueLong(control);
            if (value < 0)
            {
                result = new ValidationResult(false, "Value should be greater than zero");
            }

            return result;
        }

        public long getValue(TextBox control)
        {
            return getValueLong(control);
        }
    }

    public class MinMaxValidationRules : BaseValidationRule
    {
        protected override ValidationResult ValidateThis(TextBox minimum, TextBox maximum)
        {
            var result = new ValidationResult(true, null);
            var value1 = getValueLong(minimum);
            var value2 = getValueLong(maximum);
            if (value1 == Constants.ERROR_NUM || value2 == Constants.ERROR_NUM)
            {
                result = new ValidationResult(false, "Not a valid number");
            }

            if (value1 == Constants.NULL_NUM && value2 == Constants.NULL_NUM)
            {
                //ok
            }
            else if (value1 == Constants.NULL_NUM || value2 == Constants.NULL_NUM)
            {
                result = new ValidationResult(false, "Not all values specified");
            }
            else if (value1 > value2)
            {
                result = new ValidationResult(false, "Values are not logical");
            }
            return result;
        }

        internal minmaxConstraints getValues(TextBox minimum, TextBox maximum)
        {
            return new minmaxConstraints() {
                minimum = getValueLong(minimum),
                maximum = getValueLong(maximum) };
        }
    }

    public class DateValidationRules : BaseValidationRule
    {
        protected override ValidationResult ValidateThis(TextBox control)
        {
            var result = new ValidationResult(true, null);
            var value = getValueDate(control);
            if (value == Constants.ERROR_NUM || (value != Constants.NULL_NUM && 
                (value > Constants.MAXDATE || value < Constants.MINDATE)))
            {
                result = new ValidationResult(false, "Not a valid date");
            }
            return result;
        }

        public long getValue(TextBox control)
        {
            return getValueLong(control);
        }
    }

    public class DateMinMaxValidationRules : BaseValidationRule
    {
        protected override ValidationResult ValidateThis(TextBox minimum, TextBox maximum)
        {
            var result = new ValidationResult(true, null);
            var value1 = getValueDate(minimum);
            var value2 = getValueDate(maximum);
            if (value1 == Constants.ERROR_NUM || value2 == Constants.ERROR_NUM)
            {
                result = new ValidationResult(false, "Not a valid date");
            }

            if (value1 == Constants.NULL_NUM && value2 == Constants.NULL_NUM)
            {
                //ok
            }
            else if (value1 == Constants.NULL_NUM || value2 == Constants.NULL_NUM)
            {
                result = new ValidationResult(false, "Not all values specified");
            }
            else if (value1 > value2)
            {
                result = new ValidationResult(false, "Values are not logical");
            }
            return result;
        }

        internal minmaxConstraints getValues(TextBox minimum, TextBox maximum)
        {
            return new minmaxConstraints()
            {
                minimum = getValueDate(minimum),
                maximum = getValueDate(maximum)
            };
        }

        bool validatesAsDateLong(string textLong)
        {
            textLong = textLong.Trim();
            var t =
                Regex.IsMatch(textLong, @"^\d{8}$") ||
                Regex.IsMatch(textLong, @"^\d{4}-\d{2}-\d{2}$");
            return t;
        }
    }
}
