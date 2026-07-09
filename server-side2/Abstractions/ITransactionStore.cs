using SQLink.Models;

namespace SQLink.Abstractions
{
    public interface ITransactionStore
    {
        void Upsert(Transaction tx);
        IEnumerable<Transaction> GetAll();
        //Transaction? Get(string id);
    }
}
