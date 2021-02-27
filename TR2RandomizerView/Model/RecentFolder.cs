using System;
using System.Windows.Input;

namespace TR2RandomizerView.Model
{
    public class RecentFolder : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public int Index { get; set; }
        public string DisplayIndex => Index + ".";
        public string FolderPath { get; set; }
        public ICommand OpenCommandExecuted => this;

        private readonly IRecentFolderOpener _opener;

        public RecentFolder(IRecentFolderOpener opener)
        {
            _opener = opener;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _opener.OpenDataFolder(this);
        }
    }
}