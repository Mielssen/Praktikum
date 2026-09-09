using System;
using System.Collections.Generic;

namespace Praktikum542.Models;

public partial class Tour
{
    public int TourId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public DateOnly AvailableFrom { get; set; }

    public DateOnly AvailableTo { get; set; }

    public int TypeId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<TourAsset> TourAssets { get; set; } = new List<TourAsset>();

    public virtual TourType Type { get; set; } = null!;
}
