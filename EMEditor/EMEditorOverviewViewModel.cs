using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TREnvironmentEditor;

namespace EMEditor
{
    public class EMEditorOverviewViewModel : EMEditorVMBase
    {
        private EMEditorMapping _mapping;

        public EMEditorMapping Mapping
        {
            get { return _mapping; }
            set { SetProperty(ref _mapping, value); }
        }

        private readonly DelegateCommand _importJSONCommand;
        public ICommand ImportJSONCommand => _importJSONCommand;

        private void OnImportJSON(object commandParameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
                _mapping = EMEditorMapping.Get(openFileDialog.FileName);

            _importJSONCommand.InvokeCanExecuteChanged();
        }

        private bool CanImportJSON(object commandParameter)
        {
            return true;
        }

        private readonly DelegateCommand _exportJSONCommand;
        public ICommand ExportJSONCommand => _exportJSONCommand;

        private void OnExportJSON(object commandParameter)
        {
            _exportJSONCommand.InvokeCanExecuteChanged();
        }

        private bool CanExportJSON(object commandParameter)
        {
            return true;
        }

        private readonly DelegateCommand _applyToLevelCommand;
        public ICommand ApplyToLevelCommand => _applyToLevelCommand;

        private void OnApplyToLevel(object commandParameter)
        {
            _applyToLevelCommand.InvokeCanExecuteChanged();
        }

        private bool CanApplyToLevel(object commandParameter)
        {
            return true;
        }

        private readonly DelegateCommand _addModCommand;
        public ICommand AddModCommand => _addModCommand;

        private void OnAddMod(object commandParameter)
        {
            _addModCommand.InvokeCanExecuteChanged();
        }

        private bool CanAddMod(object commandParameter)
        {
            return true;
        }

        private readonly DelegateCommand _editModCommand;
        public ICommand EditModCommand => _editModCommand;

        private void OnEditMod(object commandParameter)
        {
            _editModCommand.InvokeCanExecuteChanged();
        }

        private bool CanEditMod(object commandParameter)
        {
            return true;
        }

        private readonly DelegateCommand _deleteModCommand;
        public ICommand DeleteModCommand => _deleteModCommand;

        private void OnDeleteMod(object commandParameter)
        {
            _deleteModCommand.InvokeCanExecuteChanged();
        }

        private bool CanDeleteMod(object commandParameter)
        {
            return true;
        }

        public EMEditorOverviewViewModel()
        {
            _importJSONCommand = new DelegateCommand(OnImportJSON, CanImportJSON);
            _exportJSONCommand = new DelegateCommand(OnExportJSON, CanExportJSON);
            _applyToLevelCommand = new DelegateCommand(OnApplyToLevel, CanApplyToLevel);
            _addModCommand = new DelegateCommand(OnAddMod, CanAddMod);
            _editModCommand = new DelegateCommand(OnEditMod, CanEditMod);
            _deleteModCommand = new DelegateCommand(OnDeleteMod, CanDeleteMod);
        }
    }
}
