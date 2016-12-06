using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    class minMaxConstraint
    {
        public long minimum { get; set; }
        public long maximum { get; set; }
    }

    class maxLenConstraint
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
        public minMaxConstraint minMax { get; set; }
        public maxLenConstraint maxLength { get; set; }
    }
}
