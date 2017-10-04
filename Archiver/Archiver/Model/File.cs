using System;
using IO = System.IO;
using Microsoft.Win32;

namespace Archiver.Model
{
    [Serializable()]
    public class File: Master 
    {
        private long theSize;

        //Create an archive file object for a given (imported) file
        internal File(string name, long size, string signature)
            :base(name, signature)
        {
            theSize = size;
        }

        public override bool IsDirectory => false;

        public long Size => theSize;

        /// <summary>
        ///         Should perform byte-wise comparison and return true if the given object is an identical image of this master object
        /// </summary>
        /// <param name="FileInfo"></param>
        /// <returns></returns>
        public bool CompareTo(IO.FileSystemInfo FileInfo)
        {
            return true;
        }

        public override void Export(string pathName)
        {
            Archive.ExportFile(Signature, pathName);
        }

        public override Object CopyTo(string ImageName, Directory parent)
        {
            return new ImageFile(ImageName, this) { Parent = parent };
        }

        public override bool IntegrityCheck(IntegrityCheckResults results)
        {
            if(base.IntegrityCheck(results))
            {
                //Make sure the file exists in the FileArchive
                if (!Archive.FileExists(this))
                {
                    results.Add(new IntegrityCheckResult("الملف مفقود من الأرشيف", "استبداله بملف من مصدر آخر", this, ReplaceFileInArchive));
                }

                return true;
            }
            return false;
        }

        private bool ReplaceFileInArchive(Object source)
        {
            //Choose replacement file
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = "archive.bin",
                ReadOnlyChecked = true,
                Multiselect = false
            };

            if (dlg.ShowDialog() == true)
            {
                Archive.ReplaceFile(this, dlg.FileName);
            }

            return Archive.FileExists(this);
        }

        public override void Delete()
        {
            base.Delete();
        }
    }
}
