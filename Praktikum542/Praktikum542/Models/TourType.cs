using System;
using System.Collections.Generic;

namespace Praktikum542.Models;

public partial class TourType
{
    public int TypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
