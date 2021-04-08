using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionVsReflection
{
    public delegate T Func<T>(params object[] args);
    public class ObjectProvider
    {

        private static ConcurrentDictionary<string,Delegate> _mapFunc = new ConcurrentDictionary<string, Delegate>();

        public static T ReflectionCreator<T>(params object[] args)
            where T : class
        {
            return Activator.CreateInstance(typeof(T), args) as T;
        }

        public static Func<T> ExpressionCreator<T>()
        {
            var key = typeof(T).Name;

            if (!_mapFunc.TryGetValue(key, out Delegate result))
            {
                ConstructorInfo ctor = typeof(T).GetConstructors().FirstOrDefault();

                ParameterInfo[] paramsInfo = ctor.GetParameters();

                ParameterExpression param =
                    Expression.Parameter(typeof(object[]), "args");

                Expression[] argsExp =
                    new Expression[paramsInfo.Length];

                for (int i = 0; i < paramsInfo.Length; i++)
                {
                    Expression index = Expression.Constant(i);
                    Type paramType = paramsInfo[i].ParameterType;

                    Expression paramAccessorExp =
                        Expression.ArrayIndex(param, index);

                    Expression paramCastExp =
                        Expression.Convert(paramAccessorExp, paramType);

                    argsExp[i] = paramCastExp;
                }

                NewExpression newExp = Expression.New(ctor, argsExp);

                LambdaExpression lambda = Expression.Lambda(typeof(Func<T>), newExp, param);

                result = lambda.Compile();

                _mapFunc.GetOrAdd(key, result);
            }


            return (Func<T>)result;
        }
    }
}