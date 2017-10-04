using System;
using System.Linq;

namespace Archiver.Model
{
    [Serializable()]
    public abstract class Image : Object
    {
        protected Master theMaster;

        public Image(string name, Master master)
            : base(name)
        {
            theMaster = master;
        }

        public Master Master
        {
            get => theMaster;

            set
            {
                if (theMaster != null)
                {
                    theMaster.RemoveImage(this);
                }

                theMaster = value;

                if (theMaster != null)
                {
                    theMaster.AddImage(this);
                }
            }
        }

        public override bool IsImage => true;

        public override string Signature => theMaster.Signature;

        public override Object CopyTo(string ImageName, Directory parent)
        {
            return Master.CopyTo(ImageName, parent);
        }

        public override void Delete()
        {
            Master.RemoveImage(this);

            base.Delete();
        }

        internal void MakeMaster()
        {
            string imageName = Name;
            Directory imageParent = Parent;
            Parent = null;
            Name = Master.Name;

            Directory masterParent = Master.Parent;
            Master.Parent = null;
            Master.Name = imageName;

            Parent = masterParent;

            Master.Parent = imageParent;
        }

    }
}
