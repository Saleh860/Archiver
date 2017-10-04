using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Archiver.Model
{

    enum MasterDeleteOptions
    {
        DeleteAllImages,
        MakeFirstImageMaster,
        AbortIfHasImages
    }

    [Serializable()]
    //Implementation of a physical object
    public abstract class Master: Object
    {
        protected string theSignature;
        protected Collection<Image> theImages;

        public Master(string name, string signature) 
            : base(name)
        {
            theImages = new Collection<Image>();
            theSignature = signature;
        }

        public override string Signature => theSignature;

        public IEnumerable<Image> Images => theImages;

        public Image FirstImage => theImages.Count()>0?theImages.First():null;

        public bool RemoveImage(Image image)
        {
            theImages.Remove(image);

            return (theImages.Count > 0); //more copies still there
        }

        public void DeleteImage(Image image)
        {
            RemoveImage(image);
            image.Delete();
        }

        public override bool IsImage => false;

        public void AddImage(Image image)
        {
            theImages.Add(image);
        }

        public virtual bool HasImages()
        {
            return theImages.Count > 0;
        }

        public virtual void RemoveAllImages()
        {
            foreach (Image image in theImages)
                image.Parent = null;

            theImages.Clear();
        }


        public override bool IntegrityCheck(IntegrityCheckResults results)
        {
            if (base.IntegrityCheck(results))
            {
                foreach (Image image in Images)
                {
                    //Check that the image master is this
                    if (image.Master == null)
                    {
                        results.Add(new IntegrityCheckResult("نسخة غير مرتبطة بالأصل الذي تعتبر نسخة منه", "ربط النسخة بالأصل", image, SetImageMasterToThis));
                    }
                    else if (image.Master != this)
                    {
                        results.Add(new IntegrityCheckResult("نسخة مرتبطة بأصل ولكنها معتبرة نسخة من أصل آخر", "فك ارتباط النسخة بالأصل الآخر", image, RemoveImage1));
                    }
                }
                return true;
            }
            return false;
        }

        private bool RemoveImage1(Object source)
        {
            theImages.Remove(source as Image);
            return !theImages.Contains(source);
        }

        private bool SetImageMasterToThis(Object source)
        {
            (source as Image).Master=this;
            return (source as Image).Master == this;
        }

        /// <summary>
        /// If this master object has one or more images, move the master to the location of the first image
        /// </summary>
        /// <returns>true if an image was found and made into a master</returns>
        public virtual bool MakeFirstImageMaster()
        {
            if (FirstImage is Image image)
            {
                image.MakeMaster();
                return true;
            }
            return false;
        }

        public override void Delete()
        {
            Delete(MasterDeleteOptions.AbortIfHasImages);
        }

        internal bool Delete(MasterDeleteOptions option)
        {
            if (HasImages())
            {
                switch(option)
                {
                    case MasterDeleteOptions.DeleteAllImages:

                        RemoveAllImages();
                        break;

                    case MasterDeleteOptions.MakeFirstImageMaster:

                        if(MakeFirstImageMaster())
                        {
                            FirstImage.Delete();
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    default:
                        return false;
                }

            }

            base.Delete();
            return true;
        }

    }
}
