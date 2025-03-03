﻿using System.ComponentModel;
using System.Globalization;
using System.Numerics;

namespace XREngine.Data.Core.TypeConverters
{
    public class Vector3TypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string str)
            {
                string[] parts = str.Split(',');
                if (parts.Length == 3)
                    return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Vector3 v)
                return $"{v.X}, {v.Y}, {v.Z}";
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
