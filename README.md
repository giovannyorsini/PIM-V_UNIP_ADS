# PIM V - Sistema de Eventos Acadêmicos Inclusivos

Projeto acadêmico com arquitetura separada em:

- **Back-end** em ASP.NET Core Minimal API (`.NET 8`)
- **Front-end** estático (HTML, CSS e JavaScript)

O sistema permite consultar programação, cadastrar participantes, realizar inscrições e emitir certificados.

## Índice

- [Visão geral](#visão-geral)
- [Tecnologias](#tecnologias)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Pré-requisitos](#pré-requisitos)
- [Clonando o repositório](#clonando-o-repositório)
- [Como executar o projeto localmente](#como-executar-o-projeto-localmente)
  - [1) Iniciar o back-end](#1-iniciar-o-back-end)
  - [2) Servir o front-end localmente](#2-servir-o-front-end-localmente)
- [Endpoints principais da API](#endpoints-principais-da-api)
- [Modo demonstração em console](#modo-demonstração-em-console)
- [Solução de problemas](#solução-de-problemas)
- [Licença](#licença)

## Visão geral

Este repositório contém uma aplicação para gestão de eventos acadêmicos com foco em inclusão, composta por:

- API HTTP para regras de negócio e persistência em JSON
- Interface web para interação do usuário final

O front-end consome a API por padrão em `http://127.0.0.1:5000/api`.

## Tecnologias

- C# / ASP.NET Core Minimal API (`.NET 8`)
- HTML5
- CSS3
- JavaScript (Vanilla JS)
- Persistência local em arquivo JSON

## Estrutura do projeto

```text
PIM-V_UNIP_ADS/
├── backend/        # API .NET 8
│   ├── Program.cs
│   └── PimV.csproj
├── frontend/       # Front-end estático
│   ├── index.html
│   ├── css/
│   ├── js/
│   └── pages/
├── LICENSE
└── README.md
```

## Pré-requisitos

Antes de executar, instale:

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- Um servidor HTTP local para arquivos estáticos (uma das opções abaixo):
  - Python 3 (`python -m http.server`)
  - Node.js + `npx` (`npx serve`)

## Clonando o repositório

Substitua a URL abaixo pela URL real do seu repositório no GitHub:

```bash
git clone https://github.com/giovannyorsini/PIM-V_UNIP_ADS.git
cd ./PIM-V_UNIP_ADS
```

Exemplo (PowerShell):

```powershell
git clone https://github.com/giovannyorsini/PIM-V_UNIP_ADS.git
Set-Location ./PIM-V_UNIP_ADS
```

## Como executar o projeto localmente

Abra dois terminais: um para o back-end e outro para o front-end.

### 1) Iniciar o back-end

No terminal 1, execute:

```bash
cd backend
dotnet run
```

API esperada:

- Base: `http://127.0.0.1:5000/api`
- Health check: `http://127.0.0.1:5000/api/health`

### 2) Servir o front-end localmente

No terminal 2, vá para `frontend/` e escolha **uma** opção:

#### Opção A - Python 3

```bash
cd frontend
python -m http.server 5500
```

Depois, acesse:

- `http://127.0.0.1:5500`

#### Opção B - Node.js (`serve`)

```bash
cd frontend
npx serve -l 5500
```

Depois, acesse:

- `http://127.0.0.1:5500`

## Endpoints principais da API

- `GET /api/health`
- `GET /api/eventos`
- `GET /api/atividades`
- `POST /api/participantes`
- `GET /api/participantes/por-email?email=...`
- `POST /api/inscricoes`
- `GET /api/inscricoes?email=...`
- `GET /api/certificados?email=...`
- `POST /api/certificados/emitir`

## Modo demonstração em console

Além da API, o back-end possui modo demonstração:

```bash
cd backend
dotnet run -- --demo
```

## Solução de problemas

- **Erro de CORS ou conexão recusada:** confirme que a API está ativa em `http://127.0.0.1:5000`.
- **Front-end sem dados:** verifique se o back-end iniciou sem erros e se a URL base da API em `frontend/js/api.js` está correta.
- **Porta ocupada:** altere a porta do servidor estático (ex.: `5501`) e abra a nova URL.
- **Falha no `dotnet run`:** confirme instalação do .NET 8 com `dotnet --version`.

## Licença

Este projeto está sob a licença descrita em `LICENSE`.
