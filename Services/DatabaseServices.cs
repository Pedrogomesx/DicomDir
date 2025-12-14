using DicomDir.Data;
using DicomDir.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DicomDir.Services
{
    //SERVIÇO RESPONSÁVEL POR GERENCIADO O BANCO DE DADOS EM MEMÓRIA
    //RESPONSÁVEL POR INICIALIZAR, FORNECER CONTEXTOS E REALIZAR CONSULTAS SIMPLES
    //TAMBÉM GARANTE QUE O BANCO PERMANEÇA VIVO DURANTE A EXECUÇÃO DO PROGRAMA
    //ISOLA AS CONEXÕES E A LÓGICA DE ACESSO AO BANCO DO RESTANTE DO CÓDIGO
    public static class DatabaseService
    {
        // Esta variável segura o banco vivo na memória RAM
        private static SqliteConnection _keepAliveConnection;

        // Inicia o banco e mantém a conexão aberta
        public static void InicializarBanco()
        {
            if (_keepAliveConnection == null)
            {
                // Cria a conexão em memória o _KEEPALIVECONNECTION RECEBE A CONEXÃO E DEPOIS INICIA
                _keepAliveConnection = new SqliteConnection("Data Source=:memory:");
                _keepAliveConnection.Open();

                //DB CONTEXT USA ESSA CONEXÃO E O EF CRIA AS TABELAS
                using (var db = GetContext())
                {
                    db.Database.EnsureCreated();
                    Console.WriteLine("[DB] Banco em Memória iniciado e tabelas criadas.");
                }
            }
        }

        // Fecha a conexão quando o programa sair
        public static void FecharBanco()
        {
            if (_keepAliveConnection != null)
            {
                _keepAliveConnection.Close();
                _keepAliveConnection.Dispose();
                _keepAliveConnection = null;
            }
        }

        // FORNECE UM NOVO DICOMDBCONTEXT USANDO A CONEXÃO ATIVA
        public static DicomDbContext GetContext()
        {
            // Se a conexão ainda não foi criada (caso alguém chame antes do Inicializar), cria agora
            if (_keepAliveConnection == null) InicializarBanco();

            // Usa a mesma conexão aberta para não perder os dados
            var options = new DbContextOptionsBuilder<DicomDbContext>()
                .UseSqlite(_keepAliveConnection)
                .Options;

            return new DicomDbContext(options);
        }

        // --- MÉTODOS DE CONSULTA (ENCAPSULAOS) ---

        public static List<PatientTable> ListarTodosPacientes()
        {
            using (var db = GetContext())
            {
                return db.PatientTable.ToList();
            }
        }

        public static List<StudyTable> BuscarEstudosPorPaciente(string patientId)
        {
            using (var db = GetContext())
            {
                return db.StudyTable.Where(s => s.PatientId == patientId).ToList();
            }
        }

        public static int ContarTotalImagens()
        {
            using (var db = GetContext())
            {
                return db.ImageTable.Count();
            }
        }
    }
}