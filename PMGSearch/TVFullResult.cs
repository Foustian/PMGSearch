using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMGSearch
{
    public class TVFullResult
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
        public TVFullRequest OriginalRequest
        {
            get {return _req;}
            set {_req = value;}
        } TVFullRequest _req = null;


        /// <summary>
        /// List of matching documents (Clips).
        /// </summary>
        public List<Hit> Hits
        {
            get { return _hits; }
            set { _hits = value; }
        } List<Hit> _hits = new List<Hit>();

        public Dictionary<string, Dictionary<string,string>> Facets
        {
            get { return _facets; }
            set { _facets = value; }
        } Dictionary<string, Dictionary<string,string>> _facets = new Dictionary<string, Dictionary<string,string>>();

        public String RequestUrl
        {
            get { return _RequestUrl; }
            set { _RequestUrl = value; }
        } String _RequestUrl = string.Empty;
    }
}
