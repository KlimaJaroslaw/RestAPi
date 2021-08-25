using Commander.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using Commander;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Commander.Data
{
    public class CommanderContext : DbContext
    {
        public CommanderContext(DbContextOptions<CommanderContext> opt) : base(opt)
        {
             
        }

        public DbSet<Command> Commands { get; set; }

        public DataTable ExecSQL(string cWhere)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = new SqlCommand(@"SELECT 
                                                    ID AS Id,
                                                    data_wystawienia AS Date,
                                                    numer_mag AS DocNum,
                                                    skrot_nazwy AS Customer
                                                FROM A_zamowienia
                                                    INNER JOIN A_klienci ON A_zamowienia.id_kontrah = A_klienci.id_klienta", GlobalData.connection);
            da.Fill(dt);
            return dt;            
        }
    }
}
