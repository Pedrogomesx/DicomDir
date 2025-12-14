using DicomDir.Auxiliar; // Para os Logs Coloridos
using DicomDir.Services; // Para acessar o DatabaseService
using FellowOakDicom;
using FellowOakDicom.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace DicomDir
{
    internal class Program
    {
        private static int portaServidor = 104;
        private static string aeTitleServidor = "PACS";

        static void Main(string[] args)
        {
            AuxiliarMethod.LogColorido("==================================================", "");
            Console.WriteLine($" =      SERVIDOR DICOM (SCP) - {aeTitleServidor}: {portaServidor}       =");
            AuxiliarMethod.LogColorido("==================================================", "");
            Console.WriteLine();
            AuxiliarMethod.LogColorido("==================================================", "");
            Console.WriteLine($" =      AGUARDANDO RECEBIMENTO DO ESTUDO       =");
            AuxiliarMethod.LogColorido("==================================================", "");

            // 2. INICIAR BANCO (TODA LÓGICA COMPLEXA ESTÁ NO DATABASESERVICE)
            DatabaseService.InicializarBanco();

            // 3 CONFIGURA E INICIAILZA O SERVIDOR DICOM
            new DicomSetupBuilder()
                .RegisterServices(services =>
                {
                    services.AddLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);
                    });
                })
                .Build();
            
            var server = DicomServerFactory.Create<DicomSaveHandler>(portaServidor);
            Console.WriteLine($"[STATUS] Servidor rodando na porta {portaServidor}...");

            // --- MENU ---
            bool rodando = true;
            while (rodando)
            {
                Console.WriteLine("\nEscolha uma opção:");
                Console.WriteLine("1 - Listar Pacientes e Estudos");
                Console.WriteLine("2 - Contar Imagens no Banco");
                Console.WriteLine("3 - Limpar Console");
                Console.WriteLine("4 - Sair");
                Console.Write("Opção: ");

                var opcao = Console.ReadLine();

                switch (opcao)
                {
                    case "1":
                        ExibirRelatorioPacientes();
                        break;
                    case "2":
                        int qtd = DatabaseService.ContarTotalImagens();
                        AuxiliarMethod.LogColorido("\nTOTAL DE IMAGENS: ", "", ConsoleColor.Green, qtd);
                        break;
                    case "3":
                        Console.Clear();
                        break;
                    case "4":
                        rodando = false;
                        break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }

            // 5. Encerramento
            Console.WriteLine("Parando servidor...");
            server.Stop();

            // Fecha a conexão global
            DatabaseService.FecharBanco();

            Console.WriteLine("Servidor encerrado.");
        }

        // Método Privado apenas para desenhar o relatório na tela
        private static void ExibirRelatorioPacientes()
        {
            Console.WriteLine("\n--- RELATÓRIO DO BANCO ---");

            var pacientes = DatabaseService.ListarTodosPacientes();

            if (pacientes.Count == 0)
            {
                Console.WriteLine("Nenhum paciente encontrado.");
                return;
            }

            foreach (var p in pacientes)
            {
                AuxiliarMethod.LogColorido("PACIENTE:", "", ConsoleColor.Cyan, p.PatientName, ConsoleColor.Gray, $"({p.PatientId})");

                var estudos = DatabaseService.BuscarEstudosPorPaciente(p.PatientId);

                foreach (var e in estudos)
                {
                    Console.WriteLine($"  └── {e.StudyDescription}");
                    Console.WriteLine($"      UID: {e.StudyInstanceUID}");
                    Console.WriteLine($"      Data: {e.StudyDate:d} | Mod: {e.Modality}");
                }
                Console.WriteLine("------------------------------------------------");
            }
        }
    }
}