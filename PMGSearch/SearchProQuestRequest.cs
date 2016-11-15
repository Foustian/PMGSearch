using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMGSearch
{
    public class SearchProQuestRequest
    {
        public object SearchProQuestRequest_CloneObject()
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

        public List<String> IDs
        {
            get { return _IDs; }
            set { _IDs = value; }
        } List<String> _IDs;

        public List<String> Publications
        {
            get { return _Publications; }
            set { _Publications = value; }
        } List<String> _Publications;

        public List<String> Authors
        {
            get { return _Authors; }
            set { _Authors = value; }
        } List<String> _Authors;

        public List<String> Languages
        {
            get { return _Languages; }
            set { _Languages = value; }
        } List<String> _Languages;

        public String Abstract
        {
            get { return _Abstract; }
            set { _Abstract = value; }

        }String _Abstract = null;

        public String Title
        {
            get { return _Title; }
            set { _Title = value; }

        }String _Title = null;

        public String Content
        {
            get { return _Content; }
            set { _Content = value; }

        }String _Content = null;


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

        public Boolean IsReturnHighlight
        {
            get { return _isReturnHighlight; }
            set { _isReturnHighlight = value; }
        }Boolean _isReturnHighlight = false;

        public string FromRecordID
        {
            get { return _FromRecordID; }
            set { _FromRecordID = value; }
        }string _FromRecordID;

        public int LeadParagraphChars { get; set; }

        public bool IsHighlightInLeadParagraph { get; set; }

        public bool Equals(SearchProQuestRequest other)
        {
            if (this.SearchTerm != other.SearchTerm)
                return false;

            if (this.StartDate != other.StartDate)
                return false;

            if (this.EndDate != other.EndDate)
                return false;

            if (this.Abstract != other.Abstract)
                return false;

            if (this.Title != other.Title)
                return false;

            if (this.Content != other.Content)
                return false;

            if (this.Publications != null && other.Publications != null)
            {
                if (this.Publications.Count() != other.Publications.Count())
                    return false;

                this.Publications = this.Publications.OrderBy(a => a).ToList();
                other.Publications = other.Publications.OrderBy(a => a).ToList();
                for (int i = 0; i < this.Publications.Count(); i++)
                {
                    string Obj1 = this.Publications.ElementAt(i);
                    string Obj2 = other.Publications.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.Publications != null && other.Publications == null) || this.Publications == null && other.Publications != null)
                return false;

            if (this.IDs != null && other.IDs != null)
            {
                if (this.IDs.Count() != other.IDs.Count())
                    return false;

                this.IDs = this.IDs.OrderBy(a => a).ToList();
                other.IDs = other.IDs.OrderBy(a => a).ToList();
                for (int i = 0; i < this.IDs.Count(); i++)
                {
                    string Obj1 = this.IDs.ElementAt(i);
                    string Obj2 = other.IDs.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.IDs != null && other.IDs == null) || this.IDs == null && other.IDs != null)
                return false;

            return true;
        }
    }
}
