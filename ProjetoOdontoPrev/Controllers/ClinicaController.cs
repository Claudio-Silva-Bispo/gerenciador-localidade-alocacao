using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ProjetoOdontoPrev.Models;

namespace ProjetoOdontoPrev.Controllers
{
    public class ClinicaController : Controller
    {
        private static List<Clinica> clinicas;
        private static string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "dados.xlsx");

        public IActionResult Index()
        {
            if (clinicas == null)
            {
                clinicas = LerDadosExcel(filePath);
            }

            // Filtra as clínicas com status diferente de "Encerrado" e que tenham um atendimento pendente
            var cliente1Clinicas = clinicas
                .Where(c => c.ID_Cliente == 1 && c.StatusClinica != "Encerrado" && c.StatusClinica != "Recusado")
                .OrderBy(c => c.RankNovo) // Ordena conforme o RankNovo (para prioridade)
                .ToList();

            // Verifica se há atendimentos pendentes
            if (!cliente1Clinicas.Any())
            {
                ViewBag.Mensagem = "Não há atendimentos pendentes para este cliente.";
                return View(); // Retorna a View com a mensagem
            }

            return View(cliente1Clinicas); // Retorna a lista de clínicas pendentes
        }


        [HttpPost]
        public async Task<IActionResult> Responder(int id, string resposta)
        {
            var clinica = clinicas.FirstOrDefault(c => c.ID_Cliente == id && c.StatusClinica != "Encerrado");
            if (clinica != null)
            {
                if (resposta == "1")
                {
                    clinica.StatusClinica = "Aceito";
                    AtualizarStatusClinicas(id, "Encerrado");
                }
                else if (resposta == "2")
                {
                    clinica.StatusClinica = "Recusado";
                }

                clinica.DataAcao = DateTime.Now;
                clinica.HoraAcao = DateTime.Now.ToString("HH:mm:ss");

                // Atualizar o arquivo Excel
                await AtualizarDadosExcel(filePath, clinica);

                // Agora, vamos buscar a próxima clínica "Pendente" (não encerrada, não recusada)
                var proximaClinica = clinicas
                    .Where(c => string.IsNullOrEmpty(c.StatusClinica) && c.ID_Cliente == id) // Filtra pela mesma clínica (ID_Cliente) e status "Pendente"
                    .OrderBy(c => c.RankNovo) // Ordena pela prioridade de atendimento
                    .FirstOrDefault(); // Pega a primeira que não foi atendida ainda


            }

            return RedirectToAction("Index");
        }

        private List<Clinica> LerDadosExcel(string filePath)
        {
            var clinicas = new List<Clinica>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    throw new Exception("Nenhuma planilha encontrada no arquivo Excel.");
                }

                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    clinicas.Add(new Clinica
                    {
                        ID_Cliente = int.Parse(worksheet.Cells[row, 1].Text),
                        Cliente = worksheet.Cells[row, 2].Text,
                        CEP_Cliente = worksheet.Cells[row, 3].Text,
                        ID_Clinica = int.Parse(worksheet.Cells[row, 4].Text),
                        NomeClinica = worksheet.Cells[row, 5].Text,
                        CEP_Clinica = worksheet.Cells[row, 6].Text,
                        Distancia = double.Parse(worksheet.Cells[row, 7].Text),
                        DistanciaReal = double.Parse(worksheet.Cells[row, 8].Text),
                        NotaPesquisa = double.Parse(worksheet.Cells[row, 9].Text),
                        ValorConsulta = double.Parse(worksheet.Cells[row, 10].Text),
                        DistanciaNormalizada = double.Parse(worksheet.Cells[row, 11].Text),
                        NotaNormalizada = double.Parse(worksheet.Cells[row, 12].Text),
                        ValorNormalizado = double.Parse(worksheet.Cells[row, 13].Text),
                        Pontuacao = double.Parse(worksheet.Cells[row, 14].Text),
                        RankNovo = int.Parse(worksheet.Cells[row, 15].Text),
                        StatusClinica = worksheet.Cells[row, 16].Text,
                        DataAcao = DateTime.MinValue,
                        HoraAcao = string.Empty
                    });
                }
            }

            return clinicas;
        }

        public IActionResult ListarClinicas()
        {
            if (clinicas == null)
            {
                clinicas = LerDadosExcel(filePath);
            }

            var cliente1Clinicas = clinicas.Where(c => c.ID_Cliente == 1).OrderBy(c => c.RankNovo).ToList();
            return View(cliente1Clinicas);
        }

        public IActionResult Agenda()
        {
            if (clinicas == null)
            {
                clinicas = LerDadosExcel(filePath);
            }

           // Filtra as clínicas do cliente 1 com status "Aceito"
            var cliente1Clinicas = clinicas
                .Where(c => c.ID_Cliente == 1 && c.StatusClinica == "Aceito")
                .OrderBy(c => c.RankNovo)
                .ToList();

            // Verifica se existe alguma clínica com status "Aceito"
            if (!cliente1Clinicas.Any())
            {
                ViewBag.Mensagem = "Nenhum agendamento foi realizado.";
                return View("SemAgendamento");
            }

            // Retorna as clínicas aceitas para a view
            return View(cliente1Clinicas);
        }

        private async Task AtualizarDadosExcel(string filePath, Clinica clinicaAtualizada)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    if (int.Parse(worksheet.Cells[row, 1].Text) == clinicaAtualizada.ID_Cliente && worksheet.Cells[row, 5].Text == clinicaAtualizada.NomeClinica)
                    {
                        worksheet.Cells[row, 16].Value = clinicaAtualizada.StatusClinica;
                        worksheet.Cells[row, 17].Value = clinicaAtualizada.DataAcao.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 18].Value = clinicaAtualizada.HoraAcao;
                        break;
                    }
                }

                await package.SaveAsync();
            }
        }

        private void AtualizarStatusClinicas(int idCliente, string status)
        {
            foreach (var clinica in clinicas.Where(c => c.ID_Cliente == idCliente && c.StatusClinica != "Aceito"))
            {
                clinica.StatusClinica = status;
                clinica.DataAcao = DateTime.Now;
                clinica.HoraAcao = DateTime.Now.ToString("HH:mm:ss");
            }

            // Atualizar o arquivo Excel
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    if (int.Parse(worksheet.Cells[row, 1].Text) == idCliente)
                    {
                        worksheet.Cells[row, 16].Value = status;
                        worksheet.Cells[row, 17].Value = DateTime.Now.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 18].Value = DateTime.Now.ToString("HH:mm:ss");
                    }
                }

                package.Save();
            }
        }
    }
}