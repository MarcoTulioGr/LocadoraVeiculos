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

carregar();