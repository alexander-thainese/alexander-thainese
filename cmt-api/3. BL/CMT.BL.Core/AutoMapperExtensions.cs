using AutoMapper;
using System;
using System.Reflection;

namespace CMT.BL.Core
{
    public static class AutoMapperExtensions
    {
        public static IMappingExpression<TSource, TDestination> IgnoreComplexObjects<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            Type sourceType = typeof(TSource);
            PropertyInfo[] destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo destinationProperty in destinationProperties)
            {
                PropertyInfo sourceProperty = sourceType.GetProperty(destinationProperty.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProperty == null || sourceProperty.PropertyType.IsClass)
                {
                    expression.ForMember(destinationProperty.Name, opt => opt.Ignore());
                }
            }

            return expression;
        }

        public static IMappingExpression<TSource, TDestination> IgnoreBinaryProperties<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            PropertyInfo[] sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                if (sourceProperty.PropertyType == typeof(System.Data.Linq.Binary))
                {
                    expression.ForMember(sourceProperty.Name, opt => opt.Ignore());
                }
            }

            return expression;
        }

        public static IMappingExpression<TSource, TDestination> ExpressionDelegate<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression, Func<IMappingExpression<TSource, TDestination>, IMappingExpression<TSource, TDestination>> expressionFunc)
        {
            return expressionFunc(expression);
        }

    }

}
