using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Common.Contracts
{
    public class PaginationRes<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public Pagination PaginationData { get; set; } = new Pagination(); // Initialize to avoid nullability issue

        public PaginationRes() { }

        public PaginationRes(IEnumerable<T> items, Pagination paginationData)
        {
            Data = items;
            PaginationData = paginationData;
        }
    }
}
