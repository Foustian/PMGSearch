using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sentiment.HelperClasses;

namespace PMGSearch
{
    public class NewsResult
    {
        public string HomeurlDomain
        {
            get { return _HomeurlDomain; }
            set { _HomeurlDomain = value; }
        } string _HomeurlDomain = null;

        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        } string _Title = null;

        public String date
        {
            get { return _date; }
            set { _date = value; }
        } String _date = null;

        public string Category
        {
            get { return _Category; }
            set { _Category = value; }
        } string _Category = null;

        public string Genre
        {
            get { return _Genre; }
            set { _Genre = value; }
        } string _Genre = null;

        public int Mentions
        {
            get { return _Mentions; }
            set { _Mentions = value; }
        } int _Mentions;

        public string Article
        {
            get { return _Article; }
            set { _Article = value; }
        } string _Article = null;

        public string IQSeqID
        {
            get { return _IQSeqID; }
            set { _IQSeqID = value; }
        } string _IQSeqID;


        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
        } string _Content;

        public Sentiments Sentiments
        {
            get { return _sentiments; }
            set { _sentiments = value; }
        }Sentiments _sentiments = new Sentiments();

        public List<string> Highlights
        {
            get { return _Highlights; }
            set { _Highlights = value; }
        }List<string> _Highlights;
         
        public decimal? IQ_AdShare_Value
        {
            get { return _IQ_AdShare_Value; }
            set { _IQ_AdShare_Value = value; }
        }decimal? _IQ_AdShare_Value;

        public Int32? C_uniq_visitor
        {
            get { return _C_uniq_visitor; }
            set { _C_uniq_visitor = value; }
        }Int32? _C_uniq_visitor;

        public bool IsCompeteAll
        {
            get { return _IsCompeteAll; }
            set { _IsCompeteAll = value; }
        }bool _IsCompeteAll;

        public bool IsUrlFound
        {
            get { return _IsUrlFound; }
            set { _IsUrlFound = value; }
        }bool _IsUrlFound;

        public string IQDmaName
        {
            get { return _IQDmaName; }
            set { _IQDmaName = value; }
        }string _IQDmaName;

        public string LastMoreoverID
        {
            get { return _LastMoreoverID; }
            set { _LastMoreoverID = value; }
        }string _LastMoreoverID;

        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }string _ID;

        public string SeqID
        {
            get { return _SeqID; }
            set { _SeqID = value; }
        }string _SeqID;

        public string Source
        {
            get { return _Source; }
            set { _Source = value; }
        }string _Source;

        public string DataFormat
        {
            get { return _DataFormat; }
            set { _DataFormat = value; }
        }string _DataFormat;

        public string publication
        {
            get { return _publication; }
            set { _publication = value; }
        }string _publication;

        public string DuplicateGroupID
        {
            get { return _DuplicateGroupID; }
            set { _DuplicateGroupID = value; }
        }string _DuplicateGroupID;

        public int? EditorialRank
        {
            get { return _EditorialRank; }
            set { _EditorialRank = value; }
        }int? _EditorialRank;

        public string Region
        {
            get { return _Region; }
            set { _Region = value; }
        }string _Region;

        public string SubRegion
        {
            get { return _SubRegion; }
            set { _SubRegion = value; }
        }string _SubRegion;

        public string Country
        {
            get { return _Country; }
            set { _Country = value; }
        }string _Country;

        public string CountryCode
        {
            get { return _CountryCode; }
            set { _CountryCode = value; }
        }string _CountryCode;

        public string ZipCode
        {
            get { return _ZipCode; }
            set { _ZipCode = value; }
        }string _ZipCode;

        public int? AutoRank
        {
            get { return _AutoRank; }
            set { _AutoRank = value; }
        }int? _AutoRank;

        public string MediaType
        {
            get { return _MediaType; }
            set { _MediaType = value; }
        }string _MediaType;

        public string Language
        {
            get { return _Language; }
            set { _Language = value; }
        }string _Language;

        public string SourceFeedLanguage
        {
            get { return _SourceFeedLanguage; }
            set { _SourceFeedLanguage = value; }
        }string _SourceFeedLanguage;

        public DateTime? PublishedDate
        {
            get { return _PublishedDate; }
            set { _PublishedDate = value; }
        }DateTime? _PublishedDate;

        public bool? InWhiteList
        {
            get { return _InWhiteList; }
            set { _InWhiteList = value; }
        }bool? _InWhiteList;

        public int? InBoundLinkCount
        {
            get { return _InBoundLinkCount; }
            set { _InBoundLinkCount = value; }
        }int? _InBoundLinkCount;

        public int? AutoRankOrder
        {
            get { return _AutoRankOrder; }
            set { _AutoRankOrder = value; }
        }int? _AutoRankOrder;

        public string AuthorName
        {
            get { return _AuthorName; }
            set { _AuthorName = value; }
        }string _AuthorName;

        public List<string> TopicNames
        {
            get { return _TopicNames; }
            set { _TopicNames = value; }
        }List<string> _TopicNames;

        public List<string> TopicGroups
        {
            get { return _TopicGroups; }
            set { _TopicGroups = value; }
        }List<string> _TopicGroups;

        public string ZipArea
        {
            get { return _ZipArea; }
            set { _ZipArea = value; }
        }string _ZipArea;

        public string State
        {
            get { return _State; }
            set { _State = value; }
        }string _State;

        public List<string> CompanySymbol
        {
            get { return _CompanySymbol; }
            set { _CompanySymbol = value; }
        }List<string> _CompanySymbol;

        public List<string> CompanyExchange
        {
            get { return _CompanyExchange; }
            set { _CompanyExchange = value; }
        }List<string> _CompanyExchange;

        public string IQMediaType
        {
            get { return _IQMediaType; }
            set { _IQMediaType = value; }
        }string _IQMediaType;

        public int IQSubMediaType
        {
            get { return _IQSubMediaType; }
            set { _IQSubMediaType = value; }
        }int _IQSubMediaType;

        public Int16 IQLicense
        {
            get { return _IQLicense; }
            set { _IQLicense = value; }
        }Int16 _IQLicense;

        public bool IsLeadParagraph
        {
            get { return _IsLeadParagraph; }
            set { _IsLeadParagraph = value; }
        }bool _IsLeadParagraph = false;

        public bool IsSearchTermInHeadline { get; set; }

        public string Copyright { get; set; }

        public string ActivationUrl { get; set; }
    }
}
