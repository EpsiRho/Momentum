using Momentum.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Momentum.ViewModels
{
    public class SearchViewModel
    {
        private ObservableCollection<IndexedFileInfo> fileCollection;
        public ObservableCollection<IndexedFileInfo> FileCollection
        {
            get { return fileCollection; }
            set { fileCollection = value; }
        }

        private ObservableCollection<FileIndexer> progressCollection;
        public ObservableCollection<FileIndexer> ProgressCollection
        {
            get { return progressCollection; }
            set { progressCollection = value; }
        }

        private ObservableCollection<string> folders;
        public ObservableCollection<string> Folders
        {
            get { return folders; }
            set { folders = value; }
        }

    }
}
