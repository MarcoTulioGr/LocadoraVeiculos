const API_URL = "http://localhost:5207/api";

async function api(caminho, opcoes = {}) {
  const resposta = await fetch(API_URL + caminho, {
    headers: { "Content-Type": "application/json" },
    ...opcoes,
  });

  if (resposta.status === 204) return null;

  const dados = await resposta.json().catch(() => null);

  if (!resposta.ok) {
    throw new Error(extrairErro(dados, resposta.status));
  }
  return dados;
}

function extrairErro(dados, status) {
  if (dados && dados.mensagem) return dados.mensagem;
  if (dados && dados.errors) return Object.values(dados.errors).flat().join(" ");
  return `Erro ${status} ao comunicar com a API.`;
}

const apiGet    = (caminho)        => api(caminho);
const apiPost   = (caminho, corpo) => api(caminho, { method: "POST", body: JSON.stringify(corpo) });
const apiPut    = (caminho, corpo) => api(caminho, { method: "PUT",  body: JSON.stringify(corpo) });
const apiDelete = (caminho)        => api(caminho, { method: "DELETE" });


function formatarMoeda(valor) {
  return Number(valor ?? 0).toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
}

function formatarData(iso) {
  if (!iso) return "—";
  const [ano, mes, dia] = iso.substring(0, 10).split("-");
  return `${dia}/${mes}/${ano}`;
}

function escapar(texto) {
  const div = document.createElement("div");
  div.textContent = texto ?? "";
  return div.innerHTML;
}

function mostrarMensagem(texto, tipo = "success") {
  const area = document.getElementById("mensagens");
  area.innerHTML = `
    <div class="alert alert-${tipo} alert-dismissible fade show" role="alert">
      ${escapar(texto)}
      <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Fechar"></button>
    </div>`;
}

function normalizar(texto) {
  return (texto ?? "")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase();
}


function hojeISO() {
  const d = new Date();
  const mes = String(d.getMonth() + 1).padStart(2, "0");
  const dia = String(d.getDate()).padStart(2, "0");
  return `${d.getFullYear()}-${mes}-${dia}`;
}

function somarDias(iso, quantidade) {
  const d = new Date(iso.substring(0, 10) + "T12:00:00");
  d.setDate(d.getDate() + quantidade);
  const mes = String(d.getMonth() + 1).padStart(2, "0");
  const dia = String(d.getDate()).padStart(2, "0");
  return `${d.getFullYear()}-${mes}-${dia}`;
}

// diferenca em dias entre duas datas (so a parte yyyy-mm-dd)
function diasEntre(inicio, fim) {
  const ms = Date.parse(fim.substring(0, 10)) - Date.parse(inicio.substring(0, 10));
  return Math.round(ms / 86400000);
}