using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Shared
{
    public class SearchSchema
    {
        public string ObjectId { get; set; }

        public string HierarchyLvl0 { get; set; }

        public string HierarchyLvl1 { get; set; }

        public string HierarchyLvl2 { get; set; }

        public string HierarchyLvl3 { get; set; }

        public string HierarchyLvl4 { get; set; }

        public string HierarchyLvl5 { get; set; }

        public string Content { get; set; }

        public string Url { get; set; }

        public List<string> Tags { get; set; }
    }

    public class SearchResult<T>
    {
        public List<T> hits { get; set; }

        public string query { get; set; }

        public static SearchResult<T> Parse(string json)
        {
            return JsonConvert.DeserializeObject<SearchResult<T>>(json);
        }
    }
}