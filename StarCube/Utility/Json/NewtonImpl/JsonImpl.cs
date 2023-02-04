using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace StarCube.Utility.Json.NewtonImpl
{
    public class JsonImpl : IJson
    {
        public static readonly JsonImpl Empty = new JsonImpl();

        public readonly JObject json;

        public JsonImpl()
        {
            json = new JObject();
        }

        public JsonImpl(string jsonString)
        {
            json = JObject.Parse(jsonString);
        }

        public JsonImpl(JObject json)
        {
            this.json = json;
        }

        public bool IsValue => false;

        public bool IsArray => false;

        public bool IsJson => true;

        public IJValue AsValue => throw new InvalidCastException();

        public IJArray AsArray => throw new InvalidCastException();

        public IJson AsJson => this;

        public bool TryGetBoolean(string key, out bool value)
        {
            if(json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JValue v && v.Type == JTokenType.Boolean)
            {
                value = (bool)v;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetInt32(string key, out int value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JValue v && v.Type == JTokenType.Integer)
            {
                value = (int)v;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetUInt32(string key, out uint value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JValue v && v.Type == JTokenType.Integer)
            {
                value = (uint)v;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetInt64(string key, out long value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JValue v && v.Type == JTokenType.Integer)
            {
                value = (long)v;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetUInt64(string key, out ulong value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JValue v && v.Type == JTokenType.Integer)
            {
                value = (ulong)v;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetFloat(string key, out float value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JValue v && v.Type == JTokenType.Float)
            {
                value = (float)v;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetDouble(string key, out double value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JValue v && v.Type == JTokenType.Float)
            {
                value = (double)v;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetGuid(string key, out Guid value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JValue v && v.Type == JTokenType.Guid)
            {
                value = (Guid)v;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetString(string key, out string value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JValue v && v.Type == JTokenType.String)
            {
                value = ((string?)v) ?? throw new InvalidCastException();
                return value != null;
            }
            value = string.Empty;
            return false;
        }

        public bool TryGetNode(string key, out IJNode value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token))
            {
                value = FromJToken(token);
                return true;
            }
            value = Empty;
            return false;
        }

        public static IJNode FromJToken(JToken token)
        {
            if (token is JValue value)
            {
                return new JValueImpl(value);
            }
            else if (token is JArray array)
            {
                return new JArrayImpl(array);
            }
            else if (token is JObject json)
            {
                return new JsonImpl(json);
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public bool TryGetValue(string key, out IJValue value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JValue v)
            {
                value = new JValueImpl(v);
                return true;
            }
            value = JValueImpl.Empty;
            return false;
        }

        public bool TryGetArray(string key, out IJArray value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JArray a)
            {
                value = new JArrayImpl(a);
                return true;
            }
            value = JArrayImpl.Empty;
            return false;
        }

        public bool TryGetJson(string key, out IJson value)
        {
            if (json.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken? token) && token is JObject j)
            {
                value = new JsonImpl(j);
                return true;
            }
            value = Empty;
            return false;
        }

        public bool TryAdd(string key, bool value)
        {
            if (json.ContainsKey(key))
            {
                return false;
            }
            json.Add(key, new JValue(value));
            return true;
        }

        public bool TryAdd(string key, int value)
        {
            if (json.ContainsKey(key))
            {
                return false;
            }
            json.Add(key, new JValue(value));
            return true;
        }

        public bool TryAdd(string key, uint value)
        {
            if (json.ContainsKey(key))
            {
                return false;
            }
            json.Add(key, new JValue(value));
            return true;
        }

        public bool TryAdd(string key, long value)
        {
            if (json.ContainsKey(key))
            {
                return false;
            }
            json.Add(key, new JValue(value));
            return true;
        }

        public bool TryAdd(string key, ulong value)
        {
            if (json.ContainsKey(key))
            {
                return false;
            }
            json.Add(key, new JValue(value));
            return true;
        }

        public bool TryAdd(string key, float value)
        {
            if (json.ContainsKey(key))
            {
                return false;
            }
            json.Add(key, new JValue(value));
            return true;
        }

        public bool TryAdd(string key, double value)
        {
            if (json.ContainsKey(key))
            {
                return false;
            }
            json.Add(key, new JValue(value));
            return true;
        }

        public bool TryAdd(string key, Guid value)
        {
            if (json.ContainsKey(key))
            {
                return false;
            }
            json.Add(key, new JValue(value));
            return true;
        }

        public bool TryAdd(string key, string value)
        {
            if (json.ContainsKey(key))
            {
                return false;
            }
            json.Add(key, new JValue(value));
            return true;
        }

        public bool TryAdd(string key, IJNode value)
        {
            if (value is IJValue v)
            {
                return TryAdd(key, v);
            }
            else if (value is IJArray a)
            {
                return TryAdd(key, a);
            }
            else if (value is IJson j)
            {
                return TryAdd(key, j);
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public bool TryAdd(string key, IJValue value)
        {
            if (json.ContainsKey(key) || !(value is JValueImpl v))
            {
                return false;
            }
            json.Add(key, v.value);
            return true;
        }

        public bool TryAdd(string key, IJArray value)
        {
            if (json.ContainsKey(key) || !(value is JArrayImpl a))
            {
                return false;
            }
            json.Add(key, a.array);
            return true;
        }

        public bool TryAdd(string key, IJson value)
        {
            if (json.ContainsKey(key) || !(value is JsonImpl j))
            {
                return false;
            }
            json.Add(key, j.json);
            return true;
        }
    }

    public class JsonFactoryImpl : IJson.IFactory
    {
        public IJson Create()
        {
            return new JsonImpl();
        }

        public IJson Parse(string jsonString)
        {
            return new JsonImpl(jsonString);
        }
    }

    public class JValueImpl : IJValue
    {
        public static readonly JValueImpl Empty = new JValueImpl(new JValue((object?)null));

        public readonly JValue value;

        public JValueImpl(JValue jValue)
        {
            value = jValue;
        }

        public bool IsValue => true;

        public bool IsArray => false;

        public bool IsJson => false;

        public bool IsBoolean => value.Type == JTokenType.Boolean;

        public bool IsInt => value.Type == JTokenType.Integer;

        public bool IsFloat => value.Type == JTokenType.Float;

        public bool IsGuid => value.Type == JTokenType.Guid;

        public bool IsString => value.Type == JTokenType.String;

        public bool AsBoolean => (bool)value;

        public int AsInt32 => (int)value;

        public uint AsUInt32 => (uint)value;

        public long AsInt64 => (long)value;

        public ulong AsUInt64 => (ulong)value;

        public float AsFloat => (float)value;

        public double AsDouble => (double)value;

        public Guid AsGuid => (Guid)value;

        public string AsString => ((string?)value) ?? throw new InvalidCastException();

        public IJValue AsValue => this;

        public IJArray AsArray => throw new InvalidCastException();

        public IJson AsJson => throw new InvalidCastException();
    }

    public class JArrayImpl : IJArray
    {
        public static readonly JArrayImpl Empty = new JArrayImpl(new JArray());

        public readonly JArray array;

        public JArrayImpl(JArray array)
        {
            this.array = array;
        }

        public IJValue AsValue => throw new InvalidCastException();

        public IJArray AsArray => this;

        public IJson AsJson => throw new InvalidCastException();

        public int Length => array.Count;

        public IEnumerable<IJNode> Nodes => from token in array select FromJToken(token);

        private IJNode FromJToken(JToken token)
        {
            if (token is JValue value)
            {
                return new JValueImpl(value);
            }
            else if (token is JArray array)
            {
                return new JArrayImpl(array);
            }
            else if (token is JObject json)
            {
                return new JsonImpl(json);
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public IEnumerable<bool> GetBools()
        {
            bool[] values = new bool[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                if (array[i] is JValue v && v.Type == JTokenType.Boolean)
                {
                    values[i] = (bool)v;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            return values;
        }

        public IEnumerable<int> GetInt32s()
        {
            int[] values = new int[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                if (array[i] is JValue v && v.Type == JTokenType.Integer)
                {
                    values[i] = (int)v;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            return values;
        }

        public IEnumerable<uint> GetUInt32s()
        {
            uint[] values = new uint[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                if (array[i] is JValue v && v.Type == JTokenType.Integer)
                {
                    values[i] = (uint)v;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            return values;
        }

        public IEnumerable<long> GetInt64s()
        {
            long[] values = new long[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                if (array[i] is JValue v && v.Type == JTokenType.Integer)
                {
                    values[i] = (long)v;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            return values;
        }

        public IEnumerable<ulong> GetUInt64s()
        {
            ulong[] values = new ulong[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                if (array[i] is JValue v && v.Type == JTokenType.Integer)
                {
                    values[i] = (ulong)v;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            return values;
        }

        public IEnumerable<float> GetFloats()
        {
            float[] values = new float[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                if (array[i] is JValue v && v.Type == JTokenType.Float)
                {
                    values[i] = (float)v;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            return values;
        }

        public IEnumerable<double> GetDoubles()
        {
            double[] values = new double[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                if (array[i] is JValue v && v.Type == JTokenType.Float)
                {
                    values[i] = (double)v;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            return values;
        }

        public IEnumerable<Guid> GetGuids()
        {
            Guid[] values = new Guid[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                if (array[i] is JValue v && v.Type == JTokenType.Guid)
                {
                    values[i] = (Guid)v;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            return values;
        }

        public IEnumerable<string> GetStrings()
        {
            string[] values = new string[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                if (array[i] is JValue v && v.Type == JTokenType.String)
                {
                    values[i] = ((string?)v) ?? throw new InvalidCastException();
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            return values;
        }

        public bool IsValue => false;

        public bool IsArray => true;

        public bool IsJson => false;

        public void Add(bool value)
        {
            array.Add(new JValue(value));
        }

        public void Add(int value)
        {
            array.Add(new JValue(value));
        }

        public void Add(uint value)
        {
            array.Add(new JValue(value));
        }

        public void Add(long value)
        {
            array.Add(new JValue(value));
        }

        public void Add(ulong value)
        {
            array.Add(new JValue(value));
        }

        public void Add(float value)
        {
            array.Add(new JValue(value));
        }

        public void Add(double value)
        {
            array.Add(new JValue(value));
        }

        public void Add(Guid value)
        {
            array.Add(new JValue(value));
        }

        public void Add(string value)
        {
            array.Add(new JValue(value));
        }

        public void Add(IJNode value)
        {
            if(value.IsValue && value.AsValue is JValueImpl v)
            {
                array.Add(v.value);
            }
            else if (value.IsArray && value.AsArray is JArrayImpl a)
            {
                array.Add(a.array);
            }
            else if (value.IsJson && value.AsJson is JsonImpl j)
            {
                array.Add(j.json);
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public void Add(IJValue value)
        {
            if (!(value is JValueImpl v))
            {
                throw new InvalidCastException();
            }
            array.Add(v.value);
        }

        public void Add(IJArray value)
        {
            if (!(value is JArrayImpl a))
            {
                throw new InvalidCastException();
            }
            array.Add(a.array);
        }

        public void Add(IJson value)
        {
            if (!(value is JsonImpl j))
            {
                throw new InvalidCastException();
            }
            array.Add(j.json);
        }
    }
}
