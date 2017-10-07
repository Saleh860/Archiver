using System;
using System.Collections.Generic;
using System.ComponentModel;
using Archiver.Model;

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
        public class CantOverwriteException : Exception
        {
            public CantOverwriteException()
                : base("لا يمكن إتمام عملية الإضافة في هذا المجلد لوجود مجلد أو ملف بنفس الاسم فيه")
            {

            }
        }

        public enum OverwriteOptions
        {
            ReplaceExistingKeepImages,
            ReplaceExistingReplaceImages,
            KeepExistingRenameNew,
            KeepExistingIgnoreNew,
            KeepExistingAbortWarn,
            NotSet
        }

        private Directory theParent = null;
        private DateTime dateAdded = DateTime.Now;
        private string theName;
        private bool isSelected=false;

        /// <summary>
        /// Move the object to another directory, i.e. removes the object from the current parent 
        /// directory and adds it to the new parent directory, and sets the Parent property to the new parent.
        /// </summary>
        /// <param name="newParent">The new parent directory to which the object is to be moved.</param>
        public virtual void MoveTo(Directory newParent)
        {

            //Remove from the children of current parent directory
            if (theParent != null)
                theParent.RemoveChild(this);

            theParent = newParent;

            //Add to the children of new parent directory
            if(theParent!=null)
                theParent.AddChild(this);
        }

        /// <summary>
        /// This is called internally to indicate that a property has changed and fire the event handler chain
        /// </summary>
        /// <param name="e"></param>
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
        ///        When assigned, moves the object to the new parent directory
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
        ///     Sets/Gets the isSelected flag, which indicates whether the object is currently selected.
        /// </summary>
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
        ///     Returns true if the object is a directory, and false if the object is a file
        /// </summary>
        public bool IsDirectory { get => this is Directory dir || this is ImageDirectory image; }

        /// <summary>
        ///     Returns true if the object is a virtual copy of another object, and false if the object is a real physical object
        /// </summary>
        public bool IsImage { get => this is Image image; }

        /// <summary>
        ///   Where event handlers are hooked up
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Construct a file-system object with the given name
        /// </summary>
        /// <param name="name">The initial name of the file-system object</param>
        public Object(string name) { theName = name; }

        /// <summary>
        /// Returns true if this object is a child of the given directory or a child of a descendant of the given directory
        /// </summary>
        /// <param name="ancestor"></param>
        /// <returns></returns>
        public bool IsDescendant(Directory ancestor)
        {
            if (Parent == null)
                return false;
            else if (Parent == ancestor)
                return true;
            else
                return Parent.IsDescendant(ancestor);
        }

        private bool SetParentToRootDirectory(Object source)
        {
            source.Parent=Archive.RootDirectory;
            return true;
        }

        //Overridables

        public virtual void Delete()
        {
            Parent=null;
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

        /// <summary>
        /// Returns a (hopefully) unique signature to identify the content of the file-system object
        /// </summary>
        public abstract string Signature { get; }

        public abstract void Export(string pathName);

        public abstract Object CopyTo(string ImageName, Directory parent);

        //Protected Members
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
    }
}
