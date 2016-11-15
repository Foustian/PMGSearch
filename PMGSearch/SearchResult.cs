using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMGSearch
{
    public class SearchResult
    {

        /// <summary>
        /// Raw XML response from the web service
        /// </summary>
        public string ResponseXml
        {
            get { return _responseXML; }
            set {_responseXML = value;}
        } string _responseXML = null;

        /// <summary>
        /// Number of documents that matched this search request.
        /// </summary>
        public int TotalHitCount
        {
            get { return _hitCount; }
            set { _hitCount = value; }
        } int _hitCount = 0;

        /// <summary>
        /// Original Search Request, including search terms and parameters
        /// </summary>
        public SearchRequest OriginalRequest {
            get {return _req;}
            set {_req = value;}
        } SearchRequest _req = null;


        /// <summary>
        /// List of matching documents (Clips).
        /// </summary>
        public List<Hit> Hits
        {
            get { return _hits; }
            set { _hits = value; }
        } List<Hit> _hits = new List<Hit>();

        public String RequestUrl
        {
            get { return _RequestUrl; }
            set { _RequestUrl = value; }
        } String _RequestUrl = string.Empty;
    }
}
