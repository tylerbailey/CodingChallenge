namespace CodingChallenge.Models.Interfaces
{
    /**
     * Base interface for entities that have an Id property, allowing consistent 
     * identification across the application. Implementing types are responsible 
     * for enforcing immutability.
     */
    public interface IEntity
    {
        int Id { get; }
    }
}
