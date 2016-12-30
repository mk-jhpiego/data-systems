using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace standard_builder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool ProcessingForVMMC = false;
            var projectHelper = ProcessingForVMMC ?
                new vmmcHelper() as ProjectInfo :
                new ppxHelper();

            var instance = SharedInstance.Instance;
            instance.metaDataProvider = new MetaDataProvider();
            var moduleNamePrefixes = projectHelper.moduleNamePrefixes;


            var text = File.ReadAllText(projectHelper.LookupChoicesFile);
            var lookups = JsonConvert.DeserializeObject<List<FieldChoices>>(text);
            lookups.ForEach(
                t =>
                {
                    t.CleanValues = new List<string>();
                    foreach (var value in t.Values)
                    {
                        t.CleanValues.Add(t.Name + "_" + value.Clean());
                    }
                }
                );

            var fields = JsonConvert.DeserializeObject<List<FieldDefinition>>(
                File.ReadAllText(projectHelper.FieldDictionaryFile));
            if (ProcessingForVMMC)
            {
                if (fields.Count != 147)
                    throw new ArgumentOutOfRangeException("Expected 210 fields");
            }
            else
            {
                if (fields.Count != 234)
                    throw new ArgumentOutOfRangeException("Expected 210 fields");
            }

            //fields.RemoveAll(t=>t.)
            var fieldsToIgnore = new List<string>() { "district", "vm_province", "vm_district" };
            foreach (var fieldname in fieldsToIgnore)
            {
                fields.RemoveAll(t => t.ViewName == fieldname);
            }

            fields.ForEach(t =>
            {
                if (!string.IsNullOrWhiteSpace(t.ListName))
                {
                    var lookup = lookups.Where(p => p.Name == t.ListName).FirstOrDefault();
                    if (lookup == null)
                        throw new ArgumentNullException("Lookup undefined for " + t.ListName);
                    t.FieldOptions = lookup;
                }
            }
                );

            var viewPages = new Dictionary<string, ViewDefinitionBuilder>();
            var pageNumbersAll = (fields.Select(
                t => new { Combined = t.FormName + t.ViewPage, FormName = t.FormName, ViewPage = t.ViewPage })).ToList();
            var pageNumbers = pageNumbersAll.Distinct().ToList();
            foreach (var page in pageNumbers)
            {
                var moduleNamePrefix = moduleNamePrefixes[page.FormName];
                var currentDataPage = new ViewDefinitionBuilder()
                {
                    metaDataProvider = SharedInstance.Instance.metaDataProvider,
                    ViewPageName = moduleNamePrefix + page.ViewPage
                };
                viewPages[page.Combined] = currentDataPage;
            }

            foreach (var field in fields)
            {
                viewPages[field.FormName + field.ViewPage].ViewFields.Add(field);
            }

            //var pagedFieldDefinitions = autoCreatePages();
            //projectHelper.LookupChoicesFile
            var fieldsLookup = projectHelper.Prefix.ToLowerInvariant() + "_fields.json";
            var stringResoures = projectHelper.Prefix.ToLowerInvariant() + "_string.json";
            var isFirst = true;
            //var addDateTitleResource = true;
            if (!Directory.Exists("output//" + projectHelper.Prefix))
                Directory.CreateDirectory("output//" + projectHelper.Prefix);

            foreach (var page in viewPages.Values)
            {
                //page.metaDataProvider = SharedInstance.Instance.metaDataProvider;
                var pageContents = page.build();
                File.WriteAllText(
                    "output//" + projectHelper.Prefix + "//" +
                    page.ViewPageName.ToLowerInvariant() + ".axml", pageContents);
                File.WriteAllText(
                    "output//" + projectHelper.Prefix + "//" +
                    page.ViewPageName.ToLowerInvariant() + ".xml", pageContents);
            }

            File.WriteAllText(
                "output//" + projectHelper.Prefix + "//" +
                stringResoures, SharedInstance.Instance.metaDataProvider.StringResourcesItems.ToString());

            //we write the field dictionary
            //process after FieldDef.build()
            var allFields = SharedInstance.Instance.metaDataProvider.ModelItems;

            var undefinedPageFields = allFields.Where(t => string.IsNullOrWhiteSpace(t.pageName)).ToList();
            if (undefinedPageFields.Count > 0)
            {
                throw new ArgumentNullException("Page name not defined for one of the field items");
            }

            File.WriteAllText("output//" + projectHelper.Prefix + "//" + fieldsLookup,
                JsonConvert.SerializeObject(allFields));

            //kipeto
            Console.WriteLine("Import completed, press any key to return");
            Console.ReadLine();
        }

    }
}
