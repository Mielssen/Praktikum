using System;
using System.Collections.Generic;

namespace Praktikum542.Models;

public partial class Penalty
{
    public int PenaltyId { get; set; }

    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public bool? IsPaid { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
