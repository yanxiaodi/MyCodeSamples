using CodeGeneratorDemo.ReflectionDemo.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CodeGeneratorDemo.ReflectionDemo
{
    public class ReflectionHelper
    {
        public static List<Type> GetAvailableSpeakers()
        {
            // You can also use AppDomain.CurrentDomain.GetAssemblies() to load all assemblies in the current domain.
            // Get the specified assembly.
            var assembly =
                Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    "CodeGeneratorDemo.ReflectionDemo.Core.dll"));
            // Find all the types in the assembly.
            var types = assembly.GetTypes();
            // Apply the filter to find the implementations of ISayHello interface.
            var result = types.Where(x => x.IsClass && typeof(ISpeaker).IsAssignableFrom(x)).ToList();
            // Or you can use types.Where(x => x.IsClass && x.GetInterfaces().Contains(typeof(ISpeaker))).ToList();
            return result;
        }
    }
}
