using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Configuration;
using System.Xml.Linq;
using System.Web;
using Sentiment.Logic;
using Sentiment.HelperClasses;
using System.Threading;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace PMGSearch
{
    /// <summary>
    /// Hey! Why didn't you use XMLSerializer for this? 
    /// Because I already had code laying around to do it this way and it's a bit lower overhead.
    /// If you want to change it, be my guest.
    /// </summary>
    /// 

    public enum SourceType
    {
        [Description("Social Media")]
        SocialMedia = 999,
        [Description("Online News")]
        OnlineNews = 1,
        [Description("Blog")]
        Blog = 2,
        [Description("Forum")]
        Forum = 3,
        [Description("Classified")]
        Classified = 4,
        [Description("Comment")]
        Comment = 5,
        [Description("Microblog")]
        Microblog = 6,
        [Description("Podcast")]
        Podcast = 7,
        [Description("Q&A")]
        QnA = 8,
        [Description("Review")]
        Review = 9,
        [Description("Social Network")]
        SocialNetwork = 10,
        [Description("Social Photo")]
        SocialPhoto = 11,
        [Description("Social Video")]
        SocialVideo = 12,
        [Description("Wiki")]
        Wiki = 13,
        [Description("Print")]
        Print = 14
    }

    public enum TVRegion
    {
        Canada = 650,
        LatinAmerica = 600
    }

    public class SearchEngine
    {
        /// <summary>
        /// URL of the RESTSearch web service to connect to
        /// </summary>
        public System.Uri Url
        {
            get { return _Url; }
            set { _Url = value; }
        } System.Uri _Url;



        public SearchEngine(System.Uri url)
        {
            this.Url = url;
            //this.Url = new Uri("http://10.100.1.48:8080/solr/core0/select/");
        }

        /// <summary>
        /// Perform a search on the RESTSearch service
        /// </summary>
        /// <param name="request">Parameters for the search request</param>
        /// <returns>SearchResult object encapsulating results and associated metadata</returns>
        public SearchResult Search(SearchRequest request, Int32? timeOutPeriod = null, Boolean isSentimentCall = false, string CustomSolrFl = "")
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SearchResult res = new SearchResult();

            try
            {


                CommonFunction.LogInfo("PMG Call Start", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Create Request", request.IsPmgLogging, request.PmgLogFileLocation);


                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q qeury parameter of solr and 
                // 'FQuery' we will pass in the fq qeury parameter of solr 
                StringBuilder Query = new StringBuilder();
                StringBuilder FQuery = new StringBuilder();

                /* Commented below condition as Now we are passing pagenumber based on 0 */
                /*if (request.PageNumber <= 0)
                {
                    request.PageNumber = 1;
                }*/

                string Fl = string.Empty;
                string FlTitle120 = string.Empty;
                string FlTitleOnSearchTerm = string.Empty;

                if (!string.IsNullOrWhiteSpace(request.Terms))
                {
                    string RequestTerm = string.Empty;

                    RequestTerm = request.Terms.Trim();

                    // if our search term starts with char '#' then
                    // we understand that user wants exact search without sysnonym.
                    // e.g. 'Find' , will only find terms with 'Find' and skip terms like 'Finding', 'Found' , ect...
                    // we added that if search term is fuzzy , then we do make search in only CCgen and not CC

                    if (RequestTerm.EndsWith("#"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            Fl = "cc_gen";
                            FlTitleOnSearchTerm = "title120_gen";
                        }
                        else
                        {
                            Fl = "cc";
                            FlTitleOnSearchTerm = "title120";
                        }
                    }
                    else
                    {
                        Fl = "cc_gen";
                        FlTitleOnSearchTerm = "title120_gen";
                    }


                    /*if (RequestTerm.EndsWith("#") || Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        Fl = "CCgen";
                    }
                    else
                    {
                        Fl = "CC";
                    }*/


                    // now if term is enclosed in double quote and it is phrase , then we must have to put slop ~2
                    // e.g. user search term "Hello World"
                    // and our CC may like "Hello 190s: World"
                    // although it is practically contious text but logically it has a word in between '190s:' 
                    // which will not allow to come in search results ,
                    // by making slop ~2 we will allow to return all search terms 
                    // which have gap of max. 2 words between them.
                    // Note : solr consider 190s: as 2 words , one is '190' (numeric) and seond is 's:' (alphabetic)

                    // New Note : we'll change the search term to lower case , but doing this
                    // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                    // we'll apply ~2 only on CC and not in CCgen. as CCgen is exact search
                    List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                    RequestTerm = Regex.Replace(
                                RequestTerm,
                                @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                        //@"([\""][\w ]+[\""])|(\w+)",
                                m => reservedWords.Contains(m.Value) ? m.Value : m.Value
                                );

                    if (request.IsTitleNContentSearch)
                    {
                        Query = Query.AppendFormat("({0}:({1}) OR {2}:({1}))", Fl, RequestTerm, FlTitleOnSearchTerm);
                    }
                    else
                    {
                        Query = Query.AppendFormat("{0}:({1})", Fl, RequestTerm);
                    }

                    // add term vector params
                    //vars.Add(new KeyValuePair<string, string>("tv", "true"));
                    //vars.Add(new KeyValuePair<string, string>("tv.fl", "CCgen"));
                    //vars.Add(new KeyValuePair<string, string>("tv.tf", "true"));
                    //vars.Add(new KeyValuePair<string, string>("tv.positions", "true"));
                }
                /*else
                {
                    // as we do not search on CC 
                    // we should turn of highligting fearure 
                    vars.Add(new KeyValuePair<string, string>("hl", "off"));
                    //vars.Add(new KeyValuePair<string, string>("tv", "false"));
                }*/

                // if appering is passed , then lets search on appearing
                if (!string.IsNullOrEmpty(request.Appearing))
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" (appearing:({0}) OR desc100:({0}))", request.Appearing);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND (appearing:({0}) OR desc100:({0}))", request.Appearing);
                    }
                }

                // if description is passed , then lets search on description
                if (!string.IsNullOrEmpty(request.Desc100))
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" desc100:({0})", request.Desc100);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND desc100:({0})", request.Desc100);
                    }
                }

                // if title is passed , then lets search on title
                if (!string.IsNullOrEmpty(request.Title120))
                {

                    string RequestTerm = string.Empty;
                    RequestTerm = request.Title120.Trim();

                    // if our search term starts with char '#' then
                    // we understand that user wants exact search without sysnonym.
                    // e.g. 'Find' , will only find terms with 'Find' and skip terms like 'Finding', 'Found' , ect...
                    // we added that if search term is fuzzy , then we do make search in only CCgen and not CC

                    if (RequestTerm.EndsWith("#"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            FlTitle120 = "title120_gen";
                        }
                        else
                        {
                            FlTitle120 = "title120";
                        }
                    }
                    else
                    {
                        FlTitle120 = "title120_gen";
                    }

                    // New Note : we'll change the search term to lower case , but doing this
                    // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                    // we'll apply ~2 only on CC and not in CCgen. as CCgen is exact search
                    List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                    RequestTerm = Regex.Replace(
                                RequestTerm,
                                @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                        //@"([\""][\w ]+[\""])|(\w+)",
                                m => reservedWords.Contains(m.Value) ? m.Value : m.Value
                                );

                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" {0}:({1})", FlTitle120, RequestTerm);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND {0}:({1})", FlTitle120, RequestTerm);
                    }
                }


                if (!string.IsNullOrEmpty(request.Title120) || !string.IsNullOrEmpty(request.Terms))
                {



                    // if user has searched with CC , then only we need to pass below params to solr.
                    // as we need to give highlight on CC ,  for user searched term.
                    // all these feilds are for highlighting functionality
                    // hl.fl =  name of the feild on which need to provide highlighting
                    // hl = value can be on/off , if on then highlighting feature is enabled otherwise disabled.
                    // hl.maxAnalyzedChars =  default max char length for highlight is 51200 , but we need unlimited
                    vars.Add(new KeyValuePair<string, string>("hl.fl", string.IsNullOrEmpty(request.Terms) ? FlTitle120 : Fl + (string.IsNullOrEmpty(request.Title120) ? string.Empty : "," + FlTitle120)));
                    vars.Add(new KeyValuePair<string, string>("hl.requireFieldMatch", "true"));
                    vars.Add(new KeyValuePair<string, string>("hl", "on"));
                    if (string.IsNullOrEmpty(request.Title120) && !request.IsTitleNContentSearch)
                    {
                        vars.Add(new KeyValuePair<string, string>("hl.maxAnalyzedChars", "2147483647"));
                    }
                    else
                    {
                        vars.Add(new KeyValuePair<string, string>("f." + Fl + ".hl.maxAnalyzedChars", "2147483647"));
                        //vars.Add(new KeyValuePair<string, string>("f." + ((string.IsNullOrWhiteSpace(FlTitle120))?FlTitleOnSearchTerm:FlTitle120) + ".hl.maxAnalyzedChars", "500"));

                        vars.Add(new KeyValuePair<string, string>("f." + (string.IsNullOrWhiteSpace(FlTitle120) ? FlTitleOnSearchTerm : FlTitle120) + ".hl.maxAnalyzedChars", "500"));
                    }

                    // as our CC text is very long , we will get exact closed-caption 
                    // only at time of showing it while we play video
                    // in all other cases we just need to display no. of hits and not the cc text.
                    // so we'll process it only at time of showing it 
                    // and in other cases we'll get count for hits from solr highlights
                    if (request.IsShowCC)
                    {
                        // hl.fragsize = char size for fragment for highlight , 
                        // by setting it to 0 ,it will not fragment and return whole CC in sigle highlight. 
                        vars.Add(new KeyValuePair<string, string>("hl.fragsize", "0"));

                    }
                    else
                    {
                        // by setting it to 145 ,it will return no. of highlights 
                        // fragment size for signle highlight is 145 (approx)
                        if (request.FragSize.HasValue)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", request.FragSize.Value.ToString()));
                        else if (!isSentimentCall)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSize"]));
                        else
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSizeForSentiment"]));

                        vars.Add(new KeyValuePair<string, string>("hl.snippets", "99"));
                    }
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("hl", "off"));
                }

                //  if guid(s) are passed , then lets make search Guid List , only that guid data will return
                if (!string.IsNullOrEmpty(request.GuidList))
                {
                    string[] _RLGUIDs = request.GuidList.Split(',');

                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" (");
                    }
                    else
                    {
                        Query = Query.Append(" AND (");
                    }

                    bool IsFirst = true;

                    foreach (string _RLGUID in _RLGUIDs)
                    {
                        if (IsFirst)
                        {
                            Query = Query.AppendFormat(" guid:{0}", _RLGUID);
                            IsFirst = false;
                        }
                        else
                        {
                            Query = Query.AppendFormat(" OR guid:{0}", _RLGUID);
                        }
                    }

                    Query = Query.Append(" )");
                }

                //  if IQCCKey(s) are passed , then lets make search IQCCKey List , only that IQCCKey data will return
                if (!string.IsNullOrEmpty(request.IQCCKeyList))
                {
                    string[] _IQCCKeys = request.IQCCKeyList.Split(',');

                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" (");
                    }
                    else
                    {
                        Query = Query.Append(" AND (");
                    }

                    bool IsFirstIQCCKey = true;

                    foreach (string _IQCCKey in _IQCCKeys)
                    {
                        if (IsFirstIQCCKey)
                        {
                            IsFirstIQCCKey = false;
                            Query = Query.AppendFormat(" iq_cc_key:{0}", _IQCCKey);
                        }
                        else
                        {
                            Query = Query.AppendFormat(" OR iq_cc_key:{0}", _IQCCKey);
                        }
                    }

                    Query = Query.Append(" )");
                }

                // ooops nothing passed for 'q' search of solr.
                // then as q is complesary search
                // if nothing is passed then we should pass *:*
                // which mean it return all without making search for q.
                // although after q , it will filter on fq (filter query) search. 
                if (string.IsNullOrEmpty(Query.ToString()))
                {
                    //Query = Query.Append("*:*");
                    Query = Query.Append("isdeleted:false");
                }
                else
                {
                    Query = Query.Append(" AND isdeleted:false");
                }

                string SortFields = string.Empty;

                // lets make solr fields and pass to solr
                if (!string.IsNullOrEmpty(request.SortFields))
                {
                    SortFields = GenerateSortField(request.SortFields);
                }

                if (request.SortFields != null)
                {
                    vars.Add(new KeyValuePair<string, string>("sort", SortFields));
                }

                // if both start and enddate is passed then lets make search on RL_Station_DateTime.
                if (request.StartDate != null && request.EndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" gmtdatetime_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND gmtdatetime_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                // if both start and enddate is passed then lets make search on RL_Station_DateTime.
                if (request.RLStationStartDate != null && request.RLStationEndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" datetime_dt:[");
                        FQuery = FQuery.Append(request.RLStationStartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.RLStationEndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND datetime_dt:[");
                        FQuery = FQuery.Append(request.RLStationStartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.RLStationEndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                // if both start Clip Date and end Clip Date is passed then lets make search on ClipDate.
                if (request.ClipStartDate != null && request.ClipEndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" date:[");
                        FQuery = FQuery.Append(request.ClipStartDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append(" TO ");
                        FQuery = FQuery.Append(request.ClipEndDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND date:[");
                        FQuery = FQuery.Append(request.ClipStartDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append(" TO ");
                        FQuery = FQuery.Append(request.ClipEndDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("]");
                    }
                }

                // if station(s) are passed , then lets make search Stations
                if (request.Stations != null && request.Stations.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append("(");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstStation = true;

                    foreach (string _Station in request.Stations)
                    {
                        if (IsFirstStation)
                        {
                            IsFirstStation = false;
                            FQuery = FQuery.AppendFormat(" stationid:{0}", _Station);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR stationid:{0}", _Station);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if hours are passed , then lets make search for hours
                if (request.Hours != null && request.Hours.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstHour = true;

                    foreach (string _Hour in request.Hours)
                    {
                        if (IsFirstHour)
                        {
                            IsFirstHour = false;
                            FQuery = FQuery.AppendFormat(" hour:{0}", _Hour);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR hour:{0}", _Hour);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }


                // if IQClassNum(s) are passed , then lets make search for IQClassNum(s)
                if (request.IQClassNum != null && request.IQClassNum.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQClass = true;

                    foreach (string _IQ_Class_Num in request.IQClassNum)
                    {
                        if (IsFirstIQClass)
                        {
                            IsFirstIQClass = false;
                            FQuery = FQuery.AppendFormat(" iq_class_num:{0}", _IQ_Class_Num);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR iq_class_num:{0}", _IQ_Class_Num);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }


                // if IQDmaName(s) are passed , then lets make search for IQDmaName(s)
                if (request.IQDmaName != null && request.IQDmaName.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (string _IQ_Dma_Name in request.IQDmaName)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" market:\"{0}\"", _IQ_Dma_Name);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR market:\"{0}\"", _IQ_Dma_Name);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if IQDmaNum(s) are passed , then lets make search for IQDmaNum(s)
                if (request.IQDmaNum != null && request.IQDmaNum.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (string _IQ_Dma_Num in request.IQDmaNum)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" iq_dma_num:{0}", _IQ_Dma_Num);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR iq_dma_num:{0}", _IQ_Dma_Num);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if StationAffil(s) are passed , then lets make search for StationAffil(s)
                if (request.StationAffil != null && request.StationAffil.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstStationAffilName = true;

                    foreach (string _Station_Affil in request.StationAffil)
                    {
                        if (IsFirstStationAffilName)
                        {
                            IsFirstStationAffilName = false;
                            FQuery = FQuery.AppendFormat(" affiliate:\"{0}\"", _Station_Affil);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR affiliate:\"{0}\"", _Station_Affil);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }


                // if StationAffilNum(s) are passed , then lets make search for StationAffilNum(s)
                if (request.StationAffilNum != null && request.StationAffilNum.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstStationAffilName = true;

                    foreach (string _Station_Affil_Num in request.StationAffilNum)
                    {
                        if (IsFirstStationAffilName)
                        {
                            IsFirstStationAffilName = false;
                            FQuery = FQuery.AppendFormat(" station_affil_num:{0}", _Station_Affil_Num);

                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR station_affil_num:{0}", _Station_Affil_Num);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if TimeZone is passed , then lets make search for TimeZone
                if (!string.IsNullOrEmpty(request.TimeZone) && request.TimeZone.ToLower(System.Globalization.CultureInfo.CurrentCulture) != "all")
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" timezone:");
                        FQuery = FQuery.Append(request.TimeZone);
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND timezone:");
                        FQuery = FQuery.Append(request.TimeZone);
                    }
                }

                // if TVREgion is passed , then lets make search for TimeZone
                /*if (request.TVRegions != null && request.TVRegions.Count > 0)
                {
                    if (!(request.TVRegions.Contains(TVRegion.Canada.ToString()) && request.TVRegions.Contains(TVRegion.LatinAmerica.ToString())))
                    {
                        if (!string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.Append(" AND");
                        }
                        if (request.TVRegions.Contains(TVRegion.Canada.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" -(iq_dma_num:{0})", (int)TVRegion.Canada);
                        }
                        else if (request.TVRegions.Contains(TVRegion.LatinAmerica.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" -(iq_dma_num:{0})", (int)TVRegion.LatinAmerica);
                        }
                        else
                        {
                            FQuery = FQuery.Append(" -(iq_dma_num:{0}) AND -(iq_dma_num:{1})", (int)TVRegion.Canada, (int)TVRegion.LatinAmerica);
                        }
                    }
                }*/

                if (request.IncludeRegionsNum != null && request.IncludeRegionsNum.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append("(");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstRegion = true;

                    foreach (int _Region in request.IncludeRegionsNum)
                    {
                        if (IsFirstRegion)
                        {
                            IsFirstRegion = false;
                            FQuery = FQuery.AppendFormat(" region_num:{0}", _Region);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR region_num:{0}", _Region);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                if (request.CountryNums != null && request.CountryNums.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append("(");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstCountry = true;

                    foreach (int _Country in request.CountryNums)
                    {
                        if (IsFirstCountry)
                        {
                            IsFirstCountry = false;
                            FQuery = FQuery.AppendFormat(" country_num:{0}", _Country);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR country_num:{0}", _Country);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // our q query is ready!!! 
                // lets add it to keyvalue pair for q,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                // as fq is not compelsary search 
                // we will pass it only if there is search criteria on fq 
                if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                {
                    vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                }

                // to make search effective and to reduce search time ,
                // we should only retrun the fields which are required
                // so lets add fields that we need to return , this is config driven,
                // to add any field , we just need to change in config :)
                if (!isSentimentCall)
                {
                    string fl = string.Empty;
                    if (string.IsNullOrEmpty(CustomSolrFl))
                    {
                        fl = System.Configuration.ConfigurationManager.AppSettings["SolrFL"];
                        if (string.IsNullOrEmpty(request.Terms) && request.IsShowCC == true)
                            fl = fl + ",cc";
                    }
                    else
                    {
                        fl = CustomSolrFl;
                    }
                    vars.Add(new KeyValuePair<string, string>("fl", fl));
                }
                else
                    vars.Add(new KeyValuePair<string, string>("fl", "null"));

                // start and rows return the required page. No. data.
                if (request.Start != null)
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.Start), System.Globalization.CultureInfo.CurrentCulture)));
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                }
                vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                //vars.Add(new KeyValuePair<string, string>("wt", "json"));


                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                string RequestURL = string.Empty;
                if (request.IsOutRequest)
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                    res.RequestUrl = RequestURL;
                }
                else
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                }

                //CommonFunction.LogInfo("Response Xml :\n " + res.ResponseXml, request.IsPmgLogging, request.PmgLogFileLocation);


                CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Solr Response - TimeTaken - for get response" + string.Format("with thread : Minutes :{0}\n Seconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                XmlDocument xDoc = new XmlDocument();

                // lets load solr response to xml so we can get data in xmk format
                xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                res.OriginalRequest = request;

                CommonFunction.LogInfo("Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);

                // lets parse response. to our 'SearchResult' object
                if (!isSentimentCall)
                    parseResponse(xDoc, res, Fl, FlTitle120);
                else
                    parseSentiment(xDoc, res, Fl);

                if (request.IsSentiment && !string.IsNullOrWhiteSpace(request.Terms) && request.LowThreshold != null && request.HighThreshold != null)
                {
                    makeSentimentRequest(res, timeOutPeriod);
                }

                sw.Stop();

                CommonFunction.LogInfo("Solr Response - TimeTaken - for parse response" + string.Format("with thread : Minutes :{0}\n Seconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo(string.Format("Total Hti Count :{0}", res.TotalHitCount), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("PMG Call End", request.IsPmgLogging, request.PmgLogFileLocation);

                return res;
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        public SearchResult SearchTVChart(SearchRequest request, out Boolean isError, Int32? timeOutPeriod = null)
        {

            SearchResult res = new SearchResult();
            isError = false;
            try
            {
                try
                {
                    // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                    List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                    // 'Query' , we will pass in q qeury parameter of solr and 
                    // 'FQuery' we will pass in the fq qeury parameter of solr 
                    StringBuilder Query = new StringBuilder();
                    StringBuilder FQuery = new StringBuilder();



                    // start and rows return the required page. No. data.
                    //vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber - 1) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                    vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));

                    if (!string.IsNullOrWhiteSpace(request.Terms))
                    {
                        string Fl = string.Empty;
                        string RequestTerm = string.Empty;
                        string FlTitleOnSearchTerm = string.Empty;
                        RequestTerm = request.Terms.Trim();

                        // if our search term starts with char '#' then
                        // we understand that user wants exact search without sysnonym.
                        // e.g. 'Find' , will only find terms with 'Find' and skip terms like 'Finding', 'Found' , ect...
                        // we added that if search term is fuzzy , then we do make search in only CCgen and not CC

                        if (RequestTerm.EndsWith("#"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                            {
                                RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                                Fl = "cc_gen";
                                FlTitleOnSearchTerm = "title120_gen";
                            }
                            else
                            {
                                Fl = "cc";
                                FlTitleOnSearchTerm = "title120";
                            }
                        }
                        else
                        {
                            Fl = "cc_gen";
                            FlTitleOnSearchTerm = "title120_gen";
                        }


                        /*if (RequestTerm.EndsWith("#") || Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            Fl = "CCgen";
                        }
                        else
                        {
                            Fl = "CC";
                        }*/

                        // now if term is enclosed in double quote and it is phrase , then we must have to put slop ~2
                        // e.g. user search term "Hello World"
                        // and our CC may like "Hello 190s: World"
                        // although it is practically contious text but logically it has a word in between '190s:' 
                        // which will not allow to come in search results ,
                        // by making slop ~2 we will allow to return all search terms 
                        // which have gap of max. 2 words between them.
                        // Note : solr consider 190s: as 2 words , one is '190' (numeric) and seond is 's:' (alphabetic)

                        // New Note : we'll change the search term to lower case , but doing this
                        // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                        // we'll apply ~2 only on CC and not in CCgen. as CCgen is exact search
                        List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                        RequestTerm = Regex.Replace(
                                    RequestTerm,
                                    @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                            //@"([\""][\w ]+[\""])|(\w+)",
                                    m => reservedWords.Contains(m.Value) ? m.Value : m.Value
                                    );

                        if (request.IsTitleNContentSearch)
                        {
                            Query = Query.AppendFormat("({0}:({1}) OR {2}:({1}))", Fl, RequestTerm, FlTitleOnSearchTerm);
                        }
                        else
                        {
                            Query = Query.AppendFormat("{0}:({1})", Fl, RequestTerm);
                        }

                    }
                    else
                    {
                        // as we do not search on CC 
                        // we should turn of highligting fearure 
                        vars.Add(new KeyValuePair<string, string>("hl", "off"));
                    }

                    // if appering is passed , then lets search on appearing
                    if (!string.IsNullOrEmpty(request.Appearing))
                    {
                        if (string.IsNullOrEmpty(Query.ToString()))
                        {
                            Query = Query.AppendFormat(" (appearing:({0}) OR desc100:({0}))", request.Appearing);
                        }
                        else
                        {
                            Query = Query.AppendFormat(" AND (appearing:({0}) OR desc100:({0}))", request.Appearing);
                        }
                    }

                    // if title is passed , then lets search on title
                    if (!string.IsNullOrEmpty(request.Title120))
                    {
                        string RequestTerm = string.Empty;
                        RequestTerm = request.Title120.Trim();
                        string FlTitle120 = string.Empty;
                        // if our search term starts with char '#' then
                        // we understand that user wants exact search without sysnonym.
                        // e.g. 'Find' , will only find terms with 'Find' and skip terms like 'Finding', 'Found' , ect...
                        // we added that if search term is fuzzy , then we do make search in only CCgen and not CC

                        if (RequestTerm.EndsWith("#"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                            {
                                RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                                FlTitle120 = "title120_gen";
                            }
                            else
                            {
                                FlTitle120 = "title120";
                            }
                        }
                        else
                        {
                            FlTitle120 = "title120_gen";
                        }

                        // New Note : we'll change the search term to lower case , but doing this
                        // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                        // we'll apply ~2 only on CC and not in CCgen. as CCgen is exact search
                        List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                        RequestTerm = Regex.Replace(
                                    RequestTerm,
                                    @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                            //@"([\""][\w ]+[\""])|(\w+)",
                                    m => reservedWords.Contains(m.Value) ? m.Value : m.Value
                                    );

                        if (string.IsNullOrEmpty(Query.ToString()))
                        {
                            Query = Query.AppendFormat(" {0}:({1})", FlTitle120, RequestTerm);
                        }
                        else
                        {
                            Query = Query.AppendFormat(" AND {0}:({1})", FlTitle120, RequestTerm);
                        }

                    }

                    // ooops nothing passed for 'q' search of solr.
                    // then as q is complesary search
                    // if nothing is passed then we should pass *:*
                    // which mean it return all without making search for q.
                    // although after q , it will filter on fq (filter query) search. 
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        //Query = Query.Append("*:*");
                        Query = Query.Append("isdeleted:false");
                    }
                    else
                    {
                        Query = Query.Append(" AND isdeleted:false");
                    }




                    string SortFields = string.Empty;

                    // lets make solr fields and pass to solr
                    if (!string.IsNullOrEmpty(request.SortFields))
                    {
                        SortFields = GenerateSortField(request.SortFields);
                    }

                    if (request.SortFields != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("sort", SortFields));
                    }

                    if (request.Facet)
                    {
                        vars.Add(new KeyValuePair<string, string>("facet", "on"));


                        if (!string.IsNullOrEmpty(request.FacetRangeOther))
                        {
                            vars.Add(new KeyValuePair<string, string>("facet.range.other", request.FacetRangeOther));

                        }

                        if (!string.IsNullOrWhiteSpace(request.FacetField))
                        {
                            vars.Add(new KeyValuePair<string, string>("facet.field", request.FacetField));
                            vars.Add(new KeyValuePair<string, string>("facet.limit", "-1"));
                        }

                        if (!string.IsNullOrWhiteSpace(request.FacetRange))
                        {
                            vars.Add(new KeyValuePair<string, string>("facet.range", request.FacetRange));

                        }
                        if (request.FacetRangeStarts != null && request.FacetRangeEnds != null)
                        {
                            vars.Add(new KeyValuePair<string, string>("facet.range.start", request.FacetRangeStarts.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
                            vars.Add(new KeyValuePair<string, string>("facet.range.end", request.FacetRangeEnds.Value.AddSeconds(1).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));

                        }

                        if (request.FacetRangeGap != null && request.FacetRangeGapDuration != null)
                        {
                            vars.Add(new KeyValuePair<string, string>("facet.range.gap", "+" + request.FacetRangeGapDuration + request.FacetRangeGap));
                            //FQuery = FQuery.Append("&facet.range.gap=%2B" + request.FacetRangeGapDuration + request.FacetRangeGap);
                        }
                        if (request.wt != null)
                        {
                            vars.Add(new KeyValuePair<string, string>("wt", request.wt.ToString()));
                        }
                    }

                    // if both start and enddate is passed then lets make search on RL_Station_DateTime.
                    if (request.StartDate != null && request.EndDate != null)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.Append(" gmtdatetime_dt:[");
                            FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                            FQuery = FQuery.Append("Z TO ");
                            FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                            FQuery = FQuery.Append("Z]");
                        }
                        else
                        {
                            FQuery = FQuery.Append(" AND gmtdatetime_dt:[");
                            FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                            FQuery = FQuery.Append("Z TO ");
                            FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                            FQuery = FQuery.Append("Z]");
                        }
                    }

                    // if IQClassNum(s) are passed , then lets make search for IQClassNum(s)
                    if (request.IQClassNum != null && request.IQClassNum.Count() > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.Append(" (");
                        }
                        else
                        {
                            FQuery = FQuery.Append(" AND (");
                        }

                        bool IsFirstIQClass = true;

                        foreach (string _IQ_Class_Num in request.IQClassNum)
                        {
                            if (IsFirstIQClass)
                            {
                                IsFirstIQClass = false;
                                FQuery = FQuery.AppendFormat(" iq_class_num:{0}", _IQ_Class_Num);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR iq_class_num:{0}", _IQ_Class_Num);
                            }
                        }

                        FQuery = FQuery.Append(" )");
                    }

                    // if IQDmaName(s) are passed , then lets make search for IQDmaName(s)
                    if (request.IQDmaName != null && request.IQDmaName.Count() > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.Append(" (");
                        }
                        else
                        {
                            FQuery = FQuery.Append(" AND (");
                        }

                        bool IsFirstIQDmaName = true;

                        foreach (string _IQ_Dma_Name in request.IQDmaName)
                        {
                            if (IsFirstIQDmaName)
                            {
                                IsFirstIQDmaName = false;
                                FQuery = FQuery.AppendFormat(" market:\"{0}\"", _IQ_Dma_Name);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR market:\"{0}\"", _IQ_Dma_Name);
                            }
                        }

                        FQuery = FQuery.Append(" )");
                    }

                    if (request.StationAffil != null && request.StationAffil.Count() > 0)
                    {
                        FQuery = FQuery.Append((FQuery.Length == 0 ? "(affiliate:" : " AND (affiliate:") + string.Join(" OR affiliate:", request.StationAffil.Select(affil => "\"" + affil + "\"")) + ")");
                    }                    
                    
                    List<String> allStationCategoryNum = new List<String>();
                    foreach (KeyValuePair<Dictionary<String, String>, List<String>> kvp in request.AffilForFacet)
                    {
                        if (kvp.Value.Count > 0)
                        {
                            foreach (KeyValuePair<String, String> kvpstationNumName in kvp.Key)
                            {
                                allStationCategoryNum.Add(kvpstationNumName.Key);
                            }

                            String stationIDFormat = string.Empty;
                            foreach (String value in kvp.Value)
                            {
                                if (string.IsNullOrWhiteSpace(stationIDFormat))
                                {
                                    stationIDFormat = "stationid:" + value;
                                }
                                else
                                {
                                    stationIDFormat = stationIDFormat + " OR stationid:" + value;
                                }
                            }
                            vars.Add(new KeyValuePair<string, string>("fq", "{!tag=" + kvp.Key.First().Key + "}" + stationIDFormat));
                        }
                    }


                    foreach (String affilCat in allStationCategoryNum)
                    {
                        string csvList = string.Join(",", (from sta in allStationCategoryNum
                                                           where sta != affilCat
                                                           select sta).ToArray());
                        vars.Add(new KeyValuePair<string, string>("facet.range", "{!key=" + affilCat + " ex=" + csvList + "}gmtdatetime_dt"));
                    }

                    // if TimeZone is passed , then lets make search for TimeZone
                    if (!string.IsNullOrEmpty(request.TimeZone) && request.TimeZone.ToLower(System.Globalization.CultureInfo.CurrentCulture) != "all")
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.Append(" timezone:");
                            FQuery = FQuery.Append(request.TimeZone);
                        }
                        else
                        {
                            FQuery = FQuery.Append(" AND timezone:");
                            FQuery = FQuery.Append(request.TimeZone);
                        }
                    }

                    // if TVREgion is passed , then lets make search for TimeZone
                    /*if (request.TVRegions != null && request.TVRegions.Count > 0)
                    {
                        if (!(request.TVRegions.Contains(TVRegion.Canada.ToString()) && request.TVRegions.Contains(TVRegion.LatinAmerica.ToString())))
                        {
                            if (!string.IsNullOrEmpty(FQuery.ToString()))
                            {
                                FQuery = FQuery.Append(" AND");
                            }
                            if (request.TVRegions.Contains(TVRegion.Canada.ToString()))
                            {
                                FQuery = FQuery.AppendFormat(" -(iq_dma_num:{0})", (int)TVRegion.Canada);
                            }
                            else if (request.TVRegions.Contains(TVRegion.LatinAmerica.ToString()))
                            {
                                FQuery = FQuery.AppendFormat(" -(iq_dma_num:{0})", (int)TVRegion.LatinAmerica);
                            }
                            else
                            {
                                FQuery = FQuery.Append(" -(iq_dma_num:{0}) AND -(iq_dma_num:{1})", (int)TVRegion.Canada, (int)TVRegion.LatinAmerica);
                            }
                        }
                    }*/

                    // if station(s) are passed , then lets make search Stations
                    if (request.Stations != null && request.Stations.Count() > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.Append("(");
                        }
                        else
                        {
                            FQuery = FQuery.Append(" AND (");
                        }

                        bool IsFirstStation = true;

                        foreach (string _Station in request.Stations)
                        {
                            if (IsFirstStation)
                            {
                                IsFirstStation = false;
                                FQuery = FQuery.AppendFormat(" stationid:{0}", _Station);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR stationid:{0}", _Station);
                            }
                        }

                        FQuery = FQuery.Append(" )");
                    }

                    if (request.IncludeRegionsNum != null && request.IncludeRegionsNum.Count() > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.Append("(");
                        }
                        else
                        {
                            FQuery = FQuery.Append(" AND (");
                        }

                        bool IsFirstRegion = true;

                        foreach (int _Region in request.IncludeRegionsNum)
                        {
                            if (IsFirstRegion)
                            {
                                IsFirstRegion = false;
                                FQuery = FQuery.AppendFormat(" region_num:{0}", _Region);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR region_num:{0}", _Region);
                            }
                        }

                        FQuery = FQuery.Append(" )");
                    }

                    if (request.CountryNums != null && request.CountryNums.Count() > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.Append("(");
                        }
                        else
                        {
                            FQuery = FQuery.Append(" AND (");
                        }

                        bool IsFirstCountry = true;

                        foreach (int _Country in request.CountryNums)
                        {
                            if (IsFirstCountry)
                            {
                                IsFirstCountry = false;
                                FQuery = FQuery.AppendFormat(" country_num:{0}", _Country);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR country_num:{0}", _Country);
                            }
                        }

                        FQuery = FQuery.Append(" )");
                    }

                    // our q query is ready!!! 
                    // lets add it to keyvalue pair for q,
                    // which untimately passed to solr.
                    vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                    // as fq is not compelsary search 
                    // we will pass it only if there is search criteria on fq 
                    if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                    {
                        vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                    }

                    // to make search effective and to reduce search time ,
                    // we should only retrun the fields which are required
                    // so lets add fields that we need to return , this is config driven,
                    // to add any field , we just need to change in config :)
                    vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["SolrFL"]));

                    // at last , we are ready to make request on solr.
                    // so lets pass solr search url and all required params 
                    // which will turn to request to solr and return solr response


                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod);
                }
                catch (Exception _Exception)
                {
                    isError = true;
                    res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";

                }

                return res;
            }
            catch (ThreadAbortException ex)
            {
                res.ResponseXml = "<response status=\"0\">" + ex.Message + "</response>";
                return res;
            }
            catch (Exception _Exception)
            {
                //CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        public TVFullResult SearchFullTVByIQCCKEY(string iqcckey, bool IsPmgLogging = false, string PmgLogFileLocation = "")
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            TVFullResult res = new TVFullResult();

            try
            {


                CommonFunction.LogInfo("PMG Call Start", IsPmgLogging, PmgLogFileLocation);
                CommonFunction.LogInfo("Create Request", IsPmgLogging, PmgLogFileLocation);

                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q query parameter of solr and 
                // 'FQuery' we will pass in the fq query parameter of solr 
                string Query = "*:*";
                string FQuery = "iq_cc_key:" + iqcckey;

                // our fq and q queries are ready!!! 
                // lets add them to keyvalue pairs,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query));
                vars.Add(new KeyValuePair<string, string>("fq", FQuery));

                vars.Add(new KeyValuePair<string, string>("wt", "xml"));


                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                res.OriginalRequest = new TVFullRequest();
                res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, IsPmgLogging, PmgLogFileLocation);

                CommonFunction.LogInfo("Load Response", IsPmgLogging, PmgLogFileLocation);
                CommonFunction.LogInfo("Solr Response - TimeTaken - for get response" + string.Format("with thread : Minutes :{0}\n Seconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), IsPmgLogging, PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                XmlDocument xDoc = new XmlDocument();

                // lets load solr response to xml so we can get data in xmk format
                xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                CommonFunction.LogInfo("Parse Response", IsPmgLogging, PmgLogFileLocation);

                // lets parse response. to our 'SearchResult' object
                parseTVFullResponse(xDoc, res, "", "");

                sw.Stop();

                CommonFunction.LogInfo("Solr Response - TimeTaken - for parse response" + string.Format("with thread : Minutes :{0}\n Seconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), IsPmgLogging, PmgLogFileLocation);
                CommonFunction.LogInfo("PMG Call End", IsPmgLogging, PmgLogFileLocation);

                return res;
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message, IsPmgLogging, PmgLogFileLocation);
                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        public TVFullResult SearchFullTV(TVFullRequest request, Int32? timeOutPeriod = null, Boolean isSentimentCall = false, string CustomSolrFl = "", bool getFacets = false)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            TVFullResult res = new TVFullResult();

            try
            {

           //     if (request.IndustryIDs != null && request.IndustryIDs.Count>0) { request.IndustryIDs.Add("72"); }
                CommonFunction.LogInfo("PMG Call Start", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Create Request", request.IsPmgLogging, request.PmgLogFileLocation);


                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q qeury parameter of solr and 
                // 'FQuery' we will pass in the fq qeury parameter of solr 
                StringBuilder Query = new StringBuilder();
                StringBuilder FQuery = new StringBuilder();

                /* Commented below condition as Now we are passing pagenumber based on 0 */
                /*if (request.PageNumber <= 0)
                {
                    request.PageNumber = 1;
                }*/

                string Fl = string.Empty;
                string FlTitle120 = string.Empty;
                string FlTitleOnSearchTerm = string.Empty;

                if (!string.IsNullOrWhiteSpace(request.Terms))
                {
                    string RequestTerm = string.Empty;

                    RequestTerm = request.Terms.Trim();

                    // if our search term starts with char '#' then
                    // we understand that user wants exact search without sysnonym.
                    // e.g. 'Find' , will only find terms with 'Find' and skip terms like 'Finding', 'Found' , ect...
                    // we added that if search term is fuzzy , then we do make search in only CCgen and not CC

                    if (RequestTerm.EndsWith("#"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            Fl = "cc_gen";
                            FlTitleOnSearchTerm = "title120_gen";
                        }
                        else
                        {
                            Fl = "cc";
                            FlTitleOnSearchTerm = "title120";
                        }
                    }
                    else
                    {
                        Fl = "cc_gen";
                        FlTitleOnSearchTerm = "title120_gen";
                    }


                    /*if (RequestTerm.EndsWith("#") || Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        Fl = "CCgen";
                    }
                    else
                    {
                        Fl = "CC";
                    }*/


                    // now if term is enclosed in double quote and it is phrase , then we must have to put slop ~2
                    // e.g. user search term "Hello World"
                    // and our CC may like "Hello 190s: World"
                    // although it is practically contious text but logically it has a word in between '190s:' 
                    // which will not allow to come in search results ,
                    // by making slop ~2 we will allow to return all search terms 
                    // which have gap of max. 2 words between them.
                    // Note : solr consider 190s: as 2 words , one is '190' (numeric) and seond is 's:' (alphabetic)

                    // New Note : we'll change the search term to lower case , but doing this
                    // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                    // we'll apply ~2 only on CC and not in CCgen. as CCgen is exact search
                    List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                    RequestTerm = Regex.Replace(
                                RequestTerm,
                                @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                        //@"([\""][\w ]+[\""])|(\w+)",
                                m => reservedWords.Contains(m.Value) ? m.Value : m.Value
                                );

                    if (request.IsTitleNContentSearch)
                    {
                        Query = Query.AppendFormat("({0}:({1}) OR {2}:({1}))", Fl, RequestTerm, FlTitleOnSearchTerm);
                    }
                    else
                    {
                        Query = Query.AppendFormat("{0}:({1})", Fl, RequestTerm);
                    }

                    // add term vector params
                    //vars.Add(new KeyValuePair<string, string>("tv", "true"));
                    //vars.Add(new KeyValuePair<string, string>("tv.fl", "CCgen"));
                    //vars.Add(new KeyValuePair<string, string>("tv.tf", "true"));
                    //vars.Add(new KeyValuePair<string, string>("tv.positions", "true"));
                }
                /*else
                {
                    // as we do not search on CC 
                    // we should turn of highligting fearure 
                    vars.Add(new KeyValuePair<string, string>("hl", "off"));
                    //vars.Add(new KeyValuePair<string, string>("tv", "false"));
                }*/

                // if appering is passed , then lets search on appearing
                if (!string.IsNullOrEmpty(request.Appearing))
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" (appearing:({0}) OR desc100:({0}))", request.Appearing);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND (appearing:({0}) OR desc100:({0}))", request.Appearing);
                    }
                }

                // if description is passed , then lets search on description
                if (!string.IsNullOrEmpty(request.Desc100))
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" desc100:({0})", request.Desc100);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND desc100:({0})", request.Desc100);
                    }
                }

                // if title is passed , then lets search on title
                if (!string.IsNullOrEmpty(request.Title120))
                {

                    string RequestTerm = string.Empty;
                    RequestTerm = request.Title120.Trim();

                    // if our search term starts with char '#' then
                    // we understand that user wants exact search without sysnonym.
                    // e.g. 'Find' , will only find terms with 'Find' and skip terms like 'Finding', 'Found' , ect...
                    // we added that if search term is fuzzy , then we do make search in only CCgen and not CC

                    if (RequestTerm.EndsWith("#"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            FlTitle120 = "title120_gen";
                        }
                        else
                        {
                            FlTitle120 = "title120";
                        }
                    }
                    else
                    {
                        FlTitle120 = "title120_gen";
                    }

                    // New Note : we'll change the search term to lower case , but doing this
                    // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                    // we'll apply ~2 only on CC and not in CCgen. as CCgen is exact search
                    List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                    RequestTerm = Regex.Replace(
                                RequestTerm,
                                @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                        //@"([\""][\w ]+[\""])|(\w+)",
                                m => reservedWords.Contains(m.Value) ? m.Value : m.Value
                                );

                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" {0}:({1})", FlTitle120, RequestTerm);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND {0}:({1})", FlTitle120, RequestTerm);
                    }
                }


                if (!string.IsNullOrEmpty(request.Title120) || !string.IsNullOrEmpty(request.Terms))
                {



                    // if user has searched with CC , then only we need to pass below params to solr.
                    // as we need to give highlight on CC ,  for user searched term.
                    // all these feilds are for highlighting functionality
                    // hl.fl =  name of the feild on which need to provide highlighting
                    // hl = value can be on/off , if on then highlighting feature is enabled otherwise disabled.
                    // hl.maxAnalyzedChars =  default max char length for highlight is 51200 , but we need unlimited
                    vars.Add(new KeyValuePair<string, string>("hl.fl", string.IsNullOrEmpty(request.Terms) ? FlTitle120 : Fl + (string.IsNullOrEmpty(request.Title120) ? string.Empty : "," + FlTitle120)));
                    vars.Add(new KeyValuePair<string, string>("hl.requireFieldMatch", "true"));
                    vars.Add(new KeyValuePair<string, string>("hl", "on"));
                    if (string.IsNullOrEmpty(request.Title120) && !request.IsTitleNContentSearch)
                    {
                        vars.Add(new KeyValuePair<string, string>("hl.maxAnalyzedChars", "2147483647"));
                    }
                    else
                    {
                        vars.Add(new KeyValuePair<string, string>("f." + Fl + ".hl.maxAnalyzedChars", "2147483647"));
                        //vars.Add(new KeyValuePair<string, string>("f." + ((string.IsNullOrWhiteSpace(FlTitle120))?FlTitleOnSearchTerm:FlTitle120) + ".hl.maxAnalyzedChars", "500"));

                        vars.Add(new KeyValuePair<string, string>("f." + (string.IsNullOrWhiteSpace(FlTitle120) ? FlTitleOnSearchTerm : FlTitle120) + ".hl.maxAnalyzedChars", "500"));
                    }

                    // as our CC text is very long , we will get exact closed-caption 
                    // only at time of showing it while we play video
                    // in all other cases we just need to display no. of hits and not the cc text.
                    // so we'll process it only at time of showing it 
                    // and in other cases we'll get count for hits from solr highlights
                    if (request.IsShowCC)
                    {
                        // hl.fragsize = char size for fragment for highlight , 
                        // by setting it to 0 ,it will not fragment and return whole CC in sigle highlight. 
                        vars.Add(new KeyValuePair<string, string>("hl.fragsize", "0"));

                    }
                    else
                    {
                        // by setting it to 145 ,it will return no. of highlights 
                        // fragment size for signle highlight is 145 (approx)
                        if (request.FragSize.HasValue)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", request.FragSize.Value.ToString()));
                        else if (!isSentimentCall)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSize"]));
                        else
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSizeForSentiment"]));

                        vars.Add(new KeyValuePair<string, string>("hl.snippets", "99"));
                    }
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("hl", "off"));
                }

                //  if guid(s) are passed , then lets make search Guid List , only that guid data will return
                if (!string.IsNullOrEmpty(request.GuidList))
                {
                    string[] _RLGUIDs = request.GuidList.Split(',');

                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" (");
                    }
                    else
                    {
                        Query = Query.Append(" AND (");
                    }

                    bool IsFirst = true;

                    foreach (string _RLGUID in _RLGUIDs)
                    {
                        if (IsFirst)
                        {
                            Query = Query.AppendFormat(" guid:{0}", _RLGUID);
                            IsFirst = false;
                        }
                        else
                        {
                            Query = Query.AppendFormat(" OR guid:{0}", _RLGUID);
                        }
                    }

                    Query = Query.Append(" )");
                }

                //  if IQCCKey(s) are passed , then lets make search IQCCKey List , only that IQCCKey data will return
                if (!string.IsNullOrEmpty(request.IQCCKeyList))
                {
                    string[] _IQCCKeys = request.IQCCKeyList.Split(',');

                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" (");
                    }
                    else
                    {
                        Query = Query.Append(" AND (");
                    }

                    bool IsFirstIQCCKey = true;

                    foreach (string _IQCCKey in _IQCCKeys)
                    {
                        if (IsFirstIQCCKey)
                        {
                            IsFirstIQCCKey = false;
                            Query = Query.AppendFormat(" iq_cc_key:{0}", _IQCCKey);
                        }
                        else
                        {
                            Query = Query.AppendFormat(" OR iq_cc_key:{0}", _IQCCKey);
                        }
                    }

                    Query = Query.Append(" )");
                }

                // ooops nothing passed for 'q' search of solr.
                // then as q is complesary search
                // if nothing is passed then we should pass *:*
                // which mean it return all without making search for q.
                // although after q , it will filter on fq (filter query) search. 
                if (string.IsNullOrEmpty(Query.ToString()))
                {
                    Query = Query.Append("*:*");
                    //Query = Query.Append("isdeleted:false");
                }
                else
                {
                    Query = Query.Append(" AND isdeleted:false");
                }

                string SortFields = string.Empty;

                // lets make solr fields and pass to solr
                if (!string.IsNullOrEmpty(request.SortFields))
                {
                    SortFields = GenerateSortField(request.SortFields);
                }

                if (request.SortFields != null)
                {
                    vars.Add(new KeyValuePair<string, string>("sort", SortFields));
                }

                // if both start and enddate is passed then lets make search on RL_Station_DateTime.
                if (request.StartDate != null && request.EndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" gmtdatetime_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND gmtdatetime_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                // if both start and enddate is passed then lets make search on RL_Station_DateTime.
                if (request.RLStationStartDate != null && request.RLStationEndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" datetime_dt:[");
                        FQuery = FQuery.Append(request.RLStationStartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.RLStationEndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND datetime_dt:[");
                        FQuery = FQuery.Append(request.RLStationStartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.RLStationEndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                // if both start Clip Date and end Clip Date is passed then lets make search on ClipDate.
                if (request.ClipStartDate != null && request.ClipEndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" date:[");
                        FQuery = FQuery.Append(request.ClipStartDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append(" TO ");
                        FQuery = FQuery.Append(request.ClipEndDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND date:[");
                        FQuery = FQuery.Append(request.ClipStartDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append(" TO ");
                        FQuery = FQuery.Append(request.ClipEndDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("]");
                    }
                }

                // if station(s) are passed , then lets make search Stations
                if (request.Stations != null && request.Stations.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append("(");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstStation = true;

                    foreach (string _Station in request.Stations)
                    {
                        if (IsFirstStation)
                        {
                            IsFirstStation = false;
                            FQuery = FQuery.AppendFormat(" stationid:{0}", _Station);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR stationid:{0}", _Station);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if hours are passed , then lets make search for hours
                if (request.Hours != null && request.Hours.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstHour = true;

                    foreach (string _Hour in request.Hours)
                    {
                        if (IsFirstHour)
                        {
                            IsFirstHour = false;
                            FQuery = FQuery.AppendFormat(" hour:{0}", _Hour);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR hour:{0}", _Hour);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }


                // if IQClassNum(s) are passed , then lets make search for IQClassNum(s)
                if (request.IQClassNum != null && request.IQClassNum.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQClass = true;

                    foreach (string _IQ_Class_Num in request.IQClassNum)
                    {
                        if (IsFirstIQClass)
                        {
                            IsFirstIQClass = false;
                            FQuery = FQuery.AppendFormat(" iq_class_num:{0}", _IQ_Class_Num);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR iq_class_num:{0}", _IQ_Class_Num);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }


                // if IQDmaName(s) are passed , then lets make search for IQDmaName(s)
                if (request.IQDmaName != null && request.IQDmaName.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (string _IQ_Dma_Name in request.IQDmaName)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" market:\"{0}\"", _IQ_Dma_Name);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR market:\"{0}\"", _IQ_Dma_Name);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if IQDmaNum(s) are passed , then lets make search for IQDmaNum(s)
                if (request.IQDmaNum != null && request.IQDmaNum.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (string _IQ_Dma_Num in request.IQDmaNum)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" iq_dma_num:{0}", _IQ_Dma_Num);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR iq_dma_num:{0}", _IQ_Dma_Num);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if StationAffil(s) are passed , then lets make search for StationAffil(s)
                if (request.StationAffil != null && request.StationAffil.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstStationAffilName = true;

                    foreach (string _Station_Affil in request.StationAffil)
                    {
                        if (IsFirstStationAffilName)
                        {
                            IsFirstStationAffilName = false;
                            FQuery = FQuery.AppendFormat(" affiliate:\"{0}\"", _Station_Affil);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR affiliate:\"{0}\"", _Station_Affil);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }


                // if StationAffilNum(s) are passed , then lets make search for StationAffilNum(s)
                if (request.StationAffilNum != null && request.StationAffilNum.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstStationAffilName = true;

                    foreach (string _Station_Affil_Num in request.StationAffilNum)
                    {
                        if (IsFirstStationAffilName)
                        {
                            IsFirstStationAffilName = false;
                            FQuery = FQuery.AppendFormat(" station_affil_num:{0}", _Station_Affil_Num);

                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR station_affil_num:{0}", _Station_Affil_Num);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if TimeZone is passed , then lets make search for TimeZone
                if (!string.IsNullOrEmpty(request.TimeZone) && request.TimeZone.ToLower(System.Globalization.CultureInfo.CurrentCulture) != "all")
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" timezone:");
                        FQuery = FQuery.Append(request.TimeZone);
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND timezone:");
                        FQuery = FQuery.Append(request.TimeZone);
                    }
                }

                // if TVREgion is passed , then lets make search for TimeZone
                /*if (request.TVRegions != null && request.TVRegions.Count > 0)
                {
                    if (!(request.TVRegions.Contains(TVRegion.Canada.ToString()) && request.TVRegions.Contains(TVRegion.LatinAmerica.ToString())))
                    {
                        if (!string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.Append(" AND");
                        }
                        if (request.TVRegions.Contains(TVRegion.Canada.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" -(iq_dma_num:{0})", (int)TVRegion.Canada);
                        }
                        else if (request.TVRegions.Contains(TVRegion.LatinAmerica.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" -(iq_dma_num:{0})", (int)TVRegion.LatinAmerica);
                        }
                        else
                        {
                            FQuery = FQuery.Append(" -(iq_dma_num:{0}) AND -(iq_dma_num:{1})", (int)TVRegion.Canada, (int)TVRegion.LatinAmerica);
                        }
                    }
                }*/

                if (request.IncludeRegionsNum != null && request.IncludeRegionsNum.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append("(");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstRegion = true;

                    foreach (int _Region in request.IncludeRegionsNum)
                    {
                        if (IsFirstRegion)
                        {
                            IsFirstRegion = false;
                            FQuery = FQuery.AppendFormat(" region_num:{0}", _Region);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR region_num:{0}", _Region);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                if (request.CountryNums != null && request.CountryNums.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append("(");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstCountry = true;

                    foreach (int _Country in request.CountryNums)
                    {
                        if (IsFirstCountry)
                        {
                            IsFirstCountry = false;
                            FQuery = FQuery.AppendFormat(" country_num:{0}", _Country);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR country_num:{0}", _Country);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if Logo(s) are passed , then lets make search for Brand(s)
                if (request.SearchLogoIDs != null && request.SearchLogoIDs.Count() > 0)
                {
                    bool IsFirst = true;

                    StringBuilder earned = new StringBuilder();
                    StringBuilder paid = new StringBuilder();

                    foreach (string SearchLogoID in request.SearchLogoIDs)
                    {
                        if (IsFirst)
                        {
                            IsFirst = false;
                        }
                        else
                        {
                            FQuery = FQuery.Append(" OR");
                        }

                        switch(request.EarnedPaid.ToLower())
                        {
                            case "earned":
                                earned = earned.AppendFormat(" logosearned:{0}", SearchLogoID);
                                break;
                            case "paid":
                                paid = paid.AppendFormat(" logospaid:{0}", SearchLogoID);
                                break;
                            default:
                                paid = paid.AppendFormat(" logospaid:{0}", SearchLogoID);
                                earned = earned.AppendFormat(" logosearned:{0}", SearchLogoID);
                                break;
                        }
                    }

                    if (earned.Length > 0 && paid.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" (({0}) OR ({1}))", earned, paid);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND (({0}) OR ({1}))", earned, paid);
                        }
                    }
                    else if (earned.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" ({0})", earned);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND ({0})", earned);
                        }
                    }
                    else if (paid.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" ({0})", paid);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND ({0})", paid);
                        }
                    }
                }

                // if Brand(s) are passed , then lets make search for Brand(s)
                if (request.BrandIDs != null && request.BrandIDs.Count() > 0)
                {
                    bool IsFirst = true;

                    StringBuilder earned = new StringBuilder();
                    StringBuilder paid = new StringBuilder();

                    foreach (string BrandID in request.BrandIDs)
                    {
                        if (IsFirst)
                        {
                            IsFirst = false;
                        }
                        else
                        {
                            FQuery = FQuery.Append(" OR");
                        }

                        switch (request.EarnedPaid.ToLower())
                        {
                            case "earned":
                                earned = earned.AppendFormat(" brandsearned:{0}", BrandID);
                                break;
                            case "paid":
                                paid = paid.AppendFormat(" brandspaid:{0}", BrandID);
                                break;
                            default:
                                paid = paid.AppendFormat(" brandspaid:{0}", BrandID);
                                earned = earned.AppendFormat(" brandsearned:{0}", BrandID);
                                break;
                        }
                    }

                    if (earned.Length > 0 && paid.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" (({0}) OR ({1}))", earned, paid);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND (({0}) OR ({1}))", earned, paid);
                        }
                    }
                    else if (earned.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" ({0})", earned);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND ({0})", earned);
                        }
                    }
                    else if (paid.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" ({0})", paid);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND ({0})", paid);
                        }
                    }
                }

                // if Industry(s) are passed , then lets make search for Brand(s)
                if (request.IndustryIDs != null && request.IndustryIDs.Count() > 0)
                {

                    StringBuilder earned = new StringBuilder();
                    StringBuilder paid = new StringBuilder();

                    foreach (string industry in request.IndustryIDs)
                    {
                        switch (request.EarnedPaid.ToLower())
                        {
                            case "earned":
                                earned = earned.AppendFormat(" industriesearned:{0}", industry);
                                break;
                            case "paid":
                                paid = paid.AppendFormat(" industriespaid:{0}", industry);
                                break;
                            default:
                                paid = paid.AppendFormat(" industriespaid:{0}", industry);
                                earned = earned.AppendFormat(" industriesearned:{0}", industry);
                                break;
                        }
                    }

                    if (earned.Length > 0 && paid.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" (({0}) OR ({1}))", earned, paid);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND (({0}) OR ({1}))", earned, paid);
                        }
                    }
                    else if (earned.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" ({0})", earned);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND ({0})", earned);
                        }
                    }
                    else if (paid.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" ({0})", paid);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND ({0})", paid);
                        }
                    }
                }

                // if Company(s) are passed , then lets make search for Brand(s)
                if (request.CompanyIDs != null && request.CompanyIDs.Count() > 0)
                {
                    bool IsFirst = true;

                    StringBuilder earned = new StringBuilder();
                    StringBuilder paid = new StringBuilder();

                    foreach (string CompanyID in request.CompanyIDs)
                    {
                        if (IsFirst)
                        {
                            IsFirst = false;
                        }
                        else
                        {
                            FQuery = FQuery.Append(" OR");
                        }

                        switch (request.EarnedPaid.ToLower())
                        {
                            case "earned":
                                earned = earned.AppendFormat(" companiesearned:{0}", CompanyID);
                                break;
                            case "paid":
                                paid = paid.AppendFormat(" companiespaid:{0}", CompanyID);
                                break;
                            default:
                                paid = paid.AppendFormat(" companiespaid:{0}", CompanyID);
                                earned = earned.AppendFormat(" companiesearned:{0}", CompanyID);
                                break;
                        }
                    }

                    if (earned.Length > 0 && paid.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" (({0}) OR ({1}))", earned, paid);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND (({0}) OR ({1}))", earned, paid);
                        }
                    }
                    else if (earned.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" ({0})", earned);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND ({0})", earned);
                        }
                    }
                    else if (paid.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" ({0})", paid);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND ({0})", paid);
                        }
                    }
                }

                if (!((request.CompanyIDs != null && request.CompanyIDs.Count() > 0) || (request.IndustryIDs != null && request.IndustryIDs.Count() > 0) || (request.BrandIDs != null && request.BrandIDs.Count() > 0) || (request.SearchLogoIDs != null && request.SearchLogoIDs.Count() > 0)))
                {
                    StringBuilder earnedPaid = new StringBuilder();
                    
                    switch (request.EarnedPaid.ToLower())
                    {
                        case "earned":
                            earnedPaid = earnedPaid.AppendFormat(" logosearned:*");
                            break;
                        case "paid":
                            earnedPaid = earnedPaid.AppendFormat(" logospaid:*");
                            break;
                        default:
                            break;
                    }

                    if (earnedPaid.Length > 0)
                    {
                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.AppendFormat(" ({0})", earnedPaid);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" AND ({0})", earnedPaid);
                        }
                    }
                }

                if (request.PEStatus == true)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append("pestatus:1");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND pestatus:1");
                    }
                }
                if (request.LogoStatus == true)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append("logostatus:1");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND logostatus:1");
                    }
                }
                if (request.AdStatus == true)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append("adstatus:1");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND adstatus:1");
                    }
                }

                // our q query is ready!!! 
                // lets add it to keyvalue pair for q,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                // as fq is not compelsary search 
                // we will pass it only if there is search criteria on fq 
                if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                {
                    vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                }

                // to make search effective and to reduce search time ,
                // we should only retrun the fields which are required
                // so lets add fields that we need to return , this is config driven,
                // to add any field , we just need to change in config :)
                if (!isSentimentCall)
                {
                    string fl = string.Empty;
                    if (string.IsNullOrEmpty(CustomSolrFl))
                    {
                        fl = System.Configuration.ConfigurationManager.AppSettings["SolrFL"];
                        if (!string.IsNullOrEmpty(fl)) fl += ",";
                        fl += "pestatus,adstatus,logostatus,logos,ads,brandspaid,brandsearned,industriespaid,industriesearned,companiespaid,companiesearned,logospaid,logosearned";
                        if (string.IsNullOrEmpty(request.Terms) && request.IsShowCC == true)
                            fl = fl + ",cc";
                    }
                    else
                    {
                        fl = CustomSolrFl;
                    }
                    vars.Add(new KeyValuePair<string, string>("fl", fl));
                }
                else
                    vars.Add(new KeyValuePair<string, string>("fl", "null"));

                //Use existent variables to get Get All the facets
                if (getFacets)
                {
                    Dictionary<string, Dictionary<string, string>> facets = GetTVFullFacets(new List<KeyValuePair<string, string>>(vars), request, sw);
                    if (facets != null && facets.Any())
                    {
                        res.Facets = facets;
                    }
                }

                // start and rows return the required page. No. data.
                if (request.Start != null)
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.Start), System.Globalization.CultureInfo.CurrentCulture)));
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                }
                vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                //vars.Add(new KeyValuePair<string, string>("wt", "json"));


                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                string RequestURL = string.Empty;
                if (request.IsOutRequest)
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                    res.RequestUrl = RequestURL;
                }
                else
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                }

                //CommonFunction.LogInfo("Response Xml :\n " + res.ResponseXml, request.IsPmgLogging, request.PmgLogFileLocation);


                CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Solr Response - TimeTaken - for get response" + string.Format("with thread : Minutes :{0}\n Seconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                XmlDocument xDoc = new XmlDocument();

                // lets load solr response to xml so we can get data in xmk format
                xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                res.OriginalRequest = request;

                CommonFunction.LogInfo("Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);

                // lets parse response. to our 'SearchResult' object
                if (!isSentimentCall)
                    parseTVFullResponse(xDoc, res, Fl, FlTitle120);
                else
                    parseTVFullSentiment(xDoc, res, Fl);

                if (request.IsSentiment && !string.IsNullOrWhiteSpace(request.Terms) && request.LowThreshold != null && request.HighThreshold != null)
                {
                    makeTVFullSentimentRequest(res, timeOutPeriod);
                }

                sw.Stop();

                CommonFunction.LogInfo("Solr Response - TimeTaken - for parse response" + string.Format("with thread : Minutes :{0}\n Seconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo(string.Format("Total Hti Count :{0}", res.TotalHitCount), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("PMG Call End", request.IsPmgLogging, request.PmgLogFileLocation);

                return res;
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        private Dictionary<string, Dictionary<string, string>> GetTVFullFacets(List<KeyValuePair<string, string>> vars, TVFullRequest request, Stopwatch sw)
        {
            var lstTasks = new List<Task>();
            var dictResults = new Dictionary<string, Dictionary<string, string>>();
            var enc = new UTF8Encoding();

            // Perform facet queries
            vars.Add(new KeyValuePair<string, string>("facet", "on"));
            vars.Add(new KeyValuePair<string, string>("facet.limit", "-1"));
            vars.Add(new KeyValuePair<string, string>("facet.mincount", "1"));
            vars.Add(new KeyValuePair<string, string>("rows", "0"));

            // Simultaneously run each facet as it's own query to improve performance
            List<KeyValuePair<string, string>> brandVars = new List<KeyValuePair<string, string>>(vars);
            brandVars.Add(new KeyValuePair<string, string>("facet.field", "brandspaid"));
            brandVars.Add(new KeyValuePair<string, string>("facet.field", "brandsearned"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(brandVars, "Brand Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "BrandFacet", TaskCreationOptions.AttachedToParent));

            List<KeyValuePair<string, string>> industryVars = new List<KeyValuePair<string, string>>(vars);
            industryVars.Add(new KeyValuePair<string, string>("facet.field", "industriespaid"));
            industryVars.Add(new KeyValuePair<string, string>("facet.field", "industriesearned"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(industryVars, "Industry Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "IndustryFacet", TaskCreationOptions.AttachedToParent));

            List<KeyValuePair<string, string>> logoVars = new List<KeyValuePair<string, string>>(vars);
            logoVars.Add(new KeyValuePair<string, string>("facet.field", "logospaid"));
            logoVars.Add(new KeyValuePair<string, string>("facet.field", "logosearned"));
            logoVars.Add(new KeyValuePair<string,string>("facet.field","logos"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(logoVars, "Logo Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "LogoFacet", TaskCreationOptions.AttachedToParent));

            //Narrow Results Filters
            List<KeyValuePair<string, string>> dmaVars = new List<KeyValuePair<string, string>>(vars);
            dmaVars.Add(new KeyValuePair<string, string>("facet.field", "market"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(dmaVars, "DMA Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "DmaFacet", TaskCreationOptions.AttachedToParent));

            List<KeyValuePair<string, string>> affiliateVars = new List<KeyValuePair<string, string>>(vars);
            affiliateVars.Add(new KeyValuePair<string, string>("facet.field", "affiliate"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(affiliateVars, "Affiliate Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "AffiliateFacet", TaskCreationOptions.AttachedToParent));

            List<KeyValuePair<string, string>> stationVars = new List<KeyValuePair<string, string>>(vars);
            stationVars.Add(new KeyValuePair<string, string>("facet.field", "stationid"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(stationVars, "Station Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "StationFacet", TaskCreationOptions.AttachedToParent));

            List<KeyValuePair<string, string>> countryVars = new List<KeyValuePair<string, string>>(vars);
            countryVars.Add(new KeyValuePair<string, string>("facet.field", "country_num"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(countryVars, "Country Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "CountryFacet", TaskCreationOptions.AttachedToParent));

            List<KeyValuePair<string, string>> regionVars = new List<KeyValuePair<string, string>>(vars);
            regionVars.Add(new KeyValuePair<string, string>("facet.field", "region_num"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(regionVars, "Region Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "RegionFacet", TaskCreationOptions.AttachedToParent));

            List<KeyValuePair<string, string>> classVars = new List<KeyValuePair<string, string>>(vars);
            classVars.Add(new KeyValuePair<string, string>("facet.field", "iq_class"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(classVars, "Class Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "ClassFacet", TaskCreationOptions.AttachedToParent));

            List<KeyValuePair<string, string>> logosPaid = new List<KeyValuePair<string, string>>(vars);
            logosPaid.Add(new KeyValuePair<string, string>("facet.query", "logospaid:[* TO *]"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(logosPaid, "Logo Paid Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "logosPaid", TaskCreationOptions.AttachedToParent));

            List<KeyValuePair<string, string>> logosEarned = new List<KeyValuePair<string, string>>(vars);
            logosEarned.Add(new KeyValuePair<string, string>("facet.query", "logosearned:[* TO *]"));
            lstTasks.Add(Task.Factory.StartNew((object obj) => ExecuteFacetSearch(logosEarned, "Logo Earned Facet Call", request.IsPmgLogging, request.PmgLogFileLocation), "logosEarned", TaskCreationOptions.AttachedToParent));

            try
            {
                Task.WaitAll(lstTasks.ToArray(), 90000);
            }
            catch (AggregateException _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message + " :: " + _Exception.StackTrace, request.IsPmgLogging, request.PmgLogFileLocation);
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message + " :: " + _Exception.StackTrace, request.IsPmgLogging, request.PmgLogFileLocation);
            }

            var dictResponses = new Dictionary<string, SearchTVFullResult>();
            foreach (var tsk in lstTasks)
            {
                SearchTVFullResult taskRes = ((Task<SearchTVFullResult>)tsk).Result;
                string taskType = (string)tsk.AsyncState;

                dictResponses.Add(taskType, taskRes);
            }

            CommonFunction.LogInfo("Solr Response - TimeTaken - for get response" + string.Format("with thread : Minutes :{0}  Seconds :{1}  Milliseconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

            // Create facet result objects
            foreach (KeyValuePair<string, SearchTVFullResult> kvResponse in dictResponses)
            {
                SearchTVFullResult resFacet = kvResponse.Value;
                XmlDocument xDocFacet = new XmlDocument();
                xDocFacet.Load(new MemoryStream(enc.GetBytes(resFacet.ResponseXml)));

                // lets get list of all the hits we get
                if (kvResponse.Key == "logosPaid" || kvResponse.Key == "logosEarned")
                {
                    XmlNodeList facetNodes = xDocFacet.SelectNodes("/response/lst[@name='facet_counts']/lst[@name='facet_queries']/int");
                
                    foreach (XmlNode node in facetNodes)
                    {
                        var results = new Dictionary<string, string>();
                        results.Add("Counts", node.InnerXml);
                        dictResults.Add(node.Attributes["name"].Value, results);
                    }
                }
                else
                {
                    XmlNodeList facetNodes = xDocFacet.SelectNodes("/response/lst[@name='facet_counts']/lst[@name='facet_fields']/lst");
                
                // now we will parse each hit one by one.
                    foreach (XmlNode node in facetNodes)
                    {
                        var results = new Dictionary<string,string>();
                        foreach (XmlNode cn in node.ChildNodes)
                        {
                            results.Add(cn.Attributes["name"].Value, cn.InnerText);
                       
                        }
                        dictResults.Add(node.Attributes["name"].Value, results);
                    }
                 }
            }

            sw.Stop();

            CommonFunction.LogInfo("Solr Response - TimeTaken - for parse response" + string.Format("with thread : Minutes :{0}  Seconds :{1}  Milliseconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);


            return dictResults;
        }
        private SearchTVFullResult ExecuteFacetSearch(List<KeyValuePair<string, string>> vars, string logMessage, bool isLogging, string logFileLocation)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                string requestUrl;
                string xml = RestClient.getXML(Url.AbsoluteUri, vars, isLogging, logFileLocation, out requestUrl);
                CommonFunction.LogInfo("\"" + logMessage + " (" + sw.ElapsedMilliseconds + "ms),\"" + requestUrl, isLogging, logFileLocation);
                sw.Stop();

                SearchTVFullResult res = new SearchTVFullResult();
                res.ResponseXml = xml;
                return res;
            }
            catch (Exception ex)
            {
                if (ex.Data.Contains("RequestUrl"))
                {
                    CommonFunction.LogInfo("\"" + logMessage + " - ERROR,\"Error occurred for request url " + ex.Data["RequestUrl"], isLogging, logFileLocation);
                }
                throw;
            }
        }

        private static string GenerateSortField(string sortfields)
        {
            try
            {
                IDictionary<string, string> PMGSearchSortFields = new Dictionary<string, string>();

                PMGSearchSortFields.Add("datetime", "gmtdatetime_dt asc,market asc");
                PMGSearchSortFields.Add("date", "gmtdatetime_dt asc,market asc");
                PMGSearchSortFields.Add("date-", "gmtdatetime_dt desc,market asc");
                PMGSearchSortFields.Add("datetime-", "gmtdatetime_dt desc,market asc");
                PMGSearchSortFields.Add("guid", "guid asc");
                PMGSearchSortFields.Add("guid-", "guid desc");
                PMGSearchSortFields.Add("station", "stationid asc");
                PMGSearchSortFields.Add("station-", "stationid desc");
                PMGSearchSortFields.Add("market", "market asc,gmtdatetime_dt desc");
                PMGSearchSortFields.Add("market-", "market desc,gmtdatetime_dt desc");
                PMGSearchSortFields.Add("clipdate", "date asc");
                PMGSearchSortFields.Add("clipdate-", "date desc");
                PMGSearchSortFields.Add("affiliate", "affiliate asc");
                PMGSearchSortFields.Add("affiliate-", "affiliate desc");
                PMGSearchSortFields.Add("dma_num", "iq_dma_num asc");
                PMGSearchSortFields.Add("dma_num-", "iq_dma_num desc");


                StringBuilder InputSortFields = new StringBuilder();

                string[] PMGSearchSortField = sortfields.Split(',');

                // max solr solr field is config driven , so we only can search on max that. no of fields
                int MaxNoOfSortFields = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxSortField"], System.Globalization.CultureInfo.CurrentCulture);
                int index = 0;

                foreach (string SortField in PMGSearchSortField)
                {
                    if (PMGSearchSortFields.ContainsKey(SortField.ToLower(System.Globalization.CultureInfo.CurrentCulture)))
                    {
                        InputSortFields.Append(PMGSearchSortFields[SortField.ToLower(System.Globalization.CultureInfo.CurrentCulture)] + ",");
                    }

                    index = index + 1;

                    if (index >= MaxNoOfSortFields)
                    {
                        break;
                    }
                }

                if (InputSortFields.Length > 0)
                {
                    InputSortFields.Remove(InputSortFields.Length - 1, 1);
                }

                return InputSortFields.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GenerateNewsSortField(string sortfields)
        {
            try
            {
                IDictionary<string, string> PMGSearchSortFields = new Dictionary<string, string>();


                PMGSearchSortFields.Add("date", "harvestdate_dt asc");
                PMGSearchSortFields.Add("date-", "harvestdate_dt desc");


                StringBuilder InputSortFields = new StringBuilder();

                string[] PMGSearchSortField = sortfields.Split(',');

                // max solr solr field is config driven , so we only can search on max that. no of fields
                int MaxNoOfSortFields = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxSortField"], System.Globalization.CultureInfo.CurrentCulture);
                int index = 0;

                foreach (string SortField in PMGSearchSortField)
                {
                    if (PMGSearchSortFields.ContainsKey(SortField.ToLower(System.Globalization.CultureInfo.CurrentCulture)))
                    {
                        InputSortFields.Append(PMGSearchSortFields[SortField.ToLower(System.Globalization.CultureInfo.CurrentCulture)] + ",");
                    }

                    index = index + 1;

                    if (index >= MaxNoOfSortFields)
                    {
                        break;
                    }
                }

                if (InputSortFields.Length > 0)
                {
                    InputSortFields.Remove(InputSortFields.Length - 1, 1);
                }

                return InputSortFields.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }


        private static void parseResponse(XmlDocument doc, SearchResult res, string highlightCCFeildName, string highlightTitleFeildName)
        {

            XmlNode root = doc.SelectSingleNode("/response/result");

            if (root != null) try { res.TotalHitCount = Convert.ToInt32(root.Attributes.GetNamedItem("numFound").Value, System.Globalization.CultureInfo.CurrentCulture); }
                catch (Exception) { }

            // lets get list of all the hits we get 
            XmlNodeList hitNodes = doc.SelectNodes("/response/result/doc");

            // lets get list for all the highlighting 
            XmlNodeList occurenceNodes = doc.SelectNodes("/response/lst[@name='highlighting']/lst");

            res.Hits = new List<Hit>();

            // now we will parse each hit one by one. 
            foreach (XmlNode hitNode in hitNodes)
            {
                List<string> ListOfTitle120 = new List<string>();
                Hit hit = parseHit(hitNode, res.OriginalRequest.Title120, res.OriginalRequest.IsTitle120List, out ListOfTitle120);
                if (!string.IsNullOrWhiteSpace(res.OriginalRequest.Terms))
                {

                    // now its turn to get highlight 
                    // as in our response all list of highlight in different list 
                    // we will find highlight for our this hit by its iq_cc_key , as iq_cc_key is unique field in solr
                    XmlNode OccurenceNode = (XmlNode)(from XmlNode OccNode in occurenceNodes
                                                      where OccNode.Attributes["name"].Value.ToLower(System.Globalization.CultureInfo.CurrentCulture) == hit.Iqcckey.ToLower(System.Globalization.CultureInfo.CurrentCulture)
                                                      select OccNode.SelectSingleNode("arr[@name='" + highlightCCFeildName + "']")).FirstOrDefault();

                    if (OccurenceNode != null)
                    {
                        if (res.OriginalRequest.IsShowCC == true)
                        {
                            int TotalOccurences = 0;
                            List<TermOccurrence> closedCaption = new List<TermOccurrence>();
                            hit.TermOccurrences = parseCC(OccurenceNode, string.Empty, out TotalOccurences, res.OriginalRequest.FragOffset, out closedCaption);
                            hit.ClosedCaption = closedCaption != null && closedCaption.Count > 0 ? closedCaption : hit.ClosedCaption;
                            hit.TotalNoOfOccurrence = TotalOccurences;
                        }
                        else
                        {
                            hit.TotalNoOfOccurrence = OccurenceNode.SelectNodes("str").Count;

                            string text = string.Empty;
                            foreach (XmlNode xmlNode3 in OccurenceNode.SelectNodes("str"))
                            {

                                if (!string.IsNullOrWhiteSpace(xmlNode3.InnerText.Trim()) && Regex.IsMatch(xmlNode3.InnerText.Trim(), "^(\\d+s:\\s*\\w*)"))
                                {
                                    text += xmlNode3.InnerText;
                                }
                                else
                                {
                                    text = text + " 9999s: " + xmlNode3.InnerText;
                                }
                            }
                            int totalNoOfOccurrence2 = 0;
                            List<TermOccurrence> closedCaption = new List<TermOccurrence>();
                            hit.TermOccurrences = parseCC(OccurenceNode, text, out totalNoOfOccurrence2, res.OriginalRequest.FragOffset, out closedCaption);
                            hit.TotalNoOfOccurrence = totalNoOfOccurrence2;
                        }
                    }

                }

                if (!res.OriginalRequest.IsTitle120List && !string.IsNullOrEmpty(res.OriginalRequest.Title120))
                {

                    XmlNode Title120Node = (XmlNode)(from XmlNode OccNode in occurenceNodes
                                                     where OccNode.Attributes["name"].Value.ToLower(System.Globalization.CultureInfo.CurrentCulture) == hit.Iqcckey.ToLower(System.Globalization.CultureInfo.CurrentCulture)
                                                     select OccNode.SelectSingleNode("arr[@name='" + highlightTitleFeildName + "']")).FirstOrDefault();

                    // we found title120 from highlight , so we will override it. 
                    if (Title120Node != null && Title120Node.SelectNodes("str").Count > 0)
                    {
                        hit.Title120 = Title120Node.SelectNodes("str").Item(0).InnerText.Replace("<span class=\"highlight\">", string.Empty).Replace("</span>", string.Empty);
                        if (string.IsNullOrEmpty(res.OriginalRequest.Terms) && ListOfTitle120.Count > 0)
                        {
                            int index = ListOfTitle120.IndexOf(hit.Title120);
                            if (hit.ListOfIQStartMinute.Count > index)
                            {
                                hit.StartMinute = hit.ListOfIQStartMinute[index];
                            }
                        }
                    }
                }
                else if (!res.OriginalRequest.IsTitle120List && hit.ListOfIQStartMinute.Count > 0 && hit.TermOccurrences.Count > 0)
                {


                    TimeSpan ts = TimeSpan.FromSeconds(hit.TermOccurrences[0].TimeOffset);
                    var minute = hit.ListOfIQStartMinute.Where(a => a.Value <= ts.Minutes).LastOrDefault();
                    int index = minute.HasValue ? Convert.ToInt32(minute / 30) : -1;
                    if (index >= 0 && ListOfTitle120.Count > index)
                    {
                        hit.Title120 = ListOfTitle120[index];
                    }


                }
                res.Hits.Add(hit);
            }
        }

        private static void parseTVFullResponse(XmlDocument doc, TVFullResult res, string highlightCCFeildName, string highlightTitleFeildName)
        {

            XmlNode root = doc.SelectSingleNode("/response/result");

            if (root != null) try { res.TotalHitCount = Convert.ToInt32(root.Attributes.GetNamedItem("numFound").Value, System.Globalization.CultureInfo.CurrentCulture); }
                catch (Exception) { }

            // lets get list of all the hits we get 
            XmlNodeList hitNodes = doc.SelectNodes("/response/result/doc");

            // lets get list for all the highlighting 
            XmlNodeList occurenceNodes = doc.SelectNodes("/response/lst[@name='highlighting']/lst");

            res.Hits = new List<Hit>();

            // now we will parse each hit one by one. 
            foreach (XmlNode hitNode in hitNodes)
            {
                List<string> ListOfTitle120 = new List<string>();
                Hit hit = parseHit(hitNode, res.OriginalRequest.Title120, res.OriginalRequest.IsTitle120List, out ListOfTitle120);
                if (!string.IsNullOrWhiteSpace(res.OriginalRequest.Terms))
                {

                    // now its turn to get highlight 
                    // as in our response all list of highlight in different list 
                    // we will find highlight for our this hit by its iq_cc_key , as iq_cc_key is unique field in solr
                    XmlNode OccurenceNode = (XmlNode)(from XmlNode OccNode in occurenceNodes
                                                      where OccNode.Attributes["name"].Value.ToLower(System.Globalization.CultureInfo.CurrentCulture) == hit.Iqcckey.ToLower(System.Globalization.CultureInfo.CurrentCulture)
                                                      select OccNode.SelectSingleNode("arr[@name='" + highlightCCFeildName + "']")).FirstOrDefault();

                    if (OccurenceNode != null)
                    {
                        if (res.OriginalRequest.IsShowCC == true)
                        {
                            int TotalOccurences = 0;
                            List<TermOccurrence> closedCaption = new List<TermOccurrence>();
                            hit.TermOccurrences = parseCC(OccurenceNode, string.Empty, out TotalOccurences, res.OriginalRequest.FragOffset, out closedCaption);
                            hit.ClosedCaption = closedCaption != null && closedCaption.Count > 0 ? closedCaption : hit.ClosedCaption;
                            hit.TotalNoOfOccurrence = TotalOccurences;
                        }
                        else
                        {
                            hit.TotalNoOfOccurrence = OccurenceNode.SelectNodes("str").Count;

                            string text = string.Empty;
                            foreach (XmlNode xmlNode3 in OccurenceNode.SelectNodes("str"))
                            {

                                if (!string.IsNullOrWhiteSpace(xmlNode3.InnerText.Trim()) && Regex.IsMatch(xmlNode3.InnerText.Trim(), "^(\\d+s:\\s*\\w*)"))
                                {
                                    text += xmlNode3.InnerText;
                                }
                                else
                                {
                                    text = text + " 9999s: " + xmlNode3.InnerText;
                                }
                            }
                            int totalNoOfOccurrence2 = 0;
                            List<TermOccurrence> closedCaption = new List<TermOccurrence>();
                            hit.TermOccurrences = parseCC(OccurenceNode, text, out totalNoOfOccurrence2, res.OriginalRequest.FragOffset, out closedCaption);
                            hit.TotalNoOfOccurrence = totalNoOfOccurrence2;
                        }
                    }

                }

                if (!res.OriginalRequest.IsTitle120List && !string.IsNullOrEmpty(res.OriginalRequest.Title120))
                {

                    XmlNode Title120Node = (XmlNode)(from XmlNode OccNode in occurenceNodes
                                                     where OccNode.Attributes["name"].Value.ToLower(System.Globalization.CultureInfo.CurrentCulture) == hit.Iqcckey.ToLower(System.Globalization.CultureInfo.CurrentCulture)
                                                     select OccNode.SelectSingleNode("arr[@name='" + highlightTitleFeildName + "']")).FirstOrDefault();

                    // we found title120 from highlight , so we will override it. 
                    if (Title120Node != null && Title120Node.SelectNodes("str").Count > 0)
                    {
                        hit.Title120 = Title120Node.SelectNodes("str").Item(0).InnerText.Replace("<span class=\"highlight\">", string.Empty).Replace("</span>", string.Empty);
                        if (string.IsNullOrEmpty(res.OriginalRequest.Terms) && ListOfTitle120.Count > 0)
                        {
                            int index = ListOfTitle120.IndexOf(hit.Title120);
                            if (hit.ListOfIQStartMinute.Count > index)
                            {
                                hit.StartMinute = hit.ListOfIQStartMinute[index];
                            }
                        }
                    }
                }
                else if (!res.OriginalRequest.IsTitle120List && hit.ListOfIQStartMinute.Count > 0 && hit.TermOccurrences.Count > 0)
                {


                    TimeSpan ts = TimeSpan.FromSeconds(hit.TermOccurrences[0].TimeOffset);
                    var minute = hit.ListOfIQStartMinute.Where(a => a.Value <= ts.Minutes).LastOrDefault();
                    int index = minute.HasValue ? Convert.ToInt32(minute / 30) : -1;
                    if (index >= 0 && ListOfTitle120.Count > index)
                    {
                        hit.Title120 = ListOfTitle120[index];
                    }


                }
                res.Hits.Add(hit);
            }
        }

        private void makeSentimentRequest(SearchResult res, Int32? timeOutPeriod = null)
        {
            if (res.Hits.Count > 0)
            {
                SearchRequest _SearchRequest = new SearchRequest();
                _SearchRequest.IsSentiment = false;
                _SearchRequest.Terms = res.OriginalRequest.Terms;
                _SearchRequest.HighThreshold = res.OriginalRequest.HighThreshold;
                _SearchRequest.LowThreshold = res.OriginalRequest.LowThreshold;
                _SearchRequest.IQCCKeyList = string.Join(",", res.Hits.Select(h => h.Iqcckey).ToArray());
                _SearchRequest.PageSize = res.OriginalRequest.PageSize;
                _SearchRequest.IsOutRequest = true;
                SearchResult _res = Search(_SearchRequest, timeOutPeriod, true);

                foreach (Hit hit in res.Hits)
                {
                    CommonFunction.LogInfo("Try getting Sentiment for iq_cc_key : " + hit.Iqcckey);
                    hit.Sentiments = _res.Hits.Where(h => h.Iqcckey.Equals(hit.Iqcckey)).Select(h => h.Sentiments).FirstOrDefault();
                    if (hit.Sentiments != null)
                    {
                        foreach (SubSentiment ss in hit.Sentiments.HighlightToWeightMap)
                        {
                            CommonFunction.LogInfo("Sentiment : => " + ss.HighlightingText + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            CommonFunction.LogInfo("Sentiment Weight : => " + Convert.ToString(ss.Weight) + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                        }
                        CommonFunction.LogInfo("Positive Sentiment IQ_CC_Key : " + hit.Iqcckey + " => " + hit.Sentiments.PositiveSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                        CommonFunction.LogInfo("Negative Sentiment IQ_CC_Key : " + hit.Iqcckey + " => " + hit.Sentiments.NegativeSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                    }
                    else
                    {
                        hit.Sentiments = new Sentiments();
                    }
                }
            }

        }

        private void makeTVFullSentimentRequest(TVFullResult res, Int32? timeOutPeriod = null)
        {
            if (res.Hits.Count > 0)
            {
                SearchRequest _SearchRequest = new SearchRequest();
                _SearchRequest.IsSentiment = false;
                _SearchRequest.Terms = res.OriginalRequest.Terms;
                _SearchRequest.HighThreshold = res.OriginalRequest.HighThreshold;
                _SearchRequest.LowThreshold = res.OriginalRequest.LowThreshold;
                _SearchRequest.IQCCKeyList = string.Join(",", res.Hits.Select(h => h.Iqcckey).ToArray());
                _SearchRequest.PageSize = res.OriginalRequest.PageSize;
                _SearchRequest.IsOutRequest = true;
                SearchResult _res = Search(_SearchRequest, timeOutPeriod, true);

                foreach (Hit hit in res.Hits)
                {
                    CommonFunction.LogInfo("Try getting Sentiment for iq_cc_key : " + hit.Iqcckey);
                    hit.Sentiments = _res.Hits.Where(h => h.Iqcckey.Equals(hit.Iqcckey)).Select(h => h.Sentiments).FirstOrDefault();
                    if (hit.Sentiments != null)
                    {
                        foreach (SubSentiment ss in hit.Sentiments.HighlightToWeightMap)
                        {
                            CommonFunction.LogInfo("Sentiment : => " + ss.HighlightingText + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            CommonFunction.LogInfo("Sentiment Weight : => " + Convert.ToString(ss.Weight) + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                        }
                        CommonFunction.LogInfo("Positive Sentiment IQ_CC_Key : " + hit.Iqcckey + " => " + hit.Sentiments.PositiveSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                        CommonFunction.LogInfo("Negative Sentiment IQ_CC_Key : " + hit.Iqcckey + " => " + hit.Sentiments.NegativeSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                    }
                    else
                    {
                        hit.Sentiments = new Sentiments();
                    }
                }
            }

        }

        private static void parseSentiment(XmlDocument doc, SearchResult res, string highlightFeildName)
        {

            Dictionary<string, List<string>> _MapIQCCKeyToListOfHighlight = new Dictionary<string, List<string>>();

            _MapIQCCKeyToListOfHighlight = (from XmlNode OccNode in doc.SelectNodes("/response/lst[@name='highlighting']/lst")
                                            select new
                                            {
                                                IQCCKey = OccNode.Attributes["name"].Value,
                                                ListOfHighlight = (from XmlNode node in OccNode.SelectNodes("arr[@name='" + highlightFeildName + "']/str") select node.InnerText).ToList()
                                            }).ToDictionary(a => a.IQCCKey, a => a.ListOfHighlight);

            SentimentLogic _SentimentLogic = new SentimentLogic();
            Dictionary<string, Sentiments> _IQCCKeyToSentimentsMap = _SentimentLogic.GetSentiment(_MapIQCCKeyToListOfHighlight, res.OriginalRequest.LowThreshold.Value, res.OriginalRequest.HighThreshold.Value, res.OriginalRequest.ClientGuid);

            List<Hit> _Hits = new List<Hit>();
            foreach (string Key in _IQCCKeyToSentimentsMap.Keys)
            {
                Hit _Hit = new Hit();
                _Hit.Iqcckey = Key;
                Sentiments _Sentiments = new Sentiments();
                _IQCCKeyToSentimentsMap.TryGetValue(Key, out _Sentiments);
                _Hit.Sentiments = _Sentiments;
                _Hits.Add(_Hit);
            }
            res.Hits = _Hits;
        }

        private static void parseTVFullSentiment(XmlDocument doc, TVFullResult res, string highlightFeildName)
        {

            Dictionary<string, List<string>> _MapIQCCKeyToListOfHighlight = new Dictionary<string, List<string>>();

            _MapIQCCKeyToListOfHighlight = (from XmlNode OccNode in doc.SelectNodes("/response/lst[@name='highlighting']/lst")
                                            select new
                                            {
                                                IQCCKey = OccNode.Attributes["name"].Value,
                                                ListOfHighlight = (from XmlNode node in OccNode.SelectNodes("arr[@name='" + highlightFeildName + "']/str") select node.InnerText).ToList()
                                            }).ToDictionary(a => a.IQCCKey, a => a.ListOfHighlight);

            SentimentLogic _SentimentLogic = new SentimentLogic();
            Dictionary<string, Sentiments> _IQCCKeyToSentimentsMap = _SentimentLogic.GetSentiment(_MapIQCCKeyToListOfHighlight, res.OriginalRequest.LowThreshold.Value, res.OriginalRequest.HighThreshold.Value, res.OriginalRequest.ClientGuid);

            List<Hit> _Hits = new List<Hit>();
            foreach (string Key in _IQCCKeyToSentimentsMap.Keys)
            {
                Hit _Hit = new Hit();
                _Hit.Iqcckey = Key;
                Sentiments _Sentiments = new Sentiments();
                _IQCCKeyToSentimentsMap.TryGetValue(Key, out _Sentiments);
                _Hit.Sentiments = _Sentiments;
                _Hits.Add(_Hit);
            }
            res.Hits = _Hits;
        }

        private static List<TermOccurrence> parseCC(XmlNode OccurenceNode, string ccText, out int TotalOccurences, int FragOffset, out List<TermOccurrence> ClosedCaption)
        {
            TotalOccurences = 0;
            string txtHighlight = (!string.IsNullOrWhiteSpace(ccText)) ? ccText : OccurenceNode.InnerText;

            List<TermOccurrence> occurences = new List<TermOccurrence>();

            List<int> _ListOfProccesedElement = new List<int>();
            List<int> _ListOfProccesedOffset = new List<int>();

            // if our search is phrase search , 
            // then it is possibility that our highlight string is like
            // e.g. 4s: and in this <em>new</em>, 5s: <em>year</em> everybody try to make our planet clean and polution free. 
            // and we need to replace it with 
            //  4s: and in this <em>new</em>, <em>year</em> everybody try to make our planet clean and polution free. 
            // below is regex for that.
            txtHighlight = Regex.Replace(txtHighlight, "</span>(.)(\\s*)(\\d*)(s:)(\\s*)(.)<span class=\"highlight\"", "</span>$1$2<span class=\"highlight\"");

            // now get all the lines in CC,
            var pattern = @"\b(?=\s*\d{1,4}s:)";


            // we will split all the lines and get their index and item string
            // it will return text in ItemName and , its arry index no. in Postion
            // ItemName : 0s: hello world  , Position : 0
            // ItemName : 1s: this is new day  , Position : 1
            // ItemName : 9999s: this is new day  , Position : 2
            // ItemName : 649s: this is new day  , Position : 3
            var templines = new Regex(pattern).Split(txtHighlight).Where(
                s =>
                string.IsNullOrEmpty(s.Trim()) == false).Select((item, index) => new
                {
                    Offset = Regex.Match(item, "(\\d+)(s:)").ToString().Replace("s:", string.Empty),
                    SText = Regex.Replace(item, "(\\d*)(s:)", string.Empty),
                    MatchIndex = index
                }).ToList();

            var lines = templines.Select((item) => new
            {
                Offset = item.Offset == "9999" && item.MatchIndex < (templines.Count() - 1) ? (Convert.ToInt32(templines.ElementAt(item.MatchIndex + 1).Offset) - 1).ToString() : item.Offset,
                SText = item.SText,
                MatchIndex = item.MatchIndex
            }).OrderBy(o => Convert.ToInt32(o.Offset)).ToList();


            Func<string,string> EncodeString = (p_CCText) =>
            {
                p_CCText = Regex.Replace(p_CCText, "<span class=\"highlight\">", "@@@");
                p_CCText = Regex.Replace(p_CCText, "</span>", "@@@");
                p_CCText = System.Web.HttpUtility.HtmlEncode(p_CCText);
                p_CCText = Regex.Replace(p_CCText, "(@@@)(.*?)(@@@)", "<span class=\"highlight\">$2</span>");

                return p_CCText;
            };

            // ItemName : 0s: hello world  , Position : 0
            // ItemName : 1s: this is new day  , Position : 1
            // ItemName : 649s: this is new day  , Position : 2
            // ItemName : 9999s: this is new day  , Position : 3
            lines = lines.Select((item, index) => new
            {
                Offset = item.Offset,
                SText = EncodeString(item.SText),
                MatchIndex = index
            }).ToList();

            ClosedCaption = lines.Select(item => new TermOccurrence
            {
                SurroundingText = item.SText,
                TimeOffset = string.IsNullOrEmpty(item.Offset) ? 0 : Convert.ToInt32(item.Offset)
            }).ToList();

            if (lines != null && lines.Count() > 0)
            {
                // again check in for lines , if line do have hightlight term in that. 
                // if yest , then we'll get its offset (second liek 4s: , then 4 ) , 
                // and text spoken on that offset.
                var kvps = (from m in lines
                            where m.ToString().Contains("<span")
                            select m).ToList();


                // now we have all the list highlights in out 'kvps' linq object
                // lets add them in ours hit's TermOccurence list.
                foreach (var item in kvps)
                {
                    try
                    {
                        // for CC , we'll disply's its perior and its later index text.
                        // so below is processing for that.
                        if (!_ListOfProccesedElement.Contains(item.MatchIndex))
                        {

                            TermOccurrence oc = new TermOccurrence();
                            oc.SurroundingText = string.Empty;

                            for (int i = FragOffset; i > 0; i--)
                            {
                                if (item.MatchIndex - i >= 0 && !_ListOfProccesedElement.Contains(item.MatchIndex - i))
                                {
                                    oc.SurroundingText += lines.ElementAt(item.MatchIndex - i).SText;
                                    _ListOfProccesedElement.Add(item.MatchIndex - i);
                                }
                            }

                            oc.SurroundingText += item.SText;
                            _ListOfProccesedElement.Add(item.MatchIndex);

                            for (int i = 1; i <= FragOffset; i++)
                            {
                                if (item.MatchIndex + i < lines.Count())
                                {
                                    oc.SurroundingText += lines.ElementAt(item.MatchIndex + i).SText;
                                    _ListOfProccesedElement.Add(item.MatchIndex + i);
                                }
                            }

                            oc.TimeOffset = Convert.ToInt32(string.IsNullOrEmpty(item.Offset) ? "0" : item.Offset, System.Globalization.CultureInfo.CurrentCulture);                                                      
                            occurences.Add(oc);

                            _ListOfProccesedOffset.Add(item.MatchIndex);

                        }
                        //else
                        //    _ListOfProccesedOffset.Add(item.MatchIndex);
                    }
                    catch (Exception)
                    {

                    }
                }

                TotalOccurences = _ListOfProccesedOffset.Count();
            }

            return occurences;
        }

        private static Hit parseHit(XmlNode node, String searchTitle120, Boolean IsTitle120List, out List<string> ListOfTitle120)
        {
            Hit hit = new Hit();
            ListOfTitle120 = new List<string>();
            foreach (XmlNode cn in node.ChildNodes)
            {
                try
                {
                    // we will set all properties for our Hit object
                    // which we can parse by getting 'name' attribute of child node of signle search node. 
                    switch (cn.Attributes["name"].Value.ToLower(System.Globalization.CultureInfo.CurrentCulture))
                    {
                        case "guid":
                            hit.Guid = cn.InnerText;
                            break;
                        case "iq_cc_key":
                            hit.Iqcckey = cn.InnerText;
                            break;
                        case "datetime_dt":
                            hit.RLStationDateTime = Convert.ToDateTime(cn.InnerText).ToUniversalTime().ToString();
                            break;
                        case "gmtdatetime_dt":
                            hit.GmtDateTime = Convert.ToDateTime(cn.InnerText).ToUniversalTime();
                            break;
                        case "stationid":
                            hit.StationId = cn.InnerText;
                            break;
                        case "timezone":
                            hit.ClipTimeZone = cn.InnerText;
                            break;
                        case "gmtadj":
                            hit.GmtOffset = Convert.ToInt16(cn.InnerText, new System.Globalization.NumberFormatInfo());
                            break;
                        case "dstadj":
                            hit.DstOffset = Convert.ToInt16(cn.InnerText, new System.Globalization.NumberFormatInfo());
                            break;
                        case "affiliate":
                            hit.Affiliate = cn.InnerText;
                            break;
                        case "market":
                            hit.Market = cn.InnerText;
                            break;
                        case "iq_dma_num":
                            hit.IQDmaNum = cn.InnerText;
                            break;
                        case "hour":
                            hit.Hour = Convert.ToInt16(cn.InnerText, new System.Globalization.NumberFormatInfo());
                            break;
                        case "date":
                            hit.Timestamp = Convert.ToDateTime(cn.InnerText, new System.Globalization.DateTimeFormatInfo());
                            break;
                        case "appearing":
                            hit.Appearing = cn.InnerText;
                            break;
                        case "cc":

                            // now get all the lines in CC,
                            var pattern = @"\b(?=\s*\d{1,4}s:)";

                            hit.ClosedCaption = new Regex(pattern).Split(System.Web.HttpUtility.HtmlEncode(cn.InnerText)).Where(
                                s =>
                                string.IsNullOrEmpty(s.Trim()) == false).Select((item, index) => new TermOccurrence
                                {
                                    SurroundingText = Regex.Replace(item, "(\\d*)(s:)", string.Empty),
                                    TimeOffset = string.IsNullOrEmpty(Regex.Match(item, "(\\d+)(s:)").ToString().Replace("s:", string.Empty)) ? 0 : Convert.ToInt32(Regex.Match(item, "(\\d+)(s:)").ToString().Replace("s:", string.Empty)),
                                }).OrderBy(o => Convert.ToInt32(o.TimeOffset)).ToList();
                            break;
                        case "title120":
                            // as title120 is multivalued fields
                            // if search is made on title120 then we'll return first title120 match with search term.
                            // otherwise we'll return first title120 node. 
                            if (!IsTitle120List)
                            {

                                // if used searched on title120 , we override title120 later from highlighting

                                //if (!string.IsNullOrWhiteSpace(searchTitle120))
                                //{


                                /*string[] titles = searchTitle120.Replace("\"", string.Empty).Split(' ');
                                XmlNode titleNode = null;
                                foreach (string title in titles)
                                {
                                    titleNode = (XmlNode)(from XmlNode xmlNode in cn.SelectNodes("str")
                                                          where xmlNode.InnerText.ToLower().Contains(title.ToLower())
                                                          select xmlNode).FirstOrDefault();
                                    if (titleNode != null)
                                        break;
                                }
                                if (titleNode != null)
                                    hit.Title120 = titleNode.InnerText;
                                else
                                    hit.Title120 = cn.SelectNodes("str").Item(0).InnerText;*/
                                //}
                                //else
                                hit.Title120 = cn.SelectNodes("str").Item(0).InnerText;
                                ListOfTitle120 = (from XmlNode xmlNode in cn.SelectNodes("str")
                                                  select xmlNode.InnerText).ToList();
                            }
                            else
                            {
                                if (cn.SelectNodes("str").Count > 0)
                                {
                                    hit.ListOfTitle120 = (from XmlNode xmlNode in cn.SelectNodes("str")
                                                          select xmlNode.InnerText).ToList();
                                    ListOfTitle120 = hit.ListOfTitle120;
                                }
                            }
                            break;
                        case "iq_ssp_unique":
                            if (cn.SelectNodes("str").Count > 0)
                            {
                                hit.ListOfIQStartPoint = (from XmlNode xmlNode in cn.SelectNodes("str")
                                                          select xmlNode.InnerText.Split(',').Length > 1 ? Convert.ToInt32(xmlNode.InnerText.Split(',')[1]) : (int?)null).ToList();

                                hit.ListOfIQStartMinute = (from XmlNode xmlNode in cn.SelectNodes("str")
                                                           select xmlNode.InnerText.Split(',').Length > 4 ? Convert.ToInt32(xmlNode.InnerText.Split(',')[4]) : (int?)null).ToList();
                            }
                            break;
                        case "iq_class":
                            if (cn.SelectNodes("str").Count > 0)
                            {
                                hit.ListOfIQClass = (from XmlNode xmlNode in cn.SelectNodes("str")
                                                     select xmlNode.InnerText).ToList();

                            }
                            break;
                        case "seqid":
                        case "iqseqid":
                            hit.SeqID = cn.InnerText;
                            break;
                        case "logostatus":
                            hit.LogoStatus = Boolean.Parse(cn.InnerText);
                            break;
                        case "adstatus":
                            hit.AdStatus = Boolean.Parse(cn.InnerText);
                            break;
                        case "pestatus":
                            hit.PEStatus = Boolean.Parse(cn.InnerText);
                            break;
                        case "logospaid":
                            if (cn.SelectNodes("int").Count > 0)
                            {
                                hit.PaidLogoIDs = (from XmlNode xmlNode in cn.SelectNodes("int")
                                                     select Convert.ToInt64(xmlNode.InnerText)).ToList();

                            }
                            break;
                        case "logosearned":
                            if (cn.SelectNodes("int").Count > 0)
                            {
                                hit.EarnedLogoIDs = (from XmlNode xmlNode in cn.SelectNodes("int")
                                                   select Convert.ToInt64(xmlNode.InnerText)).ToList();

                            }
                            break;
                        case "brandspaid":
                            if (cn.SelectNodes("int").Count > 0)
                            {
                                hit.PaidBrandIDs = (from XmlNode xmlNode in cn.SelectNodes("int")
                                                   select Convert.ToInt64(xmlNode.InnerText)).ToList();

                            }
                            break;
                        case "brandsearned":
                            if (cn.SelectNodes("int").Count > 0)
                            {
                                hit.EarnedBrandIDs = (from XmlNode xmlNode in cn.SelectNodes("int")
                                                   select Convert.ToInt64(xmlNode.InnerText)).ToList();

                            }
                            break;
                        case "industriespaid":
                            if (cn.SelectNodes("str").Count > 0)
                            {
                                hit.PaidIndustryIDs = (from XmlNode xmlNode in cn.SelectNodes("str")
                                                   select Convert.ToInt64(xmlNode.InnerText)).ToList();

                            }
                            break;
                        case "industriesearned":
                            if (cn.SelectNodes("str").Count > 0)
                            {
                                hit.EarnedIndustryIDs = (from XmlNode xmlNode in cn.SelectNodes("str")
                                                   select Convert.ToInt64(xmlNode.InnerText)).ToList();

                            }
                            break;
                        case "companiespaid":
                            if (cn.SelectNodes("int").Count > 0)
                            {
                                hit.PaidCompanyIDs = (from XmlNode xmlNode in cn.SelectNodes("int")
                                                   select Convert.ToInt64(xmlNode.InnerText)).ToList();

                            }
                            break;
                        case "companiesearned":
                            if (cn.SelectNodes("int").Count > 0)
                            {
                                hit.EarnedCompanyIDs = (from XmlNode xmlNode in cn.SelectNodes("int")
                                                   select Convert.ToInt64(xmlNode.InnerText)).ToList();

                            }
                            break;
                        case "ads":
                            if (cn.SelectNodes("str").Count > 0)
                            {
                                hit.Ads = (from XmlNode xmlNode in cn.SelectNodes("str")
                                                   select xmlNode.InnerText).ToList();

                            }
                            break;
                        case "logos":
                            if (cn.SelectNodes("str").Count > 0)
                            {
                                hit.Logos = (from XmlNode xmlNode in cn.SelectNodes("str")
                                                   select xmlNode.InnerText).ToList();

                            }
                            break;
                    }
                }
                catch (Exception)
                {

                }
            }
            return hit;
        }

        public SearchNewsResults SearchNews(SearchNewsRequest request, Int32? timeOutPeriod = null, Boolean isParagraphSearch = false)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SearchNewsResults res = new SearchNewsResults();

            try
            {


                /*CommonFunction.LogInfo("PMG Call Start", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Create Request", request.IsPmgLogging, request.PmgLogFileLocation);*/


                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q qeury parameter of solr and 
                // 'FQuery' we will pass in the fq qeury parameter of solr 
                StringBuilder Query = new StringBuilder();
                StringBuilder FQuery = new StringBuilder();

                /* Commented below condition as Now we are passing pagenumber based on 0 */
                /*if (request.PageNumber <= 0)
                {
                    request.PageNumber = 1;
                }*/

                // start and rows return the required page. No. data.
                if (request.Start != null)
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.Start), System.Globalization.CultureInfo.CurrentCulture)));
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                }

                vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));

                string Fl = string.Empty;
                string FlTitleOnSearchTerm = "";

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string RequestTerm = request.SearchTerm.Trim();

                    if (RequestTerm.EndsWith("#"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            Fl = "content_gen";
                            FlTitleOnSearchTerm = "title_gen";
                        }
                        else
                        {
                            Fl = "content";
                            FlTitleOnSearchTerm = "title";
                        }
                    }
                    else
                    {
                        Fl = "content_gen";
                        FlTitleOnSearchTerm = "title_gen";
                    }

                    // New Note : we'll change the search term to lower case , but doing this
                    // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                    List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                    RequestTerm = Regex.Replace(
                                RequestTerm,
                                @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                        //@"([\""][\w ]+[\""])|(\w+)",
                                m => reservedWords.Contains(m.Value) ? m.Value : m.Value
                                );

                    if (request.IsTitleNContentSearch)
                    {
                        Query = Query.AppendFormat("({0}:({1}) OR {2}:({1}))", Fl, RequestTerm, FlTitleOnSearchTerm);
                    }
                    else
                    {
                        Query = Query.AppendFormat("{0}:({1})", Fl, RequestTerm);
                    }

                    if (request.IsSentiment || request.IsReturnHighlight || isParagraphSearch)
                    {
                        // all these feilds are for highlighting functionality
                        // hl.fl =  name of the feild on which need to provide highlighting
                        // hl = value can be on/off , if on then highlighting feature is enabled otherwise disabled.
                        // hl.maxAnalyzedChars =  default max char length for highlight is 51200 , but we need unlimited
                        if (!isParagraphSearch)
                        {
                            vars.Add(new KeyValuePair<string, string>("hl.fl", Fl));
                        }
                        else
                        {
                            vars.Add(new KeyValuePair<string, string>("hl.fl", Fl + "," + FlTitleOnSearchTerm));
                        }

                        if (!isParagraphSearch)
                        {
                            vars.Add(new KeyValuePair<string, string>("hl.requireFieldMatch", "true")); 
                        }

                        vars.Add(new KeyValuePair<string, string>("hl", "on"));

                        if (!isParagraphSearch)
                        {
                            vars.Add(new KeyValuePair<string, string>("hl.maxAnalyzedChars", "-1"));
                        }
                        else
                        {
                            vars.Add(new KeyValuePair<string, string>("hl.maxAnalyzedChars", request.LeadParagraphChars > 0 ? request.LeadParagraphChars.ToString() : "500"));
                        }


                        // by setting it to 225 ,it will return no. of highlights 
                        // fragment size for signle highlight is 145 (approx)
                        if (request.FragSize.HasValue)
                        {
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", request.FragSize.Value.ToString()));
                        }
                        else if (request.IsSentiment)
                        {
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSizeForSentiment"]));
                        }
                        else
                        {
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSize"]));
                        }

                        vars.Add(new KeyValuePair<string, string>("hl.snippets", "99"));
                    }
                }


                // if ID(s) are passed , then lets make search for ID(s)
                if (request.IDs != null && request.IDs.Count() > 0)
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" (");
                    }
                    else
                    {
                        Query = Query.Append(" AND (");
                    }

                    bool IsFirst = true;

                    foreach (string _id in request.IDs)
                    {
                        if (IsFirst)
                        {
                            IsFirst = false;
                            Query = Query.AppendFormat(" iqseqid:{0}", _id);
                        }
                        else
                        {
                            Query = Query.AppendFormat(" OR iqseqid:{0}", _id);
                        }
                    }

                    Query = Query.Append(" )");
                }

                // ooops nothing passed for 'q' search of solr.
                // then as q is complesary search
                // if nothing is passed then we should pass *:*
                // which mean it return all without making search for q.
                // although after q , it will filter on fq (filter query) search. 
                if (request.SourceType == null || request.SourceType.Count == 0)
                {
                    // Ensure backwards compatibility with code written before the SourceType property was introduced
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat("iqsubmediatype:({0} {1})", (int)SourceType.OnlineNews, (int)SourceType.Print);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND iqsubmediatype:({0} {1})", (int)SourceType.OnlineNews, (int)SourceType.Print);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" AND");
                    }

                    Query = Query.Append(" iqsubmediatype:(");
                    foreach (int sourceType in request.SourceType)
                    {
                        Query = Query.Append(sourceType + " ");
                    }
                    Query = Query.Append(")");
                }

                /*string SortFields = string.Empty;

                // lets make solr fields and pass to solr
                if (!string.IsNullOrEmpty(request.SortFields))
                {
                    SortFields = GenerateSortField(request.SortFields);
                }

                if (request.SortFields != null)
                {
                    vars.Add(new KeyValuePair<string, string>("sort", SortFields));
                }*/

                // if both start and enddate is passed then lets make search on RL_Station_DateTime.
                if (request.StartDate != null && request.EndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" harvestdate_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND harvestdate_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                if (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPmgNewsLicenseSearch"]) && request.IQLicense != null && request.IQLicense.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (Int16 _newsRight in request.IQLicense)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" iqlicense:\"{0}\"", _newsRight);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR iqlicense:\"{0}\"", _newsRight);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }


                string SortFields = string.Empty;

                // lets make solr fields and pass to solr
                if (!string.IsNullOrEmpty(request.SortFields))
                {
                    SortFields = GenerateNewsSortField(request.SortFields);
                }

                if (request.SortFields != null)
                {
                    vars.Add(new KeyValuePair<string, string>("sort", SortFields));
                }


                // if Source is passed , then lets search on Source
                if (!string.IsNullOrEmpty(request.Source))
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.AppendFormat(" source:({0})", request.Source.Trim());
                    }
                    else
                    {
                        FQuery = FQuery.AppendFormat(" AND source:({0})", request.Source.Trim());
                    }
                }

                // if Genre(s) are passed , then lets make search for Genre(s)
                if (request.Genre != null && request.Genre.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (string _gnr in request.Genre)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" genre:\"{0}\"", _gnr.Trim());
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR genre:\"{0}\"", _gnr.Trim());
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                //market and dmaid advanced search
                if (request.Market != null && request.Market.Count() > 0) 
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else 
                    {
                        FQuery = FQuery.Append(" AND (");
                    }
                    bool IsFirst = true;
                    foreach (string mrk in request.Market)
                    {
                        if (IsFirst)
                        {
                            FQuery = FQuery.AppendFormat(" iqdmaname:\"{0}\"", mrk.Trim());
                            IsFirst = false;
                        }
                        else 
                        {
                            FQuery = FQuery.AppendFormat(" OR iqdmaname:\"{0}\"", mrk.Trim());
                        }
                    }
                    FQuery = FQuery.Append(" )");
                }

                // if FromRecordID is not null then get data after this recordID
                if (request.FromRecordID != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    FQuery = FQuery.AppendFormat(" iqseqid:[* TO {0}])", request.FromRecordID);
                }

                // if News Category(s) are passed , then lets make search for News Category(s)
                if (request.NewsCategory != null && request.NewsCategory.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (string _newsCat in request.NewsCategory)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" source_category:\"{0}\"", _newsCat.Trim());
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR source_category:\"{0}\"", _newsCat.Trim());
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if News Category(s) are passed , then lets make search for News Category(s)
                if (request.NewsRegion != null && request.NewsRegion.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (string _newsregion in request.NewsRegion)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" region:\"{0}\"", _newsregion.Trim());
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR region:\"{0}\"", _newsregion.Trim());
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // Country

                if (request.Country != null && request.Country.Count > 0)
                {
                    FQuery = FQuery.Append((FQuery.Length == 0 ? "(country_code:" : " AND (country_code:") + string.Join(" OR country_code:", request.Country.Select(c => "\"" + c + "\"")) + " OR country_code:" + string.Join(" OR country_code:", request.Country.Select(c => "\"" + c.ToLower() + "\"")) + ")");
                }

                // Language

                if (request.Language != null && request.Language.Count > 0)
                {
                    FQuery = FQuery.Append((FQuery.Length == 0 ? "(language:" : " AND (language:") + string.Join(" OR language:", request.Language.Select(l => "\"" + l + "\"")) + ")");
                }


                // Publications
                if (request.Publications != null && request.Publications.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool isFirstPublication = true;
                    string tempPub = "";

                    foreach (string publication in request.Publications)
                    {
                        if (!String.IsNullOrWhiteSpace(publication))
                        {
                            tempPub = publication;

                            if (!tempPub.Contains("*"))
                            {
                                if (!tempPub.StartsWith("\""))
                                    tempPub = "\"" + tempPub;
                                if (!tempPub.EndsWith("\""))
                                    tempPub += "\"";
                            }

                            if (isFirstPublication)
                            {
                                isFirstPublication = false;
                                FQuery = FQuery.AppendFormat(" homeurl_domain:{0}", tempPub);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR homeurl_domain:{0}", tempPub);
                            }
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // Exclude Domains
                if (request.ExcludeDomains != null && request.ExcludeDomains.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" NOT (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND NOT (");
                    }

                    bool IsFirstExcludeDomain = true;
                    string tempDomain = "";

                    foreach (string domain in request.ExcludeDomains)
                    {
                        if (!String.IsNullOrWhiteSpace(domain))
                        {
                            tempDomain = domain;

                            if (!tempDomain.Contains("*"))
                            {
                                if (!tempDomain.StartsWith("\""))
                                    tempDomain = "\"" + tempDomain;
                                if (!tempDomain.EndsWith("\""))
                                    tempDomain += "\"";
                            }

                            if (IsFirstExcludeDomain)
                            {
                                IsFirstExcludeDomain = false;
                                FQuery = FQuery.AppendFormat(" homeurl_domain:{0}", tempDomain);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR homeurl_domain:{0}", tempDomain);
                            }
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if Publication Category(s) are passed , then lets make search for Publication Category(s)
                if (request.PublicationCategory != null && request.PublicationCategory.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (int _publicationCat in request.PublicationCategory)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" editorialrank:{0}", _publicationCat);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR editorialrank:{0}", _publicationCat);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }



                /*// if TimeZone is passed , then lets make search for TimeZone
                if (!string.IsNullOrEmpty(request.TimeZone) && request.TimeZone.ToLower(System.Globalization.CultureInfo.CurrentCulture) != "all")
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" timezone:");
                        FQuery = FQuery.Append(request.TimeZone);
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND timezone:");
                        FQuery = FQuery.Append(request.TimeZone);
                    }
                }*/

                // our q query is ready!!! 
                // lets add it to keyvalue pair for q,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                // as fq is not compelsary search 
                // we will pass it only if there is search criteria on fq 
                if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                {
                    vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                }

                // to make search effective and to reduce search time ,
                // we should only retrun the fields which are required
                // so lets add fields that we need to return , this is config driven,
                // to add any field , we just need to change in config :)
                if (!isParagraphSearch)
                {
                    if (!String.IsNullOrWhiteSpace(request.FieldList))
                    {
                        vars.Add(new KeyValuePair<string, string>("fl", request.FieldList));
                    }
                    else
                    {
                        if (request.IsShowContent)
                        //vars.Add(new KeyValuePair<string, string>("fl", "content"));
                        {
                            vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["SolrNewsContentFL"]));
                        }
                        else
                        {
                            vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["solrNewsFL"]));
                        }
                    }
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("fl", "null"));
                }

                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                string RequestURL = string.Empty;
                if (request.IsOutRequest)
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                    res.RequestUrl = RequestURL;
                }
                else
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                }

                //CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);

                //CommonFunction.LogInfo("Solr Response - TimeTaken - for get response" + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                XmlDocument xDoc = new XmlDocument();
                // lets load solr response to xml so we can get data in xmk format
                xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                res.OriginalRequest = request;

                //CommonFunction.LogInfo("Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);

                // lets parse response. to our 'SearchResult' object
                if (!isParagraphSearch)
                {
                    parseNewsResponse(xDoc, res);
                }
                else
                {
                    parseParagraphSearch(xDoc, res, Fl, FlTitleOnSearchTerm);
                }

                if (request.IsHilightInLeadParagraph && !string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    CheckNMHighlightInLeadParagraph(res, request.LeadParagraphChars, timeOutPeriod);
                }


                sw.Stop();

                //CommonFunction.LogInfo("Solr Response - TimeTaken - for parse response" + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                /*CommonFunction.LogInfo(string.Format("Total Hti Count :{0}", res.TotalHitCount), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("PMG Call End", request.IsPmgLogging, request.PmgLogFileLocation);*/

                return res;
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo(_Exception.ToString(), request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        public string SearchNewsChart(SearchNewsRequest request, out Boolean isError, Int32? timeOutPeriod = null)
        {
            isError = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SearchNewsResults res = new SearchNewsResults();
            string responseString = string.Empty;
            try
            {


                /*CommonFunction.LogInfo("PMG Call Start", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Create Request", request.IsPmgLogging, request.PmgLogFileLocation);*/


                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q qeury parameter of solr and 
                // 'FQuery' we will pass in the fq qeury parameter of solr 
                StringBuilder Query = new StringBuilder();
                StringBuilder FQuery = new StringBuilder();

                /* Commented below condition as Now we are passing pagenumber based on 0 */
                /*if (request.PageNumber <= 0)
                {
                    request.PageNumber = 1;
                }*/

                // start and rows return the required page. No. data.
                vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string Fl = string.Empty;
                    string FlTitleOnSearchTerm = string.Empty;
                    string RequestTerm = request.SearchTerm.Trim();

                    if (RequestTerm.EndsWith("#"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            Fl = "content_gen";
                            FlTitleOnSearchTerm = "title_gen";
                        }
                        else
                        {
                            Fl = "content";
                            FlTitleOnSearchTerm = "title";
                        }
                    }
                    else
                    {
                        Fl = "content_gen";
                        FlTitleOnSearchTerm = "title_gen";
                    }

                    // New Note : we'll change the search term to lower case , but doing this
                    // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                    List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                    RequestTerm = Regex.Replace(
                                RequestTerm,
                                @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                        //@"([\""][\w ]+[\""])|(\w+)",
                                m => reservedWords.Contains(m.Value) ? m.Value : m.Value
                                );

                    if (request.IsTitleNContentSearch)
                    {
                        Query = Query.AppendFormat("({0}:({1}) OR {2}:({1}))", Fl, RequestTerm, FlTitleOnSearchTerm);
                    }
                    else
                    {
                        Query = Query.AppendFormat("{0}:({1})", Fl, RequestTerm);
                    }

                }

                // ooops nothing passed for 'q' search of solr.
                // then as q is complesary search
                // if nothing is passed then we should pass *:*
                // which mean it return all without making search for q.
                // although after q , it will filter on fq (filter query) search. 
                if (request.SourceType == null || request.SourceType.Count == 0)
                {
                    // Ensure backwards compatibility with code written before the SourceType property was introduced
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat("iqsubmediatype:({0} {1})", (int)SourceType.OnlineNews, (int)SourceType.Print);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND iqsubmediatype:({0} {1})", (int)SourceType.OnlineNews, (int)SourceType.Print);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" AND");
                    }

                    Query = Query.Append(" iqsubmediatype:(");
                    foreach (int sourceType in request.SourceType)
                    {
                        Query = Query.Append(sourceType + " ");
                    }
                    Query = Query.Append(")");
                }

                string SortFields = string.Empty;

                // lets make solr fields and pass to solr
                if (!string.IsNullOrEmpty(request.SortFields))
                {
                    SortFields = GenerateNewsSortField(request.SortFields);
                }

                if (request.SortFields != null)
                {
                    vars.Add(new KeyValuePair<string, string>("sort", SortFields));
                }


                /*string SortFields = string.Empty;

                // lets make solr fields and pass to solr
                if (!string.IsNullOrEmpty(request.SortFields))
                {
                    SortFields = GenerateSortField(request.SortFields);
                }

                if (request.SortFields != null)
                {
                    vars.Add(new KeyValuePair<string, string>("sort", SortFields));
                }*/

                // if both start and enddate is passed then lets make search on RL_Station_DateTime.
                if (request.StartDate != null && request.EndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" harvestdate_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND harvestdate_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                if (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPmgNewsLicenseSearch"]) && request.IQLicense != null && request.IQLicense.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (Int16 _newsRight in request.IQLicense)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" iqlicense:\"{0}\"", _newsRight);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR iqlicense:\"{0}\"", _newsRight);
                        }

                        if (_newsRight == 0)
                        {
                            FQuery = FQuery.Append(" OR (-iqlicense:[* TO *])");
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                if (request.Facet)
                {
                    vars.Add(new KeyValuePair<string, string>("facet", "on"));

                    //FQuery = FQuery.Append("&facet=on");
                    if (!string.IsNullOrEmpty(request.FacetRangeOther))
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.other", request.FacetRangeOther));
                        //FQuery = FQuery.Append("&facet.range.other=" + request.FacetRangeOther);
                    }

                    if (!string.IsNullOrWhiteSpace(request.FacetRange))
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range", request.FacetRange));
                    }
                    if (request.FacetRangeStarts != null && request.FacetRangeEnds != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.start", request.FacetRangeStarts.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
                        vars.Add(new KeyValuePair<string, string>("facet.range.end", request.FacetRangeEnds.Value.AddSeconds(1).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
                        /*FQuery = FQuery.Append("&facet.range.start=" + request.FacetRangeStarts.Value.ToString().Substring(0, 10) + "T" + request.FacetRangeStarts.Value.Hour + ":" + request.FacetRangeStarts.Value.Minute + ":" + request.FacetRangeStarts.Value.Second + "Z");
                        FQuery = FQuery.Append("&facet.range.end=" + request.FacetRangeEnds.Value.ToString().Substring(0, 10) + "T" + request.FacetRangeEnds.Value.Hour + ":" + request.FacetRangeEnds.Value.Minute + ":" + request.FacetRangeEnds.Value.Second + "Z");*/
                    }

                    if (request.FacetRangeGap != null && request.FacetRangeGapDuration != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.gap", "+" + request.FacetRangeGapDuration + request.FacetRangeGap));
                        //FQuery = FQuery.Append("&facet.range.gap=%2B" + request.FacetRangeGapDuration + request.FacetRangeGap);
                    }
                    if (request.wt != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("wt", request.wt.ToString()));
                    }
                }



                // if Source is passed , then lets search on Source
                if (!string.IsNullOrEmpty(request.Source))
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.AppendFormat(" source:({0})", request.Source);
                    }
                    else
                    {
                        FQuery = FQuery.AppendFormat(" AND source:({0})", request.Source);
                    }
                }

                // if FromRecordID is not null then get data after this recordID
                if (request.FromRecordID != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    FQuery = FQuery.AppendFormat(" iqseqid:[* TO {0}])", request.FromRecordID);
                }

                //if Market(s) are passed , then lets make a search for Market(s) 
                if (request.Market != null && request.Market.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }
                    var IsFirstIQDmaName = true;
                    foreach (string _newsMark in request.Market)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" iqdmaname:\"{0}\"", _newsMark);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR iqdmaname:\"{0}\"", _newsMark);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if Genre(s) are passed , then lets make search for Genre(s)
                if (request.Genre != null && request.Genre.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (string _gnr in request.Genre)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" genre:\"{0}\"", _gnr);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR genre:\"{0}\"", _gnr);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if News Category(s) are passed , then lets make search for News Category(s)
                if (request.NewsCategory != null && request.NewsCategory.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (string _newsCat in request.NewsCategory)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" source_category:{0}", _newsCat);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR source_category:{0}", _newsCat);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if News Category(s) are passed , then lets make search for News Category(s)
                if (request.NewsRegion != null && request.NewsRegion.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirstIQDmaName = true;

                    foreach (string _newsregion in request.NewsRegion)
                    {
                        if (IsFirstIQDmaName)
                        {
                            IsFirstIQDmaName = false;
                            FQuery = FQuery.AppendFormat(" region:{0}", _newsregion);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR region:{0}", _newsregion);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if Publication Category(s) are passed , then lets make search for Publication Category(s)
                if (request.lstfacetRange != null && request.lstfacetRange.Count() > 0)
                {
                    foreach (string publicationCat in request.lstfacetRange)
                    {
                        if (request.PublicationCategory.Count <= 0 || request.PublicationCategory.Contains(Convert.ToInt32(publicationCat)))
                        {
                            string csvList = string.Join(",", (from cat in request.lstfacetRange
                                                               where cat != publicationCat
                                                               select cat).ToArray());
                            vars.Add(new KeyValuePair<string, string>("facet.range", "{!key=" + publicationCat + " ex=" + csvList + "}harvestdate_dt"));
                            vars.Add(new KeyValuePair<string, string>("fq", "{!tag=" + publicationCat + "}editorialrank:" + publicationCat + ""));
                        }

                    }
                }
                if (request.PublicationCategory != null && request.PublicationCategory.Count > 0)
                {
                    FQuery = FQuery.Append((FQuery.Length == 0 ? "(editorialrank:" : " AND (editorialrank:") + string.Join(" OR editorialrank:", request.PublicationCategory.Select(p => "\"" + p + "\"")) + ")");
                }

                // Country

                if (request.Country != null && request.Country.Count > 0)
                {
                    FQuery = FQuery.Append((FQuery.Length == 0 ? "(country_code:" : " AND (country_code:") + string.Join(" OR country_code:", request.Country.Select(c => "\"" + c + "\"")) + " OR country_code:" + string.Join(" OR country_code:", request.Country.Select(c => "\"" + c.ToLower() + "\"")) + ")");
                }

                // Language

                if (request.Language != null && request.Language.Count > 0)
                {
                    FQuery = FQuery.Append((FQuery.Length == 0 ? "(language:" : " AND (language:") + string.Join(" OR language:", request.Language.Select(l => "\"" + l + "\"")) + ")");
                }

                // Publications
                if (request.Publications != null && request.Publications.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool isFirstPublication = true;
                    string tempPub = "";

                    foreach (string publication in request.Publications)
                    {
                        if (!String.IsNullOrWhiteSpace(publication))
                        {
                            tempPub = publication;

                            if (!tempPub.Contains("*"))
                            {
                                if (!tempPub.StartsWith("\""))
                                    tempPub = "\"" + tempPub;
                                if (!tempPub.EndsWith("\""))
                                    tempPub += "\"";
                            }

                            if (isFirstPublication)
                            {
                                isFirstPublication = false;
                                FQuery = FQuery.AppendFormat(" homeurl_domain:{0}", tempPub);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR homeurl_domain:{0}", tempPub);
                            }
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // Exclude Domains
                if (request.ExcludeDomains != null && request.ExcludeDomains.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" NOT (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND NOT (");
                    }

                    bool IsFirstExcludeDomain = true;
                    string tempDomain = "";

                    foreach (string domain in request.ExcludeDomains)
                    {
                        if (!String.IsNullOrWhiteSpace(domain))
                        {
                            tempDomain = domain;

                            if (!tempDomain.Contains("*"))
                            {
                                if (!tempDomain.StartsWith("\""))
                                    tempDomain = "\"" + tempDomain;
                                if (!tempDomain.EndsWith("\""))
                                    tempDomain += "\"";
                            }

                            if (IsFirstExcludeDomain)
                            {
                                IsFirstExcludeDomain = false;
                                FQuery = FQuery.AppendFormat(" homeurl_domain:{0}", tempDomain);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR homeurl_domain:{0}", tempDomain);
                            }
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // our q query is ready!!! 
                // lets add it to keyvalue pair for q,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                // as fq is not compelsary search 
                // we will pass it only if there is search criteria on fq 
                if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                {
                    vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                }

                // to make search effective and to reduce search time ,
                // we should only retrun the fields which are required
                // so lets add fields that we need to return , this is config driven,
                // to add any field , we just need to change in config :)
                vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["solrNewsFL"]));

                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                string RequestURL = string.Empty;
                if (request.IsOutRequest)
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                    res.RequestUrl = RequestURL;
                }
                else
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                }



                //CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);

                //CommonFunction.LogInfo("Solr Response - TimeTaken - for get response" + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                res.OriginalRequest = request;
                sw.Stop();



                //CommonFunction.LogInfo(string.Format("Total Hti Count :{0}", res.TotalHitCount), request.IsPmgLogging, request.PmgLogFileLocation);

                //CommonFunction.LogInfo("PMG Call End", request.IsPmgLogging, request.PmgLogFileLocation);

                return res.ResponseXml;
            }
            catch (Exception _Exception)
            {
                /*CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);*/
                isError = true;
                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res.ResponseXml;
            }
        }

        private static void parseNewsResponse(XmlDocument doc, SearchNewsResults res, bool isGet = false)
        {
            try
            {
                XDocument xDoc = XDocument.Parse(Convert.ToString(doc.InnerXml));

                res.newsResults = new List<NewsResult>();

                int totalRecords;

                if (Int32.TryParse((from p in xDoc.Descendants("response").Descendants("result")
                                        select p.Attribute("numFound").Value).FirstOrDefault(), 
                                    out totalRecords))
                {
                    res.TotalResults = totalRecords;
                }

                XElement occurenceNodes = (from p in xDoc.Descendants("response").Descendants("lst")
                                           where p.Attribute("name").Value == "highlighting"
                                           select p).FirstOrDefault();

                List<XElement> docNodes = xDoc.Descendants("response").Descendants("result").Descendants("doc").ToList();
                if ((docNodes == null || docNodes.Count == 0) && isGet)
                {
                    // If performing a get request and only one record is returned, there is no result node
                    docNodes = xDoc.Descendants("response").Descendants("doc").ToList();
                }

                foreach (XElement elem in docNodes)
                {
                    NewsResult newsResult = new NewsResult();
                    foreach (XElement childelem in elem.Elements())
                    {

                        if (childelem.Attribute("name").Value == "genre")
                        {
                            newsResult.Genre = childelem.Value;
                        }

                        if (childelem.Attribute("name").Value == "harvestdate_dt")
                        {
                            //newsResult.date = Convert.ToDateTime(childelem.Value).ToUniversalTime();
                            newsResult.date = Convert.ToDateTime(childelem.Value).ToUniversalTime().ToString();
                        }

                        if (childelem.Attribute("name").Value == "title")
                        {
                            newsResult.Title = childelem.Value;
                        }

                        if (childelem.Attribute("name").Value == "source_category")
                        {
                            newsResult.Category = childelem.Value;
                        }

                        if (childelem.Attribute("name").Value == "url")
                        {
                            newsResult.Article = childelem.Value;
                        }

                        if (childelem.Attribute("name").Value == "homeurl_domain")
                        {
                            newsResult.HomeurlDomain = childelem.Value;
                        }

                        if (childelem.Attribute("name").Value == "content")
                        {
                            newsResult.Content = childelem.Value;
                        }

                        if (childelem.Attribute("name").Value == "iqdmaname")
                        {
                            newsResult.IQDmaName = childelem.Value;
                        }

                        if (childelem.Attribute("name").Value == "iqseqid")
                        {
                            newsResult.IQSeqID = childelem.Value;
                            if ((res.OriginalRequest.IsReturnHighlight) && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm))
                            {
                                var Occurances = (from c in occurenceNodes.Descendants("lst")
                                                  where c.Attribute("name").Value == childelem.Value
                                                  select c);

                                newsResult.Mentions = Occurances != null ? Occurances.Descendants("str").Count() : 0;

                                newsResult.Highlights = Occurances.Descendants("str").Select(a => a.Value).ToList();
                            }
                        }
                        else if (childelem.Attribute("name").Value == "lastmoreoverid")
                        {
                            newsResult.LastMoreoverID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "id")
                        {
                            newsResult.ID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "seqid")
                        {
                            newsResult.SeqID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "source")
                        {
                            newsResult.Source = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "dataformat")
                        {
                            newsResult.DataFormat = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "homeurl")
                        {
                            newsResult.publication = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "duplicategroupid")
                        {
                            newsResult.DuplicateGroupID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "editorialrank" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            newsResult.EditorialRank = Convert.ToInt32(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "region")
                        {
                            newsResult.Region = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "subregion")
                        {
                            newsResult.SubRegion = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "country")
                        {
                            newsResult.Country = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "country_code")
                        {
                            newsResult.CountryCode = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "zipcode")
                        {
                            newsResult.ZipCode = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "autorank" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            newsResult.AutoRank = Convert.ToInt32(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "mediatype")
                        {
                            newsResult.MediaType = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "language")
                        {
                            newsResult.Language = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "sourcefeedlanguage")
                        {
                            newsResult.SourceFeedLanguage = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "publisheddate" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            newsResult.PublishedDate = Convert.ToDateTime(childelem.Value).ToUniversalTime();
                        }
                        else if (childelem.Attribute("name").Value == "inwhitelist" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            newsResult.InWhiteList = Convert.ToBoolean(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "inboundlinkcount" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            newsResult.InBoundLinkCount = Convert.ToInt32(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "autorankorder" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            newsResult.AutoRankOrder = Convert.ToInt32(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "authorname")
                        {
                            newsResult.AuthorName = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "topicname")
                        {
                            newsResult.TopicNames = childelem.Descendants("str").ToList().Select(x => x.Value.ToString()).ToList();
                        }
                        else if (childelem.Attribute("name").Value == "topicgroup")
                        {
                            newsResult.TopicGroups = childelem.Descendants("str").ToList().Select(x => x.Value.ToString()).ToList();
                        }
                        else if (childelem.Attribute("name").Value == "ziparea")
                        {
                            newsResult.ZipArea = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "state")
                        {
                            newsResult.State = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "companysymbol")
                        {
                            newsResult.CompanySymbol = childelem.Descendants("str").ToList().Select(x => x.Value.ToString()).ToList();
                        }
                        else if (childelem.Attribute("name").Value == "companyexchange")
                        {
                            newsResult.CompanyExchange = childelem.Descendants("str").ToList().Select(x => x.Value.ToString()).ToList();
                        }
                        else if (childelem.Attribute("name").Value == "iqmediatype")
                        {
                            newsResult.IQMediaType = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "iqsubmediatype")
                        {
                            newsResult.IQSubMediaType = Convert.ToInt32(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "iqlicense")
                        {
                            newsResult.IQLicense = Convert.ToInt16(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "copyright")
                        {
                            newsResult.Copyright = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "activationurl")
                        {
                            newsResult.ActivationUrl = childelem.Value;
                        }
                    }
                    res.newsResults.Add(newsResult);
                }

                if (res.OriginalRequest.IsSentiment && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm) && res.OriginalRequest.LowThreshold != null && res.OriginalRequest.HighThreshold != null)
                {
                    Dictionary<string, List<string>> _MapIDToListOfHighlight = new Dictionary<string, List<string>>();

                    _MapIDToListOfHighlight = (from XmlNode OccNode in doc.SelectNodes("/response/lst[@name='highlighting']/lst")
                                               select new
                                               {
                                                   ID = OccNode.Attributes["name"].Value,
                                                   ListOfHighlight = (from XmlNode node in OccNode.SelectNodes("arr/str") select node.InnerText).ToList()
                                               }).ToDictionary(a => a.ID, a => a.ListOfHighlight);

                    SentimentLogic _SentimentLogic = new SentimentLogic();
                    Dictionary<string, Sentiments> _IDToSentimentsMap = _SentimentLogic.GetSentiment(_MapIDToListOfHighlight, res.OriginalRequest.LowThreshold.Value, res.OriginalRequest.HighThreshold.Value, res.OriginalRequest.ClientGuid);

                    foreach (NewsResult newsResult in res.newsResults)
                    {
                        Sentiments _Sentiments = new Sentiments();
                        if (_IDToSentimentsMap.TryGetValue(newsResult.IQSeqID, out _Sentiments))
                        {
                            newsResult.Sentiments = _Sentiments;
                            foreach (SubSentiment ss in _Sentiments.HighlightToWeightMap)
                            {
                                CommonFunction.LogInfo("Sentiment : => " + ss.HighlightingText + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                                CommonFunction.LogInfo("Sentiment Weight : => " + Convert.ToString(ss.Weight) + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            }
                            CommonFunction.LogInfo("Positive Sentiment ID : " + newsResult.IQSeqID + " => " + _Sentiments.PositiveSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            CommonFunction.LogInfo("Negative Sentiment ID : " + newsResult.IQSeqID + " => " + _Sentiments.NegativeSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                        }
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }


        }

        private void CheckNMHighlightInLeadParagraph(SearchNewsResults res, int p_LeadParagraphChars, Int32? timeOutPeriod = null)
        {
            if (res.newsResults.Count > 0)
            {
                SearchNewsRequest _SearchNewsRequest = new SearchNewsRequest();
                _SearchNewsRequest.SearchTerm = res.OriginalRequest.SearchTerm;
                _SearchNewsRequest.IDs = res.newsResults.Select(h => h.IQSeqID).ToList();
                _SearchNewsRequest.PageSize = res.OriginalRequest.PageSize;

                _SearchNewsRequest.IsOutRequest = true;
                _SearchNewsRequest.IsHilightInLeadParagraph = false;
                _SearchNewsRequest.LeadParagraphChars = p_LeadParagraphChars;
                SearchNewsResults _res = SearchNews(_SearchNewsRequest, timeOutPeriod, true);

                foreach (NewsResult newsResult in res.newsResults)
                {
                    CommonFunction.LogInfo("Try getting paragraph for IQSeqID : " + newsResult.IQSeqID);
                    var highlightResult = _res.newsResults.Where(h => h.IQSeqID.Equals(newsResult.IQSeqID)).FirstOrDefault();

                    if (highlightResult != null && highlightResult.Highlights != null && highlightResult.Highlights.Count() > 0)
                    {
                        newsResult.IsLeadParagraph = true;
                    }
                    else
                    {
                        newsResult.IsLeadParagraph = false;
                    }

                    if (highlightResult!=null)
                    {
                        newsResult.IsSearchTermInHeadline = highlightResult.IsSearchTermInHeadline;
                    }
                }
            }
        }

        private void CheckPQHighlightInLeadParagraph(SearchProQuestResult res, int leadParagraphChars, Int32? timeOutPeriod = null)
        {
            if (res.ProQuestResults.Count > 0)
            {
                SearchProQuestRequest _SearchProQuestRequest = new SearchProQuestRequest();
                _SearchProQuestRequest.SearchTerm = res.OriginalRequest.SearchTerm;
                _SearchProQuestRequest.IDs = res.ProQuestResults.Select(h => h.IQSeqID.ToString()).ToList();
                _SearchProQuestRequest.PageSize = res.OriginalRequest.PageSize;

                _SearchProQuestRequest.IsOutRequest = true;
                _SearchProQuestRequest.IsHighlightInLeadParagraph = false;
                _SearchProQuestRequest.LeadParagraphChars = leadParagraphChars;

                bool isError;
                SearchProQuestResult _res = SearchProQuest(_SearchProQuestRequest, false, out isError, timeOutPeriod, true);

                foreach (ProQuestResult proQuestResult in res.ProQuestResults)
                {
                    CommonFunction.LogInfo("Try getting paragraph for IQSeqID : " + proQuestResult.IQSeqID);
                    var highlightResult = _res.ProQuestResults.Where(h => h.IQSeqID.Equals(proQuestResult.IQSeqID)).FirstOrDefault();

                    if (highlightResult != null && highlightResult.Highlights != null && highlightResult.Highlights.Count() > 0)
                    {
                        proQuestResult.IsLeadParagraph = true;
                    }
                    else
                    {
                        proQuestResult.IsLeadParagraph = false;
                    }

                    if (highlightResult != null)
                    {
                        proQuestResult.IsSearchTermInHeadline = highlightResult.IsSearchTermInHeadline;
                    }
                }
            }
        }

        private static void parseParagraphSearch(XmlDocument doc, SearchNewsResults res, string highlightFieldName, string p_highlightTitleFieldName)
        {
            //Dictionary<string, List<string>> _MapIDToListOfHighlight = new Dictionary<string, List<string>>();

            var mapIDToListOfHighlight = (from XmlNode OccNode in doc.SelectNodes("/response/lst[@name='highlighting']/lst")
                                          select new
                                          {
                                              ID = OccNode.Attributes["name"].Value,
                                              ListOfHighlight = (from XmlNode node in OccNode.SelectNodes("arr[@name='" + highlightFieldName + "']/str") select node.InnerText).ToList(),
                                              HeadlineHighlight = (from XmlNode node in OccNode.SelectNodes("arr[@name='" + p_highlightTitleFieldName + "']/str") select node.InnerText)
                                          }).Where(a => a.ListOfHighlight != null && a.ListOfHighlight.Count > 0);

            List<NewsResult> _NewsResults = new List<NewsResult>();

            foreach (var item in mapIDToListOfHighlight)
            {
                NewsResult _NewsResult = new NewsResult();
                _NewsResult.IQSeqID = item.ID;
                _NewsResult.Highlights = item.ListOfHighlight;
                _NewsResult.IsSearchTermInHeadline = item.HeadlineHighlight.Count()>0 ? true : false;
                _NewsResults.Add(_NewsResult);
            }

            res.newsResults = _NewsResults;
        }

        private static void parsePQParagraphSearch(XmlDocument doc, SearchProQuestResult res, string highlightFieldName, string highlightTitleFieldName)
        {
            var mapIDToListOfHighlight = (from XmlNode OccNode in doc.SelectNodes("/response/lst[@name='highlighting']/lst")
                                          select new
                                          {
                                              ID = OccNode.Attributes["name"].Value,
                                              ListOfHighlight = (from XmlNode node in OccNode.SelectNodes("arr[@name='" + highlightFieldName + "']/str") select node.InnerText).ToList(),
                                              HeadlineHighlight = (from XmlNode node in OccNode.SelectNodes("arr[@name='" + highlightTitleFieldName + "']/str") select node.InnerText)
                                          }).Where(a => a.ListOfHighlight != null && a.ListOfHighlight.Count > 0);

            List<ProQuestResult> _ProQuestResults = new List<ProQuestResult>();

            foreach (var item in mapIDToListOfHighlight)
            {
                ProQuestResult _ProQuestResult = new ProQuestResult();
                _ProQuestResult.IQSeqID = Convert.ToInt64(item.ID);
                _ProQuestResult.Highlights = item.ListOfHighlight;
                _ProQuestResult.IsSearchTermInHeadline = item.HeadlineHighlight.Count() > 0 ? true : false;
                _ProQuestResults.Add(_ProQuestResult);
            }

            res.ProQuestResults = _ProQuestResults;
        }

        public SearchSMResult SearchSocialMedia(SearchSMRequest request, Int32? timeOutPeriod = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SearchSMResult res = new SearchSMResult();

            try
            {
                CommonFunction.LogInfo("PMG Social Media Call Start", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Create Request for Social Media", request.IsPmgLogging, request.PmgLogFileLocation);


                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q qeury parameter of solr and 
                // 'FQuery' we will pass in the fq qeury parameter of solr 
                StringBuilder Query = new StringBuilder();
                StringBuilder FQuery = new StringBuilder();

                /* Commented below condition as Now we are passing pagenumber based on 0 */
                /*if (request.PageNumber <= 0)
                {
                    request.PageNumber = 1;
                }*/

                // start and rows return the required page. No. data.
                if (request.Start != null)
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.Start), System.Globalization.CultureInfo.CurrentCulture)));
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                }

                vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string Fl = string.Empty;
                    string FlTitleOnSearchTerm = string.Empty;
                    string RequestTerm = request.SearchTerm.Trim();

                    if (RequestTerm.EndsWith("#"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            Fl = "content_gen";
                            FlTitleOnSearchTerm = "title_gen";
                        }
                        else
                        {
                            Fl = "content";
                            FlTitleOnSearchTerm = "title";
                        }
                    }
                    else
                    {
                        Fl = "content_gen";
                        FlTitleOnSearchTerm = "title_gen";
                    }

                    // New Note : we'll change the search term to lower case , but doing this
                    // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                    List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                    RequestTerm = Regex.Replace(
                                RequestTerm,
                                @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                        //@"([\""][\w ]+[\""])|(\w+)",
                                m => reservedWords.Contains(m.Value) ? m.Value : m.Value
                                );

                    if (request.IsTitleNContentSearch)
                    {
                        Query = Query.AppendFormat("({0}:({1}) OR {2}:({1}))", Fl, RequestTerm, FlTitleOnSearchTerm);
                    }
                    else
                    {
                        Query = Query.AppendFormat("{0}:({1})", Fl, RequestTerm);
                    }


                    if (request.IsSentiment || request.IsReturnHighlight)
                    {
                        // all these feilds are for highlighting functionality
                        // hl.fl =  name of the feild on which need to provide highlighting
                        // hl = value can be on/off , if on then highlighting feature is enabled otherwise disabled.
                        // hl.maxAnalyzedChars =  default max char length for highlight is 51200 , but we need unlimited
                        vars.Add(new KeyValuePair<string, string>("hl.fl", Fl));
                        vars.Add(new KeyValuePair<string, string>("hl.requireFieldMatch", "true"));
                        vars.Add(new KeyValuePair<string, string>("hl", "on"));
                        vars.Add(new KeyValuePair<string, string>("hl.maxAnalyzedChars", "-1"));


                        // by setting it to 225 ,it will return no. of highlights 
                        // fragment size for signle highlight is 145 (approx)
                        if (request.FragSize.HasValue)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", request.FragSize.Value.ToString()));
                        else if (request.IsSentiment)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSizeForSentiment"]));
                        else
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSize"]));
                        vars.Add(new KeyValuePair<string, string>("hl.snippets", "99"));
                    }
                }


                // if SocialMediaSources are passed , then lets search on source
                if (request.SocialMediaSources != null && request.SocialMediaSources.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool isFirstSource = true;
                    string tempSource = "";

                    foreach (string source in request.SocialMediaSources)
                    {
                        if (!String.IsNullOrWhiteSpace(source))
                        {
                            tempSource = source;

                            if (!tempSource.Contains("*"))
                            {
                                if (!tempSource.StartsWith("\""))
                                    tempSource = "\"" + tempSource;
                                if (!tempSource.EndsWith("\""))
                                    tempSource += "\"";
                            }

                            if (isFirstSource)
                            {
                                isFirstSource = false;
                                FQuery = FQuery.AppendFormat(" homeurl_domain:{0}", tempSource);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR homeurl_domain:{0}", tempSource);
                            }
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if Author is passed , then lets search on author
                if (!string.IsNullOrEmpty(request.Author))
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" authorname:({0})", request.Author);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND authorname:({0})", request.Author);
                    }
                }

                // if title is passed , then lets search on title
                if (!string.IsNullOrEmpty(request.Title))
                {
                    string FlTitle120 = string.Empty;
                    string RequestTerm = string.Empty;
                    RequestTerm = request.Title.Trim();

                    // if our search term starts with char '#' then
                    // we understand that user wants exact search without sysnonym.
                    // e.g. 'Find' , will only find terms with 'Find' and skip terms like 'Finding', 'Found' , ect...
                    // we added that if search term is fuzzy , then we do make search in only CCgen and not CC

                    if (RequestTerm.EndsWith("#"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            FlTitle120 = "title_gen";
                        }
                        else
                        {
                            FlTitle120 = "title";
                        }
                    }
                    else
                    {
                        FlTitle120 = "title_gen";
                    }

                    // New Note : we'll change the search term to lower case , but doing this
                    // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                    List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                    RequestTerm = Regex.Replace(
                                RequestTerm,
                                @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                        //@"([\""][\w ]+[\""])|(\w+)",
                                m => reservedWords.Contains(m.Value) ? m.Value : m.Value);

                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" {0}:({1})", FlTitle120, RequestTerm);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND {0}:({1})", FlTitle120, RequestTerm);
                    }
                }

                // Exclude Domains
                if (request.ExcludeDomains != null && request.ExcludeDomains.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" NOT (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND NOT (");
                    }

                    bool IsFirstExcludeDomain = true;
                    string tempDomain = "";

                    foreach (string domain in request.ExcludeDomains)
                    {
                        if (!String.IsNullOrWhiteSpace(domain))
                        {
                            tempDomain = domain;

                            if (!tempDomain.Contains("*"))
                            {
                                if (!tempDomain.StartsWith("\""))
                                    tempDomain = "\"" + tempDomain;
                                if (!tempDomain.EndsWith("\""))
                                    tempDomain += "\"";
                            }

                            if (IsFirstExcludeDomain)
                            {
                                IsFirstExcludeDomain = false;
                                FQuery = FQuery.AppendFormat(" homeurl_domain:{0}", tempDomain);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR homeurl_domain:{0}", tempDomain);
                            }
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if Source Rank is passed , then lets search on Source Rank
                if (request.SourceRank != null && request.SourceRank.Count() > 0)
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" (");
                    }
                    else
                    {
                        Query = Query.Append(" AND (");
                    }

                    bool IsFirst = true;

                    foreach (string _sourcerank in request.SourceRank)
                    {
                        if (IsFirst)
                        {
                            IsFirst = false;
                            Query = Query.AppendFormat(" autorank:{0}", _sourcerank);
                        }
                        else
                        {
                            Query = Query.AppendFormat(" OR autorank:{0}", _sourcerank);
                        }
                    }

                    Query = Query.Append(" )");
                }

                // if ID(s) are passed , then lets make search for ID(s)
                if (request.ids != null && request.ids.Count() > 0)
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" (");
                    }
                    else
                    {
                        Query = Query.Append(" AND (");
                    }

                    bool IsFirst = true;

                    foreach (string _id in request.ids)
                    {
                        if (IsFirst)
                        {
                            IsFirst = false;
                            Query = Query.AppendFormat(" iqseqid:{0}", _id);
                        }
                        else
                        {
                            Query = Query.AppendFormat(" OR iqseqid:{0}", _id);
                        }
                    }

                    Query = Query.Append(" )");
                }


                // ooops nothing passed for 'q' search of solr.
                // then as q is complesary search
                // if nothing is passed then we should pass *:*
                // which mean it return all without making search for q.
                // although after q , it will filter on fq (filter query) search. 
                if (string.IsNullOrEmpty(Query.ToString()))
                {
                    Query = Query.Append("*:*");
                }

                // if both start and enddate is passed then lets make search on RL_Station_DateTime.
                if (request.StartDate != null && request.EndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" harvestdate_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND harvestdate_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                request.ExcludedSourceType = request.ExcludedSourceType == null ? new List<string>() : request.ExcludedSourceType;
                request.ExcludedSourceType.Add(GetEnumDescription(SourceType.OnlineNews));
                request.ExcludedSourceType.Add(GetEnumDescription(SourceType.Print));
                if (request.SourceType == null || !request.SourceType.Contains(GetEnumDescription(SourceType.Comment)))
                {
                    request.ExcludedSourceType.Add(GetEnumDescription(SourceType.Comment));
                }

                // if SourceType are passed , then lets make search for feedClass
                if (request.SourceType != null && request.SourceType.Count() > 0)
                {

                    bool IsFirstHour = true;

                    foreach (string srcType in request.SourceType)
                    {
                        SourceType iqsubtype = GetValueFromDescription<SourceType>(srcType);
                        if (iqsubtype != SourceType.SocialMedia)
                        {
                            if (IsFirstHour)
                            {
                                IsFirstHour = false;

                                if (string.IsNullOrEmpty(FQuery.ToString()))
                                {
                                    FQuery = FQuery.Append(" (");
                                }
                                else
                                {
                                    FQuery = FQuery.Append(" AND (");
                                }

                                FQuery = FQuery.AppendFormat(" iqsubmediatype:{0}", (int)iqsubtype);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR iqsubmediatype:{0}", (int)iqsubtype);
                            }

                            if (iqsubtype == SourceType.Forum)
                            {
                                FQuery = FQuery.AppendFormat(" OR iqsubmediatype:{0}", (int)SourceType.Review);
                            }
                        }
                    }

                    if (request.SourceType.Contains(GetEnumDescription(SourceType.SocialMedia)))
                    {
                        request.ExcludedSourceType = request.ExcludedSourceType == null ? new List<string>() : request.ExcludedSourceType;
                        if (!request.SourceType.Contains(GetEnumDescription(SourceType.Blog)))
                        {
                            request.ExcludedSourceType.Add(GetEnumDescription(SourceType.Blog));
                        }

                        if (!request.SourceType.Contains(GetEnumDescription(SourceType.Forum)))
                        {
                            request.ExcludedSourceType.Add(GetEnumDescription(SourceType.Forum));
                            request.ExcludedSourceType.Add(GetEnumDescription(SourceType.Review));
                        }
                    }

                    if (!IsFirstHour)
                    {
                        FQuery = FQuery.Append(" )");
                    }
                }

                if (request.ExcludedSourceType != null && request.ExcludedSourceType.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" NOT (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND NOT (");
                    }

                    bool IsFirstHour = true;

                    foreach (string srcType in request.ExcludedSourceType)
                    {
                        SourceType iqsubtype = GetValueFromDescription<SourceType>(srcType);
                        if (IsFirstHour)
                        {
                            IsFirstHour = false;
                            FQuery = FQuery.AppendFormat(" iqsubmediatype:{0}", (int)iqsubtype);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR iqsubmediatype:{0}", (int)iqsubtype);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if FromRecordID is not null then get data after this recordID
                if (request.FromRecordID != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    FQuery = FQuery.AppendFormat(" iqseqid:[* TO {0}])", request.FromRecordID);
                }


                // our q query is ready!!! 
                // lets add it to keyvalue pair for q,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                // as fq is not compelsary search 
                // we will pass it only if there is search criteria on fq 
                if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                {
                    vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                }

                string sortFields = string.Empty;

                // lets make solr fields and pass to solr
                if (!string.IsNullOrEmpty(request.SortFields))
                {
                    sortFields = GenerateSMSortField(request.SortFields);
                }

                if (request.SortFields != null)
                {
                    vars.Add(new KeyValuePair<string, string>("sort", sortFields));
                }


                // to make search effective and to reduce search time ,
                // we should only retrun the fields which are required
                // so lets add fields that we need to return , this is config driven,
                // to add any field , we just need to change in config :)
                if (request.isShowContent)
                    //vars.Add(new KeyValuePair<string, string>("fl", "content"));
                    vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["SolrSMContentFL"]));
                else
                    vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["solrSMFL"]));


                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                string RequestURL = string.Empty;
                if (request.IsOutRequest)
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                    res.RequestUrl = RequestURL;
                }
                else
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                }

                CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Solr Response - TimeTaken - for Social Media get response" + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                XmlDocument xDoc = new XmlDocument();

                // lets load solr response to xml so we can get data in xmk format
                xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                res.OriginalRequest = request;

                CommonFunction.LogInfo("Social Media Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);

                // lets parse response. to our 'SearchResult' object
                parseSMResponse(xDoc, res);

                sw.Stop();

                CommonFunction.LogInfo("Solr Response - TimeTaken - for social Media parse response " + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo(string.Format("Total Results for Social Media Count :{0}", res.TotalResults), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("PMG Social Media Call End", request.IsPmgLogging, request.PmgLogFileLocation);

                return res;
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        private static void parseSMResponse(XmlDocument doc, SearchSMResult res, bool isGet = false)
        {
            try
            {
                XDocument xDoc = XDocument.Parse(Convert.ToString(doc.InnerXml));

                res.smResults = new List<SMResult>();

                int totalRecords = Convert.ToInt32((from p in xDoc.Descendants("response").Descendants("result")
                                                    select p.Attribute("numFound").Value).FirstOrDefault());
                res.TotalResults = totalRecords;

                XElement occurenceNodes = (from p in xDoc.Descendants("response").Descendants("lst")
                                           where p.Attribute("name").Value == "highlighting"
                                           select p).FirstOrDefault();

                List<XElement> docNodes = xDoc.Descendants("response").Descendants("result").Descendants("doc").ToList();
                if ((docNodes == null || docNodes.Count == 0) && isGet)
                {
                    // If performing a get request and only one record is returned, there is no result node
                    docNodes = xDoc.Descendants("response").Descendants("doc").ToList();
                }

                foreach (XElement elem in docNodes)
                {
                    SMResult smResult = new SMResult();
                    foreach (XElement childelem in elem.Elements())
                    {

                        if (childelem.Attribute("name").Value == "homeurl_domain")
                        {
                            smResult.HomeurlDomain = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "title")
                        {
                            smResult.description = childelem.Value;
                            //smResult.date = Convert.ToDateTime(childelem.Value).ToUniversalTime().ToString("MM/dd/yyyy hh:mm tt");
                        }
                        else if (childelem.Attribute("name").Value == "harvestdate_dt")
                        {
                            smResult.itemHarvestDate_DT = Convert.ToDateTime(childelem.Value).ToUniversalTime().ToString();
                        }
                        else if (childelem.Attribute("name").Value == "mediatype")
                        {
                            smResult.feedClass = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "autorank")
                        {
                            smResult.feedRank = string.IsNullOrEmpty(childelem.Value) ? 0 : Convert.ToInt32(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "url")
                        {
                            smResult.link = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "content")
                        {
                            smResult.content = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "iqseqid")
                        {
                            smResult.IQSeqID = childelem.Value;

                            if (res.OriginalRequest.IsReturnHighlight && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm))
                            {
                                var Occurances = (from c in occurenceNodes.Descendants("lst")
                                                  where c.Attribute("name").Value == childelem.Value
                                                  select c);
                                smResult.Highlights = Occurances.Descendants("str").Select(a => a.Value).ToList();

                                smResult.Mentions = Occurances != null ? Occurances.Descendants("str").Count() : 0;
                            }
                        }
                        else if (childelem.Attribute("name").Value == "lastmoreoverid")
                        {
                            smResult.LastMoreoverID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "id")
                        {
                            smResult.id = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "seqid")
                        {
                            smResult.SeqID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "source")
                        {
                            smResult.Source = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "source_category")
                        {
                            smResult.SourceCategory = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "genre")
                        {
                            smResult.Genre = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "dataformat")
                        {
                            smResult.DataFormat = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "homeurl")
                        {
                            smResult.homeLink = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "duplicategroupid")
                        {
                            smResult.DuplicateGroupID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "editorialrank" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            smResult.EditorialRank = Convert.ToInt32(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "region")
                        {
                            smResult.Region = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "subregion")
                        {
                            smResult.SubRegion = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "country")
                        {
                            smResult.Country = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "country_code")
                        {
                            smResult.CountryCode = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "zipcode")
                        {
                            smResult.ZipCode = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "language")
                        {
                            smResult.Language = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "sourcefeedlanguage")
                        {
                            smResult.SourceFeedLanguage = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "publisheddate" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            smResult.PublishedDate = Convert.ToDateTime(childelem.Value).ToUniversalTime();
                        }
                        else if (childelem.Attribute("name").Value == "inwhitelist" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            smResult.InWhiteList = Convert.ToBoolean(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "inboundlinkcount" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            smResult.InBoundLinkCount = Convert.ToInt32(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "autorankorder" && !string.IsNullOrWhiteSpace(childelem.Value))
                        {
                            smResult.AutoRankOrder = Convert.ToInt32(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "authorname")
                        {
                            smResult.AuthorName = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "topicname")
                        {
                            smResult.TopicNames = childelem.Descendants("str").ToList().Select(x => x.Value.ToString()).ToList();
                        }
                        else if (childelem.Attribute("name").Value == "topicgroup")
                        {
                            smResult.TopicGroups = childelem.Descendants("str").ToList().Select(x => x.Value.ToString()).ToList();
                        }
                        else if (childelem.Attribute("name").Value == "ziparea")
                        {
                            smResult.ZipArea = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "state")
                        {
                            smResult.State = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "companysymbol")
                        {
                            smResult.CompanySymbol = childelem.Descendants("str").ToList().Select(x => x.Value.ToString()).ToList();
                        }
                        else if (childelem.Attribute("name").Value == "companyexchange")
                        {
                            smResult.CompanyExchange = childelem.Descendants("str").ToList().Select(x => x.Value.ToString()).ToList();
                        }
                        else if (childelem.Attribute("name").Value == "iqmediatype")
                        {
                            smResult.IQMediaType = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "iqsubmediatype")
                        {
                            smResult.IQSubMediaType = Convert.ToInt32(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "iqdmaname")
                        {
                            smResult.IQDmaName = childelem.Value;
                        }

                    }
                    res.smResults.Add(smResult);
                }

                if (res.OriginalRequest.IsSentiment && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm) && res.OriginalRequest.LowThreshold != null && res.OriginalRequest.HighThreshold != null)
                {
                    Dictionary<string, List<string>> _MapIDToListOfHighlight = new Dictionary<string, List<string>>();

                    _MapIDToListOfHighlight = (from XmlNode OccNode in doc.SelectNodes("/response/lst[@name='highlighting']/lst")
                                               select new
                                               {
                                                   ID = OccNode.Attributes["name"].Value,
                                                   ListOfHighlight = (from XmlNode node in OccNode.SelectNodes("arr/str") select node.InnerText).ToList()
                                               }).ToDictionary(a => a.ID, a => a.ListOfHighlight);

                    SentimentLogic _SentimentLogic = new SentimentLogic();
                    Dictionary<string, Sentiments> _IDToSentimentsMap = _SentimentLogic.GetSentiment(_MapIDToListOfHighlight, res.OriginalRequest.LowThreshold.Value, res.OriginalRequest.HighThreshold.Value, res.OriginalRequest.ClientGuid);

                    foreach (SMResult smResult in res.smResults)
                    {
                        Sentiments _Sentiments = new Sentiments();
                        if (_IDToSentimentsMap.TryGetValue(smResult.IQSeqID, out _Sentiments))
                        {
                            smResult.Sentiments = _Sentiments;

                            foreach (SubSentiment ss in _Sentiments.HighlightToWeightMap)
                            {
                                CommonFunction.LogInfo("Sentiment : => " + ss.HighlightingText + " ", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                                CommonFunction.LogInfo("Sentiment Weight : => " + Convert.ToString(ss.Weight) + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            }
                            CommonFunction.LogInfo("Positive Sentiment ID : " + smResult.IQSeqID + " => " + _Sentiments.PositiveSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            CommonFunction.LogInfo("Negative Sentiment ID : " + smResult.IQSeqID + " => " + _Sentiments.NegativeSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }


        }

        private static string GenerateSMSortField(string sortfields)
        {
            try
            {
                IDictionary<string, string> PMGSMSearchSortFields = new Dictionary<string, string>();


                PMGSMSearchSortFields.Add("date", "harvestdate_dt asc");
                PMGSMSearchSortFields.Add("date-", "harvestdate_dt desc");

                PMGSMSearchSortFields.Add("title", "title asc");
                PMGSMSearchSortFields.Add("title-", "title desc");


                StringBuilder InputSortFields = new StringBuilder();

                string[] PMGSMSearchSortField = sortfields.Split(',');

                // max solr solr field is config driven , so we only can search on max that. no of fields
                int MaxNoOfSortFields = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxSortField"], System.Globalization.CultureInfo.CurrentCulture);
                int index = 0;

                foreach (string SortField in PMGSMSearchSortField)
                {
                    if (PMGSMSearchSortFields.ContainsKey(SortField.ToLower(System.Globalization.CultureInfo.CurrentCulture)))
                    {
                        InputSortFields.Append(PMGSMSearchSortFields[SortField.ToLower(System.Globalization.CultureInfo.CurrentCulture)] + ",");
                    }

                    index = index + 1;

                    if (index >= MaxNoOfSortFields)
                    {
                        break;
                    }
                }

                if (InputSortFields.Length > 0)
                {
                    InputSortFields.Remove(InputSortFields.Length - 1, 1);
                }

                return InputSortFields.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string SearchSocialMediaChart(SearchSMRequest request, out Boolean isError, Int32? timeOutPeriod = null)
        {
            isError = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SearchSMResult res = new SearchSMResult();
            string responseString = string.Empty;
            try
            {


                CommonFunction.LogInfo("PMG Call for Social Media Start", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Create Request for Social Media Start", request.IsPmgLogging, request.PmgLogFileLocation);


                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q qeury parameter of solr and 
                // 'FQuery' we will pass in the fq qeury parameter of solr 
                StringBuilder Query = new StringBuilder();
                StringBuilder FQuery = new StringBuilder();

                /* Commented below condition as Now we are passing pagenumber based on 0 */
                /*if (request.PageNumber <= 0)
                {
                    request.PageNumber = 1;
                }*/

                // start and rows return the required page. No. data.
                vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {

                    string Fl = string.Empty;
                    string FlTitleOnSearchTerm = string.Empty;
                    string RequestTerm = request.SearchTerm.Trim();

                    if (RequestTerm.EndsWith("#"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            Fl = "content_gen";
                            FlTitleOnSearchTerm = "title_gen";
                        }
                        else
                        {
                            Fl = "content";
                            FlTitleOnSearchTerm = "title";
                        }
                    }
                    else
                    {
                        Fl = "content_gen";
                        FlTitleOnSearchTerm = "title_gen";
                    }

                    // New Note : we'll change the search term to lower case , but doing this
                    // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                    List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                    RequestTerm = Regex.Replace(
                                RequestTerm,
                                @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                        //@"([\""][\w ]+[\""])|(\w+)",
                                m => reservedWords.Contains(m.Value) ? m.Value : m.Value
                                );

                    if (request.IsTitleNContentSearch)
                    {
                        Query = Query.AppendFormat("({0}:({1}) OR {2}:({1}))", Fl, RequestTerm, FlTitleOnSearchTerm);
                    }
                    else
                    {
                        Query = Query.AppendFormat("{0}:({1})", Fl, RequestTerm);
                    }

                }
                vars.Add(new KeyValuePair<string, string>("hl", "off"));



                if (request.Facet)
                {
                    vars.Add(new KeyValuePair<string, string>("facet", "on"));

                    //FQuery = FQuery.Append("&facet=on");
                    if (!string.IsNullOrEmpty(request.FacetRangeOther))
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.other", request.FacetRangeOther));
                        //FQuery = FQuery.Append("&facet.range.other=" + request.FacetRangeOther);
                    }

                    if (!string.IsNullOrWhiteSpace(request.FacetRange))
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range", request.FacetRange));
                    }

                    if (!string.IsNullOrWhiteSpace(request.FacetField))
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.field", request.FacetField));
                        vars.Add(new KeyValuePair<string, string>("facet.limit", "-1"));
                    }

                    if (request.FacetRangeStarts != null && request.FacetRangeEnds != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.start", request.FacetRangeStarts.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
                        vars.Add(new KeyValuePair<string, string>("facet.range.end", request.FacetRangeEnds.Value.AddSeconds(1).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
                        /*FQuery = FQuery.Append("&facet.range.start=" + request.FacetRangeStarts.Value.ToString().Substring(0, 10) + "T" + request.FacetRangeStarts.Value.Hour + ":" + request.FacetRangeStarts.Value.Minute + ":" + request.FacetRangeStarts.Value.Second + "Z");
                        FQuery = FQuery.Append("&facet.range.end=" + request.FacetRangeEnds.Value.ToString().Substring(0, 10) + "T" + request.FacetRangeEnds.Value.Hour + ":" + request.FacetRangeEnds.Value.Minute + ":" + request.FacetRangeEnds.Value.Second + "Z");*/
                    }

                    if (request.FacetRangeGap != null && request.FacetRangeGapDuration != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.gap", "+" + request.FacetRangeGapDuration + request.FacetRangeGap));
                        //FQuery = FQuery.Append("&facet.range.gap=%2B" + request.FacetRangeGapDuration + request.FacetRangeGap);
                    }
                    if (request.wt != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("wt", request.wt.ToString()));
                    }
                }


                // if SocialMediaSources are passed , then lets search on source
                if (request.SocialMediaSources != null && request.SocialMediaSources.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool isFirstSource = true;
                    string tempSource = "";

                    foreach (string source in request.SocialMediaSources)
                    {
                        if (!String.IsNullOrWhiteSpace(source))
                        {
                            tempSource = source;

                            if (!tempSource.Contains("*"))
                            {
                                if (!tempSource.StartsWith("\""))
                                    tempSource = "\"" + tempSource;
                                if (!tempSource.EndsWith("\""))
                                    tempSource += "\"";
                            }

                            if (isFirstSource)
                            {
                                isFirstSource = false;
                                FQuery = FQuery.AppendFormat(" homeurl_domain:{0}", tempSource);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR homeurl_domain:{0}", tempSource);
                            }
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if Author is passed , then lets search on author
                if (!string.IsNullOrEmpty(request.Author))
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" authorname:({0})", request.Author);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND authorname:({0})", request.Author);
                    }
                }

                // if title is passed , then lets search on title
                if (!string.IsNullOrEmpty(request.Title))
                {
                    string FlTitle120 = string.Empty;
                    string RequestTerm = string.Empty;
                    RequestTerm = request.Title.Trim();

                    // if our search term starts with char '#' then
                    // we understand that user wants exact search without sysnonym.
                    // e.g. 'Find' , will only find terms with 'Find' and skip terms like 'Finding', 'Found' , ect...
                    // we added that if search term is fuzzy , then we do make search in only CCgen and not CC

                    if (RequestTerm.EndsWith("#"))
                    {
                        RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                        if (Regex.IsMatch(RequestTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            RequestTerm = RequestTerm.EndsWith("#") ? RequestTerm.Remove(RequestTerm.Length - 1, 1) : RequestTerm;
                            FlTitle120 = "title_gen";
                        }
                        else
                        {
                            FlTitle120 = "title";
                        }
                    }
                    else
                    {
                        FlTitle120 = "title_gen";
                    }

                    // New Note : we'll change the search term to lower case , but doing this
                    // we have to preserve the boolean keywords "AND" , "OR" , "NOT" in upper case, otherwise they loose their boolean operation
                    List<string> reservedWords = new List<string>() { "AND", "OR", "NOT" };
                    RequestTerm = Regex.Replace(
                                RequestTerm,
                                @"([\""](.*?)[\w(.*?) ]+[\""](~\d+)?)|(\w+)",
                        //@"([\""][\w ]+[\""])|(\w+)",
                                m => reservedWords.Contains(m.Value) ? m.Value : m.Value);

                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" {0}:({1})", FlTitle120, RequestTerm);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND {0}:({1})", FlTitle120, RequestTerm);
                    }
                }

                // Exclude Domains
                if (request.ExcludeDomains != null && request.ExcludeDomains.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" NOT (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND NOT (");
                    }

                    bool IsFirstExcludeDomain = true;
                    string tempDomain = "";

                    foreach (string domain in request.ExcludeDomains)
                    {
                        if (!String.IsNullOrWhiteSpace(domain))
                        {
                            tempDomain = domain;

                            if (!tempDomain.Contains("*"))
                            {
                                if (!tempDomain.StartsWith("\""))
                                    tempDomain = "\"" + tempDomain;
                                if (!tempDomain.EndsWith("\""))
                                    tempDomain += "\"";
                            }

                            if (IsFirstExcludeDomain)
                            {
                                IsFirstExcludeDomain = false;
                                FQuery = FQuery.AppendFormat(" homeurl_domain:{0}", tempDomain);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR homeurl_domain:{0}", tempDomain);
                            }
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if Source Rank is passed , then lets search on Source Rank
                if (request.SourceRank != null && request.SourceRank.Count() > 0)
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" (");
                    }
                    else
                    {
                        Query = Query.Append(" AND (");
                    }

                    bool IsFirst = true;

                    foreach (string _sourcerank in request.SourceRank)
                    {
                        if (IsFirst)
                        {
                            IsFirst = false;
                            Query = Query.AppendFormat(" autorank:{0}", _sourcerank);
                        }
                        else
                        {
                            Query = Query.AppendFormat(" OR autorank:{0}", _sourcerank);
                        }
                    }

                    Query = Query.Append(" )");
                }


                // ooops nothing passed for 'q' search of solr.
                // then as q is complesary search
                // if nothing is passed then we should pass *:*
                // which mean it return all without making search for q.
                // although after q , it will filter on fq (filter query) search. 
                if (string.IsNullOrEmpty(Query.ToString()))
                {
                    Query = Query.Append("*:*");
                }

                string sortFields = string.Empty;

                // lets make solr fields and pass to solr
                if (!string.IsNullOrEmpty(request.SortFields))
                {
                    sortFields = GenerateSMSortField(request.SortFields);
                }

                if (request.SortFields != null)
                {
                    vars.Add(new KeyValuePair<string, string>("sort", sortFields));
                }

                // if both start and enddate is passed then lets make search on RL_Station_DateTime.
                if (request.StartDate != null && request.EndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" harvestdate_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND harvestdate_dt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                // if FromRecordID is not null then get data after this recordID
                if (request.FromRecordID != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    FQuery = FQuery.AppendFormat(" iqseqid:[* TO {0}])", request.FromRecordID);
                }

                request.ExcludedSourceType = request.ExcludedSourceType == null ? new List<string>() : request.ExcludedSourceType;
                request.ExcludedSourceType.Add(GetEnumDescription(SourceType.OnlineNews));
                request.ExcludedSourceType.Add(GetEnumDescription(SourceType.Print));
                if (request.SourceType == null || !request.SourceType.Contains(GetEnumDescription(SourceType.Comment)))
                {
                    request.ExcludedSourceType.Add(GetEnumDescription(SourceType.Comment));
                }

                if (request.IsTaggingExcluded)
                {
                    // if SourceType are passed , then lets make search for feedClass
                    if (request.SourceType != null && request.SourceType.Count() > 0)
                    {

                        bool IsFirstHour = true;

                        foreach (string srcType in request.SourceType)
                        {
                            SourceType iqsubtype = GetValueFromDescription<SourceType>(srcType);
                            if (iqsubtype != SourceType.SocialMedia)
                            {
                                if (IsFirstHour)
                                {
                                    IsFirstHour = false;

                                    if (string.IsNullOrEmpty(FQuery.ToString()))
                                    {
                                        FQuery = FQuery.Append(" (");
                                    }
                                    else
                                    {
                                        FQuery = FQuery.Append(" AND (");
                                    }

                                    FQuery = FQuery.AppendFormat(" iqsubmediatype:{0}", (int)iqsubtype);
                                }
                                else
                                {
                                    FQuery = FQuery.AppendFormat(" OR iqsubmediatype:{0}", (int)iqsubtype);
                                }

                                if (iqsubtype == SourceType.Forum)
                                {
                                    FQuery = FQuery.AppendFormat(" OR iqsubmediatype:{0}", (int)SourceType.Review);
                                }
                            }
                        }

                        if (request.SourceType.Contains(GetEnumDescription(SourceType.SocialMedia)))
                        {
                            request.ExcludedSourceType = request.ExcludedSourceType == null ? new List<string>() : request.ExcludedSourceType;
                            if (!request.SourceType.Contains(GetEnumDescription(SourceType.Blog)))
                            {
                                request.ExcludedSourceType.Add(GetEnumDescription(SourceType.Blog));
                            }

                            if (!request.SourceType.Contains(GetEnumDescription(SourceType.Forum)))
                            {
                                request.ExcludedSourceType.Add(GetEnumDescription(SourceType.Forum));
                                request.ExcludedSourceType.Add(GetEnumDescription(SourceType.Review));
                            }
                        }

                        if (!IsFirstHour)
                        {
                            FQuery = FQuery.Append(" )");
                        }

                    }

                    // if Excluded SourceType are passed , then lets make search for feedClass
                    if (request.ExcludedSourceType != null && request.ExcludedSourceType.Count() > 0)
                    {

                        if (string.IsNullOrEmpty(FQuery.ToString()))
                        {
                            FQuery = FQuery.Append(" NOT (");
                        }
                        else
                        {
                            FQuery = FQuery.Append(" AND NOT (");
                        }

                        bool IsFirstHour = true;

                        foreach (string srcType in request.ExcludedSourceType)
                        {
                            SourceType iqsubtype = GetValueFromDescription<SourceType>(srcType);
                            if (IsFirstHour)
                            {
                                IsFirstHour = false;
                                FQuery = FQuery.AppendFormat(" iqsubmediatype:{0}", (int)iqsubtype);
                            }
                            else
                            {
                                FQuery = FQuery.AppendFormat(" OR iqsubmediatype:{0}", (int)iqsubtype);
                            }
                        }

                        FQuery = FQuery.Append(" )");
                    }
                }
                else
                {
                    // if Publication Category(s) are passed , then lets make search for Source Type(s)
                    if (request.SourceType != null && request.SourceType.Count() > 0)
                    {
                        foreach (string srcType in request.SourceType)
                        {
                            SourceType iqsubtype = GetValueFromDescription<SourceType>(srcType);
                            //if (request.SourceType.Count <= 0 || request.SourceType.Contains(srcType))
                            //{
                            string csvList = string.Join(",", (from sType in request.SourceType
                                                               where sType != srcType
                                                               select (int)GetValueFromDescription<SourceType>(sType)).ToArray());
                            vars.Add(new KeyValuePair<string, string>("facet.range", "{!key=" + (int)iqsubtype + " ex=" + csvList + "}harvestdate_dt"));
                            vars.Add(new KeyValuePair<string, string>("fq", "{!tag=" + (int)iqsubtype + "}iqsubmediatype:" + (int)iqsubtype + ""));
                            //}

                        }
                    }
                }

                // our q query is ready!!! 
                // lets add it to keyvalue pair for q,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                // as fq is not compelsary search 
                // we will pass it only if there is search criteria on fq 
                if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                {
                    vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                }

                // to make search effective and to reduce search time ,
                // we should only retrun the fields which are required
                // so lets add fields that we need to return , this is config driven,
                // to add any field , we just need to change in config :)
                vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["solrSMFL"]));

                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                string RequestURL = string.Empty;
                if (request.IsOutRequest)
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                    res.RequestUrl = RequestURL;
                }
                else
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                }

                //CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);

                //CommonFunction.LogInfo("Solr Response - TimeTaken - for get response" + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                res.OriginalRequest = request;

                sw.Stop();



                //CommonFunction.LogInfo(string.Format("Total Hti Count :{0}", res.TotalHitCount), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("PMG Call for Social Media Chart End", request.IsPmgLogging, request.PmgLogFileLocation);

                return res.ResponseXml;
            }
            catch (Exception _Exception)
            {
                /*CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);*/
                isError = true;
                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res.ResponseXml;
            }
        }

        public SearchTwitterResult SearchTwitter(SearchTwitterRequest request, bool isBindChart, out Boolean isError, Int32? timeOutPeriod = null)
        {
            SearchTwitterResult res = new SearchTwitterResult();
            isError = false;
            try
            {
                CommonFunction.LogInfo("PMG Twitter Call Start", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Create Request for Twitter ", request.IsPmgLogging, request.PmgLogFileLocation);


                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q qeury parameter of solr and 
                // 'FQuery' we will pass in the fq qeury parameter of solr 
                StringBuilder Query = new StringBuilder();
                StringBuilder FQuery = new StringBuilder();

                /* Commented below condition as Now we are passing pagenumber based on 0 */
                /*if (request.PageNumber <= 0)
                {
                    request.PageNumber = 1;
                }*/

                // start and rows return the required page. No. data.
                if (request.Start != null)
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.Start), System.Globalization.CultureInfo.CurrentCulture)));
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                }

                vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string Fl = string.Empty;
                    request.SearchTerm = request.SearchTerm.Trim();
                    if (request.SearchTerm.EndsWith("#") && !Regex.IsMatch(request.SearchTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                    {
                        Fl = "tweet_body";
                    }
                    else
                    {
                        Fl = "tweet_body_gen";
                    }

                    Query = Query.AppendFormat("{0}:({1})", Fl, request.SearchTerm.EndsWith("#") ? request.SearchTerm.Remove(request.SearchTerm.Length - 1, 1) : request.SearchTerm);

                    if (!isBindChart && (request.IsSentiment || request.IsHighlighting))
                    {
                        // all these feilds are for highlighting functionality
                        // hl.fl =  name of the feild on which need to provide highlighting
                        // hl = value can be on/off , if on then highlighting feature is enabled otherwise disabled.
                        // hl.maxAnalyzedChars =  default max char length for highlight is 51200 , but we need unlimited
                        vars.Add(new KeyValuePair<string, string>("hl.fl", Fl));
                        vars.Add(new KeyValuePair<string, string>("hl.requireFieldMatch", "true"));
                        vars.Add(new KeyValuePair<string, string>("hl", "on"));
                        vars.Add(new KeyValuePair<string, string>("hl.maxAnalyzedChars", "1000"));


                        // by setting it to 225 ,it will return no. of highlights 
                        // fragment size for signle highlight is 145 (approx)
                        if (request.FragSize.HasValue)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", request.FragSize.Value.ToString()));
                        else
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSizeForSentiment"]));
                        vars.Add(new KeyValuePair<string, string>("hl.snippets", "99"));
                    }
                }


                // if actor display name is passed , then lets search on actor display name
                if (!string.IsNullOrEmpty(request.ActorDisplayName))
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.AppendFormat(" (actor_displayname:({0}) OR actor_preferredusername:({0}))", request.ActorDisplayName);
                    }
                    else
                    {
                        Query = Query.AppendFormat(" AND (actor_displayname:({0}) OR actor_preferredusername:({0}))", request.ActorDisplayName);
                    }
                }

                // if ID(s) are passed , then lets make search for ID(s)
                if (request.IDs != null && request.IDs.Count() > 0)
                {
                    if (string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" (");
                    }
                    else
                    {
                        Query = Query.Append(" AND (");
                    }

                    bool IsFirst = true;

                    foreach (long _id in request.IDs)
                    {
                        if (IsFirst)
                        {
                            IsFirst = false;
                            Query = Query.AppendFormat(" iqseqid:{0}", _id);
                        }
                        else
                        {
                            Query = Query.AppendFormat(" OR iqseqid:{0}", _id);
                        }
                    }

                    Query = Query.Append(" )");
                }

                // Exclude Domains
                if (request.ExcludeHandles != null && request.ExcludeHandles.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" NOT (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND NOT (");
                    }

                    bool IsFirstExcludeDomain = true;

                    foreach (string handle in request.ExcludeHandles)
                    {
                        if (IsFirstExcludeDomain)
                        {
                            IsFirstExcludeDomain = false;
                            FQuery = FQuery.AppendFormat(" actor_preferredusername:{0}", handle);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR actor_preferredusername:{0}", handle);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // ooops nothing passed for 'q' search of solr.
                // then as q is complesary search
                // if nothing is passed then we should pass *:*
                // which mean it return all without making search for q.
                // although after q , it will filter on fq (filter query) search. 
                if (string.IsNullOrEmpty(Query.ToString()))
                {
                    Query = Query.Append("*:*");
                }

                // if both start and enddate is passed then lets make search on tweet_posteddatetime.
                if (request.StartDate != null && request.EndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" tweet_posteddatetime:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND tweet_posteddatetime:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                if (request.FriendsRangeFrom != null || request.FriendsRangeTo != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString())) FQuery = FQuery.Append(" AND");

                    FQuery = FQuery.AppendFormat(" actor_friendscount:[{0} TO {1}]", request.FriendsRangeFrom != null ? request.FriendsRangeFrom.ToString() : "*", request.FriendsRangeTo != null ? request.FriendsRangeTo.ToString() : "*");
                }

                if (request.FollowersRangeFrom != null || request.FollowersRangeTo != null)
                {
                    if (!string.IsNullOrEmpty(FQuery.ToString())) FQuery = FQuery.Append(" AND");

                    FQuery = FQuery.AppendFormat(" actor_followerscount:[{0} TO {1}]", request.FollowersRangeFrom != null ? request.FollowersRangeFrom.ToString() : "*", request.FollowersRangeTo != null ? request.FollowersRangeTo.ToString() : "*");
                }

                if (request.KloutRangeFrom != null || request.KloutRangeTo != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString())) FQuery = FQuery.Append(" AND");
                    
                    FQuery = FQuery.AppendFormat(" gnip_klout_score:[{0} TO {1}]", request.KloutRangeFrom != null ? request.KloutRangeFrom.ToString() : "*", request.KloutRangeTo != null ? request.KloutRangeTo.ToString() : "*");
                }

                if (request.gnip_tag != null && request.gnip_tag.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool IsFirst = true;

                    foreach (Guid _gnip_tag in request.gnip_tag)
                    {
                        if (IsFirst)
                        {
                            IsFirst = false;
                            FQuery = FQuery.AppendFormat(" gnip_tag:{0}", _gnip_tag);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR gnip_tag:{0}", _gnip_tag);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // if FromRecordID is not null then get data after this recordID
                if (request.FromRecordID != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    FQuery = FQuery.AppendFormat(" iqseqid:[* TO {0}])", request.FromRecordID);

                }

                if (request.IsDeleted != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }
                    
                    FQuery = FQuery.AppendFormat(" isdeleted:{0})", request.IsDeleted == true ? 1 : 0);
                }

                if (request.Facet && isBindChart)
                {
                    vars.Add(new KeyValuePair<string, string>("facet", "on"));

                    //FQuery = FQuery.Append("&facet=on");
                    if (!string.IsNullOrEmpty(request.FacetRangeOther))
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.other", request.FacetRangeOther));
                        //FQuery = FQuery.Append("&facet.range.other=" + request.FacetRangeOther);
                    }

                    if (!string.IsNullOrWhiteSpace(request.FacetRange))
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range", request.FacetRange));
                    }
                    if (request.FacetRangeStarts != null && request.FacetRangeEnds != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.start", request.FacetRangeStarts.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
                        vars.Add(new KeyValuePair<string, string>("facet.range.end", request.FacetRangeEnds.Value.AddSeconds(1).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
                        /*FQuery = FQuery.Append("&facet.range.start=" + request.FacetRangeStarts.Value.ToString().Substring(0, 10) + "T" + request.FacetRangeStarts.Value.Hour + ":" + request.FacetRangeStarts.Value.Minute + ":" + request.FacetRangeStarts.Value.Second + "Z");
                        FQuery = FQuery.Append("&facet.range.end=" + request.FacetRangeEnds.Value.ToString().Substring(0, 10) + "T" + request.FacetRangeEnds.Value.Hour + ":" + request.FacetRangeEnds.Value.Minute + ":" + request.FacetRangeEnds.Value.Second + "Z");*/
                    }

                    if (request.FacetRangeGap != null && request.FacetRangeGapDuration != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.gap", "+" + request.FacetRangeGapDuration + request.FacetRangeGap));
                        //FQuery = FQuery.Append("&facet.range.gap=%2B" + request.FacetRangeGapDuration + request.FacetRangeGap);
                    }
                    if (request.wt != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("wt", request.wt.ToString()));
                    }


                    //vars.Add(new KeyValuePair<string, string>("fl", "null"));


                }
                else
                {
                    string sortFields = string.Empty;

                    // lets make solr fields and pass to solr
                    if (!string.IsNullOrEmpty(request.SortFields))
                    {
                        sortFields = GenerateTwitterSortField(request.SortFields);
                    }

                    if (request.SortFields != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("sort", sortFields));
                    }

                    // to make search effective and to reduce search time ,
                    // we should only retrun the fields which are required
                    // so lets add fields that we need to return , this is config driven,
                    // to add any field , we just need to change in config :)
                    vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["solrTwitterFL"]));
                }

                // our q query is ready!!! 
                // lets add it to keyvalue pair for q,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                // as fq is not compelsary search 
                // we will pass it only if there is search criteria on fq 
                if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                {
                    vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                }




                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                string RequestURL = string.Empty;
                if (request.IsOutRequest)
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                    res.RequestUrl = RequestURL;
                }
                else
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                }

                res.OriginalRequest = request;

                CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);

                //CommonFunction.LogInfo("Solr Response - TimeTaken - for Social Media get response" + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                if (!request.Facet || !isBindChart)
                {
                    XmlDocument xDoc = new XmlDocument();

                    // lets load solr response to xml so we can get data in xmk format
                    xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                    // lets parse response. to our 'SearchResult' object
                    parseTwitterResponse(xDoc, res);
                }

                CommonFunction.LogInfo("Twitter Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);


                //CommonFunction.LogInfo("Solr Response - TimeTaken - for social Media parse response " + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo(string.Format("Total Results for Twitter Count :{0}", res.TotalResults), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("PMG Twitter Call End", request.IsPmgLogging, request.PmgLogFileLocation);


                return res;
            }
            catch (Exception _Exception)
            {
                isError = true;
                CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        private static void parseTwitterResponse(XmlDocument doc, SearchTwitterResult res)
        {
            try
            {
                XDocument xDoc = XDocument.Parse(Convert.ToString(doc.InnerXml));

                res.TwitterResults = new List<TwitterResult>();

                int totalRecords = Convert.ToInt32((from p in xDoc.Descendants("response").Descendants("result")
                                                    select p.Attribute("numFound").Value).FirstOrDefault());
                res.TotalResults = totalRecords;


                XElement highlightingEle = (from p in xDoc.Descendants("response").Descendants("lst")
                                            where p.Attribute("name").Value == "highlighting"
                                            select p).FirstOrDefault();

                foreach (XElement elem in xDoc.Descendants("response").Descendants("result").Descendants("doc"))
                {
                    TwitterResult TwitterResult = new TwitterResult();
                    foreach (XElement childelem in elem.Elements())
                    {

                        if (childelem.Attribute("name").Value == "iqseqid")
                        {
                            TwitterResult.iqseqid = Convert.ToInt64(childelem.Value);
                            TwitterResult.tweet_id = Convert.ToInt64(childelem.Value);

                            if (res.OriginalRequest.IsHighlighting && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm))
                            {
                                TwitterResult.Highlight = (from c in highlightingEle.Descendants("lst")
                                                           where c.Attribute("name").Value == childelem.Value
                                                           select c).FirstOrDefault().Descendants("str").FirstOrDefault().Value;
                            }
                        }
                        else if (childelem.Attribute("name").Value == "tweet_body")
                        {
                            TwitterResult.tweet_body = ConvertURLsToHyperlinks(HttpUtility.HtmlDecode(childelem.Value.Replace("â€", "").Replace("â€³", "")));
                        }
                        else if (childelem.Attribute("name").Value == "actor_displayname")
                        {
                            TwitterResult.actor_displayName = childelem.Value;
                            //smResult.date = Convert.ToDateTime(childelem.Value).ToUniversalTime().ToString("MM/dd/yyyy hh:mm tt");
                        }
                        else if (childelem.Attribute("name").Value == "actor_preferredusername")
                        {
                            TwitterResult.actor_prefferedUserName = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "tweet_posteddatetime")
                        {
                            TwitterResult.tweet_postedDateTime = Convert.ToDateTime(childelem.Value).ToUniversalTime().ToString();
                        }
                        else if (childelem.Attribute("name").Value == "actor_friendscount")
                        {
                            TwitterResult.friends_count = Convert.ToInt64(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "actor_image")
                        {
                            TwitterResult.actor_image = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "actor_followerscount")
                        {
                            TwitterResult.followers_count = Convert.ToInt64(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "gnip_klout_score")
                        {
                            TwitterResult.Klout_score = Convert.ToInt64(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "actor_link")
                        {
                            TwitterResult.actor_link = childelem.Value;
                        }
                    }


                    res.TwitterResults.Add(TwitterResult);
                }



                if (res.OriginalRequest.IsSentiment && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm) && res.OriginalRequest.LowThreshold != null && res.OriginalRequest.HighThreshold != null)
                {
                    Dictionary<string, List<string>> _MapIDToListOfHighlight = new Dictionary<string, List<string>>();

                    _MapIDToListOfHighlight = (from XmlNode OccNode in doc.SelectNodes("/response/lst[@name='highlighting']/lst")
                                               select new
                                               {
                                                   ID = OccNode.Attributes["name"].Value,
                                                   ListOfHighlight = (from XmlNode node in OccNode.SelectNodes("arr/str") select node.InnerText).ToList()
                                               }).ToDictionary(a => a.ID, a => a.ListOfHighlight);

                    SentimentLogic _SentimentLogic = new SentimentLogic();
                    Dictionary<string, Sentiments> _IDToSentimentsMap = _SentimentLogic.GetSentiment(_MapIDToListOfHighlight, res.OriginalRequest.LowThreshold.Value, res.OriginalRequest.HighThreshold.Value, res.OriginalRequest.ClientGuid);

                    foreach (TwitterResult twitterResult in res.TwitterResults)
                    {
                        Sentiments _Sentiments = new Sentiments();
                        if (_IDToSentimentsMap.TryGetValue(twitterResult.iqseqid.ToString(), out _Sentiments))
                        {
                            twitterResult.Sentiments = _Sentiments;

                            foreach (SubSentiment ss in _Sentiments.HighlightToWeightMap)
                            {
                                CommonFunction.LogInfo("Sentiment : => " + ss.HighlightingText + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                                CommonFunction.LogInfo("Sentiment Weight : => " + Convert.ToString(ss.Weight) + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            }
                            CommonFunction.LogInfo("Positive Sentiment ID : " + twitterResult.iqseqid + " => " + _Sentiments.PositiveSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            CommonFunction.LogInfo("Negative Sentiment ID : " + twitterResult.iqseqid + " => " + _Sentiments.NegativeSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public SearchProQuestResult SearchProQuest(SearchProQuestRequest request, bool isBindChart, out Boolean isError, Int32? timeOutPeriod = null, bool isParagraphSearch = false)
        {
            SearchProQuestResult res = new SearchProQuestResult();
            isError = false;
            try
            {
                CommonFunction.LogInfo("PMG ProQuest Call Start", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Create Request for ProQuest ", request.IsPmgLogging, request.PmgLogFileLocation);


                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q qeury parameter of solr and 
                // 'FQuery' we will pass in the fq qeury parameter of solr 
                StringBuilder Query = new StringBuilder();
                StringBuilder FQuery = new StringBuilder();

                // start and rows return the required page. No. data.
                if (request.Start != null)
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.Start), System.Globalization.CultureInfo.CurrentCulture)));
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                }

                vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));

                string FlContent = String.Empty;
                string FlTitle = String.Empty;
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    request.SearchTerm = request.SearchTerm.Trim();

                    string searchTerm = request.SearchTerm.EndsWith("#") ? request.SearchTerm.Remove(request.SearchTerm.Length - 1, 1) : request.SearchTerm;
                    if (request.SearchTerm.EndsWith("#") && !Regex.IsMatch(request.SearchTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                    {
                        FlContent = "content";
                        FlTitle = "title";
                        Query = Query.AppendFormat("({0}:({3}) OR {1}:({3}) OR {2}:({3}))", FlContent, FlTitle, "author", searchTerm);
                    }
                    else
                    {
                        FlContent = "contentgen";
                        FlTitle = "titlegen";
                        Query = Query.AppendFormat("({0}:({3}) OR {1}:({3}) OR {2}:({3}))", FlContent, FlTitle, "authorgen", searchTerm);
                    }

                    if (!isBindChart && (request.IsSentiment || request.IsReturnHighlight || isParagraphSearch))
                    {
                        // all these feilds are for highlighting functionality
                        // hl.fl =  name of the feild on which need to provide highlighting
                        // hl = value can be on/off , if on then highlighting feature is enabled otherwise disabled.
                        // hl.maxAnalyzedChars =  default max char length for highlight is 51200 , but we need unlimited
                        if (!isParagraphSearch)
                        {
                            vars.Add(new KeyValuePair<string, string>("hl.fl", FlContent));
                            vars.Add(new KeyValuePair<string, string>("hl.requireFieldMatch", "true"));
                            vars.Add(new KeyValuePair<string, string>("hl.maxAnalyzedChars", "-1"));
                        }
                        else
                        {
                            vars.Add(new KeyValuePair<string, string>("hl.fl", FlContent + "," + FlTitle));
                            vars.Add(new KeyValuePair<string, string>("hl.maxAnalyzedChars", request.LeadParagraphChars > 0 ? request.LeadParagraphChars.ToString() : "500"));
                        }
                        vars.Add(new KeyValuePair<string, string>("hl", "on"));

                        // by setting it to 225 ,it will return no. of highlights 
                        // fragment size for signle highlight is 145 (approx)
                        if (request.FragSize.HasValue)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", request.FragSize.Value.ToString()));
                        else if (request.IsSentiment)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSizeForSentiment"]));
                        else
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSize"]));

                        vars.Add(new KeyValuePair<string, string>("hl.snippets", "99"));
                    }
                }

                // ooops nothing passed for 'q' search of solr.
                // then as q is required search
                // if nothing is passed then we should pass *:*
                // which mean it return all without making search for q.
                // although after q , it will filter on fq (filter query) search. 
                if (string.IsNullOrEmpty(Query.ToString()))
                {
                    Query = Query.Append("*:*");
                }

                // Abstract
                if (!string.IsNullOrEmpty(request.Abstract))
                {
                    string abstractField = "abstract";
                    if (!request.Abstract.EndsWith("#") || Regex.IsMatch(request.Abstract, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                    {
                        abstractField = "abstractgen";
                    }

                    if (!string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" AND");
                    }
                    FQuery = FQuery.AppendFormat(" ({0}:({1}))", abstractField, request.Abstract.EndsWith("#") ? request.Abstract.Remove(request.Abstract.Length - 1, 1) : request.Abstract);
                }

                // Content
                if (!string.IsNullOrEmpty(request.Content))
                {
                    string contentField = "content";
                    if (!request.Content.EndsWith("#") || Regex.IsMatch(request.Content, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                    {
                        contentField = "contentgen";
                    }

                    if (!string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" AND");
                    }
                    FQuery = FQuery.AppendFormat(" ({0}:({1}))", contentField, request.Content.EndsWith("#") ? request.Content.Remove(request.Content.Length - 1, 1) : request.Content);
                }

                // Title
                if (!string.IsNullOrEmpty(request.Title))
                {
                    string titleField = "title";
                    if (!request.Title.EndsWith("#") || Regex.IsMatch(request.Title, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                    {
                        titleField = "titlegen";
                    }

                    if (!string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" AND");
                    }
                    FQuery = FQuery.AppendFormat(" ({0}:({1}))", titleField, request.Title.EndsWith("#") ? request.Title.Remove(request.Title.Length - 1, 1) : request.Title);
                }

                // IQSeqID
                if (request.IDs != null && request.IDs.Count() > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" iqseqid:(");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND iqseqid:(");
                    }

                    foreach (string _id in request.IDs)
                    {
                        FQuery = FQuery.Append(_id + " ");
                    }

                    FQuery = FQuery.Append(")");
                }

                // Publications
                if (request.Publications != null && request.Publications.Count > 0)
                {
                    string publicationField = String.Empty;

                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool isFirstPublication = true;

                    foreach (string publication in request.Publications)
                    {
                        publicationField = "publication";
                        if (!publication.EndsWith("#") || Regex.IsMatch(publication, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            publicationField = "publicationgen";
                        }

                        if (isFirstPublication)
                        {
                            isFirstPublication = false;
                            FQuery = FQuery.AppendFormat(" {0}:{1}", publicationField, publication);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR {0}:{1}", publicationField, publication);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // Authors
                if (request.Authors != null && request.Authors.Count > 0)
                {
                    string authorField = String.Empty;

                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool isFirstAuthor = true;

                    foreach (string author in request.Authors)
                    {
                        authorField = "author";
                        if (!author.EndsWith("#") || Regex.IsMatch(author, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            authorField = "authorgen";
                        }

                        if (isFirstAuthor)
                        {
                            isFirstAuthor = false;
                            FQuery = FQuery.AppendFormat(" {0}:{1}", authorField, author);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR {0}:{1}", authorField, author);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // Languages
                if (request.Languages != null && request.Languages.Count > 0)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool isFirstLanguage = true;

                    foreach (string language in request.Languages)
                    {
                        if (isFirstLanguage)
                        {
                            isFirstLanguage = false;
                            FQuery = FQuery.AppendFormat(" language:{0}", language);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR language:{0}", language);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // Media Date
                if (request.StartDate != null && request.EndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" mediadatedt:[");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND mediadatedt:[");
                    }

                    FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                    FQuery = FQuery.Append("Z TO ");
                    FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                    FQuery = FQuery.Append("Z]");
                }

                // if FromRecordID is not null then don't get data after this recordID
                if (request.FromRecordID != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    FQuery = FQuery.AppendFormat(" iqseqid:[* TO {0}])", request.FromRecordID);

                }

                if (request.Facet && isBindChart)
                {
                    vars.Add(new KeyValuePair<string, string>("facet", "on"));

                    if (!string.IsNullOrEmpty(request.FacetRangeOther))
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.other", request.FacetRangeOther));
                    }

                    if (!string.IsNullOrWhiteSpace(request.FacetRange))
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range", request.FacetRange));
                    }
                    if (request.FacetRangeStarts != null && request.FacetRangeEnds != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.start", request.FacetRangeStarts.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
                        vars.Add(new KeyValuePair<string, string>("facet.range.end", request.FacetRangeEnds.Value.AddSeconds(1).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
                    }

                    if (request.FacetRangeGap != null && request.FacetRangeGapDuration != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("facet.range.gap", "+" + request.FacetRangeGapDuration + request.FacetRangeGap));
                    }
                    if (request.wt != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("wt", request.wt.ToString()));
                    }
                }
                else
                {
                    string sortFields = string.Empty;

                    if (!string.IsNullOrEmpty(request.SortFields))
                    {
                        sortFields = GenerateProQuestSortField(request.SortFields);
                    }

                    if (request.SortFields != null)
                    {
                        vars.Add(new KeyValuePair<string, string>("sort", sortFields));
                    }

                    // to make search effective and to reduce search time ,
                    // we should only retrun the fields which are required
                    // so lets add fields that we need to return , this is config driven,
                    // to add any field , we just need to change in config :)
                    if (!isParagraphSearch)
                    {
                        vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["solrProQuestFL"]));
                    }
                    else
                    {
                        vars.Add(new KeyValuePair<string, string>("fl", "null"));
                    }
                }

                // our q query is ready!!! 
                // lets add it to keyvalue pair for q,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                // as fq is not required search 
                // we will pass it only if there is search criteria on fq 
                if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                {
                    vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                }

                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                string RequestURL = string.Empty;
                if (request.IsOutRequest)
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                    res.RequestUrl = RequestURL;
                }
                else
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                }

                res.OriginalRequest = request;

                CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                if (!request.Facet || !isBindChart)
                {
                    XmlDocument xDoc = new XmlDocument();

                    // lets load solr response to xml so we can get data in xmk format
                    xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                    // lets parse response. to our 'SearchResult' object
                    if (!isParagraphSearch)
                    {
                        parseProQuestResponse(xDoc, res);
                    }
                    else
                    {
                        parsePQParagraphSearch(xDoc, res, FlContent, FlTitle);
                    }

                    if (request.IsHighlightInLeadParagraph && !string.IsNullOrWhiteSpace(request.SearchTerm))
                    {
                        CheckPQHighlightInLeadParagraph(res, request.LeadParagraphChars, timeOutPeriod);
                    }
                }

                CommonFunction.LogInfo("ProQuest Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);
                CommonFunction.LogInfo(string.Format("Total Results for ProQuest Count :{0}", res.TotalResults), request.IsPmgLogging, request.PmgLogFileLocation);
                CommonFunction.LogInfo("PMG ProQuest Call End", request.IsPmgLogging, request.PmgLogFileLocation);

                return res;
            }
            catch (Exception _Exception)
            {
                isError = true;
                CommonFunction.LogInfo("Exception:" + _Exception.Message + " || Stack Trace: " + _Exception.StackTrace, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        private static void parseProQuestResponse(XmlDocument doc, SearchProQuestResult res, bool isGet = false)
        {
            try
            {
                XDocument xDoc = XDocument.Parse(Convert.ToString(doc.InnerXml));

                res.ProQuestResults = new List<ProQuestResult>();

                int totalRecords = Convert.ToInt32((from p in xDoc.Descendants("response").Descendants("result")
                                                    select p.Attribute("numFound").Value).FirstOrDefault());
                res.TotalResults = totalRecords;

                XElement occurenceNodes = (from p in xDoc.Descendants("response").Descendants("lst")
                                           where p.Attribute("name").Value == "highlighting"
                                           select p).FirstOrDefault();

                List<XElement> docNodes = xDoc.Descendants("response").Descendants("result").Descendants("doc").ToList();
                if ((docNodes == null || docNodes.Count == 0) && isGet)
                {
                    // If performing a get request and only one record is returned, there is no result node
                    docNodes = xDoc.Descendants("response").Descendants("doc").ToList();
                }

                foreach (XElement elem in docNodes)
                {
                    ProQuestResult ProQuestResult = new ProQuestResult();
                    foreach (XElement childelem in elem.Elements())
                    {
                        if (childelem.Attribute("name").Value == "iqseqid")
                        {
                            ProQuestResult.IQSeqID = Convert.ToInt64(childelem.Value);
                            if ((res.OriginalRequest.IsReturnHighlight) && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm))
                            {
                                var Occurences = (from c in occurenceNodes.Descendants("lst")
                                                  where c.Attribute("name").Value == childelem.Value
                                                  select c);

                                ProQuestResult.Mentions = Occurences != null ? Occurences.Descendants("str").Count() : 0;
                                ProQuestResult.Highlights = Occurences.Descendants("str").Select(a => a.Value).ToList();
                            }
                        }
                        else if (childelem.Attribute("name").Value == "abstract")
                        {
                            ProQuestResult.Abstract = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "abstracthtml")
                        {
                            ProQuestResult.AbstractHTML = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "mediacategory")
                        {
                            ProQuestResult.MediaCategory = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "content")
                        {
                            ProQuestResult.Content = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "contenthtml")
                        {
                            ProQuestResult.ContentHTML = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "availdate")
                        {
                            ProQuestResult.AvailableDate = Convert.ToDateTime(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "mediadate")
                        {
                            ProQuestResult.MediaDate = Convert.ToDateTime(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "publication")
                        {
                            ProQuestResult.Publication = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "author")
                        {
                            ProQuestResult.Authors = childelem.Descendants("str").ToList().Select(x => x.Value.ToString()).ToList();
                        }
                        else if (childelem.Attribute("name").Value == "title")
                        {
                            ProQuestResult.Title = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "language")
                        {
                            ProQuestResult.Language = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "languagenum")
                        {
                            ProQuestResult.LanguageNum = Convert.ToInt16(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "copyright")
                        {
                            ProQuestResult.Copyright = childelem.Value;
                        }
                    }

                    res.ProQuestResults.Add(ProQuestResult);
                }

                if (res.OriginalRequest.IsSentiment && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm) && res.OriginalRequest.LowThreshold != null && res.OriginalRequest.HighThreshold != null)
                {
                    Dictionary<string, List<string>> _MapIDToListOfHighlight = new Dictionary<string, List<string>>();

                    _MapIDToListOfHighlight = (from XmlNode OccNode in doc.SelectNodes("/response/lst[@name='highlighting']/lst")
                                               select new
                                               {
                                                   ID = OccNode.Attributes["name"].Value,
                                                   ListOfHighlight = (from XmlNode node in OccNode.SelectNodes("arr/str") select node.InnerText).ToList()
                                               }).ToDictionary(a => a.ID, a => a.ListOfHighlight);

                    SentimentLogic _SentimentLogic = new SentimentLogic();
                    Dictionary<string, Sentiments> _IDToSentimentsMap = _SentimentLogic.GetSentiment(_MapIDToListOfHighlight, res.OriginalRequest.LowThreshold.Value, res.OriginalRequest.HighThreshold.Value, res.OriginalRequest.ClientGuid);

                    foreach (ProQuestResult proQuestResult in res.ProQuestResults)
                    {
                        Sentiments _Sentiments = new Sentiments();
                        if (_IDToSentimentsMap.TryGetValue(proQuestResult.IQSeqID.ToString(), out _Sentiments))
                        {
                            proQuestResult.Sentiments = _Sentiments;

                            foreach (SubSentiment ss in _Sentiments.HighlightToWeightMap)
                            {
                                CommonFunction.LogInfo("Sentiment : => " + ss.HighlightingText + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                                CommonFunction.LogInfo("Sentiment Weight : => " + Convert.ToString(ss.Weight) + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            }
                            CommonFunction.LogInfo("Positive Sentiment ID : " + proQuestResult.IQSeqID + " => " + _Sentiments.PositiveSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            CommonFunction.LogInfo("Negative Sentiment ID : " + proQuestResult.IQSeqID + " => " + _Sentiments.NegativeSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SearchSMResult SearchFacebook(SearchSMRequest request, Int32? timeOutPeriod = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SearchSMResult res = new SearchSMResult();

            try
            {
                CommonFunction.LogInfo("PMG Facebook Call Start", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Create Request for Facebook", request.IsPmgLogging, request.PmgLogFileLocation);

                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q qeury parameter of solr and 
                // 'FQuery' we will pass in the fq qeury parameter of solr 
                StringBuilder Query = new StringBuilder();
                StringBuilder FQuery = new StringBuilder();

                // start and rows return the required page. No. data.
                if (request.Start != null)
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.Start), System.Globalization.CultureInfo.CurrentCulture)));
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                }

                vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string Fl = string.Empty;
                    request.SearchTerm = request.SearchTerm.Trim();
                    if (request.SearchTerm.EndsWith("#") && !Regex.IsMatch(request.SearchTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                    {
                        Fl = "content";
                    }
                    else
                    {
                        Fl = "contentgen";
                    }

                    Query = Query.AppendFormat("{0}:({1})", Fl, request.SearchTerm.EndsWith("#") ? request.SearchTerm.Remove(request.SearchTerm.Length - 1, 1) : request.SearchTerm);

                    if (request.IsReturnHighlight || request.IsSentiment)
                    {
                        // all these feilds are for highlighting functionality
                        // hl.fl =  name of the feild on which need to provide highlighting
                        // hl = value can be on/off , if on then highlighting feature is enabled otherwise disabled.
                        // hl.maxAnalyzedChars =  default max char length for highlight is 51200 , but we need unlimited
                        vars.Add(new KeyValuePair<string, string>("hl.fl", Fl));
                        vars.Add(new KeyValuePair<string, string>("hl.requireFieldMatch", "true"));
                        vars.Add(new KeyValuePair<string, string>("hl", "on"));
                        vars.Add(new KeyValuePair<string, string>("hl.maxAnalyzedChars", "1000"));


                        // by setting it to 225 ,it will return no. of highlights 
                        // fragment size for signle highlight is 145 (approx)
                        if (request.FragSize.HasValue)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", request.FragSize.Value.ToString()));
                        else if (request.IsSentiment)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSizeForSentiment"]));
                        else
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSize"]));
                        vars.Add(new KeyValuePair<string, string>("hl.snippets", "99"));
                    }
                }

                // Author
                if (!string.IsNullOrEmpty(request.Author))
                {
                    string authorField = "authorgen";
                    if (request.SearchTerm.EndsWith("#") && !Regex.IsMatch(request.SearchTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                    {
                        authorField = "author";
                    }

                    if (!string.IsNullOrEmpty(Query.ToString()))
                    {
                        Query = Query.Append(" AND");
                    }
                    Query = Query.AppendFormat(" {0}:({1})", authorField, request.Author.EndsWith("#") ? request.Author.Remove(request.Author.Length - 1, 1) : request.Author);
                }

                // ooops nothing passed for 'q' search of solr.
                // then as q is complesary search
                // if nothing is passed then we should pass *:*
                // which mean it return all without making search for q.
                // although after q , it will filter on fq (filter query) search. 
                if (string.IsNullOrEmpty(Query.ToString()))
                {
                    Query = Query.Append("*:*");
                }

                // Media Date
                if (request.StartDate != null && request.EndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" mediadatedt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND mediadatedt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                // Media Category
                if (!String.IsNullOrWhiteSpace(request.MediaCategory))
                {
                    if (!string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" AND");
                    }

                    FQuery = FQuery.AppendFormat(" mediacategory:{0}", request.MediaCategory);
                }

                // Publications
                if (request.SocialMediaSources != null && request.SocialMediaSources.Count > 0)
                {
                    string sourceField = String.Empty;

                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" (");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND (");
                    }

                    bool isFirstSource = true;

                    foreach (string source in request.SocialMediaSources)
                    {
                        sourceField = "publication";
                        if (!source.EndsWith("#") || Regex.IsMatch(source, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                        {
                            sourceField = "publicationgen";
                        }

                        if (isFirstSource)
                        {
                            isFirstSource = false;
                            FQuery = FQuery.AppendFormat(" {0}:{1}", sourceField, source);
                        }
                        else
                        {
                            FQuery = FQuery.AppendFormat(" OR {0}:{1}", sourceField, source);
                        }
                    }

                    FQuery = FQuery.Append(" )");
                }

                // Page IDs
                if (request.FBPageIDs != null && request.FBPageIDs.Count > 0)
                {
                    if (!String.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" AND");
                    }

                    FQuery = FQuery.Append(" publicationid:(");
                    foreach (string ID in request.FBPageIDs)
                    {
                        FQuery = FQuery.Append(ID + " ");
                    }
                    FQuery = FQuery.Append(")");
                }

                // Exclude Page IDs
                if (request.ExcludeFBPageIDs != null && request.ExcludeFBPageIDs.Count > 0)
                {
                    if (!String.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" AND");
                    }

                    FQuery = FQuery.Append(" -publicationid:(");
                    foreach (string ID in request.ExcludeFBPageIDs)
                    {
                        FQuery = FQuery.Append(ID + " ");
                    }
                    FQuery = FQuery.Append(")");
                }

                // Include Default Pages
                if (!request.IncludeDefaultFBPages)
                {
                    if (!String.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" AND");
                    }

                    // Some records may not have values for this field and should be treated as false, so check for !=1 instead of =0
                    FQuery = FQuery.Append(" -isdefaultsource:1");
                }

                // our q query is ready!!! 
                // lets add it to keyvalue pair for q,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                // as fq is not compelsary search 
                // we will pass it only if there is search criteria on fq 
                if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                {
                    vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                }

                // to make search effective and to reduce search time ,
                // we should only retrun the fields which are required
                // so lets add fields that we need to return , this is config driven,
                // to add any field , we just need to change in config :)
                if (request.isShowContent)
                    vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["SolrFBContentFL"]));
                else
                    vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["SolrFBFL"]));


                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                string RequestURL = string.Empty;
                if (request.IsOutRequest)
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                    res.RequestUrl = RequestURL;
                }
                else
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                }

                CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Solr Response - TimeTaken - for Facebook get response" + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                XmlDocument xDoc = new XmlDocument();

                // lets load solr response to xml so we can get data in xmk format
                xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                res.OriginalRequest = request;

                CommonFunction.LogInfo("Facebook Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);

                // lets parse response. to our 'SearchResult' object
                parseFacebookResponse(xDoc, res);

                sw.Stop();

                CommonFunction.LogInfo("Solr Response - TimeTaken - for Facebook parse response " + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo(string.Format("Total Results for Facebook Count :{0}", res.TotalResults), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("PMG Facebook Call End", request.IsPmgLogging, request.PmgLogFileLocation);

                return res;
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        private static void parseFacebookResponse(XmlDocument doc, SearchSMResult res)
        {
            try
            {
                XDocument xDoc = XDocument.Parse(Convert.ToString(doc.InnerXml));

                res.smResults = new List<SMResult>();

                int totalRecords = Convert.ToInt32((from p in xDoc.Descendants("response").Descendants("result")
                                                    select p.Attribute("numFound").Value).FirstOrDefault());
                res.TotalResults = totalRecords;

                XElement highlightingEle = (from p in xDoc.Descendants("response").Descendants("lst")
                                            where p.Attribute("name").Value == "highlighting"
                                            select p).FirstOrDefault();

                foreach (XElement elem in xDoc.Descendants("response").Descendants("result").Descendants("doc"))
                {
                    SMResult result = new SMResult();
                    foreach (XElement childelem in elem.Elements())
                    {
                        if (childelem.Attribute("name").Value == "iqseqid")
                        {
                            result.IQSeqID = childelem.Value;

                            if (res.OriginalRequest.IsReturnHighlight && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm))
                            {
                                result.Highlights = new List<string>() { 
                                                    (from c in highlightingEle.Descendants("lst")
                                                     where c.Attribute("name").Value == childelem.Value
                                                     select c).FirstOrDefault().Descendants("str").FirstOrDefault().Value 
                                };
                            }
                        }
                        else if (childelem.Attribute("name").Value == "author")
                        {
                            result.AuthorName = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "content")
                        {
                            result.content = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "mediadate")
                        {
                            result.itemHarvestDate_DT = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "ispageverified")
                        {
                            result.IsPageVerified = childelem.Value == "1";
                        }
                        else if (childelem.Attribute("name").Value == "mediacategory")
                        {
                            result.feedClass = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "publication")
                        {
                            result.Source = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "picture")
                        {
                            result.PictureUrl = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "publicationid")
                        {
                            result.PublicationID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "postid")
                        {
                            result.PostID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "postlikes")
                        {
                            result.NumLikes = Convert.ToInt64(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "postshares")
                        {
                            result.NumShares = Convert.ToInt64(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "postcomments")
                        {
                            result.NumComments = Convert.ToInt64(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "url")
                        {
                            result.link = childelem.Value;
                        }
                    }

                    res.smResults.Add(result);
                }

                if (res.OriginalRequest.IsSentiment && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm) && res.OriginalRequest.LowThreshold != null && res.OriginalRequest.HighThreshold != null)
                {
                    Dictionary<string, List<string>> _MapIDToListOfHighlight = new Dictionary<string, List<string>>();

                    _MapIDToListOfHighlight = (from XmlNode OccNode in doc.SelectNodes("/response/lst[@name='highlighting']/lst")
                                               select new
                                               {
                                                   ID = OccNode.Attributes["name"].Value,
                                                   ListOfHighlight = (from XmlNode node in OccNode.SelectNodes("arr/str") select node.InnerText).ToList()
                                               }).ToDictionary(a => a.ID, a => a.ListOfHighlight);

                    SentimentLogic _SentimentLogic = new SentimentLogic();
                    Dictionary<string, Sentiments> _IDToSentimentsMap = _SentimentLogic.GetSentiment(_MapIDToListOfHighlight, res.OriginalRequest.LowThreshold.Value, res.OriginalRequest.HighThreshold.Value, res.OriginalRequest.ClientGuid);

                    foreach (SMResult smResult in res.smResults)
                    {
                        Sentiments _Sentiments = new Sentiments();
                        if (_IDToSentimentsMap.TryGetValue(smResult.IQSeqID, out _Sentiments))
                        {
                            smResult.Sentiments = _Sentiments;

                            foreach (SubSentiment ss in _Sentiments.HighlightToWeightMap)
                            {
                                CommonFunction.LogInfo("Sentiment : => " + ss.HighlightingText + " ", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                                CommonFunction.LogInfo("Sentiment Weight : => " + Convert.ToString(ss.Weight) + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            }
                            CommonFunction.LogInfo("Positive Sentiment ID : " + smResult.IQSeqID + " => " + _Sentiments.PositiveSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            CommonFunction.LogInfo("Negative Sentiment ID : " + smResult.IQSeqID + " => " + _Sentiments.NegativeSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SearchSMResult SearchInstagram(SearchSMRequest request, Int32? timeOutPeriod = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SearchSMResult res = new SearchSMResult();

            try
            {
                CommonFunction.LogInfo("PMG Instagram Call Start", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Create Request for Instagram", request.IsPmgLogging, request.PmgLogFileLocation);

                // lets make list of keyvalue pair all the required paramerters(querystring) to pass in solr request
                List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();

                // 'Query' , we will pass in q qeury parameter of solr and 
                // 'FQuery' we will pass in the fq qeury parameter of solr 
                StringBuilder Query = new StringBuilder();
                StringBuilder FQuery = new StringBuilder();

                // start and rows return the required page. No. data.
                if (request.Start != null)
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.Start), System.Globalization.CultureInfo.CurrentCulture)));
                }
                else
                {
                    vars.Add(new KeyValuePair<string, string>("start", Convert.ToString((request.PageNumber) * request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));
                }

                vars.Add(new KeyValuePair<string, string>("rows", Convert.ToString(request.PageSize, System.Globalization.CultureInfo.CurrentCulture)));

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string Fl = string.Empty;
                    request.SearchTerm = request.SearchTerm.Trim();
                    if (request.SearchTerm.EndsWith("#") && !Regex.IsMatch(request.SearchTerm, @"\w+~[\d.]*(?=[^""]*(?:""[^""]*""[^""]*)*[^""]*$)"))
                    {
                        Fl = "content";
                    }
                    else
                    {
                        Fl = "contentgen";
                    }

                    Query = Query.AppendFormat("{0}:({1})", Fl, request.SearchTerm.EndsWith("#") ? request.SearchTerm.Remove(request.SearchTerm.Length - 1, 1) : request.SearchTerm);

                    if (request.IsReturnHighlight || request.IsSentiment)
                    {
                        // all these feilds are for highlighting functionality
                        // hl.fl =  name of the feild on which need to provide highlighting
                        // hl = value can be on/off , if on then highlighting feature is enabled otherwise disabled.
                        // hl.maxAnalyzedChars =  default max char length for highlight is 51200 , but we need unlimited
                        vars.Add(new KeyValuePair<string, string>("hl.fl", Fl));
                        vars.Add(new KeyValuePair<string, string>("hl.requireFieldMatch", "true"));
                        vars.Add(new KeyValuePair<string, string>("hl", "on"));
                        vars.Add(new KeyValuePair<string, string>("hl.maxAnalyzedChars", "1000"));


                        // by setting it to 225 ,it will return no. of highlights 
                        // fragment size for signle highlight is 145 (approx)
                        if (request.FragSize.HasValue)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", request.FragSize.Value.ToString()));
                        else if (request.IsSentiment)
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSizeForSentiment"]));
                        else
                            vars.Add(new KeyValuePair<string, string>("hl.fragsize", ConfigurationManager.AppSettings["SolrFragSize"]));
                        vars.Add(new KeyValuePair<string, string>("hl.snippets", "99"));
                    }
                }

                // ooops nothing passed for 'q' search of solr.
                // then as q is complesary search
                // if nothing is passed then we should pass *:*
                // which mean it return all without making search for q.
                // although after q , it will filter on fq (filter query) search. 
                if (string.IsNullOrEmpty(Query.ToString()))
                {
                    Query = Query.Append("*:*");
                }

                // Media Date
                if (request.StartDate != null && request.EndDate != null)
                {
                    if (string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" mediadatedt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                    else
                    {
                        FQuery = FQuery.Append(" AND mediadatedt:[");
                        FQuery = FQuery.Append(request.StartDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z TO ");
                        FQuery = FQuery.Append(request.EndDate.Value.ToString("s", System.Globalization.CultureInfo.CurrentCulture));
                        FQuery = FQuery.Append("Z]");
                    }
                }

                // Tags/Users
                if (request.SocialMediaSources != null && request.SocialMediaSources.Count == 1)
                {
                    // Tags and users are entered in the agent as solr syntax. Simply replace # with tags: and @ with author:.
                    string queryString = request.SocialMediaSources[0].Replace("#", "tags:").Replace("@", "author:");

                    if (!string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" AND");
                    }

                    FQuery = FQuery.Append(" (" + queryString);
                    FQuery = FQuery.Append(")");
                }

                // Media Category
                if (!String.IsNullOrWhiteSpace(request.MediaCategory))
                {
                    if (!string.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" AND");
                    }

                    FQuery = FQuery.AppendFormat(" mediacategory:{0}", request.MediaCategory);
                }

                // IDs
                if (request.ids != null && request.ids.Count > 0)
                {
                    if (!String.IsNullOrEmpty(FQuery.ToString()))
                    {
                        FQuery = FQuery.Append(" AND");
                    }

                    FQuery = FQuery.Append(" iqseqid:(");
                    foreach (string ID in request.ids)
                    {
                        FQuery = FQuery.Append(ID + " ");
                    }
                    FQuery = FQuery.Append(")");
                }

                // our q query is ready!!! 
                // lets add it to keyvalue pair for q,
                // which untimately passed to solr.
                vars.Add(new KeyValuePair<string, string>("q", Query.ToString()));

                // as fq is not compelsary search 
                // we will pass it only if there is search criteria on fq 
                if (!string.IsNullOrWhiteSpace(FQuery.ToString()))
                {
                    vars.Add(new KeyValuePair<string, string>("fq", FQuery.ToString()));
                }

                // to make search effective and to reduce search time ,
                // we should only retrun the fields which are required
                // so lets add fields that we need to return , this is config driven,
                // to add any field , we just need to change in config :)
                if (request.isShowContent)
                    vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["SolrIGContentFL"]));
                else
                    vars.Add(new KeyValuePair<string, string>("fl", System.Configuration.ConfigurationManager.AppSettings["SolrIGFL"]));


                // at last , we are ready to make request on solr.
                // so lets pass solr search url and all required params 
                // which will turn to request to solr and return solr response
                string RequestURL = string.Empty;
                if (request.IsOutRequest)
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                    res.RequestUrl = RequestURL;
                }
                else
                {
                    res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                }

                CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("Solr Response - TimeTaken - for Instagram get response" + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                UTF8Encoding enc = new UTF8Encoding();

                XmlDocument xDoc = new XmlDocument();

                // lets load solr response to xml so we can get data in xmk format
                xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                res.OriginalRequest = request;

                CommonFunction.LogInfo("Instagram Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);

                // lets parse response. to our 'SearchResult' object
                parseInstagramResponse(xDoc, res);

                sw.Stop();

                CommonFunction.LogInfo("Solr Response - TimeTaken - for Instagram parse response " + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo(string.Format("Total Results for Instagram Count :{0}", res.TotalResults), request.IsPmgLogging, request.PmgLogFileLocation);

                CommonFunction.LogInfo("PMG Instagram Call End", request.IsPmgLogging, request.PmgLogFileLocation);

                return res;
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        private static void parseInstagramResponse(XmlDocument doc, SearchSMResult res)
        {
            try
            {
                XDocument xDoc = XDocument.Parse(Convert.ToString(doc.InnerXml));

                res.smResults = new List<SMResult>();

                int totalRecords = Convert.ToInt32((from p in xDoc.Descendants("response").Descendants("result")
                                                    select p.Attribute("numFound").Value).FirstOrDefault());
                res.TotalResults = totalRecords;

                XElement highlightingEle = (from p in xDoc.Descendants("response").Descendants("lst")
                                            where p.Attribute("name").Value == "highlighting"
                                            select p).FirstOrDefault();

                foreach (XElement elem in xDoc.Descendants("response").Descendants("result").Descendants("doc"))
                {
                    SMResult result = new SMResult();
                    foreach (XElement childelem in elem.Elements())
                    {
                        if (childelem.Attribute("name").Value == "iqseqid")
                        {
                            result.IQSeqID = childelem.Value;

                            if (res.OriginalRequest.IsReturnHighlight && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm))
                            {
                                result.Highlights = new List<string>() { 
                                                    (from c in highlightingEle.Descendants("lst")
                                                     where c.Attribute("name").Value == childelem.Value
                                                     select c).FirstOrDefault().Descendants("str").FirstOrDefault().Value 
                                };
                            }
                        }
                        else if (childelem.Attribute("name").Value == "author")
                        {
                            result.AuthorName = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "authorid")
                        {
                            result.AuthorID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "content")
                        {
                            result.content = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "mediadate")
                        {
                            result.itemHarvestDate_DT = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "mediacategory")
                        {
                            result.feedClass = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "publication")
                        {
                            result.Source = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "picture")
                        {
                            result.PictureUrl = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "publicationid")
                        {
                            result.PublicationID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "postid")
                        {
                            result.PostID = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "postlikes")
                        {
                            result.NumLikes = Convert.ToInt64(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "postshares")
                        {
                            result.NumShares = Convert.ToInt64(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "postcomments")
                        {
                            result.NumComments = Convert.ToInt64(childelem.Value);
                        }
                        else if (childelem.Attribute("name").Value == "url")
                        {
                            result.link = childelem.Value;
                        }
                        else if (childelem.Attribute("name").Value == "taggedusers")
                        {
                            result.TaggedUsers = childelem.Descendants("str").ToList().Select(x => x.Value.ToString()).ToList();
                        }
                    }

                    res.smResults.Add(result);
                }

                if (res.OriginalRequest.IsSentiment && !string.IsNullOrWhiteSpace(res.OriginalRequest.SearchTerm) && res.OriginalRequest.LowThreshold != null && res.OriginalRequest.HighThreshold != null)
                {
                    Dictionary<string, List<string>> _MapIDToListOfHighlight = new Dictionary<string, List<string>>();

                    _MapIDToListOfHighlight = (from XmlNode OccNode in doc.SelectNodes("/response/lst[@name='highlighting']/lst")
                                               select new
                                               {
                                                   ID = OccNode.Attributes["name"].Value,
                                                   ListOfHighlight = (from XmlNode node in OccNode.SelectNodes("arr/str") select node.InnerText).ToList()
                                               }).ToDictionary(a => a.ID, a => a.ListOfHighlight);

                    SentimentLogic _SentimentLogic = new SentimentLogic();
                    Dictionary<string, Sentiments> _IDToSentimentsMap = _SentimentLogic.GetSentiment(_MapIDToListOfHighlight, res.OriginalRequest.LowThreshold.Value, res.OriginalRequest.HighThreshold.Value, res.OriginalRequest.ClientGuid);

                    foreach (SMResult smResult in res.smResults)
                    {
                        Sentiments _Sentiments = new Sentiments();
                        if (_IDToSentimentsMap.TryGetValue(smResult.IQSeqID, out _Sentiments))
                        {
                            smResult.Sentiments = _Sentiments;

                            foreach (SubSentiment ss in _Sentiments.HighlightToWeightMap)
                            {
                                CommonFunction.LogInfo("Sentiment : => " + ss.HighlightingText + " ", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                                CommonFunction.LogInfo("Sentiment Weight : => " + Convert.ToString(ss.Weight) + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            }
                            CommonFunction.LogInfo("Positive Sentiment ID : " + smResult.IQSeqID + " => " + _Sentiments.PositiveSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                            CommonFunction.LogInfo("Negative Sentiment ID : " + smResult.IQSeqID + " => " + _Sentiments.NegativeSentiment + "", res.OriginalRequest.IsPmgLogging, res.OriginalRequest.PmgLogFileLocation);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GenerateTwitterSortField(string sortfields)
        {
            try
            {
                IDictionary<string, string> PMGSMSearchSortFields = new Dictionary<string, string>();

                PMGSMSearchSortFields.Add("user", "actor_displayname asc");
                PMGSMSearchSortFields.Add("user-", "actor_displayname desc");

                PMGSMSearchSortFields.Add("body", "tweet_body asc");
                PMGSMSearchSortFields.Add("body-", "tweet_body desc");

                PMGSMSearchSortFields.Add("klout_score", "gnip_klout_score asc");
                PMGSMSearchSortFields.Add("klout_score-", "gnip_klout_score desc");

                PMGSMSearchSortFields.Add("date", "tweet_posteddatetime asc");
                PMGSMSearchSortFields.Add("date-", "tweet_posteddatetime desc");


                StringBuilder InputSortFields = new StringBuilder();

                string[] PMGSMSearchSortField = sortfields.Split(',');

                // max solr solr field is config driven , so we only can search on max that. no of fields
                int MaxNoOfSortFields = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxSortField"], System.Globalization.CultureInfo.CurrentCulture);
                int index = 0;

                foreach (string SortField in PMGSMSearchSortField)
                {
                    if (PMGSMSearchSortFields.ContainsKey(SortField.ToLower(System.Globalization.CultureInfo.CurrentCulture)))
                    {
                        InputSortFields.Append(PMGSMSearchSortFields[SortField.ToLower(System.Globalization.CultureInfo.CurrentCulture)] + ",");
                    }

                    index = index + 1;

                    if (index >= MaxNoOfSortFields)
                    {
                        break;
                    }
                }

                if (InputSortFields.Length > 0)
                {
                    InputSortFields.Remove(InputSortFields.Length - 1, 1);
                }

                return InputSortFields.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GenerateProQuestSortField(string sortfields)
        {
            try
            {
                IDictionary<string, string> PMGPQSearchSortFields = new Dictionary<string, string>();

                PMGPQSearchSortFields.Add("date", "mediadatedt asc");
                PMGPQSearchSortFields.Add("date-", "mediadatedt desc");

                StringBuilder InputSortFields = new StringBuilder();

                string[] PMGPQSearchSortField = sortfields.Split(',');

                // max solr solr field is config driven , so we only can search on max that. no of fields
                int MaxNoOfSortFields = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxSortField"], System.Globalization.CultureInfo.CurrentCulture);
                int index = 0;

                foreach (string SortField in PMGPQSearchSortField)
                {
                    if (PMGPQSearchSortFields.ContainsKey(SortField.ToLower(System.Globalization.CultureInfo.CurrentCulture)))
                    {
                        InputSortFields.Append(PMGPQSearchSortFields[SortField.ToLower(System.Globalization.CultureInfo.CurrentCulture)] + ",");
                    }

                    index = index + 1;

                    if (index >= MaxNoOfSortFields)
                    {
                        break;
                    }
                }

                if (InputSortFields.Length > 0)
                {
                    InputSortFields.Remove(InputSortFields.Length - 1, 1);
                }

                return InputSortFields.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SearchNewsResults SearchNewsByID(SearchNewsRequest request, Int32? timeOutPeriod = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SearchNewsResults res = new SearchNewsResults();

            try
            {
                CommonFunction.LogInfo("Search News By ID Start", request.IsPmgLogging, request.PmgLogFileLocation);

                if (request.IDs != null && request.IDs.Count > 0)
                {
                    List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();
                    vars.Add(new KeyValuePair<string, string>("wt", "xml"));

                    foreach (string id in request.IDs)
                    {
                        vars.Add(new KeyValuePair<string, string>("id", id));
                    }

                    res.OriginalRequest = request;

                    string RequestURL = string.Empty;
                    if (request.IsOutRequest)
                    {
                        res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                        res.RequestUrl = RequestURL;
                    }
                    else
                    {
                        res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                    }

                    CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);
                    CommonFunction.LogInfo("Solr Response - TimeTaken - for News get response " + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                    UTF8Encoding enc = new UTF8Encoding();
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                    CommonFunction.LogInfo("News Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);

                    parseNewsResponse(xDoc, res, true);

                    sw.Stop();

                    CommonFunction.LogInfo("Solr Response - TimeTaken - for News parse response " + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);
                    CommonFunction.LogInfo(string.Format("Total Results for News Count :{0}", res.newsResults != null ? res.newsResults.Count : 0), request.IsPmgLogging, request.PmgLogFileLocation);
                    CommonFunction.LogInfo("Search News By ID Call End", request.IsPmgLogging, request.PmgLogFileLocation);

                    return res;
                }
                else
                {
                    CommonFunction.LogInfo("No IDs specified", request.IsPmgLogging, request.PmgLogFileLocation);
                    res.ResponseXml = "<response status=\"0\">No IDs specified</response>";
                    return res;
                }
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        public SearchSMResult SearchSocialMediaByID(SearchSMRequest request, Int32? timeOutPeriod = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SearchSMResult res = new SearchSMResult();

            try
            {
                CommonFunction.LogInfo("Search Social Media By ID Start", request.IsPmgLogging, request.PmgLogFileLocation);

                if (request.ids != null && request.ids.Count > 0)
                {
                    List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();
                    vars.Add(new KeyValuePair<string, string>("wt", "xml"));

                    foreach (string id in request.ids)
                    {
                        vars.Add(new KeyValuePair<string, string>("id", id));
                    }

                    res.OriginalRequest = request;

                    string RequestURL = string.Empty;
                    if (request.IsOutRequest)
                    {
                        res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                        res.RequestUrl = RequestURL;
                    }
                    else
                    {
                        res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                    }

                    CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);
                    CommonFunction.LogInfo("Solr Response - TimeTaken - for Social Media get response " + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                    UTF8Encoding enc = new UTF8Encoding();
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                    CommonFunction.LogInfo("Social Media Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);

                    parseSMResponse(xDoc, res, true);

                    sw.Stop();

                    CommonFunction.LogInfo("Solr Response - TimeTaken - for Social Media parse response " + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);
                    CommonFunction.LogInfo(string.Format("Total Results for Social Media Count :{0}", res.smResults != null ? res.smResults.Count : 0), request.IsPmgLogging, request.PmgLogFileLocation);
                    CommonFunction.LogInfo("Search Social Media By ID Call End", request.IsPmgLogging, request.PmgLogFileLocation);

                    return res;
                }
                else
                {
                    CommonFunction.LogInfo("No IDs specified", request.IsPmgLogging, request.PmgLogFileLocation);
                    res.ResponseXml = "<response status=\"0\">No IDs specified</response>";
                    return res;
                }
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        public SearchProQuestResult SearchProQuestByID(SearchProQuestRequest request, Int32? timeOutPeriod = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SearchProQuestResult res = new SearchProQuestResult();

            try
            {
                CommonFunction.LogInfo("Search ProQuest By ID Start", request.IsPmgLogging, request.PmgLogFileLocation);

                if (request.IDs != null && request.IDs.Count > 0)
                {
                    List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();
                    vars.Add(new KeyValuePair<string, string>("wt", "xml"));

                    foreach (string id in request.IDs)
                    {
                        vars.Add(new KeyValuePair<string, string>("id", id));
                    }

                    res.OriginalRequest = request;

                    string RequestURL = string.Empty;
                    if (request.IsOutRequest)
                    {
                        res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, out RequestURL, request.RawParam);
                        res.RequestUrl = RequestURL;
                    }
                    else
                    {
                        res.ResponseXml = RestClient.getXML(Url.AbsoluteUri, vars, request.IsPmgLogging, request.PmgLogFileLocation, timeOutPeriod, request.RawParam);
                    }

                    CommonFunction.LogInfo("Load Response", request.IsPmgLogging, request.PmgLogFileLocation);
                    CommonFunction.LogInfo("Solr Response - TimeTaken - for ProQuest get response " + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);

                    UTF8Encoding enc = new UTF8Encoding();
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(new MemoryStream(enc.GetBytes(res.ResponseXml)));

                    CommonFunction.LogInfo("ProQuest Parse Response", request.IsPmgLogging, request.PmgLogFileLocation);

                    parseProQuestResponse(xDoc, res, true);

                    sw.Stop();

                    CommonFunction.LogInfo("Solr Response - TimeTaken - for ProQuest parse response " + string.Format("with thread : Minutes :{0}\nSeconds :{1}\n Mili seconds :{2}", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds), request.IsPmgLogging, request.PmgLogFileLocation);
                    CommonFunction.LogInfo(string.Format("Total Results for ProQuest Count :{0}", res.ProQuestResults != null ? res.ProQuestResults.Count : 0), request.IsPmgLogging, request.PmgLogFileLocation);
                    CommonFunction.LogInfo("Search ProQuest By ID Call End", request.IsPmgLogging, request.PmgLogFileLocation);

                    return res;
                }
                else
                {
                    CommonFunction.LogInfo("No IDs specified", request.IsPmgLogging, request.PmgLogFileLocation);
                    res.ResponseXml = "<response status=\"0\">No IDs specified</response>";
                    return res;
                }
            }
            catch (Exception _Exception)
            {
                CommonFunction.LogInfo("Exception:" + _Exception.Message, request.IsPmgLogging, request.PmgLogFileLocation);

                res.ResponseXml = "<response status=\"0\">" + _Exception.Message + "</response>";
                return res;
            }
        }

        /// <summary>
        /// Adds to the input string a target=_blank in the hyperlinks
        /// </summary>
        public static string ConvertURLsToHyperlinks(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var reg = new Regex(@"(?!(?!.*?<a)[^<]*<\/a>)(\b((http|https)://|www\.)[^ ><]+\b)");
                return reg.Replace(input, new MatchEvaluator(ConvertUrlsMatchDelegate));

            }
            return input;
        }

        public static string ConvertUrlsMatchDelegate(Match m)
        {
            // add in additional http:// in front of the www. for the hyperlinks
            var additional = "";
            if (m.Value.StartsWith("www."))
            {
                additional = "http://";
            }
            return "<a href=\"" + additional + m.Value + "\" target=\"_blank\">" + m.Value + "</a>";
        }

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        /// <summary>
        /// Creates a new disconnected instance of the SearchEngine class. You must set the URL manually after calling this contstructor.
        /// </summary>
        public SearchEngine()
        {
        }
    }
}
