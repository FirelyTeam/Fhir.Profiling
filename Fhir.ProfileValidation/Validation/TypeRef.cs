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

namespace Fhir.Profiling
{
    public class TypeRef
    {
        public string Code;
        public string ProfileName { get; set; }
        public Structure Structure { get; set; }
        public override string ToString()
        {
            string s = Code;
            return s;
        }
    }
}
