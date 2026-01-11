using System;
using UnityEngine;

namespace Helpers
{
    public static class JsonArrayWrapper<T>
    {
        [Serializable]
        public class Wrapper
        {
            public T[] items = Array.Empty<T>();
        }

        public static Wrapper FromJson(string json)
        {
            string fixedJson = $"{{\"items\":{json}}}";
            return JsonUtility.FromJson<Wrapper>(fixedJson);
        }
    }
}