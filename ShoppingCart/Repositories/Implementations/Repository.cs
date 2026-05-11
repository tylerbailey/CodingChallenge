using CodingChallenge.Models.Interfaces;
using CodingChallenge.Repositories.Interfaces;

namespace CodingChallenge.Repositories.Implementations
{
    // In-memory repository used for the coding challenge and tests.   
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly List<T> _items = [];

        public void Save(T item) => _items.Add(item);

        public void Update(T item)
        {
            var index = _items.FindIndex(i => i.Id == item.Id);
            if (index != -1)
                _items[index] = item;
        }

        public T? Retrieve(int id) => _items.FirstOrDefault(item => item.Id == id);
    }
}
