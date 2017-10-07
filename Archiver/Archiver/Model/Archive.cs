using System;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using IO = System.IO;
using System.Collections.Generic;
using static Archiver.Model.Object;
using static Archiver.Model.Object.OverwriteOptions;

namespace Archiver.Model
{
    [Serializable()]
    public class Archive  
    {
        public class CantOverwriteException : Exception
        {
            public CantOverwriteException()
                : base("لا يمكن إتمام عملية الإضافة في هذا المجلد لوجود مجلد أو ملف بنفس الاسم فيه")
            {

            }
        }

        [NonSerialized()]
        static private Archive theArchive = null;

        [NonSerialized()]
        private string thePathName;

        private Directory theRootDirectory;

        private FileArchive theFileArchive;

        private DirectoryArchive theDirectoryArchive;

        internal static string PathName { get => (theArchive!=null)?theArchive.thePathName:string.Empty; }
        internal static string FileName => "archive.bin";
        internal static string DatabaseFileName(string pathName) { return pathName + "\\" + FileName ; }
        internal static Directory RootDirectory { get => theArchive.theRootDirectory; }

        internal static void Create(string pathName)
        {
            theArchive = new Archive(pathName);
            theArchive.Create();
        }

        public class ArchiveAlreadyOpenException : Exception
        {
            public ArchiveAlreadyOpenException()
                : base("أغلق الأرشيف المفتوح أولا.")
            { }
        }

        internal static void Load(string fileName)
        {
            if (theArchive != null)
                throw new ArchiveAlreadyOpenException();

            IO.FileInfo info = new IO.FileInfo(fileName);

            using (IO.Stream stream = info.OpenRead())
            { 
                BinaryFormatter formatter = new BinaryFormatter();

                theArchive = (Archive)formatter.Deserialize(stream);
            }

            theArchive.thePathName = info.DirectoryName;
        }

        public class NoOpenArchiveException:Exception
        {
            public NoOpenArchiveException()
                :base("لا يوجد أرشيف مفتوح")
            {

            }
        }

        public class ArchiveNotFoundException: Exception
        {
            public ArchiveNotFoundException()
                :base("لا يمكن حفظ الأرشيف، لا يمكن العثور على مجلد الأرشيف.")
            {

            }
        }

        internal static bool Save()
        {
            if (theArchive == null)
                throw new NoOpenArchiveException();

            if (!IO.Directory.Exists(PathName))
                throw new ArchiveNotFoundException();

            IO.Stream stream = IO.File.Open(DatabaseFileName(PathName), IO.FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, theArchive);

            stream.Close();

            return true;
        }

        public class FolderAlreadyExistsException : Exception
        {
            public FolderAlreadyExistsException()
                :base("لا يمكن حفظ الأرشيف في هذا المكان لوجود مجلد بنفس الاسم")
            {

            }
        }

        internal static void SaveAs(string pathName)
        {
            if (theArchive == null)
                throw new NoOpenArchiveException();

            if (!IO.Directory.Exists(pathName) && !IO.File.Exists(pathName))
                IO.Directory.CreateDirectory(pathName);

            if (IO.File.Exists(pathName))
                pathName = new IO.FileInfo(pathName).DirectoryName;

            theArchive.thePathName = pathName;

            Save();
        }

        private Archive(string pathName)
        {
            if (theArchive != null)
                throw new ArchiveAlreadyOpenException();

            thePathName = pathName;
        }

        private void Create()
        {
            theArchive.theFileArchive = new FileArchive();
            theArchive.theDirectoryArchive = new DirectoryArchive(theArchive.theFileArchive);

            theArchive.theRootDirectory = new Directory(thePathName);
        }

        internal static string ComputeSignature(Directory directory)
        {
            string signature;
            string[] ChildrenSignatures = new string[directory.Subdirectories.Count + directory.Files.Count];
            int i = 0;
            foreach (Object obj in directory.Children)
            {
                ChildrenSignatures[i++] = obj.Name.ToLower() + obj.Signature;
            }

            Array.Sort(ChildrenSignatures);

            char[] accumulatedSignatures = ChildrenSignatures.Aggregate(string.Empty, (str1, str2) => str1 + str2).ToCharArray();

            using (SHA256Managed sha = new SHA256Managed())
            {
                signature = System.Text.Encoding.UTF8.GetString(sha.ComputeHash(
                    System.Text.Encoding.UTF8.GetBytes(accumulatedSignatures)));
            }

            return signature;
        }

        internal static string ComputeSignature(IO.FileInfo info)
        {
            string signature;
            long size;
            using (IO.Stream stream = info.OpenRead())
            {
                using (SHA256Managed sha = new SHA256Managed())
                {
                    size = stream.Length;
                    signature = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""); // +info.Extension.ToLower();
                }
            }
            return signature;
        }

        public class PathNotFoundException : Exception
        {
            public PathNotFoundException(string filename)
                : base("لا يمكن العثور على المسار : " + filename)
            {

            }
        }

        public static void ArchiveObject(string filename, Directory parent, Object.OverwriteOptions option)
        {
            IO.DirectoryInfo dirInfo = new IO.DirectoryInfo(filename);
            IO.FileInfo fileInfo = new IO.FileInfo(filename);
            IO.FileSystemInfo Info;

            if (dirInfo.Exists)
            {
                Info = dirInfo;
            }
            else if (fileInfo.Exists)
            {
                Info = fileInfo;
            }
            else
            {
                throw new PathNotFoundException(filename);
            }

            if (parent.GetChild(Info.Name) is Object existing)
            {
                if (existing is Master master)
                {
                    switch (option)
                    {
                        case ReplaceExistingKeepImages:
                            master.Delete(Master.DeleteOptions.MakeFirstImageMaster);
                            break;

                        case ReplaceExistingReplaceImages:
                            ReplaceObject(master, filename);
                            return;

                        case KeepExistingIgnoreNew:
                            return;

                        case KeepExistingRenameNew:
                            throw new NotImplementedException();

                        default:
                            throw new CantOverwriteException();
                    }

                }
                else
                    switch (option)
                    {
                        case ReplaceExistingKeepImages:
                        case ReplaceExistingReplaceImages:
                            existing.Delete();
                            break;

                        case KeepExistingIgnoreNew:
                            return;

                        case KeepExistingRenameNew:
                            throw new NotImplementedException();

                        default:
                            throw new CantOverwriteException();
                    }
            }
            if (dirInfo.Exists)
            {
                theArchive.theDirectoryArchive.ArchiveDirectory(dirInfo, parent);
            }
            else
            {
                theArchive.theFileArchive.ArchiveFile(fileInfo, parent);
            }
        }

        public static void ArchiveObjects(string[] Filenames, Directory parent, OverwriteOptions options)
        {
            foreach (string filename in Filenames)
            {
                ArchiveObject(filename, parent, options);
            }
        }

        private static void ReplaceObject(Master master, string filename)
        {
            if (master is Directory dir)
                ReplaceDirectory(dir, filename);
            else if (master is File file)
                ReplaceFile(file, filename);
        }

        internal static bool IsOpen()
        {
            return theArchive != null;
        }

        internal static void Close()
        {
            theArchive = null;
        }

        internal static void ExportFile(string signature, string pathName)
        {
            theArchive.theFileArchive.ExportFile(signature, pathName);
        }

        internal static void IntegrityCheck(IntegrityCheckResults results)
        {
            //Check all objects accessible from the tree
            RootDirectory.IntegrityCheck(results, true);

            //Check all objects accessible from archives
            theArchive.theFileArchive.IntegrityCheck(results);
            theArchive.theDirectoryArchive.IntegrityCheck(results);

        }

        internal static bool FileExists(File file)
        {
            return theArchive.theFileArchive.FileExists(file.Size, file.Signature);
        }

        internal static void ReplaceDirectory(Directory dir, string newDirectoryName)
        {
            Object newDir = theArchive.theDirectoryArchive.ReplaceDirectory(dir, newDirectoryName);

            //If the old directory has images, they should become images of the new directory
            //But the new directory itself could be just an image of another so
            //Find out if the new directory is a master
            Directory master = newDir as Directory;

            //If the new directory is an image, make its master the master of all replaced directory images
            if (master == null)
                master = (newDir as ImageDirectory).Master as Directory;

            List<Image> dirImages = new List<Image>(dir.Images);

            foreach (Image image in dirImages)
            {
                if (master != null)
                {
                    image.Master = master;
                }
                else
                {
                    image.Delete();
                }
            }
        }



        internal static void ReplaceFile(File file, string newFileName)
        {
            Object newFile = theArchive.theFileArchive.ReplaceFile(file, newFileName);

            //If the old file has images, they should become images of the new file
            File master = newFile as File;

            if (master==null)
            {
                master = (newFile as ImageFile).Master as File;
            }

            if(master !=null)
            {
                foreach (Image image in file.Images)
                {
                    master.AddImage(image);
                    image.Master = master;
                }
                file.Parent = null;
            }
        }

        internal static void ReplaceFile(ImageFile file, string newFileName)
        {

            Object fileParent = file.Parent;
            file.Delete();
            theArchive.theFileArchive.ArchiveFile(new IO.FileInfo(newFileName), file.Parent);
        }

    }
}
