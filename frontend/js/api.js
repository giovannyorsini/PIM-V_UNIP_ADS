/**
 * api.js — Camada de integração HTTP com o back-end C# (ASP.NET Minimal API).
 *
 * A URL base é configurável: defina window.PIMV_API_BASE antes de carregar os scripts,
 * ou edite API_BASE_URL abaixo (ex.: outro host/porta em laboratório).
 *
 * Endpoints esperados (prefixo /api):
 *   GET  /health
 *   GET  /eventos
 *   GET  /atividades
 *   POST /participantes
 *   GET  /participantes/por-email?email=
 *   POST /inscricoes
 *   GET  /inscricoes?email=
 *   GET  /certificados?email=
 *   POST /certificados/emitir
 */

(function (global) {
  "use strict";

  /** URL base da API — ajuste aqui se o back-end rodar em outra porta. */
  const API_BASE_URL =
    (typeof global.PIMV_API_BASE === "string" && global.PIMV_API_BASE.trim()) ||
    "http://127.0.0.1:5000/api";

  /**
   * Monta URL absoluta e executa fetch com JSON.
   * @param {string} caminho Caminho após /api (ex.: "/eventos" ou "eventos")
   * @param {RequestInit} [opcoes] Opções do fetch (method, body, headers)
   * @returns {Promise<any>} Corpo parseado como JSON ou null se vazio
   */
  async function apiFetch(caminho, opcoes) {
    const base = API_BASE_URL.replace(/\/$/, "");
    const path = caminho.startsWith("/") ? caminho : `/${caminho}`;
    const url = `${base}${path}`;

    const headers = {
      Accept: "application/json",
      ...(opcoes && opcoes.headers ? opcoes.headers : {}),
    };

    if (
      opcoes &&
      opcoes.body &&
      typeof opcoes.body === "string" &&
      !headers["Content-Type"] &&
      !headers["content-type"]
    ) {
      headers["Content-Type"] = "application/json";
    }

    const res = await fetch(url, { ...opcoes, headers });
    const texto = await res.text();
    let dados = null;
    if (texto) {
      try {
        dados = JSON.parse(texto);
      } catch {
        dados = { message: texto };
      }
    }

    if (!res.ok) {
      const msg =
        dados && typeof dados.message === "string"
          ? dados.message
          : `Erro HTTP ${res.status}`;
      const err = new Error(msg);
      err.status = res.status;
      err.body = dados;
      throw err;
    }

    return dados;
  }

  /**
   * Testa se a API está acessível (diagnóstico rápido no front-end).
   */
  function healthCheck() {
    return apiFetch("/health", { method: "GET" });
  }

  /**
   * Lista eventos ordenados por data de início.
   * @returns {Promise<object[]>}
   */
  function getEventos() {
    return apiFetch("/eventos", { method: "GET" });
  }

  /**
   * Lista atividades da programação com metadados do evento.
   * @returns {Promise<object[]>}
   */
  function getAtividades() {
    return apiFetch("/atividades", { method: "GET" });
  }

  /**
   * Cadastra participante (nome, e-mail obrigatórios; instituição/CPF opcionais).
   * @param {{ nome: string, email: string, instituicao?: string, cpf?: string }} dados
   */
  function cadastrarParticipante(dados) {
    return apiFetch("/participantes", {
      method: "POST",
      body: JSON.stringify(dados),
    });
  }

  /**
   * Inscreve participante existente (identificado por e-mail) em um evento.
   * @param {{ email: string, eventoId: string }} dados eventoId como GUID string
   */
  function realizarInscricao(dados) {
    return apiFetch("/inscricoes", {
      method: "POST",
      body: JSON.stringify(dados),
    });
  }

  /**
   * Busca participante pelo e-mail.
   * @param {string} email
   */
  function getParticipantePorEmail(email) {
    const q = new URLSearchParams({ email });
    return apiFetch(`/participantes/por-email?${q.toString()}`, {
      method: "GET",
    });
  }

  /**
   * Lista inscrições do participante a partir do e-mail.
   * @param {string} email
   */
  function getInscricoesPorEmail(email) {
    const q = new URLSearchParams({ email });
    return apiFetch(`/inscricoes?${q.toString()}`, { method: "GET" });
  }

  /**
   * Lista certificados emitidos para o participante (filtrado por e-mail no servidor).
   * @param {string} email
   */
  function getCertificados(email) {
    const q = new URLSearchParams({ email });
    return apiFetch(`/certificados?${q.toString()}`, { method: "GET" });
  }

  /**
   * Emite certificado para inscrição confirmada.
   * @param {{ inscricaoId?: string, email?: string, eventoId?: string }} dados
   */
  function emitirCertificado(dados) {
    return apiFetch("/certificados/emitir", {
      method: "POST",
      body: JSON.stringify(dados),
    });
  }

  /** Expõe funções e constante de base para depuração no console */
  global.PimVApi = {
    API_BASE_URL,
    apiFetch,
    healthCheck,
    getEventos,
    getAtividades,
    cadastrarParticipante,
    realizarInscricao,
    getParticipantePorEmail,
    getInscricoesPorEmail,
    getCertificados,
    emitirCertificado,
  };
})(typeof window !== "undefined" ? window : globalThis);
