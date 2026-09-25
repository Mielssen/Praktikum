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
                .FirstOrDefault(t => t.Token == token && !t.Used && t.ExpiresAt > DateTime.Now);
        }
        public void MarkUsed(PasswordResetToken token)
        {
            token.Used = true;
            _context.PasswordResetTokens.Update(token);
            _context.SaveChanges();
        }
        public void InvalidateOldTokens(int credentialId)
        {
            var tokens = _context.PasswordResetTokens
                .Where(t => t.CredentialId == credentialId && !t.Used)
                .ToList();
            foreach (var t in tokens)
                t.Used = true;

            _context.SaveChanges();
        }
    }
}
