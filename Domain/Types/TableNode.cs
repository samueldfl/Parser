namespace Domain.Types;

public class TableNode(string tableName) : AlgebraNode
{
    public string TableName { get; } = tableName;

    public override string ToAlgebraString() => TableName;
}
