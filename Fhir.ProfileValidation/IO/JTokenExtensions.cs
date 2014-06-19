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
using Newtonsoft.Json.Linq;

namespace Fhir.Profiling.IO
{
    internal static class JTokenExtensions
    {
        public static string ElementText(this JToken token)
        {
            if (token is JObject)
            {
                var result = new StringBuilder();

                foreach (var child in ((JContainer)token).ElementChildren())
                    result.Append(child.ElementText());

                return result.ToString();
            }
            else if (token is JValue)
            {
                return token.Type != JTokenType.Null ? ((JValue)token).Value.ToString() : String.Empty;
            }
            else if (token is JProperty)
            {
                return ((JProperty)token).Value.ElementText();
            }
            else
                throw new InvalidOperationException("Don't know how to get text from a JToken of type " + token.GetType().Name);

        }

        public const string PRIMITIVE_PROP_NAME = "(value)";

        
        private static JObject combinePrimitiveWithAppendix(JProperty primitive,JProperty appendix)
        {
            var primitiveProp = new JProperty(PRIMITIVE_PROP_NAME, (JValue)primitive.Value);
            
            if(appendix == null)
                return new JObject(primitiveProp);
            else
            {
 	            return new JObject(primitiveProp, ((JObject)appendix.Value).Properties());
            }
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


        public static IEnumerable<JProperty> ElementChildren(this JProperty prop)
        {
            // At the leaves of the model, we'll find primitive properties named "(value)". They have no children.
            if (prop.Value is JValue && prop.Name == PRIMITIVE_PROP_NAME) yield break;    

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

                if(child.Value is JValue)
                {
                    // If the child is a primitive convert it to an JObject with, a single '(value)' member,
                    // combined with -if present- an appendix member, prefixed by "_"

                    // Look for the "appendix" child with the same name
                    var appendix = children.SingleOrDefault( p => p.Name == "_" + name);
                               
                    // The special "appendix" child *must* be complex...
                    if(appendix != null && !(appendix.Value is JObject))
                        throw new FormatException(String.Format("Found appendix property {0}, but it is not a complex value.", appendix.Name));
                    
                    // Combine both the primitive and the appendix into a single property
                    yield return new JProperty(name, combinePrimitiveWithAppendix(child,appendix));
                }

                else if(child.Value is JObject)
                {
                    yield return child;     // Do nothing, normal child
                }

                else if(isPrimitiveArray(child.Value))
                {                    
                    // If the child is an array of primitives, convert it to multiple JObjects with, a single '(value)' member,
                    // combined with -if present- an appendix member, prefixed by "_"

                    // Look for the "appendix" child with the same name
                    var appendix = children.SingleOrDefault( p => p.Name == "_" + name);

                    // The special "appendix" child *must* be an array of complex members
                    if(appendix != null && !isComplexArray(appendix.Value))
                        throw new FormatException(String.Format("Found appendix property {0}, but it is not an array of complex values.", appendix.Name));

                    var appendixChildren = ((JArray)appendix.Value).Children();

                    var index = 0;
                    throw new NotImplementedException();
                    foreach(var arrayElem in ((JArray)child.Value).Children())
                    {
                        //TODO: detect if size is the same
                        //TODO: combine children
                    }
                }

                // If the property is an Array, return it as sibling properties
                else if (child.Value is JArray)
                {
                    foreach (var elem in prop.Value.Children())
                    {
                        var sibling = new JProperty(name, elem);
                        yield return sibling;
                    }
                }
            }
        }
    }
}
