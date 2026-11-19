let fabricantes = [];   

async function carregar() {
  try {
    fabricantes = await apiGet("/fabricantes");
    renderizar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

function renderizar() {
  const nome = normalizar(document.getElementById("filtroNome").value);
  const pais = normalizar(document.getElementById("filtroPais").value);

  const lista = fabricantes.filter(f =>
    normalizar(f.nome).includes(nome) &&
    normalizar(f.paisOrigem).includes(pais)
  );

  const tbody = document.getElementById("tabela");

  if (lista.length === 0) {
    tbody.innerHTML = `<tr><td colspan="4" class="text-center text-secondary">Nenhum fabricante encontrado.</td></tr>`;
  } else {
    tbody.innerHTML = lista.map(f => `
      <tr>
        <td>${f.id}</td>
        <td>${escapar(f.nome)}</td>
        <td>${escapar(f.paisOrigem ?? "—")}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-outline-dark" onclick="abrirEdicao(${f.id})">Editar</button>
          <button class="btn btn-sm btn-outline-danger" onclick="pedirExclusao(${f.id})">Excluir</button>
        </td>
      </tr>`).join("");
  }

  document.getElementById("contador").textContent =
    `${lista.length} de ${fabricantes.length} registro(s)`;
}

function limparFiltros() {
  document.getElementById("filtroNome").value = "";
  document.getElementById("filtroPais").value = "";
  renderizar();
}


document.getElementById("filtroNome").addEventListener("input", renderizar);
document.getElementById("filtroPais").addEventListener("input", renderizar);


const modalForm    = new bootstrap.Modal(document.getElementById("modalFabricante"));
const modalExcluir = new bootstrap.Modal(document.getElementById("modalExcluir"));
const form         = document.getElementById("formFabricante");

let idEmEdicao   = null;   
let idParaExcluir = null;

function abrirNovo() {
  idEmEdicao = null;
  form.reset();
  form.classList.remove("was-validated");
  document.getElementById("tituloModal").textContent = "Novo fabricante";
  modalForm.show();
}

function abrirEdicao(id) {
  const f = fabricantes.find(x => x.id === id);
  idEmEdicao = id;
  form.classList.remove("was-validated");
  document.getElementById("campoNome").value = f.nome;
  document.getElementById("campoPais").value = f.paisOrigem ?? "";
  document.getElementById("tituloModal").textContent = `Editar fabricante #${id}`;
  modalForm.show();
}

form.addEventListener("submit", async (evento) => {
  evento.preventDefault();               // impede o navegador de recarregar a página

  if (!form.checkValidity()) {           // campos obrigatórios vazios?
    form.classList.add("was-validated"); // mostra as mensagens em vermelho
    return;
  }

  const corpo = {
    nome: document.getElementById("campoNome").value.trim(),
    paisOrigem: document.getElementById("campoPais").value.trim() || null,
  };

  try {
    if (idEmEdicao === null) {
      await apiPost("/fabricantes", corpo);
      mostrarMensagem("Fabricante cadastrado com sucesso.");
    } else {
      await apiPut(`/fabricantes/${idEmEdicao}`, { id: idEmEdicao, ...corpo });
      mostrarMensagem("Fabricante atualizado com sucesso.");
    }
    modalForm.hide();
    carregar();                          // recarrega a lista do banco
  } catch (erro) {
    modalForm.hide();
    mostrarMensagem(erro.message, "danger");
  }
});


function pedirExclusao(id) {
  const f = fabricantes.find(x => x.id === id);
  idParaExcluir = id;
  document.getElementById("nomeExcluir").textContent = f.nome;
  modalExcluir.show();
}

async function confirmarExclusao() {
  try {
    await apiDelete(`/fabricantes/${idParaExcluir}`);
    mostrarMensagem("Fabricante excluído com sucesso.");
    carregar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");   // ex.: 409 se tiver veículos
  }
  modalExcluir.hide();
}
carregar();