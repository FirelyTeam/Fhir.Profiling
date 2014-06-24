/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace Fhir.Profiling.IO
{
    internal static class JTokenExtensions
    {
        private const string PRIMITIVE_PROP_NAME = "(value)";
        private const string RESOURCE_TYPE_PROP_NAME = "resourceType";

        public static JProperty AsResourceRoot(this JObject root)
        {
            if (root[RESOURCE_TYPE_PROP_NAME] != null)
            {
                var name = root[RESOURCE_TYPE_PROP_NAME] as JValue;

                if (name == null || name.Type != JTokenType.String)
                    throw new FormatException("Found 'resourceType' property, but it is not a primitive string");

                return new JProperty(name.ToString(), root);
            }
            else
                throw new FormatException("Cannot parse this resource, the 'resourceType' property is missing to indicate the type of resource");
        }
     
        public static string ElementText(this JProperty prop)
        {
            if (prop.IsValueProperty())
            {
                var primitive = (JValue)prop.Value;

                // We accept four primitive json types, convert them to the correct xml string representations
                if (primitive.Type == JTokenType.Integer)
                    return XmlConvert.ToString((Int64)primitive.Value);
                else if (primitive.Type == JTokenType.Float)
                    return XmlConvert.ToString((Decimal)primitive.Value);
                else if (primitive.Type == JTokenType.Boolean)
                    return XmlConvert.ToString((bool)primitive.Value);
                else if (primitive.Type == JTokenType.String)
                    return (string)primitive.Value;
                else if (primitive.Type == JTokenType.Null)
                    return "(null)";
                else
                    throw new FormatException("Only integer, float, boolean and string primitives are allowed in FHIR Json");
            }
            else if (prop.Value is JObject)
            {
                var result = new StringBuilder();

                foreach (var child in prop.ElementChildren())
                    result.Append(child.ElementText());

                return result.ToString();
            }
            else
                throw new InvalidOperationException("Don't know how to get text from a JToken of type " + prop.GetType().Name);
        }


        public static bool IsValueProperty(this JProperty prop)
        {
            return prop.Name == PRIMITIVE_PROP_NAME;
        }

        public static bool IsNullValueProperty(this JProperty prop)
        {
            return IsValueProperty(prop) && prop.Value.Type == JTokenType.Null;
        }

        public static JValue PrimitivePropertyValue(this JProperty token)
        {
            if (token.Value is JObject)
            {
                var obj = (JObject)token.Value;
                var prim = obj.Properties().Single(p => p.Name == PRIMITIVE_PROP_NAME);
                if (prim.IsValueProperty())
                {
                    return (JValue)prim.Value;
                }
            }

            throw new ArgumentException("Property is not a JObject representing a primitive value", "token");
        }

        public static IEnumerable<JProperty> ElementChildren(this JProperty prop)
        {
            // At the leaves of the model, we'll find primitive properties named "(value)". They have no children.
            if (prop.IsValueProperty()) yield break;    

            // Otherwise, property MUST be a complex object, since we translate primitives
            // to objects with a value property + extensions + id, and thus, this function may never
            // be called with anything else
            var parent = prop.Value as JObject;
            if(parent == null) throw new InvalidOperationException("ElementChildren expects a property that's either a JValue named '(value)' or a JObject");

            // Expand the list once, since we need to scan it anyway, and need to rescan it in some cases
            var children = parent.Properties().ToList();

            foreach(var child in children)
            {
                var name = child.Name;

                if (name.StartsWith("_")) continue;     // Skip, appendix members will be included with their non-"_" part
                if (name == RESOURCE_TYPE_PROP_NAME) continue;      // Skip, has been used as the root name

                if (child.Value is JValue)
                {
                    // Special case, if this is a primitive value property, don't expand it again, just add it as a child
                    // A primitive value property is a child that was a Json primitive and got turned into a complex type with
                    // a value member (see else)
                    if (child.IsValueProperty())
                        yield return child;
                    else
                    {
                        // If the child is a primitive convert it to an JObject with, a single '(value)' member,
                        // combined with -if present- an appendix member, prefixed by "_"

                        // Look for the "appendix" child with the same name
                        var appendix = children.SingleOrDefault(p => p.Name == "_" + name);
                        JObject appendixElement = null;

                        // The special "appendix" child *must* be complex...
                        if (appendix != null)
                        {
                            appendixElement = appendix.Value as JObject;
                            if (appendixElement == null)
                                throw new FormatException(String.Format("Found appendix property {0}, but it is not a complex value.", appendix.Name));
                        }

                        // Combine both the primitive and the appendix into a single property
                        yield return new JProperty(name, combinePrimitiveWithAppendix(((JValue)child.Value), appendixElement));
                    }
                }

                else if (child.Value is JObject)
                {
                    yield return child;     // Do nothing, normal child
                }

                else if (isPrimitiveArray(child.Value))
                {
                    // If the child is an array of primitives, convert it to multiple JObjects with, a single '(value)' member,
                    // combined with -if present- an appendix member, prefixed by "_"

                    // Look for the "appendix" child with the same name
                    var appendix = children.SingleOrDefault(p => p.Name == "_" + name);
                    List<JObject> appendixElements = appendix != null ? convertAppendixArray(appendix) : null;

                    var elements = ((JArray)child.Value).Children().ToList();

                    // Arrays should be a 1-to-1 mapping, so the same size
                    if (appendixElements != null && elements.Count != appendixElements.Count)
                        throw new FormatException(String.Format("The appendix array for property {0} does have the same number of elements", child.Name));

                    for (var index = 0; index < elements.Count; index++)
                    {
                        var primitive = (JValue)elements[index];
                        // Combine both the primitive and the appendix into a single property
                        if (appendixElements == null)
                            yield return new JProperty(name, combinePrimitiveWithAppendix(primitive, null));
                        else
                            yield return new JProperty(name, combinePrimitiveWithAppendix(primitive, appendixElements[index]));
                    }
                }

                // If the property is an Array, return it as sibling properties
                else if (child.Value is JArray)
                {
                    foreach (var elem in child.Value.Children())
                    {
                        var sibling = new JProperty(name, elem);
                        yield return sibling;
                    }
                }

                else
                    throw new FormatException(String.Format("Encountered a property {0} of unexpected type {1} in JObject", name, child.Value.GetType().Name));
            }
        }


        private static JObject combinePrimitiveWithAppendix(JValue primitive, JObject appendix)
        {
            var primitiveProp = new JProperty(PRIMITIVE_PROP_NAME, primitive);

            if (appendix == null)
                return new JObject(primitiveProp);
            else
            {
                return new JObject(primitiveProp, appendix.Properties());
            }
        }


        private static List<JObject> convertAppendixArray(JProperty prop)
        {
            var appendixArray = prop.Value as JArray;
            if (appendixArray == null)
                throw new FormatException(String.Format("Found appendix property {0}, but it is not an array", prop.Name));

            var result = new List<JObject>();

            foreach (var element in appendixArray.Children())
            {
                if (element is JObject)
                    result.Add((JObject)element);
                else if (element is JValue && element.Type == JTokenType.Null)
                    result.Add((JObject)null);
                else
                    throw new FormatException(String.Format("Found appendix property {0}, but it contains an element that is not complex or null", prop.Name));
            }

            return result;
        }

        private static bool isPrimitiveArray(JToken value)
        {
            var arr = value as JArray;
            if (arr != null)
            {
                return arr.Children().All(c => c is JValue);
            }

            return false;
        }

        private static bool isComplexArray(JToken value)
        {
            var arr = value as JArray;
            if (arr != null)
            {
                return arr.Children().All(c => c is JObject);
            }

            return false;
        }

    }
}
