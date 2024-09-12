﻿using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Tastys.BLL.Services.RecetaCRUD;
using Tastys.BLL;
using Tastys.Domain;

namespace Tastys.API.Controllers.Recetas
{

[Route("/api/receta")]
[ApiController]
public class RecetaController : ControllerBase
{
    private readonly ILogger<RecetaController> _logger;
        private readonly RecetaCRUD _recetaService;

        public RecetaController(ILogger<RecetaController> logger, RecetaCRUD recetaService)
    {
        _logger = logger;
        _recetaService = recetaService;
    }

       [HttpGet]
        public async Task<ActionResult<Receta[]>> Get()
       {
         // Los queryParameters se validan automáticamente
          // de acuerdo a las anotaciones en RecetasQuery

          try
          {
                var recetas = _recetaService.GetAllRecetas();

              return Ok(recetas);
          }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error al traer recetas desde DB");
                  return StatusCode(500);
              }
        }
        [HttpGet(":id")]
        public async Task<ActionResult<RecetaDto>> GetRecetaByID(int ID)
        {
            try
            {
                var receta = await  _recetaService.GetRecetaByID(ID);
                if(receta == null){
                    return NotFound();
                }
                return Ok(receta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al traer la receta, comprobar que el ID sea correcto");
                return StatusCode(500);
            }
        }
        [HttpPost]
        public async Task<ActionResult<Receta>> CreateReceta([FromBody]NewRecetaDTO recetaData)
        {
            try
            {
                Receta postReceta = await _recetaService.CreateReceta(recetaData.receta,recetaData.list_c,recetaData.user_id);

                //ok, esta la vi en un tutorial y esta interesante:
                //te retorna el codigo 201 -created- cuando se crea
                //y ademas te dice en el header, che, encontras esta receta en la ruta /recetas/:id
                return CreatedAtAction(nameof(CreateReceta), new { id = postReceta.RecetaID }, postReceta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la receta en DB");
            return StatusCode(500);
        }
    }
        [HttpDelete("{ID}")]
        public async Task<ActionResult<RecetaDto>> DeleteReceta(int ID)
        {
            try
            {
                var receta = await _recetaService.GetRecetaByID(ID);
                if (receta==null)
                {
                    return NotFound();
                }
                bool deleted = await _recetaService.DeleteReceta(ID);
                if (!deleted)
                {
                    return StatusCode(500,$"Error al borrar la receta con ID {ID}");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al borrar la receta en DB");
                return StatusCode(500,"Error no específico, contactar a un administrador");
            }
        }
    }
}
