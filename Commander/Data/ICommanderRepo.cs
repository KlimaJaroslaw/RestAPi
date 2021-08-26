using Commander.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commander.Data
{
    public interface ICommanderRepo
    {
        bool SaveChanges();
        IEnumerable<Orders> GetOrders();
        Command GetCommandById(int id);
        void CreateCommand(Command cmd);

    }
}
