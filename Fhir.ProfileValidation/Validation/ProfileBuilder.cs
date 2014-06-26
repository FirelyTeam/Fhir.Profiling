using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Profiling
{
    public class ProfileBuilder
    {
        private Profile profile = new Profile();

        public void Add(IEnumerable<ValueSet> valuesets)
        {
            profile.ValueSets.AddRange(valuesets);
        }

        public void Add(List<Structure> structures)
        {
            profile.Structures.AddRange(structures);
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
                        element.ElementRef = profile.GetElementByName(structure, element.ElementRefPath);
                }
            }
        }

        public Element FindParent(Structure structure, Element element)
        {
            Path p = element.Path.Parent();
            Element parent = structure.Elements.Find(e => e.Path.Equals(p));
            return parent;
        }

        public bool TryLinkToParent(Structure structure, Element element)
        {
            Element parent = FindParent(structure, element);
            if (parent != null)
            {
                parent.Children.Add(element);
                return true;
            }
            return false;
        }

        public void _linkElements(Structure structure)
        {
            foreach (Element e in profile.Elements)
            {
                TryLinkToParent(structure, e);
            }
        }

        public void _linkElements()
        {
            foreach (Structure structure in profile.Structures)
            {
                _linkElements(structure);
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
        private void surrect()
        {
            _linkBindings();
            _linkStructures();
            _linkElementRefs();
            _compileConstraints();
        }

        public Profile ToProfile()
        {
            surrect();
            return profile;

        }
    }
}
