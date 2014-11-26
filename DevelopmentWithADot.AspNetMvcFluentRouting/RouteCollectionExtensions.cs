using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace DevelopmentWithADot.AspNetMvcFluentRouting
{
	public static class RouteCollectionExtensions
	{
		public static _Route Action<TController>(this RouteCollection routes, Expression<Func<TController, Object>> action, String url = "") where TController : IController, new()
		{
			MethodCallExpression method = action.Body as MethodCallExpression;

			_Route route = new _Route(routes, typeof(TController), method.Method);
			route.SetUrl(url);

			return (route);
		}

		public static _Route ActionWithDefaultParameters<TController>(this RouteCollection routes, Expression<Func<TController, Object>> action, String url = "") where TController : IController, new()
		{
			_Route route = Action<TController>(routes, action, url);
			MethodCallExpression method = action.Body as MethodCallExpression;
			ParameterInfo[] parameters = method.Method.GetParameters();
			Expression[] arguments = method.Arguments.ToArray();

			for (Int32 i = 0; i < parameters.Length; ++i)
			{
				if (arguments[i] is ConstantExpression)
				{
					route.AddDefaultParameterValue(parameters[i].Name, (arguments[i] as ConstantExpression).Value);
				}
				else if (arguments[i] is MemberExpression)
				{
					route.AddDefaultParameterValue(parameters[i].Name, Expression.Lambda(arguments[i]).Compile().DynamicInvoke());
				}
				else if (arguments[i] is MethodCallExpression)
				{
					route.AddDefaultParameterValue(parameters[i].Name, (arguments[i] as MethodCallExpression).Method.Invoke((arguments[i] as MethodCallExpression).Object, (arguments[i] as MethodCallExpression).Arguments.ToArray()));
				}
			}

			return (route);
		}
	}

}