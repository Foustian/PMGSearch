using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMGSearch
{
    public class SearchNewsResults
    {
        public string ResponseXml
        {
            get { return _responseXML; }
            set { _responseXML = value; }
        } string _responseXML = null;

        public SearchNewsRequest OriginalRequest
        {
            get { return _req; }
            set { _req = value; }
        } SearchNewsRequest _req = null;

        public List<NewsResult> newsResults
        {
            get { return _newsResult; }
            set { _newsResult = value; }
        } List<NewsResult> _newsResult;

        public int TotalResults
        {
            get { return _TotalResults; }
            set { _TotalResults = value; }
        } int _TotalResults;

        public String RequestUrl
        {
            get { return _RequestUrl; }
            set { _RequestUrl = value; }
        } String _RequestUrl = string.Empty;
    }
}
