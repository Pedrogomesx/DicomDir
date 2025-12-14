using DicomDir.Data;
using DicomDir.Models;
using DicomDir.Auxiliar;
using FellowOakDicom;
using FellowOakDicom.Network;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DicomDir.Services
{
    //OBSERVAÇÕES PARA PEDRO FERNANDES
    //DICOMSAVEHANDLER HERDA DE DICOMSERVICE PARA GERENCIAR A CONEXÃO DE REDE 
    //SERVIÇO RESPONSÁVEL POR RECEBER E SALVAR AS IMAGENS DICOM
    //DICOMSAVEHANDLER IMPLEMENTA VÁRIOS PROVEDORES DE SERVIÇO  C-ECHO (PING), C-FIND (PESQUISA) E C-STORE (RECEBER IMAGEM)
    //IMPLEMENTA IDICOMSERVICEPROVIDER PARA GERENCIAR O CICLO DE VIDA DA ASSOCIAÇÃO DICOM
    //IMPLEMENT IDICOMSERVICERUNNER PARA SER EXECUTADO PELO DICOMSERVERFACTORY
    //IMPLEMENTA IDISPOSAVBLE PARA GERENCIAR RECURSOS E LIBERAÇÃO DE MEMÓRIA
    //IDicomCEchoProvider PARA TRATAR OS PEDIDOS DE PING C-ECHO 
    //IDicomCFindProvider PARA TRATAR OS PEDIDOS DE PESQUISA C-FIND
    //IDicomCStoreProvider PARA TRATAR OS PEDIDOS DE ARMAZENAMENTO C-STORE
    //IDicomServiceProvider PARA GERENCIAR O CICLO DE VIDA DA ASSOCIAÇÃO DICOM
    public class DicomSaveHandler : DicomService, IDicomServiceRunner, IDisposable, IDicomCEchoProvider, IDicomCFindProvider, IDicomCStoreProvider, IDicomServiceProvider
    {
        private readonly string _storagePath = @"C:\DicomServer_Storage";
        private readonly DicomServiceDependencies _dependencies;

        //CONSTRUTOR PADRÃO QUE RECEBE OS PARAMETROS NECESSÁRIOS PARA INICIALIZAR A CLASSE BASE DICOMSERVICE
        //ESSE CONSTRUTOR É CHAMADO PELO DICOMSERVERFACTORY AO CRIAR UMA NOVA INSTÂNCIA DE DICOMSAVEHANDLER
        public DicomSaveHandler(INetworkStream stream, Encoding fallbackEncoding, ILogger logger, DicomServiceDependencies dependencies)
            : base(stream, fallbackEncoding, logger, dependencies)
        {
            _dependencies = dependencies;
        }

        // --- C-ECHO (PING) ---
        public Task<DicomCEchoResponse> OnCEchoRequestAsync(DicomCEchoRequest request)
        {
            Console.WriteLine($"[PING] Recebido de {Association.CallingAE}");
            return Task.FromResult(new DicomCEchoResponse(request, DicomStatus.Success));
        }

        // --- C-STORE (RECEBER IMAGEM) ---
        public Task<DicomCStoreResponse> OnCStoreRequestAsync(DicomCStoreRequest request)
        {
            try
            { 

                using (var db =  DatabaseService.GetContext())
                {
                    var dataset = request.Dataset;

                    // --- PACIENTE ---
                    string patientId = dataset.GetValueOrDefault(DicomTag.PatientID, 0, "DESCONHECIDO");
                    var paciente = db.PatientTable.Find(patientId);

                    if (paciente == null)
                    {
                        paciente = new PatientTable
                        {
                            PatientId = patientId,
                            PatientName = dataset.GetString(DicomTag.PatientName),
                            PatientSex = dataset.GetString(DicomTag.PatientSex),
                            PatientBirthDay = AuxiliarMethod.ParseDicomDate(dataset.GetString(DicomTag.PatientBirthDate))
                        };
                        db.PatientTable.Add(paciente);
                        Console.WriteLine($"[NOVO PACIENTE] {paciente.PatientName}");
                    }

                    // --- ESTUDO ---
                    var studyInstanceUID = dataset.GetValueOrDefault<string>(DicomTag.StudyInstanceUID, 0, null);
                    var studyUID = db.StudyTable.Find(studyInstanceUID);

                    if (studyUID == null)
                    {
                        studyUID = new StudyTable
                        {
                            StudyInstanceUID = studyInstanceUID,
                            Accessionnumber = dataset.GetValueOrDefault(DicomTag.AccessionNumber, 0, "SEM ACN"), // Proteção extra para ACN
                            PatientName = paciente.PatientName,
                            StudyDate = AuxiliarMethod.ParseDicomDate(dataset.GetString(DicomTag.StudyDate)),
                            Modality = dataset.GetString(DicomTag.Modality),
                            PatientId = paciente.PatientId,
                            StudyDescription = dataset.GetValueOrDefault(DicomTag.StudyDescription, 0, "SEM DESCRICAO")
                        };
                        db.StudyTable.Add(studyUID);
                        AuxiliarMethod.LogColorido("  ├── ", "[NOVO ESTUDO]", $"{studyUID.Modality}");
                    }

                    // --- SÉRIE ---
                    var seriesInstanceUID = dataset.GetValueOrDefault<string>(DicomTag.SeriesInstanceUID, 0, null);
                    var seriesUID = db.SeriesTable.Find(seriesInstanceUID);

                    if (seriesUID == null)
                    {
                        seriesUID = new SeriesTable
                        {
                            SeriesUID = seriesInstanceUID,
                            StudyUID = studyUID.StudyInstanceUID,
                            Modality = studyUID.Modality,
                            SeriesNumber = dataset.GetValueOrDefault<string>(DicomTag.SeriesNumber, 0, null)
                        };
                        db.SeriesTable.Add(seriesUID);
                    }

                    // --- IMAGEM ---
                    var sopInstaceUID = dataset.GetValueOrDefault<string>(DicomTag.SOPInstanceUID, 0, null);

                    if (db.ImageTable.Find(sopInstaceUID) == null)
                    {
                        var instanceUID = new ImageTable
                        {
                            StudyUID = studyUID.StudyInstanceUID,
                            SeriesUID = seriesUID.SeriesUID,
                            InstanceUID = sopInstaceUID,
                            SopClassUID = dataset.GetValueOrDefault<string>(DicomTag.SOPClassUID, 0, null),
                            PathFile = dataset.GetValueOrDefault<string>(DicomTag.AccessionNumber, 0, null)
                        };
                        db.ImageTable.Add(instanceUID);

                        // SALVAR NO BANCO
                        db.SaveChanges();
                        SalvarArquivoDicom(dataset, patientId, studyInstanceUID, seriesUID.SeriesNumber, sopInstaceUID);
                        AuxiliarMethod.LogColorido("      └── ", "[ID PACIENTE]", paciente.PatientName, "[PRONTUÁRIO] =", paciente.PatientId, "[ACNUMBER]", studyUID.Accessionnumber);
                        AuxiliarMethod.LogColorido("      └── ", "[STUDY UID]", studyInstanceUID);
                        AuxiliarMethod.LogColorido("      └── ", "[SERIES UID]", seriesUID.SeriesUID, "[SERIESNUMBER]", seriesUID.SeriesNumber );
                        AuxiliarMethod.LogColorido("      └── ", "[INSTANCE UID]", instanceUID.InstanceUID);
                    }
                }

                return Task.FromResult(new DicomCStoreResponse(request, DicomStatus.Success));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERRO GRAVAÇÃO] {ex.Message}");

                // --- NOVO: MOSTRAR O MOTIVO REAL DO ERRO ---
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[DETALHE TÉCNICO] {ex.InnerException.Message}");
                }
                // -------------------------------------------

                Console.ResetColor();
                return Task.FromResult(new DicomCStoreResponse(request, DicomStatus.ProcessingFailure));
            }
        }

        //MÉTODO AUXILIAR PARA SALVAR O ARQUIVO DICOM NO SISTEMA DE ARQUIVOS
        private string SalvarArquivoDicom(DicomDataset dataset, string patientiId, string studyInstanceUid, string seriesNumber, string imageInstanceUid)
        {
            if (string.IsNullOrWhiteSpace(seriesNumber))
                seriesNumber = "SERIE_SEM_NUMERO";

            var folderPath = Path.Combine(_storagePath, "ID_" + patientiId, studyInstanceUid, "Ser_"+ seriesNumber);


            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            var fileFullPath = Path.Combine(folderPath, imageInstanceUid + ".dcm");
            var dicomFile = new DicomFile(dataset);
            dicomFile.Save(fileFullPath);
            return fileFullPath;
        }

        // --- TRATAMENTO DE ERRO DE REDE ---
        public void OnCStoreRequestException(string tempFileName, Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERRO DE REDE] {e.Message}");
            Console.ResetColor();
        }

        // --- C-FIND (PESQUISA) ---
        // Implementação vazia para cumprir o contrato IDicomCFindProvider
        //IDICOMCFINDPROVIDER FOI IMPLEMENTADO MAS POR ENQUANTO NÃO SERÁ USADO, FOI IMPLEMENTADO POR CAUSA DO ILOGGER
        public async IAsyncEnumerable<DicomCFindResponse> OnCFindRequestAsync(DicomCFindRequest request)
        {
            // Implementação vazia para cumprir o contrato IDicomCFindProvider
            yield break;
        }

        // --- 1. RECEBER PEDIDO DE CONEXÃO ---
        // IMPLEMENTADO: ACEITA APENAS VERIFICAÇÃO E ARMAZENAMENTO
        public Task OnReceiveAssociationRequestAsync(DicomAssociation association)
        {
            // Aqui decidimos o que aceitamos. 
            // Vamos aceitar tudo o que for Verificação (Ping) ou Storage (Imagens)
            foreach (var pc in association.PresentationContexts)
            {
                if (pc.AbstractSyntax == DicomUID.Verification ||
                    pc.AbstractSyntax.StorageCategory != DicomStorageCategory.None)
                {
                    // Aceitamos a transferência com a sintaxe proposta (ex: JPEG, Raw, etc)
                    pc.AcceptTransferSyntaxes(pc.GetTransferSyntaxes().ToArray());
                }
            }

            // Envia a resposta dizendo "Pode entrar"
            return SendAssociationAcceptAsync(association);
        }

        // --- 2. FINALIZAR CONEXÃO EDUCADA ---
        // IMPLEMENTADO: RESPONDE AO PEDIDO DE LIBERAÇÃO DE ASSOCIAÇÃO
        public Task OnReceiveAssociationReleaseRequestAsync()
        {
            return SendAssociationReleaseResponseAsync();
        }

        // --- 3. QUANDO A CONEXÃO CAI OU FECHA ---
        public void OnConnectionClosed(Exception exception)
        {
            //INFORMA NO LOG SE A CONEXÃO FOI FECHADA COM OU SEM ERRO
            if (exception != null)
            {
                Console.WriteLine($"[CONEXÃO FECHADA COM ERRO] {exception.Message}");
            }
        }

        //IMPLEMENTADO: PARA LOGAR QUE A CONEXÃO FOI ABORTADA, MAS MANTEM A APLICAÇÃO RODANDO
        public void OnReceiveAbort(DicomAbortSource source, DicomAbortReason reason)
        {
            Console.WriteLine($"[CONEXÃO ABORTADA] Fonte: {source}, Razão: {reason}");
        }

        // IMPLEMENTADO: Apenas logar, sem lançar erro
        public Task OnCStoreRequestExceptionAsync(string tempFileName, Exception e)
        {
            Console.WriteLine($"[ERRO NO ARMAZENAMENTO] {e.Message}");
            return Task.CompletedTask;
        }
    }
}