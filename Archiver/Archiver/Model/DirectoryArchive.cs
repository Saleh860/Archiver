using System;
using IO = System.IO;
using System.Collections.Generic;

namespace Archiver.Model
{
    [Serializable()]
    class DirectoryArchive
    {
        private Dictionary<string, Directory> dictionary;
        private FileArchive theFileArchive;

        internal DirectoryArchive(FileArchive fileArchive)
        {
            theFileArchive = fileArchive;
            dictionary = new Dictionary<string, Directory>();
        }

        internal Directory Lookup(Directory directory)
        {

            if (dictionary.TryGetValue(directory.Signature, out Directory master))
            {
                if (master.CompareTo(directory))
                    return master;
            }
            return null;
        }

        /// <summary>
        /// Add the given directory to the archive if no directory with the same context exists
        /// Otherwise, return the existing directory which has identical content. 
        /// </summary>
        /// <param name="directory">New directory to archive</param>
        /// <returns></returns>
        internal Directory Add(Directory directory)
        {
            if (!dictionary.TryGetValue(directory.Signature, out Directory existing))
            {
                dictionary.Add(directory.Signature, directory);
                return directory;
            }
            else
            {
                return existing;
            }
        }
        //Return the parent of the master directory from which a image has been made, 
        //  or null if a new directory has been created
        internal Object ArchiveDirectory(IO.DirectoryInfo Info, Directory parent)
        {
            Directory NewDirectory = new Directory(Info.Name);

            foreach (IO.FileInfo finfo in Info.GetFiles())
            {
                theFileArchive.ArchiveFile(finfo, NewDirectory);
            }

            foreach (IO.DirectoryInfo finfo in Info.GetDirectories())
            {
                ArchiveDirectory(finfo, NewDirectory);
            }

            Directory Master = Add(NewDirectory);

            if (NewDirectory != Master)
            {
                return Master.CopyTo(Info.Name, parent);
            }
            else
            {
                NewDirectory.Parent = parent;
                return NewDirectory;
            }
        }

        internal void IntegrityCheck(IntegrityCheckResults results)
        {
            foreach ( Directory dir in dictionary.Values)
            {
                dir.IntegrityCheck(results,true);
            }
        }


        internal Object ReplaceDirectory(Directory dir, string newDirectoryName)
        {
            string theSignature = dir.Signature;

            if (dictionary.ContainsKey(theSignature))
            {
                dictionary.Remove(theSignature);
            }

            Directory dirParent = dir.Parent;
            dir.Parent = null;

            return ArchiveDirectory(new IO.DirectoryInfo(newDirectoryName), dirParent);
    }

}
}
