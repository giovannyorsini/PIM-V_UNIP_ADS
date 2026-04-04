using System.Text.Json;
using System.Text.Json.Serialization;
using Backend;
using Backend.Excecoes;
using Backend.Models;
using Backend.Services;

// Modo demonstração em console (requisito acadêmico): dotnet run -- --demo
if (args.Contains("--demo"))
{
    DemonstracaoConsole.Executar();
    return;
}

var builder = WebApplication.CreateBuilder(args);

// URL padrão alinhada ao front-end (api.js); altere em launchSettings se necessário
builder.WebHost.UseUrls("http://127.0.0.1:5000");

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

// Uma única instância do domínio + persistência JSON compartilhada por todas as requisições
builder.Services.AddSingleton(_ =>
{
    var path = Path.Combine(AppContext.BaseDirectory, "dados_pimv_api.json");
    var sistema = new SistemaPimV(path);
    try
    {
        sistema.CarregarDados();
    }
    catch (PersistenciaException)
    {
        // Arquivo ausente ou inválido: inicia vazio e grava após seed
    }

    GarantirDadosDemonstracaoWeb(sistema);
    try
    {
        sistema.SalvarDados();
    }
    catch (PersistenciaException)
    {
        // Persistência opcional na primeira execução sem permissão de escrita
    }

    return sistema;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors();

RegistrarEndpoints(app);

app.Run();

/// <summary>
/// Expõe os gerenciadores C# existentes via HTTP para o front-end PIM V.
/// </summary>
static void RegistrarEndpoints(WebApplication app)
{
    var api = app.MapGroup("/api");

    // Verificação rápida de conectividade (útil para testes e diagnóstico)
    api.MapGet("/health", () => Results.Ok(new { status = "ok", sistema = "PIM V API", ts = DateTime.UtcNow }));

    api.MapGet("/eventos", (SistemaPimV sistema) =>
        Results.Ok(sistema.Eventos.ListarOrdenadosPorDataInicio().ToList()));

    api.MapGet("/eventos/{id:guid}", (Guid id, SistemaPimV sistema) =>
    {
        var ev = sistema.Eventos.ObterPorId(id);
        return ev is null ? Results.NotFound() : Results.Ok(ev);
    });

    // Programação: todas as atividades com nome do evento (join em memória)
    api.MapGet("/atividades", (SistemaPimV sistema) =>
    {
        var eventos = sistema.Eventos.Listar().ToDictionary(e => e.Id);
        var lista = sistema.Atividades.ListarTodas()
            .OrderBy(a => a.DataHoraInicio)
            .Select(a => new AtividadeProgramacaoResponse(
                a.Id,
                a.EventoId,
                eventos.TryGetValue(a.EventoId, out var ev) ? ev.Titulo : "(Evento removido)",
                a.Titulo,
                string.IsNullOrWhiteSpace(a.Descricao) ? "Sem descrição cadastrada." : a.Descricao,
                a.DataHoraInicio,
                a.DataHoraFim,
                a.Tipo.ToString(),
                a.Responsavel))
            .ToList();
        return Results.Ok(lista);
    });

    api.MapPost("/participantes", (CadastroParticipanteRequest body, SistemaPimV sistema) =>
    {
        try
        {
            var p = new Participante
            {
                Nome = (body.Nome ?? string.Empty).Trim(),
                Email = (body.Email ?? string.Empty).Trim(),
                Instituicao = string.IsNullOrWhiteSpace(body.Instituicao)
                    ? "Cadastro via web (não informado)"
                    : body.Instituicao.Trim(),
                Cpf = string.IsNullOrWhiteSpace(body.Cpf) ? string.Empty : body.Cpf.Trim()
            };
            var criado = sistema.Participantes.Cadastrar(p);
            PersistirQuietamente(sistema);
            return Results.Created($"/api/participantes/por-email?email={Uri.EscapeDataString(criado.Email)}", criado);
        }
        catch (DadosInvalidosException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    });

    api.MapGet("/participantes/por-email", (string email, SistemaPimV sistema) =>
    {
        if (string.IsNullOrWhiteSpace(email))
            return Results.BadRequest(new { message = "Informe o parâmetro email." });
        var p = sistema.Participantes.ObterPorEmail(email);
        return p is null ? Results.NotFound(new { message = "Participante não encontrado." }) : Results.Ok(p);
    });

    api.MapPost("/inscricoes", (NovaInscricaoRequest body, SistemaPimV sistema) =>
    {
        try
        {
            var participante = sistema.Participantes.ObterPorEmail(body.Email ?? string.Empty);
            if (participante is null)
                return Results.NotFound(new { message = "Participante não encontrado. Cadastre-se antes." });

            var insc = sistema.Inscricoes.RealizarInscricao(participante.Id, body.EventoId);
            PersistirQuietamente(sistema);
            return Results.Ok(insc);
        }
        catch (DadosInvalidosException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (InscricaoDuplicadaException ex)
        {
            return Results.Conflict(new { message = ex.Message });
        }
    });

    api.MapGet("/inscricoes", (string email, SistemaPimV sistema) =>
    {
        if (string.IsNullOrWhiteSpace(email))
            return Results.BadRequest(new { message = "Informe o parâmetro email." });

        var participante = sistema.Participantes.ObterPorEmail(email);
        if (participante is null)
            return Results.Ok(Array.Empty<InscricaoDetalheResponse>());

        var eventos = sistema.Eventos.Listar().ToDictionary(e => e.Id);
        var lista = sistema.Inscricoes.BuscarPorParticipante(participante.Id)
            .Select(i => new InscricaoDetalheResponse(
                i.Id,
                i.EventoId,
                eventos.TryGetValue(i.EventoId, out var ev) ? ev.Titulo : "(Evento)",
                i.Status.ToString(),
                i.DataInscricao))
            .ToList();
        return Results.Ok(lista);
    });

    api.MapGet("/certificados", (string email, SistemaPimV sistema) =>
    {
        if (string.IsNullOrWhiteSpace(email))
            return Results.BadRequest(new { message = "Informe o parâmetro email." });

        var participante = sistema.Participantes.ObterPorEmail(email);
        if (participante is null)
            return Results.Ok(Array.Empty<CertificadoListaResponse>());

        var eventos = sistema.Eventos.Listar().ToDictionary(e => e.Id);
        var lista = sistema.Certificados.Listar()
            .Where(c => c.ParticipanteId == participante.Id)
            .Select(c => new CertificadoListaResponse(
                c.Id,
                c.InscricaoId,
                c.EventoId,
                eventos.TryGetValue(c.EventoId, out var ev) ? ev.Titulo : "(Evento)",
                c.DataEmissao,
                c.ConteudoTexto.Length > 220 ? c.ConteudoTexto[..220] + "…" : c.ConteudoTexto,
                c.ConteudoTexto,
                c.CaminhoArquivo))
            .ToList();
        return Results.Ok(lista);
    });

    api.MapPost("/certificados/emitir", (EmitirCertificadoRequest body, SistemaPimV sistema) =>
    {
        try
        {
            Inscricao? insc = body.InscricaoId is { } iid
                ? sistema.Inscricoes.ObterPorId(iid)
                : null;

            if (insc is null && !string.IsNullOrWhiteSpace(body.Email) && body.EventoId is { } eid)
            {
                var p = sistema.Participantes.ObterPorEmail(body.Email);
                if (p is null)
                    return Results.NotFound(new { message = "Participante não encontrado." });
                insc = sistema.Inscricoes.BuscarPorParticipante(p.Id)
                    .FirstOrDefault(x => x.EventoId == eid && x.Status == StatusInscricao.Confirmada);
            }

            if (insc is null)
                return Results.NotFound(new { message = "Inscrição confirmada não encontrada para emissão." });

            var cert = sistema.Certificados.GerarParaInscricao(insc.Id);
            PersistirQuietamente(sistema);
            return Results.Ok(cert);
        }
        catch (DadosInvalidosException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (PersistenciaException ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    });
}

/// <summary>
/// Se o arquivo JSON estiver vazio, cria evento e atividades de exemplo para o front-end listar dados reais.
/// </summary>
static void GarantirDadosDemonstracaoWeb(SistemaPimV sistema)
{
    if (sistema.Eventos.Listar().Count > 0)
        return;

    var evento = new Evento
    {
        Titulo = "PIM V — Eventos Acadêmicos Inclusivos em TI",
        Descricao =
            "Encontro com palestras, minicursos e espaços acessíveis voltados à inclusão em tecnologia da informação.",
        DataInicio = new DateTime(2026, 5, 10, 8, 0, 0),
        DataFim = new DateTime(2026, 5, 12, 18, 0, 0),
        Local = "Campus principal / transmissão online (legendas e Libras)",
        CargaHorariaHoras = 24,
        InscricoesAbertas = true
    };
    sistema.Eventos.Criar(evento);

    sistema.Atividades.Cadastrar(new Atividade
    {
        EventoId = evento.Id,
        Titulo = "Abertura — Acessibilidade e eventos acadêmicos",
        Descricao = "Apresentação institucional, boas práticas de inclusão e canais de apoio em Libras.",
        Tipo = TipoAtividade.Palestra,
        DataHoraInicio = new DateTime(2026, 5, 10, 9, 0, 0),
        DataHoraFim = new DateTime(2026, 5, 10, 10, 30, 0),
        Responsavel = "Equipe Organizadora PIM V"
    });

    sistema.Atividades.Cadastrar(new Atividade
    {
        EventoId = evento.Id,
        Titulo = "Minicurso: Design responsivo e testes com leitores de tela",
        Descricao = "Oficina prática com foco em HTML semântico, contraste e navegação por teclado.",
        Tipo = TipoAtividade.Minicurso,
        DataHoraInicio = new DateTime(2026, 5, 11, 14, 0, 0),
        DataHoraFim = new DateTime(2026, 5, 11, 17, 30, 0),
        Responsavel = "Profª. Convida TI Acessível"
    });

    sistema.Atividades.Cadastrar(new Atividade
    {
        EventoId = evento.Id,
        Titulo = "Mesa redonda: carreiras inclusivas em TI",
        Descricao = "Debates com profissionais sobre mercado de trabalho e políticas de diversidade.",
        Tipo = TipoAtividade.Palestra,
        DataHoraInicio = new DateTime(2026, 5, 12, 10, 0, 0),
        DataHoraFim = new DateTime(2026, 5, 12, 12, 0, 0),
        Responsavel = "Mediadora convidada"
    });
}

/// <summary>
/// Persiste alterações após comandos mutáveis; falhas de disco não derrubam a resposta HTTP já montada.
/// </summary>
static void PersistirQuietamente(SistemaPimV sistema)
{
    try
    {
        sistema.SalvarDados();
    }
    catch (PersistenciaException)
    {
        // Log poderia ser adicionado em produção
    }
}

// Contratos JSON compartilhados com o front-end (camelCase via opções globais)
internal sealed record CadastroParticipanteRequest(string Nome, string Email, string? Instituicao, string? Cpf);

internal sealed record NovaInscricaoRequest(string Email, Guid EventoId);

internal sealed record EmitirCertificadoRequest(Guid? InscricaoId, string? Email, Guid? EventoId);

internal sealed record AtividadeProgramacaoResponse(
    Guid Id,
    Guid EventoId,
    string EventoTitulo,
    string Titulo,
    string Descricao,
    DateTime DataHoraInicio,
    DateTime? DataHoraFim,
    string Tipo,
    string? Responsavel);

internal sealed record InscricaoDetalheResponse(
    Guid Id,
    Guid EventoId,
    string EventoTitulo,
    string Status,
    DateTime DataInscricao);

internal sealed record CertificadoListaResponse(
    Guid Id,
    Guid InscricaoId,
    Guid EventoId,
    string EventoTitulo,
    DateTime DataEmissao,
    string PreviewTexto,
    string ConteudoTexto,
    string? CaminhoArquivo);
