using System;

namespace Archiver.Model
{
    [Serializable()]
    public class ImageFile : Image
    {

        public ImageFile(string name, Master master)
            :base(name, master)
        {
            master.AddImage(this);
        }

        public long Size => ((File)theMaster).Size;

        public override void Export(string pathName)
        {
            (Master as File).Export(pathName);
        }
    }
}
