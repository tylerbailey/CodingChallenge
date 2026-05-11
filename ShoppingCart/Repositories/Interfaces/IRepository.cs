using CodingChallenge.Models.Interfaces;
namespace CodingChallenge.Repositories.Interfaces
{
    /**
     * Generic repository interface for performing basic CRUD operations on entities.
     * It defines methods for saving, retrieving, and updating entities of type T, where T is a class that implements the IEntity interface.
     */
    public interface IRepository<T> where T : class, IEntity
    {
        void Save(T value);
        T? Retrieve(int id);
        void Update(T value);
    }
}
