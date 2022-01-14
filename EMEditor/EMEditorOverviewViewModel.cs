using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TREnvironmentEditor;
using TREnvironmentEditor.Model;

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


            All = new ObservableCollection<BaseEMFunction>(_mapping.All.AsEnumerable());
            AllConditional = new ObservableCollection<EMConditionalSingleEditorSet>(_mapping.ConditionalAll.AsEnumerable());
            Mirrored = new ObservableCollection<BaseEMFunction>(_mapping.Mirrored.AsEnumerable());
            NonPurist = new ObservableCollection<BaseEMFunction>(_mapping.NonPurist.AsEnumerable());
            Any = new ObservableCollection<EMEditorSet>(_mapping.Any.AsEnumerable());
            AllWithin = new ObservableCollection<List<EMEditorSet>>(_mapping.AllWithin.AsEnumerable());
            OneOf = new ObservableCollection<EMEditorGroupedSet>(_mapping.OneOf.AsEnumerable());
            ConditionalAllWithin = new ObservableCollection<EMConditionalEditorSet>(_mapping.ConditionalAllWithin.AsEnumerable());

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
            return false;
        }

        private readonly DelegateCommand _applyToLevelCommand;
        public ICommand ApplyToLevelCommand => _applyToLevelCommand;

        private void OnApplyToLevel(object commandParameter)
        {
            _applyToLevelCommand.InvokeCanExecuteChanged();
        }

        private bool CanApplyToLevel(object commandParameter)
        {
            return false;
        }

        private readonly DelegateCommand _addModCommand;
        public ICommand AddModCommand => _addModCommand;

        private void OnAddMod(object commandParameter)
        {
            _addModCommand.InvokeCanExecuteChanged();
        }

        private bool CanAddMod(object commandParameter)
        {
            return false;
        }

        private readonly DelegateCommand _editModCommand;
        public ICommand EditModCommand => _editModCommand;

        private void OnEditMod(object commandParameter)
        {
            _editModCommand.InvokeCanExecuteChanged();
        }

        private bool CanEditMod(object commandParameter)
        {
            return false;
        }

        private readonly DelegateCommand _deleteModCommand;
        public ICommand DeleteModCommand => _deleteModCommand;

        private void OnDeleteMod(object commandParameter)
        {
            _deleteModCommand.InvokeCanExecuteChanged();
        }

        private bool CanDeleteMod(object commandParameter)
        {
            return false;
        }

        private ObservableCollection<BaseEMFunction> _all;
        public ObservableCollection<BaseEMFunction> All
        {
            get { return _all; }
            set { SetProperty(ref _all, value); }
        }

        private ObservableCollection<EMConditionalSingleEditorSet> _allConditional;
        public ObservableCollection<EMConditionalSingleEditorSet> AllConditional
        {
            get { return _allConditional; }
            set { SetProperty(ref _allConditional, value); }
        }

        private ObservableCollection<BaseEMFunction> _mirrored;
        public ObservableCollection<BaseEMFunction> Mirrored
        {
            get { return _mirrored; }
            set { SetProperty(ref _mirrored, value); }
        }

        private ObservableCollection<BaseEMFunction> _nonPurist;
        public ObservableCollection<BaseEMFunction> NonPurist
        {
            get { return _nonPurist; }
            set { SetProperty(ref _nonPurist, value); }
        }

        private ObservableCollection<EMEditorSet> _any;
        public ObservableCollection<EMEditorSet> Any
        {
            get { return _any; }
            set { SetProperty(ref _any, value); }
        }

        private ObservableCollection<List<EMEditorSet>> _allWithin;
        public ObservableCollection<List<EMEditorSet>> AllWithin
        {
            get { return _allWithin; }
            set { SetProperty(ref _allWithin, value); }
        }

        private ObservableCollection<EMEditorGroupedSet> _oneOf;
        public ObservableCollection<EMEditorGroupedSet> OneOf
        {
            get { return _oneOf; }
            set { SetProperty(ref _oneOf, value); }
        }

        private ObservableCollection<EMConditionalEditorSet> _conditionalAllWithin;
        public ObservableCollection<EMConditionalEditorSet> ConditionalAllWithin
        {
            get { return _conditionalAllWithin; }
            set { SetProperty(ref _conditionalAllWithin, value); }
        }

        private object _selectedMod;
        public object SelectedMod
        {
            get { return _selectedMod; }
            set { SetProperty(ref _selectedMod, value); }
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
