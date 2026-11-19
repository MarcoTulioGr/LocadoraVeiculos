let categorias = [];
let idEmEdicao = null;
let idParaExcluir = null;

const modalForm    = new bootstrap.Modal(document.getElementById("modalForm"));
const modalExcluir = new bootstrap.Modal(document.getElementById("modalExcluir"));
const form         = document.getElementById("formCategoria");

// listagem
async function carregar() {
  try {
    categorias = await apiGet("/categoriasveiculo");
    renderizar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

function renderizar() {
  const nome = normalizar(document.getElementById("filtroNome").value);
  const minTxt = document.getElementById("filtroMin").value;
  const maxTxt = document.getElementById("filtroMax").value;
  const min = minTxt === "" ? 0        : Number(minTxt);
  const max = maxTxt === "" ? Infinity : Number(maxTxt);

  const lista = categorias.filter(c =>
    normalizar(c.nome).includes(nome) &&
    c.valorDiariaBase >= min &&
    c.valorDiariaBase <= max
  );

  const tbody = document.getElementById("tabela");
  tbody.innerHTML = lista.length === 0
    ? `<tr><td colspan="5" class="text-center text-secondary">Nenhuma categoria encontrada.</td></tr>`
    : lista.map(c => `
      <tr>
        <td>${c.id}</td>
        <td>${escapar(c.nome)}</td>
        <td>${escapar(c.descricao ?? "—")}</td>
        <td>${formatarMoeda(c.valorDiariaBase)}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-outline-dark" onclick="abrirEdicao(${c.id})">Editar</button>
          <button class="btn btn-sm btn-outline-danger" onclick="pedirExclusao(${c.id})">Excluir</button>
        </td>
      </tr>`).join("");

  document.getElementById("contador").textContent = `${lista.length} de ${categorias.length} registro(s)`;
}

function limparFiltros() {
  ["filtroNome", "filtroMin", "filtroMax"].forEach(id => document.getElementById(id).value = "");
  renderizar();
}

["filtroNome", "filtroMin", "filtroMax"].forEach(id =>
  document.getElementById(id).addEventListener("input", renderizar));

// casdastro/edit
function abrirNovo() {
  idEmEdicao = null;
  form.reset();
  form.classList.remove("was-validated");
  document.getElementById("tituloModal").textContent = "Nova categoria";
  modalForm.show();
}

function abrirEdicao(id) {
  const c = categorias.find(x => x.id === id);
  idEmEdicao = id;
  form.classList.remove("was-validated");
  document.getElementById("campoNome").value      = c.nome;
  document.getElementById("campoDescricao").value = c.descricao ?? "";
  document.getElementById("campoDiaria").value    = c.valorDiariaBase;
  document.getElementById("tituloModal").textContent = `Editar categoria #${id}`;
  modalForm.show();
}

form.addEventListener("submit", async (evento) => {
  evento.preventDefault();
  if (!form.checkValidity()) {
    form.classList.add("was-validated");
    return;
  }

  const corpo = {
    nome: document.getElementById("campoNome").value.trim(),
    descricao: document.getElementById("campoDescricao").value.trim() || null,
    valorDiariaBase: Number(document.getElementById("campoDiaria").value),
  };

  try {
    if (idEmEdicao === null) {
      await apiPost("/categoriasveiculo", corpo);
      mostrarMensagem("Categoria cadastrada com sucesso.");
    } else {
      await apiPut(`/categoriasveiculo/${idEmEdicao}`, { id: idEmEdicao, ...corpo });
      mostrarMensagem("Categoria atualizada com sucesso.");
    }
    carregar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
  modalForm.hide();
});

// exclusao
function pedirExclusao(id) {
  idParaExcluir = id;
  document.getElementById("nomeExcluir").textContent = categorias.find(x => x.id === id).nome;
  modalExcluir.show();
}

async function confirmarExclusao() {
  try {
    await apiDelete(`/categoriasveiculo/${idParaExcluir}`);
    mostrarMensagem("Categoria excluída com sucesso.");
    carregar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
  modalExcluir.hide();
}

carregar();