using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sentiment.HelperClasses;

namespace PMGSearch
{
    public class SMResult
    {

        public string IQSeqID
        {
            get { return _IQSeqID; }
            set { _IQSeqID = value; }
        }string _IQSeqID;

        public string HomeurlDomain
        {
            get { return _HomeurlDomain; }
            set { _HomeurlDomain = value; }
        } string _HomeurlDomain = null;

        public string description
        {
            get { return _description; }
            set { _description = value; }
        } string _description = null;

        public String itemHarvestDate_DT
        {
            get { return _itemHarvestDate_DT; }
            set { _itemHarvestDate_DT = value; }
        } String _itemHarvestDate_DT;

        public String feedClass
        {
            get { return _feedClass; }
            set { _feedClass = value; }
        } String _feedClass = null;

        public int feedRank
        {
            get { return _feedRank; }
            set { _feedRank = value; }
        } int _feedRank;

        public String link
        {
            get { return _link; }
            set { _link = value; }
        } String _link = null;

        public String content
        {
            get { return _content; }
            set { _content = value; }
        } String _content;

        public Sentiments Sentiments
        {
            get { return _sentiments; }
            set { _sentiments = value; }
        }Sentiments _sentiments = new Sentiments();

        public int Mentions
        {
            get { return _mentions; }
            set { _mentions = value; }
        }int _mentions;

        public List<string> Highlights
        {
            get { return _highlights; }
            set { _highlights = value; }
        }List<string> _highlights;

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

        public string LastMoreoverID
        {
            get { return _LastMoreoverID; }
            set { _LastMoreoverID = value; }
        }string _LastMoreoverID;

        public string id
        {
            get { return _id; }
            set { _id = value; }
        }string _id;

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

        public string SourceCategory
        {
            get { return _SourceCategory; }
            set { _SourceCategory = value; }
        }string _SourceCategory;

        public string Genre
        {
            get { return _Genre; }
            set { _Genre = value; }
        } string _Genre = null;

        public string DataFormat
        {
            get { return _DataFormat; }
            set { _DataFormat = value; }
        }string _DataFormat;

        public string homeLink
        {
            get { return _homeLink; }
            set { _homeLink = value; }
        } string _homeLink = null;

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

        public string IQDmaName
        {
            get { return _IQDmaName; }
            set { _IQDmaName = value; }
        }string _IQDmaName;

        public bool IsPageVerified { get; set; }

        public string PictureUrl { get; set; }

        public string PublicationID { get; set; }

        public string PostID { get; set; }

        public Int64 NumLikes { get; set; }

        public Int64 NumShares { get; set; }

        public Int64 NumComments { get; set; }

        public string AuthorID { get; set; }

        public List<string> TaggedUsers { get; set; }
    }


}