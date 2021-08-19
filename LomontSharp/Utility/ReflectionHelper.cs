using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lomont.Algorithms;

namespace Lomont.Utility
{
    public static class ReflectionHelper
    {
        // get XML comments from items
        // see https://stackoverflow.com/questions/15602606/programmatically-get-summary-comments-at-runtime
        // https://swharden.com/blog/2021-01-31-xml-doc-name-reflection/
        // https://www.brad-smith.info/blog/archives/220
        // moved to own file as extension methods


        /// <summary>
        /// Get all types with custom attributes
        /// Call (usually) with Assembly.GetExecutingAssembly(), typeof(attrib)
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesWithAttribute(Assembly assembly, Type attributeType)
        {

            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(attributeType, true).Length > 0)
                    yield return type;
            }
        }

        public static IEnumerable<Type> GetTypesWithAttribute(Type attributeType)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            foreach (var t in GetTypesWithAttribute(assembly, attributeType))
                yield return t;
        }

        /// <summary>
        /// Look through currently executing assemblies, look for similar named classes and/or methods, report on everything
        /// Dumps to console
        /// Useful for finding things to merge in larger codebases
        /// </summary>
        public static void SeekSimilarText(double thresh = 0.70, bool types = true, bool methods = true, bool prop = true)
        {
            HashSet<string> typeNames = new();
            HashSet<string> methodNames = new();
            HashSet<string> propNames = new();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // above is too many items, this too few?
            //assemblies = new Assembly[]
            //{
            //    //Assembly.GetEntryAssembly()
            //    //Assembly.GetExecutingAssembly()
            //    Assembly.GetAssembly(typeof(ReflectionHelper))
            //};

            foreach (var assembly in assemblies)
            {
                if (!assembly.Location.StartsWith("C:\\Users"))
                    continue;
                if (types)
                {
                    foreach (var t in assembly.DefinedTypes)
                    {
                        typeNames.Add(t.Name);
                        Console.WriteLine($"Type {t.Name}");
                        if (methods)
                        {
                            foreach (var m in t.DeclaredMethods)
                            {
                                methodNames.Add(m.Name);
                                Console.WriteLine($"   Method {m.Name}");
                            }
                        }
                        if (prop)
                        {
                            foreach (var p in t.DeclaredProperties)
                            {
                                propNames.Add(p.Name);
                                Console.WriteLine($"   Prop {p.Name}");
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"{typeNames.Count} types, {methodNames.Count} methods");

            Func(typeNames.ToList());
            Func(methodNames.ToList());
            Func(propNames.ToList());

            // given the strings, group them
            void Func(List<string> t)
            {
                var c = t.Count;
                for (var i =0; i < c; ++i)
                for (var j = i + 1; j < c; ++j)
                {
                    var(s1,s2)=(t[i],t[j]);
                    var dist = StringDistance.LevenshteinSimilarity(t[i], t[j]);
                    if (dist > thresh)
                    {
                        Console.WriteLine($"Similar {t[i]} ~ {t[j]}  {dist:F3}");
                    }
                }
            }
            //todo


        }

        /// <summary>
        /// Convert list of strings into a list of parameter objects for a specific function call
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static object[] MakeCallParameters(Type type, string methodName, List<string> inputs)
        {

            List<object> ans = new();

            var mi = type.GetMethod(methodName);
            if (mi == null)
                return ans.ToArray();

            int paramIndex = 0;
            string Next()
            {
                return inputs[paramIndex++];
            }

            foreach (var parameterInfo in mi.GetParameters())
            {
                var pType = parameterInfo.ParameterType; 

                if (TerminalType(pType))
                    ans.Add(Parse(pType, Next()));
                else
                {
                    // for now, only one level deep - figure out recursion later

                    // the item to fill in
                    var item = Activator.CreateInstance(pType);

                    foreach (PropertyInfo propertyInfo in pType.GetProperties())
                    {
                        propertyInfo.SetValue(item, Parse(propertyInfo.PropertyType, Next()));
                    }
                    ans.Add(item);
                }

            }
            return ans.ToArray();
        }

        /// <summary>
        /// Given a type, and a method name, get all parameter names and types,
        /// recursing down paths to simple types
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<(string Name, Type Type, string DefaultValue, string XmlComment)> DecomposeToBaseTypes(Type type, string methodName)
        {
            var mi = type.GetMethod(methodName);
            if (mi == null) yield break;

            foreach (var parameterInfo in mi.GetParameters())
            {
                var pName = parameterInfo.Name;
                var pType = parameterInfo.ParameterType; // recurse on type if not terminal
                var pDefault = parameterInfo.HasDefaultValue ? parameterInfo.DefaultValue.ToString(): "";

                if (TerminalType(pType))
                {
                    var xmlComment = parameterInfo.GetXmlDocumentation();
                    yield return (pName, pType, pDefault, xmlComment);
                }
                else 
                {
                    foreach (var val in RecurseType(pType, parameterInfo.Name))
                        yield return val;
                }
            }

            IEnumerable<(string Name, Type Type, string DefaultValue, string XmlComment)> RecurseType(Type type, string path)
            {
                foreach (PropertyInfo propertyInfo in type.GetProperties())
                {
                    var pName = propertyInfo.Name;
                    var pType = propertyInfo.PropertyType;
                    var path2 = path + "." + pName;
                    if (pType.Name.EndsWith("[]")) // array type
                        path2 += "[]";
                    if (TerminalType(pType))
                    {
                        var xmlComment = propertyInfo.GetXmlDocumentation();
                        yield return (path2, pType, "", xmlComment);
                    }
                    else
                    {
                        foreach (var p in RecurseType(pType, path2))
                            yield return p;
                    }
                }
            }
        }

        // see if done - which is when type hits base types or arrays of base types
        /// <summary>
        /// Types we consider terminal for now
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static bool TerminalType(Type type)
        {
            if (type.FullName == "System.String[]")
                return true;
            if (type == typeof(string))
                return true;
            if (type == typeof(bool))
                return true;
            if (type == typeof(int))
                return true;
            if (type == typeof(long))
                return true;
            if (type == typeof(double))
                return true;
            if (type == typeof(float))
                return true;
            return false;
        }

        /// <summary>
        /// Parse string to terminal type
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static object Parse(Type type, string text)
        {
            if (type == typeof(int))
                return Int32.Parse(text);
            if (type == typeof(long))
                return Int64.Parse(text);
            if (type == typeof(float))
                return float.Parse(text);
            if (type == typeof(double))
                return double.Parse(text);
            if (type == typeof(string))
                return text;
            if (type == typeof(bool))
                return text.ToLower() == "true";
            if (type.FullName == "System.String[]")
                return text.Split(';');
            throw new NotImplementedException($"Type {type} not supported in parse");
        }


    }
}
