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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fhir.Profiling
{
    public struct Segment : IComparable<Segment>
    {
        public string Name;
        public bool Multi;
        public int CompareTo(Segment other)
        {
            throw new NotImplementedException();
        }
        public override string ToString()
        {
            string s = Name;
            if (Multi) s += "[x]";
            return s;
        }
    }

    public class Path : IEquatable<Path>
    {
        public List<Segment> Segments { get; private set; }

        public Path(string path)
        {
            Segments = new List<Segment>();
            foreach (string s in path.Split('.'))
            {
                Segment segment = new Segment();
                string name = Regex.Replace(s, @"\[x\]", "");
                if (name != s) segment.Multi = true;
                segment.Name = name;
                Segments.Add(segment);
            }
        }

        public Path(IEnumerable<Segment> segments)
        {
            this.Segments = segments.ToList();
        }

        public Path Parent()
        {
            Path parent = new Path(this.Segments);
            parent.Segments.RemoveAt(parent.Segments.Count - 1);
            return parent;
        }

        public string ElementName
        {
            get
            {
                return Segments.Last().Name;
            }
        }

        public string NodeMatch
        {
            get
            {
                string xpath;

                if (Segments.Last().Multi)
                {
                    xpath = string.Format("./*[starts-with(name(),'{0}')]", ElementName);
                }
                else
                {
                    xpath = string.Format("./f:{0}", ElementName);
                }
                return xpath;
            }
        }

        private string ToXPath()
        {
            var parts = Segments.Select(s => "f:" + s.Name);
            return "../" + string.Join("/", parts);
        }

        public int Count
        {
            get
            {
                return Segments.Count;
            }
        }

        public override string ToString()
        {
            return string.Join(".", Segments);
        }

        public bool Equals(Path other)
        {
            return this.Segments.SequenceEqual(other.Segments);
        }
    }

}
