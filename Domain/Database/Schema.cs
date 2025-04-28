namespace Domain.Database;

public static class Schema
{
    public static readonly HashSet<string> Tables =
    [
        "Categoria",
        "Produto",
        "TipoCliente",
        "Cliente",
        "TipoEndereco",
        "Endereco",
        "Telefone",
        "Status",
        "Pedido",
        "Pedido_has_Produto",
    ];

    public static readonly Dictionary<string, List<string>> Columns = new()
    {
        { "Categoria", ["idCategoria", "Descricao"] },
        {
            "Produto",
            ["idProduto", "Nome", "Descricao", "Preco", "QuantEstoque", "Categoria_idCategoria"]
        },
        { "TipoCliente", ["idTipoCliente", "Descricao"] },
        {
            "Cliente",
            [
                "idCliente",
                "Nome",
                "Email",
                "Nascimento",
                "Senha",
                "TipoCliente_idTipoCliente",
                "DataRegistro",
            ]
        },
        { "TipoEndereco", ["idTipoEndereco", "Descricao"] },
        {
            "Endereco",
            [
                "idEndereco",
                "EnderecoPadrao",
                "Logradouro",
                "Numero",
                "Complemento",
                "Bairro",
                "Cidade",
                "UF",
                "CEP",
                "TipoEndereco_idTipoEndereco",
                "Cliente_idCliente",
            ]
        },
        { "Telefone", ["Numero", "Cliente_idCliente"] },
        { "Status", ["idStatus", "Descricao"] },
        {
            "Pedido",
            ["idPedido", "Status_idStatus", "DataPedido", "ValorTotalPedido", "Cliente_idCliente"]
        },
        {
            "Pedido_has_Produto",
            [
                "idPedidoProduto",
                "Pedido_idPedido",
                "Produto_idProduto",
                "Quantidade",
                "PrecoUnitario",
            ]
        },
    };

    public static readonly Dictionary<string, Dictionary<string, string>> ColumnTypes = new()
    {
        {
            "Categoria",
            new Dictionary<string, string> { { "idCategoria", "int" }, { "Descricao", "string" } }
        },
        {
            "Produto",
            new Dictionary<string, string>
            {
                { "idProduto", "int" },
                { "Nome", "string" },
                { "Descricao", "string" },
                { "Preco", "decimal" },
                { "QuantEstoque", "int" },
                { "Categoria_idCategoria", "int" },
            }
        },
        {
            "TipoCliente",
            new Dictionary<string, string> { { "idTipoCliente", "int" }, { "Descricao", "string" } }
        },
        {
            "Cliente",
            new Dictionary<string, string>
            {
                { "idCliente", "int" },
                { "Nome", "string" },
                { "Email", "string" },
                { "Nascimento", "date" },
                { "Senha", "string" },
                { "TipoCliente_idTipoCliente", "int" },
                { "DataRegistro", "date" },
            }
        },
        {
            "TipoEndereco",
            new Dictionary<string, string>
            {
                { "idTipoEndereco", "int" },
                { "Descricao", "string" },
            }
        },
        {
            "Endereco",
            new Dictionary<string, string>
            {
                { "idEndereco", "int" },
                { "EnderecoPadrao", "bool" },
                { "Logradouro", "string" },
                { "Numero", "int" },
                { "Complemento", "string" },
                { "Bairro", "string" },
                { "Cidade", "string" },
                { "UF", "string" },
                { "CEP", "string" },
                { "TipoEndereco_idTipoEndereco", "int" },
                { "Cliente_idCliente", "int" },
            }
        },
        {
            "Telefone",
            new Dictionary<string, string>
            {
                { "Numero", "string" },
                { "Cliente_idCliente", "int" },
            }
        },
        {
            "Status",
            new Dictionary<string, string> { { "idStatus", "int" }, { "Descricao", "string" } }
        },
        {
            "Pedido",
            new Dictionary<string, string>
            {
                { "idPedido", "int" },
                { "Status_idStatus", "int" },
                { "DataPedido", "date" },
                { "ValorTotalPedido", "decimal" },
                { "Cliente_idCliente", "int" },
            }
        },
        {
            "Pedido_has_Produto",
            new Dictionary<string, string>
            {
                { "idPedidoProduto", "int" },
                { "Pedido_idPedido", "int" },
                { "Produto_idProduto", "int" },
                { "Quantidade", "int" },
                { "PrecoUnitario", "decimal" },
            }
        },
    };
}
