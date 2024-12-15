using Project.Infrastructure.Interfaces;
using Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("DiasPreferencia")] 
public class DiasPreferenciaController : Controller
{
    //private readonly ApplicationDbContext _context;
    private readonly IDiasPreferenciaService _diasPreferenciaService;

    public DiasPreferenciaController(IDiasPreferenciaService diasPreferenciaService)
    {
        _diasPreferenciaService = diasPreferenciaService;
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
    public async Task<IActionResult> Criar(DiasPreferenciaDTO diasPreferenciaDTO)
    {

        if (ModelState.IsValid)
        {

            var idUsuario = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(idUsuario))
            {
                return Unauthorized("Usuário não logado.");
            }

            // Cria o objeto Endereco
            var dia = new DiasPreferencia
            {
                DiasSemana = diasPreferenciaDTO.DiasSemana,
                IdUsuario = idUsuario
            };

            await _diasPreferenciaService.Criar(dia);

            TempData["SuccessMessage"] = "Preferencia do dia cadastrado com sucesso!";
            //return RedirectToAction("Mensagem");
        }
        return View(diasPreferenciaDTO);
    }

    [HttpGet("Mensagem")]
    public IActionResult Mensagem()
    {
        return View();
    }

    [HttpGet("Consultar")]
    public async Task<IActionResult> Consultar()
    {
        var dias = await _diasPreferenciaService.ConsultarTodos(); 
        return View(dias); 
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

        var dia = await _diasPreferenciaService.ConsultarPorUsuarioId(userIdString);
        if (dia == null)
        {
            return NotFound();
        }

        return View(dia);
    }

    [HttpPost("Atualizar")]
    public async Task<IActionResult> Atualizar(DiasPreferencia dia)
    {
        if (!ModelState.IsValid)
        {
            return View(dia);
        }

        //var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == "IdUsuario")?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Error");
        }

        var diaExistente = await _diasPreferenciaService.ConsultarPorUsuarioId(userIdString);

        if (diaExistente == null)
        {
            return NotFound();
        }

        diaExistente.DiasSemana = dia.DiasSemana;
       

        await _diasPreferenciaService.Atualizar(diaExistente);

        TempData["SuccessMessage"] = "Dado atualizado com sucesso!";
        return RedirectToAction("Consultar");
    }


    [HttpPost("Excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(string idDia)
    {
        Console.WriteLine($"Excluindo DiasPreferencia com Id: {idDia}");

        try
        {
            await _diasPreferenciaService.Excluir(idDia);
            TempData["SuccessMessage"] = "Exclusão realizada com sucesso!";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = $"Erro: {ex.Message}";
        }

        return RedirectToAction("Consultar");
    }

}