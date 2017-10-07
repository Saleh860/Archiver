using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Archiver.Model
{
    [Serializable()]
    public class ImageDirectory : Image
    {
        public ImageDirectory(string name, Master master)
            :base(name, master)
        {
            master.AddImage(this);
        }

        public ObservableCollection<Object> Subdirectories => ((Directory)theMaster).Subdirectories;

        public ObservableCollection<Object> Files => ((Directory)theMaster).Files;

        public string Label => Name; 

        public IEnumerable<Object> Children => ((Directory)theMaster).Children;

        public IEnumerable<File> MasterFiles => ((Directory)theMaster).MasterFiles;

        public IEnumerable<Directory> MasterSubdirectories => ((Directory)theMaster).MasterSubdirectories;

        private bool isExpanded;

        public bool IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsExpanded"));
                }
            }
        }

        public override void Export(string pathName)
        {
            (Master as Directory).Export(pathName);
        }
    }
}
