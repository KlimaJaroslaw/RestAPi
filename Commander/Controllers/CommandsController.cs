﻿using AutoMapper;
using Commander.Data;
using Commander.Dtos;
using Commander.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        public ActionResult <IEnumerable<CommandReadDto>>    GetAllCommands()
        {
            ///_context = new CommanderContext(opt => opt.UseSqlServer(Configuration.GetConnectionString("CommanderConnection")));
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

        [HttpGet("{id}", Name = "GetCommandById")]
        public ActionResult <CommandReadDto>  GetCommandById(int id)
        {
            var commandItem = _repository.GetCommandById(id);
            if (commandItem!=null)
                return Ok(_mapper.Map<CommandReadDto>(commandItem));

            return NotFound();

        }

        [HttpPost]
        public ActionResult  <CommandReadDto> CreateCommand(CommandCreateDto commandCreateDto)
        {
            var commandModel = _mapper.Map<Command>(commandCreateDto);
            _repository.CreateCommand(commandModel);
            _repository.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(commandModel);

            return CreatedAtRoute(nameof(GetCommandById), new { Id = commandReadDto.Id}, commandReadDto);

            //return Ok(commandReadDto);

        }
    }
}
