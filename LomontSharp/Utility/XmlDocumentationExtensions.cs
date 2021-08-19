// Reading XML Documentation at Run-Time
// Bradley Smith - 2010/11/25
// https://www.brad-smith.info/blog/archives/220

// modified and fixed for more cases Chris Lomont 2021

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Lomont.Utility
{
    /// <summary>
    /// Provides extension methods for reading XML comments from reflected members.
    /// Usage:
    ///	Console.WriteLine(typeof(SomeClass).GetMethod("SomeMethod").GetXmlDocumentation());
    /// Console.WriteLine(typeof(SomeClass).GetMethod("SomeMethod").GetParameter("someParam").GetXmlDocumentation());
    /// Console.WriteLine(typeof(SomeClass).GetMethod("SomeMethod").ReturnParameter.GetXmlDocumentation());
    /// </summary>
    public static class XmlDocumentationExtensions
    {
        /// <summary>
        /// Returns the XML documentation (summary tag) for the specified member.
        /// </summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the summary tag for the member.</returns>
        public static string GetXmlDocumentation(this MemberInfo member) =>
            GetXmlDocumentation(member, member.Module.Assembly.GetName().Name + ".xml");

        /// <summary>
        /// Returns the XML documentation (returns/param tag) for the specified parameter.
        /// </summary>
        /// <param name="parameter">The reflected parameter (or return value).</param>
        /// <returns>The contents of the returns/param tag for the parameter.</returns>
        public static string GetXmlDocumentation(this ParameterInfo parameter) =>
            GetXmlDocumentation(parameter, parameter.Member.Module.Assembly.GetName().Name + ".xml");

        #region Implementation
        /// <summary>
        /// Returns the XML documentation (summary tag) for the specified member.
        /// </summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="pathToXmlFile">Path to the XML documentation file.</param>
        /// <returns>The contents of the summary tag for the member.</returns>
        static string GetXmlDocumentation(this MemberInfo member, string pathToXmlFile)
        {
            return GetXmlDocumentation(member,
                GetCached(member.Module.Assembly.GetName().FullName, pathToXmlFile));
        }

        /// <summary>
        /// Returns the XML documentation (returns/param tag) for the specified parameter.
        /// </summary>
        /// <param name="parameter">The reflected parameter (or return value).</param>
        /// <param name="pathToXmlFile">Path to the XML documentation file.</param>
        /// <returns>The contents of the returns/param tag for the parameter.</returns>
        static string GetXmlDocumentation(this ParameterInfo parameter, string pathToXmlFile)
        {
            return GetXmlDocumentation(parameter, 
                GetCached(parameter.Member.Module.Assembly.GetName().FullName, pathToXmlFile)
               );
        }

        /// <summary>
        /// Returns the XML documentation (summary tag) for the specified member.
        /// </summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="xml">XML documentation.</param>
        /// <returns>The contents of the summary tag for the member.</returns>
        static string GetXmlDocumentation(this MemberInfo member, XDocument xml) =>
            xml.XPathEvaluate($"string(/doc/members/member[@name='{GetMemberElementName(member)}']/summary)").ToString()?.Trim();



        /// <summary>
        /// Returns the XML documentation (returns/param tag) for the specified parameter.
        /// </summary>
        /// <param name="parameter">The reflected parameter (or return value).</param>
        /// <param name="xml">XML documentation.</param>
        /// <returns>The contents of the returns/param tag for the parameter.</returns>
        static string GetXmlDocumentation(this ParameterInfo parameter, XDocument xml)
        {
            var st = (parameter.IsRetval || String.IsNullOrEmpty(parameter.Name)) 
                ? $"string(/doc/members/member[@name='{GetMemberElementName(parameter.Member)}']/returns)"
                : $"string(/doc/members/member[@name='{GetMemberElementName(parameter.Member)}']/param[@name='{parameter.Name}'])";

            return xml.XPathEvaluate(st).ToString()?.Trim();
        }

        /// <summary>
        /// Returns the expected name for a member element in the XML documentation file.
        /// </summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The name of the member element.</returns>
        static string GetMemberElementName(MemberInfo member)
        {
            char prefixCode;
            string memberName = (member is Type type)
                ? type.FullName // member is a Type
                : (member.DeclaringType.FullName + "." + member.Name); // member belongs to a Type

            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                    // XML documentation uses slightly different constructor names
                    memberName = memberName.Replace(".ctor", "#ctor");
                    goto case MemberTypes.Method;
                case MemberTypes.Method:
                    prefixCode = 'M';

                    // parameters are listed according to their type, not their name
                    var paramTypesList = String.Join(
                        ",",
                        ((MethodBase) member).GetParameters()
                        .Select(x => x.ParameterType.FullName
                        ).ToArray()
                    );
                    if (!String.IsNullOrEmpty(paramTypesList)) memberName += "(" + paramTypesList + ")";
                    break;

                case MemberTypes.Event:
                    prefixCode = 'E';
                    break;

                case MemberTypes.Field:
                    prefixCode = 'F';
                    break;

                case MemberTypes.NestedType:
                    // XML documentation uses slightly different nested type names
                    memberName = memberName.Replace('+', '.');
                    goto case MemberTypes.TypeInfo;
                case MemberTypes.TypeInfo:
                    prefixCode = 'T';
                    break;

                case MemberTypes.Property:
                    prefixCode = 'P';
                    break;

                default:
                    throw new ArgumentException("Unknown member type", nameof(member));
            }

            // elements are of the form "M:Namespace.Class.Method"
            var str = $"{prefixCode}:{memberName}";
            str = str.Replace("+", "."); // nested types end up as this in xml
            return str;
        }

        /// <summary>
        /// Look up cached value, or load and cache it
        /// </summary>
        /// <param name="key"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        static XDocument GetCached(string key, string path)
        {
            if (!CachedXml.ContainsKey(key))
                CachedXml.Add(key, XDocument.Load(path));
            return CachedXml[key];
        }
        static readonly Dictionary<string, XDocument> CachedXml = new(StringComparer.OrdinalIgnoreCase);



        #endregion

    }
}