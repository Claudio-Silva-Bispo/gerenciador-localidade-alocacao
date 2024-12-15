using Project.Infrastructure.Interfaces;
using Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("Turno")] 
public class TurnoController : Controller
{
    private readonly ITurnoService _turnoService;

    public TurnoController(ITurnoService turnoService)
    {
        _turnoService = turnoService;
    }

    [AllowAnonymous]
    [HttpGet("Criar")]
    public IActionResult Criar()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost("Criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(TurnoDTO turnoDTO)
    {

        if (ModelState.IsValid)
        {

            var idUsuario = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(idUsuario))
            {
                return Unauthorized("Usuário não logado.");
            }

            // Cria o objeto Endereco
            var turno = new Turno
            {
                TurnoPreferencia = turnoDTO.TurnoPreferencia,
                IdUsuario = idUsuario
            };

            await _turnoService.Criar(turno);

            TempData["SuccessMessage"] = "Preferencia de turno cadastrado com sucesso!";
            //return RedirectToAction("Mensagem");
        }
        return View(turnoDTO);
    }

    [HttpGet("Mensagem")]
    public IActionResult Mensagem()
    {
        return View();
    }

    [HttpGet("Consultar")]
    public async Task<IActionResult> Consultar()
    {
        var turnos = await _turnoService.ConsultarTodos(); 
        return View(turnos); 
    }

    [HttpGet("Atualizar")]
    public async Task<IActionResult> Atualizar()
    {
        //var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == "IdUsuario")?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Error");
        }

        var turno = await _turnoService.ConsultarPorUsuarioId(userIdString);
        if (turno == null)
        {
            return NotFound();
        }

        return View(turno);
    }

    [HttpPost("Atualizar")]
    public async Task<IActionResult> Atualizar(Turno turno)
    {
        if (!ModelState.IsValid)
        {
            return View(turno);
        }

        //var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == "IdUsuario")?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Error");
        }

        var turnoExistente = await _turnoService.ConsultarPorUsuarioId(userIdString);

        if (turnoExistente == null)
        {
            return NotFound();
        }

        turnoExistente.TurnoPreferencia = turno.TurnoPreferencia;
       

        await _turnoService.Atualizar(turnoExistente);

        TempData["SuccessMessage"] = "Dado atualizado com sucesso!";
        return RedirectToAction("Consultar");
    }


    [HttpPost("Excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(string idDia)
    {
        Console.WriteLine($"Excluindo Turno com Id: {idDia}");

        try
        {
            await _turnoService.Excluir(idDia);
            TempData["SuccessMessage"] = "Exclusão realizada com sucesso!";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = $"Erro: {ex.Message}";
        }

        return RedirectToAction("Consultar");
    }

}