using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace khahn.common.entity.fetchExpressions
{
    public class EntityPageRequest<T>
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }

        public int Skip
        {
            get { return PageSize * (PageNo - 1); }
        }

        public IEnumerable<T> Page { get; set; }

        public Expression<Func<T, bool>> WhereFilter { get; set; }
        private ICollection<SortBy> _sortFields;

        public ICollection<SortBy> SortFields
        {
            get { return _sortFields ?? (_sortFields = new[] {SortBy.DefaultSortBy}); }
            set { _sortFields = value; }
        }
    }

    public class SortBy
    {
        public static readonly SortBy DefaultSortBy = new SortBy("", true);

        public SortBy(string field, bool @ascending)
        {
            Field = field;
            Ascending = @ascending;
        }

        public string Field { get; set; }
        public bool Ascending { get; set; }
    }
}