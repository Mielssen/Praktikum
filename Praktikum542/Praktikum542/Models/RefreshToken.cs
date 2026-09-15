using System;
using System.Collections.Generic;

namespace Praktikum542.Models;

public partial class RefreshToken
{
    public int TokenId { get; set; }

    public int CredentialId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Credential Credential { get; set; } = null!;
}
