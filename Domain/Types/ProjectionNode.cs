namespace Domain.Types;

public class ProjectionNode(string attributes, AlgebraNode child) : AlgebraNode
{
    public string Attributes { get; } = attributes;

    public AlgebraNode Child { get; } = child;

    public override string ToAlgebraString() => $"π_{Attributes}({Child.ToAlgebraString()})";
}
