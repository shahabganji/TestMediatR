using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TestMediatr.Commands;

namespace TestMediatr
{
    public static class HandleRegistration
    {

        public static void AddHandlers(this IServiceCollection services)
        {
            var handlerTypes = typeof(Startup).Assembly.GetTypes()
                .Where(x => x.GetInterfaces().Any(IsHandlerInterface))
                .Where(x => x.Name.EndsWith("Handler"))
                .ToList();

            foreach (Type handler in handlerTypes)
            {
                services.AddHandler( handler);
            }
        }

        private static void AddHandler<T>(this IServiceCollection services)
        {
            var type = typeof(T);
            services.AddHandler(type);
        }
        
        private static void AddHandler(this IServiceCollection services, Type type)
        {
            object[] attributes = type.GetCustomAttributes(false);

            var pipeline = attributes
                .Select(ToDecorator)
                .Concat(new[] { type })
                .Reverse()
                .ToList();

            var interfaceType = type.GetInterfaces().Single(IsHandlerInterface);
            Func<IServiceProvider, object> factory = BuildPipeline(pipeline, interfaceType);

            services.AddScoped(interfaceType, factory);

        }

        private static Func<IServiceProvider, object> BuildPipeline(List<Type> pipeline, Type interfaceType)
        {
            List<ConstructorInfo> constructors = pipeline
                .Select(x =>
                {
                    Type type = x.IsGenericType ? x.MakeGenericType(interfaceType.GenericTypeArguments) : x;
                    return type.GetConstructors().Single();
                })
                .ToList();

            object Factory(IServiceProvider provider)
            {
                object current = null;

                foreach (ConstructorInfo constructor in constructors)
                {
                    List<ParameterInfo> parameterInfos = constructor.GetParameters().ToList();

                    object[] parameters = GetParameters(parameterInfos, current, provider);

                    current = constructor.Invoke(parameters);
                }

                return current;
            }

            return Factory;

        }

        private static object[] GetParameters(List<ParameterInfo> parameterInfos, object current, IServiceProvider provider)
        {
            var result = new object[parameterInfos.Count];

            for (int i = 0; i < parameterInfos.Count; i++)
            {
                result[i] = GetParameter(parameterInfos[i], current, provider);
            }

            return result;
        }

        private static object GetParameter(ParameterInfo parameterInfo, object current, IServiceProvider provider)
        {
            var parameterType  = parameterInfo.ParameterType;

            if (IsHandlerInterface(parameterType))
                return current;

            object service = provider.GetService(parameterType);
            if (service != null)
                return service;
            
            throw new ArgumentException($"Type {parameterType} not found.");
        }

        private static Type ToDecorator(object attribute)
        {
            var type = attribute.GetType();

            if (type == typeof(AuditAttribute))
                return typeof(PingCommand.AuditDecorator<,>);
            
            if (type == typeof(DatabaseAttribute))
                return typeof(PingCommand.DatabaseDecorator<,>);
            
            if (type == typeof(PongAttribute))
                return typeof(PingCommand.PongDecorator);
            
            throw  new ArgumentException(attribute.ToString());
        }

        private static bool IsHandlerInterface(Type type)
        {
            if (!type.IsGenericType)
                return false;

            Type typeDefinition = type.GetGenericTypeDefinition();

            return typeDefinition == typeof(IRequestHandler<,>);
        }
        
    }
}
