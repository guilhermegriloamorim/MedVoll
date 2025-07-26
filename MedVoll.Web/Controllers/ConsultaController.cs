using MedVoll.Web.Dtos;
using MedVoll.Web.Exceptions;
using MedVoll.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedVoll.Web.Controllers
{
    [Authorize]
    [Route("consultas")]
    public class ConsultaController : BaseController
    {
        private const string PaginaListagem = "Listagem";
        private const string PaginaCadastro = "Formulario";

        private readonly IConsultaService _consultaservice;
        private readonly IMedicoService _medicoService;

        public ConsultaController(IConsultaService consultaService, IMedicoService medicoService, SignInManager<IdentityUser> signInManager)
        : base(signInManager)
        {
            _consultaservice = consultaService;
            _medicoService = medicoService;
        }

        [HttpGet]
        [Route("{page?}")]
        public async Task<IActionResult> ListarAsync([FromQuery] int page = 1)
        {
            var consultasAtivas = await _consultaservice.ListarAsync(page);
            ViewBag.Consultas = consultasAtivas;
            ViewData["Url"] = "Consultas";
            return View(PaginaListagem, consultasAtivas);
        }

        [HttpGet]
        [Route("formulario/{id?}")]
        public async Task<IActionResult> ObterFormularioAsync(long? id)
        {
            var dados = id.HasValue
                ? await _consultaservice.CarregarPorIdAsync(id.Value)
                : new ConsultaDto { Data = DateTime.Now };
            IEnumerable<MedicoDto> medicos = _medicoService.ListarTodos();
            ViewData["Medicos"] = medicos.ToList();
            return View(PaginaCadastro, dados);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SalvarAsync([FromForm] ConsultaDto dados)
        {
            if (dados._method == "delete")
            {
                if (dados.Id.HasValue)
                {
                    await _consultaservice.ExcluirAsync(dados.Id.Value);
                }
                return Redirect("/consultas");
            }

            try
            {
                await _consultaservice.CadastrarAsync(dados);
                return Redirect("/consultas");
            }
            catch (RegraDeNegocioException ex)
            {
                ViewBag.Erro = ex.Message;
                ViewBag.Dados = dados;
                return View(PaginaCadastro);
            }
        }
    }
}
