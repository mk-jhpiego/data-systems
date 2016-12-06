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
            
        }

        private void closeButton_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void setValue(TextBox control, string textOption)
        {
            textOption = string.IsNullOrWhiteSpace(textOption) ? "" : textOption;
            control.Text = textOption;
        }

        void setValue(string textOption, params RadioButton[] controls)
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

        string getCheckedItem(params RadioButton[] controls)
        {
            var selectedControl = controls.Where(control => control.IsChecked != null && control.IsChecked == true)
                .FirstOrDefault();
            return selectedControl == null ? "" : selectedControl.Content.ToString();
        }

        void updateModuleDefinition(moduleDefinition module)
        {
            module.name = getValue(textModuleName);
            module.description = getValue(textModuleDescription);
            module.version = getValue(textModuleVersion);
            var moduleTypeOption = getCheckedItem(
                rdbModTypePerson, rdbModTypeNonPerson, rdbModTypeGroup);
            module.moduleType = moduleTypeOption;
            module.userId = getValue(textEmailAddress);
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
            //gridModuleActivities.re
        }

        private void saveButton_click(object sender, RoutedEventArgs e)
        {
            //we update the module definition
            _currentModule = _currentModule ?? new moduleDefinition() { id = Guid.NewGuid().ToString("N"), subModules=new List<subModuleDefinition>() };
            updateModuleDefinition(_currentModule);

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
            //weread the field values and clear the form

        }
        //cancelAddField_click
        private void cancelAddField_click(object sender, RoutedEventArgs e) {
            //discard changes and clear the fields

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
            var subModuleName = getValue(textSubModuleName);
            var subModuleDescription = getValue(textSubModuleDescription);
            var subModuleVersion = getValue(textSubModuleVersion);

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

        void showDialog(string msg)
        {
            MessageBox.Show(msg, "No module selected", MessageBoxButton.OK);
        }
    }
}
