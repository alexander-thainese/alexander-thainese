using System;
using System.Linq.Expressions;

namespace CMT.BL.Core
{
    public static class PredicateConverter
    {
        public static Expression<Func<TTarget, bool>> Convert<TSource, TTarget>(Expression<Func<TSource, bool>> predicate)
            where TSource : class
            where TTarget : class
        {
            PredicateExpressionVisitor<TSource, TTarget> visitor = new PredicateExpressionVisitor<TSource, TTarget>(Expression.Parameter(typeof(TTarget), predicate.Parameters[0].Name));
            Expression<Func<TTarget, bool>> newPredicate = Expression.Lambda<Func<TTarget, bool>>(visitor.Visit(predicate.Body), visitor.NewParameterExp);

            return newPredicate;
        }
    }
}
