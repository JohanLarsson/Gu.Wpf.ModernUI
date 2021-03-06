﻿namespace Gu.Wpf.ModernUI
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;

    /// <summary>Converts string values to lower case.</summary>
    [MarkupExtensionReturnType(typeof(ToLowerConverter))]
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class ToLowerConverter : MarkupExtension, IValueConverter
    {
        /// <summary>For use in xaml via {x:Static mui:ToLowerConverter.Default}</summary>
        public static readonly ToLowerConverter Default = new ToLowerConverter();

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (!(value is string) && Is.InDesignMode)
            {
                throw new ArgumentException($"Expected value to be of type string. Was {value} of type {value.GetType().PrettyName()} ({value})");
            }

            var text = value as string;
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.ToLowerInvariant();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Is.InDesignMode)
            {
                throw new NotSupportedException($"{this.GetType().Name} does not support use in bindings with Mode = TwoWay.");
            }

            return Binding.DoNothing;
        }
    }
}
