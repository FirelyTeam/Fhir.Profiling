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
using Fhir.IO;

namespace Fhir.Profiling
{
    public class ProfileBuilder
    {
        private Profile profile = new Profile();

        public void Add(IEnumerable<ValueSet> valuesets)
        {
            profile.Add(valuesets);
        }

        public void Add(List<Structure> structures)
        {
            profile.Add(structures);
        }

        private void _linkBindings()
        {
            foreach (Element element in profile.Elements)
            {
                if (element.BindingUri != null)
                    element.Binding = profile.GetValueSetByUri(element.BindingUri);
            }
        }

        private IEnumerable<TypeRef> NewTypeRefs
        {
            get
            {
                return profile.Elements.SelectMany(e => e.TypeRefs).Where(r => r.Structure == null);
            }
        }

        private void _linkStructures()
        {
            foreach (TypeRef typeref in NewTypeRefs)
            {
                typeref.Structure = profile.GetStructureByName(typeref.Code);
            }
        }

        private void _linkElementRefs()
        {
            foreach (Structure structure in profile.Structures)
            {
                foreach (Element element in structure.Elements)
                {
                    if (element.ElementRef == null && element.ElementRefPath != null)
                        element.ElementRef = profile.GetElementByPath(structure, element.ElementRefPath);
                }
            }
        }

        private bool _tryLinkToParent(Structure structure, Element element)
        {
            Element parent = profile.ParentOf(structure, element);
            if (parent != null)
            {
                parent.Children.Add(element);
                return true;
            }
            return false;
        }

        public void _linkElements(Structure structure)
        {
            foreach (Element e in structure.Elements)
            {
                _tryLinkToParent(structure, e);
            }
        }

        public void _linkElements()
        {
            foreach (Structure structure in profile.Structures)
            {
                _linkElements(structure);
            }
        }

        public void _addNameSpace(Element element)
        {
            if (element.HasTypeRef)
            {
                TypeRef typeref = element.TypeRefs.FirstOrDefault(t => t.Structure != null);
                if (typeref != null)
                {
                    element.NameSpacePrefix = typeref.Structure.NameSpacePrefix;
                }
            }

            if (element.NameSpacePrefix == null)
                element.NameSpacePrefix = FhirNamespaceManager.Fhir;
            
        }

        public void _addNameSpaces()
        {
            foreach (Element element in profile.Elements.Where(e => e.NameSpacePrefix == null))
            {
                _addNameSpace(element);
            }
        }

        private void _compileConstraints()
        {
            foreach (Constraint c in profile.Constraints)
            {
                if (!c.Compiled)
                    ConstraintCompiler.Compile(c);
            }
        }

        /// <summary>
        /// Make the profile complete and usable by linking all internal structures and perform precompilation
        /// </summary>
        private void _surrect()
        {
            _linkBindings();
            _linkElements();
            _linkStructures();
            _linkElementRefs();
            _compileConstraints();
            _addNameSpaces();
        }

        public Profile ToProfile()
        {
            _surrect();
            return profile;

        }
    }
}
