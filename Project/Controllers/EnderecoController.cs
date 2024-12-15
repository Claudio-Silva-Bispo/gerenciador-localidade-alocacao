using Project.Infrastructure.Interfaces;
using Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("Endereco")] 
public class EnderecoController : Controller
{
    //private readonly ApplicationDbContext _context;
    private readonly IEnderecoService _enderecoService;

    public EnderecoController(IEnderecoService enderecoService)
    {
        _enderecoService = enderecoService;
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
    public async Task<IActionResult> Criar(EnderecoDTO enderecoDTO)
    {

        if (ModelState.IsValid)
        {

            var idUsuario = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(idUsuario))
            {
                return Unauthorized("Usuário não logado.");
            }

            // Cria o objeto Endereco
            var endereco = new Endereco
            {
                CEP = enderecoDTO.CEP,
                Estado = enderecoDTO.Estado,
                Cidade = enderecoDTO.Cidade,
                Bairro = enderecoDTO.Bairro,
                Rua = enderecoDTO.Rua,
                IdUsuario = idUsuario
            };

            await _enderecoService.Criar(endereco);

            TempData["SuccessMessage"] = "Endereco cadastrado com sucesso!";
            return RedirectToAction("Mensagem");
        }
        return View(enderecoDTO);
    }

    [HttpGet("Mensagem")]
    public IActionResult Mensagem()
    {
        return View();
    }

    [HttpGet("Consultar")]
    public async Task<IActionResult> Consultar()
    {
        var enderecos = await _enderecoService.ConsultarTodos(); 
        return View(enderecos); 
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

        var endereco = await _enderecoService.ConsultarPorUsuarioId(userIdString);
        if (endereco == null)
        {
            return NotFound();
        }

        return View(endereco);
    }

    [HttpPost("Atualizar")]
    public async Task<IActionResult> Atualizar(Endereco endereco)
    {
        if (!ModelState.IsValid)
        {
            return View(endereco);
        }

        //var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == "IdUsuario")?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Error");
        }

        var enderecoExistente = await _enderecoService.ConsultarPorUsuarioId(userIdString);

        if (enderecoExistente == null)
        {
            return NotFound();
        }

        enderecoExistente.CEP = endereco.CEP;
        enderecoExistente.Estado = endereco.Estado;
        enderecoExistente.Cidade = endereco.Cidade;
        enderecoExistente.Bairro = endereco.Bairro;
        enderecoExistente.Rua = endereco.Rua;

        await _enderecoService.Atualizar(enderecoExistente);

        TempData["SuccessMessage"] = "Usuário atualizado com sucesso!";
        return RedirectToAction("MensagemAtualizacao");
    }

    [HttpGet("MensagemAtualizacao")]
    public IActionResult MensagemAtualizacao()
    {
        return View();
    }

    [HttpGet("ConfirmarExcluir/{id}")]
    public async Task<IActionResult> ConfirmarExcluir(string id)
    {
        var endereco = await _enderecoService.ConsultarPorUsuarioId(id);
        
        if (endereco == null)
        {
            return NotFound();
        }

        return View(endereco);
    }


    [HttpPost("Excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(string id)
    {
        var endereco = await _enderecoService.ConsultarPorUsuarioId(id);
        
        if (endereco != null)
        {
            // Exclui o endereco do banco de dados
            await _enderecoService.Excluir(id);
            
            // Redireciona para a página de login ou para onde você preferir
            TempData["SuccessMessage"] = "Endereco excluído com sucesso.";
            return RedirectToAction("MensagemExclusao", "Endereco"); 
        }

        TempData["ErrorMessage"] = "Endereco não encontrado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("MensagemExclusao")]
    public IActionResult MensagemExclusao()
    {
        return View();
    }



}