using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fourth.Starchef.Packages.Model
{
    public class HeaderFooter    /**/    
    {
        //TODO: Implement Suppress Headers/Footers checkbox functionality (waiting for Defect/US from BA/QA)

        public HeaderFooterItem Left { get; set; }
        public HeaderFooterItem Right { get; set; }
        public HeaderFooterItem Middle { get; set; }

        //returns true if all parts of Header/Footer are blank
        public bool SuppressHeaderFooter
        {
            get
            {
                return this.Left.HeaderFooterType == HeaderFooterType.Blank &&
                            this.Right.HeaderFooterType == HeaderFooterType.Blank &&
                                 this.Middle.HeaderFooterType == HeaderFooterType.Blank;
            }
        }
    }
}
