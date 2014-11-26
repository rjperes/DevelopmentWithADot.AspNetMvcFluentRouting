using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace DevelopmentWithADot.AspNetMvcFluentRouting
{
	public sealed class _Route
	{
		private readonly RouteCollection routes;

		public _Route(RouteCollection routes, Type controllerType, MethodInfo actionMethod)
		{
			this.routes = routes;
			this.ControllerType = controllerType;
			this.Action = actionMethod;
			this.Defaults = new RouteValueDictionary();
			this.Constraints = new RouteValueDictionary();
			this.Name = String.Concat(controllerType.Name.Remove(controllerType.Name.LastIndexOf("Controller")), "_", actionMethod.Name);
			this.Url = String.Join("/", actionMethod.GetParameters().Select(x => String.Concat("{", x.Name, "}")));
			this.RouteHandler = new MvcRouteHandler();
		}

		public String Url
		{
			get;
			private set;
		}

		public String Name
		{
			get;
			private set;
		}

		public IRouteHandler RouteHandler
		{
			get;
			private set;
		}

		public RouteValueDictionary Defaults
		{
			get;
			private set;
		}

		public RouteValueDictionary Constraints
		{
			get;
			private set;
		}

		public Type ControllerType
		{
			get;
			private set;
		}

		public MethodInfo Action
		{
			get;
			private set;
		}

		public _Route SetRouteHandler<TRouteHandler>() where TRouteHandler : MvcRouteHandler, new()
		{
			this.RouteHandler = new TRouteHandler();

			return (this);
		}

		public _Route SetRouteHandler(MvcRouteHandler routeHandler)
		{
			if (routeHandler != null)
			{
				this.RouteHandler = routeHandler;
			}

			return (this);
		}

		public _Route SetUrl(String url)
		{
			if (String.IsNullOrWhiteSpace(url) == false)
			{
				this.Url = url;
			}

			return (this);
		}

		public _Route SetName(String name)
		{
			this.Name = name;

			return (this);
		}

		public _Route AddDefaultParameterValue(String parameterName, Object value)
		{
			this.Defaults[parameterName] = value;

			return (this);
		}

		public _Route AddDefaultParameterValues()
		{
			foreach (ParameterInfo parameter in this.Action.GetParameters())
			{
				if ((parameter.IsOptional == true) && ((parameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault))
				{
					this.Defaults[parameter.Name] = parameter.DefaultValue;
				}
			}

			return (this);
		}

		public _Route SetOptionalParameter(String parameterName)
		{
			this.Defaults[parameterName] = UrlParameter.Optional;

			return (this);
		}

		public Route Map()
		{
			String url = String.Concat(this.ControllerType.Name.Remove(this.ControllerType.Name.LastIndexOf("Controller")), "/", this.Url);
			Route route = this.routes.MapRoute(this.Name, url, null, null);

			if (route.RouteHandler != this.RouteHandler)
			{
				route.RouteHandler = this.RouteHandler;
			}

			foreach (KeyValuePair<String, Object> @default in this.Defaults)
			{
				route.Defaults[@default.Key] = @default.Value;
			}

			foreach (KeyValuePair<String, Object> constraint in this.Constraints)
			{
				route.Constraints[constraint.Key] = constraint.Value;
			}

			route.Defaults["Controller"] = this.ControllerType.Name.Remove(this.ControllerType.Name.LastIndexOf("Controller"));
			route.Defaults["Action"] = this.Action.Name;

			return (route);
		}

		public _Route AddConstraints(Object constraints)
		{
			PropertyDescriptorCollection props = TypeDescriptor.GetProperties(constraints);

			foreach (PropertyDescriptor prop in props)
			{
				Object value = prop.GetValue(constraints);

				if (value is String)
				{
					this.AddConstraint(prop.Name, value as String);
				}
				else if (value is IRouteConstraint)
				{
					this.AddConstraint(prop.Name, value as IRouteConstraint);
				}
			}

			return (this);
		}

		public _Route AddConstraint<TRouteConstraint>() where TRouteConstraint : IRouteConstraint, new()
		{
			return (this.AddConstraint(Guid.NewGuid().ToString(), new TRouteConstraint()));
		}

		public _Route AddConstraint(IRouteConstraint constraint)
		{
			return (this.AddConstraint(Guid.NewGuid().ToString(), constraint));
		}

		public _Route AddConstraint<TRouteConstraint>(String parameterName) where TRouteConstraint : IRouteConstraint, new()
		{
			return (this.AddConstraint(parameterName, new TRouteConstraint()));
		}

		public _Route AddConstraint(String parameterName, String regularExpression)
		{
			this.Constraints[parameterName] = regularExpression;

			return (this);
		}

		public _Route AddConstraint(String parameterName, IRouteConstraint constraint)
		{
			this.Constraints[parameterName] = constraint;

			return (this);
		}
	}
}