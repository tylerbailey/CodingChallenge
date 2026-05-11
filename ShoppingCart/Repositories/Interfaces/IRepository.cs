using CodingChallenge.Models.Interfaces;
namespace CodingChallenge.Repositories.Interfaces
{
    public interface IRepository<T> where T : class, IEntity
    {
        void Save(T value);
        T? Retrieve(int id);
        void Update(T value);
    }
}
