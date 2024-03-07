using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Common.Utilities
{
    public class ExpressionUtilities
    {
        public static Expression<Func<T, object>> ApplyNewRoot<T, K>(Expression<Func<K, object>> i, string oldRootPropertyName)
        {
            var expr = i.Body;
            var param1 = Expression.Parameter(typeof(T), "p");

            var coreProj = typeof(T).GetProperty(oldRootPropertyName);
            var coreProjAccess = Expression.MakeMemberAccess(param1, coreProj);

            var list = new List<Tuple<Type, string>>();

            var isMethod = false;
            MethodInfo methodInfo = null;
            Expression arg = null;

            if (expr is MethodCallExpression)
            {
                isMethod = true;
                var methExpr = expr as MethodCallExpression;
                expr = methExpr.Arguments[0];
                methodInfo = methExpr.Method;
                arg = methExpr.Arguments[1];
            }

            while (expr is MemberExpression)
            {
                var memExpr = expr as MemberExpression;
                list.Add(new Tuple<Type, string>(memExpr.Member.DeclaringType, memExpr.Member.Name));
                expr = memExpr.Expression;
            }

            list.Reverse();
            MemberExpression access = null;
            foreach (var item in list)
            {
                var prop = item.Item1.GetProperty(item.Item2);

                if (access == null)
                    access = Expression.MakeMemberAccess(coreProjAccess, prop);
                else
                    access = Expression.MakeMemberAccess(access, prop);
            }

            if (isMethod)
            {
                var newExpr = Expression.Call(methodInfo, access, arg);
                return Expression.Lambda<Func<T, object>>(newExpr, param1);
            }
            else
                return Expression.Lambda<Func<T, object>>(access, param1);
        }
    }
}
