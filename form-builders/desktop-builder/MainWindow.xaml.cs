using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Globalization;

namespace desktop_builder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        moduleDefinition _currentModule = null;
        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        //actvityFieldRowSelected
        private void actvityFieldRowSelected(object sender, MouseButtonEventArgs e)
        {
            var selectedRow = gridActivityFields.SelectedItem;
            if (selectedRow == null) return;
            var moduleActivity = selectedRow as fieldProperties;
            if (moduleActivity == null) return;

            //we update the view with these details
            updateFieldEditor(moduleActivity);
        }

        void updateFieldEditor(fieldProperties module)
        {
            //first we clear it
            clearFieldEditor();
            clearGroupPanel();
            stackFieldAttributes.Visibility = Visibility.Visible;
            stackGroupAttributes.Visibility = Visibility.Collapsed;

            setValue(fieldTextUniqueId, module.uniqueId);

            var allRadioButtons =
                (from panel in new List<StackPanel> { dataTypeParent1, dataTypeParent2 }
                 let children = panel.Children.OfType<RadioButton>()
                 from child in children
                 select child).ToList();
            setValue(module.dataType, allRadioButtons);
            if (module.fieldChoices != null)
            {
                setValue(textFieldChoices,
                    string.Join(Environment.NewLine, module.fieldChoices));
            }
            setValue(fieldTextQuestionName, module.questionName);
            setValue(fieldTextQuestion, module.question);
            if (module.numberConstraints != null)
            {
                setValue(fieldTextMinimum, module.numberConstraints.minimum);
                setValue(fieldTextMaximum, module.numberConstraints.maximum);
            }

            if (module.textConstraints != null)
                setValue(fieldTextMaxLength, module.textConstraints.maxLength);

            setValue(module.isRequired, fieldIsRequiredYes, fieldIsRequiredNo);
            setValue(module.isIndexed, fieldIsIndexedYes, fieldIsIndexedNo);
            if (module.dateConstraints != null)
            {
                setValue(fieldTextDateMin, module.dateConstraints.minimum);
                setValue(fieldTextDateMax, module.dateConstraints.maximum);
            }
            //setValue(textSubModuleName, module.name);
            //setValue(textSubModuleVersion, module.version);
            //setValue(textSubModuleDescription, module.description);
        }

        void clearFieldEditor()
        {
            setValue(fieldTextUniqueId, "");
            setValue(fieldTextQuestion, "");
            var allRadioButtons =
(from panel in new List<StackPanel> { dataTypeParent1, dataTypeParent2 }
 let children = panel.Children.OfType<RadioButton>()
 from child in children
 select child).ToList();
            setValue("KUMBAYA", allRadioButtons);
            setValue(textFieldChoices, "");
            setValue(fieldTextQuestionName, "");

            setValue(fieldTextMinimum, "");
            setValue(fieldTextMaximum, "");
            setValue(fieldTextMaxLength, "");

            setValue("KUMBAYA", fieldIsRequiredYes, fieldIsRequiredNo);
            setValue(fieldTextDateMin, "");
            setValue(fieldTextDateMax, "");
            setValue("KUMBAYA", fieldIsIndexedYes, fieldIsIndexedNo);
        }

        void updateGroupView(IEnumerable<fieldProperties> fields)
        {
            //clearGroupPanel();
            var first = fields.FirstOrDefault();// t => !string.IsNullOrWhiteSpace(t.groupName));
            if (first != null)
            {
                setValue(fieldTextGroupName, first.groupName ?? "");
                setValue(fieldGroupPageNumber, first.pageNumber);
                var areSingleSelect = isSingleSelectAndAreConsistent(fields);
                if (areSingleSelect)
                {
                    setValue((first.displayAsBlock ? "Yes" : "No"),
                        fieldDisplayAsBlockYes, fieldDisplayAsBlockNo);
                }
            }
        }                

        void updateGroupProperties(IEnumerable<fieldProperties> fields)
        {
            //fieldTextGroupName
            var groupName = textRequiredValidationRules.getValue(fieldTextGroupName);

            //fieldGroupPageNumber
            var res = intValidationRule.ValidateControl(fieldGroupPageNumber);
            if (!res.IsValid)
            {
                return;
            }

            //update details
            var areSingleSelect = isSingleSelectAndAreConsistent(fields);
            var displayAsBlock = false;
            if (areSingleSelect)
            {
                //fieldDisplayAsBlockYes, fieldDisplayAsBlockNo
                var checkedOption = getCheckedItem(fieldDisplayAsBlockYes, fieldDisplayAsBlockNo);
                if (!string.IsNullOrWhiteSpace(checkedOption))
                {
                    displayAsBlock = checkedOption == "Yes";
                }
            }
            
            foreach (var field in fields)
            {
                if (!string.IsNullOrWhiteSpace(groupName))
                {
                    field.groupName = groupName;
                }

                if (areSingleSelect)
                {
                    field.displayAsBlock = displayAsBlock;
                }

                var pageNum = intValidationRule.getValue(fieldGroupPageNumber);
                if (pageNum != Constants.NULL_NUM)
                {
                    field.pageNumber = pageNum;
                }
            }

            //clear the group panel
            clearGroupPanel();
        }

        void clearGroupPanel()
        {
            setValue(fieldTextGroupName, "");
            setValue(fieldGroupPageNumber, "");
            setValue("KUMBAYA", fieldDisplayAsBlockYes, fieldDisplayAsBlockNo);            
        }

        void clearSubModuleEditor()
        {
            var blank = new subModuleDefinition()
            {
                moduleFields = new List<fieldProperties>()
            };
            updateSubModuleEditor(blank);
            updateFieldPropertiesList(blank);
            clearFieldEditor();
            clearGroupPanel();
        }

        bool updateFieldProperties(fieldProperties module)
        {
            var toRetun = false;
            var validationRes = textRequiredValidationRules.ValidateControl(fieldTextQuestion);
            if (!validationRes.IsValid)
            {
                return toRetun;
            }
            module.question = textRequiredValidationRules.getValue(fieldTextQuestion);

            var allRadioButtons =
    (from panel in new List<StackPanel> { dataTypeParent1, dataTypeParent2 }
     let children = panel.Children.OfType<RadioButton>()
     from child in children
     select child).ToList();

            var datatype = getCheckedItem(allRadioButtons);
            if (string.IsNullOrWhiteSpace(datatype))
            {
                setValue("Text", allRadioButtons);
                datatype = getCheckedItem(allRadioButtons);
            }
            module.dataType = datatype;

            validationRes = textRequiredValidationRules.ValidateControl(fieldTextQuestionName);
            if (!validationRes.IsValid)
            {
                return toRetun;
            }
            module.questionName = textRequiredValidationRules.getValue(fieldTextQuestionName);

            module.isRequired = getCheckedItem(fieldIsRequiredYes, fieldIsRequiredNo);
            module.isIndexed = getCheckedItem(fieldIsIndexedYes, fieldIsIndexedNo);
            module.numberConstraints = null;
            module.dateConstraints = null;
            module.textConstraints = null;

            if (module.dataType == "Single Select" || module.dataType == "Multiple Select")
            {
                var res = multiLineValidationRules.ValidateControl(textFieldChoices);
                if (!res.IsValid)
                { return toRetun; }
                var choices = multiLineValidationRules.getValueMultiline(textFieldChoices);
                module.fieldChoices = choices;
            }
            else if (module.dataType== "Number" || module.dataType == "Integer")
            {
                var res = intValidationRule.ValidateControl(fieldTextMinimum);
                if (!res.IsValid)
                {
                    return toRetun;
                }
                res = intValidationRule.ValidateControl(fieldTextMaximum);
                if (!res.IsValid)
                {
                    return toRetun;
                }

                res = minmaxValidationRule.ValidateControl(fieldTextMinimum, fieldTextMaximum);
                if (!res.IsValid)
                {
                    return toRetun;
                }

                var numberConstraints = minmaxValidationRule.
                    getValues(fieldTextMinimum, fieldTextMaximum);
                if (numberConstraints.minimum == Constants.NULL_NUM)
                    numberConstraints = null;
                module.numberConstraints = numberConstraints;
           }
            else if (module.dataType == "Date")
            {
                var res = dateValidationRules.ValidateControl(fieldTextDateMin);
                if (!res.IsValid)
                {
                    return toRetun;
                }
                res = dateValidationRules.ValidateControl(fieldTextDateMax);
                if (!res.IsValid)
                {
                    return toRetun;
                }

                res = dateMinMaxValidationRules.ValidateControl(
                    fieldTextDateMin, fieldTextDateMax);
                if (!res.IsValid)
                {
                    return toRetun;
                }

                var numberConstraints = dateMinMaxValidationRules.
                    getValues(fieldTextDateMin, fieldTextDateMax);
                if (numberConstraints.minimum == Constants.NULL_NUM)
                    numberConstraints = null;
                module.dateConstraints = numberConstraints;
            }
            else if (module.dataType == "Text")
            {
                //DateMinMaxValidationRules
                var res = intValidationRule.ValidateControl(fieldTextMaxLength);
                if (!res.IsValid)
                {
                    return toRetun;
                }
                
                res = gtZeroValidationRules.ValidateControl(fieldTextMaxLength);
                if (!res.IsValid)
                {
                    return toRetun;
                }
                
                var maxLength = intValidationRule.getValue(fieldTextMaxLength);
                if (maxLength != Constants.NULL_NUM)
                {
                    module.textConstraints = new textConstraints() { maxLength = maxLength };
                }                               
            }

            //we clear the current view
            clearFieldEditor();
            return true;
        }

        private void actvitiesRowSelected(object sender, MouseButtonEventArgs e)
        {           
            var selectedRow = gridModuleActivities.SelectedItem;
            if (selectedRow == null) return;
            var moduleActivity = selectedRow as subModuleDefinition;
            if (moduleActivity == null) return;

            var id = moduleActivity.id;
            //we update the view with these details
            updateSubModuleEditor(moduleActivity);
        }

        void updateSubModuleEditor(subModuleDefinition module)
        {
            setValue(textSubModuleId, module.id);
            setValue(textSubModuleName, module.name);
            setValue(textSubModuleVersion, module.version);
            setValue(textSubModuleDescription, module.description);
            
            //bind activities list
            updateFieldPropertiesList(module);         
        }

        private void updateFieldPropertiesList(subModuleDefinition module)
        {
            //sort
            if (module.moduleFields != null)
            {
                sortFields(module.moduleFields);
            }            

            //gridActivityFields
            labelActivityName.Content = "Currently editing - " + module.name;
            module.moduleFields = module.moduleFields ?? new List<fieldProperties>();
            gridActivityFields.ItemsSource = "";
            gridActivityFields.ItemsSource = module.moduleFields;
        }

        private void closeButton_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void setValue(TextBox control, int textOption)
        {
            setValue(control, textOption.ToString());
        }
        void setValue(TextBox control, long textOption)
        {
            setValue(control, textOption.ToString());
        }

        void setValue(TextBox control, string textOption)
        {
            textOption = string.IsNullOrWhiteSpace(textOption) ? "" : textOption;
            control.Text = textOption;
        }

        void setValue(string textOption, params RadioButton[] controls)        
        {
            setValue(textOption, controls.ToList());
        }

        void setValue(string textOption, List<RadioButton> controls)
        {
            foreach (var control in controls)
            {
                control.IsChecked = false;
            }

            if (string.IsNullOrWhiteSpace(textOption)) return;

            foreach (var control in controls)
            {
                if (control.Content.ToString() == textOption)
                {
                    control.IsChecked = true;
                    break;
                }
            }
        }

        string getValue(TextBox control)
        {
            return string.IsNullOrWhiteSpace(control.Text) ? "" : control.Text;
        }

        string getCheckedItem(List<RadioButton> controls)
        {
            var selectedControl = controls.Where(control => control.IsChecked != null && control.IsChecked == true)
                .FirstOrDefault();
            return selectedControl == null ? "" : selectedControl.Content.ToString();
        }

        string getCheckedItem(params RadioButton[] controls)
        {
            return getCheckedItem(controls.ToList());
        }

        bool updateModuleDefinition(moduleDefinition module)
        {
            var toReturn = false;
            var validationRes = textRequiredValidationRules.ValidateControl(textModuleName);
            if (!validationRes.IsValid)
            {
                return toReturn;
            }
            module.name = textRequiredValidationRules.getValue(textModuleName);

            module.description = getValue(textModuleDescription);

            validationRes = textRequiredValidationRules.ValidateControl(textModuleVersion);
            if (!validationRes.IsValid)
            {
                return toReturn;
            }
            module.version = textRequiredValidationRules.getValue(textModuleVersion);


            var moduleTypeOption = getCheckedItem(
                rdbModTypePerson, rdbModTypeNonPerson, rdbModTypeGroup);
            module.moduleType = moduleTypeOption;

            validationRes = textRequiredValidationRules.ValidateControl(textEmailAddress);
            if (!validationRes.IsValid)
            {
                return toReturn;
            }
            module.userId = textRequiredValidationRules.getValue(textEmailAddress);
            return true;
        }

        void updateView(moduleDefinition module)
        {
            _currentModule = module;
            setValue(textModuleName, module.name);
            setValue(textModuleDescription, module.description);
            setValue(textModuleVersion, module.version);
            setValue(module.moduleType, rdbModTypePerson, rdbModTypeNonPerson, rdbModTypeGroup);
            setValue(textEmailAddress, module.userId);
            setValue(textModuleName, module.name);

            //bind activities list
            updateActivitiesList(_currentModule.subModules);
        }

        void updateActivitiesList(List<subModuleDefinition> subModules)
        {
            _currentModule.subModules = _currentModule.subModules ?? 
                new List<subModuleDefinition>();
            gridModuleActivities.ItemsSource = "";
            gridModuleActivities.ItemsSource = _currentModule.subModules;

            clearSubModuleEditor();            
        }

        private void saveButton_click(object sender, RoutedEventArgs e)
        {
            //we update the module definition
            _currentModule = _currentModule ?? new moduleDefinition() { id = Guid.NewGuid().ToString("N"), subModules=new List<subModuleDefinition>() };
            var validates = updateModuleDefinition(_currentModule);
            if (!validates)
                return;

            //and save
            var name = _currentModule.name.Replace(' ', '-').ToLowerInvariant() + ".json";
            var dialog = new SaveFileDialog() { FileName = name, OverwritePrompt = true, Filter = "*.json|json", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
            var dialogResult = dialog.ShowDialog();
            if (dialogResult != null && dialogResult == true)
            {
                File.WriteAllText(dialog.FileName,
                    Newtonsoft.Json.JsonConvert.SerializeObject(_currentModule));
            }
        }

        //addField_click
        private void addField_click(object sender, RoutedEventArgs e) {
            //we get the current activity
            var activityId = getValue(textSubModuleId);
            if (string.IsNullOrWhiteSpace(activityId))
            {
                showDialog("Please select an activity first");
                return;
            }

            var currentActivity = _currentModule.subModules
                .FirstOrDefault(t => t.id == activityId);
            if (currentActivity == null)
            {
                showDialog("An unknown error occurred. Could not find named activity");
                return;
            }            

            //we read the field values and clear the form
            //we check if we have a field id and find the target record
            var fieldId = getValue(fieldTextUniqueId);
            fieldProperties currentField = null;
            if (string.IsNullOrWhiteSpace(fieldId))
            {
                //either is in group mode or is a new record
                if(gridActivityFields.SelectedItems!=null && gridActivityFields.SelectedItems.Count > 1)
                {
                    //is in group mode
                    //we get all the fields
                    var fields = gridActivityFields.SelectedItems;
                    updateGroupProperties(fields.Cast<fieldProperties>().ToList());
                    updateFieldPropertiesList(currentActivity);
                    return;
                }
                else
                {
                    //means this is a new record
                    currentField = new fieldProperties();

                    //todo: make changes to a copy and add to main list once user confirms
                    //setValue(fieldTextUniqueId, currentField.uniqueId);
                }
            }
            else
            {
                //we attempt to locate it
                //todo: make changes to a copy and add to main list once user confirms
                var oldField = currentActivity.moduleFields.FirstOrDefault(t => t.uniqueId == fieldId);
                if (oldField == null)
                {
                    showDialog("An unknown error occurred. Could not find named field");
                    return;
                }

                //we deepclone it
                var serialised = Newtonsoft.Json.JsonConvert.SerializeObject(oldField);
                currentField = Newtonsoft.Json.JsonConvert
                    .DeserializeObject<fieldProperties>(serialised);
            }

            if (updateFieldProperties(currentField))
            {
                //update activity fields
                currentActivity.moduleFields.RemoveAll(t => t.uniqueId == fieldId);

                if (string.IsNullOrWhiteSpace(currentField.uniqueId))
                {
                    currentField.uniqueId = Guid.NewGuid().ToString("N");
                    currentField.position = currentActivity.moduleFields.Count + 1;
                }

                //sortFields(currentActivity.moduleFields);
                //renumberFields(currentActivity);

                currentActivity.moduleFields.Add(currentField);
                updateFieldPropertiesList(currentActivity);
            }
        }
        //cancelAddField_click
        private void cancelAddField_click(object sender, RoutedEventArgs e) {
            //discard changes and clear the fields
            //we clear the current view
            clearFieldEditor();
        }

        private void saveSubModuleChangesButton_click(object sender, RoutedEventArgs e)
        {
            //we read the field values and clear the form
            if (_currentModule == null)
            {
                showDialog("Please select a module first");
                return;
            }

            if (_currentModule.subModules == null)
                _currentModule.subModules = new List<subModuleDefinition>();

            //we get all the values
            var subModuleId = getValue(textSubModuleId);

            var validationRes = textRequiredValidationRules.ValidateControl(textSubModuleName);
            if (!validationRes.IsValid)
            {
                return;
            }
            var subModuleName = textRequiredValidationRules.getValue(textSubModuleName);

            var subModuleDescription = getValue(textSubModuleDescription);

            validationRes = textRequiredValidationRules.ValidateControl(textSubModuleVersion);
            if (!validationRes.IsValid)
            {
                return;
            }
            var subModuleVersion = textRequiredValidationRules.getValue(textSubModuleVersion);
           // var subModuleVersion = getValue(textSubModuleVersion);

            //check if this activity exist by id
            subModuleDefinition activity = null;
            if (!string.IsNullOrWhiteSpace(subModuleId))
                activity = _currentModule.subModules
                    .FirstOrDefault(mod => mod.id == subModuleId);

            //add if it doesn't exist
            if (activity == null)
            {
                activity = new subModuleDefinition() { id = Guid.NewGuid().ToString("N"),
                    moduleFields = new List<fieldProperties>() };
                _currentModule.subModules.Add(activity);
                setValue(textSubModuleId, activity.id);
            }

            //activity.id = subModuleId;
            activity.name = subModuleName;
            activity.description = subModuleDescription;
            activity.version = subModuleVersion;

            //refresh the activities list
            updateActivitiesList(_currentModule.subModules);
        }
        private void cancelSubModuleChanges_click(object sender, RoutedEventArgs e)
        {
            //discard changes and clear the fields
            //clear the form
            clearSubModuleEditor();
        }

        private void openExisting_click(object sender, RoutedEventArgs e)
        {          
            var dialog = new OpenFileDialog() { CheckFileExists = true,
                Multiselect =false, Filter="JSON|*.json",
                InitialDirectory =Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments) };
            var dialogResult = dialog.ShowDialog();
            if (dialogResult != null && dialogResult == true)
            {                
                //we open the file
                var savedModule =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<moduleDefinition>
                    (File.ReadAllText(dialog.FileName));
                updateView(savedModule);
            }
        }

        private void cancelButton_click(object sender, RoutedEventArgs e)
        {
            //warn that they wil lose data before attempting to close
            var msg = MessageBox.Show("Do you want to close? Unsaved changes will be lost","Confirm action", MessageBoxButton.OKCancel);
            if (msg == MessageBoxResult.OK)
            {
                var module = new moduleDefinition() { id = Guid.NewGuid().ToString("N") };
                updateView(module);
            }            
        }

        //deleteFieldButton_click
        private void changeOrderButton_click(object sender, RoutedEventArgs e)
        {         
            var fieldId = getCurrentFieldId();
            if (fieldId == "0") return;

            var activityId = getCurrentActivityId();
            if (activityId == "0") return;

            var actvity = _currentModule.subModules.FirstOrDefault(t => t.id == activityId);
            if (actvity == null)
            {
                showDialog("An unknown issue occurred. Action not performed. Please restart the application");
                return;
            }

            renumberFields(actvity);
            
            var field = actvity.moduleFields.FirstOrDefault(x => x.uniqueId == fieldId);

            //we ascertain the action to perform
            var button = sender as Button;
            if (button == null) return;
            if(button.Content.ToString()== "Move Up")
            {
                //check if we are at the top
                if (field.position <= 1)
                    return;

                var field2 = actvity.moduleFields.FirstOrDefault(x => x.position == field.position - 1);
                //this shouldn't occur as we just renumbered them
                if (field2 == null) return;
                field.position--;
                field2.position++;
            }
            else
            {
                //we go down
                //check if we are rock bottom
                if (field.position >= actvity.moduleFields.Count)
                { return; }
                
                var field2 = actvity.moduleFields.FirstOrDefault(x => x.position == field.position + 1);
                //this shouldn't occur as we just renumbered them
                if (field2 == null) return;
                field.position++;
                field2.position--;
            }

            sortFields(actvity.moduleFields);
            updateFieldPropertiesList(actvity);
        }

        string getCurrentActivityId()
        {
            if (_currentModule == null)
            {
                showDialog("Please select a module first");
                return "0";
            }

            var submoduleId = textRequiredValidationRules.getValue(textSubModuleId);
            if (string.IsNullOrWhiteSpace(submoduleId))
            {
                showDialog("Please select an activity");
                return "0";
            }
            return submoduleId;
        }

        string getCurrentFieldId()
        {
            if (_currentModule == null)
            {
                showDialog("Please select a module first");
                return "0";
            }

            var submoduleId = textRequiredValidationRules.getValue(textSubModuleId);
            if (string.IsNullOrWhiteSpace(submoduleId))
            {
                showDialog("Please select an activity");
                return "0";
            }

            //we get the field id
            var fieldId = textRequiredValidationRules.getValue(fieldTextUniqueId);
            if (string.IsNullOrWhiteSpace(fieldId))
            {
                showDialog("Please select the field to delete. Double click a field to select it");
                return "0";
            }
            return fieldId;
        }

        private void deleteFieldButton_click(object sender, RoutedEventArgs e)
        {
            var fieldId = getCurrentFieldId();
            if (fieldId == "0") return;

            //warn that they wil lose data before attempting to close
            var msg = MessageBox.Show("Are you sure you want to remove this field? This action can not be reversed", "Confirm action", MessageBoxButton.OKCancel);
            if (msg == MessageBoxResult.OK)
            {
                var activityId = getCurrentActivityId();
                if (activityId == "0") return;

                var actvity = _currentModule.subModules.FirstOrDefault(t => t.id == activityId);
                if (actvity == null)
                {
                    showDialog("An unknown issue occurred. Action not performed. Please restart the application");
                    return;
                }

                //remove from the module fields
                var field = actvity.moduleFields.FirstOrDefault(x => x.uniqueId == fieldId);
                actvity.moduleFields.Remove(field);

                //refresh the view
                clearFieldEditor();

                //renumber the fields
                renumberFields(actvity);

                updateFieldPropertiesList(actvity);
            }
        }

        private void renumberFields(subModuleDefinition actvity)
        {
            for (var i = 0; i < actvity.moduleFields.Count; i++)
            {
                actvity.moduleFields[i].position = i + 1;
            }
        }

        private void sortFields(List<fieldProperties> moduleFields)
        {
            moduleFields.Sort((x, y) => x.position.CompareTo(y.position));
        }

        void showDialog(string msg)
        {
            MessageBox.Show(msg, "No module selected", MessageBoxButton.OK);
        }

        private void fieldTextQuestion_TextChanged(object sender, TextChangedEventArgs e)
        {
            //fieldTextQuestionName.Text = Regex.Replace(
            //    fieldTextQuestion.Text.ToLowerInvariant(),
            //    "[^a-zA-Z0-9 -]+", string.Empty, RegexOptions.Compiled);
        }

        bool isSingleSelectAndAreConsistent(IEnumerable<fieldProperties> selectedRows)
        {
            return ofSameType(selectedRows) && haveSameChoices(selectedRows);
        }

        bool ofSameType(IEnumerable<fieldProperties> selectedRows)
        {
            var first = selectedRows.First();
            var ofSameType = selectedRows.All(
                t => t.dataType == "Single Select"
                && t.fieldChoices != null && t.fieldChoices.Count == first.fieldChoices.Count);
            return ofSameType;
        }

        bool haveSameChoices(IEnumerable<fieldProperties> selectedRows)
        {
            var first = selectedRows.First();
            var haveSameChoices =
                (from field in selectedRows
                 from choice in field.fieldChoices
                 select first.fieldChoices.Contains(choice)).All(t => t);
            return haveSameChoices;
        }

        private void gridActivityFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridActivityFields.SelectedItems!=null && gridActivityFields.SelectedItems.Count > 1)
            {
                var fieldId = getValue(fieldTextUniqueId);
                if (!string.IsNullOrWhiteSpace(fieldId))
                {
                    var res = MessageBox.Show(
                        "You might lose your changes if you continue. Are you sure?", "",
                        MessageBoxButton.OKCancel);
                    if (res == MessageBoxResult.OK)
                    {
                        clearFieldEditor();
                    }
                    else
                    {
                        return;
                    }
                }

                //display the field for group / table diplay
                stackFieldAttributes.Visibility = Visibility.Collapsed;
                stackGroupAttributes.Visibility = Visibility.Visible;
                stackTableDisplayOptions.IsEnabled = false;

                var selectedRows = gridActivityFields.SelectedItems.Cast<fieldProperties>();
                if (ofSameType(selectedRows))
                {
                    stackTableDisplayOptions.IsEnabled = haveSameChoices(selectedRows);
                }
                updateGroupView(selectedRows);
            }
            else if (gridActivityFields.SelectedItems != null && gridActivityFields.SelectedItems.Count == 1)
            {
                stackFieldAttributes.Visibility = Visibility.Visible;
                stackGroupAttributes.Visibility = Visibility.Collapsed;
            }
        }
    }
}
