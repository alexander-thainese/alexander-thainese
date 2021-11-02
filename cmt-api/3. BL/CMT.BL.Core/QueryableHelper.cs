using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace CMT.BL.Core
{
    public static class QueryableHelper
    {
        public static IQueryable<T> Apply<T>(IQueryable<T> query, string keyFieldName, string searchText, string[] searchFields, Dictionary<string, object> columnFilters, string sort, string order, int offset, int limit, out int totalRowCount)
        {
            return (IQueryable<T>)Apply((IQueryable)query, keyFieldName, searchText, searchFields, columnFilters, sort, order, offset, limit, out totalRowCount);
        }

        public static IQueryable Apply(IQueryable query, string keyFieldName, string searchText, string[] searchFields, Dictionary<string, object> columnFilters, string sort, string order, int offset, int limit, out int totalRowCount)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (string.IsNullOrEmpty(keyFieldName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(keyFieldName));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit));
            }

            query = ApplyFilters(query, searchText, searchFields, columnFilters);
            query = ApplyOrdering(query, keyFieldName, sort, order);
            return ApplyPaging(query, offset, limit, out totalRowCount);
        }

        public static IQueryable<T> ApplyFilters<T>(IQueryable<T> query, string searchText, string[] searchFields, Dictionary<string, object> columnFilters)
        {
            return (IQueryable<T>)ApplyFilters((IQueryable)query, searchText, searchFields, columnFilters);
        }

        public static IQueryable ApplyFilters(IQueryable query, string searchText, string[] searchFields, Dictionary<string, object> columnFilters)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (!string.IsNullOrEmpty(searchText) && searchFields != null && searchFields.Any())
            {
                List<string> conditions = new List<string>();

                foreach (string fieldName in searchFields)
                {
                    conditions.Add(string.Format("{0}.Contains(@0)", fieldName));
                }

                query = query.Where(string.Join(" or ", conditions.ToArray()), searchText);
            }

            if (columnFilters != null && columnFilters.Any())
            {
                foreach (KeyValuePair<string, object> filter in columnFilters)
                {
                    if (filter.Value is DateTime)
                    {
                        DateTime date = (DateTime)filter.Value;
                        query = query.Where(string.Format("{0} >= DateTime(@0, @1, @2, 0, 0, 0)", filter.Key), date.Year, date.Month, date.Day);
                        query = query.Where(string.Format("{0} <= DateTime(@0, @1, @2, 23, 59, 59)", filter.Key), date.Year, date.Month, date.Day);
                    }
                    else if (filter.Value is int || filter.Value is decimal || filter.Value is float)
                    {
                        query = query.Where(string.Format("{0} = @0", filter.Key), filter.Value);
                    }
                    else if (filter.Value is Guid)
                    {
                        query = query.Where(string.Format("{0}.Equals(@0)", filter.Key), filter.Value);
                    }
                    else
                    {
                        query = query.Where(string.Format("{0}.Contains(@0)", filter.Key), filter.Value);
                    }
                }
            }

            return query;
        }

        public static IQueryable<T> ApplyOrdering<T>(IQueryable<T> query, string keyFieldName, string sort, string order)
        {
            return (IQueryable<T>)ApplyOrdering((IQueryable)query, keyFieldName, sort, order);
        }

        public static IQueryable ApplyOrdering(IQueryable query, string keyFieldName, string sort, string order)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (string.IsNullOrEmpty(keyFieldName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(keyFieldName));
            }

            string sortFieldName = sort ?? keyFieldName;
            string ordering = string.Format("{0} {1}", sortFieldName, order);

            query = query.OrderBy(ordering);

            return query;
        }

        public static IQueryable<T> ApplyPaging<T>(IQueryable<T> query, int offset, int limit, out int totalRowCount)
        {
            return (IQueryable<T>)ApplyPaging((IQueryable)query, offset, limit, out totalRowCount);
        }

        public static IQueryable ApplyPaging(IQueryable query, int offset, int limit, out int totalRowCount)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit));
            }

            IQueryable result = query.Skip(offset).Take(limit);
            totalRowCount = query.Count();

            return result;
        }
    }

}
