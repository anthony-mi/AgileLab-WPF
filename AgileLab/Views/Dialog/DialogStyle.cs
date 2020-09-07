using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Views.Dialog
{
    enum DialogStyle
    {
        //
        // Summary:
        //     Just "OK"
        Affirmative = 0,
        //
        // Summary:
        //     "OK" and "Cancel"
        AffirmativeAndNegative = 1,
        AffirmativeAndNegativeAndSingleAuxiliary = 2,
        AffirmativeAndNegativeAndDoubleAuxiliary = 3
    }
}
