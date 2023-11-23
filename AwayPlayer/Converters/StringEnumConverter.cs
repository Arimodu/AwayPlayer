using IPA.Config.Data;
using IPA.Config.Stores;
using System;

namespace AwayPlayer.Converters
{
    public class StringEnumConverter<T> : ValueConverter<T>
    {
        public override T FromValue(Value value, object parent)
        {
            return (T)Enum.Parse(typeof(T), (value as Text).Value);
        }

        public override Value ToValue(T obj, object parent)
        {
            return Value.Text(Enum.GetName(typeof(T), obj));
        }
    }
}
