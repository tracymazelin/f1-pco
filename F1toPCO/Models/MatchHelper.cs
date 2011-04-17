using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace F1toPCO.Model {
    public class MatchHelperData {
        public Model.F1.person F1Person { get; set; }
        public Model.PCO.people PCOPeople { get; set; }
    }

    public class MatchHelper : List<MatchHelperData> {
        public Model.PCO.person FindPCOPersonByID(string id) {
            return (from x in this
                    from y in x.PCOPeople.person
                    where y.id.Value == id.ToString()
                    select y).FirstOrDefault();
        }

        public Model.F1.person FindF1PersonByID(string id) {
            return this.Where(x => x.F1Person.id == id.ToString()).FirstOrDefault().F1Person;
        }
    }
}