﻿using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Tastys.BLL.Services.RecetaCRUD;
using Tastys.BLL;
using Tastys.Domain;
using Microsoft.CodeAnalysis.Host.Mef;
using Tastys.API.Middlewares;
using Swashbuckle.AspNetCore.Filters;

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
        [HttpGet("order")]
        public async Task<ActionResult<List<RecetaDto>>> GetOrderRecetas([FromQuery] int page = 0, [FromQuery] int pageSize = 10, [FromQuery] QueryOrdersRecetas sort_by = QueryOrdersRecetas.Fav)
        {
            try
            {
                List<RecetaDto> recetas = await _recetaService.GetOrderRecetas(page, pageSize, sort_by);

                return Ok(recetas);

            }
            catch (System.Exception ex)
            {
                _logger.LogError("Error al traer recetas ordenadas: " + ex);
                return StatusCode(500);
                throw;
            }
        }
        [HttpGet]
        public async Task<ActionResult<List<RecetaDto>>> GetAllRecetas()
        {
            // Los queryParameters se validan automáticamente
            // de acuerdo a las anotaciones en RecetasQuery

            try
            {
                List<RecetaDto> recetas = await _recetaService.GetAllRecetas();

                return Ok(recetas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al traer recetas desde DB");
                return StatusCode(404);
            }
        }
        [HttpGet(":id")]
        public async Task<ActionResult<Receta>> GetRecetaByID(int ID)
        {
            try
            {
                var receta = await _recetaService.GetRecetaByID(ID);
                if (receta == null)
                {
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
        [SwaggerRequestExample(typeof(NewRecetaDTO), typeof(RecetaRequestExample))]
        [CheckToken]
        [CheckPermissions("user:user")]
        public async Task<ActionResult<Receta>> CreateReceta([FromBody] NewRecetaDTO recetaData)
        {
            try
            {
                if (HttpContext.Items["userdata"] is not UserDataToken userData)
                {
                    return BadRequest("No se encontró información del usuario.");
                }
                Receta postReceta = await _recetaService.CreateReceta(
                    new Receta
                    {
                        Nombre = recetaData.receta.nombre,
                        ImageUrl = recetaData.receta.imageUrl,
                        Descripcion = recetaData.receta.descripcion
                    }
                    , userData.authId, recetaData.list_c,recetaData.list_i);

                //te retorna el codigo 201 -created- cuando se crea
                //y ademas te dice en el header, che, encontras esta receta en la ruta /recetas/:id
                return CreatedAtAction(nameof(CreateReceta), new { id = postReceta.RecetaID }, postReceta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la receta en DB");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("admin")]
        [SwaggerRequestExample(typeof(NewRecetaDTO), typeof(RecetaRequestExample))]
        [CheckToken]
        [CheckPermissions("user:admin")]
        public async Task<ActionResult<Receta>> CreateRecetaAdmin([FromBody] NewRecetaDTO recetaData)
        {
            try
            {
                Receta postReceta = await _recetaService.CreateReceta(
                    new Receta
                    {
                        Nombre = recetaData.receta.nombre,
                        ImageUrl = recetaData.receta.imageUrl,
                        Descripcion = recetaData.receta.descripcion,
                        TiempoCoccion = recetaData.receta.tiempo_de_coccion
                    }
                    , recetaData.user_id, recetaData.list_c,recetaData.list_i);

                //te retorna el codigo 201 -created- cuando se crea
                //y ademas te dice en el header, che, encontras esta receta en la ruta /recetas/:id
                return CreatedAtAction(nameof(CreateReceta), new { id = postReceta.RecetaID }, postReceta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la receta en DB");
                return StatusCode(404,ex);
            }
        }
        [HttpDelete("{ID}")]
        public async Task<ActionResult<RecetaDto>> DeleteReceta(int ID)
        {
            try
            {
                var receta = await _recetaService.GetRecetaByID(ID);
                if (receta == null)
                {
                    return NotFound();
                }
                bool deleted = await _recetaService.DeleteReceta(ID);
                if (!deleted)
                {
                    return StatusCode(500, $"Error al borrar la receta con ID {ID}");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al borrar la receta en DB");
                return StatusCode(500, "Error no específico, contactar a un administrador");
            }
        }
    }
}
