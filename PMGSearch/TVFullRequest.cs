using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMGSearch
{
    [Serializable]
    public class TVFullRequest : IEquatable<TVFullRequest>
    {

        public object TVFullRequest_CloneObject()
        {
            return this.MemberwiseClone();
        }
        /// <summary>
        /// How many results to include on each 'page' of results. Also see PageNumber property.
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        } int _pageSize = 25;

        public int MaxHighlights
        {
            get { return _maxHighlights; }
            set { _maxHighlights = value; }
        } int _maxHighlights;

        /// <summary>
        /// Text to search for, parsed by the Lucene QueryParser interface. See http://lucene.apache.org/java/2_9_1/queryparsersyntax.html
        /// </summary>
        public String Terms
        {
            get { return _terms; }
            set { _terms = value; }
        } String _terms = string.Empty;

        /// <summary>
        /// Page number (0-based) of results to return. Based on PageSize property.
        /// </summary>

        public int PageNumber
        {
            get { return _pageNum; }
            set { _pageNum = value; }
        } int _pageNum = 0;

        /// <summary>
        /// Start date (inclusive) of the search
        /// </summary>
        public DateTime? StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        } DateTime? _startDate;

        /// <summary>
        /// End date (inclusive) of the search
        /// </summary>
        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        } DateTime? _endDate;

        public DateTime? ClipStartDate
        {
            get { return _clipStartDate; }
            set { _clipStartDate = value; }
        }DateTime? _clipStartDate;

        public DateTime? ClipEndDate
        {
            get { return _clipEndDate; }
            set { _clipEndDate = value; }
        }DateTime? _clipEndDate;

        public String Title120
        {
            get { return _Title120; }
            set { _Title120 = value; }
        }String _Title120;

        public String Desc100
        {
            get { return _Desc100; }
            set { _Desc100 = value; }
        }String _Desc100;

        public String TimeZone
        {
            get { return _TimeZone; }
            set { _TimeZone = value; }
        }String _TimeZone;

        /// <summary>
        /// Comma-separated list of station ids to search for
        /// </summary>
        public List<string> Stations
        {
            get { return _stationIds; }
            set { _stationIds = value; }
        } List<string> _stationIds = null;

        /// <summary>
        /// Comma-separated list of fields to sort by.
        /// </summary>
        public String SortFields
        {
            get { return _sortFields; }
            set { _sortFields = value; }
        } string _sortFields;

        /// <summary>
        /// Comma-separated list of guids to search
        /// </summary>
        public String GuidList
        {
            get { return _guidList; }
            set { _guidList = value; }
        } string _guidList;

        /// <summary>
        /// Comma-separated list of IQCCKey to search
        /// </summary>
        public String IQCCKeyList
        {
            get { return _iqcckeyList; }
            set { _iqcckeyList = value; }
        } string _iqcckeyList;


        /// <summary>
        /// Comma-separated list of Hours to search
        /// </summary>
        public List<string> Hours
        {
            get { return _hours; }
            set { _hours = value; }
        }List<string> _hours;

        /// <summary>
        /// Comma-separated list of DMA Num
        /// </summary>
        public List<string> IQDmaNum
        {
            get { return _IQ_Dma_Num; }
            set { _IQ_Dma_Num = value; }
        }List<string> _IQ_Dma_Num;


        /// <summary>
        /// Comma-separated list of DMA Name
        /// </summary>
        public List<string> IQDmaName
        {
            get { return _IQ_Dma_Name; }
            set { _IQ_Dma_Name = value; }
        }List<string> _IQ_Dma_Name;

        /// <summary>
        /// Comma-separated list of Clss  Name
        /// </summary>
        public List<string> IQClassNum
        {
            get { return _IQ_Class_Num; }
            set { _IQ_Class_Num = value; }
        }List<string> _IQ_Class_Num;

        /// <summary>
        /// Comma-separated list of Affiliate Name
        /// </summary>
        public List<string> StationAffilNum
        {
            get { return _Station_Affil_Num; }
            set { _Station_Affil_Num = value; }
        }List<string> _Station_Affil_Num;

        /// <summary>
        /// Comma-separated list of Affiliate 
        /// </summary>
        public List<string> StationAffil
        {
            get { return _Station_Affil; }
            set { _Station_Affil = value; }
        }List<string> _Station_Affil;

        public DateTime? RLStationStartDate
        {
            get { return _rlStationStartDate; }
            set { _rlStationStartDate = value; }
        } DateTime? _rlStationStartDate;

        /// <summary>
        /// End date (inclusive) of the search
        /// </summary>
        public DateTime? RLStationEndDate
        {
            get { return _rlStationEndDate; }
            set { _rlStationEndDate = value; }
        } DateTime? _rlStationEndDate;

        public Boolean IsPmgLogging
        {
            get { return _isPMGLogging; }
            set { _isPMGLogging = value; }
        }bool _isPMGLogging;

        public String PmgLogFileLocation
        {
            get { return _pmgLogFileLocation; }
            set { _pmgLogFileLocation = value; }
        }string _pmgLogFileLocation;

        public String SolrQT
        {
            get { return _solrQT; }
            set { _solrQT = value; }
        }string _solrQT;

        public String Appearing
        {
            get { return _appearing; }
            set { _appearing = value; }
        }string _appearing;

        public Boolean IsShowCC
        {
            get { return _isShowCC; }
            set { _isShowCC = value; }
        }Boolean _isShowCC = false;

        public int FragOffset
        {
            get { return _fragOffset; }
            set { _fragOffset = value; }
        }int _fragOffset = 3;

        public Boolean IsTitle120List
        {
            get { return _isTitle120List; }
            set { _isTitle120List = value; }

        }Boolean _isTitle120List = false;

        public Boolean Facet
        {
            get { return _Facet; }
            set { _Facet = value; }
        }Boolean _Facet;

        public String FacetRangeOther
        {
            get { return _FacetRangeOther; }
            set { _FacetRangeOther = value; }
        }String _FacetRangeOther;

        public DateTime? FacetRangeStarts
        {
            get { return _FacetRangeStarts; }
            set { _FacetRangeStarts = value; }
        }DateTime? _FacetRangeStarts;

        public DateTime? FacetRangeEnds
        {
            get { return _FacetRangeEnds; }
            set { _FacetRangeEnds = value; }
        }DateTime? _FacetRangeEnds;

        public RangeGap FacetRangeGap
        {
            get { return _FacetRangeGap; }
            set { _FacetRangeGap = value; }
        }RangeGap _FacetRangeGap;

        public int FacetRangeGapDuration
        {
            get { return _FacetRangeGapDuration; }
            set { _FacetRangeGapDuration = value; }
        }int _FacetRangeGapDuration;

        public ReponseType wt
        {
            get { return _wt; }
            set { _wt = value; }
        }ReponseType _wt;

        public String FacetRange
        {
            get;
            set;
        }String _FacetRange;

        public Dictionary<Dictionary<String, String>, List<String>> AffilForFacet
        {
            get { return _AffilForFacet; }
            set { _AffilForFacet = value; }
        } Dictionary<Dictionary<String, String>, List<String>> _AffilForFacet;

        public Boolean IsOutRequest
        {
            get { return _IsOutRequest; }
            set { _IsOutRequest = value; }
        } Boolean _IsOutRequest;

        public Guid ClientGuid
        {
            get { return _clientGuid; }
            set { _clientGuid = value; }
        }Guid _clientGuid;

        public float? LowThreshold
        {
            get { return _lowThreshold; }
            set { _lowThreshold = value; }
        }float? _lowThreshold = null;

        public float? HighThreshold
        {
            get { return _highThreshold; }
            set { _highThreshold = value; }
        }float? _highThreshold = null;

        public Boolean IsSentiment
        {
            get { return _isSentiment; }
            set { _isSentiment = value; }

        }Boolean _isSentiment = false;

        public int? FragSize
        {
            get { return _fragSize; }
            set { _fragSize = value; }
        }int? _fragSize = null;

        public string FacetField
        {
            get { return _FacetField; }
            set { _FacetField = value; }
        }string _FacetField = null;

        public Int64? Start
        {
            get { return _Start; }
            set { _Start = value; }
        }Int64? _Start = null;


        public string RawParam
        {
            get { return _RawParam; }
            set { _RawParam = value; }
        }string _RawParam = null;

        public List<string> TVRegions
        {
            get { return _TVRegions; }
            set { _TVRegions = value; }
        }List<string> _TVRegions = new List<string>();


        public List<int> IncludeRegionsNum
        {
            get { return _IncludeRegionsNum; }
            set { _IncludeRegionsNum = value; }
        }List<int> _IncludeRegionsNum = new List<int>();


        public List<int> CountryNums
        {
            get { return _CountryNums; }
            set { _CountryNums = value; }
        }List<int> _CountryNums = new List<int>();

        public Boolean LogoStatus
        {
            get { return _LogoStatus; }
            set { _LogoStatus = value; }
        }Boolean _LogoStatus = false;

        public Boolean AdStatus
        {
            get { return _AdStatus; }
            set { _AdStatus = value; }
        }Boolean _AdStatus = false;

        public Boolean PEStatus
        {
            get { return _PEStatus; }
            set { _PEStatus = value; }
        }Boolean _PEStatus = false;

        public List<string> SearchLogoIDs
        {
            get { return _SearchLogoIDs; }
            set { _SearchLogoIDs = value; }
        }List<string> _SearchLogoIDs = new List<string>();

        public List<string> BrandIDs
        {
            get { return _BrandIDs; }
            set { _BrandIDs = value; }
        }List<string> _BrandIDs = new List<string>();

        public List<string> IndustryIDs
        {
            get { return _IndustryIDs; }
            set { _IndustryIDs = value; }
        }List<string> _IndustryIDs = new List<string>();

        public List<string> CompanyIDs
        {
            get { return _CompanyIDs; }
            set { _CompanyIDs = value; }
        }List<string> _CompanyIDs = new List<string>();

        public string EarnedPaid
        {
            get { return _EarnedPaid; }
            set { _EarnedPaid = value; }
        }string _EarnedPaid = "";

        public Boolean IsTitleNContentSearch
        {
            get { return _IsTitleNContentSearch; }
            set { _IsTitleNContentSearch = value; }
        }bool _IsTitleNContentSearch = false;

        public bool Equals(TVFullRequest other)
        {
            if (this.Terms != other.Terms)
                return false;

            if (this.ClipStartDate != other.ClipStartDate)
                return false;

            if (this.ClipEndDate != other.ClipEndDate)
                return false;

            if (this.StartDate != other.StartDate)
                return false;

            if (this.EndDate != other.EndDate)
                return false;

            if (this.Title120 != other.Title120)
                return false;

            if (this.Desc100 != other.Desc100)
                return false;
            if (this.Appearing != other.Appearing)
                return false;

            if (this.TimeZone != other.TimeZone)
                return false;

            if (this.GuidList != other.GuidList)
                return false;

            if (this.IQCCKeyList != other.IQCCKeyList)
                return false;

            if (this.Facet != other.Facet)
                return false;

            if (this.FacetRange != other.FacetRange)
                return false;

            if (this.FacetRangeOther != other.FacetRangeOther)
                return false;

            if (this.FacetRangeGap != other.FacetRangeGap)
                return false;

            if (this.FacetRangeGapDuration != other.FacetRangeGapDuration)
                return false;

            if (this.FacetRangeStarts != other.FacetRangeStarts)
                return false;

            if (this.FacetRangeEnds != other.FacetRangeEnds)
                return false;

            if (this.StationAffil != null && other.StationAffil != null)
            {
                if (this.StationAffil.Count() != other.StationAffil.Count())
                    return false;

                this.StationAffil = this.StationAffil.OrderBy(a => a).ToList();
                other.StationAffil = other.StationAffil.OrderBy(a => a).ToList();
                for (int i = 0; i < this.StationAffil.Count(); i++)
                {
                    string Obj1 = this.StationAffil.ElementAt(i);
                    string Obj2 = other.StationAffil.ElementAt(i);

                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.StationAffil == null && other.StationAffil != null) || (this.StationAffil != null && other.StationAffil == null))
                return false;

            if (this.IQDmaName != null && other.IQDmaName != null)
            {
                if (this.IQDmaName.Count() != other.IQDmaName.Count())
                    return false;

                this.IQDmaName = this.IQDmaName.OrderBy(a => a).ToList();
                other.IQDmaName = other.IQDmaName.OrderBy(a => a).ToList();
                for (int i = 0; i < this.IQDmaName.Count(); i++)
                {
                    string Obj1 = this.IQDmaName.ElementAt(i);
                    string Obj2 = other.IQDmaName.ElementAt(i);

                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.IQDmaName == null && other.IQDmaName != null) || (this.IQDmaName != null && other.IQDmaName == null))
                return false;

            if (this.IQClassNum != null && other.IQClassNum != null)
            {
                if (this.IQClassNum.Count() != other.IQClassNum.Count())
                    return false;

                this.IQClassNum = this.IQClassNum.OrderBy(a => a).ToList();
                other.IQClassNum = other.IQClassNum.OrderBy(a => a).ToList();
                for (int i = 0; i < this.IQClassNum.Count(); i++)
                {
                    string Obj1 = this.IQClassNum.ElementAt(i);
                    string Obj2 = other.IQClassNum.ElementAt(i);

                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.IQClassNum == null && other.IQClassNum != null) || (this.IQClassNum != null && other.IQClassNum == null))
                return false;

            if (this.Hours != null && other.Hours != null)
            {
                if (this.Hours.Count() != other.Hours.Count())
                    return false;

                this.Hours = this.Hours.OrderBy(a => a).ToList();
                other.Hours = other.Hours.OrderBy(a => a).ToList();
                for (int i = 0; i < this.Hours.Count(); i++)
                {
                    string Obj1 = this.Hours.ElementAt(i);
                    string Obj2 = other.Hours.ElementAt(i);

                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.Hours == null && other.Hours != null) || (this.Hours != null && other.Hours == null))
                return false;

            return true;
        }
    }
}
