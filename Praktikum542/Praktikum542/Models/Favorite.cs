using System;
using System.Collections.Generic;

namespace Praktikum542.Models;

public partial class Favorite
{
    public int FavoriteId { get; set; }

    public int CredentialId { get; set; }

    public int TourId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Credential Credential { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}
