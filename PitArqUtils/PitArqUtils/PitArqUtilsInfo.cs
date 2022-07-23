using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace PitArqUtils
{
    public class PitArqUtilsInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "PitArq";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("8803b044-4cee-4d53-ba8d-991590b88e50");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
