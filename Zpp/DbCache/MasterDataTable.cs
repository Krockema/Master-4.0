using System.Collections.Generic;
using System.Linq;
using Master40.DB;
using Master40.DB.Data.WrappersForPrimitives;
using Microsoft.EntityFrameworkCore;
using Zpp.Utils;

namespace Zpp
{
    public class MasterDataTable<T> : IMasterDataTable<T> where T : BaseEntity
    {
        private readonly Dictionary<Id, T> _entitesAsDictionary;
        private readonly List<T> _entities;

        public MasterDataTable(DbSet<T> entitySet)
        {
            _entities = entitySet.ToList();
            _entitesAsDictionary = entityListToDictionary(_entities);
        }
        
        private Dictionary<Id, T> entityListToDictionary<T>(List<T> entityList) where T : BaseEntity
        {
            Dictionary<Id, T> dictionary = new Dictionary<Id, T>();
            foreach (var entity in entityList)
            {
                dictionary.Add(entity.GetId(), entity);
            }

            return dictionary;
        }

        public T GetById(Id id)
        {
            if (!_entitesAsDictionary.ContainsKey(id))
            {
                throw new MrpRunException("Given id is not present in this masterDataTable.");
            }
            return _entitesAsDictionary[id];
        }

        public List<T> GetAll()
        {
            return _entities;
        }
    }
}