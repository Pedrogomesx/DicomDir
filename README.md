# 🏥 SharpPACS - Servidor DICOM SCP (.NET)

**SharpPACS** (Projeto DicomDir) é um servidor DICOM leve e modular desenvolvido em C#. O sistema atua como um **SCP (Service Class Provider)**, recebendo imagens médicas via rede, organizando os arquivos físicos e indexando metadados em memória.

> 🚧 **Status:** Em desenvolvimento (Funcional)

## 🚀 Funcionalidades

- **Recepção de Imagens (C-STORE):** Aceita conexões de modalidades médicas e armazena arquivos `.dcm`.
- **Verificação de Conexão (C-ECHO):** Responde a pings DICOM.
- **Armazenamento Inteligente:** Cria automaticamente uma árvore de diretórios organizada:
  `Storage \ ID_Paciente \ UID_Estudo \ Ser_NumeroSerie \ Imagem.dcm`
- **Banco de Dados em Memória:** Utiliza SQLite In-Memory com Entity Framework Core para consultas rápidas durante a execução.
- **Dashboard Interativo:** Interface via Console com logs coloridos em tempo real para monitoramento.

## 🛠️ Tecnologias Utilizadas

- **Linguagem:** C# (.NET 6/8)
- **Protocolo DICOM:** [fo-dicom](https://github.com/fo-dicom/fo-dicom)
- **ORM:** Entity Framework Core
- **Banco de Dados:** SQLite (:memory:)

## 📂 Estrutura do Projeto
`
DicomDir/
├── Auxiliar/          # Métodos utilitários (Logs, Parsers)
├── Data/              # Contexto do Banco (DbContext)
├── Models/            # Modelos das Tabelas (Paciente, Estudo, Série, Imagem)
├── Services/          # Lógica de Negócio (DicomHandler, DatabaseService)
└── Program.cs         # Ponto de entrada e Menu do Console
`    
⚙️ Como Rodar
1- Clone este repositório.

2- Abra a solução no Visual Studio.

3 -Restaure os pacotes NuGet.

4 - Execute o projeto (F5).

5 - O servidor iniciará na porta 104 (AE Title: PACS).

🧪 Como Testar
Você pode usar softwares como MicroDicom, RadiAnt ou ferramentas de linha de comando (dcmsend) para enviar imagens para:

IP: localhost (ou seu IP local)

Porta: 104

AE Title: PACS

Os arquivos recebidos serão salvos em: C:\DicomServer_Storage
