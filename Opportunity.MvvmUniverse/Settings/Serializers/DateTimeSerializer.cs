﻿using System;

namespace Opportunity.MvvmUniverse.Settings.Serializers
{
    public sealed class DateTimeSerializer : ISerializer<DateTime>
    {
        public DateTime Deserialize(object value) => DateTime.FromBinary((long)value);
        public object Serialize(DateTime value) => value.ToBinary();
    }
}
