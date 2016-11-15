using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sentiment.HelperClasses;
namespace PMGSearch
{
    public class Hit
    {
        /// <summary>
        /// GUID of the hit
        /// </summary>
        
        public String Guid
        {
            set { _guid = value; }
            get { return _guid; }
        } String _guid = null;

        public string Iqcckey
        {
            set { _iq_cc_key = value; }
            get { return _iq_cc_key; }
        } string _iq_cc_key = null;

        public string RLStationDateTime
        {
            set { _RL_Station_DateTime = value; }
            get { return _RL_Station_DateTime; }
        } string _RL_Station_DateTime = null;

        /// <summary>
        /// A list of all occurrences of the search term in this hit.
        /// </summary>

        public List<TermOccurrence> TermOccurrences
        {
            get { return _termOccurrences; }
            set { _termOccurrences = value; }
        } List<TermOccurrence> _termOccurrences = new List<TermOccurrence>();

        public Sentiments Sentiments
        {
            get { return _sentiments; }
            set { _sentiments = value; }
        }Sentiments _sentiments = new Sentiments();

        /// <summary>
        /// Station Identifier for the Clip
        /// </summary>
        public string StationId
        {
            get { return _stationID; }
            set { _stationID = value; }
        } string _stationID;

        /// <summary>
        /// Time zone of the Clip
        /// </summary>
        public String ClipTimeZone
        {
            get { return _clipTimeZone; }
            set { _clipTimeZone = value; }
        } String _clipTimeZone;



        /// <summary>
        /// Hour of the clip
        /// </summary>

        public int Hour
        {
            get { return _hour; }
            set { _hour = value; }
        } int _hour;


        /// <summary>
        /// GMT Offset
        /// </summary>
        public int GmtOffset
        {
            get { return _gmtOffset; }
            set { _gmtOffset = value; }
        } int _gmtOffset;

        /// <summary>
        /// DST Adjustment
        /// </summary>
        public int DstOffset
        {
            get { return _dstOffset; }
            set { _dstOffset = value; }
        } int _dstOffset;

        public String Affiliate
        {
            get { return _affiliate; }
            set { _affiliate = value; }
        } String _affiliate;

        public String Market
        {
            get { return _market; }
            set { _market = value; }
        } String _market;

        /// <summary>
        /// Date and time (UTC) of the Clip
        /// </summary>
        public DateTime Timestamp
        {
            get { return _timeStamp;}
            set { _timeStamp = value; }
        } DateTime _timeStamp;

        public int TotalNoOfOccurrence
        {
            get { return _totalNoOfOccurrence; }
            set { _totalNoOfOccurrence = value; }
        }int _totalNoOfOccurrence;

        public string Appearing
        {
            get { return _appearing; }
            set { _appearing = value; }
        }string _appearing;

        public string Title120
        {
            get { return _title120; }
            set { _title120 = value; }
        }string _title120;

        public string IQDmaNum
        {
            get { return _iQDmaNum; }
            set { _iQDmaNum = value; }
        }string _iQDmaNum;


        public List<string> ListOfTitle120
        {
            get { return _listOfTitle120; }
            set { _listOfTitle120 = value; }
        }List<string> _listOfTitle120 = new List<string>();

        public List<int?> ListOfIQStartPoint
        {
            get { return _listOfIQStartPoint; }
            set { _listOfIQStartPoint = value; }
        }List<int?> _listOfIQStartPoint = new List<int?>();


        public List<int?> ListOfIQStartMinute
        {
            get { return _listOfIQStartMinute; }
            set { _listOfIQStartMinute = value; }
        }List<int?> _listOfIQStartMinute = new List<int?>();
        public String AUDIENCE
        {
            set { _audience = value; }
            get { return _audience; }
        }  String _audience;
        public String SQAD_SHAREVALUE
        {
            set { _sqad_sharevalue = value; }
            get { return _sqad_sharevalue; }
        }  String _sqad_sharevalue ;

        public List<TermOccurrence> ClosedCaption
        {
            get { return _closedCaption; }
            set { _closedCaption = value; }
        }List<TermOccurrence> _closedCaption = new List<TermOccurrence>();

        public DateTime GmtDateTime
        {
            set { _gmtDateTime = value; }
            get { return _gmtDateTime; }
        }DateTime _gmtDateTime;

        public int? StartMinute
        {
            get { return _startMinute; }
            set { _startMinute = value; }
        } int? _startMinute;

        public List<string> ListOfIQClass
        {
            get { return _ListOfIQClass; }
            set { _ListOfIQClass = value; }
        }List<string> _ListOfIQClass = new List<string>();

        public string SeqID
        {
            get { return _seqID; }
            set { _seqID = value; }
        } string _seqID;

        public bool LogoStatus { get; set; } //Logo data has been processed
        public bool AdStatus { get; set; } //Ad data has been processed
        public bool PEStatus { get; set; } //Paid/Earned data has been processed
        public List<Int64> PaidLogoIDs
        {
            get { return _PaidLogoIDs; }
            set { _PaidLogoIDs = value; }
        }List<Int64> _PaidLogoIDs = new List<Int64>();
        public List<Int64> EarnedLogoIDs
        {
            get { return _EarnedLogoIDs; }
            set { _EarnedLogoIDs = value; }
        }List<Int64> _EarnedLogoIDs = new List<Int64>();
        public List<Int64> PaidBrandIDs
        {
            get { return _PaidBrandIDs; }
            set { _PaidBrandIDs = value; }
        }List<Int64> _PaidBrandIDs = new List<Int64>();
        public List<Int64> EarnedBrandIDs
        {
            get { return _EarnedBrandIDs; }
            set { _EarnedBrandIDs = value; }
        }List<Int64> _EarnedBrandIDs = new List<Int64>();
        public List<Int64> PaidIndustryIDs
        {
            get { return _PaidIndustryIDs; }
            set { _PaidIndustryIDs = value; }
        }List<Int64> _PaidIndustryIDs = new List<Int64>();
        public List<Int64> EarnedIndustryIDs
        {
            get { return _EarnedIndustryIDs; }
            set { _EarnedIndustryIDs = value; }
        }List<Int64> _EarnedIndustryIDs = new List<Int64>();
        public List<Int64> PaidCompanyIDs
        {
            get { return _PaidCompanyIDs; }
            set { _PaidCompanyIDs = value; }
        }List<Int64> _PaidCompanyIDs = new List<Int64>();
        public List<Int64> EarnedCompanyIDs
        {
            get { return _EarnedCompanyIDs; }
            set { _EarnedCompanyIDs = value; }
        }List<Int64> _EarnedCompanyIDs = new List<Int64>();
        public List<string> Ads
        {
            get { return _Ads; }
            set { _Ads = value; }
        }List<string> _Ads = new List<string>();
        public List<string> Logos
        {
            get { return _Logos; }
            set { _Logos = value; }
        }List<string> _Logos = new List<string>();
    }
}
