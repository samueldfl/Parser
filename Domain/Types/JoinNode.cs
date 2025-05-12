namespace Domain.Types;

public class JoinNode(string condition, AlgebraNode left, AlgebraNode right) : AlgebraNode
{
    public string Condition { get; } = condition;

    public AlgebraNode Left { get; } = left;

    public AlgebraNode Right { get; } = right;

    public override string ToAlgebraString() =>
        $"({Left.ToAlgebraString()} ⨝_{Condition} {Right.ToAlgebraString()})";
}
