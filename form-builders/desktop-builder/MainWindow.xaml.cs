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
            return control.Text ?? control.Text;
        }

        string getCheckedItem(params RadioButton[] controls)
        {
            var selectedControl = controls.Where(control => control.IsChecked != null && control.IsChecked == true)
                .FirstOrDefault();
            return selectedControl == null ? "" : selectedControl.Content.ToString();
        }

        void updateModuleDefinition(moduleDefinition module)
        {
            module.moduleName = getValue(textModuleName);
            module.moduleDescription = getValue(textModuleDescription);
            module.moduleVersion = getValue(textModuleVersion);
            var moduleTypeOption = getCheckedItem(
                rdbModTypePerson, rdbModTypeNonPerson, rdbModTypeGroup);
            module.moduleType = moduleTypeOption;
            module.userId = getValue(textEmailAddress);
        }

        void updateView(moduleDefinition module)
        {
            _currentModule = module;
            setValue(textModuleName, module.moduleName);
            setValue(textModuleDescription, module.moduleDescription);
            setValue(textModuleVersion, module.moduleVersion);
            setValue(module.moduleType, rdbModTypePerson, rdbModTypeNonPerson, rdbModTypeGroup);
            setValue(textEmailAddress, module.userId);
            setValue(textModuleName, module.moduleName);
        }

        private void saveButton_click(object sender, RoutedEventArgs e)
        {
            //we update the module definition
            _currentModule = _currentModule ?? new moduleDefinition() { moduleId = Guid.NewGuid().ToString("N") };
            updateModuleDefinition(_currentModule);

            //and save
            var name = _currentModule.moduleName.Replace(' ', '-').ToLowerInvariant() + ".json";
            var dialog = new SaveFileDialog() { FileName = name, OverwritePrompt = true, Filter = "*.json|json", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
            var dialogResult = dialog.ShowDialog();
            if (dialogResult != null && dialogResult == true)
            {
                File.WriteAllText(dialog.FileName,
                    Newtonsoft.Json.JsonConvert.SerializeObject(_currentModule));
            }
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
                var module = new moduleDefinition() { moduleId = Guid.NewGuid().ToString("N") };
                updateView(module);
            }            
        }
    }
}
