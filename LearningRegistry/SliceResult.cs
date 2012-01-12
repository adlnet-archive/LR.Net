using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LearningRegistry
{
    public class SliceResult : ResumableResult
    {
        protected override ResumableResult getPage()
        {
            return LRUtils.Slice(this.BaseUri, this._Args);
        }
    }
}
