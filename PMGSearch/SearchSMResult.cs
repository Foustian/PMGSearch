using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMGSearch
{
    public class SearchSMResult
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
        public SearchSMRequest OriginalRequest
        {
            get { return _req; }
            set { _req = value; }
        } SearchSMRequest _req = null;

        public List<SMResult> smResults
        {
            get { return _smResults; }
            set { _smResults = value; }
        } List<SMResult> _smResults;

        public String RequestUrl
        {
            get { return _RequestUrl; }
            set { _RequestUrl = value; }
        } String _RequestUrl = string.Empty;
    }

}