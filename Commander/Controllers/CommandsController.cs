using AutoMapper;
using Commander.Data;
using Commander.Dtos;
using Commander.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Controllers
{
    [Route("api/con")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private  readonly ICommanderRepo _repository;
        private readonly IMapper _mapper;
        private  CommanderContext _context;

        public CommandsController(ICommanderRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        //private readonly MockCommanderRepo _repository = new MockCommanderRepo();
        [HttpGet]
        public ActionResult<IEnumerable<OrdersReadDto>> GetOrders()
        {
            //_context = new CommanderContext(opt => opt.UseSqlServer(Configuration.GetConnectionString("CommanderConnection")));
            string sql = @"SELECT
                                                    ID AS Id,
                                                    data_wystawienia AS Date,
                                                    numer_mag AS DocNum,
                                                    skrot_nazwy AS Customer
                                                FROM A_zamowienia
                                                    INNER JOIN A_klienci ON A_zamowienia.id_kontrah = A_klienci.id_klienta";
            DataTable commandItems = GlobalData.ExecSQL(sql);

            string JSONString = string.Empty;
            //JSONString = JsonConvert.SerializeObject(commandItems);
            JSONString = GlobalData.DataTableToJsonObj(commandItems);
            return Ok(JSONString);

            //_repository.GetAllCommands();
            //return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }

        //[HttpGet("test")]
        //public ActionResult <CommandReadDto>  GetCommandById([FromQuery] int id)
        //{
        //    var commandItem = _repository.GetCommandById(id);
        //    if (commandItem!=null)
        //        return Ok(_mapper.Map<CommandReadDto>(commandItem));

        //    return NotFound();

        //}

        /// <summary>
        /// Pobranie nagłówków zamówień wg następujący filtrów:
        /// </summary>
        /// <param name="date_min"></param>
        ///                     Data początkowa - parametr wymagany
        /// <param name="date_max"></param>
        ///                     Data końcowa - parametr wymagany
        /// <param name="invoiced"></param>
        ///                         Zafakturowany (zafakturowany) (A_zamowienia.zrealizowano <> 0)        
        ///                         -1 - brak parametru
        ///                         0 - nie zafakturowane
        ///                         1 - zafakturowane
        /// <param name="paid"></param>
        ///     Opłacony ISNULL(A_zamowienia.brutto,0) <= ISNULL(A_zanowienia.rozliczony,0)
        ///                         -1 - brak filtru
        ///                         0 - nie opłacone
        ///                         1 - opłacone
        /// <param name="ext_id"></param>
        ///     Zewnętrzne id: A_zamowienia.ext_id
        ///                     '' - puste 
        /// <param name="status_id"></param>
        ///                     Status zamówienia na podstaiwie statusów zamówień
        ///                     Na podstawie statusów GetOrderStatus
        /// <returns></returns>
        [HttpGet("GetOrders")]
        public ActionResult/*<OrdersReadDto>*/ GetOrders([FromQuery] string ext_id, [FromQuery] DateTime date_min, [FromQuery] DateTime date_max, [FromQuery] int invoiced=2, [FromQuery] int paid=2, [FromQuery] int status_id=-999)
          {
            ///_context = new CommanderContext(opt => opt.UseSqlServer(Configuration.GetConnectionString("CommanderConnection")));
            string sql;
            //string cWhere = string.Empty;


            if (
                    date_min == Convert.ToDateTime("0001-01-01")
                    || date_max == Convert.ToDateTime("0001-01-01")
                    || (invoiced!=-1 && invoiced != 0 && invoiced != 1)
                    || (paid != -1 && paid != 0 && paid != 1)                                
                    || ext_id==null 
                    || status_id==-999
               ) return NotFound("złe parametry");

            //TODO: Zrobić walidację parametrów
            //GetOrderStatuses: SELECT id, wartosc AS Status FROM A_slo_statusy_www
            //TODO: GetOrdersItems: parametr id - id_zamowenia:
            //SELECT 
            //kat_towary.symbol AS ItemCode,
            //				kat_towary.nazwa AS ItemName,
            //				A_zamowienia_skl.ilosc AS Qty,
            //				A_zamowienia_skl.cena_s AS NetPrice,
            //				A_zamowienia_skl.cena_s AS Net
            //            FROM A_zamowienia_skl
            //              INNER JOIN kat_towary ON kat_towary.id = A_zamowienia_skl.id_towaru


            //cWhere = " AND A_zamowienia.id = @id ";
            //if (numer_ogolny != "")
            //    cWhere += " AND A_zamowienia.numer_ogolny = @name ";
            StringBuilder cWhere = new StringBuilder();

            cWhere.Append($" AND A_zamowienia.data_wystawienia>{date_min}");
            cWhere.Append($" AND A_zamowienia.data_wystawienia<{date_max}");
            switch (invoiced)
            {
                case 0:
                    cWhere.Append("A_zamowienia.zrealizowano = 0");
                    break;
                case 1:
                    cWhere.Append("A_zamowienia.zrealizowano <> 0");
                    break;
                default:
                    break;
            }
            switch (paid)
            {
                case 0:
                    cWhere.Append(" AND ISNULL(A_zamowienia.brutto,0) > ISNULL(A_zanowienia.rozliczony,0) ");
                    break;
                case 1:
                    cWhere.Append(" AND ISNULL(A_zamowienia.brutto,0) <= ISNULL(A_zanowienia.rozliczony,0) ");
                    break;
                default:
                    break;
            }
            cWhere.Append($" AND A_zamowienia.ext_id={ext_id}");
            cWhere.Append($" AND A_slo_statusy_www.id={status_id}");

            sql = $@"SELECT 
                         A_zamowienia.ID AS Id,
                         A_zamowienia.data_wystawienia AS Date,
                         A_zamowienia.numer_mag AS DocNum,
                         A_klienci.skrot_nazwy AS Customer,
                         A_zamowienia.ext_id,
                         A_zamowienia.netto AS Net,
                         A_zamowienia.netto AS Total,
						 A_slo_statusy_www.id AS Status_id,
						 A_slo_statusy_www.wartosc AS Status
                     FROM A_zamowienia
                         INNER JOIN A_klienci ON A_zamowienia.id_kontrah = A_klienci.id_klienta
						 LEFT JOIN A_slo_statusy_www ON A_zamowienia.id_slo_status_www = A_slo_statusy_www.id
                     WHERE (1=1) {cWhere}";

            SqlCommand cmd = GlobalData.SqlIntoCommand(sql);
            cmd.Parameters.Add("@id", SqlDbType.Int);
            cmd.Parameters.Add("@name", SqlDbType.NVarChar);
          //  cmd.Parameters["@id"].Value = id;
           // cmd.Parameters["@name"].Value = numer_ogolny;
            
            //cmd.Parameters.AddWithValue("@id", id);
            //if (numer_ogolny != "") cmd.Parameters.AddWithValue("@name", numer_ogolny);
            //else cmd.Parameters.AddWithValue("@name", "numer_ogolny");
            DataTable commandItems = GlobalData.ExecCmd(cmd);

            //DataTable commandItems = GlobalData.ExecSQL(sql);
            string JSONString = string.Empty;
            //JSONString = JsonConvert.SerializeObject(commandItems);
            JSONString = GlobalData.DataTableToJsonObj(commandItems);
            return Ok(JSONString);

        }



        //[HttpPost]
        //public ActionResult  <CommandReadDto> CreateCommand(CommandCreateDto commandCreateDto)
        //{   
        //    var commandModel = _mapper.Map<Command>(commandCreateDto);
        //    _repository.CreateCommand(commandModel);
        //    _repository.SaveChanges();

        //    var commandReadDto = _mapper.Map<CommandReadDto>(commandModel);

        //    return CreatedAtRoute(nameof(GetCommandById), new { Id = commandReadDto.Id}, commandReadDto);

        //    //return Ok(commandReadDto);

        //}
    }
}
