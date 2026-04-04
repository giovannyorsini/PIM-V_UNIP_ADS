/**
 * main.js — Comportamento do front-end PIM V: DOM, validação e chamadas à API.
 * Cada página define data-page no <body> para despachar o inicializador correto.
 */

(function () {
  "use strict";

  const Api = window.PimVApi;
  if (!Api) {
    console.error("PimVApi não encontrado. Carregue api.js antes de main.js.");
    return;
  }

  /** Atraso mínimo opcional para exibir estado de carregamento (simulação pedida nos requisitos). */
  const SIMULAR_LATENCIA_MS = 380;

  /**
   * Executa uma função async com delay mínimo (UX de “carregando”).
   * @template T
   * @param {() => Promise<T>} fn
   * @returns {Promise<T>}
   */
  async function comLatenciaSimulada(fn) {
    const t0 = Date.now();
    const resultado = await fn();
    const dt = Date.now() - t0;
    if (dt < SIMULAR_LATENCIA_MS) {
      await new Promise((r) => setTimeout(r, SIMULAR_LATENCIA_MS - dt));
    }
    return resultado;
  }

  /**
   * Formata data/hora ISO para exibição local em pt-BR.
   * @param {string} iso
   */
  function formatarDataHora(iso) {
    try {
      const d = new Date(iso);
      if (Number.isNaN(d.getTime())) return iso;
      return d.toLocaleString("pt-BR", {
        dateStyle: "short",
        timeStyle: "short",
      });
    } catch {
      return iso;
    }
  }

  /**
   * Valida e-mail simples (front-end); o servidor valida de novo.
   * @param {string} v
   */
  function emailValido(v) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(v).trim());
  }

  /**
   * Exibe mensagem em elemento dedicado.
   * @param {HTMLElement|null} el
   * @param {string} texto
   * @param {"ok"|"erro"|"info"} tipo
   */
  function mostrarAlerta(el, texto, tipo) {
    if (!el) return;
    el.hidden = false;
    el.textContent = texto;
    el.className = `alerta alerta--${tipo}`;
    el.setAttribute("role", "status");
  }

  function limparAlerta(el) {
    if (!el) return;
    el.hidden = true;
    el.textContent = "";
    el.removeAttribute("role");
  }

  /**
   * Painel de acessibilidade: alto contraste e texto maior.
   */
  function initBarraAcessibilidade() {
    const btnContraste = document.getElementById("a11y-contraste");
    const btnTexto = document.getElementById("a11y-texto");
    const root = document.documentElement;

    if (btnContraste) {
      const salvo = localStorage.getItem("pimv-a11y-contraste") === "1";
      if (salvo) {
        root.classList.add("a11y-contraste-alto");
        btnContraste.setAttribute("aria-pressed", "true");
      }
      btnContraste.addEventListener("click", () => {
        root.classList.toggle("a11y-contraste-alto");
        const on = root.classList.contains("a11y-contraste-alto");
        btnContraste.setAttribute("aria-pressed", on ? "true" : "false");
        localStorage.setItem("pimv-a11y-contraste", on ? "1" : "0");
      });
    }

    if (btnTexto) {
      const salvoT = localStorage.getItem("pimv-a11y-texto") === "1";
      if (salvoT) {
        root.classList.add("a11y-texto-maior");
        btnTexto.setAttribute("aria-pressed", "true");
      }
      btnTexto.addEventListener("click", () => {
        root.classList.toggle("a11y-texto-maior");
        const on = root.classList.contains("a11y-texto-maior");
        btnTexto.setAttribute("aria-pressed", on ? "true" : "false");
        localStorage.setItem("pimv-a11y-texto", on ? "1" : "0");
      });
    }
  }

  /**
   * Página inicial: teste silencioso de API (mostra aviso se falhar).
   */
  async function initHome() {
    const status = document.getElementById("status-api-home");
    if (!status) return;
    try {
      await comLatenciaSimulada(() => Api.healthCheck());
      mostrarAlerta(
        status,
        "Conexão com o back-end verificada. Inicie a API com: dotnet run (pasta backend).",
        "ok"
      );
    } catch (e) {
      mostrarAlerta(
        status,
        "Não foi possível contatar a API. Confirme se o back-end está em " +
          Api.API_BASE_URL +
          " (veja js/api.js).",
        "erro"
      );
    }
  }

  /**
   * Programação: carrega atividades e renderiza lista; trata erro de rede/servidor.
   */
  async function initProgramacao() {
    const lista = document.getElementById("lista-atividades");
    const status = document.getElementById("status-programacao");
    const skel = document.getElementById("skeleton-programacao");
    if (!lista) return;

    limparAlerta(status);
    if (skel) skel.hidden = false;
    lista.innerHTML = "";

    try {
      const dados = await comLatenciaSimulada(() => Api.getAtividades());
      if (skel) skel.hidden = true;

      if (!Array.isArray(dados) || dados.length === 0) {
        lista.innerHTML =
          "<li><p>Nenhuma atividade cadastrada. Verifique o seed no servidor.</p></li>";
        return;
      }

      const frag = document.createDocumentFragment();
      dados.forEach((a) => {
        const li = document.createElement("li");
        li.className = "item-atividade";
        const inicio = formatarDataHora(a.dataHoraInicio);
        const fim = a.dataHoraFim ? formatarDataHora(a.dataHoraFim) : "—";
        li.innerHTML = `
          <p class="item-atividade__meta">${escapeHtml(a.eventoTitulo || "")} · ${escapeHtml(
          a.tipo || ""
        )}</p>
          <h3 class="item-atividade__titulo">${escapeHtml(a.titulo || "")}</h3>
          <p><strong>Horário:</strong> ${escapeHtml(inicio)} — ${escapeHtml(fim)}</p>
          <p>${escapeHtml(a.descricao || "")}</p>
          ${
            a.responsavel
              ? `<p class="item-atividade__meta">Responsável: ${escapeHtml(a.responsavel)}</p>`
              : ""
          }
        `;
        frag.appendChild(li);
      });
      lista.appendChild(frag);
    } catch (e) {
      if (skel) skel.hidden = true;
      mostrarAlerta(
        status,
        "Erro ao carregar programação: " + (e.message || String(e)),
        "erro"
      );
    }
  }

  function escapeHtml(s) {
    return String(s)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  /**
   * Inscrição: validação client-side + POST participante e inscrição.
   */
  async function initInscricao() {
    const form = document.getElementById("form-inscricao");
    const status = document.getElementById("status-inscricao");
    const selEvento = document.getElementById("select-evento");
    if (!form || !selEvento) return;

    try {
      const eventos = await comLatenciaSimulada(() => Api.getEventos());
      selEvento.innerHTML = '<option value="">Selecione um evento</option>';
      if (Array.isArray(eventos)) {
        eventos.forEach((ev) => {
          if (!ev.inscricoesAbertas) return;
          const op = document.createElement("option");
          op.value = ev.id;
          op.textContent = ev.titulo;
          selEvento.appendChild(op);
        });
      }
    } catch (e) {
      mostrarAlerta(
        status,
        "Não foi possível carregar eventos: " + (e.message || String(e)),
        "erro"
      );
    }

    form.addEventListener("submit", async (ev) => {
      ev.preventDefault();
      limparAlerta(status);

      const nome = document.getElementById("insc-nome");
      const email = document.getElementById("insc-email");
      const inst = document.getElementById("insc-instituicao");

      nome.classList.remove("erro");
      email.classList.remove("erro");
      selEvento.classList.remove("erro");

      let ok = true;
      if (!nome.value.trim() || nome.value.trim().length < 3) {
        nome.classList.add("erro");
        ok = false;
      }
      if (!emailValido(email.value)) {
        email.classList.add("erro");
        ok = false;
      }
      if (!selEvento.value) {
        selEvento.classList.add("erro");
        ok = false;
      }
      if (!ok) {
        mostrarAlerta(status, "Corrija os campos destacados.", "erro");
        return;
      }

      try {
        await Api.cadastrarParticipante({
          nome: nome.value.trim(),
          email: email.value.trim(),
          instituicao: inst.value.trim() || undefined,
          cpf: undefined,
        });
      } catch (e) {
        const msg = (e.message || "").toLowerCase();
        const duplicado =
          e.status === 400 && msg.includes("já existe participante cadastrado com este e-mail");
        if (!duplicado) {
          mostrarAlerta(status, "Cadastro: " + (e.message || String(e)), "erro");
          return;
        }
        /* E-mail já cadastrado: segue para tentar apenas a inscrição no evento escolhido. */
      }

      try {
        await Api.realizarInscricao({
          email: email.value.trim(),
          eventoId: selEvento.value,
        });
        mostrarAlerta(
          status,
          "Inscrição realizada com sucesso! Consulte seus dados em Participante.",
          "ok"
        );
        form.reset();
      } catch (e2) {
        mostrarAlerta(status, "Inscrição: " + (e2.message || String(e2)), "erro");
      }
    });
  }

  /**
   * Participante: busca por e-mail e lista inscrições.
   */
  async function initParticipante() {
    const form = document.getElementById("form-participante");
    const painel = document.getElementById("painel-participante");
    const status = document.getElementById("status-participante");
    if (!form || !painel) return;

    form.addEventListener("submit", async (ev) => {
      ev.preventDefault();
      limparAlerta(status);
      const email = document.getElementById("part-email");
      if (!emailValido(email.value)) {
        mostrarAlerta(status, "Informe um e-mail válido.", "erro");
        return;
      }

      painel.hidden = true;
      painel.innerHTML = "";

      try {
        const p = await comLatenciaSimulada(() =>
          Api.getParticipantePorEmail(email.value.trim())
        );
        const inscricoes = await Api.getInscricoesPorEmail(email.value.trim());

        const bloco = document.createElement("div");
        bloco.className = "cartao";
        bloco.innerHTML = `
          <h3>Dados do participante</h3>
          <p><strong>Nome:</strong> ${escapeHtml(p.nome)}</p>
          <p><strong>E-mail:</strong> ${escapeHtml(p.email)}</p>
          <p><strong>Instituição:</strong> ${escapeHtml(p.instituicao || "—")}</p>
          <h3 class="mt-1">Inscrições</h3>
        `;

        if (!Array.isArray(inscricoes) || inscricoes.length === 0) {
          bloco.innerHTML += "<p>Nenhuma inscrição encontrada.</p>";
        } else {
          const ul = document.createElement("ul");
          ul.className = "lista-atividades";
          inscricoes.forEach((i) => {
            const li = document.createElement("li");
            li.className = "item-atividade";
            li.innerHTML = `
              <p><strong>${escapeHtml(i.eventoTitulo || "")}</strong></p>
              <p>Status: ${escapeHtml(i.status || "")} · ${escapeHtml(
              formatarDataHora(i.dataInscricao)
            )}</p>
            <p class="item-atividade__meta">Id inscrição: ${escapeHtml(String(i.id))}</p>
            `;
            ul.appendChild(li);
          });
          bloco.appendChild(ul);
        }

        painel.appendChild(bloco);
        painel.hidden = false;
      } catch (e) {
        if (e.status === 404) {
          mostrarAlerta(
            status,
            "Participante não encontrado. Utilize a página Inscrição para cadastrar.",
            "info"
          );
        } else {
          mostrarAlerta(status, e.message || String(e), "erro");
        }
      }
    });
  }

  /**
   * Certificados: listagem, visualização em modal e emissão simulada.
   */
  async function initCertificados() {
    const form = document.getElementById("form-cert-email");
    const tabela = document.getElementById("corpo-tabela-cert");
    const status = document.getElementById("status-cert");
    const modal = document.getElementById("modal-cert");
    const modalTitulo = document.getElementById("modal-cert-titulo");
    const modalTexto = document.getElementById("modal-cert-texto");
    const modalFechar = document.getElementById("modal-cert-fechar");

    if (!form || !tabela) return;

    function fecharModal() {
      if (modal) {
        modal.hidden = true;
        modalTexto.textContent = "";
      }
    }

    if (modalFechar) modalFechar.addEventListener("click", fecharModal);
    if (modal) {
      modal.addEventListener("click", (ev) => {
        if (ev.target === modal) fecharModal();
      });
    }

    form.addEventListener("submit", async (ev) => {
      ev.preventDefault();
      limparAlerta(status);
      const email = document.getElementById("cert-email");
      if (!emailValido(email.value)) {
        mostrarAlerta(status, "E-mail inválido.", "erro");
        return;
      }

      tabela.innerHTML = "";

      try {
        const lista = await comLatenciaSimulada(() =>
          Api.getCertificados(email.value.trim())
        );

        if (!Array.isArray(lista) || lista.length === 0) {
          tabela.innerHTML =
            '<tr><td colspan="4">Nenhum certificado. Você pode emitir após inscrição confirmada.</td></tr>';
        } else {
          lista.forEach((c) => {
            const tr = document.createElement("tr");
            tr.innerHTML = `
              <td>${escapeHtml(c.eventoTitulo || "")}</td>
              <td>${escapeHtml(formatarDataHora(c.dataEmissao))}</td>
              <td>${escapeHtml((c.previewTexto || "").slice(0, 80))}…</td>
              <td>
                <button type="button" class="btn btn--secundario btn-ver-cert" data-id="${escapeHtml(
                  String(c.id)
                )}">Visualizar</button>
              </td>
            `;
            tabela.appendChild(tr);
          });

          tabela.querySelectorAll(".btn-ver-cert").forEach((btn) => {
            btn.addEventListener("click", () => {
              const id = btn.getAttribute("data-id");
              const cert = lista.find((x) => String(x.id) === id);
              if (!cert || !modal) return;
              modalTitulo.textContent = "Certificado — " + (cert.eventoTitulo || "");
              modalTexto.textContent = cert.conteudoTexto || "";
              modal.hidden = false;
              if (modalFechar) modalFechar.focus();
            });
          });
        }
      } catch (e) {
        mostrarAlerta(status, e.message || String(e), "erro");
      }
    });

    const selEmitirEvento = document.getElementById("emitir-evento");
    if (selEmitirEvento) {
      Api.getEventos()
        .then((eventos) => {
          selEmitirEvento.innerHTML = '<option value="">Selecione o evento</option>';
          if (Array.isArray(eventos)) {
            eventos.forEach((ev) => {
              const op = document.createElement("option");
              op.value = ev.id;
              op.textContent = ev.titulo;
              selEmitirEvento.appendChild(op);
            });
          }
        })
        .catch(() => {
          /* formulário ainda aceita GUID manual se a lista falhar */
        });
    }

    const formEmitir = document.getElementById("form-emitir-cert");
    if (formEmitir) {
      formEmitir.addEventListener("submit", async (ev) => {
        ev.preventDefault();
        limparAlerta(status);
        const em = document.getElementById("emitir-email");
        const evSel = document.getElementById("emitir-evento");
        if (!emailValido(em.value) || !evSel || !evSel.value) {
          mostrarAlerta(status, "Preencha e-mail e selecione o evento da inscrição confirmada.", "erro");
          return;
        }
        try {
          await Api.emitirCertificado({
            email: em.value.trim(),
            eventoId: evSel.value,
          });
          mostrarAlerta(status, "Certificado emitido (ou já existente). Atualize a listagem.", "ok");
        } catch (e) {
          mostrarAlerta(status, e.message || String(e), "erro");
        }
      });
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    initBarraAcessibilidade();
    const page = document.body && document.body.getAttribute("data-page");
    switch (page) {
      case "home":
        initHome();
        break;
      case "programacao":
        initProgramacao();
        break;
      case "inscricao":
        initInscricao();
        break;
      case "participante":
        initParticipante();
        break;
      case "certificados":
        initCertificados();
        break;
      default:
        break;
    }
  });
})();
