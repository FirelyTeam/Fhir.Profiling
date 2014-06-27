/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Profiling
{
    public enum Kind { Valid, Invalid, Incomplete, Unknown, Info, Start, End, Skipped, Any }

    public static class KindExtensions
    {
        public static Kind ToKind(this bool b)
        {
            return b ? Kind.Valid : Kind.Invalid;
        }
        public static bool Failed(this Kind kind)
        {
            Kind[] fails = { Kind.Invalid, Kind.Incomplete, Kind.Unknown };
            return fails.Contains(kind);
        }
    }

    public struct Outcome
    {
        public string Type;
        public Vector Vector;
        public string Message;
        public int Nesting;
        public Kind Kind;
        public Outcome(string type, Kind kind, Vector vector, string message, int nesting = 0)
        {
            this.Type = type;
            this.Kind = kind;
            this.Vector = vector;
            this.Message = message;
            this.Nesting = nesting;
        }
        public override string ToString()
        {
            return string.Format("{0} {1}: {2}", this.Type,  Kind.ToString().ToLower(), this.Message);
        }
    }

    public class Report
    {
        public List<Outcome> Outcomes = new List<Outcome>();

        private int nesting = 0;

        public void Start(string type, Vector vector)
        {
            Add(type, Kind.Start, vector);
            nesting++;
        }
        public void End()
        {
            nesting--;
            Add(null, Kind.End);
        }

        private void Add(string type, Kind kind, Vector vector)
        {
            Outcome outcome = new Outcome(type, kind, vector, null, this.nesting);
            
            Outcomes.Add(outcome);
        }
        
        private void Add(string type, Kind kind)
        {
            Outcome outcome = new Outcome(type, kind, null, null, this.nesting);

            Outcomes.Add(outcome);
        }


        public void Add(string type, Kind kind, Vector vector, string message, params object[] args)
        {
            Outcome outcome;
            outcome.Type = type;
            outcome.Vector = vector;
            outcome.Message = string.Format(message, args);
            outcome.Kind = kind;
            outcome.Nesting = this.nesting;
            Outcomes.Add(outcome);
        }
        public void Add(string type, Kind kind, string message, params object[] args)
        {
            Outcome outcome;
            outcome.Type = type;
            outcome.Vector = null;
            outcome.Message = string.Format(message, args);
            outcome.Kind = kind;
            outcome.Nesting = this.nesting;
            Outcomes.Add(outcome);
        }

        public bool Valid
        {
            get 
            {
                return Outcomes.Count(o => o.Kind.Failed()) == 0;
            }
            
        }
        public void Clear()
        {
            Outcomes.Clear();
        }
    }

}
