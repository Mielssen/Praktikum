using System;
using System.Collections.Generic;

namespace Praktikum542.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int CredentialId { get; set; }

    public int TourId { get; set; }

    public int BookingId { get; set; }

    public sbyte Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Credential Credential { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}
