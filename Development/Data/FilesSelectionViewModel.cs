﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ReportTransfert.Data
{
    public class FilesSelectionViewModel : INotifyPropertyChanged
    {
        private string _folderBase;
        private IEnumerable<FilesSelectionItem> _selectedFiles = null;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        public FilesSelectionViewModel()
        {
            this.Files = new ObservableCollection<FilesSelectionItem>();
            this.AcceptCommand = new RelayCommand(async (p) => await this.AcceptExecute(p), (p) => this.SelectedFiles != null && this.SelectedFiles.Count() > 0);
            this.CancelCommand = new RelayCommand(async (p) => await this.CancelExecute(p));

            using (Registry registry = new Registry(ReportServices.REGISTRY_APPLICATIONNAME, "Upload"))
            {
                this.FolderBase = registry.GetValue("FolderBase");
            }
        }

        /// <summary>
        /// Gets or sets a Root Folder to find all sub-files.
        /// </summary>
        public string FolderBase
        {
            get
            {
                return _folderBase;
            }
            set
            {
                _folderBase = value;
                this.OnPropertyChanged("FolderBase");
                this.FindAllFiles();
            }
        }

        /// <summary>
        /// Gets the Window.DialogResult: True if the user was clicked on OK, False if he was clicked on Cancel.
        /// </summary>
        public bool? DialogResult { get; private set; }

        /// <summary>
        /// Gets the command to execute when the user click on OK button
        /// </summary>
        public RelayCommand AcceptCommand { get; private set; }

        /// <summary>
        /// Gets the command to execute when the user click on Cancel button
        /// </summary>
        public RelayCommand CancelCommand { get; private set; }

        /// <summary>
        /// Gets a list with all files found in FolderBase and sub-folders.
        /// </summary>
        public ObservableCollection<FilesSelectionItem> Files { get; private set; }

        /// <summary>
        /// Gets or sets all selected files
        /// </summary>
        public IEnumerable<FilesSelectionItem> SelectedFiles
        {
            get
            {
                return _selectedFiles;
            }
            set
            {
                _selectedFiles = value;
                OnPropertyChanged("SelectedFiles");
            }
        }

        /// <summary>
        /// Search all files of FolderBase folder and sub folders
        /// </summary>
        private void FindAllFiles()
        {
            if (string.IsNullOrEmpty(this.FolderBase))
                return;

            DirectoryInfo baseFolder = new DirectoryInfo(this.FolderBase);

            if (baseFolder.Exists && baseFolder.FullName.Length > 3)
            {
                this.Files = new ObservableCollection<FilesSelectionItem>(baseFolder.EnumerateFiles("*.*", SearchOption.AllDirectories).Select(f => new FilesSelectionItem(f, baseFolder)));
            }
            else
            {
                this.Files = new ObservableCollection<FilesSelectionItem>();
            }

            this.OnPropertyChanged("Files");
        }

        /// <summary>
        /// Close the window and sets the SelectedFiles property
        /// </summary>
        /// <param name="parameter"></param>
        private async Task AcceptExecute(object parameter)
        {
            Window win = parameter as Window;
            if (win != null)
            {
                using (Registry registry = new Registry(ReportServices.REGISTRY_APPLICATIONNAME, "Upload"))
                {
                    registry.SetValue("FolderBase", this.FolderBase);
                }

                this.DialogResult = true;
                win.DialogResult = true;
                win.Close();
            }
        }

        /// <summary>
        /// Close the window
        /// </summary>
        /// <param name="parameter"></param>
        private async Task CancelExecute(object parameter)
        {
            Window win = parameter as Window;
            if (win != null)
            {
                this.DialogResult = false;
                win.DialogResult = false;
                win.Close();
            }
        }

        /// <summary>
        /// Raises the PropertyChanged to notify UI
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
