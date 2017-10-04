using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Archiver.Model
{
    /// <summary>
    ///     A File System Object is an abstraction of either a file or a directory.
    ///     Through this interface, the object can be identified by its name and its path
    /// </summary>
    [Serializable()]
    //Implementation of an abstract file-system object
    public abstract class Object : INotifyPropertyChanged
    {
        private Directory theParent = null;
        private DateTime dateAdded = DateTime.Now;
        private string theName;
        private bool isSelected=false;

        private void MoveTo(Directory newParent)
        {
            if (newParent != null)
            {
                //Make sure the newParent is no a child of this object
                if (newParent.IsDescendant(this))
                    throw new Exception("لا يمكن نقل المجلد داخل نفسه");
            }

            if (theParent != null)
                theParent.RemoveChild(this);

            theParent = newParent;

            if(theParent!=null)
                theParent.AddChild(this);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged == null)
                return;
            PropertyChanged(this, e);
        }

        //Public Interface
        //================

        /// <summary>
        ///        When queried, returns the parent directory or null if object is a root object.
        ///        When assigned, removes the object from the current parent directory and add it to the new parent directory, and sets the Parent property to the new parent
        /// </summary>
        public Directory Parent { get => theParent; set => MoveTo(value); }

        /// <summary>
        /// The date the object was first added to the archive
        /// </summary>
        public DateTime DateAdded => dateAdded;

        /// <summary>
        ///     Returns the name of the object relative to its containing directory
        /// </summary>
        public string Name { get => theName; set => theName = value; }

        /// <summary>
        ///     Returns the full path-name of the object, i.e. relative to its root directory
        /// </summary>
        public string FullName => (theParent == null) ? theName:theParent.FullName + "\\" + theName;

        /// <summary>
        ///     Returns true if the object is a directory, and false if the object is a file
        /// </summary>
        public abstract bool IsDirectory { get; }

        /// <summary>
        ///     Returns true if the object is a virtual copy of another object, and false if the object is a real physical object
        /// </summary>
        public abstract bool IsImage { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsSelected"));
                }
            }
        }

        /// <summary>
        /// Returns a (hopefully) unique signature to identify the content of the file-system object
        /// </summary>
        public abstract string Signature { get; }

        /// <summary>
        /// Construct a file-system object with the given name
        /// </summary>
        /// <param name="name">The initial name of the file-system object</param>
        public Object(string name) { theName = name; }

        public abstract void Export(string pathName);


        public abstract Object CopyTo(string ImageName, Directory parent);

        public bool IsDescendant(Object ancestor)
        {
            if (Parent == null)
                return false;
            else if (Parent == ancestor)
                return true;
            else
                return Parent.IsDescendant(ancestor);
        }


        public virtual bool IntegrityCheck(IntegrityCheckResults results)
        {
            if (results.Once(this))
            {
                if (this != Archive.RootDirectory)
                {
                    //Check that his object is a descendant of the RootDirectory of the current archive
                    if (Parent == null)
                        results.Add(new IntegrityCheckResult("ملف يتيم ليس مرتبطا بأي مجلد", "الربط بالمجلد الرئيسي", this, SetParentToRootDirectory));
                    else if (!IsDescendant(Archive.RootDirectory))
                    {
                        results.Add(new IntegrityCheckResult("ملف مرتبط بشجرة مجلدات يتيمة ليست مرتبطة بالمجلد الرئيسي", "ربط الملف بالمجلد الرئيسي", this, SetParentToRootDirectory));
                    }
                }
                return true;
            }
            return false;
        }

        private bool SetParentToRootDirectory(Object source)
        {
            source.Parent=Archive.RootDirectory;
            return true;
        }

        protected class OfTheSameNameComparer : IEqualityComparer<Object>
        {
            public bool Equals(Object x, Object y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(Object obj)
            {
                return obj.Name.GetHashCode();
            }
        };

        protected static OfTheSameNameComparer OfTheSameName = new OfTheSameNameComparer();

        public virtual void Delete()
        {
            Parent=null;
        }
    }
}
