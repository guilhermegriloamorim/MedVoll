using MedVoll.Web.Dtos;
using MedVoll.Web.Exceptions;
using MedVoll.Web.Interfaces;
using MedVoll.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedVoll.Web.Controllers
{
    [Authorize]
    [Route("medicos")]
    public class MedicoController : BaseController
    {
        private const string PaginaListagem = "Listagem";
        private const string PaginaCadastro = "Formulario";
        private readonly IMedicoService _service;

        public MedicoController(IMedicoService service, SignInManager<IdentityUser> signInManager)
            : base(signInManager)
        {
            _service = service;
        }

        [HttpGet]
        [Route("{page?}")]
        public async Task<IActionResult> ListarAsync([FromQuery] int page = 1)
        {
            var medicosCadastrados = await _service.ListarAsync(page);
            ViewData["Url"] = "Medicos";
            return View(PaginaListagem, medicosCadastrados);
        }

        [HttpGet]
        [Route("formulario/{id?}")]
        public async Task<IActionResult> ObterFormularioAsync(long? id)
        {
            var dados = id.HasValue 
                ? await _service.CarregarPorIdAsync(id.Value) 
                : new MedicoDto();

            return View(PaginaCadastro, dados);
        }

        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SalvarAsync([FromForm] MedicoDto dados)
        {
            if (dados._method == "delete")
            {
                await _service.ExcluirAsync(dados.Id.Value);
                return Redirect("/medicos");
            }

            if (!ModelState.IsValid)
            {
                return View(PaginaCadastro, dados);
            }

            try
            {
                await _service.CadastrarAsync(dados);
                return Redirect("/medicos");
            }
            catch (RegraDeNegocioException ex)
            {
                ViewBag.Erro = ex.Message;
                ViewBag.Dados = dados;
                return View(PaginaCadastro);
            }
        }

        [HttpGet]
        [Route("especialidade/{especialidade}")]
        public async Task<IActionResult> ListarPorEspecialidadeAsync(string especialidade)
        {
            if (Enum.TryParse(especialidade, out Especialidade especEnum))
            {
                var medicos = await _service.ListarPorEspecialidadeAsync(especEnum);
                return Json(medicos);
            }
            return BadRequest("Especialidade inválida.");
        }
    }
}
