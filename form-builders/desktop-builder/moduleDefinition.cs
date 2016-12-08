using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace desktop_builder
{
    class moduleBaseDefinition
    {
        public string id { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public string description { get; set; }
    }

    class moduleDefinition: moduleBaseDefinition
    {
        public string moduleType { get; set; }
        public string userId { get; set; }
        public List<subModuleDefinition> subModules { get; set; }

    }

    class subModuleDefinition: moduleBaseDefinition
    {
        public List<fieldProperties> moduleFields { get; set; }
    }

    //class moduleRelationships
    //{
    //}
    class minmaxConstraints
    {
        public long minimum { get; set; }
        public long maximum { get; set; }
    }
    class textConstraints
    {
        public int maxLength { get; set; }
    }

    class fieldProperties
    {
        public bool isDeprecated { get; set; }
        public string uniqueId { get; set; }

        public int position { get; set; }
        public string question { get; set; }
        public string dataType { get; set; }
        public List<string> fieldChoices { get; set; }
        public string questionName { get; set; }
        public string isRequired { get; set; }
        public string isIndexed { get; set; }
        public minmaxConstraints numberConstraints { get; set; }
        public textConstraints textConstraints { get; set; }
        public minmaxConstraints dateConstraints { get; set; }
        //public maxLenConstraint maxLength { get; set; }
    }

    static class Constants
    {
        public const int NULL_NUM = int.MinValue;
        public const int ERROR_NUM = -9999999;
        public const int MINDATE = 19000101;
        public const int MAXDATE = 21001231;
        public const int ATTODAYDATECONSTANT = 21001230;
        public const string TODAY = "@today";


        public static int toInt(this string stringVal)
        {
            return Convert.ToInt32(stringVal);
        }
    }
}
