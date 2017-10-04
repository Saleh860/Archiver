
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archiver.Model
{
    public delegate bool IntegrityFixer(Object source);

    public class IntegrityCheckResults : Collection<IntegrityCheckResult>
    {
        HashSet<Object> AlreadyChecked = new HashSet<Object>();

        public IntegrityCheckResults()
        {

        }

        //Return true for the object only once. Afterwards, return false for the same object 
        public bool Once(Object obj)
        {
            if (!AlreadyChecked.Contains(obj))
            {
                AlreadyChecked.Add(obj);
                return true;
            }
            else
                return false;
        }

        public override string ToString()
        {
            return this.Items.Select(obj => obj.ToString()).Aggregate(string.Empty, (x, y) => x + "\n" + y);
        }
    }


    public class IntegrityCheckResult
    {
        private string errorMessage, fixMessage;
        private Object sourceObject;
        private IntegrityFixer fixer;

        public string ErrorMessage => errorMessage;
        public string FixMessage => fixMessage;

        public IntegrityCheckResult(string errorMessage, string fixMessage, Object sourceObject, IntegrityFixer fixer)
        {
            this.errorMessage = errorMessage;
            this.fixMessage = fixMessage;
            this.sourceObject = sourceObject;
            this.fixer = fixer;
        }

        public bool Fix()
        {
            return fixer(sourceObject);
        }

        public override string ToString()
        {
            return sourceObject.FullName + " : " + errorMessage + " ==> " + fixMessage;
        }
        
    }
}
