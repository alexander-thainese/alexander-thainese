using System;
using System.Linq;
using System.Linq.Expressions;

namespace CMT.BL.Core
{
    public class PredicateExpressionVisitor<TSource, TTarget> : ExpressionVisitor
        where TSource : class
        where TTarget : class
    {
        public ParameterExpression NewParameterExp { get; private set; }

        public PredicateExpressionVisitor(ParameterExpression newParameterExp)
        {
            NewParameterExp = newParameterExp;
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            return NewParameterExp;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.Member.DeclaringType == typeof(TSource) || (memberExpression.Member.DeclaringType.IsAssignableFrom(typeof(TSource)) && memberExpression.Expression.Type == typeof(TSource)))
            {
                return Expression.MakeMemberAccess(Visit(memberExpression.Expression),
                   typeof(TTarget).GetMember(memberExpression.Member.Name).FirstOrDefault());
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            Expression left = Visit(expression.Left);
            Expression right = Visit(expression.Right);
            Expression conversion = Visit(expression.Conversion);

            if ((left == expression.Left) && (right == expression.Right) && (conversion == expression.Conversion))
            {
                return expression;
            }

            if ((expression.NodeType == ExpressionType.Coalesce) && (expression.Conversion != null))
            {
                return Expression.Coalesce(left, right, conversion as LambdaExpression);
            }

            if (IsNullableType(left.Type) && !IsNullableType(right.Type))
            {
                right = Expression.Convert(right, typeof(Nullable<>).MakeGenericType(right.Type));
            }
            else if (!IsNullableType(left.Type) && IsNullableType(right.Type))
            {
                left = Expression.Convert(left, typeof(Nullable<>).MakeGenericType(left.Type));
            }
            else
            {
                return base.VisitBinary(expression);
            }

            return Expression.MakeBinary(expression.NodeType, left, right, expression.IsLiftedToNull, expression.Method);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return node.Update(Visit(node.Operand));
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}
