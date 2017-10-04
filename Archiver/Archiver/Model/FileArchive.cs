using System;
using System.Collections.Generic;
using IO = System.IO;
using System.IO.Compression;

namespace Archiver.Model
{
    [Serializable()]
    class FileArchive
    {
        Dictionary<long, Dictionary<string, File>> dictionary;

        internal FileArchive()
        {
            dictionary = new Dictionary<long, Dictionary<string, File>>();
        }

        internal File Lookup(IO.FileInfo info, string signature)
        {

            if (dictionary.TryGetValue(info.Length, out Dictionary<string, File> subdic))
            {
                if (subdic.TryGetValue(signature, out File master))
                {
                    if (master.CompareTo(info))
                        return master;
                }
            }
            return null;
        }

        //Must check first that a master file with the same hash does not already exist in the archive
        /// <summary>
        /// Add the given file to the archive 
        /// If a file with the same contents already exists return the existing file
        /// </summary>
        /// <param name="theFile">The new file to be added to the archive</param>
        /// <returns>The newly added file if no match exists, or the existing matching file</returns>
        internal File Add(File theFile)
        {
            if (!dictionary.TryGetValue(theFile.Size, out Dictionary<string, File> subdic))
            {
                subdic = new Dictionary<string, File>();
                dictionary.Add(theFile.Size, subdic);
            }

            if (!subdic.TryGetValue(theFile.Signature, out File existing))
            {
                subdic.Add(theFile.Signature, theFile);
                return theFile;
            }
            else
            {
                return existing;
            }
        }

        /// <summary>
        /// Add the file with the given info into the FileArchive under the given parent.
        /// If a file with the same conetent exists, return the existing object
        /// </summary>
        /// <param name="Info"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        internal Object ArchiveFile(IO.FileInfo Info, File New, Directory parent)
        {
            File Master = Add(New);

            if (New == Master)
            {
                New.Parent = parent;
                //Embed a compressed copy of the file contents inside the archive
                CompressFile(Info, FileName(New.Signature));

                return New;
            }
            else
            {
                return Master.CopyTo(Info.Name, parent);
            }
        }

        private string FileName(string signature)
        {
            return Archive.PathName + "\\" + signature;
        }

        private void CompressFile(IO.FileInfo info, string filename)
        {
            using (IO.FileStream output = new IO.FileStream(filename, IO.FileMode.Create))
            {
                using (DeflateStream compressor = new DeflateStream(output, CompressionMode.Compress))
                {
                    using (IO.FileStream input = info.OpenRead())
                    {
                        input.CopyTo(compressor);
                        compressor.Close();
                    }
                }
            }
        }

        internal void ExportFile(string signature, string pathName)
        {
            DecompressFile(new IO.FileInfo(FileName(signature)), pathName);
        }

        private void DecompressFile(IO.FileInfo info, string filename)
        {
            using (IO.FileStream input = info.OpenRead())
            {
                using (DeflateStream decompressor = new DeflateStream(input, CompressionMode.Decompress))
                {
                    using (IO.FileStream output = new IO.FileStream(filename, IO.FileMode.CreateNew))
                    {
                        decompressor.CopyTo(output);
                        decompressor.Close();
                    }
                }
            }
        }

        internal bool FileExists(long theSize, string theSignature)
        {
            if (dictionary.TryGetValue(theSize, out Dictionary<string, File> subdic))
            {
                if (subdic.ContainsKey(theSignature))
                {
                    return IO.File.Exists(FileName(theSignature));
                }
            }
            return false;
         }


        internal Object ReplaceFile(File file, string newFileName)
        {
            string theSignature = file.Signature;
            long theSize = file.Size;

            if (dictionary.TryGetValue(theSize, out Dictionary<string, File> subdic))
            {
                if (subdic.ContainsKey(theSignature))
                {
                    if (IO.File.Exists(FileName(theSignature)))
                        IO.File.Delete(FileName(theSignature));
                    subdic.Remove(theSignature);
                }
                if (subdic.Count == 0)
                    dictionary.Remove(theSize);
            }

            return ArchiveFile(new IO.FileInfo(newFileName), file.Parent);
        }

        internal Object ArchiveFile(IO.FileInfo finfo, Directory parent)
        {
            File NewFile = new File(finfo.Name, finfo.Length, Archive.ComputeSignature(finfo));

            return ArchiveFile(finfo, NewFile, parent);
        }

        internal void IntegrityCheck(IntegrityCheckResults results)
        {
            foreach (Dictionary<string, File> subdic in dictionary.Values)
            {
                foreach(File file in subdic.Values)
                {
                    file.IntegrityCheck(results);
                }
            }
        }
    }
}
