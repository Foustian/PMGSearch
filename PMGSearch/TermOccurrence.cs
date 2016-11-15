using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMGSearch
{
    public class TermOccurrence
    {

        public int TimeOffset
        {
            set { _timeOffset = value; }
            get { return _timeOffset; }
        } int _timeOffset = 0;

        public string SurroundingText
        {
            set { _surroundingText = value; }
            get { return _surroundingText; }
        } string _surroundingText = null;

        public string SearchTerm
        {
            set { _term = value; }
            get { return _term; }
        } string _term;

    }
}
