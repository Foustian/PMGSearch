using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMGSearch
{
    [Serializable]
    public class SearchNewsRequest : IEquatable<SearchNewsRequest>
    {
        public object SearchNewsRequest_CloneObject()
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

        /// <summary>
        /// End date (inclusive) of the search
        /// </summary>
        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        } DateTime? _endDate;

        public string Source
        {
            get { return _Source; }
            set { _Source = value; }
        } string _Source;

        public List<String> Publications
        {
            get { return _Publications; }
            set { _Publications = value; }
        } List<String> _Publications;

        public List<String> ExcludeDomains
        {
            get { return _ExcludeDomains; }
            set { _ExcludeDomains = value; }
        } List<String> _ExcludeDomains;

        public List<String> NewsCategory
        {
            get { return _NewsCategory; }
            set { _NewsCategory = value; }
        } List<String> _NewsCategory;

        public List<int> PublicationCategory
        {
            get { return _PublicationCategory; }
            set { _PublicationCategory = value; }
        } List<int> _PublicationCategory;

        public List<string> Market
        {
            get { return _Market; }
            set { _Market = value; }
        } List<string> _Market;

        public List<String> Genre
        {
            get { return _Genre; }
            set { _Genre = value; }
        } List<String> _Genre;

        public List<String> NewsRegion
        {
            get { return _NewsRegion; }
            set { _NewsRegion = value; }
        } List<String> _NewsRegion;

        public List<string> Country { get; set; }

        public List<string> Language { get; set; }

        public List<int> SourceType { get; set; }

        public String SearchTerm
        {
            get { return _SearchTerm; }
            set { _SearchTerm = value; }
        } String _SearchTerm;

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

        public ReponseType wt
        {
            get { return _wt; }
            set { _wt = value; }
        }ReponseType _wt;

        public List<String> lstfacetRange
        {

            get { return _lstfacetRange; }
            set { _lstfacetRange = value; }
        } List<String> _lstfacetRange;

        public String SortFields
        {
            get { return _sortFields; }
            set { _sortFields = value; }
        } string _sortFields;

        public List<String> IDs
        {
            get { return _IDs; }
            set { _IDs = value; }
        } List<String> _IDs;

        public Boolean IsShowContent
        {
            get { return _isShowContent; }
            set { _isShowContent = value; }
        }Boolean _isShowContent = false;

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


        public Boolean IsReturnHighlight
        {
            get { return _isReturnHighlight; }
            set { _isReturnHighlight = value; }
        }Boolean _isReturnHighlight = false;

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

        public string FromRecordID
        {
            get { return _FromRecordID; }
            set { _FromRecordID = value; }
        }string _FromRecordID;

        public Boolean IsTitleNContentSearch
        {
            get { return _IsTitleNContentSearch; }
            set { _IsTitleNContentSearch = value; }
        }bool _IsTitleNContentSearch = false;

        public string RawParam
        {
            get { return _RawParam; }
            set { _RawParam = value; }
        }string _RawParam = null;

        public bool IsHilightInLeadParagraph
        {
            get { return _IsHilightInLeadParagraph; }
            set { _IsHilightInLeadParagraph = value; }
        }bool _IsHilightInLeadParagraph = false;

        public int LeadParagraphChars { get; set; }

        public List<Int16> IQLicense
        {
            get { return _IQLicense; }
            set { _IQLicense = value; }
        }List<Int16> _IQLicense = new List<Int16>();

        public string FieldList
        {
            get { return _FieldList; }
            set { _FieldList = value; }
        }string _FieldList = null;

        public bool Equals(SearchNewsRequest other)
        {
            if (this.SearchTerm != other.SearchTerm)
                return false;

            if (this.StartDate != other.StartDate)
                return false;

            if (this.EndDate != other.EndDate)
                return false;

            if (this.Source != other.Source)
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

            if (this.Market != null && other.Market != null)
            {
                if (this.Market.Count() != other.Market.Count())
                    return false;
                this.Market = this.Market.OrderBy(a => a).ToList();
                other.Market = other.Market.OrderBy(a => a).ToList();
                for (int i = 0; i < this.Market.Count(); i++)
                {
                    string obj1 = this.Market.ElementAt(i);
                    string obj2 = other.Market.ElementAt(i);
                    if (obj1 != obj2)
                        return false;
                }
            }
            else if ((this.Market != null && other.Market == null) || (this.Market == null && other.Market != null))
                return false;


            if (this.PublicationCategory != null && other.PublicationCategory != null)
            {
                if (this.PublicationCategory.Count() != other.PublicationCategory.Count())
                    return false;

                this.PublicationCategory = this.PublicationCategory.OrderBy(a => a).ToList();
                other.PublicationCategory = other.PublicationCategory.OrderBy(a => a).ToList();
                for (int i = 0; i < this.PublicationCategory.Count(); i++)
                {
                    int Obj1 = this.PublicationCategory.ElementAt(i);
                    int Obj2 = other.PublicationCategory.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.PublicationCategory != null && other.PublicationCategory == null) || this.PublicationCategory == null && other.PublicationCategory != null)
                return false;


            if (this.NewsCategory != null && other.NewsCategory != null)
            {
                if (this.NewsCategory.Count() != other.NewsCategory.Count())
                    return false;

                this.NewsCategory = this.NewsCategory.OrderBy(a => a).ToList();
                other.NewsCategory = other.NewsCategory.OrderBy(a => a).ToList();
                for (int i = 0; i < this.NewsCategory.Count(); i++)
                {
                    string Obj1 = this.NewsCategory.ElementAt(i);
                    string Obj2 = other.NewsCategory.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.NewsCategory != null && other.NewsCategory == null) || this.NewsCategory == null && other.NewsCategory != null)
                return false;

            if (this.NewsRegion != null && other.NewsRegion != null)
            {
                if (this.NewsRegion.Count() != other.NewsRegion.Count())
                    return false;

                this.NewsRegion = this.NewsRegion.OrderBy(a => a).ToList();
                other.NewsRegion = other.NewsRegion.OrderBy(a => a).ToList();
                for (int i = 0; i < this.NewsRegion.Count(); i++)
                {
                    string Obj1 = this.NewsRegion.ElementAt(i);
                    string Obj2 = other.NewsRegion.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.NewsRegion != null && other.NewsRegion == null) || this.NewsRegion == null && other.NewsRegion != null)
                return false;

            if (this.Country != null && other.Country != null)
            {
                if (this.Country.Count() != other.Country.Count())
                    return false;

                this.Country = this.Country.OrderBy(a => a).ToList();
                other.Country = other.Country.OrderBy(a => a).ToList();
                for (int i = 0; i < this.Country.Count(); i++)
                {
                    string Obj1 = this.Country.ElementAt(i);
                    string Obj2 = other.Country.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.Country != null && other.Country == null) || this.Country == null && other.Country != null)
                return false;

            if (this.Language != null && other.Language != null)
            {
                if (this.Language.Count() != other.Language.Count())
                    return false;

                this.Language = this.Language.OrderBy(a => a).ToList();
                other.Language = other.Language.OrderBy(a => a).ToList();
                for (int i = 0; i < this.Language.Count(); i++)
                {
                    string Obj1 = this.Language.ElementAt(i);
                    string Obj2 = other.Language.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.Language != null && other.Language == null) || this.Language == null && other.Language != null)
                return false;

            if (this.SourceType != null && other.SourceType != null)
            {
                if (this.SourceType.Count() != other.SourceType.Count())
                    return false;

                this.SourceType = this.SourceType.OrderBy(a => a).ToList();
                other.SourceType = other.SourceType.OrderBy(a => a).ToList();
                for (int i = 0; i < this.SourceType.Count(); i++)
                {
                    int Obj1 = this.SourceType.ElementAt(i);
                    int Obj2 = other.SourceType.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.SourceType != null && other.SourceType == null) || this.SourceType == null && other.SourceType != null)
                return false;

            if (this.Genre != null && other.Genre != null)
            {
                if (this.Genre.Count() != other.Genre.Count())
                    return false;

                this.Genre = this.Genre.OrderBy(a => a).ToList();
                other.Genre = other.Genre.OrderBy(a => a).ToList();
                for (int i = 0; i < this.Genre.Count(); i++)
                {
                    string Obj1 = this.Genre.ElementAt(i);
                    string Obj2 = other.Genre.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.Genre != null && other.Genre == null) || this.Genre == null && other.Genre != null)
                return false;

            if (this.lstfacetRange != null && other.lstfacetRange != null)
            {
                if (this.lstfacetRange.Count() != other.lstfacetRange.Count())
                    return false;

                this.lstfacetRange = this.lstfacetRange.OrderBy(a => a).ToList();
                other.lstfacetRange = other.lstfacetRange.OrderBy(a => a).ToList();
                for (int i = 0; i < this.lstfacetRange.Count(); i++)
                {
                    string Obj1 = this.lstfacetRange.ElementAt(i);
                    string Obj2 = other.lstfacetRange.ElementAt(i);
                    if (Obj1 != Obj2)
                        return false;
                }
            }
            else if ((this.lstfacetRange != null && other.lstfacetRange == null) || this.lstfacetRange == null && other.lstfacetRange != null)
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
