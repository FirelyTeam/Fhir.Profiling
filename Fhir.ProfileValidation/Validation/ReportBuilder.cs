using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Profiling
{
    public class ReportBuilder
    {
        private int nesting = 0;
        private Report report = new Report();
        public Report Report
        {
            get
            {
                return report;
            }
        }
        public void Start(Group group, Vector vector)
        {
            Add(group, Status.Start, vector);
            nesting++;
        }
        public void End()
        {
            nesting--;
            Add(Group.Hierarchy, Status.End);
        }
        public void Add(Group group, Status kind, Vector vector)
        {
            Report.Outcome outcome = new Report.Outcome(group, kind, vector, null, this.nesting);

            report.Add(outcome);
        }
        public void Add(Group group, Status kind)
        {
            Report.Outcome outcome = new Report.Outcome(group, kind, null, null, this.nesting);

            report.Add(outcome);
        }
        public void Add(Group group, Status kind, Vector vector, string message, params object[] args)
        {
            Report.Outcome outcome;
            outcome.Type = group;
            outcome.Vector = vector;
            outcome.Message = string.Format(message, args);
            outcome.Kind = kind;
            outcome.Nesting = this.nesting;
            report.Add(outcome);
        }
        public void Add(Group group, Status kind, string message, params object[] args)
        {
            Report.Outcome outcome;
            outcome.Type = group;
            outcome.Vector = null;
            outcome.Message = string.Format(message, args);
            outcome.Kind = kind;
            outcome.Nesting = this.nesting;
            report.Add(outcome);
        }


        public void Clear()
        {
            report.Clear();
        }
    }
}
