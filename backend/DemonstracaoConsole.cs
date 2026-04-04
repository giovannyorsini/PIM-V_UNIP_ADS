using Backend.Excecoes;
using Backend.Models;
using Backend.Persistencia;
using Backend.Services;

namespace Backend;

/// <summary>
/// Demonstração em console do fluxo completo (requisito acadêmico PIM V).
/// Executar com: <c>dotnet run -- --demo</c>
/// </summary>
public static class DemonstracaoConsole
{
    /// <summary>Inicia a demonstração textual no terminal.</summary>
    public static void Executar()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== PIM V — Sistema de Eventos Acadêmicos (back-end C#) ===\n");

        // Repositório em arquivo temporário da demonstração (não mistura com dados reais do usuário)
        var jsonDemo = Path.Combine(Path.GetTempPath(), "pimv_demo_academico.json");
        var sistema = new SistemaPimV(jsonDemo);

        try
        {
            sistema.CarregarDados();
            Console.WriteLine($"[Info] Dados carregados de: {sistema.Repositorio.CaminhoArquivo}\n");
        }
        catch (PersistenciaException ex)
        {
            LogErro("Falha ao carregar persistência — iniciando estado vazio.", ex);
        }

        DemonstrarFluxoFeliz(sistema);
        DemonstrarLinQ(sistema);
        DemonstrarTratamentoDeErros(sistema);

        try
        {
            sistema.SalvarDados();
            Console.WriteLine($"\n[Info] Estado salvo em: {sistema.Repositorio.CaminhoArquivo}");
        }
        catch (PersistenciaException ex)
        {
            LogErro("Não foi possível salvar ao final da demonstração.", ex);
        }

        Console.WriteLine("\n--- Fim da demonstração ---");
    }

    /// <summary>Cadastros, evento, atividades, inscrição e certificado.</summary>
    private static void DemonstrarFluxoFeliz(SistemaPimV s)
    {
        Console.WriteLine("--- Fluxo principal (cenário feliz) ---\n");

        var admin = new UsuarioAdministrador
        {
            Nome = "Coordenação PIM",
            Email = "coord@instituicao.edu",
            Login = "admin_pim",
            Senha = "senha123",
            Perfil = "Coordenador"
        };
        s.Administradores.Cadastrar(admin);
        Console.WriteLine($"Administrador cadastrado: {admin.Login}");

        var p1 = new Participante
        {
            Nome = "Maria Silva",
            Email = "maria.silva@email.com",
            Instituicao = "Universidade Exemplo",
            Cpf = "12345678901"
        };
        var p2 = new Participante
        {
            Nome = "João Santos",
            Email = "joao.santos@email.com",
            Instituicao = "IF Tecnológico",
            Cpf = "98765432100"
        };
        s.Participantes.Cadastrar(p1);
        s.Participantes.Cadastrar(p2);
        Console.WriteLine("Participantes cadastrados: Maria e João.");

        var evento = new Evento
        {
            Titulo = "Semana de Ciência e Tecnologia 2026",
            Descricao = "Evento integrador com palestras e minicursos.",
            DataInicio = new DateTime(2026, 5, 10, 8, 0, 0),
            DataFim = new DateTime(2026, 5, 12, 18, 0, 0),
            Local = "Auditório Central / Híbrido",
            CargaHorariaHoras = 20,
            InscricoesAbertas = true
        };
        s.Eventos.Criar(evento);
        Console.WriteLine($"Evento criado: {evento.Titulo}");

        s.Atividades.Cadastrar(new Atividade
        {
            EventoId = evento.Id,
            Titulo = "Abertura e boas-vindas",
            Tipo = TipoAtividade.Palestra,
            DataHoraInicio = new DateTime(2026, 5, 10, 8, 30, 0),
            DataHoraFim = new DateTime(2026, 5, 10, 9, 30, 0),
            Responsavel = "Reitoria"
        });
        s.Atividades.Cadastrar(new Atividade
        {
            EventoId = evento.Id,
            Titulo = "Minicurso: Introdução ao C#",
            Tipo = TipoAtividade.Minicurso,
            DataHoraInicio = new DateTime(2026, 5, 11, 14, 0, 0),
            DataHoraFim = new DateTime(2026, 5, 11, 18, 0, 0),
            Responsavel = "Prof. Exemplo"
        });
        Console.WriteLine("Atividades associadas ao evento: 2.");

        var insc = s.Inscricoes.RealizarInscricao(p1.Id, evento.Id);
        Console.WriteLine($"Inscrição realizada (Id={insc.Id}) para Maria no evento.");

        var cert = s.Certificados.GerarParaInscricao(insc.Id);
        Console.WriteLine("Certificado gerado.");
        if (!string.IsNullOrEmpty(cert.CaminhoArquivo))
            Console.WriteLine($"  Arquivo: {cert.CaminhoArquivo}");

        // Edição de evento (requisito)
        s.Eventos.Editar(evento.Id, e => e.Descricao += " [Atualizado na demonstração.]");
        Console.WriteLine("Evento editado (descrição atualizada).");

        Console.WriteLine("\nListagem de inscrições:");
        foreach (var i in s.Inscricoes.Listar())
            Console.WriteLine($"  - Inscrição {i.Id} | participante {i.ParticipanteId} | evento {i.EventoId} | {i.Status}");
    }

    /// <summary>Exibe uso de LINQ nos gerenciadores (filtros, ordenação, buscas).</summary>
    private static void DemonstrarLinQ(SistemaPimV s)
    {
        Console.WriteLine("\n--- Consultas com LINQ ---\n");

        Console.WriteLine("Eventos ordenados por data de início:");
        foreach (var e in s.Eventos.ListarOrdenadosPorDataInicio())
            Console.WriteLine($"  • {e.DataInicio:yyyy-MM-dd} — {e.Titulo}");

        Console.WriteLine("\nBusca de eventos com 'Ciência' no título:");
        foreach (var e in s.Eventos.BuscarPorTitulo("Ciência"))
            Console.WriteLine($"  • {e.Titulo}");

        // Dictionary: inscrições confirmadas por evento (requisito de estrutura genérica)
        Console.WriteLine("\nContagem (Dictionary) de inscrições confirmadas por evento:");
        foreach (var kv in s.Inscricoes.ContagemConfirmadasPorEvento())
            Console.WriteLine($"  • Evento {kv.Key} → {kv.Value} inscrição(ões)");

        Console.WriteLine("\nParticipantes filtrados por instituição 'IF':");
        foreach (var p in s.Participantes.FiltrarPorInstituicao("IF"))
            Console.WriteLine($"  • {p.Nome} — {p.Instituicao}");

        var primeiroParticipante = s.Participantes.Listar().FirstOrDefault();
        if (primeiroParticipante != null)
        {
            Console.WriteLine($"\nInscrições do participante {primeiroParticipante.Nome}:");
            foreach (var i in s.Inscricoes.BuscarPorParticipante(primeiroParticipante.Id))
                Console.WriteLine($"  • evento {i.EventoId} — {i.Status}");
        }
    }

    /// <summary>try/catch para inscrição duplicada, dados inválidos e persistência.</summary>
    private static void DemonstrarTratamentoDeErros(SistemaPimV s)
    {
        Console.WriteLine("\n--- Tratamento de erros (demonstração) ---\n");

        var evento = s.Eventos.Listar().FirstOrDefault();
        var p1 = s.Participantes.BuscarPorNome("Maria").FirstOrDefault();
        if (evento == null || p1 == null)
        {
            Console.WriteLine("(Pulando erros: evento ou participante não encontrado.)");
            return;
        }

        try
        {
            s.Inscricoes.RealizarInscricao(p1.Id, evento.Id);
        }
        catch (InscricaoDuplicadaException ex)
        {
            Console.WriteLine($"[Esperado] Inscrição duplicada: {ex.Message}");
        }

        try
        {
            s.Participantes.Cadastrar(new Participante
            {
                Nome = "",
                Email = "invalido",
                Instituicao = "X"
            });
        }
        catch (DadosInvalidosException ex)
        {
            Console.WriteLine($"[Esperado] Dados inválidos no cadastro: {ex.Message}");
        }

        try
        {
            s.Atividades.Cadastrar(new Atividade
            {
                EventoId = Guid.NewGuid(),
                Titulo = "Órfã",
                Tipo = TipoAtividade.Outro,
                DataHoraInicio = DateTime.UtcNow
            });
        }
        catch (DadosInvalidosException ex)
        {
            Console.WriteLine($"[Esperado] Atividade sem evento: {ex.Message}");
        }

        // Simula falha de persistência em caminho inválido (Windows: dispositivo inválido)
        var repoRuim = new RepositorioJson(@"Z:\caminho_inexistente_impossivel\pimv.json");
        try
        {
            repoRuim.Salvar(s.Contexto.ParaSnapshot());
        }
        catch (PersistenciaException ex)
        {
            Console.WriteLine($"[Esperado] Falha de persistência simulada: {ex.Message}");
        }
    }

    private static void LogErro(string titulo, Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[ERRO] {titulo}");
        Console.WriteLine($"       Detalhe: {ex.Message}");
        Console.ResetColor();
    }
}
