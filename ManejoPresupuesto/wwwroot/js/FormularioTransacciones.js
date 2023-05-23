function inicializarFormularioTransacciones(urlObetenerCategorias) {
    $("#TipoOperacionId").change(async function () {
        const valorSeleccionado = $(this).val();

        const respuesta = await fetch(urlObetenerCategorias, {
            method: 'POST',
            body: valorSeleccionado,
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const json = await respuesta.json();
        const opciones = json.map(categoria => `<option value=${categoria.value}>${categoria.text}</option>`);
        document.getElementById("CategoriaId").innerHTML = opciones;
        //$("#CategoriaId").Html(opciones);
    })
}