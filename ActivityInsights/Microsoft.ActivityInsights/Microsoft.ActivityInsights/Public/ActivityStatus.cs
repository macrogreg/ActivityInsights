﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ActivityInsights
{
    public enum ActivityStatus
    {
        Created = 0,
        Running = 3,
        Completed = 5,
        Faulted = 7
    }
}
