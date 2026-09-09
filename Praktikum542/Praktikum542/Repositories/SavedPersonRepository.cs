using Praktikum542.Models;

namespace Praktikum542.Repositories
{
    public class SavedPersonRepository
    {
        private readonly PraktikumContext _context;
        public SavedPersonRepository(PraktikumContext context)
        {
            _context = context;
        }
        public List<SavedPerson> GetByCredentialId(int credentialId)
        {
            return _context.SavedPersons
                .Where(p=> p.CredentialId == credentialId)
                .ToList();
        }
        public void Add(SavedPerson person)
        {
            _context.SavedPersons.Add(person);
            _context.SaveChanges();
        }
        public SavedPerson? GetById(int id)
        {
            return _context.SavedPersons.
                FirstOrDefault(p => p.PersonId == id);
        }
        public void Delete(SavedPerson person)
        {
            _context.SavedPersons.Remove(person);
            _context.SaveChanges();
        }
    }
}
