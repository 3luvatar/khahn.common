using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using khahn.common.entity;
using khahn.common.entity.fetchExpressions;

namespace khahn.common.data.interfaces
{
    public interface IRepository<T>
    {
        EntityPageRequest<T> GetPagedEntity(EntityPageRequest<T> pageRequest);
        T GetById<T_ID>(T_ID id);
        void Update(T entity);
        void Save(T entity);
        void Insert(T entity);
        void Delete(T entity);
    }
}