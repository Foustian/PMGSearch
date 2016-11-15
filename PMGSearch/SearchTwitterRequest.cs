using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMGSearch
{
    [Serializable]
    public class SearchTwitterRequest : IEquatable<SearchTwitterRequest>
    {
        public object SearchTwitterRequest_CloneObject()
        {
            return this.MemberwiseClone();
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        } int _pageSize = 25;

        public int PageNumber
        {
            get { return _pageNum; }
            set { _pageNum = value; }
        } int _pageNum = 0;

        public List<string> ExcludeHandles
        {
            get { return _ExcludeHandles; }
            set { _ExcludeHandles = value; }
        }List<String> _ExcludeHandles;

        public List<long> IDs
        {
            get { return _ids; }
            set { _ids = value; }
        }List<long> _ids;

        public DateTime? StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        } DateTime? _startDate;

        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        } DateTime? _endDate;

        public String SearchTerm
        {
            get { return _SearchTerm; }
            set { _SearchTerm = value; }

        }String _SearchTerm = null;

        public String ActorDisplayName
        {
            get { return _ActorDisplayName; }
            set { _ActorDisplayName = value; }

        }String _ActorDisplayName = null;

        public Int64? FriendsRangeFrom
        {
            get { return _FriendsRangeFrom; }
            set { _FriendsRangeFrom = value; }
        }Int64? _FriendsRangeFrom;

        public Int64? FriendsRangeTo
        {
            get { return _FriendsRangeTo; }
            set { _FriendsRangeTo = value; }
        }Int64? _FriendsRangeTo;

        public Int64? FollowersRangeFrom
        {
            get { return _FollowersRangeFrom; }
            set { _FollowersRangeFrom = value; }
        }Int64? _FollowersRangeFrom;

        public Int64? FollowersRangeTo
        {
            get { return _FollowersRangeTo; }
            set { _FollowersRangeTo = value; }
        }Int64? _FollowersRangeTo;

        public Int64? KloutRangeFrom
        {
            get { return _KloutRangeFrom; }
            set { _KloutRangeFrom = value; }
        }Int64? _KloutRangeFrom;

        public Int64? KloutRangeTo
        {
            get { return _KloutRangeTo; }
            set { _KloutRangeTo = value; }
        }Int64? _KloutRangeTo;


        public List<Guid> gnip_tag
        {
            get { return _gnip_tag; }
            set { _gnip_tag = value; }
        }List<Guid> _gnip_tag;


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

        public String SortFields
        {
            get { return _sortFields; }
            set { _sortFields = value; }
        } string _sortFields;

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

        public String FacetRange
        {
            get;
            set;
        }String _FacetRange;

        public string RawParam
        {
            get { return _RawParam; }
            set { _RawParam = value; }
        }string _RawParam = null;

        public ReponseType wt
        {
            get { return _wt; }
            set { _wt = value; }
        }ReponseType _wt;

        public Boolean IsOutRequest
        {
            get { return _IsOutRequest; }
            set { _IsOutRequest = value; }
        } Boolean _IsOutRequest;

        public Boolean IsSentiment
        {
            get { return _isSentiment; }
            set { _isSentiment = value; }
        }Boolean _isSentiment = false;

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

        public int? FragSize
        {
            get { return _fragSize; }
            set { _fragSize = value; }
        }int? _fragSize = null;

        public Int64? Start
        {
            get { return _Start; }
            set { _Start = value; }
        }Int64? _Start = null;

        public bool IsHighlighting { get; set; }

        public string FromRecordID
        {
            get { return _FromRecordID; }
            set { _FromRecordID = value; }
        }string _FromRecordID;

        public Boolean IsDeleted
        {
            get { return _IsDeleted; }
            set { _IsDeleted = value; }
        }Boolean _IsDeleted = false;

        public bool Equals(SearchTwitterRequest other)
        {
            if (this.SearchTerm != other.SearchTerm)
                return false;

            if (this.StartDate != other.StartDate)
                return false;

            if (this.EndDate != other.EndDate)
                return false;

            if (this.ActorDisplayName != other.ActorDisplayName)
                return false;

            if (this.FollowersRangeFrom != other.FollowersRangeFrom)
                return false;

            if (this.FollowersRangeTo != other.FollowersRangeTo)
                return false;

            if (this.FriendsRangeFrom != other.FriendsRangeFrom)
                return false;

            if (this.FriendsRangeTo != other.FriendsRangeTo)
                return false;

            if (this.KloutRangeFrom != other.KloutRangeFrom)
                return false;

            if (this.KloutRangeTo != other.KloutRangeTo)
                return false;

            if (this.gnip_tag != other.gnip_tag)
                return false;

            return true;
        }
    }
}
