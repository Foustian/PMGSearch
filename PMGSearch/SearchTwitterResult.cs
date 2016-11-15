using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMGSearch
{
    public class SearchTwitterResult
    {
        /// <summary>
        /// Raw XML response from the web service
        /// </summary>
        public string ResponseXml
        {
            get { return _responseXML; }
            set { _responseXML = value; }
        } string _responseXML = null;

        /// <summary>
        /// Number of documents that matched this search request.
        /// </summary>
        public int TotalResults
        {
            get { return _TotalResults; }
            set { _TotalResults = value; }
        } int _TotalResults = 0;

        /// <summary>
        /// Original Search Request, including search terms and parameters
        /// </summary>
        public SearchTwitterRequest OriginalRequest
        {
            get { return _req; }
            set { _req = value; }
        } SearchTwitterRequest _req = null;

        public List<TwitterResult> TwitterResults
        {
            get { return _twitterResults; }
            set { _twitterResults = value; }
        } List<TwitterResult> _twitterResults;

        public String RequestUrl
        {
            get { return _RequestUrl; }
            set { _RequestUrl = value; }
        } String _RequestUrl = string.Empty;
    }
}
