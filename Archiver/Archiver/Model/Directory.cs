using System;
using IO = System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Archiver.Model
{
    [Serializable()]
    public class Directory : Master
    {
        private ObservableCollection<Object> theSubdirectories;
        private ObservableCollection<Object> theFiles;

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

        public void ExpandPath()
        {
            if (Parent != null)
            {
                Parent.ExpandPath();
            }
            IsExpanded = true;
        }

        public string Label => Name;

        public override string Signature => (theSignature == string.Empty) ? (theSignature = Archive.ComputeSignature(this)) : theSignature;

        public ObservableCollection<Object> Subdirectories => theSubdirectories;

        public ObservableCollection<Object> Files => theFiles;

        internal Directory(string name)
            : base(name, string.Empty)
        {
            theSubdirectories = new ObservableCollection<Object>();
            theFiles = new ObservableCollection<Object>();
        }

        public override bool HasImages()
        {
            if (base.HasImages())
                return true;

            foreach (File file in MasterFiles)
            {
                if (file.HasImages())
                    return true;
            }

            foreach (Directory subdir in MasterSubdirectories)
            {
                if (subdir.HasImages())
                    return true;
            }

            return false;
        }

        public IEnumerable<Object> Children => theSubdirectories.AsEnumerable<Object>().Union(theFiles);

        //        public IEnumerable<FSImage> ImageChildren => Children.Where((obj) => ((FSObject)obj).IsImage).Select((obj) => (FSImage)obj);

        public IEnumerable<File> MasterFiles => theFiles.OfType<File>();

        //        public IEnumerable<FSMaster> MasterChildren => Children.Where((obj) => !((FSObject)obj).IsImage).Select((obj) => (FSMaster)obj);

        public override void RemoveAllImages()
        {
            base.RemoveAllImages();

            foreach (File file in this.MasterFiles)
            {
                file.RemoveAllImages();
            }

            foreach (Directory subdir in this.MasterSubdirectories)
            {
                subdir.RemoveAllImages();
            }

        }

        public IEnumerable<Directory> MasterSubdirectories =>
            theSubdirectories.AsEnumerable().Where((obj) => !((Object)obj).IsImage).Select((obj) => (Directory)obj);

        public override bool IsDirectory => true;

        public void AddChild(Object obj)
        {
            if (theSubdirectories.Contains(obj, OfTheSameName))
                throw new Exception("لا يمكن إتمام عملية الإضافة في هذا المكان لوجود مجلد بنفس الاسم فيه");
            else if (theFiles.Contains(obj, OfTheSameName))
                throw new Exception("لا يمكن إتمام عملية الإضافة في هذا المكان لوجود ملف بنفس الاسم فيه");


            if (obj.IsDirectory)
            {
                theSubdirectories.Add(obj);
                theSubdirectories = new ObservableCollection<Object>(theSubdirectories.OrderBy((o) => o.Name.ToLower()));
            }
            else
            {
                theFiles.Add(obj);
                theFiles = new ObservableCollection<Object>(theFiles.OrderBy((o) => o.Name.ToLower()));
            }
        }

        public void RemoveChild(Object obj)
        {
            if (obj.IsDirectory)
            {
                theSubdirectories.Remove(obj);
            }
            else
            {
                theFiles.Remove(obj);
            }
        }

        /// <summary>
        ///         TODO: Should perform byte-wise comparison and return true if the given object is an identical image of this master object
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool CompareTo(Directory other)
        {
            return true;
        }

        public override void Export(string pathName)
        {
            IO.Directory.CreateDirectory(pathName);

            foreach (Object child in Children)
            {
                child.Export(pathName + "\\" + child.Name);
            }
        }

        public override Object CopyTo(string ImageName, Directory parent)
        {
            //Make sure the parent of the new copy is not a descendant of the master directory 
            if (parent.IsDescendant(this))
                throw new Exception("لا يمكن عمل نسخة من المجلد داخل نفسه");

            return new ImageDirectory(ImageName, this) { Parent = parent };
        }

        internal void IntegrityCheck(IntegrityCheckResults results, bool Recursive)
        {
            if (IntegrityCheck(results))
            {
                if (Recursive)
                {
                    foreach (Object obj in theFiles)
                    {
                        obj.IntegrityCheck(results);
                    }

                    foreach (Object obj in theSubdirectories)
                    {
                        if (obj is Directory dir)
                        {
                            dir.IntegrityCheck(results, Recursive);
                        }
                        else
                            obj.IntegrityCheck(results);
                    }
                }
            }
        }

        internal Object GetChild(string name)
        {
            IEnumerable<Object> existingDirectories = theSubdirectories.Where((obj) => (obj.Name.ToLower() == name.ToLower()));
            if (existingDirectories.Count() > 0)
                return existingDirectories.First();

            IEnumerable<Object> existingFiles = theSubdirectories.Where((obj) => (obj.Name.ToLower() == name.ToLower()));
            if (existingFiles.Count() > 0)
                return existingFiles.First();

            return null;
        }

        public override bool MakeFirstImageMaster()
        {
            //If this directory has no images, children must have their images made master to avoid losing them when this is deleted.
            if(!base.MakeFirstImageMaster())
            {
                //Make children's images into masters
                List<Object> children = new List<Object>(Children);

                foreach (Object obj in children)
                    if (obj is Master master)
                        master.MakeFirstImageMaster();

                return false;
            }

            return true;
        }
    }
}
