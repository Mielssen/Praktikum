
using Microsoft.EntityFrameworkCore;
using Praktikum542.Models;

namespace Praktikum542.Repositories
{
    public class PasswordResetRepository
    {
        private readonly PraktikumContext _context;

        public PasswordResetRepository(PraktikumContext context)
        {
            _context = context;
        }

        public void Add(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Add(token);
            _context.SaveChanges();
        }

        public PasswordResetToken? GetValidToken(string token)
        {
            return _context.PasswordResetTokens
                .FirstOrDefault(t =>
                    t.Token == token &&
                    !t.Used &&
                    t.ExpiresAt > DateTime.Now);
        }
        public bool TryMarkUsed(string token)
        {
            // EF Core InMemory cannot translate ExecuteUpdate. Keep the
            // atomic database-side update for relational providers.
            if (string.Equals(_context.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
            {
                var validToken = _context.PasswordResetTokens.FirstOrDefault(t =>
                    t.Token == token &&
                    !t.Used &&
                    t.ExpiresAt > DateTime.Now);

                if (validToken == null)
                    return false;

                validToken.Used = true;
                _context.SaveChanges();
                return true;
            }

            var affectedRows = _context.PasswordResetTokens
                .Where(t =>
                    t.Token == token &&
                    !t.Used &&
                    t.ExpiresAt > DateTime.Now)
                .ExecuteUpdate(setters =>
                    setters.SetProperty(t => t.Used, true));

            return affectedRows == 1;
        }

        public void InvalidateOldTokens(int credentialId)
        {
            var tokens = _context.PasswordResetTokens
                .Where(t => t.CredentialId == credentialId && !t.Used)
                .ToList();

            foreach (var token in tokens)
                token.Used = true;

            _context.SaveChanges();
        }
    }
}
