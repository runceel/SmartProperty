﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Smart.Bindings.Internals
{
    /// <summary>
    /// Accessor Cache
    /// </summary>
    /// <typeparam name="TType">The type of the type.</typeparam>
    static class AccessorCache<TType>
    {
        private static readonly Dictionary<string, Delegate> getCache = new Dictionary<string, Delegate>();
        private static readonly Dictionary<string, Delegate> setCache = new Dictionary<string, Delegate>();

        /// <summary>
        /// Lookups the get.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertySelector">The property selector.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static Func<TType, TProperty> LookupGet<TProperty>(Expression<Func<TType, TProperty>> propertySelector, out string propertyName)
        {
            propertyName = GetPropertyName(propertySelector);
            Delegate accessor;

            lock (getCache)
            {
                if (!getCache.TryGetValue(propertyName, out accessor))
                {
                    accessor = propertySelector.Compile();
                    getCache.Add(propertyName, accessor);
                }
            }

            return (Func<TType, TProperty>)accessor;
        }

        private static string GetPropertyName<TProperty>(Expression<Func<TType, TProperty>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
            {
                var unaryExpression = propertySelector.Body as UnaryExpression;
                if (unaryExpression == null) { throw new ArgumentException(nameof(propertySelector)); }
                memberExpression = unaryExpression.Operand as MemberExpression;
                if (memberExpression == null) { throw new ArgumentException(nameof(propertySelector)); }
            }

            return memberExpression.Member.Name;
        }

        /// <summary>
        /// Lookups the set.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertySelector">The property selector.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static Action<TType, TProperty> LookupSet<TProperty>(Expression<Func<TType, TProperty>> propertySelector, out string propertyName)
        {
            propertyName = GetPropertyName(propertySelector);
            Delegate accessor;

            lock (setCache)
            {
                if (!setCache.TryGetValue(propertyName, out accessor))
                {
                    accessor = CreateSetAccessor(propertySelector);
                    setCache.Add(propertyName, accessor);
                }
            }

            return (Action<TType, TProperty>)accessor;
        }

        private static Delegate CreateSetAccessor<TProperty>(Expression<Func<TType, TProperty>> propertySelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
            var selfParameter = Expression.Parameter(typeof(TType), "self");
            var valueParameter = Expression.Parameter(typeof(TProperty), "value");
            var body = Expression.Assign(Expression.Property(selfParameter, propertyInfo), valueParameter);
            var lambda = Expression.Lambda<Action<TType, TProperty>>(body, selfParameter, valueParameter);
            return lambda.Compile();
        }
    }
}
