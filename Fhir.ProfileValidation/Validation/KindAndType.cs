﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Profiling
{
    /// <summary>
    /// The possible outcomes of a validation fragment
    /// </summary>
    public enum Status { Valid, Failed, Incomplete, Unknown, Info, Start, End, Skipped, Any }

    public static class KindExtensions
    {
        public static Status ToStatus(this bool b)
        {
            return b ? Status.Valid : Status.Failed;
        }
        public static bool Failed(this Status kind)
        {
            Status[] fails = { Status.Failed, Status.Incomplete, Status.Unknown };
            return fails.Contains(kind);
        }
    }

    /// <summary>
    /// The groups under which all validation results are grouped
    /// </summary>
    public enum Group { 
        
        Cardinality, Constraint, 
        Profile, Hierarchy,
        Structure, Element, Primitive, Attribute,
        Reference, Coding, Value
    }
    
}
