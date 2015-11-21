using System;
using System.Collections.Generic;

namespace App1.Models
{
    public class OmnivaJson
    {
       public DateTime Updated { get; set; }
       public List<OmnivaLocation> Locations { get; set; }
    }
}