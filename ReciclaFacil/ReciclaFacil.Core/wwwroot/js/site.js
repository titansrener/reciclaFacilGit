(() => {
    const form = document.querySelector("#form-busca");
    const results = document.querySelector("#resultados");
    if (!form || !results) return;

    form.addEventListener("submit", async event => {
        event.preventDefault();
        const button = form.querySelector("button");
        button.disabled = true;
        results.innerHTML = "<p class=\"loading\">Pesquisando…</p>";

        try {
            const response = await fetch(`${form.action}?${new URLSearchParams(new FormData(form))}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            results.innerHTML = await response.text();
        } catch {
            results.innerHTML = "<p class=\"error\">Não foi possível realizar a pesquisa. Tente novamente.</p>";
        } finally {
            button.disabled = false;
        }
    });
})();
